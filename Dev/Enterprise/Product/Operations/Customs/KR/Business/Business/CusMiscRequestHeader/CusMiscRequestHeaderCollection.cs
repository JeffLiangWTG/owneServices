using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class CusMiscRequestHeaderCollection : Customs.Business.CusMiscRequestHeaderCollection
	{
		public CusMiscRequestHeaderCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory, company)
		{
		}
	}
}
