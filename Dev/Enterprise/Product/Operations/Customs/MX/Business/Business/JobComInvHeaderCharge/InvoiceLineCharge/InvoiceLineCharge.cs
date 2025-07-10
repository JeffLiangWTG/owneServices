using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.MX.Business
{
	public partial class InvoiceLineCharge : AutoInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
