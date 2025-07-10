using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public class ReimportCountryCodeCollection : CusCodeDataCollection<ReimportCountryCode>
	{
		public ReimportCountryCodeCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.ReimportCountryCode)
		{
			MaxCountValidationEnable(MaxCountForValidation);
		}

		const int MaxCountForValidation = 99;
	}
}
