using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.My_Account
{
	public class DownloadsTest : TestCaseWithFactory
	{
		public void TestSetupReleaseRingDownloadInfo()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "XXY";
			org.CreateAndLoadLicenceForOrg();
			CreateLicenceDatabaseForTest(org, ZBool.True, "BOR");
			CreateLicenceDatabaseForTest(org, ZBool.False, "");
			CreateLicenceDatabaseForTest(org, ZBool.True, "ENT");
			CreateLicenceDatabaseForTest(org, ZBool.True, "CW1");
			CreateLicenceDatabaseForTest(org, ZBool.True, "GLW");
			CreateLicenceDatabaseForTest(org, ZBool.True, "HUB");
			CreateLicenceDatabaseForTest(org, ZBool.True, "CWN");
			CreateLicenceDatabaseForTest(org, ZBool.True, "CGW");
			Factory.Save();
			var downLoad = new Downloads();
			downLoad.PopulateActiveDatabasesSupportedDownload(org);
			AssertEquals(5, downLoad.LicenceDatabases.Count);
			AssertContainsExactElementsInAnyOrder("Show only active liscence and LD_Product is not ENT/CW1/GLW", new List<string> { "ENT", "CW1", "GLW", "CWN", "CGW" }, downLoad.LicenceDatabases.ToArray<LicenceDatabase>().Select(x => x.LD_Product));
		}

		public void TestSetupReleaseRingDownloadInfo_GP2()
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
			var db1 = CreateLicenceDatabaseForTest(org, ZBool.True, "CW1");
			db1.LD_ReleaseRing = "GP1";
			db1.LD_HL_CurrentRunningVersion = gp2_1611501.PK;
			Factory.Save();
			var downLoad = new Downloads();
			downLoad.PopulateActiveDatabasesSupportedDownload(org);
			AssertEquals(db1, downLoad.LicenceDatabases.Single() as LicenceDatabase);
			var pItem = new RepeaterItem(0, ListItemType.Item);
			pItem.DataItem = db1;
			var dropDownList = new DropDownList()
			{ ID = "DropDownVersion" };
			var lb = new Label()
			{ ID = "CurrentVersion" };
			pItem.Controls.Add(dropDownList);
			pItem.Controls.Add(lb);
			var itemArgs = new RepeaterItemEventArgs(pItem);
			downLoad.R1_ItemDataBound(null, itemArgs);
			AssertEquals(2, dropDownList.Items.Count);
			AssertStartsWith("GP2", "CargoWise One - Previous GP Release 2016 Jan 02 patch 601 - 16.1.2.601 -", dropDownList.Items[0].Text);
			AssertEquals(gp2_1612601.PK.ToString(), dropDownList.Items[0].Value);
			AssertStartsWith("GP1", "CargoWise One - GP Release CW1 2016 Jan 02 patch 201 - 16.1.2.201 - ", dropDownList.Items[1].Text);
			AssertEquals(gp1_1612201.PK.ToString(), dropDownList.Items[1].Value);
		}

		public void TestSetupReleaseRingDownloadInfo_HostedDatabase()
		{
			var gp1_22_11_9_260 = CreateNewReleaseBuild(22, 11, 9, 260, true, "GP1");
			gp1_22_11_9_260.HL_ExeVersionDate = new ZDateTime(2023, 1, 10);

			var gp1_22_11_9_294 = CreateNewReleaseBuild(22, 11, 9, 294, false, "GP1");
			gp1_22_11_9_294.HL_ExeVersionDate = new ZDateTime(2023, 2, 2);
			gp1_22_11_9_294.HL_Comment = "Latest weekly GP1";

			var gp1_22_11_9_303 = CreateNewReleaseBuild(22, 11, 9, 303, false, "GP1");
			gp1_22_11_9_303.HL_ExeVersionDate = new ZDateTime(2023, 2, 8);

			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "DDDABCSYD";
			org1.CreateAndLoadLicenceForOrg();
			var db1 = CreateLicenceDatabaseForTest(org1, true, "CW1");
			db1.LD_ReleaseRing = "GP1";
			db1.LD_HostedLocation = "SYD";
			db1.LD_HL_CurrentRunningVersion = gp1_22_11_9_260.PK;

			var org2 = Factory.New<EDIOrgHeader>();
			org2.OH_Code = "EEETTTMEL";
			org2.CreateAndLoadLicenceForOrg();
			var db2 = CreateLicenceDatabaseForTest(org2, true, "CW1");
			db2.LD_ReleaseRing = "GP1";
			db2.LD_HostedLocation = "NCW";
			db2.LD_HL_CurrentRunningVersion = gp1_22_11_9_260.PK;

			Factory.Save();

			var downloadPage1 = new Downloads();
			downloadPage1.PopulateActiveDatabasesSupportedDownload(org1);
			var lb = new Label() { ID = "CurrentVersion" };

			var dropDownList1 = new DropDownList() { ID = "DropDownVersion" };
			var repearter1 = new RepeaterItem(0, ListItemType.Item);
			repearter1.DataItem = db1;
			repearter1.Controls.Add(dropDownList1);
			repearter1.Controls.Add(lb);

			var itemArgs1 = new RepeaterItemEventArgs(repearter1);
			downloadPage1.R1_ItemDataBound(null, itemArgs1);
			AssertEquals(1, dropDownList1.Items.Count);
			AssertStartsWith("Weekly version", "CargoWise One - GP Release CW1 2022 Nov 09 patch 294 - 22.11.9.294 -", dropDownList1.Items[0].Text);
			AssertEquals(gp1_22_11_9_294.PK.ToString(), dropDownList1.Items[0].Value);

			var downloadPage2 = new Downloads();
			downloadPage2.PopulateActiveDatabasesSupportedDownload(org2);

			var dropDownList2 = new DropDownList() { ID = "DropDownVersion" };
			var repearter2 = new RepeaterItem(0, ListItemType.Item);
			repearter2.DataItem = db2;
			repearter2.Controls.Add(dropDownList2);
			repearter2.Controls.Add(lb);

			var itemArgs2 = new RepeaterItemEventArgs(repearter2);
			downloadPage2.R1_ItemDataBound(null, itemArgs2);
			AssertEquals(1, dropDownList2.Items.Count);
			AssertStartsWith("Latest version", "CargoWise One - GP Release CW1 2022 Nov 09 patch 303 - 22.11.9.303 -", dropDownList2.Items[0].Text);
			AssertEquals(gp1_22_11_9_303.PK.ToString(), dropDownList2.Items[0].Value);
		}

		public void TestSetupReleaseRingDownloadInfo_DownloadLinks()
		{
			var downloadPage1 = new DownloadsForTest();
			AssertEquals("#", downloadPage1.CW1DvdIsoFileLinkHRef_Exposed);
			AssertEquals("#", downloadPage1.CW1DvdZipFileLinkHRef_Exposed);

			using (EDIDataRegistry.Instance.CW1DvdIsoFileDownloadURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.wisetechglobal.com/myaccount/downloads/DvdCargoWiseOneWebServerSetup.iso"))
			using (EDIDataRegistry.Instance.CW1DvdZipFileDownloadURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.wisetechglobal.com/myaccount/downloads/DvdCargoWiseOneWebServerSetup.zip"))
			using (EDIDataRegistry.Instance.CW1ExeFileDownloadURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://www.wisetechglobal.com/myaccount/downloads/CargoWiseOneWebServerSetup.exe"))
			{
				var downloadPage2 = new DownloadsForTest();
				AssertEquals("http://www.wisetechglobal.com/myaccount/downloads/DvdCargoWiseOneWebServerSetup.iso", downloadPage2.CW1DvdIsoFileLinkHRef_Exposed);
				AssertEquals("http://www.wisetechglobal.com/myaccount/downloads/DvdCargoWiseOneWebServerSetup.zip", downloadPage2.CW1DvdZipFileLinkHRef_Exposed);
				AssertEquals("http://www.wisetechglobal.com/myaccount/downloads/CargoWiseOneWebServerSetup.exe", downloadPage2.CW1ExeFileLinkHRef_Exposed);
			}
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

		ReleaseBuild CreateNewReleaseBuild(int majorVersion, int minorVersion, int release, int patch, bool superceded, string releaseStatus)
		{
			var releaseBuild = ReleaseBuild.NewForTesting(Factory, releaseStatus, superceded);
			releaseBuild.HL_MajorVersion = majorVersion;
			releaseBuild.HL_MinorVersion = minorVersion;
			releaseBuild.HL_Release = release;
			releaseBuild.HL_Patch = patch;
			return releaseBuild;
		}

		class DownloadsForTest : Downloads
		{
			public DownloadsForTest() : base()
			{
				StaticDownloadLinksSection = new System.Web.UI.HtmlControls.HtmlGenericControl();
				UpdateNotesLinksSection = new System.Web.UI.HtmlControls.HtmlGenericControl();
				CW1DvdIsoFileLink = new System.Web.UI.HtmlControls.HtmlAnchor();
				CW1DvdZipFileLink = new System.Web.UI.HtmlControls.HtmlAnchor();
				CW1ExeFileLink = new System.Web.UI.HtmlControls.HtmlAnchor();
				SetupDvdDownloadInfo();
			}

			public string CW1DvdIsoFileLinkHRef_Exposed => CW1DvdIsoFileLink.HRef;
			public string CW1DvdZipFileLinkHRef_Exposed => CW1DvdZipFileLink.HRef;
			public string CW1ExeFileLinkHRef_Exposed => CW1ExeFileLink.HRef;
		}

		#endregion
	}
}
