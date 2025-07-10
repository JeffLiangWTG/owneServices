using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public class IdentificationMeansCodeCollection : CusCodeDataCollection<IdentificationMeansCode>
	{
		public IdentificationMeansCodeCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.IdentificationMeansCode)
		{
			MaxCountValidationEnable(MaxCountForValidation);
		}

		const int MaxCountForValidation = 7;
	}
}
