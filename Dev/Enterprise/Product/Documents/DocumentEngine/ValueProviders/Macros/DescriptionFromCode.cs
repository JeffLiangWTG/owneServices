using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class DescriptionFromCode : ValueProvider
	{
		public const string JobServices = "JOBSERVICES";
		public const string VoyageAccountingSubGroups = "VOYAGEACCOUNTINGSUBGROUPS";

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DescriptionFromCode({codetype},{code} [,{fallback}])>",
				ResString.GetMultilingualString("00e62cf2-57a6-40da-9763-a366505670c6",
				@"Gets a description for the specified code using the code type to obtain the correct list of codes/descriptions. Currently the only code types supported are '{0}' and '{1}' which reads from a user definable list of available Job Services and Voyage Accounting Sub Groups in the Registry.
An optional fallback can be provided for cases where the code does not exist in the specified list.",
				"JobServices", "VoyageAccountingSubGroups"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<DescriptionFromCode(VoyageAccountingSubGroups, POC)>", (NoResString)"Port Charge"),
					((NoResString)"<DescriptionFromCode(VoyageAccountingSubGroups, AAA, Fallback)>", (NoResString)"Fallback") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = fRegex.Match(macro);
			string type = match.Groups[1].Value;
			string code = match.Groups[2].Value;
			string fallBack = string.IsNullOrEmpty(match.Groups[3].Value) ? null : match.Groups[3].Value;
			object result = "";

			switch (type.ToUpperInvariant())
			{
				case JobServices:
					result = FreightDataRegistry.Instance.JobServices.Value.GetDescriptionFromCode(code) ?? fallBack;
					break;
				case VoyageAccountingSubGroups:
					result = LinerAgencyDataRegistry.Instance.DisbursementSubGroups.Value.GetDescriptionFromCode(code) ?? fallBack;
					break;
				default:
					ReportMacroError(report, Res.GetString("e39145ba-e5ea-4b2d-afed-0870d7b2768c", "Type {0} is not defined.", type));
					break;
			}

			return result;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*Description\s*From\s*Code\(\s*([^\s,]+)\s*,\s*([^\s,]*)\s*(?:,\s*([^\s,]*)\s*)?\)\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
