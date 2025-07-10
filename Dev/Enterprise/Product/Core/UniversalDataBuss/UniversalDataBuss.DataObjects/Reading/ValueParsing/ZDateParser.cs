using System.Globalization;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZDateParser : ZTypeBaseParser<ZDate>
	{
		protected override bool TryParse(string sourceValue, out ZDate typedResult)
		{
			bool success = ZDateTime.TryParseIgnoreTimezone(sourceValue, CultureInfo.InvariantCulture, out ZDateTime dt);
			typedResult = dt.Date;

			if (success && typedResult.IsValid && dt.IsValidSmallDateTime && typedResult >= minXmlDate)
			{
				return true;
			}

			typedResult = ZDate.Empty;
			return false;
		}

		protected override string ValidValueDescription
		{
			get { return Res.GetString("{9CA57EE4-6218-4C7D-89E7-7D6856FD3B83}", "date between 2-Jan-1900 and 6-Jun-2079"); }
		}

		readonly ZDateTime minXmlDate = new ZDate(1900, 1, 2);
	}
}
