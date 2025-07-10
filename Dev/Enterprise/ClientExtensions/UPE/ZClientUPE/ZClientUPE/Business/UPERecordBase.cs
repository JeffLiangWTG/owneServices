using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPERecordBase
	{
		protected virtual string DateTimeFormat => "ddMMMyyyy";

		internal protected ZDateTime ToZDateTime(string value) => ZDateTime.TryParseExact(value, out var result, DateTimeFormat) ? result : ZDateTime.Empty;

		protected ZBool ToZBool(string value) => value.Trim() == "Y";

		protected ZInt ToZInt(string value) => ZInt.TryParse(value, out var result) ? result : (ZInt)0;

		protected ZDecimal ToZDecimal(string value) => ZDecimal.TryParse(value, out var result) ? result : 0;
	}
}
