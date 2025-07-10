using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public abstract class AutoInvoiceLineCharge : Customs.Business.BaseInvoiceLineCharge
{
	protected AutoInvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
