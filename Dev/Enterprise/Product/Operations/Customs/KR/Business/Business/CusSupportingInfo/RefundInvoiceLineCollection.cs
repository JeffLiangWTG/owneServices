using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class RefundInvoiceLineCollection : CusSupportingInfoCollection<RefundInvoiceLine>
	{
		public RefundInvoiceLineCollection(CusReconEntryLine parent)
			: base(parent, CusSupportingInfoTypeList.Codes.RefundInvoiceLine)
		{
		}
	}
}
