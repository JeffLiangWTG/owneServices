
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentSnapshotCollection : BusinessObjectCollection<CMRTreatmentSnapshot>
	{
		public CMRTreatmentSnapshotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
