using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZGeographyParser : ZTypeBaseParser<ZGeography>
	{
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Same as ZDateTime version")]
		protected override bool TryParse(string sourceValue, out ZGeography typedResult)
		{
			return ZGeography.TryParse(sourceValue, out typedResult) && typedResult.IsValid;
		}

		protected override string ValidValueDescription
		{
			get { return "SqlGeography"; }
		}
	}
}
