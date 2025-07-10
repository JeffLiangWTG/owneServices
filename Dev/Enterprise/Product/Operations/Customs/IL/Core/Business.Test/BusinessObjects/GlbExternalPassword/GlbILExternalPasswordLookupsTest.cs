using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	public sealed class GlbILExternalPasswordLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPasswordStatusList()
		{
			var externalPassword = Factory.New<GlbILExternalPassword>();
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
				AssertEquals("AWA description should be 'Registered'", "Registered", regDescription);
				AssertSame("Both lists should be the same instance", list1, list2);
			});
		}

		public void TestStaff()
		{
			var staffWithoutPassword = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutPassword.GS_Code = "AAA";
			staffWithoutPassword.CompanyName = GlbCompany.CurrentCompany.CompanyName;

			var staffWithPassword = Factory.NewWithValidTestData<GlbStaff>();
			staffWithPassword.GS_Code = "BBB";
			staffWithPassword.CompanyName = GlbCompany.CurrentCompany.CompanyName;
			var pwd = Factory.New<GlbILStaffExternalPassword>();
			pwd.GP_CertificateAuthority = "COM";
			pwd.CurrentDecryptedPassword = "1234";
			pwd.GP_GS = staffWithPassword.PK;

			Factory.Save();
			var externalPassword = Factory.New<GlbILExternalPassword>();
			var staffCollection1 = externalPassword.Lookups.Staff;
			var staffCollection2 = externalPassword.Lookups.Staff;

			var actualWithPassword = staffCollection1.FindByPK(staffWithPassword.PK);
			var actualWithoutPassword = staffCollection1.FindByPK(staffWithoutPassword.PK);

			CombineAssertions(() =>
			{
				AssertNotNull("Collection should contain staff with password", actualWithPassword);
				AssertNull("Collection should not contain staff without password", actualWithoutPassword);
				AssertSame("Both collections should be the same instance", staffCollection1, staffCollection2);
			});
		}
	}
}
