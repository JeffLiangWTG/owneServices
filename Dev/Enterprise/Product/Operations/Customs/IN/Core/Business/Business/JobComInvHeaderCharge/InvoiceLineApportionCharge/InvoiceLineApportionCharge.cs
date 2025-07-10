using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public partial class InvoiceLineApportionCharge : AutoInvoiceLineApportionCharge
{
	public InvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
