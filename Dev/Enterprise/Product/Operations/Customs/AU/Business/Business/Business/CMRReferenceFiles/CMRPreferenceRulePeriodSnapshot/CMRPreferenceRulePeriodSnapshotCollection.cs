
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceRulePeriodSnapshotCollection : BusinessObjectCollection<CMRPreferenceRulePeriodSnapshot>
	{
		public CMRPreferenceRulePeriodSnapshotCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
