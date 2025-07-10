namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	internal interface IComplianceReportAdditionalDataHeaderDetailsProvider
	{
		(string extraColumns, string extraConditions) GetHeaderExtraColumnsAndConditions(ComplianceReportDataCollectionMode mode);
	}
}
