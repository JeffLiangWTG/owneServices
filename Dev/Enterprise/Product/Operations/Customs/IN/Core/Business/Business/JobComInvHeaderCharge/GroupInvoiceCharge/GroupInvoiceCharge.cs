using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public partial class GroupInvoiceCharge : AutoGroupInvoiceCharge
{
	public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
