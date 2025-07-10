using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZIntParser : ZTypeBaseParser<ZInt>
	{
		protected override bool TryParse(string sourceValue, out ZInt typedResult)
		{
			return ZInt.TryParse(sourceValue, out typedResult);
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("afedf236-714c-4528-9246-ed749afd6823", "Integer"); }
		}
	}
}
