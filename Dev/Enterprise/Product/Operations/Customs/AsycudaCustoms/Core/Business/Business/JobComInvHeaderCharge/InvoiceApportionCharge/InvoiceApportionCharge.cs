using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class InvoiceApportionCharge : AutoInvoiceApportionCharge, Integration.Customs.AsycudaCustoms.IInvoiceApportionCharge
	{
		public InvoiceApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
