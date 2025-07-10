using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZByteParser : ZTypeBaseParser<ZByte>
	{
		protected override bool TryParse(string sourceValue, out ZByte typedResult)
		{
			return ZByte.TryParse(sourceValue, out typedResult);
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("e087ecbb-23b0-400b-8b81-b030f7707c33", "Byte"); }
		}
	}
}
