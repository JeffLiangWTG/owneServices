using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class ReferenceInvoiceManualCollection : Customs.Business.CusSupportingInfoCollection<ReferenceInvoiceManual>
	{
		public ReferenceInvoiceManualCollection(BusinessObject parent)
			: base(parent, CusSupportingInfoTypeList.Codes.ReferenceInvoiceManual)
		{
		}
	}
}
