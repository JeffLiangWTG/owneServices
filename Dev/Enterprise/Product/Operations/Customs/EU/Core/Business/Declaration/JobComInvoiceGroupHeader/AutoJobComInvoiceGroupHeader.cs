using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class AutoJobComInvoiceGroupHeader : Customs.Business.BaseJobComInvoiceGroupHeader
	{
		protected AutoJobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
