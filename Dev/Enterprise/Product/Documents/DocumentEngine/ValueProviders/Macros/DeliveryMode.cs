using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class DeliveryMode : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DeliveryMode>",
				ResString.GetMultilingualString("de145251-d196-47eb-91d6-527e142dcc06", "This macro returns the 'Type' in 'Templates Used', which is the delivery medium used for that template, for example either the printer, email or fax."),
				new List<(string example, object expectedResult)> { ("<DeliveryMode>", "PRN") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return report.DeliveryMode;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Delivery(?:[\s]*)Mode(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
