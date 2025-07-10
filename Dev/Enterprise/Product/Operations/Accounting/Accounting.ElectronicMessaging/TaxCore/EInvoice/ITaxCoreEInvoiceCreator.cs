namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public interface ITaxCoreEInvoiceCreator
	{
		ITaxCoreEInvoice Create(TaxCoreEInvoiceCreatorParameter eInvoiceCreatorParameter);
	}
}
