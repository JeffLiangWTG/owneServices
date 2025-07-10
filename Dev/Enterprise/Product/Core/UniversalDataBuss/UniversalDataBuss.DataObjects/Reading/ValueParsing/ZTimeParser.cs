using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZTimeParser : ZTypeBaseParser<ZTime>
	{
		protected override bool TryParse(string sourceValue, out ZTime typedResult)
		{
			bool success = ZTime.TryParseExact(sourceValue, out ZTime dt, ZTime.TimeFormat);

			typedResult = dt;
			if (success && typedResult.IsValid)
			{
				return true;
			}

			typedResult = ZTime.Empty;
			return false;
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("{C8DAFEDE-7366-4146-9462-1E50B53C9F66}", "time between 00:00 and 23:59"); }
		}
	}
}
