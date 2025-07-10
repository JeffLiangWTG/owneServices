using Enterprise.MasterFiles.Business;
namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class TurkeyEInvoicingPreEligibilityProvider : EInvoicingPreEligibilityProvider
	{
		public override bool CanEvaluateByTransaction(AccTransactionHeader transaction) =>
			(AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.Value == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual)
			|| (transaction != null && !transaction.IsInDatabase);
	}
}
