using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class IcsOfficeCodeCollection : CusCodeDataCollection<IcsOfficeCode>
	{
		public IcsOfficeCodeCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.OfficeCode)
		{
		}
	}
}
