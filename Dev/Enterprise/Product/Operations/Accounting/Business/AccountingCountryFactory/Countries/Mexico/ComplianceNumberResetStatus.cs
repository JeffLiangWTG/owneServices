namespace Enterprise.Accounting.Business.AccountingCountryFactory.Mexico
{
	class ComplianceNumberResetStatus : IComplianceNumberResetStatus
	{
		bool IComplianceNumberResetStatus.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(IComplianceNumberResetStatusInputData inputData) => true;
	}
}
