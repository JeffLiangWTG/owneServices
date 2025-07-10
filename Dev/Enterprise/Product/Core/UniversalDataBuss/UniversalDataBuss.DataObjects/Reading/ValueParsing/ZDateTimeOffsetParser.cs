using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Reading.ValueParsing
{
	public class ZDateTimeOffsetParser : ZTypeBaseParser<ZDateTimeOffset>
	{
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Same as ZDateTime version")]
		protected override bool TryParse(string sourceValue, out ZDateTimeOffset typedResult)
		{
			return ZDateTimeOffset.TryParse(sourceValue, out typedResult) && typedResult.IsValid && typedResult.IsValidSmallDateTime;
		}

		protected override string ValidValueDescription
		{
			get { return "DateTimeOffset"; }
		}
	}
}
