using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class InvoiceLineCharge : AutoInvoiceLineCharge, Integration.Customs.AsycudaCustoms.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
