
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentSnapshot : AutoCMRTreatmentSnapshot
	{
		public CMRTreatmentSnapshot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTreatmentSnapshot New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTreatmentSnapshot>();
		}
	}
}
