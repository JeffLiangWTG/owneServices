namespace Enterprise.Customs.IN.Business;

public class InvoiceLineApportionChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
{
	public InvoiceLineApportionChargeLookups(InvoiceLineApportionCharge invoiceLineApportionCharge)
		: base(invoiceLineApportionCharge)
	{
	}
}
