
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPermitRequirementExclusions : AutoCMRPermitRequirementExclusions
	{
		public CMRPermitRequirementExclusions(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRPermitRequirementExclusions New(BusinessObjectFactory factory)
		{
			return factory.New<CMRPermitRequirementExclusions>();
		}
	}
}
