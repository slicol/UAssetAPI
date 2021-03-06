﻿using System;
using System.IO;

namespace UAssetAPI.PropertyTypes
{
    public class TextPropertyData : PropertyData<string[]>
    {
        public int Flag;
        public TextHistoryType HistoryType = TextHistoryType.Base;
        public byte[] Extras;

        public TextPropertyData(string name, AssetReader asset) : base(name, asset)
        {
            Type = "TextProperty";
        }

        public TextPropertyData()
        {
            Type = "TextProperty";
        }

        public override void Read(BinaryReader reader, bool includeHeader, long leng)
        {
            if (includeHeader)
            {
                reader.ReadByte();
            }

            Flag = reader.ReadInt32();
            HistoryType = (TextHistoryType)reader.ReadSByte();

            switch(HistoryType)
            {
                case TextHistoryType.None:
                    Extras = new byte[0];
                    Value = null;
                    break;
                case TextHistoryType.Base:
                    Extras = reader.ReadBytes(4);
                    Value = new string[] { reader.ReadUString(), reader.ReadUString() };
                    break;
                case TextHistoryType.StringTableEntry:
                    Extras = reader.ReadBytes(8);
                    Value = new string[] { reader.ReadUString() };
                    break;
                default:
                    throw new FormatException("Unimplemented reader for " + HistoryType.ToString());
            }
        }

        public override int Write(BinaryWriter writer, bool includeHeader)
        {
            if (includeHeader)
            {
                writer.Write((byte)0);
            }

            int here = (int)writer.BaseStream.Position;
            writer.Write(Flag);
            writer.Write((byte)HistoryType);
            writer.Write(Extras);

            switch(HistoryType)
            {
                case TextHistoryType.None:
                    Value = null;
                    break;
                case TextHistoryType.Base:
                    for (int i = 0; i < 2; i++)
                    {
                        writer.WriteUString(Value[i]);
                    }
                    break;
                case TextHistoryType.StringTableEntry:
                    writer.WriteUString(Value[0]);
                    break;
                default:
                    throw new FormatException("Unimplemented writer for " + HistoryType.ToString());
            }

            return (int)writer.BaseStream.Position - here;
        }

        public override string ToString()
        {
            if (Value == null) return "null";

            string oup = "";
            for (int i = 0; i < Value.Length; i++)
            {
                oup += Value[i] + " ";
            }
            return oup.TrimEnd(' ');
        }

        public override void FromString(string[] d)
        {
            if (d[1] != null)
            {
                HistoryType = TextHistoryType.Base;
                Value = new string[] { d[0], d[1] };
                return;
            }

            if (d[0].Equals("null"))
            {
                HistoryType = TextHistoryType.None;
                Value = null;
                return;
            }

            HistoryType = TextHistoryType.StringTableEntry;
            Value = new string[] { d[0] };
        }
    }
}