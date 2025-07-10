using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public class InvoiceApportionedCharge : TypeSafeInvoiceApportionedCharge, Integration.Customs.AE.IApportionedCharge
{
	public InvoiceApportionedCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
