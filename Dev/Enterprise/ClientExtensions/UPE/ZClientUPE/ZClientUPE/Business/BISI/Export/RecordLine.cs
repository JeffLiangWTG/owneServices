
using CargoWise.Types;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business.BISI
{
	public abstract class RecordLine
	{
		public ZString LineAsString
		{
			get
			{
				ZStringBuilder lineBuilder = new ZStringBuilder();
				AppendFields(lineBuilder);
				return lineBuilder.ToString();
			}
		}

		protected void AppendFixedLengthField(ZStringBuilder lineBuilder, ZBool value, int fieldLength)
		{
			ZString valueAsString = (value) ? "1" : "0";
			AppendFixedLengthField(lineBuilder, valueAsString, fieldLength);
		}

		protected void AppendFixedLengthField(ZStringBuilder lineBuilder, ZDecimal value, int decimalPlace, int fieldLength)
		{
			ZString valueAsString = value.ToString(decimalPlace).Replace(Culture.Current.NumberFormat.NumberDecimalSeparator, "").PadLeft(fieldLength, '0');
			AppendFixedLengthField(lineBuilder, valueAsString, fieldLength);
		}

		protected void AppendFixedLengthField(ZStringBuilder lineBuilder, ZDateTime value, bool asTime, int fieldLength)
		{
			ZString valueAsString = (asTime) ? value.ToString("HH:mm:ss") : value.ToString("yyyy-MM-dd");
			AppendFixedLengthField(lineBuilder, valueAsString, fieldLength);
		}

		protected void AppendFixedLengthField(ZStringBuilder lineBuilder, ZInt value, int fieldLength)
		{
			ZString valueAsString = value.ToString().PadLeft(fieldLength, '0');
			AppendFixedLengthField(lineBuilder, valueAsString, fieldLength);
		}

		protected void AppendFixedLengthField(ZStringBuilder lineBuilder, ZString value, int fieldLength)
		{
			ZString fieldValue = value.Left(fieldLength);
			if (fieldValue.Length < fieldLength)
			{
				fieldValue = fieldValue.PadRight(fieldLength);
			}
			lineBuilder.Append(fieldValue);
		}

		#region Abstract

		protected abstract void AppendFields(ZStringBuilder lineBuilder);

		#endregion
	}
}
