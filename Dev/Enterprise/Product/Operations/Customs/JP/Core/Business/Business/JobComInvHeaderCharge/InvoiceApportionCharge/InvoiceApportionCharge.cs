using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Business
{
	public partial class InvoiceApportionCharge : AutoInvoiceApportionCharge
	{
		public InvoiceApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
