using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class GlbCompanyCredentialLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPasswordStatusList()
		{
			var externalPassword = Factory.New<GlbCompanyCredential>();
			var list1 = externalPassword.Lookups.PasswordStatusList;
			var list2 = externalPassword.Lookups.PasswordStatusList;

			var codes = list1.GetAllCodes().ToList();
			var awaDescription = list1.GetDescriptionFromCode("AWA");
			var regDescription = list1.GetDescriptionFromCode("REG");

			CombineAssertions(() =>
			{
				Assert("List should contain AWA code", codes.Contains("AWA"));
				Assert("List should contain REG code", codes.Contains("REG"));
				AssertEquals("AWA description should be 'Awaiting Response'", "Awaiting Response", awaDescription);
				AssertEquals("REG description should be 'Registered'", "Registered", regDescription);
				AssertSame("Both lists should be the same instance", list1, list2);
			});
		}
	}
}
