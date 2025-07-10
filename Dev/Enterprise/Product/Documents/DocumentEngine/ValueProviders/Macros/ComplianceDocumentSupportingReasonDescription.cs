using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ComplianceDocumentSupportingReasonDescription : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ComplianceDocumentSupportingReasonDescription({supportingReasonCode},{ledger})>"
				, ResString.GetMultilingualString("909FDE5C-F3C5-4396-B34A-4489F2CC241D", "Will return the description of the compliance supporting reason code."),
				new List<(string example, object expectedResult)> { ((NoResString)"<ComplianceDocumentSupportingReasonDescription(COD, AR)>", "REASON") });
		}
		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var supportingReasonCode = match.Groups["SupportingReasonCode"].Value;
			var ledger = match.Groups["Ledger"].Value;

			if (ledger == LedgerTypes.AccountsPayable)
			{
				return AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsPayables.Value.FindByCode(supportingReasonCode)?.Description ?? ZString.Empty;
			}
			else
			{
				return AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingReasonsReceivables.Value.FindByCode(supportingReasonCode)?.Description ?? ZString.Empty;
			}
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<ComplianceDocumentSupportingReasonDescription\((?:\s*)(?<SupportingReasonCode>.*)(?:\s*),(?:\s*)(?<Ledger>\S*)(?:\s*)\)(?:\s*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
