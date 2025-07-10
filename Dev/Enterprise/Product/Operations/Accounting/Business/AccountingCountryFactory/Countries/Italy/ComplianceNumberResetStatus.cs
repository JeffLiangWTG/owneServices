using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Italy
{
	class ComplianceNumberResetStatus : IComplianceNumberResetStatus
	{
		bool IComplianceNumberResetStatus.CheckIsComplianceNumberResetAllowedForSubmittedEInvoice(IComplianceNumberResetStatusInputData inputData)
		{
			var eReportingTransactionNumber = AccountingMasterFilesRegistry.Instance.EReportingTransactionNumber.Value;

			return eReportingTransactionNumber == TransactionNumberCodes.InvoiceNr;
		}
	}
}
