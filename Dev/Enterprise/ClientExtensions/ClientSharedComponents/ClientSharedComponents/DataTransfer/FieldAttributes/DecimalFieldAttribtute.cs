using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ClientSharedComponents
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DecimalFieldAttribute : FieldAttribute
	{
		public DecimalFieldAttribute(short position) : this(position, 7, 0, AlignTypes.None, '0') { }
		public DecimalFieldAttribute(short position, short length) : this(position, length, 0, AlignTypes.None, '0') { }
		public DecimalFieldAttribute(short position, short length, byte decimalPlaces) : this(position, length, decimalPlaces, AlignTypes.None, '0') { }
		public DecimalFieldAttribute(short position, short length, byte decimalPlaces, AlignTypes align) : this(position, length, decimalPlaces, align, '0') { }
		public DecimalFieldAttribute(short position, short length, byte decimalPlaces, AlignTypes align, char padChar)
			: base(position, length, align, padChar)
		{
			DecimalPlaces = decimalPlaces;
		}

		protected override IZType GetValueCore(FlatFileDataRow rawData)
		{
			return rawData.GetFieldAsZDecimal(Position, DecimalPlaces);
		}

		protected override void SetValueAsStringAlignLeft(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, Utilities.Round((ZDecimal)value, DecimalPlaces).ToString().PadRight(Length, PadChar).Substring(0, Length));
		}

		protected override void SetValueAsStringAlignRight(IZType value, FlatFileDataRow rawRow)
		{
			String valueAsString = String.Format(formatMask, new string(PadChar, Length - DecimalPlaces - 1), new string(PadChar, DecimalPlaces));
			rawRow.SetField(Position, Utilities.Round((ZDecimal)value, DecimalPlaces).ToString(valueAsString).Substring(0, Length));
		}
		const string formatMask = "{0}.{1}";

		protected override void SetValueAsStringAlignNone(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, (ZDecimal)value, DecimalPlaces);
		}

		public readonly byte DecimalPlaces;
	}
}
