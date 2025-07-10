using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class ComplementaryLogisticInvoiceCollection : Customs.Business.CusSupportingInfoCollection<ComplementaryLogisticInvoice>
	{
		public ComplementaryLogisticInvoiceCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.ComplementaryLogisticInvoice)
		{
		}
	}
}
