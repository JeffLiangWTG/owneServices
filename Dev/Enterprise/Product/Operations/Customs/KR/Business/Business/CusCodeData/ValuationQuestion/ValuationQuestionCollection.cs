using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationQuestionCollection : CusCodeDataCollection<ValuationQuestion>
	{
		public ValuationQuestionCollection(JobComInvoiceHeader header)
			: base(header, CusCodeDataTypeList.Codes.ValuationQuestion)
		{
		}
	}
}
