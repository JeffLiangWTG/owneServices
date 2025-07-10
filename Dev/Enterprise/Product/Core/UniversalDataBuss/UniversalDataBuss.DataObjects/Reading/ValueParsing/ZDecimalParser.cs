using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZDecimalParser : ZTypeBaseParser<ZDecimal>
	{
		protected override bool TryParse(string sourceValue, out ZDecimal typedResult)
		{
			return ZDecimal.TryParse(sourceValue, out typedResult);
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("fc1afe9f-7642-441a-b7ec-e91c96705d50", "Decimal"); }
		}
	}
}
