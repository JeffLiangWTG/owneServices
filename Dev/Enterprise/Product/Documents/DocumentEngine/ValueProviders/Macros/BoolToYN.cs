using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class BoolToYN : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<BoolToYN({boolvalue})>",
				ResString.GetMultilingualString("03ec2966-c7f4-4489-b121-94075095e62e", @"Returns the Y for true, N for false."),
				new List<(string example, object expectedResult)> { ("<BoolToYN(<ChargeCode.IsActive>)>", "Y"), ((NoResString)"<BoolToYN(<Is Active>)>", "N") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			string boolValueAsString = match.Groups[1].Value;

			if (string.IsNullOrEmpty(boolValueAsString))
			{
				return "";
			}

			return TryParseWithReportOnFail(
				report: report,
				defaultValue: string.Empty,
				errorMessage: string.Format((NoResString)"Value: '{0}', Error: {1}", boolValueAsString, "{0}"),
				parseFunc: () => new ZBool(bool.Parse(boolValueAsString)).ToYN());
		}

		public override Regex Regex
		{
			get { return regex; }
		}

		static readonly Regex regex = new Regex(@"^<\s*BoolToYN\s*\(\s*(.+)\s*\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
