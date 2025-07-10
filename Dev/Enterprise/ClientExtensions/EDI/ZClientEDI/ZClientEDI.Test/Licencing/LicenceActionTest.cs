using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.Interop.OutlookIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.Licencing.Module.Testing
{
	public class LicenceActionTest : TestCaseWithFactory
	{
		public void TestGenerateAndAutoDeployLicenceKey_DeploymentSuccess()
		{
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = LicenceDatabase.DateFromWhichLicenceIsAutoDeployable;
			build.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion.AddMinor(1);

			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_FullName = "Zubins Org";
			org.OH_Code = "PPP";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicEnterprise.LE_EnterpriseCode = "ZUB";
			org.LicCompany.LC_CompanyCode = "CCC";
			LicenceDatabase database = org.LicEnterprise.Databases.AddNew();
			database.LD_HostServerName = "ZUBIN";
			database.LD_HL_CurrentRunningVersion = build.PK;
			database.LD_PublicEmailAddressForUpdate = "XerxesTest@edi.com.au";
			org.LicCompany.LC_CompanyCountry = "AU";
			org.LicCompany.LicDatabases.Add(database);
			database.LD_LE = org.LicEnterprise.PK;
			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			LicenceActionForTest licAction = new LicenceActionForTest(null, null, database, org, false);
			licAction.NextDialogResult = DialogResult.OK;
			licAction.SimulateErrorWhenSendingLicence = false;
			licAction.UpdateLicenceRemotely(this, EventArgs.Empty);

			string expected = "1 license key(s) successfully sent. Changes to the license won't be enforced at the client until operators log out and log back into Enterprise.";
			AssertEquals("Msg shown", expected, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestGenerateLicenceKeyWithOutlookError()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_FullName = "Zubins Org";
			org.OH_Code = "PPP";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicEnterprise.LE_EnterpriseCode = "ZUB";
			org.LicCompany.LC_CompanyCode = "CCC";
			LicenceDatabase database = org.LicEnterprise.Databases.AddNew();
			database.LD_HostServerName = "ZUBIN";
			org.LicCompany.LC_CompanyCountry = "AU";
			org.LicCompany.LicDatabases.Add(database);
			database.LD_LE = org.LicEnterprise.PK;
			ReleaseBuild releaseBuild = Factory.New<ReleaseBuild>();
			releaseBuild.HL_ReleaseStatus = ReleaseRings.Codes.ALP;
			releaseBuild.VersionNumber = LicenceHeader.LineLevelOnDemandLicenceVersion.AddMinor(1);
			database.LD_HL_CurrentRunningVersion = releaseBuild.PK;
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			LicenceActionForTest licAction = new LicenceActionForTest(null, null, database, org, false);
			licAction.NextDialogResult = DialogResult.OK;
			licAction.NextDirectoryName = Env.TempPath;
			licAction.ThrowOutlookExceptionInSendLicenceKeyInEmail = true;

			try
			{
				licAction.SendEmailLicence(this, EventArgs.Empty);
				AssertEquals("Error Starting Outlook", "There was an unexpected error trying to show the Send Email dialog: \r\n\r\nOutlook broke", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				string filePath = Path.Combine(Env.TempPath, "ZUBCCC-SYD.key");
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestGenerateLicenceKeyNoDatabase()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_Code = "XXY";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicDatabases.RemoveAndDeleteAll();
			org.Factory.Save();

			LicenceActionForTest licAction = new LicenceActionForTest(null, null, null, org, false);
			licAction.SendEmailLicence(this, EventArgs.Empty);
			AssertEquals("Error message should be shown", "You cannot generate the License Key until you have assigned a Database to the Company", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSendEmailLicence()
		{
			var organisation = Factory.NewWithValidTestData<EDIOrgHeader>();
			organisation.OH_FullName = "ZEBRA";
			var licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = organisation.PK;
			var currency = Factory.NewWithValidTestData<RefCurrency>();
			licenceCompany.LC_RX_NKCurrency = currency.RX_Code;
			var db = licenceCompany.LicDatabases.AddNew();
			db.LD_Product = ProductTypes.Codes.Enterprise;
			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = Country.LicenceKeyBuilderSupportedCountryCodes[0];
			organisation.OH_RL_NKClosestPort = unloco.RL_Code;

			licenceCompany.LicEnterprise.LE_EnterpriseCode = "ZEC";
			licenceCompany.LC_CompanyCode = "ZCC";
			GlbStaff.CurrentUser.GS_FullName = "Andrew Luong";
			GlbStaff.CurrentUser.GS_EmailAddress = "andrew.luong@cargowise.com";

			Factory.Save();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			using (var tempDir = new TempDirectory())
			{
				var licenceAction = new LicenceActionWithBrowserPathSetForTest(tempDir.DirectoryName, null, null, db, organisation, false);
				licenceAction.SendEmailLicence(null, null);

				AssertEquals(typeof(EmailContactForm), licenceAction.LastFormShownForTest.GetType());

				var emailContactForm = (EmailContactForm)licenceAction.LastFormShownForTest;
				var emailToContactBusinessObject = emailContactForm.BusinessEntity;
				AssertEquals("ZECZCC-.key", (emailToContactBusinessObject.AttachmentList[0].Code));
				AssertEquals("ediEnterprise License Key for ZEBRA", emailToContactBusinessObject.Subject);
				AssertEquals("Andrew Luong", emailToContactBusinessObject.FromDisplayName);
				AssertEquals("andrew.luong@cargowise.com", emailToContactBusinessObject.FromEmailAddress);

				licenceAction.LastFormShownForTest.Dispose();
			}
		}
	}

	public class LicenceActionWithBrowserPathSetForTest : LicenceAction
	{
		public LicenceActionWithBrowserPathSetForTest(string path, Form inMainForm, ZFilterGridModule inFilterGridModule, LicenceDatabase inLicDatabase, EDIOrgHeader inOrg, bool inIsBatchProcess)
			: base(inMainForm, inFilterGridModule, inLicDatabase, inOrg, inIsBatchProcess)
		{
			this.path = path;
		}

		readonly string path;
		protected override DialogResult ShowDialog(ZFolderBrowserDialog browser)
		{
			return DialogResult.OK;
		}
		protected override string GetPath(ZFolderBrowserDialog browser)
		{
			return path;
		}

		public ZForm LastFormShownForTest;
		protected override void ShowForm(ZForm form)
		{
			LastFormShownForTest = form;
		}
	}

	public class LicenceActionForTest : LicenceAction
	{
		DialogResult fNextDialogResult;
		public DialogResult NextDialogResult
		{
			get { return fNextDialogResult; }
			set { fNextDialogResult = value; }
		}

		string fNextDirectoryName;
		public string NextDirectoryName
		{
			get { return fNextDirectoryName; }
			set { fNextDirectoryName = value; }
		}

		public bool SimulateErrorWhenSendingLicence;

		public bool ThrowOutlookExceptionInSendLicenceKeyInEmail;

		public LicenceActionForTest(Form inMainForm, ZFilterGridModule inFilterGridModule, LicenceDatabase inLicDatabase, EDIOrgHeader inOrg, bool inIsBatchProcess)
			: base(inMainForm, inFilterGridModule, inLicDatabase, inOrg, inIsBatchProcess)
		{
		}

		protected override DialogResult ShowDialog(ZFolderBrowserDialog browser)
		{
			return NextDialogResult;
		}

		protected override string GetPath(ZFolderBrowserDialog browser)
		{
			return NextDirectoryName;
		}

		protected override bool GenerateAndAutoDeployLicenceKey(EDIOrgHeader inOrganisation, LicenceHeader inLicence)
		{
			bool result = false;
			if (!SimulateErrorWhenSendingLicence)
			{
				result = base.GenerateAndAutoDeployLicenceKey(inOrganisation, inLicence);
			}
			else
			{
				throw new InvalidOperationException("Simulated Exception from Email Sender");
			}
			return result;
		}

		protected override void SendEmailCore(string fileName, EDIOrgHeader inOrganisation, string subject)
		{
			if (ThrowOutlookExceptionInSendLicenceKeyInEmail)
			{
				throw new OutlookException("Outlook broke");
			}
		}
	}
}
