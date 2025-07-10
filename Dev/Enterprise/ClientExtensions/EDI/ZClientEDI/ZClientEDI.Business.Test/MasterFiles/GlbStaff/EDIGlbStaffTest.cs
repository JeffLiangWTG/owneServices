using System;
using System.Data;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.HR;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIGlbStaff))]
	public class EDIGlbStaffTest : EnterpriseBusinessObjectTestCase
	{
		EDIOrgHeader organisation1;
		LicenceDatabase database1;
		EDIOrgHeader organisation2;
		LicenceDatabase database2;
		GlbBranch branch;

		void PrepareTestData()
		{
			organisation1 = Factory.NewWithValidTestData<EDIOrgHeader>();

			organisation1.CreateAndLoadLicenceForOrg();
			organisation1.LicenceEnterpriseCode = "LE1";
			organisation1.LicCompany.LC_CompanyCode = "LC1";

			database1 = organisation1.LicCompany.LicDatabases.AddNew();
			database1.LD_ServerCode = "LD1";
			database1.LD_DatabaseNumber = 5;
			database1.LD_OH_WebAccessOrg = organisation1.PK;

			branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "ZZZ";
			var clientBranch1 = Factory.New<ClientBranch>();
			clientBranch1.LCB_Code = "ZZZ";
			clientBranch1.LCB_LD = database1.PK;
			clientBranch1.LCB_OA = organisation1.MainAddress.PK;

			organisation2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			organisation2.CreateAndLoadLicenceForOrg();
			organisation2.LicenceEnterpriseCode = "LE2";
			organisation2.LicCompany.LC_CompanyCode = "LC2";

			database2 = organisation2.LicCompany.LicDatabases.AddNew();
			database2.LD_ServerCode = "LD2";
			database2.LD_DatabaseNumber = 7;
			database2.LD_OH_WebAccessOrg = organisation2.PK;

			var clientBranch2 = Factory.New<ClientBranch>();
			clientBranch2.LCB_Code = "ZZZ";
			clientBranch2.LCB_LD = database2.PK;
			clientBranch2.LCB_OA = organisation2.MainAddress.PK;

			Factory.Save();
		}

		public void TestCreateContact_Db1()
		{
			PrepareTestData();
			var staff = CreateStaff(5);

			AssertEquals(0, organisation1.Contacts.Count);
			AssertEquals(0, organisation2.Contacts.Count);

			Factory.Save();

			organisation1.Contacts.Reload(true);
			organisation2.Contacts.Reload(true);
			AssertEquals(1, organisation1.Contacts.Count);
			AssertEquals(0, organisation2.Contacts.Count);

			AssertStaff(staff, organisation1);
		}

		public void TestCreateContact_Db2()
		{
			PrepareTestData();
			var staff = CreateStaff(7);

			AssertEquals(0, organisation1.Contacts.Count);
			AssertEquals(0, organisation2.Contacts.Count);

			Factory.Save();

			organisation1.Contacts.Reload(true);
			organisation2.Contacts.Reload(true);
			AssertEquals(0, organisation1.Contacts.Count);
			AssertEquals(1, organisation2.Contacts.Count);

			AssertStaff(staff, organisation2);
		}

		public void TestCreateContact_NoLicenceDb()
		{
			PrepareTestData();
			var staff = CreateStaff(2);

			AssertEquals(0, organisation1.Contacts.Count);
			AssertEquals(0, organisation2.Contacts.Count);

			Factory.Save();

			AssertEquals(0, organisation1.Contacts.Count);
			AssertEquals(0, organisation2.Contacts.Count);
		}

		EDIGlbStaffForTest CreateStaff(int dbNum)
		{
			var staff = Factory.New<EDIGlbStaffForTest>();
			staff.GS_FullName = "full name";
			staff.GS_Title = "title";
			staff.GS_WorkingLanguage = "UK-UA";
			staff.GS_EmailAddress = "email@address.com";
			staff.GS_WorkPhone = "123";
			staff.GS_MobilePhone = "456";
			staff.GS_GB_HomeBranch = branch.PK;
			staff.DbNum = dbNum;
			return staff;
		}

		void AssertStaff(EDIGlbStaffForTest staff, EDIOrgHeader org, bool isWebAccessEnabled = true)
		{
			AssertEquals("full name", org.Contacts[0].OC_ContactName);
			AssertEquals("title", org.Contacts[0].OC_Title);
			AssertEquals("UK-UA", org.Contacts[0].OC_Language);
			AssertEquals(isWebAccessEnabled ? "email@address.com" : string.Empty, org.Contacts[0].OC_Email);
			AssertEquals("+123", org.Contacts[0].OC_Phone);
			AssertEquals("+456", org.Contacts[0].OC_Mobile);
			AssertEquals(org.MainAddress.PK, org.Contacts[0].OC_OA_OrgAddress);
			AssertEquals(isWebAccessEnabled, org.Contacts[0].OC_WebAccessEnabled);
			AssertEquals(staff.GS_PER, org.Contacts[0].OC_PER);

			var userAccounts = Factory.Load<EdiCustomerUserAccount>(new ZQuery(EdiCustomerUserAccountSchema.EUA_OC_WebAccessContact, org.Contacts[0].PK));
			AssertEquals(1, userAccounts.Length);
			AssertEquals(staff.GS_Code, userAccounts[0].EUA_UserID);
			AssertEquals(staff.GS_FullName, userAccounts[0].EUA_FullName);
			AssertEquals(staff.GS_EmailAddress, userAccounts[0].EUA_Email);
		}

		public void TestCalendarEmailAddress()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			AssertEquals("Pre-condition", "", staff.CalendarEmailAddress);

			staff.Notes.AddNew(false, EDIPredefinedNoteTypes.Instance.StaffCalendarEmailAddress.Description, "resource.samuel.wang@cargowise.com");
			AssertEquals("resource.samuel.wang@cargowise.com", staff.CalendarEmailAddress);

			Factory.Save();

			var loadedStaff = new BusinessObjectFactory().Load<EDIGlbStaff>(staff.PK);
			AssertEquals("resource.samuel.wang@cargowise.com", loadedStaff.CalendarEmailAddress);
		}

		public void TestOnSavingProfilePhoto()
		{
			Factory.RefreshEnabled = false;
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			Factory.Save();
			var changes = Factory.Load<EdiStaffChange>(new ZQuery());
			AssertEquals(0, changes.Length);

			using (var img = new Bitmap(8, 8))
			{
				using (var g = Graphics.FromImage(img))
				{
					g.Clear(Color.Green);
				}
				staff.ProfileImage = img;
			}
			Factory.Save();

			changes = new BusinessObjectFactory() { RefreshEnabled = false }
				.Load<EdiStaffChange>(new ZQuery());
			AssertEquals(1, changes.Length);
			AssertEquals(true, changes[0].ES9_IsProfilePhoto);
			EdiStaffChange.ClearStaffChangesTable();

			staff.GS_ProfilePhoto = ZBlob.Empty;
			Factory.Save();
			changes = new BusinessObjectFactory() { RefreshEnabled = false }
				.Load<EdiStaffChange>(new ZQuery());
			AssertEquals("removing photo is ignored", 0, changes.Length);
		}

		public void TestStaffEx()
		{
			var staff1 = Factory.NewWithValidTestData<EDIGlbStaff>();
			var resource1 = Factory.NewWithValidTestData<EDIGlbStaff>();
			var systemAccount1 = Factory.NewWithValidTestData<EDIGlbStaff>();
			resource1.GS_IsResource = true;
			systemAccount1.GS_IsSystemAccount = true;
			Factory.Save();
			AssertNull("PRE", staff1.ReadonlyStaffEx);
			AssertEquals("PRE", false, staff1.HasChanges);

			AssertNotNull(staff1.StaffEx);
			AssertEquals("creating StaffEx does not set HasChanges", false, staff1.HasChanges);

			AssertNull(resource1.StaffEx);
			AssertEquals("calling StaffEx does not set HasChanges", false, resource1.HasChanges);

			AssertNull(systemAccount1.StaffEx);
			AssertEquals("calling StaffEx does not set HasChanges", false, systemAccount1.HasChanges);
		}

		public void TestSetStaffMainEmailShouldUpdateContactOnSaving()
		{
			PrepareTestData();
			var staff = CreateStaff(5);
			staff.GS_EmailAddress = string.Empty;
			AssertEquals("Precondition", 0, organisation1.Contacts.Count);

			Factory.Save();

			organisation1.Contacts.Reload(true);
			organisation2.Contacts.Reload(true);
			AssertEquals("Precondition", 1, organisation1.Contacts.Count);

			AssertStaff(staff, organisation1, false);

			var address = staff.EmailAddresses.FirstOrDefault(x => x.GSE_Type.EqualsIgnoringCase(Core.Constants.EmailFromAddressTypes.Codes.Main));
			AssertNotNull(address);
			AssertEquals("Precondition: Main email address should be set", staff.GS_EmailAddress, address.GSE_EmailAddress);

			address.GSE_EmailAddress = "new.email@address.com";
			staff.GS_FullName = "new name";
			staff.GS_Title = "New Title";
			staff.GS_WorkingLanguage = "EN";
			staff.GS_WorkPhone = "234";
			staff.GS_MobilePhone = "567";
			Factory.Save();

			organisation1.Contacts.Reload(true);
			AssertEquals("Email should have propagated to contact", "new.email@address.com", organisation1.Contacts[0].OC_Email);
			AssertEquals("Title should have propagated to contact", "New Title", organisation1.Contacts[0].OC_Title);
			AssertEquals("Working language should have propagated to contact", "EN", organisation1.Contacts[0].OC_Language);
			AssertEquals("Work Phone should have propagated to contact", "+234", organisation1.Contacts[0].OC_Phone);
			AssertEquals("Mobile Phone should have propagated to contact", "+567", organisation1.Contacts[0].OC_Mobile);
		}

		public void TestChangeStaffMainEmailShouldUpdateContactOnSaving()
		{
			PrepareTestData();
			var staff = CreateStaff(5);
			AssertEquals("Precondition", 0, organisation1.Contacts.Count);

			Factory.Save();

			organisation1.Contacts.Reload(true);
			organisation2.Contacts.Reload(true);
			AssertEquals("Precondition", 1, organisation1.Contacts.Count);

			AssertStaff(staff, organisation1);

			var address = staff.EmailAddresses.FirstOrDefault(x => x.GSE_Type.EqualsIgnoringCase(Core.Constants.EmailFromAddressTypes.Codes.Main));
			AssertNotNull(address);
			AssertEquals("Precondition: Main email address should be set", staff.GS_EmailAddress, address.GSE_EmailAddress);

			address.GSE_EmailAddress = "new.email@address.com";
			staff.GS_FullName = "new name";
			staff.GS_Title = "New Title";
			staff.GS_WorkingLanguage = "EN";
			staff.GS_WorkPhone = "234";
			staff.GS_MobilePhone = "567";
			Factory.Save();

			organisation1.Contacts.Reload(true);
			AssertEquals("Email should have propagated to contact", "new.email@address.com", organisation1.Contacts[0].OC_Email);
			AssertEquals("Title should have propagated to contact", "New Title", organisation1.Contacts[0].OC_Title);
			AssertEquals("Working language should have propagated to contact", "EN", organisation1.Contacts[0].OC_Language);
			AssertEquals("Work Phone should have propagated to contact", "+234", organisation1.Contacts[0].OC_Phone);
			AssertEquals("Mobile Phone should have propagated to contact", "+567", organisation1.Contacts[0].OC_Mobile);
		}

		public void TestChangePropertiesOtherThanEmailShouldNotUpdateContactOnSaving()
		{
			PrepareTestData();
			var staff = CreateStaff(5);
			AssertEquals("Precondition", 0, organisation1.Contacts.Count);

			Factory.Save();

			organisation1.Contacts.Reload(true);
			organisation2.Contacts.Reload(true);
			AssertEquals("Precondition", 1, organisation1.Contacts.Count);

			AssertStaff(staff, organisation1);

			var address = staff.EmailAddresses.FirstOrDefault(x => x.GSE_Type.EqualsIgnoringCase(Core.Constants.EmailFromAddressTypes.Codes.Main));
			AssertNotNull(address);
			AssertEquals("Precondition: Main email address should be set", staff.GS_EmailAddress, address.GSE_EmailAddress);

			staff.GS_FullName = "new name";
			staff.GS_Title = "New Title";
			staff.GS_WorkingLanguage = "EN";
			staff.GS_WorkPhone = "234";
			staff.GS_MobilePhone = "567";

			Factory.Save();

			organisation1.Contacts.Reload(true);
			AssertEquals("Title should not have propagated to contact", "title", organisation1.Contacts[0].OC_Title);
			AssertEquals("Working language should not have propagated to contact", "UK-UA", organisation1.Contacts[0].OC_Language);
			AssertEquals("Work Phone should not have propagated to contact", "+123", organisation1.Contacts[0].OC_Phone);
			AssertEquals("Mobile Phone should not have propagated to contact", "+456", organisation1.Contacts[0].OC_Mobile);
		}

		public void TestDomesticName()
		{
			var staff1 = Factory.NewWithValidTestData<EDIGlbStaff>();
			var staff2 = Factory.NewWithValidTestData<EDIGlbStaff>();
			var staffEx = Factory.NewWithValidTestData<EdiGlbStaffEx>();
			staffEx.GS9_DomesticName = "Domestic Name Test";
			staffEx.GS9_GS = staff2.PK;
			Factory.Save();

			AssertEquals("", staff1.DomesticName);
			AssertEquals("Domestic Name Test", staff2.DomesticName);
		}

		public void TestLoadOrCreateApplicantAndOrgContactWhenNewStaffIsCreated()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "MEO");
			var database = licHeader.Database;
			database.LD_DatabaseNumber = 9907;
			Factory.Save();

			#region new
			var testEmail = "MeowBigTail@123.com";
			var staff = Factory.NewWithValidTestData<EDIGlbStaffForTest>();
			staff.DbNum = database.LD_DatabaseNumber;
			staff.GS_Gender = "M";
			staff.GS_EmailAddress = testEmail;
			AssertNoExceptionThrown(() => Factory.Save());

			AssertForLoadOrCreateOrgContactWhenNewStaffIsCreated("creating", staff);
			#endregion

			#region Update Person & email
			var updateEmail = "SmallTail@123.com";
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();
			newPerson.PER_FullName = "Not default name";
			staff.GS_EmailAddress = updateEmail;
			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals("GlbStaff's email isn't updated", updateEmail, staff.GS_EmailAddress);
			AssertForLoadOrCreateOrgContactWhenNewStaffIsCreated("update", staff);
			#endregion
		}

		public void TestUpdateFromPerson()
		{
			var staff = Factory.New<EDIGlbStaff>();
			var person = Factory.New<GlbPerson>();

			person.PER_FullName = "Name Person";
			staff.GS_FullName = "Name Staff";

			staff.UpdateFromPerson(person);
			AssertEquals(staff.GS_FullName, "Name Person");

			using (staff.Factory.SetTempContext(EDIConstants.BusinessContext.VersionReportContactImporter))
			{
				person.PER_FullName = "Name Person New";

				staff.UpdateFromPerson(person);
				AssertEquals(staff.GS_FullName, "Name Person");
			}
		}

		void AssertForLoadOrCreateOrgContactWhenNewStaffIsCreated(string stage, GlbStaff staff)
		{
			var orgContact = Factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.OC_PER, staff.GS_PER));
			AssertNotNull($"OrgContact isn't expected after the {stage}", orgContact);
			Assert($"OrgContact's email should be equal to the GlbStaff after the {stage}", orgContact.OC_Email.Equals(staff.GS_EmailAddress));
		}

		public void TestSendGitHubInvite()
		{
			Env.Security.StaffGroups.IsAllowed = false;

			var githubGroup = Factory.NewWithValidTestData<GlbGroup>();
			githubGroup.GG_Code = "GHUSERS";
			githubGroup.GG_Desc = "GITHUB USERS";
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			Factory.Save();

			Env.Security.FindOrCreateGroupOwnerSecurityCheckpoint(githubGroup.PK.ToGuid()).IsAllowed = false;
			Env.Security.FindOrCreateChangeGroupSecurityCheckpoint(githubGroup.PK.ToGuid()).IsAllowed = false;

			AssertEquals("staff should have no group attached event(s)", 0, GetGroupEventCount(AutoEvents.Attached.Description, staff));
			AssertEquals("staff should have no group detached event(s)", 0, GetGroupEventCount(AutoEvents.Detached.Description, staff));

			githubGroup.GG_Code = "NODEFAULTGITHUB";
			Factory.Save();
			AssertEquals("registry will be empty guid as it will use the default GG_Code(GHUSERS) to locate PK", Guid.Empty, EDIDataRegistry.Instance.GitHubUsersGroup.Value);

			staff.SendGitHubInvite();
			AssertEquals("staff should have no group attached event(s)", 0, GetGroupEventCount(AutoEvents.Attached.Description, staff));
			AssertEquals("staff should have no group detached event(s)", 0, GetGroupEventCount(AutoEvents.Detached.Description, staff));

			githubGroup.GG_Code = "GHUSERS";
			Factory.Save();

			using (EDIDataRegistry.Instance.GitHubUsersGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, githubGroup.PK.ToGuid()))
			{
				staff.SendGitHubInvite();
				AssertEquals("staff should have 1 group attached event(s)", 1, GetGroupEventCount(AutoEvents.Attached.Description, staff));
				AssertEquals("staff should have no group detached event(s)", 0, GetGroupEventCount(AutoEvents.Detached.Description, staff));
				Assert("staff should belong to GitHub group", staff.Groups.Contains(githubGroup));
				Assert("staff's GlbGroupLink should not have errors", !Factory.Load<GlbGroupLink>(new ZQuery(GlbGroupLinkSchema.GK_GG, githubGroup.PK)).FirstOrDefault().HasErrors);

				staff.SendGitHubInvite();
				AssertEquals("staff should have 2 group attached event(s)", 2, GetGroupEventCount(AutoEvents.Attached.Description, staff));
				AssertEquals("staff should have 1 group detached event(s)", 1, GetGroupEventCount(AutoEvents.Detached.Description, staff));
				Assert("staff should belong to GitHub group", staff.Groups.Contains(githubGroup));
				Assert("staff's GlbGroupLink should not have errors", !Factory.Load<GlbGroupLink>(new ZQuery(GlbGroupLinkSchema.GK_GG, githubGroup.PK)).FirstOrDefault().HasErrors);
			}

			int GetGroupEventCount(string actionName, EDIGlbStaff staff) =>
				staff.Logs.Find(e => e.SL_SE_NKEvent == AutoEvents.EditedARecord.Code && e.SL_Reference == $"{actionName} - (GHUSERS) GITHUB USERS").Count();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			AssertEquals(1, staff.Groups.Count);
			return staff;
		}

		public class EDIGlbStaffForTest : EDIGlbStaff
		{
			public EDIGlbStaffForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override StaffContactImporter GetContactImporter()
			{
				var mockRego = new Mock<IProductRegistration>();
				mockRego.Setup(m => m.Key.DatabaseNumber).Returns(DbNum);
				using (ObjectFactory.Substitute(mockRego.Object))
				{
					return new StaffContactImporter(Factory);
				}
			}

			public int DbNum { get; set; }
		}
	}
}
