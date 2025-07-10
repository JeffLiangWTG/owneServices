using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MX.Business
{
	public partial class InvoiceCharge : AutoInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
