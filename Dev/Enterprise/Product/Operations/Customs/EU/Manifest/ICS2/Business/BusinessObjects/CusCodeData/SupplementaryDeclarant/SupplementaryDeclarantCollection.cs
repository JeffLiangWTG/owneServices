using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SupplementaryDeclarantCollection : CusCodeDataCollection<SupplementaryDeclarant>
	{
		public SupplementaryDeclarantCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.EUICS2SupplementaryDeclarant)
		{
			MaxCountValidationEnable(99);
		}
	}
}
