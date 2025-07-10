using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public abstract class TypeSafeInvoiceApportionedCharge : AutoInvoiceApportionedCharge
{
	public TypeSafeInvoiceApportionedCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
