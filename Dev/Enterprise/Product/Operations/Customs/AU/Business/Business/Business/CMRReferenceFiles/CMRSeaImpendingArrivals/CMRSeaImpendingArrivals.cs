
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRSeaImpendingArrivals : AutoCMRSeaImpendingArrivals
	{
		public CMRSeaImpendingArrivals(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRSeaImpendingArrivals New(BusinessObjectFactory factory)
		{
			return factory.New<CMRSeaImpendingArrivals>();
		}
	}
}
