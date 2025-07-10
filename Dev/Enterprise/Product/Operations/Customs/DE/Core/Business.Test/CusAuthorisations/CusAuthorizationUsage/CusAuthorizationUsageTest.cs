using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusAuthorizationUsage))]
	class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestLookups()
		{
			NUnit.Framework.Assert.That(Factory.New<CusAuthorizationUsage>().Lookups, Is.TypeOf<CusAuthorizationUsageLookups>());
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			NUnit.Framework.Assert.That(Factory.New<CusAuthorizationUsage>().Validation, Is.TypeOf<CusAuthorizationUsageValidation>());
		}
	}
}
