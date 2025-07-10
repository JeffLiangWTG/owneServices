using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	class ComplianceReportAdditionalDataProviderSAFT1_30 : IComplianceReportAdditionalDataProvider
	{
		bool IComplianceReportAdditionalDataProvider.GetIsValidForCustomers(string ledger, string transactionType)
		{
			return ledger == LedgerTypes.AccountsReceivable;
		}

		bool IComplianceReportAdditionalDataProvider.GetIsValidForSuppliers(string ledger, string transactionType)
		{
			return ledger == LedgerTypes.AccountsPayable;
		}
	}
}
