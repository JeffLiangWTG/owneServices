
using CargoWise.Types;

namespace Enterprise.Client.OSP.Data_Import
{
	public abstract class OSPDataRow
	{
		protected virtual string DateTimeFormat
		{
			get { return "yyyyMMdd"; }
		}

		protected ZDateTime ToZDateTime(string value)
		{
			ZDateTime result;
			return ZDateTime.TryParseExact(value, out result, DateTimeFormat) ? result : ZDateTime.Empty;
		}

		protected ZBool ToZBool(string value)
		{
			return value.Trim() == "Y";
		}

		protected ZInt ToZInt(string value)
		{
			ZInt result;
			return ZInt.TryParse(value, out result) ? result : (ZInt)0;
		}

		protected ZDecimal ToZDecimal(string value)
		{
			ZDecimal result;
			return ZDecimal.TryParse(value, out result) ? result : 0;
		}
	}
}
