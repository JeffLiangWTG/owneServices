
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCommunityProtectionRiskMessageAdvice : AutoCMRCommunityProtectionRiskMessageAdvice
	{
		public CMRCommunityProtectionRiskMessageAdvice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRCommunityProtectionRiskMessageAdvice New(BusinessObjectFactory factory)
		{
			return factory.New<CMRCommunityProtectionRiskMessageAdvice>();
		}
	}
}
