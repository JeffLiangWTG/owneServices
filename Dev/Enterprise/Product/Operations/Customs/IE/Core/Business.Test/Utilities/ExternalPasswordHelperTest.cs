using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class ExternalPasswordHelperTest : TestCaseWithFactory
	{
		public void TestGetHashedPassword()
		{
			AssertEquals("Value should be hashed correctly", ROSCertificateTestHelper.ValidHashedPassword, ExternalPasswordHelper.GetHashedPassword(ROSCertificateTestHelper.ValidPassword));
		}
	}
}
