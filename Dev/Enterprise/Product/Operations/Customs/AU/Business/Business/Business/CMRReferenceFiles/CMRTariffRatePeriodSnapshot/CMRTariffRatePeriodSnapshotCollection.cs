
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffRatePeriodSnapshotCollection : BusinessObjectCollection<CMRTariffRatePeriodSnapshot>
	{
		public CMRTariffRatePeriodSnapshotCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public CMRTariffRatePeriodSnapshotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
