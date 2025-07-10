using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.CH.Business;

public class DocFinanceDataWrapper : DocBaseWrapper
{
	public static DocFinanceDataWrapper New(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factoryToWrap)
		=> new DocFinanceDataWrapper(invoiceHeader, factoryToWrap);

	DocFinanceDataWrapper(JobComInvoiceHeader invoiceHeader, BusinessObjectFactory factory)
		: base(invoiceHeader, factory)
	{
		dataProvider = FinanceDataProvider.New(invoiceHeader);
	}
	readonly FinanceDataProvider dataProvider;

	public ZString Incoterms => dataProvider.Incoterms;

	public ZString VatNumber => dataProvider.VatNumber;

	public ZString CustomsInvoiceRecipient => dataProvider.CustomsInvoiceRecipient;
}
