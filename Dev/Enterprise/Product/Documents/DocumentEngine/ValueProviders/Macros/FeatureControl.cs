using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class FeatureControl : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<FeatureControl({code})>",
				ResString.GetMultilingualString("d5311dcc-e48c-409c-8f12-6d5a7992b886",	"Returns whether the specified feature is enabled. Returns 'Y' if enabled, 'N' if disabled."),
				new List<(string example, object expectedResult)> {
					("<FeatureControl(ICEGHGCAL)>", "N"),
				});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var featureCode = match.Groups[1].Value.Trim();

			if (string.IsNullOrEmpty(featureCode))
			{
				ReportMacroError(report, Res.GetString("4ff5b6d3-2831-478c-9f6d-4b5d62541bce", "Feature code must be provided"));
				return "N";
			}

			var isEnabled = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(featureCode) != null;
			return ((ZBool)isEnabled).ToYN();
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(
			@"^<\s*FeatureControl\s*\(\s*([A-Za-z][A-Za-z0-9]*)\s*\)\s*>$",
			RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
