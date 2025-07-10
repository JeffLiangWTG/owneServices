using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCommunityProtectionRiskMessageAdvice))]
	sealed class CMRCommunityProtectionRiskMessageAdviceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => CMRCommunityProtectionRiskMessageAdvice.New(Factory);
	}
}
