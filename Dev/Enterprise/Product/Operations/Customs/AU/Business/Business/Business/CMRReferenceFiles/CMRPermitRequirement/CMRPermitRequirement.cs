
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPermitRequirement : AutoCMRPermitRequirement
	{
		public CMRPermitRequirement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRPermitRequirement New(BusinessObjectFactory factory)
		{
			return factory.New<CMRPermitRequirement>();
		}
	}
}
