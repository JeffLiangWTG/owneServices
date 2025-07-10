using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public class CusIntrastatGroupCollection : ActiveBusinessObjectCollection<CusIntrastatGroup>
	{
		public CusIntrastatGroupCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public CusIntrastatGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
