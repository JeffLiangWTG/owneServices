using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;

namespace Enterprise.Customs.JP.AFR.Module
{
	public class JPAFRHeaderModuleCollection : ActiveBusinessObjectCollection<JPAFRHeader>
	{
		public JPAFRHeaderModuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
