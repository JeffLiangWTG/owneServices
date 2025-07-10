using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class JobComInvoiceGroupHeader : AutoJobComInvoiceGroupHeader, Integration.Customs.AsycudaCustoms.IJobComInvoiceGroupHeader
	{
		public JobComInvoiceGroupHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
