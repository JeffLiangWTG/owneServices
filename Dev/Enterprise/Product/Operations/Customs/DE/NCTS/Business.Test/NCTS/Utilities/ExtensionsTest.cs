using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class ExtensionsTest : TestCaseWithFactory
	{
		public void TestGetDocAddressOrNullIfInvalid()
		{
			CombineAssertions(() =>
			{
				var docAddress = Factory.New<JobDocAddress>();
				AssertEquals("Invalid", null, docAddress.GetDocAddressOrNullIfInvalid());

				docAddress.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "TestOrg").PK;
				AssertSame("Valid", docAddress, docAddress.GetDocAddressOrNullIfInvalid());
			});
		}
	}
}
