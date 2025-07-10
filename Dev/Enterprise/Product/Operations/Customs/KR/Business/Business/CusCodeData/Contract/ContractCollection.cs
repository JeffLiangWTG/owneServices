using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ContractCollection : CusCodeDataCollection<Contract>
	{
		public ContractCollection(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader, CusCodeDataTypeList.Codes.Contract)
		{
		}
	}
}
