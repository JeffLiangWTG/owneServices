using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class MyAccountWebUserTest : OrgContactWebUserTest
	{
		protected override void AssertAllUserRelatedOrgs(params ZGuid[] expectedOrgsPK)
		{
			Assert(true);
		}

		public override void TestCanPublishCompanyLayouts()
		{
			// There's is no publish right for MyAccount
			Assert(true);
		}

		public void TestLoginCheckSupportContract()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDSCWSYD";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "sam@test.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("12340");
			Factory.Save();
			User.Login("DDDSCWSYD", "Sam@test.com", "12340");
			AssertNull("Org has no licence", org.LicCompany);
			Assert("Should login if no licence", User.IsLoggedIn);
			User.Logout();
			org.CreateAndLoadLicenceForOrg();
			var db1 = org.LicCompany.LicDatabases.AddNew();
			var licHeader1 = org.LicCompany.GetHeader(db1);
			db1.LD_ServerCode = "PRD";
			db1.LD_PublicEmailAddressForUpdate = "test@test.com";
			Factory.Save();
			User.Login("DDDSCWSYD", "Sam@test.com", "12340");
			Assert("Should login if no expiry date on database", User.IsLoggedIn);
			User.Logout();
			licHeader1.LA_ContractExpiryDate = ZDateTime.Now.AddDays(1);
			Factory.Save();
			User.Login("DDDSCWSYD", "Sam@test.com", "12340");
			Assert("Should login if contract is current", User.IsLoggedIn);
			User.Logout();
			licHeader1.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();
			User.Login("DDDSCWSYD", "Sam@test.com", "12340");
			Assert("Should NOT login if contract is expired", !User.IsLoggedIn);
			var db2 = org.LicCompany.LicDatabases.AddNew();
			db2.LD_ServerCode = "TST";
			db2.LD_PublicEmailAddressForUpdate = "test@test.com";
			var licHeader2 = org.LicCompany.GetHeader(db2);
			licHeader2.LA_ContractExpiryDate = ZDateTime.Now.AddDays(5);
			Factory.Save();
			User.Login("DDDSCWSYD", "Sam@test.com", "12340");
			Assert("Should login if at least one contract is current", User.IsLoggedIn);
			User.Logout();
			licHeader2.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-5);
			Factory.Save();
			User.Login("DDDSCWSYD", "Sam@test.com", "12340");
			Assert("Should NOT login if all contracts are expired", !User.IsLoggedIn);
			licHeader1.LA_IsActive = false;
			db2.LD_IsActive = false;
			Factory.Save();
			User.Login("DDDSCWSYD", "Sam@test.com", "12340");
			Assert("Should login if no active databases", User.IsLoggedIn);
			User.Logout();
		}

		public override void TestAllRelatedContactsAndOrgs()
		{
			Company.OH_FullName = "Borland";
			var company2 = CreateNewCompany();
			company2.OH_Code = "YYYYY";
			company2.OH_FullName = "Apple";
			OrgContact contact2 = CreateNewContact(UserName, Email, Password, company2);
			Factory.Save();
			User.Login(Company.OH_Code, Email, Password);
			Assert(User.IsLoggedIn);
			AssertNotNull("AllRelatedOrgs", User.AllUserRelatedOrgs);
			AssertEquals("AllRelatedOrgs.Count", 1, User.AllUserRelatedOrgs.Count);
			AssertNotNull("Should contain organization Borland", User.AllUserRelatedOrgs.FindByPK(Company.PK));
		}

		public void TestGetLoginContactsWithCurrentSupport()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "DDDSCWSYD";
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_Code = "AAAAGASYD";
			Factory.Save();
			org.CreateAndLoadLicenceForOrg();
			org2.CreateAndLoadLicenceForOrg();
			var db1 = org.LicCompany.LicDatabases.AddNew();
			var db21 = org2.LicCompany.LicDatabases.AddNew();
			var licHeader = org.LicCompany.GetHeader(db1);
			var licHeader21 = org2.LicCompany.GetHeader(db21);
			db1.LD_ServerCode = "PRD";
			db21.LD_ServerCode = "PRD";
			db1.LD_PublicEmailAddressForUpdate = "test@test.com";
			db21.LD_PublicEmailAddressForUpdate = "test@test.com";
			licHeader.LA_ContractExpiryDate = ZDateTime.Now.AddDays(1);
			licHeader21.LA_ContractExpiryDate = ZDateTime.Now.AddDays(-1);
			var commonEmail = "user@1.com";
			var commonPassword = "ChangeMe123!";
			var contact1 = CreateNewContact("user1", commonEmail, string.Empty, org);
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_IsActive = true;
			contact1.SetHashedPassword(commonPassword);
			var contact2 = CreateNewContact("user1", commonEmail, string.Empty, org2);
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_IsActive = true;
			contact2.SetHashedPassword(commonPassword);
			Factory.Save();
			var hasCurrentSupportResult = EDIOrgHeader.GetOrgsWithCurrentSupport(new[] { org.PK.ToGuid(), org2.PK.ToGuid() });
			AssertEquals("Precondition: Only one database and support expiry date is in the future", true, hasCurrentSupportResult.Contains(org.PK.ToGuid()));
			AssertEquals("Precondition: Only one database and support expiry date is in the past", false, hasCurrentSupportResult.Contains(org2.PK.ToGuid()));
			var (result, loginContacts) = User.GetLoginContacts(org.OH_Code, commonEmail, commonPassword);
			AssertEquals("Should only return contact1 since contact2's org has no support", LoginContactsResult.Success, result);
			AssertEquals("Should only return contact1 since contact2's org has no support", 1, loginContacts.Length);
			AssertEquals("Should only return contact1 since contact2's org has no support", contact1.PK, loginContacts[0].PK);
		}

		public override void TestCanAddNewOrganisations()
		{
			Assert(true);
		}

		protected new MyAccountWebUser User
		{
			get
			{
				return (MyAccountWebUser)base.User;
			}
		}

		protected override WebUser GetNewWebUser()
		{
			return new MyAccountWebUser();
		}
	}
}
