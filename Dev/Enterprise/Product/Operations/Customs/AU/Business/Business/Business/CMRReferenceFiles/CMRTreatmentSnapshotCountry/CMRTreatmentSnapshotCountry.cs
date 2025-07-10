
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentSnapshotCountry : AutoCMRTreatmentSnapshotCountry
	{
		public CMRTreatmentSnapshotCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTreatmentSnapshotCountry New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTreatmentSnapshotCountry>();
		}
	}
}
