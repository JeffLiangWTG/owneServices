using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class AutoGroupInvoiceCharge : Customs.Business.BaseGroupInvoiceCharge
	{
		protected AutoGroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
