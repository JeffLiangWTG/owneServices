using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class JordanQRCodeDataProvider : IQRCodeDataProvider
	{
		public string GetTransactionQRCodeString(InvoicingBase invoicing)
		{
			var transactionHeaderAuthorisationRecord = AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(invoicing.Factory, invoicing.PK, invoicing.Company.Country.RN_Code);

			return transactionHeaderAuthorisationRecord?.AHF_VerificationUrl ?? string.Empty;
		}
	}
}
