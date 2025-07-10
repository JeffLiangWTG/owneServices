using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public abstract class AutoInvoiceApportionedCharge : Customs.Business.BaseApportionedCharge
{
	public AutoInvoiceApportionedCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
