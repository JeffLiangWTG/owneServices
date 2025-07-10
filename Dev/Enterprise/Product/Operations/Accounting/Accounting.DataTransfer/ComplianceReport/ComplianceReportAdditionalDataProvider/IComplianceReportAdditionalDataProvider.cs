namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	internal interface IComplianceReportAdditionalDataProvider
	{
		bool GetIsValidForCustomers(string ledger, string transactionType);

		bool GetIsValidForSuppliers(string ledger, string transactionType);
	}
}
