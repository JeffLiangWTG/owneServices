using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.China
{
	public class ChinaQueueInvoiceForTransmissionProvider : IQueueInvoiceForTransmissionProvider
	{
		public bool ShouldShowQueueInvoiceForTransmissionMenuItem(string ledgerType) => ledgerType == LedgerTypes.AccountsReceivable && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

		public bool ShouldQueueTransactionForTransmission(TransactionHeader transaction) => transaction.AH_TransactionType == TransactionTypes.Invoice &&
							!transaction.AH_ComplianceSubType.IsEmpty &&
							transaction.AH_ComplianceDocumentDate.IsEmpty &&
							transaction.AH_TransactionReference.IsEmpty &&
							transaction.EInvoicingProxy.IsEligibleToCreatePivot();

		public string GetAdditionalErrorMessage() => Res.GetString("A8725054-1A1B-4EE4-B487-7DFB00B32D71", @"* Transaction Header Branch Does not have a value recorded against 'E-Invoicing Credentials' under Accounting > E-Reporting and E-Invoicing Configurations > China registry.
* Transaction Type is not INV.
* Compliance Sub Type is blank.
* E-Reporting Status is not blank.
* Compliance Number/Date is not blank.");
	}
}
