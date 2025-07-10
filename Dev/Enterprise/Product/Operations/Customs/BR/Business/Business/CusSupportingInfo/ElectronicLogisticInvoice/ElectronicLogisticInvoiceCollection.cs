using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class ElectronicLogisticInvoiceCollection : Customs.Business.CusSupportingInfoCollection<ElectronicLogisticInvoice>
	{
		public ElectronicLogisticInvoiceCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.ElectronicLogisticInvoice)
		{
		}
	}
}
