using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IQRCodeDataProvider
	{
		string GetTransactionQRCodeString(InvoicingBase invoicing);
	}
}
