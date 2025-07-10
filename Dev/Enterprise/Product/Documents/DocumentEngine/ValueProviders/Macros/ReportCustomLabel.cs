using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class ReportCustomLabel : CustomLabel
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ReportCustomLabel({customfieldname}, {defaultlabeltext})>",
				ResString.GetMultilingualString("d41d0810-7dd2-4d1e-969c-773d83a5474a",
				@"This functionality is used when Custom Labels are defined on the Organization the report is being produced for. Falls back to the Company Organization Proxy to find the Custom Label if not found. 
If no Custom Label is found for the specified custom field name, the default label text is used."),
				new List<(string example, object expectedResult)> { ((NoResString)"<ReportCustomLabel(CustomText1, PalletType)>", "PalletValue") });
		}

		protected override string GetCustomLabelsType()
		{
			return OrgConstants.CustomLabelType.Report;
		}

		protected override OrgHeader GetConfigOrganisation(Report report)
		{
			return GetReportConfigOrganisation(report);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)ReportCustomLabel(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*),(?:[\s]*)(.*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
