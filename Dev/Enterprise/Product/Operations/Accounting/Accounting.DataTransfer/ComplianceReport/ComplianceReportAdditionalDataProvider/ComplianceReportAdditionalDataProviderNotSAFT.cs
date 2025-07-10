namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	class ComplianceReportAdditionalDataProviderNotSAFT : IComplianceReportAdditionalDataLineDetailsProvider, IComplianceReportAdditionalDataProvider
	{
		public (string extraJoin, string extraConditions) GetLineExtraJoinsAndConditions(ComplianceReportDataCollectionMode mode)
		{
			var extraJoin = "";
			var extraCondition = "AND AL_LineType NOT IN ('WIP', 'ACR')";    // Hardcoded part of SQL statement

			return (extraJoin, extraCondition);
		}

		bool IComplianceReportAdditionalDataProvider.GetIsValidForCustomers(string ledger, string transactionType)
		{
			return true;
		}

		bool IComplianceReportAdditionalDataProvider.GetIsValidForSuppliers(string ledger, string transactionType)
		{
			return true;
		}
	}
}
