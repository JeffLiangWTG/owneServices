
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceSchemePeriodSnapshotCollection : BusinessObjectCollection<CMRPreferenceSchemePeriodSnapshot>
	{
		public CMRPreferenceSchemePeriodSnapshotCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
