
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisCommodityCollection : BusinessObjectCollection<CMRAqisCommodity>
	{
		public CMRAqisCommodityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
