namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	internal interface IComplianceReportAdditionalDataLineDetailsProvider
	{
		(string extraJoin, string extraConditions) GetLineExtraJoinsAndConditions(ComplianceReportDataCollectionMode mode);
	}
}
