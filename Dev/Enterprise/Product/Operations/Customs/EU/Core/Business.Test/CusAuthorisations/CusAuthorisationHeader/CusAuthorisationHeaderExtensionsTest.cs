using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusAuthorisationHeaderExtensionsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestTypeThatHaveAGuidLOCRuleValueFrom()
		{
			NUnit.Framework.Assert.That(CusAuthorisationHeaderExtensions.TypeThatHaveAGuidLOCRuleValueFrom.Count, NUnit.Framework.Is.EqualTo(1), "Number of entries");
			NUnit.Framework.Assert.That(CusAuthorisationHeaderExtensions.TypeThatHaveAGuidLOCRuleValueFrom, NUnit.Framework.Has.Some.EqualTo(UniversalReferenceConstants.CusAuthorisationHeaderType.CustomsAuthorizedLocations).Using(CustomComparers.TypeComparison), "Has CustomsAuthorizedLocation CLO");
		}
	}
}
