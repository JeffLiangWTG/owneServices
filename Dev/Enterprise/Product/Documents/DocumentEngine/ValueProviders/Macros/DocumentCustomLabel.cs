using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class DocumentCustomLabel : CustomLabel
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DocumentCustomLabel({customfieldname},{defaultlabeltext})>",
				ResString.GetMultilingualString("6167fc07-d7f9-4eec-acfd-e0b78680a59e",
				@"This functionality is used when Custom Labels are defined on the Organization the document is being produced for. Falls back to the Company Organization Proxy to find the Custom Label if not found. If no Custom Label is found for the specified custom field name, the default label text is used."),
				new List<(string example, object expectedResult)> { ((NoResString)"<DocumentCustomLabel(CustomText1, PalletType)>", "PalletValue") });
		}

		protected override string GetCustomLabelsType()
		{
			return OrgConstants.CustomLabelType.Document;
		}

		protected override OrgHeader GetConfigOrganisation(Report report)
		{
			return GetDocumentConfigOrganisation(report);
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)DocumentCustomLabel(?:[\s]*)\((?:[\s]*)([^\s]+)(?:[\s]*),(?:[\s]*)(.*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
