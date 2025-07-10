
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentRatePeriodSnapshotCollection : BusinessObjectCollection<CMRTreatmentRatePeriodSnapshot>
	{
		public CMRTreatmentRatePeriodSnapshotCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
