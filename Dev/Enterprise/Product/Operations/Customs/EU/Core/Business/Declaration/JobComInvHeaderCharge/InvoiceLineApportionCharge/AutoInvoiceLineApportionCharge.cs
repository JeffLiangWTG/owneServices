using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public abstract class AutoInvoiceLineApportionCharge : Customs.Business.BaseInvoiceLineApportionedCharge
	{
		protected AutoInvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
