using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZShortParser : ZTypeBaseParser<ZShort>
	{
		protected override bool TryParse(string sourceValue, out ZShort typedResult)
		{
			return ZShort.TryParse(sourceValue, out typedResult);
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("baeaa7fe-bf3d-47fe-81a1-2cdac0265da4", "Short Integer"); }
		}
	}
}
