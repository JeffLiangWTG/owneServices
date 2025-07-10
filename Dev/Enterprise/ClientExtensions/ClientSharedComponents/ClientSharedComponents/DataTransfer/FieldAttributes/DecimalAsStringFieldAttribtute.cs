using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DecimalAsStringFieldAttribute : DecimalFieldAttribute
	{
		public DecimalAsStringFieldAttribute(short position) : this(position, 7, 0, AlignTypes.None, '0') { }
		public DecimalAsStringFieldAttribute(short position, short length) : this(position, length, 0, AlignTypes.None, '0') { }
		public DecimalAsStringFieldAttribute(short position, short length, byte decimalPlaces) : this(position, length, decimalPlaces, AlignTypes.None, '0') { }
		public DecimalAsStringFieldAttribute(short position, short length, byte decimalPlaces, AlignTypes align) : this(position, length, decimalPlaces, align, '0') { }
		public DecimalAsStringFieldAttribute(short position, short length, byte decimalPlaces, AlignTypes align, char padChar)
			: base(position, length, decimalPlaces, align, padChar) { }

		protected override IZType GetValueCore(FlatFileDataRow rawData)
		{
			ZString value = rawData.GetField(Position);
			if (value.IsEmpty || !ZDecimal.CanParse(value))
			{
				value = "0";
			}
			return ConvertStringToDecimal(value);
		}

		protected override void SetValueAsStringAlignLeft(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ConvertDecimalToString((ZDecimal)value).PadRight(Length, PadChar).Substring(0, Length));
		}

		protected override void SetValueAsStringAlignRight(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ConvertDecimalToString((ZDecimal)value).PadLeft(Length, PadChar).Substring(0, Length));
		}

		protected override void SetValueAsStringAlignNone(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ConvertDecimalToString((ZDecimal)value));
		}

		string ConvertDecimalToString(ZDecimal decimalValue)
		{
			string result = string.Empty;
			if (!decimalValue.IsEmpty)
			{
				result = Decimal.Round(decimalValue, DecimalPlaces).ToString();
			}
			return result;
		}

		ZDecimal ConvertStringToDecimal(ZString stringValue)
		{
			return Decimal.Round(Convert.ToDecimal(stringValue), DecimalPlaces);
		}
	}
}
