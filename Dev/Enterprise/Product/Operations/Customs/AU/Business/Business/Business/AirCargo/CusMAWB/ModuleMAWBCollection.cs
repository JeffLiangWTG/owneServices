
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ModuleMAWBCollection : BusinessObjectCollection<CusMAWB>
	{
		public ModuleMAWBCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
