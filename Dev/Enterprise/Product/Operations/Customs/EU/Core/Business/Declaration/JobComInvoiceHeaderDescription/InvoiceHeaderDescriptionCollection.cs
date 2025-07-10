using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceHeaderDescriptionCollection : CusCodeDataCollection<InvoiceHeaderDescription>
	{
		public InvoiceHeaderDescriptionCollection(JobComInvoiceHeader header)
			: base(header, CusCodeDataTypeList.Codes.DescriptionCode)
		{
		}
	}
}
