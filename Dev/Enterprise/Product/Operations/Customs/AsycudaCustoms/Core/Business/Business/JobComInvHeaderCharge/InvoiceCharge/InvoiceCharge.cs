using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class InvoiceCharge : AutoInvoiceCharge, Integration.Customs.AsycudaCustoms.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
