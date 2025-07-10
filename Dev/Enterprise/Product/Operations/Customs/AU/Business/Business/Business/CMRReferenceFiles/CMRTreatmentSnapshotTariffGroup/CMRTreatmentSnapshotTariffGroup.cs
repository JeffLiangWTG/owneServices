
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentSnapshotTariffGroup : AutoCMRTreatmentSnapshotTariffGroup
	{
		public CMRTreatmentSnapshotTariffGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTreatmentSnapshotTariffGroup New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTreatmentSnapshotTariffGroup>();
		}
	}
}
