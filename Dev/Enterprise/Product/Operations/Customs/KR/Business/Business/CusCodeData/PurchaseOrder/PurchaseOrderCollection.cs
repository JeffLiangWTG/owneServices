using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class PurchaseOrderCollection : CusCodeDataCollection<PurchaseOrder>
	{
		public PurchaseOrderCollection(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader, CusCodeDataTypeList.Codes.PurchaseOrder)
		{
		}
	}
}
