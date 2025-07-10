using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public class CusIntrastatHeaderCollection : ActiveBusinessObjectCollection<CusIntrastatHeader>
	{
		public CusIntrastatHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CusIntrastatHeaderCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}
	}
}
