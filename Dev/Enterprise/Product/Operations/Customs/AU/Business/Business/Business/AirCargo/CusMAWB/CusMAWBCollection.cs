
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusMAWBCollection : BusinessObjectCollection<CusMAWB>
	{
		public CusMAWBCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
