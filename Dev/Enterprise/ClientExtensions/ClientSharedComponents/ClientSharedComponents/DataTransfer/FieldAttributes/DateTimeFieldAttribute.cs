using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.ClientSharedComponents
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DateTimeFieldAttribute : FieldAttribute
	{
		public DateTimeFieldAttribute(short position) : this(position, (short)defaultDateFormat.Length, defaultDateFormat, AlignTypes.None, ' ') { }
		public DateTimeFieldAttribute(short position, short length) : this(position, length, defaultDateFormat, AlignTypes.None, ' ') { }
		public DateTimeFieldAttribute(short position, string dateFormat) : this(position, (short)dateFormat.Length, dateFormat, AlignTypes.None, ' ') { }
		public DateTimeFieldAttribute(short position, short length, string dateFormat) : this(position, length, dateFormat, AlignTypes.None, ' ') { }
		public DateTimeFieldAttribute(short position, short length, string dateFormat, AlignTypes align, char padChar)
			: base(position, length, align, padChar)
		{
			if (string.IsNullOrEmpty(dateFormat))
			{
				dateFormat = defaultDateFormat;
			}
			DateTimeFormat = dateFormat;
		}

		protected override IZType GetValueCore(FlatFileDataRow rawData)
		{
			ZString rawString = rawData.GetField(Position).Replace("T", " ");
			ZDateTime result;
			if (!ZDateTime.TryParseExact(rawString, out result, DateTimeFormat))
			{
				result = ZDateTime.Empty;
			}
			return result;
		}

		protected override void SetValueAsStringAlignLeft(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ((ZDateTime)value).ToString(DateTimeFormat).PadRight(Length, PadChar).Substring(0, Length));
		}

		protected override void SetValueAsStringAlignRight(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, ((ZDateTime)value).ToString(DateTimeFormat).PadLeft(Length, PadChar).Substring(0, Length));
		}

		protected override void SetValueAsStringAlignNone(IZType value, FlatFileDataRow rawRow)
		{
			rawRow.SetField(Position, (ZDateTime)value, DateTimeFormat);
		}

		public readonly string DateTimeFormat;

		const string defaultDateFormat = "dd/MMM/yyyy";
	}
}
