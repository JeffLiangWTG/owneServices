using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class SuspensionDrawbackInvoiceCollection : Customs.Business.CusSupportingInfoCollection<SuspensionDrawbackInvoice>
	{
		public SuspensionDrawbackInvoiceCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.SuspensionDrawbackInvoice)
		{
		}
	}
}
