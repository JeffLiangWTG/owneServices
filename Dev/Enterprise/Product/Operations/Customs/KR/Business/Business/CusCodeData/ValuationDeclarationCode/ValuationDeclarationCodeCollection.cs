using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationDeclarationCodeCollection : CusCodeDataCollection<ValuationDeclarationCode>
	{
		public ValuationDeclarationCodeCollection(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader, CusCodeDataTypeList.Codes.ValuationDeclarationCode)
		{
		}
	}
}
