using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI.Test
{
	[TestedType(typeof(LicenceEnterpriseForm))]
	public class LicenceEnterpriseFormTest : ZFormBasherTest
	{
		public void TestTokenAuthenticationInGridOfLicenceEnterpriseForm()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database1 = licence1.Database;
			var org1 = database1.LicEnterprise.Organisation;

			database1.LD_LicenceType = DatabaseTypes.Codes.Production;
			database1.LD_TokenAuthenticationEnabled = true;
			database1.LD_StaffFirstReportUtc = new ZDateTime(2019, 1, 1);
			database1.LD_OH_WebAccessOrg = org1.PK;

			Factory.Save();

			var enterprise = database1.LicEnterprise;
			using (var form = new LicenceEnterpriseForm(enterprise))
			{
				form.Show();

				var grid = form.Controls.Find("DbDetailsGrid", true)[0] as ZGrid;
				var column = grid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(column => column.Caption == "Token Authentication");
				AssertNotNull(column);
			}
		}

		public void TestOriginalOrgContactShouldBeInactiveWhenUpdatingLicenceEnterpriseOrg()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database1 = licence1.Database;
			var org1 = database1.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			database1.LD_LicenceType = DatabaseTypes.Codes.Production;
			database1.LD_StaffFirstReportUtc = new ZDateTime(2019, 1, 1);
			database1.LD_OH_WebAccessOrg = org1.PK;

			var licence2 = BillingTestHelper.CreateAnotherDatabase(licence1, "MEL");
			var database2 = licence2.Database;

			database2.LD_LicenceType = DatabaseTypes.Codes.Test;
			database2.LD_StaffFirstReportUtc = new ZDateTime(2019, 1, 1);
			database2.LD_OH_WebAccessOrg = org1.PK;

			var licence3 = BillingTestHelper.CreateAnotherLicence(database1, "TST");
			var org2 = licence3.Company.Header;
			org2.Contacts.RemoveAndDeleteAll();

			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "User One";
			contact1.OC_Email = "user.one@test.org";
			contact1.OC_WebAccessEnabled = true;
			contact1.SetHashedPassword("123456");
			contact1.OC_OA_OrgAddress = org1.MainAddress.PK;
			contact1.OC_IsActive = true;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database1.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1.PK;

			var userAccount2 = Factory.New<EdiCustomerUserAccount>();
			userAccount2.EUA_LD = database2.PK;
			userAccount2.EUA_UserID = "US1";
			userAccount2.EUA_FullName = "User One";
			userAccount2.EUA_Email = "user.one@test.org";
			userAccount2.EUA_OC_WebAccessContact = contact1.PK;

			Factory.Save();

			database1.Lookups.WebAccessOrgs.Reload(true);
			database2.Lookups.WebAccessOrgs.Reload(true);

			var enterprise = database1.LicEnterprise;
			using (var form = new LicenceEnterpriseForm(enterprise))
			{
				form.Show();

				AssertEquals("Precondition", true, contact1.OC_IsActive);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				enterprise.LE_OH = org2.PK;
				form.FireSaveButton();

				AssertEquals("Contact is cloned to new web access org", 1, org2.Contacts.Count);
				var contact2 = org2.Contacts.OfType<EDIOrgContact>().FirstOrDefault(x => x.OC_ContactName == "User One");
				AssertEquals("Will be falso because EDIContact.CanSupersede sets to false", false, contact2.OC_IsActive);
				AssertEquals("User One", contact2.OC_ContactName);
				AssertEquals("user.one@test.org", contact2.OC_Email);
				AssertEquals(contact2.PK, userAccount1.EUA_OC_WebAccessContact);
				AssertEquals(contact1.OC_PER, contact2.OC_PER);

				contact1.Reload();
				AssertEquals(false, contact1.OC_IsActive);
				ErrorReporter.Instance.Clear();
			}
		}

		public void TestChangeEnterpriseOrg()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence1.Database;
			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.Contacts.RemoveAndDeleteAll();

			database.LD_LicenceType = DatabaseTypes.Codes.Production;
			database.LD_StaffFirstReportUtc = new ZDateTime(2019, 1, 1);
			database.LD_OH_WebAccessOrg = org1.PK;

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_WebAccessEnabled = true;
			contact1a.SetHashedPassword("123456");
			contact1a.OC_OA_OrgAddress = org1.MainAddress.PK;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			Factory.Save();

			database.Lookups.WebAccessOrgs.Reload(true);

			var enterprise = database.LicEnterprise;
			using (var form = new LicenceEnterpriseForm(enterprise))
			{
				form.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				enterprise.LE_OH = org2.PK;
				form.FireSaveButton();

				AssertEquals("Contact is cloned to new web access org", 1, org2.Contacts.Count);
				var contact2a = org2.Contacts[0];
				AssertEquals(true, contact2a.OC_IsActive);
				AssertEquals("User One", contact2a.OC_ContactName);
				AssertEquals("user.one@test.org", contact2a.OC_Email);
				AssertEquals(contact2a.PK, userAccount1.EUA_OC_WebAccessContact);
				AssertEquals(contact1a.OC_PER, contact2a.OC_PER);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				enterprise.LE_OH = org1.PK;
				form.FireSaveButton();

				AssertEquals("No new contact if matched one is found", 1, org1.Contacts.Count);
				AssertEquals(true, contact1a.OC_IsActive);
				AssertEquals("User One", contact1a.OC_ContactName);
				AssertEquals("user.one@test.org", contact1a.OC_Email);
				AssertEquals(contact1a.PK, userAccount1.EUA_OC_WebAccessContact);
				AssertEquals(contact1a.OC_PER, contact2a.OC_PER);
			}
		}

		public void TestChangeEnterpriseOrg_Rollback()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence1.Database;
			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.Contacts.RemoveAndDeleteAll();

			database.LD_LicenceType = DatabaseTypes.Codes.Production;
			database.LD_StaffFirstReportUtc = new ZDateTime(2019, 1, 1);
			database.LD_OH_WebAccessOrg = org1.PK;

			var contact1a = org1.Contacts.AddNew();
			contact1a.OC_ContactName = "User One";
			contact1a.OC_Email = "user.one@test.org";
			contact1a.OC_WebAccessEnabled = true;
			contact1a.SetHashedPassword("123456");
			contact1a.OC_OA_OrgAddress = org1.MainAddress.PK;

			var userAccount1 = Factory.New<EdiCustomerUserAccount>();
			userAccount1.EUA_LD = database.PK;
			userAccount1.EUA_UserID = "US1";
			userAccount1.EUA_FullName = "User One";
			userAccount1.EUA_Email = "user.one@test.org";
			userAccount1.EUA_OC_WebAccessContact = contact1a.PK;

			Factory.Save();

			database.Lookups.WebAccessOrgs.Reload(true);

			var enterprise = database.LicEnterprise;
			using (var form = new LicenceEnterpriseForm(enterprise))
			{
				form.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				enterprise.LE_OH = org2.PK;
				form.FireSaveButton();
				AssertEquals(org2.PK, enterprise.LE_OH);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;
				enterprise.LE_OH = org1.PK;
				AssertEquals(org2.PK, enterprise.LE_OH);
			}
		}

		protected override Form GetFormToBashCore()
		{
			EDIOrgHeader header = Factory.NewWithValidTestData<EDIOrgHeader>();
			header.CreateAndLoadLicenceForOrg();
			LicenceEnterprise licence = header.LicCompany.LicEnterprise;
			return new LicenceEnterpriseForm(licence);
		}
	}
}
