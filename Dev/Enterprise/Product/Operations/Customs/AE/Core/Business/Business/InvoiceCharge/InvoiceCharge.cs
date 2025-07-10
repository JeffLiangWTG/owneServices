using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public class InvoiceCharge : TypeSafeInvoiceCharge, Integration.Customs.AE.IInvoiceCharge
{
	public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
