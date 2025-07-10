using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Licencing.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI.Testing
{
	[TestedType(typeof(LicenceDatabaseForm))]
	public class LicenceDatabaseFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			EDIOrgHeader header = Factory.New<EDIOrgHeader>();
			header.CreateAndLoadLicenceForOrg();
			LicenceDatabase dB = header.LicCompany.LicDatabases.AddNew();
			var form = new LicenceDatabaseForm(dB, null);
			form.Controls.Find("BottomPanel", true)[0].Visible = false;
			return form;
		}

		public void TestRequestVersionReport()
		{
			EDIOrgHeader header = Factory.New<EDIOrgHeader>();
			header.CreateAndLoadLicenceForOrg();
			LicenceDatabase dB = header.LicCompany.LicDatabases.AddNew();

			using (LicenceDatabaseFormForTesting form = new LicenceDatabaseFormForTesting(dB))
			{
				form.SimulateErrorWhenRequestingReport = true;
				form.RequestVersionReportButton_Click_Exposed();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("A request for a Version Report could not be sent to the client.\n\nPlease check that they are running a recent version of the software, and that the Public Email Address is correctly specified for the database if the version is before 16.10.20.0.", UnitTestUserNotification.Instance.LastMessage.Text);

				ReleaseBuild build = Factory.New<ReleaseBuild>();
				build.HL_ExeVersionDate = LicenceDatabase.DateFromWhichLicenceIsAutoDeployable;
				dB.LD_HL_CurrentRunningVersion = build.PK;
				dB.LD_PublicEmailAddressForUpdate = "Test@edi.com.au";
				form.SimulateErrorWhenRequestingReport = false;

				form.RequestVersionReportButton_Click_Exposed();
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals(LicenceDatabase.LegacyVersionReportRequestSubject, Env.OutgoingMailManager.EmailsCreated[0].Subject);
				AssertEquals("A request for a Version Report has been sent to the client's batch processor.\n\nTheir system will respond with a Version Report and ediProd will be automatically updated in a few minutes.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRequestLogs()
		{
			EDIOrgHeader header = Factory.New<EDIOrgHeader>();
			header.CreateAndLoadLicenceForOrg();
			LicenceDatabase db = header.LicCompany.LicDatabases.AddNew();

			using (LicenceDatabaseFormForTesting form = new LicenceDatabaseFormForTesting(db))
			{
				ReleaseBuild build = Factory.New<ReleaseBuild>();
				build.VersionNumber = LicenceUsageRequest.FirstVersion;
				db.LD_HL_CurrentRunningVersion = build.PK;
				db.LD_PublicEmailAddressForUpdate = "Test@edi.com.au";

				form.RequestLicenceDatabaseLogsButton_Click_Exposed();
				AssertEquals(typeof(LicenceDatabaseLogsRequestForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		public void TestBottomPanelAllowsZPreviousNextControlToNotOverlap()
		{
			var header = Factory.New<EDIOrgHeader>();
			header.CreateAndLoadLicenceForOrg();
			var db = header.LicCompany.LicDatabases.AddNew();

			using (var form = new LicenceDatabaseFormForTesting(db))
			{
				form.Show();
				AssertEquals("BottomPanel must be Dock.None so the ZPreviousNextControl does not overlap", form.Dock, System.Windows.Forms.DockStyle.None);
				Assert("BottomPanel must be at least 54 in height so the ZPreviousNextControl does not overlap", form.BottomPanel.Height >= 54);
			}
		}

		public void TestResetHeartbeat()
		{
			EDIDataRegistry.CreateProductsAndModulesForTest();
			EDIOrgHeader header = Factory.New<EDIOrgHeader>();
			header.OH_Code = "ABCCCCC";
			header.CreateAndLoadLicenceForOrg();
			header.MainAddress.OA_Address1 = "Address1";
			LicenceDatabase dB = header.LicCompany.LicDatabases.AddNew();
			dB.LD_HostServerSID = ZGuid.NewZGuid();
			string legacyTypesList = "AAA" + ", " + "BBB" + ", ENT";
			string confirmationMessage = "This procedure will reset all heartbeat information collected from the client system and request new information from the client.\n\nPlease ensure the client has their e-mail batch processor running and configured correctly.";
			string failedMessage =
@"The Heartbeat information could not be reset.

Please check the following details:
- The Public Email Address must be filled, in order to send an email to the client's batch processor
- The client's Current Version must be on or after 11-Nov-05 for this feature to work.
- The Licence Type must NOT be set to one of the Legacy Database Types (" + legacyTypesList + @").
- The Registration must be not be REG - Registered.
- The software must be earlier than version 16.10.20.0.

Please check that these details are correct before trying again.";
			string successMessage = "The Heartbeat information has been reset for this database.";

			using (LicenceDatabaseFormForTesting form = new LicenceDatabaseFormForTesting(dB))
			{
				form.SimulateVersionNotDeployable = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.ResetHeartbeatButton_Click_Exposed();
				Assert("SID NOT Reset", dB.LD_HostServerSID != ZGuid.Empty);
				AssertEquals(confirmationMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.ResetHeartbeatButton_Click_Exposed();
				Assert("SID NOT Reset", dB.LD_HostServerSID != ZGuid.Empty);
				AssertEquals(failedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				ReleaseBuild build = Factory.New<ReleaseBuild>();
				build.HL_Product = ProductTypes.Codes.Enterprise;
				build.HL_ExeVersionDate = new ZDateTime(2006, 12, 18);
				dB.LD_HL_CurrentRunningVersion = build.PK;
				dB.LD_PublicEmailAddressForUpdate = "Test@edi.com.au";
				form.SimulateVersionNotDeployable = false;
				form.SimulateErrorWhenResettingHeartbeat = true;
				form.ResetHeartbeatButton_Click_Exposed();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("There was an error in reqesting a version report. Please check that the ediProd batch processor is running, or your SMTP mail settings are correct.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.SimulateErrorWhenResettingHeartbeat = false;
				form.ResetHeartbeatButton_Click_Exposed();
				Assert("SID is reset", dB.LD_HostServerSID == ZGuid.Empty);
				AssertEquals(successMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResetHeartbeatButtonDisabled()
		{
			EDIOrgHeader header = Factory.New<EDIOrgHeader>();
			header.CreateAndLoadLicenceForOrg();
			LicenceDatabase dB = header.LicCompany.LicDatabases.AddNew();

			using (LicenceDatabaseFormForTesting form = new LicenceDatabaseFormForTesting(dB))
			{
				Assert("Enabled because there is access", form.ResetHeartbeatButton.Enabled);
			}

			bool oldValue = EDISecurityCheckpoints.OrgLicenceModify.IsAllowed;
			try
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = false;
				using (LicenceDatabaseFormForTesting form = new LicenceDatabaseFormForTesting(dB))
				{
					Assert("Disabled because there is no access", !form.ResetHeartbeatButton.Enabled);
				}
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = oldValue;
			}
		}

		public void TestRequestLicenceUsage()
		{
			EDIOrgHeader header = Factory.New<EDIOrgHeader>();
			header.CreateAndLoadLicenceForOrg();
			LicenceDatabase db = header.LicCompany.LicDatabases.AddNew();

			using (LicenceDatabaseFormForTesting form = new LicenceDatabaseFormForTesting(db))
			{
				ReleaseBuild build = Factory.New<ReleaseBuild>();
				build.VersionNumber = LicenceUsageRequest.FirstVersion;
				db.LD_HL_CurrentRunningVersion = build.PK;
				db.LD_PublicEmailAddressForUpdate = "Test@edi.com.au";

				form.RequestLicenceUsageButton_Click_Exposed();
				AssertEquals(typeof(LicenceUsageRequestForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		public void TestRequestStaffReport()
		{
			DebugOnlyOutgoingSystemMessage.Initialize();
			ObjectFactory.Substitute<IOutgoingSystemMessage>(new DebugOnlyOutgoingSystemMessage());

			var db = BillingTestHelper.CreateLicence(Factory, "DDD").Database;

			using (var form = new LicenceDatabaseFormForTesting(db))
			{
				form.RequestStaffReportButton_Click_Exposed();
				string expectedXml = "<StaffReportRequest />";
				var actualXml = DebugOnlyOutgoingSystemMessage.XmlMessageBody;
				AssertEquals(expectedXml, actualXml);
			}
		}

		public void TestChangeWebAccessOrg()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence1.Database;

			var org1 = database.LicEnterprise.Organisation;
			org1.Contacts.RemoveAndDeleteAll();

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

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var org2 = licence2.Company.Header;
			org2.Contacts.RemoveAndDeleteAll();

			var licence3 = BillingTestHelper.CreateLicence(Factory, "EEE", "TTT", "SYD");
			var org3 = licence3.Company.Header;
			org3.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			using (var form = new LicenceDatabaseFormForTesting(database))
			{
				form.Show();

				database.LD_LicenceType = DatabaseTypes.Codes.Test;
				database.LD_OH_WebAccessOrg = org2.PK;
				form.FireSaveButton();
				AssertEquals("Contact is not cloned to new web access org for non-production database", 0, org2.Contacts.Count);

				database.LD_OH_WebAccessOrg = org1.PK;
				database.LD_LicenceType = DatabaseTypes.Codes.Production;
				Factory.Save();

				database.LD_OH_WebAccessOrg = org3.PK;
				form.FireSaveButton();
				AssertEquals("Contact is not cloned to invalid web access org", 0, org3.Contacts.Count);

				database.LD_OH_WebAccessOrg = org1.PK;
				Factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				database.LD_LicenceType = DatabaseTypes.Codes.Production;
				database.LD_OH_WebAccessOrg = org2.PK;
				form.FireSaveButton();

				AssertEquals("Contact is cloned to new web access org", 1, org2.Contacts.Count);
				var contact2a = org2.Contacts[0];
				AssertEquals(true, contact2a.OC_IsActive);
				AssertEquals("User One", contact2a.OC_ContactName);
				AssertEquals("user.one@test.org", contact2a.OC_Email);
				AssertEquals(contact2a.PK, userAccount1.EUA_OC_WebAccessContact);
				AssertEquals(contact1a.OC_PER, contact2a.OC_PER);
				AssertEquals(@"Cloning Contact Addresses... 1 / 1 - 100
Cloning Contacts... 1 / 1 - 100
", form.StatusLogs.ToString());

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				database.LD_OH_WebAccessOrg = org1.PK;
				form.FireSaveButton();

				AssertEquals("No new contact if matched one is found", 1, org1.Contacts.Count);
				AssertEquals(true, contact1a.OC_IsActive);
				AssertEquals("User One", contact1a.OC_ContactName);
				AssertEquals("user.one@test.org", contact1a.OC_Email);
				AssertEquals(contact1a.PK, userAccount1.EUA_OC_WebAccessContact);
				AssertEquals(contact1a.OC_PER, contact2a.OC_PER);
			}
		}

		public void TestControlsVisibility()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence1.Database;
			Factory.Save();

			using (var form = new LicenceDatabaseFormForTesting(database))
			{
				form.Show();
				var systemReferenceIDTextBox = form.Controls.Find("TenantIDTextBox", true).Single();
				var productionDatabaseDropEdit = form.Controls.Find("ProductionDatabaseDropEdit", true).Single();

				AssertEquals(true, database.IsEnterpriseFamilyDatabase);
				AssertEquals(DatabaseTypes.Codes.Production, database.LD_LicenceType);
				AssertEquals(false, systemReferenceIDTextBox.Visible);
				AssertEquals(false, productionDatabaseDropEdit.Visible);

				database.LD_Product = "BOR";
				database.LD_LicenceType = DatabaseTypes.Codes.Test;
				AssertEquals(false, database.IsEnterpriseFamilyDatabase);
				AssertEquals(DatabaseTypes.Codes.Test, database.LD_LicenceType);
				AssertEquals(true, systemReferenceIDTextBox.Visible);
				AssertEquals(true, productionDatabaseDropEdit.Visible);
			}
		}

		[GuiTest]
		public override void TestMinimumSizeNotTooBig()
		{
			using (var testForm = GetFormToBash())
			{
				const int MinScreenWidthSupported = 750;
				const int MinScreenHeightSupported = 800;
				const int TypicalTaskbarHeight = 43;

				int maxSizeWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(MinScreenWidthSupported);
				int maxSizeHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(MinScreenHeightSupported - TypicalTaskbarHeight);

				Assert("Form min size too wide (" + testForm.MinimumSize.Width + ") for the screen. Should be less than or equal to " + maxSizeWidth, testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height + ") for the screen. Should be less than or equal to " + maxSizeHeight, testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		public void TestShowPreSaveDialogs_MTDB()
		{
			var mtdbCode = MultiTenantDatabaseProductTypeList.Codes.CSP;
			var products = new SystemProductCollection();
			var mtdbPrd = products.AddNew();
			mtdbPrd.Code = mtdbCode;
			mtdbPrd.Description = mtdbCode;
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, products);

			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database1 = licence1.Database;
			database1.LD_Product = mtdbCode;
			database1.GetOrCreateTrustedSystem().ETS_SystemEndpointUrl = "http://123";
			database1.LD_AvailableUpgradeMethod = "BLK";

			var licence2 = BillingTestHelper.CreateLicence(Factory, "DD2", "AB2", "SY2");
			var database2 = licence2.Database;
			licence2.Database.LD_Product = mtdbCode;
			database2.LD_AvailableUpgradeMethod = "BLK";
			Factory.Save();

			AssertEquals(true, database1.IsMultiTenantDatabase);
			AssertEquals(true, database2.IsMultiTenantDatabase);
			AssertEquals("http://123", database1.TrustedSystem.ETS_SystemEndpointUrl);
			AssertNull(database2.TrustedSystem);

			using (var form = new LicenceDatabaseFormForTesting(database1))
			{
				form.Show();
				database1.GetOrCreateTrustedSystem().ETS_SystemEndpointUrl = "http://456";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoErrors(database1);
				AssertNoErrors(database2);
				form.FireSaveButton();
			}

			AssertEquals("The changes on 'Trusted Messaging' will affect all LicenceDatabases for this Product.\r\nAre you sure you want to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, database1.HasChanges);
			AssertEquals(false, database2.HasChanges);
			AssertEquals("http://456", database1.TrustedSystem.ETS_SystemEndpointUrl);
			AssertEquals("http://456", database2.TrustedSystem.ETS_SystemEndpointUrl);
			AssertEquals(database1.LD_ETS_TrustedSystem, database2.LD_ETS_TrustedSystem);
		}

		public void TestShowPreSaveDialogs_DatabaseConfig()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database1 = licence1.Database;
			database1.LD_Product = "CW1";
			database1.GetOrCreateTrustedSystem().ETS_SystemEndpointUrl = "http://123";
			database1.LD_AvailableUpgradeMethod = "BLK";
			Factory.Save();

			using (var form = new LicenceDatabaseFormForTesting(database1))
			{
				form.Show();
				database1.GetOrCreateTrustedSystem().ETS_SystemEndpointUrl = "http://456";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoErrors(database1);
				form.FireSaveButton();
			}

			AssertEquals("The changes on 'Trusted Messaging' may affect the communication between client system and MyAccount.\r\nAre you sure you want to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, database1.HasChanges);
		}

		public void TestEnterpriseIDWithoutDefault()
		{
			var licenceEnterpriseWebAPIDefault = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenceEnterpriseWebAPIDefault.LE_EnterpriseID = "ET0001";
			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, licenceEnterpriseWebAPIDefault.LE_EnterpriseID);

			var database = Factory.New<LicenceDatabase>();
			database.LD_LE = licenceEnterpriseWebAPIDefault.PK;
			AssertEquals("ET0001", database.EnterpriseID);
			AssertEquals(string.Empty, database.EnterpriseIDWithoutDefault);

			var licenceEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			licenceEnterprise.LE_EnterpriseID = "LE0025";
			database = Factory.New<LicenceDatabase>();
			database.LD_LE = licenceEnterprise.PK;
			AssertEquals("LE0025", database.EnterpriseID);
			AssertEquals("LE0025", database.EnterpriseIDWithoutDefault);
		}

		public void TestTableLayoutPanelWithAutoScroll()
		{
			var header = Factory.New<EDIOrgHeader>();
			header.CreateAndLoadLicenceForOrg();
			var db = header.LicCompany.LicDatabases.AddNew();

			using (var form = new LicenceDatabaseFormForTesting(db))
			{
				form.Show();
				var panel = form.Controls.Find("tableLayoutPanel1", true).Single() as KTableLayoutPanel;
				AssertEquals(true, panel.AutoScroll);
			}
		}

		public void TestControls_EnablePackageDownloadOptimizationCheckBox()
		{
			var header = Factory.New<EDIOrgHeader>();
			header.CreateAndLoadLicenceForOrg();
			var db = header.LicCompany.LicDatabases.AddNew();

			using (var form = new LicenceDatabaseFormForTesting(db))
			{
				var enablePackageDownloadOptimizationCheckBox = form.Controls.Find("enablePackageDownloadOptimizationCheckBox", true).Single() as ZCheckBox;
				AssertEquals(false, enablePackageDownloadOptimizationCheckBox.Checked);
				AssertEquals("Package Download Skip Optimization", enablePackageDownloadOptimizationCheckBox.Text);
			}
		}

		public void TestControls()
		{
			var header = Factory.New<EDIOrgHeader>();
			header.CreateAndLoadLicenceForOrg();
			var db = header.LicCompany.LicDatabases.AddNew();

			using (var form = new LicenceDatabaseFormForTesting(db))
			{
				form.Show();
				var eAdaptorUrlBox = form.Controls.Find("eAdaptorUrlBox", true).Single() as ZTextBox;
				AssertEquals(CharacterCasing.Normal, eAdaptorUrlBox.CharacterCasing);
			}
		}

		class LicenceDatabaseFormForTesting : LicenceDatabaseForm
		{
			public LicenceDatabaseFormForTesting(LicenceDatabase businessEntity, ILicenceDatabaseViewController licenceDatabaseViewController = null)
				: base(businessEntity, licenceDatabaseViewController)
			{
			}

			public new ZButton ResetHeartbeatButton
			{
				get { return base.ResetHeartbeatButton; }
			}

			public void RequestVersionReportButton_Click_Exposed()
			{
				base.RequestVersionReportButton_Click(this, EventArgs.Empty);
			}

			public void RequestLicenceDatabaseLogsButton_Click_Exposed()
			{
				base.RequestLicenceDatabaseLogsButton_Click(this, EventArgs.Empty);
			}

			public new KPanel BottomPanel
			{
				get { return base.BottomPanel; }
			}

			public void ResetHeartbeatButton_Click_Exposed()
			{
				base.ResetHeartbeatButton_Click(this, EventArgs.Empty);
			}

			protected override void RequestVersionReport()
			{
				if (!SimulateErrorWhenRequestingReport)
				{
					base.RequestVersionReport();
				}
				else
				{
					throw new InvalidOperationException("Simulated Exception from Email Sender");
				}
			}
			public bool SimulateErrorWhenRequestingReport;

			protected override bool ResetHeartbeat()
			{
				bool result = false;
				if (SimulateErrorWhenResettingHeartbeat)
				{
					throw new InvalidOperationException("Simulated Exception from Email Sender");
				}
				else
				{
					result = !SimulateVersionNotDeployable && base.ResetHeartbeat();
				}

				return result;
			}
			public bool SimulateErrorWhenResettingHeartbeat;
			public bool SimulateVersionNotDeployable;

			public void RequestLicenceUsageButton_Click_Exposed()
			{
				base.RequestLicenceUsageButton_Click(this, EventArgs.Empty);
			}

			public void RequestStaffReportButton_Click_Exposed()
			{
				base.RequestStaffReportButton_Click(this, EventArgs.Empty);
			}

			public override void UpdateStatus(string status, int progressValue)
			{
				StatusLogs.AppendLine($"{status} - {progressValue}");
			}

			public readonly ZStringBuilder StatusLogs = new ZStringBuilder();
		}
	}
}
