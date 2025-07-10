using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DecimalAsIntFieldAttribute : DecimalFieldAttribute
	{
		public DecimalAsIntFieldAttribute(short position) : this(position, 7, 0, AlignTypes.None, '0') { }
		public DecimalAsIntFieldAttribute(short position, short length) : this(position, length, 0, AlignTypes.None, '0') { }
		public DecimalAsIntFieldAttribute(short position, short length, byte decimalPlaces) : this(position, length, decimalPlaces, AlignTypes.None, '0') { }
		public DecimalAsIntFieldAttribute(short position, short length, byte decimalPlaces, AlignTypes align) : this(position, length, decimalPlaces, align, '0') { }
		public DecimalAsIntFieldAttribute(short position, short length, byte decimalPlaces, AlignTypes align, char padChar)
			: base(position, length, decimalPlaces, align, padChar) { }

		protected override IZType GetValueCore(FlatFileDataRow rawData)
		{
			return ConvertIntToDecimal(rawData.GetFieldAsZInt(Position));
		}

		protected override void SetValueAsStringAlignLeft(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ConvertDecimalToInt((ZDecimal)value).ToString().PadRight(Length, PadChar).Substring(0, Length));
		}

		protected override void SetValueAsStringAlignRight(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ConvertDecimalToInt((ZDecimal)value).ToString().PadLeft(Length, PadChar).Substring(0, Length));
		}

		protected override void SetValueAsStringAlignNone(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ConvertDecimalToInt((ZDecimal)value).ToString());
		}

		int ConvertDecimalToInt(ZDecimal decimalValue)
		{
			return Convert.ToInt32(decimalValue * (decimal)(Math.Pow(10, DecimalPlaces)));
		}

		ZDecimal ConvertIntToDecimal(ZInt integerValue)
		{
			return Convert.ToDecimal(integerValue / (decimal)(Math.Pow(10, DecimalPlaces)));
		}
	}
}
