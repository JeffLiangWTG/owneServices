using System;
using System.Web;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using Enterprise.ZClientWebCargoWiseEDI.My_account;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.ZClientWebCargoWiseEDI.My_Account
{
	[TestedType(typeof(RequestUpgrade))]
	public class RequestUpgradeTest : ZPageTestCase
	{
		[HttpContextEnabledTest]
		public void TestRequestUpgrade_ReleaseRingFallback_ShouldNotShowError()
		{
			var currentRunningVersion = CreateNewReleaseBuild(24, 10, 23, 100, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			CreateNewReleaseBuild(24, 10, 30, 115, superceded: true, ReleaseRings.Codes.DPR, ProductTypes.Codes.Enterprise);
			var cwnDPR = CreateNewReleaseBuild(24, 11, 27, 5, superceded: false, ReleaseRings.Codes.DPR, ProductTypes.Codes.CargoWiseNext);
			var cw1STD = CreateNewReleaseBuild(24, 10, 30, 136, superceded: false, ReleaseRings.Codes.STD, ProductTypes.Codes.Enterprise);
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "XXY";
			org.CreateAndLoadLicenceForOrg();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "alex@contact.com";
			contact.OC_WebAccessEnabled = true;

			var licenceDatabase = CreateLicenceDatabaseForTest(org, ZBool.True, ProductTypes.Codes.CargoWiseOne);
			licenceDatabase.LD_ReleaseRing = ReleaseRings.Codes.DPR;
			licenceDatabase.LD_HL_CurrentRunningVersion = currentRunningVersion.PK;
			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			var upgradeForCW1DPR = builds.GetLatestAvailableBuildForInstalledVersion(licenceDatabase.LD_Product, licenceDatabase.LD_ReleaseRing, licenceDatabase.CurrentVersion.VersionNumber, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: licenceDatabase.IsHostedOnWiseCloud);

			AssertEquals("Precondition: CW1 DPR should get CW1 STD build as latest DPR is CWN", cw1STD, upgradeForCW1DPR);

			var secureQueryString = new SecureQueryString
			{
				{ "LicenceDatabasePK", licenceDatabase.PK.ToString() },
				{ "ReleaseBuildPK", cw1STD.PK.ToString() },
			};
			HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, secureQueryString.ToString());
			HttpContext.Current.Request.QueryString.Add("RequestUpgradeButton", "hello");
			TestPage.SiteUser.LoginSupportForTest(org.OH_Code);
			TestPage.IsPostBack = true;
			TestPage.Page_Load_Exposed(null, EventArgs.Empty);

			AssertNotEquals(IHtmlEncodableLabelControlExtensions.GetHtmlEncodableLabelContent(TestPage.LabelStatus_Exposed, "Release ring doesn't match database ring"), TestPage.LabelStatus_Exposed.Text);
		}

		[HttpContextEnabledTest]
		public void TestRequestUpgrade_ReleaseRingFallback_GP1GP2ShouldNotShowError()
		{
			var gp1_1611101 = CreateNewReleaseBuild(16, 1, 1, 101, false, "GP1");
			var gp1_1612201 = CreateNewReleaseBuild(16, 1, 2, 201, false, "GP1");
			var gp2_1611501 = CreateNewReleaseBuild(16, 1, 1, 501, false, "GP2");
			var gp2_1612601 = CreateNewReleaseBuild(16, 1, 2, 601, false, "GP2");
			var now = ZDateTime.Now.Date;
			gp2_1612601.HL_ExeVersionDate = gp1_1612201.HL_ExeVersionDate = now.AddDays(-3);
			gp2_1611501.HL_ExeVersionDate = gp1_1611101.HL_ExeVersionDate = now.AddDays(-6);
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "XXY";
			org.CreateAndLoadLicenceForOrg();
			var licenceDatabase = CreateLicenceDatabaseForTest(org, ZBool.True, "CW1");
			licenceDatabase.LD_ReleaseRing = "GP1";
			licenceDatabase.LD_HL_CurrentRunningVersion = gp2_1611501.PK;
			Factory.Save();

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "alex@contact.com";
			contact.OC_WebAccessEnabled = true;
			Factory.Save();

			var builds = new LatestReleaseBuildsDictionary(Factory);
			builds.Load();

			var upgradeForCW1GP1 = builds.GetLatestAvailableBuildForInstalledVersion(licenceDatabase.LD_Product, licenceDatabase.LD_ReleaseRing, licenceDatabase.CurrentVersion.VersionNumber, isPatchOnly: false, takeWeeklyBuildsInsteadOfLatest: licenceDatabase.IsHostedOnWiseCloud);
			var fallbackUpgradeForCW1GP1 = builds.GetLatestAvailableBuild(licenceDatabase.LD_Product, ReleaseRings.Codes.GP1, takeWeeklyBuildsInsteadOfLatest: licenceDatabase.IsHostedOnWiseCloud);

			AssertEquals("Precondition: CW1 GP1 should get CW1 GP1 build", gp2_1612601, upgradeForCW1GP1);
			AssertEquals("Precondition: CW1 GP1 should get CW1 GP2 build", gp1_1612201, fallbackUpgradeForCW1GP1);

			var secureQueryString = new SecureQueryString
			{
				{ "LicenceDatabasePK", licenceDatabase.PK.ToString() },
				{ "ReleaseBuildPK", gp2_1612601.PK.ToString() },
			};
			HttpContext.Current.Request.QueryString.Add(SecureQueryString.QueryStringKey, secureQueryString.ToString());
			HttpContext.Current.Request.QueryString.Add("RequestUpgradeButton", "hello");
			TestPage.SiteUser.LoginSupportForTest(org.OH_Code);
			TestPage.IsPostBack = true;
			TestPage.Page_Load_Exposed(null, EventArgs.Empty);

			AssertNotEquals(IHtmlEncodableLabelControlExtensions.GetHtmlEncodableLabelContent(TestPage.LabelStatus_Exposed, "Release ring doesn't match database ring"), TestPage.LabelStatus_Exposed.Text);
		}

		#region Implementation

		LicenceDatabase CreateLicenceDatabaseForTest(EDIOrgHeader org, ZBool isActive, string productType)
		{
			var database = org.LicCompany.LicDatabases.AddNew();
			database.FillWithValidTestData();
			database.LD_IsActive = isActive;
			database.LD_Product = productType;
			return database;
		}

		ReleaseBuild CreateNewReleaseBuild(int majorVersion, int minorVersion, int release, int patch, bool superceded, string releaseRing, string product = "ENT")
		{
			var releaseBuild = ReleaseBuild.NewForTesting(Factory, releaseRing, superceded);
			releaseBuild.HL_Product = product;
			releaseBuild.HL_MajorVersion = majorVersion;
			releaseBuild.HL_MinorVersion = minorVersion;
			releaseBuild.HL_Release = release;
			releaseBuild.HL_Patch = patch;
			releaseBuild.HL_ExeVersionDate = superceded ? ZDateTime.Now.AddHours(-1) : ZDateTime.Now;

			return releaseBuild;
		}

		RequestUpgradeForTest TestPage => Page as RequestUpgradeForTest;

		protected override ZPage GetNewZPage()
		{
			return GetPageForTest();
		}

		class RequestUpgradeForTest : RequestUpgrade
		{
			public void Page_Load_Exposed(object sender, EventArgs e) => Page_Load(sender, e);
			public ZTextLabel LabelReleaseBuildName_Exposed { get => base.LabelReleaseBuildName; set => base.LabelReleaseBuildName = value; }
			public ZTextLabel LabelUpgradeMethodInfo_Exposed { get => base.LabelUpgradeMethodInfo; set => base.LabelUpgradeMethodInfo = value; }
			public ZTextLabel LabelUpgradeMethod_Exposed { get => base.LabelUpgradeMethod; set => base.LabelUpgradeMethod = value; }
			public ZTextLabel LabelPleaseSelectUpgradeMethod_Exposed { get => base.LabelPleaseSelectUpgradeMethod; set => base.LabelPleaseSelectUpgradeMethod = value; }
			public ZTextLabel LabelUpgradeMethodDescription_Exposed { get => base.LabelUpgradeMethodDescription; set => base.LabelUpgradeMethodDescription = value; }
			public ZTextLabel LabelStatus_Exposed { get => base.LabelStatus; set => base.LabelStatus = value; }
			public DropDownList UpgradeMethod_Exposed { get => base.UpgradeMethod; set => base.UpgradeMethod = value; }
			public Button CloseButton_Exposed { get => base.CloseButton; set => base.CloseButton = value; }
			public Button RequestUpgradeButton_Exposed { get => base.RequestUpgradeButton; set => base.RequestUpgradeButton = value; }
		}

		static RequestUpgradeForTest GetPageForTest()
		{
			var page = new RequestUpgradeForTest();
			page.LabelReleaseBuildName_Exposed = new ZTextLabel();
			page.LabelUpgradeMethod_Exposed = new ZTextLabel();
			page.LabelUpgradeMethodInfo_Exposed = new ZTextLabel();
			page.LabelPleaseSelectUpgradeMethod_Exposed = new ZTextLabel();
			page.LabelUpgradeMethodDescription_Exposed = new ZTextLabel();
			page.LabelStatus_Exposed = new ZTextLabel();
			page.UpgradeMethod_Exposed = new DropDownList();
			page.CloseButton_Exposed = new Button();
			page.RequestUpgradeButton_Exposed = new Button();

			return page;
		}

		#endregion
	}
}
