using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public abstract class TypeSafeInvoiceCharge : AutoInvoiceCharge
{
	protected TypeSafeInvoiceCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
