using System.Globalization;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZDateTimeParser : ZTypeBaseParser<ZDateTime>
	{
		protected override bool TryParse(string sourceValue, out ZDateTime typedResult)
		{
			bool success = ZDateTime.TryParseIgnoreTimezone(sourceValue, CultureInfo.InvariantCulture, out ZDateTime dt);
			typedResult = dt;
			return success && typedResult.IsValidSmallDateTime && typedResult >= minXmlDateTime;
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("4c1f8d3f-2dab-4688-86c9-4d416232e2a6", "date between 2-Jan-1900 and 6-Jun-2079"); }
		}

		readonly ZDateTime minXmlDateTime = new ZDateTime(1900, 1, 2, 0, 0, 0);
	}
}
