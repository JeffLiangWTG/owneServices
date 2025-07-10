using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class FieldAttribute : Attribute
	{
		public FieldAttribute(short position) : this(position, 1, AlignTypes.None, ' ') { }
		public FieldAttribute(short position, short length) : this(position, length, AlignTypes.None, ' ') { }
		public FieldAttribute(short position, short length, AlignTypes alignType) : this(position, length, alignType, ' ') { }
		public FieldAttribute(short position, short length, AlignTypes alignType, char padChar)
		{
			if (position < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(position), "Position must be 0 or greater.");
			}

			if (length < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(length), "Length must be 1 or greater.");
			}

			Position = position;
			Length = length;
			PadChar = padChar;
			AlignType = alignType;
		}

		public IZType GetValue(FlatFileDataRow rawDataRow)
		{
			return GetValueCore(rawDataRow);
		}

		protected virtual IZType GetValueCore(FlatFileDataRow rawData)
		{
			return rawData.GetField(Position).Trim();
		}

		public void SetValueAsString(IZType value, FlatFileDataRow rawRow)
		{
			switch (AlignType)
			{
				case AlignTypes.Left:
					SetValueAsStringAlignLeft(value, rawRow);
					break;

				case AlignTypes.Right:
					SetValueAsStringAlignRight(value, rawRow);
					break;

				case AlignTypes.None:
					SetValueAsStringAlignNone(value, rawRow);
					break;
			}
		}

		protected virtual void SetValueAsStringAlignLeft(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, (ToCleanString(value).PadRight(Length, PadChar)).SubstringSafe(0, Length));
		}

		protected virtual void SetValueAsStringAlignRight(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, (ToCleanString(value).PadLeft(Length, PadChar)).SubstringSafe(0, Length));
		}

		protected virtual void SetValueAsStringAlignNone(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ToCleanString(value).SubstringSafe(0, Length));
		}

		static ZString ToCleanString(IZType value)
		{
			return ((ZString)value.ToString()).Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
		}

		public readonly short Position;  // Field number.  Character position is automatically calculated as required in FixedWidthFlatFileDataRow.
		public readonly short Length;
		public readonly AlignTypes AlignType;
		public readonly char PadChar;
	}

	public enum AlignTypes
	{
		None,
		Left,
		Right
	}
}
