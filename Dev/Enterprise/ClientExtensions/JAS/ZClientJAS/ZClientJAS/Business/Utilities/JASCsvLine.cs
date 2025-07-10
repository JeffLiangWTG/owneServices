
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;

namespace Enterprise.Client.JAS.Business.Utilities
{
	public class JASCsvLine : OCsvLine
	{
		public JASCsvLine(string line, char delimiter)
			: base(line, delimiter)
		{
		}

		protected JASCsvLine(string[] fieldValues, bool[] includeQuotes)
			: base(fieldValues, includeQuotes)
		{
		}

		public ZString GetFieldValue(int fieldPosition)
		{
			return (fieldPosition >= 0 && fieldPosition < FieldValues.Length) ? FieldValues[fieldPosition] : "";
		}

		public ZByte GetByteFieldValue(int fieldPosition)
		{
			ZByte result = ZByte.Zero;
			ZString byteText = GetFieldValue(fieldPosition);
			return (ZByte.TryParse(byteText, out result)) ? result : ZByte.Zero;
		}

		public ZInt GetIntFieldValue(int fieldPosition)
		{
			ZInt result = ZInt.Zero;
			ZString intText = GetFieldValue(fieldPosition);
			return (ZInt.TryParse(intText, out result)) ? result : ZInt.Zero;
		}

		public ZDateTime GetDateTimeFieldValue(int fieldPosition)
		{
			ZDateTime result = ZDateTime.Empty;
			ZString dateTimeText = GetFieldValue(fieldPosition);
			return (ZDateTime.TryParseExact(dateTimeText, out result, JXCConstants.DateFormat)) ? result : ZDateTime.Empty;
		}

		public ZDecimal GetDecimalFieldValue(int fieldPosition)
		{
			ZDecimal result = ZDecimal.Zero;
			ZString decimalText = GetFieldValue(fieldPosition);
			return (ZDecimal.TryParse(decimalText, out result)) ? result : ZDecimal.Zero;
		}
	}
}
