using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	class ZStringParser : ZTypeBaseParser<ZString>
	{
		protected override bool TryParse(string sourceValue, out ZString typedResult)
		{
			typedResult = sourceValue;
			return true;
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("d464b1d2-74ba-4b90-b552-520fa3f1b207", "String"); }
		}
	}
}