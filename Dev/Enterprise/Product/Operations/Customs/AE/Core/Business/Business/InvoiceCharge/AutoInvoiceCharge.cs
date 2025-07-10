using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public abstract class AutoInvoiceCharge : Customs.Business.BaseInvoiceCharge
{
	protected AutoInvoiceCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
