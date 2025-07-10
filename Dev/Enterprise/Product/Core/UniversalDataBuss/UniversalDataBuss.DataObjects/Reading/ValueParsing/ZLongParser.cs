using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZLongParser : ZTypeBaseParser<ZLong>
	{
		protected override bool TryParse(string sourceValue, out ZLong typedResult)
		{
			return ZLong.TryParse(sourceValue, out typedResult);
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("8930F004-9670-43F9-B417-CF330E6295E7", "Long"); }
		}
	}
}
