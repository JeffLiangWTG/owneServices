using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public class CompletionCustomsOfficeCollection : CusCodeDataCollection<CompletionCustomsOffice>
	{
		public CompletionCustomsOfficeCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.CompletionCustomsOffice)
		{
			MaxCountValidationEnable(MaxCountForValidation);
		}

		const int MaxCountForValidation = 999;
	}
}
