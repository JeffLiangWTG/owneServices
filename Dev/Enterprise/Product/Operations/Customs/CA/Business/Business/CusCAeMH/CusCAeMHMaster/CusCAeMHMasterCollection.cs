using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHMasterCollection : ActiveBusinessObjectCollection<CusCAeMHMaster>
	{
		public CusCAeMHMasterCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }
	}
}
