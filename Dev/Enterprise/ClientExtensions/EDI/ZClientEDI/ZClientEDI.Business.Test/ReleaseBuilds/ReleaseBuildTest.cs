using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using MailManager;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.ReleaseBuilds.Business.Test
{
	[TestedType(typeof(ReleaseBuild))]
	public class ReleaseBuildTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRetryCalculateCodes_NoException()
		{
			var build = Factory.New<MockReleaseBuild>();
			build.SkipGetCodes = true;

			AssertNoExceptionThrown(delegate
			{ build.CallCalculateClientSpecificCodesForTest(); });
			AssertEquals(1, build.RetryCount);
		}

		public void TestRetryCalculateCodes_NetworkPath()
		{
			var build = Factory.New<MockReleaseBuild>();
			build.ThrowDuringGetCodes(new IOException("The network path was not found."));

			AssertExceptionThrown(typeof(IOException), delegate
			{ build.CallCalculateClientSpecificCodesForTest(); });
			AssertEquals(3, build.RetryCount);
		}

		public void TestRetryCalculateCodes_OtherException()
		{
			var build = Factory.New<MockReleaseBuild>();
			build.ThrowDuringGetCodes(new IOException("some other text"));

			AssertExceptionThrown(typeof(IOException), delegate
			{ build.CallCalculateClientSpecificCodesForTest(); });
			AssertEquals(1, build.RetryCount);
		}

		#region Remove Bad Build

		public void TestRemoveBuildAndStopQueue_FTPDeleteFails()
		{
			LastFTPDeleteError = "";
			string testRootDir = Path.Combine(Env.TempPath, "ediEnterpriseUpgrades");
			string testClientDir = Path.Combine(testRootDir, "ClientSpecific");
			string testClientDirWithClient1 = Path.Combine(Env.TempPath, @"ediEnterpriseUpgrades\ClientSpecific\SomeClient");
			string testClientDirWithClient2 = Path.Combine(Env.TempPath, @"ediEnterpriseUpgrades\ClientSpecific\AnotherClient");
			string clientFile1 = testClientDirWithClient1 + "\\Package20041229_135600_1_2_3_4.txt";
			string clientFile2 = testClientDirWithClient2 + "\\Package20041229_135600_1_2_3_4.txt";

			try
			{
				Directory.CreateDirectory(testClientDir);
				Directory.CreateDirectory(testClientDirWithClient1);
				Directory.CreateDirectory(testClientDirWithClient2);
				CreateFile(clientFile1);
				CreateFile(clientFile2);

				MockReleaseBuild build = Factory.New<MockReleaseBuild>();
				build.HL_Product = ProductTypes.Codes.Enterprise;
				build.FtpDeleteShouldFail = true;
				build.DeleteBuild += new CancelEventHandler(Build_DeleteBuild_Accept);
				build.FTPDeleteFailed += new ReleaseBuild.FTPDeleteFailedEventHandler(Build_FTPDeleteFailed);
				build.HL_ExeVersionDate = ZDateTime.Now;
				build.HL_Superceded = false;
				Factory.Save();

				build.HL_Superceded = true;
				Factory.Save();
				AssertEquals(testClientDirWithClient2 + @"\Package20041229_135600_1_2_3_4.txt - Something happened" + System.Environment.NewLine +
					testClientDirWithClient1 + @"\Package20041229_135600_1_2_3_4.txt - Something happened" + System.Environment.NewLine, LastFTPDeleteError);
			}
			finally
			{
				TempDirectory.DeleteDirectory(testRootDir);
			}
		}

		void Build_FTPDeleteFailed(List<string> clientCodesAndReasons)
		{
			foreach (string error in clientCodesAndReasons)
			{
				LastFTPDeleteError += error + System.Environment.NewLine;
			}
		}

		string LastFTPDeleteError;

		public void TestRemoveBuildAndStopQueue()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1ZUB";

			LicenceEnterprise licEnterprise = Factory.New<LicenceEnterprise>();
			licEnterprise.LE_OH = org1.PK;
			licEnterprise.LE_EnterpriseCode = "XYZ";

			LicenceCompany licCompany = Factory.New<LicenceCompany>();
			licCompany.LC_CompanyCode = "CO1";
			licCompany.LC_OH = org1.PK;
			licCompany.LC_LE = licEnterprise.PK;

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_LE = licEnterprise.PK;
			database.LD_ServerCode = "SYD";

			LicenceHeader licHeader = Factory.New<LicenceHeader>();
			licHeader.LA_LC = licCompany.PK;
			licHeader.LA_LD = database.PK;

			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2ZUB";

			MockReleaseBuild build = Factory.New<MockReleaseBuild>();
			build.HL_Product = ProductTypes.Codes.Enterprise;
			build.HL_ExeVersionDate = ZDateTime.Now;
			build.HL_Superceded = false;

			ReleaseBuild otherBuild = Factory.New<ReleaseBuild>();
			otherBuild.HL_Product = ProductTypes.Codes.Enterprise;
			otherBuild.HL_ExeVersionDate = ZDateTime.Now.AddDays(-5);
			otherBuild.HL_Superceded = false;

			build.Logs.AddNew(Events.Delivered, org1.OH_Code);

			UpgradesToClient queued = Factory.New<UpgradesToClient>();
			queued.L1_HL = build.PK;
			queued.L1_OH = org1.PK;
			queued.L1_LD = database.PK;
			queued.L1_RequestedDateTime = ZDateTime.Now.AddDays(1);
			queued.L1_CurrentStatus = UpgradesToClientStatus.Codes.Queued;

			UpgradesToClient nonQueued = Factory.New<UpgradesToClient>();
			nonQueued.L1_HL = build.PK;
			nonQueued.L1_OH = org1.PK;
			nonQueued.L1_LD = database.PK;
			nonQueued.L1_RequestedDateTime = ZDateTime.Now.AddDays(1);
			nonQueued.L1_CurrentStatus = UpgradesToClientStatus.Codes.Sent;

			UpgradesToClient otherBuildQueued = Factory.New<UpgradesToClient>();
			otherBuildQueued.L1_HL = otherBuild.PK;
			otherBuildQueued.L1_OH = org1.PK;
			otherBuildQueued.L1_LD = database.PK;
			otherBuildQueued.L1_RequestedDateTime = ZDateTime.Now.AddDays(1);
			otherBuildQueued.L1_CurrentStatus = UpgradesToClientStatus.Codes.Queued;

			MailItem pendingMailItem = Factory.New<MailItem>();
			pendingMailItem.MI_Subject = "Upgrade for " + build.PackageName;
			pendingMailItem.MI_Status = MailStatus.Queued;
			pendingMailItem.MI_SendDateTime = ZDateTime.Today.AddDays(-10);
			pendingMailItem.MI_ReceivedDateTime = ZDateTime.Today.AddDays(-10);
			pendingMailItem.MI_Direction = DirectionList.Codes.Receive;

			MailItem sentMailItem = Factory.New<MailItem>();
			sentMailItem.MI_Subject = "Upgrade for " + build.PackageName;
			sentMailItem.MI_Status = MailStatus.Sent;
			sentMailItem.MI_SendDateTime = ZDateTime.Today.AddDays(-10);
			sentMailItem.MI_ReceivedDateTime = ZDateTime.Today.AddDays(-10);
			sentMailItem.MI_Direction = DirectionList.Codes.Receive;

			MailItem pendingMailItemOtherBuild = Factory.New<MailItem>();
			pendingMailItemOtherBuild.MI_Subject = "Upgrade for " + otherBuild.PackageName;
			pendingMailItemOtherBuild.MI_Status = MailStatus.Queued;
			pendingMailItemOtherBuild.MI_SendDateTime = ZDateTime.Today.AddDays(-10);
			pendingMailItemOtherBuild.MI_ReceivedDateTime = ZDateTime.Today.AddDays(-10);
			pendingMailItemOtherBuild.MI_Direction = DirectionList.Codes.Receive;

			Factory.Save();

			string testRootDir = Path.Combine(Env.TempPath, "ediEnterpriseUpgrades");
			string testGenericDir = Path.Combine(testRootDir, "Generic");
			string testClientDir = Path.Combine(testRootDir, "ClientSpecific");
			string testClientDirWithClient = Path.Combine(testClientDir, "SomeClient");
			string genericFile = testGenericDir + "\\Package20041229_135600_1_2_3_4.txt";
			string clientFile = testClientDirWithClient + "\\Package20041229_135600_1_2_3_4.txt";

			Directory.CreateDirectory(testGenericDir);
			Directory.CreateDirectory(testClientDir);
			Directory.CreateDirectory(testClientDirWithClient);
			CreateFile(genericFile);
			CreateFile(clientFile);

			Assert("Precondition", File.Exists(clientFile));
			Assert("Precondition", File.Exists(genericFile));
			AssertEquals("Precondition", UpgradesToClientStatus.Codes.Queued, queued.L1_CurrentStatus);

			try
			{
				AssertEquals("There is 1 client who has been sent this build", 1, build.ClientsSentToButNotApplied.Count);

				build.DeleteBuild += new CancelEventHandler(Build_DeleteBuild_Cancel);
				build.HL_Superceded = true;
				Factory.Save();

				Assert("User cancelled - nothing should change", File.Exists(genericFile));
				Assert("User cancelled - nothing should change", File.Exists(clientFile));
				AssertEquals("User cancelled - nothing should change", UpgradesToClientStatus.Codes.Queued, queued.L1_CurrentStatus);
				AssertEquals("User cancelled - nothing should change", MailStatus.Queued, pendingMailItem.MI_Status);
				AssertEquals("User cancelled - nothing should change", 1, build.ClientsSentToButNotApplied.Count);

				build.HL_Superceded = false;
				Factory.Save();

				build.DeleteBuild -= new CancelEventHandler(Build_DeleteBuild_Cancel);
				build.DeleteBuild += new CancelEventHandler(Build_DeleteBuild_Accept);
				build.HL_Superceded = true;
				Factory.Save();

				Assert("Generic FTP Deleted", !File.Exists(genericFile));
				Assert("Client Specific FTP Deleted", !File.Exists(clientFile));
				AssertEquals("Queued Upgrade for this build blocked", UpgradesToClientStatus.Codes.Blocked, queued.L1_CurrentStatus);
				AssertEquals("Non Queued Upgrade for this build is not touched", UpgradesToClientStatus.Codes.Sent, nonQueued.L1_CurrentStatus);
				AssertEquals("Queued Upgrade for other build is not blocked", UpgradesToClientStatus.Codes.Queued, otherBuildQueued.L1_CurrentStatus);

				AssertEquals("Pending mail item for this build is blocked", MailStatus.Failed, pendingMailItem.MI_Status);
				AssertEquals("Pending mail item for another build is not touched", MailStatus.Queued, pendingMailItemOtherBuild.MI_Status);
				AssertEquals("Non-pending mail item for this build is not touched", MailStatus.Sent, sentMailItem.MI_Status);

				AssertEquals("No clients have received this build as it was succesfully removed", 0, build.ClientsSentToButNotApplied.Count);
			}
			finally
			{
				if (Directory.Exists(testRootDir))
				{
					Directory.Delete(testRootDir, true);
				}
			}
		}

		void Build_DeleteBuild_Cancel(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
		}

		void Build_DeleteBuild_Accept(object sender, CancelEventArgs e)
		{
		}

		void CreateFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				using (FileStream stream = File.Create(fileName))
				{
				}
			}
		}

		#endregion

		public void TestIsCargoWiseProduct()
		{
			var build = Factory.NewWithValidTestData<ReleaseBuild>();
			AssertEquals("ENT", build.HL_Product);
			Assert(build.IsCargoWiseProduct);

			build.HL_Product = "AAA";
			Assert(!build.IsCargoWiseProduct);

			build.HL_Product = "CWN";
			Assert(build.IsCargoWiseProduct);

			build.HL_Product = ProductTypes.Codes.CargoWise;
			Assert(build.IsCargoWiseProduct);
		}

		public void TestIsCompatibleProduct()
		{
			var build = Factory.NewWithValidTestData<ReleaseBuild>();
			AssertEquals(ProductTypes.Codes.Enterprise, build.HL_Product);
			Assert(build.IsCompatibleProduct(ProductTypes.Codes.Enterprise));
			Assert(build.IsCompatibleProduct(ProductTypes.Codes.CargoWiseOne));
			Assert(build.IsCompatibleProduct(ProductTypes.Codes.CargoWiseNext));
			Assert(build.IsCompatibleProduct(ProductTypes.Codes.ProductivityWise));
			Assert(!build.IsCompatibleProduct("AAA"));

			build.HL_Product = "AAA";
			Assert(!build.IsCompatibleProduct(ProductTypes.Codes.Enterprise));
			Assert(!build.IsCompatibleProduct(ProductTypes.Codes.CargoWiseOne));
			Assert(!build.IsCompatibleProduct(ProductTypes.Codes.CargoWiseNext));
			Assert(!build.IsCompatibleProduct(ProductTypes.Codes.ProductivityWise));
			Assert(build.IsCompatibleProduct("AAA"));

			build.HL_Product = ProductTypes.Codes.CargoWiseNext;
			Assert(!build.IsCompatibleProduct(ProductTypes.Codes.Enterprise));
			Assert(build.IsCompatibleProduct(ProductTypes.Codes.CargoWiseOne));
			Assert(build.IsCompatibleProduct(ProductTypes.Codes.CargoWiseNext));
			Assert(build.IsCompatibleProduct(ProductTypes.Codes.ProductivityWise));
			Assert(!build.IsCompatibleProduct("AAA"));
		}

		public void TestIsCompatibleProductCGW()
		{
			var build = Factory.NewWithValidTestData<ReleaseBuild>();
			build.HL_Product = ProductTypes.Codes.CargoWise;
			Assert(build.IsCompatibleProduct(ProductTypes.Codes.Enterprise));
			Assert(build.IsCompatibleProduct(ProductTypes.Codes.CargoWiseOne));
			Assert(build.IsCompatibleProduct(ProductTypes.Codes.CargoWiseNext));
			Assert(build.IsCompatibleProduct(ProductTypes.Codes.ProductivityWise));
			Assert(!build.IsCompatibleProduct("AAA"));
		}

		public void TestIsUpgradeableProduct()
		{
			var build = Factory.NewWithValidTestData<ReleaseBuild>();
			AssertEquals(ProductTypes.Codes.Enterprise, build.HL_Product);
			Assert(build.IsUpgradeableProduct(ProductTypes.Codes.Enterprise));
			Assert(build.IsUpgradeableProduct(ProductTypes.Codes.CargoWiseOne));
			Assert(!build.IsUpgradeableProduct(ProductTypes.Codes.CargoWiseNext));
			Assert(build.IsUpgradeableProduct(ProductTypes.Codes.ProductivityWise));
			Assert(!build.IsUpgradeableProduct("AAA"));

			build.HL_Product = "AAA";
			Assert(!build.IsUpgradeableProduct(ProductTypes.Codes.Enterprise));
			Assert(!build.IsUpgradeableProduct(ProductTypes.Codes.CargoWiseOne));
			Assert(!build.IsUpgradeableProduct(ProductTypes.Codes.CargoWiseNext));
			Assert(!build.IsUpgradeableProduct(ProductTypes.Codes.ProductivityWise));
			Assert(build.IsUpgradeableProduct("AAA"));

			build.HL_Product = ProductTypes.Codes.CargoWiseNext;
			Assert(!build.IsUpgradeableProduct(ProductTypes.Codes.Enterprise));
			Assert(!build.IsUpgradeableProduct(ProductTypes.Codes.CargoWiseOne));
			Assert(build.IsUpgradeableProduct(ProductTypes.Codes.CargoWiseNext));
			Assert(!build.IsUpgradeableProduct(ProductTypes.Codes.ProductivityWise));
			Assert(!build.IsUpgradeableProduct("AAA"));
		}

		public void TestIsUpgradeableProductCGW()
		{
			var build = Factory.NewWithValidTestData<ReleaseBuild>();
			build.HL_Product = ProductTypes.Codes.CargoWise;
			Assert(build.IsUpgradeableProduct(ProductTypes.Codes.Enterprise));
			Assert(build.IsUpgradeableProduct(ProductTypes.Codes.CargoWiseOne));
			Assert(build.IsUpgradeableProduct(ProductTypes.Codes.CargoWiseNext));
			Assert(build.IsUpgradeableProduct(ProductTypes.Codes.ProductivityWise));
			Assert(!build.IsUpgradeableProduct("AAA"));
		}

		public void TestCodeAndDescriptionPropertyAttributes()
		{
			AssertEquals("CodeProperty", ReleaseBuild.Schema.ExeVersion, ((CodePropertyAttribute)typeof(ReleaseBuild).GetCustomAttributes(typeof(CodePropertyAttribute), false)[0]).PropertyName);
			AssertEquals("DescriptionProperty", ReleaseBuild.Schema.ReleaseDisplayText, ((DescriptionPropertyAttribute)typeof(ReleaseBuild).GetCustomAttributes(typeof(DescriptionPropertyAttribute), false)[0]).PropertyName);
		}

		public void TestPatchLicences()
		{
			ReleaseBuild build1 = Factory.New<ReleaseBuild>();
			ReleaseBuild build2 = Factory.New<ReleaseBuild>();

			EDIOrgHeader client = Factory.New<EDIOrgHeader>();
			client.OH_Code = "ABCXYZ";
			client.MainAddress.OA_Address1 = "Test Address";
			client.CreateAndLoadLicenceForOrg();

			LicenceDatabase db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			LicenceDatabase db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			LicenceDatabase db3 = Factory.NewWithValidTestData<LicenceDatabase>();
			LicenceDatabase db4 = Factory.NewWithValidTestData<LicenceDatabase>();

			LicenceHeader licence1 = Factory.NewWithValidTestData<LicenceHeader>();
			LicenceHeader licence2 = Factory.NewWithValidTestData<LicenceHeader>();

			licence1.LA_LC = client.LicCompany.PK;
			licence1.LA_LD = db1.PK;
			licence2.LA_LC = client.LicCompany.PK;
			licence2.LA_LD = db3.PK;

			db1.LD_HL_CurrentRunningVersion = build1.PK;
			db2.LD_HL_CurrentRunningVersion = build1.PK;
			db3.LD_HL_CurrentRunningVersion = build2.PK;
			db4.LD_HL_CurrentRunningVersion = build2.PK;

			Factory.Save();

			AssertEquals("build1.PatchLicences.ReadOnly", true, build1.PatchLicences.ReadOnly);
			AssertEquals("build2.PatchLicences.ReadOnly", true, build2.PatchLicences.ReadOnly);

			AssertEquals("build1.PatchLicences.Count", 1, build1.PatchLicences.Count);
			AssertEquals("build2.PatchLicences.Count", 1, build2.PatchLicences.Count);

			AssertEquals("build1.PatchLicences.Contains(licence1.PK)", true, build1.PatchLicences.Contains(licence1.PK));
			AssertEquals("build2.PatchLicences.Contains(licence2.PK)", true, build2.PatchLicences.Contains(licence2.PK));
		}

		public void TestFilteredPatchLicences()
		{
			ReleaseBuild build1 = Factory.New<ReleaseBuild>();
			ReleaseBuild build2 = Factory.New<ReleaseBuild>();

			EDIOrgHeader client = Factory.New<EDIOrgHeader>();
			client.OH_Code = "ABCXYZ";
			client.MainAddress.OA_Address1 = "Test Address";
			client.CreateAndLoadLicenceForOrg();

			LicenceDatabase db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			LicenceDatabase db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			LicenceDatabase db3 = Factory.NewWithValidTestData<LicenceDatabase>();
			LicenceDatabase db4 = Factory.NewWithValidTestData<LicenceDatabase>();

			LicenceHeader licence1 = Factory.NewWithValidTestData<LicenceHeader>();
			LicenceHeader licence2 = Factory.NewWithValidTestData<LicenceHeader>();

			licence1.LA_LC = client.LicCompany.PK;
			licence1.LA_LD = db1.PK;
			licence2.LA_LC = client.LicCompany.PK;
			licence2.LA_LD = db3.PK;

			db1.LD_HL_CurrentRunningVersion = build1.PK;
			db2.LD_HL_CurrentRunningVersion = build1.PK;
			db3.LD_HL_CurrentRunningVersion = build2.PK;
			db4.LD_HL_CurrentRunningVersion = build2.PK;

			Factory.Save();

			InternalIncidentLicenceSettings settings = new InternalIncidentLicenceSettings();
			LicenceEnterpriseKey key = new LicenceEnterpriseKey();
			key.LE_PK = client.LicEnterprise.PK;
			settings.LicenceEnterpriseKeys.Add(key);
			settings.EdiProd_LicencePK = licence1.PK;
			settings.UAT_ALP_LicencePK = licence1.PK;
			settings.UAT_DPR_LicencePK = licence1.PK;
			settings.UAT_GPC_LicencePK = licence1.PK;
			settings.UAT_GPR_LicencePK = licence1.PK;
			settings.UAT_STD_LicencePK = licence1.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			AssertEquals("build1.PatchLicences.Count", 0, build1.FilteredPatchLicences.Count);
			AssertEquals("build2.PatchLicences.Count", 0, build2.FilteredPatchLicences.Count);

			AssertEquals("build1.PatchLicences.Contains(licence1.PK)", false, build1.FilteredPatchLicences.Contains(licence1.PK));
			AssertEquals("build2.PatchLicences.Contains(licence2.PK)", false, build2.FilteredPatchLicences.Contains(licence2.PK));

			build1.ShowCargoWiseLicences = true;
			build2.ShowCargoWiseLicences = true;

			AssertEquals("build1.PatchLicences.Count", 1, build1.FilteredPatchLicences.Count);
			AssertEquals("build2.PatchLicences.Count", 1, build2.FilteredPatchLicences.Count);

			AssertEquals("build1.PatchLicences.Contains(licence1.PK)", true, build1.FilteredPatchLicences.Contains(licence1.PK));
			AssertEquals("build2.PatchLicences.Contains(licence2.PK)", true, build2.FilteredPatchLicences.Contains(licence2.PK));
		}

		public void TestIncidentsReported()
		{
			ReleaseBuild build1 = Factory.New<ReleaseBuild>();
			ReleaseBuild build2 = Factory.New<ReleaseBuild>();

			SupportIncident incident1 = Factory.New<SupportIncident>();
			SupportIncident incident2 = Factory.New<SupportIncident>();

			incident1.IM_HL_ClientReportedOnVersion = build1.PK;
			incident2.IM_HL_ClientReportedOnVersion = build2.PK;

			SupportIncidentCollection collection1 = build1.IncidentsReported;
			SupportIncidentCollection collection2 = build2.IncidentsReported;

			Assert("Collection1 should be ReadOnly", collection1.ReadOnly);
			Assert("Collection2 should be ReadOnly", collection2.ReadOnly);

			AssertEquals("Collection1 Count", 1, collection1.Count);
			AssertEquals("Collection2 Count", 1, collection2.Count);

			Assert("Collection1 should contain Incident1", collection1.Contains(incident1.PK));
			Assert("Collection1 should contain Incident2", collection2.Contains(incident2.PK));
		}

		public void TestHL_ExeVersionDate()
		{
			Build.HL_ExeVersionDate = new ZDateTime(2005, 1, 31, 12, 13, 50);
			AssertEquals("HL_ExeVersionDate", new ZDateTime(2005, 1, 31, 12, 13, 0), Build.HL_ExeVersionDate);

			Build.HL_ExeVersionDate = ZDateTime.Empty;
			AssertEquals("HL_ExeVersionDate", ZDateTime.Empty, Build.HL_ExeVersionDate);

			Build.HL_ExeVersionDate = ZDateTime.Invalid;
			AssertEquals("HL_ExeVersionDate", ZDateTime.Invalid, Build.HL_ExeVersionDate);
		}

		public void TestDefaultValues()
		{
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			AssertEquals("default product is ENT - for package import and build", ProductTypes.Codes.Enterprise, build.HL_Product);
		}

		public void TestPropertiesReadOnlyByDefault()
		{
			ReleaseBuild buildWithENT = Factory.NewWithValidTestData<ReleaseBuild>();
			buildWithENT.HL_Product = ProductTypes.Codes.Enterprise;
			ReleaseBuild buildWithOutENT = Factory.NewWithValidTestData<ReleaseBuild>();
			buildWithOutENT.HL_Product = ZString.Empty;

			Assert("HL_ExeVersionDateInfo should be ReadOnly by default", buildWithENT.HL_ExeVersionDateInfo.ReadOnly);
			Assert("HL_MajorVersionInfo should be ReadOnly by default", buildWithENT.HL_MajorVersionInfo.ReadOnly);
			Assert("HL_MinorVersionInfo should be ReadOnly by default", buildWithENT.HL_MinorVersionInfo.ReadOnly);
			Assert("HL_PatchInfo should be ReadOnly by default", buildWithENT.HL_PatchInfo.ReadOnly);
			Assert("HL_ReleaseInfo should be ReadOnly by default", buildWithENT.HL_ReleaseInfo.ReadOnly);
			Assert("HL_PackagePath should be ReadOnly by default", buildWithENT.HL_PatchInfo.ReadOnly);

			Assert("HL_ExeVersionDateInfo should be ReadOnly by default", !buildWithOutENT.HL_ExeVersionDateInfo.ReadOnly);
			Assert("HL_MajorVersionInfo should be ReadOnly by default", buildWithOutENT.HL_MajorVersionInfo.ReadOnly);
			Assert("HL_MinorVersionInfo should be ReadOnly by default", buildWithOutENT.HL_MinorVersionInfo.ReadOnly);
			Assert("HL_PatchInfo should be ReadOnly by default", buildWithOutENT.HL_PatchInfo.ReadOnly);
			Assert("HL_ReleaseInfo should be ReadOnly by default", buildWithOutENT.HL_ReleaseInfo.ReadOnly);
			Assert("HL_PackagePath should be ReadOnly by default", buildWithENT.HL_PatchInfo.ReadOnly);
		}

		public void TestExeVersion()
		{
			Build.HL_MajorVersion = 1;
			Build.HL_MinorVersion = 2;
			Build.HL_Release = 100;
			Build.HL_Patch = 1234;
			AssertEquals("ExeVersion", "1.2.100.1234", Build.ExeVersion);
		}

		public void TestPackageName()
		{
			Build.HL_ExeVersionDate = new ZDateTime(2006, 9, 5, 12, 37, 0);
			Build.VersionNumber = new VersionNumber(1, 2, 2372, 16);
			AssertEquals("PackageName", "Package20060630_000000_1_2_2372_16.edp", Build.PackageName);
			AssertEquals("GetPackageName()", Build.PackageName, ReleaseBuild.GetPackageName(Build.VersionNumber));
		}

		public void TestClientSpecificCodes()
		{
			var build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = ZDateTime.Now;
			build.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			build.HL_PackagePath = TestBuildPackagePath;
			AssertEquals("Count", 4, build.ClientSpecificCodes.Count);
			AssertEquals("Contains(\"BCD\")", true, build.ClientSpecificCodes.Contains("BCD"));
			AssertEquals("Contains(\"ABC\")", true, build.ClientSpecificCodes.Contains("ABC"));
			AssertEquals("Contains(\"BBC\")", true, build.ClientSpecificCodes.Contains("BBC"));
			AssertEquals("Contains(\"XYZ\")", true, build.ClientSpecificCodes.Contains("XYZ"));
		}

		public void TestClientSpecificCodesThrowsWhenBuildIsNotReadable()
		{
			var build = Factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = ZDateTime.Now;
			build.HL_ReleaseStatus = ReleaseRings.Codes.DPR;
			build.HL_PackagePath = @"\\somewhere\out\there";
			AssertExceptionThrown<IOException>(() => build.ClientSpecificCodes.Any());
			AssertExceptionThrown<IOException>(() => build.IsSpecificForClient("BCD"));
		}

		public void TestClientSpecificCodesIsCached()
		{
			var build = Factory.New<ReleaseBuild>();
			build.HL_PackagePath = TestBuildPackagePath;
			var codes = build.ClientSpecificCodes;
			build.HL_PackagePath = @"\\nosuch\file";
			AssertSame("ClientSpecificCodes should be cached.", codes, build.ClientSpecificCodes);
		}

		public void TestClientsSentToButNotApplied()
		{
			ReleaseBuild olderBuild = Factory.New<ReleaseBuild>();
			ReleaseBuild newerBuild = Factory.New<ReleaseBuild>();

			Build.HL_ExeVersionDate = ZDateTime.Now;
			olderBuild.HL_ExeVersionDate = ZDateTime.Now.AddHours(-1);
			newerBuild.HL_ExeVersionDate = ZDateTime.Now.AddHours(1);

			EDIOrgHeader client1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client5 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client6 = Factory.NewWithValidTestData<EDIOrgHeader>();

			client1.CreateAndLoadLicenceForOrg();
			client2.CreateAndLoadLicenceForOrg();
			client3.CreateAndLoadLicenceForOrg();
			client4.CreateAndLoadLicenceForOrg();
			client5.CreateAndLoadLicenceForOrg();
			client6.CreateAndLoadLicenceForOrg();

			client1.LicenceEnterpriseCode = "E01";
			client2.LicenceEnterpriseCode = "E02";
			client3.LicenceEnterpriseCode = "E03";
			client4.LicenceEnterpriseCode = "E04";
			client5.LicenceEnterpriseCode = "E05";
			client6.LicenceEnterpriseCode = "E06";

			client1.OH_Code = "C1";
			client2.OH_Code = "C2";
			client3.OH_Code = "C3";
			client4.OH_Code = "C4";
			client5.OH_Code = "C5";
			client6.OH_Code = "C6";

			LicenceDatabase database1 = client1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database2 = client2.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database3 = client3.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database4 = client4.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database5 = client5.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database6 = client6.LicCompany.LicDatabases.AddNew();

			database1.LD_HL_CurrentRunningVersion = Build.PK;
			database2.LD_HL_CurrentRunningVersion = newerBuild.PK;
			database3.LD_HL_CurrentRunningVersion = olderBuild.PK;
			database4.LD_HL_CurrentRunningVersion = olderBuild.PK;
			database5.LD_HL_CurrentRunningVersion = ZGuid.Empty;
			database6.LD_HL_CurrentRunningVersion = ZGuid.Empty;

			Build.Logs.AddNew(Events.Delivered, "C1");
			Build.Logs.AddNew(Events.Delivered, "C2");
			Build.Logs.AddNew(Events.Delivered, "C3");
			Build.Logs.AddNew(Events.Delivered, "C5");

			Factory.Save();

			AssertEquals("ClientsSentToButNotApplied.Count", 2, Build.ClientsSentToButNotApplied.Count);
			AssertEquals("ClientsSentToButNotApplied.Contains(Client3)", true, Build.ClientsSentToButNotApplied.Contains(client3));
			AssertEquals("ClientsSentToButNotApplied.Contains(Client5)", true, Build.ClientsSentToButNotApplied.Contains(client5));
			AssertEquals("ClientsSentToButNotApplied.ReadOnly", true, Build.ClientsSentToButNotApplied.ReadOnly);
		}

		public void TestClientsSentToButNotAppliedWhenHL_ExeVersionDateIsEmpty()
		{
			Build.HL_ExeVersionDate = ZDateTime.Empty;
			AssertEquals("ClientsSentToButNotApplied.Count", 0, Build.ClientsSentToButNotApplied.Count);
		}

		public void TestFullDisplayText()
		{
			Build.HL_ExeVersionDate = new ZDateTime(2006, 1, 12, 15, 10, 0);
			Build.VersionNumber = new VersionNumber(1, 2, 3, 4);
			AssertEquals("FullDisplayText", $"ediEnterprise - {Build.ReleaseDisplayText} - 1.2.3.4 - 12-Jan-06 15:10", Build.FullDisplayText);

			Build.VersionNumber = new VersionNumber(14, 1, 1, 193);
			AssertEquals("FullDisplayText", $"CargoWise One - {Build.ReleaseDisplayText} - 14.1.1.193 - 12-Jan-06 15:10", Build.FullDisplayText);

			Build.VersionNumber = new VersionNumber(24, 11, 1, 90);
			Build.HL_Product = ProductTypes.Codes.CargoWiseNext;
			AssertEquals("FullDisplayText", $"CargoWise Next - {Build.VersionNumberDisplayText} - 24.11.1.90 - 12-Jan-06 15:10", Build.FullDisplayText);
		}

		public void TestShortDisplayText()
		{
			Build.HL_ExeVersionDate = new ZDateTime(2006, 1, 12, 15, 10, 0);
			Build.VersionNumber = new VersionNumber(1, 2, 3, 4);
			var releaseDate = Build.VersionNumber.GetReleaseDate().ToShortDateString();
			Build.HL_ReleaseStatus = "ALP";
			AssertEquals("ShortDisplayText", $"ediEnterprise - ALP {releaseDate} 1.2.3.4 - 12-Jan-06 15:10", Build.ShortDisplayText);

			Build.VersionNumber = new VersionNumber(14, 1, 1, 193);
			releaseDate = Build.VersionNumber.GetReleaseDate().ToShortDateString();
			AssertEquals("ShortDisplayText", $"CargoWise One - ALP {releaseDate} 14.1.1.193 - 12-Jan-06 15:10", Build.ShortDisplayText);

			Build.VersionNumber = new VersionNumber(24, 11, 1, 90);
			Build.HL_Product = ProductTypes.Codes.CargoWiseNext;
			releaseDate = Build.VersionNumber.GetReleaseDate().ToShortDateString();
			AssertEquals("ShortDisplayText", $"CargoWise Next - {releaseDate} 24.11.1.90 - 12-Jan-06 15:10", Build.ShortDisplayText);
		}

		public void TestReleaseLicencesAndReleaseClientCount()
		{
			ReleaseBuild build1 = Factory.New<ReleaseBuild>();
			ReleaseBuild build2 = Factory.New<ReleaseBuild>();
			ReleaseBuild build3 = Factory.New<ReleaseBuild>();

			build1.HL_Superceded = true;
			build2.HL_Superceded = false;
			build3.HL_Superceded = false;

			build1.VersionNumber = new VersionNumber(1, 1, 1, 0);
			build2.VersionNumber = new VersionNumber(1, 1, 1, 1);
			build3.VersionNumber = new VersionNumber(1, 1, 2, 0);

			EDIOrgHeader client1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client3 = Factory.NewWithValidTestData<EDIOrgHeader>();

			client1.CreateAndLoadLicenceForOrg();
			client2.CreateAndLoadLicenceForOrg();
			client3.CreateAndLoadLicenceForOrg();

			client1.LicenceEnterpriseCode = "CL1";
			client2.LicenceEnterpriseCode = "CL2";
			client3.LicenceEnterpriseCode = "CL3";

			var client1Lic1 = client1.LicCompany.GetHeader(client1.LicCompany.LicDatabases.AddNew());
			var client1Lic2 = client1.LicCompany.GetHeader(client1.LicCompany.LicDatabases.AddNew());
			var client2Lic1 = client2.LicCompany.GetHeader(client2.LicCompany.LicDatabases.AddNew());
			var client2Lic2 = client2.LicCompany.GetHeader(client2.LicCompany.LicDatabases.AddNew());
			var client3Lic = client3.LicCompany.GetHeader(client3.LicCompany.LicDatabases.AddNew());

			client1Lic1.Database.LD_HL_CurrentRunningVersion = build1.PK;
			client1Lic2.Database.LD_HL_CurrentRunningVersion = build2.PK;
			client2Lic1.Database.LD_HL_CurrentRunningVersion = build1.PK;
			client2Lic2.Database.LD_HL_CurrentRunningVersion = build3.PK;
			client3Lic.Database.LD_HL_CurrentRunningVersion = build2.PK;

			client1Lic1.Database.LD_ServerCode = "DB1";
			client1Lic2.Database.LD_ServerCode = "DB2";
			client2Lic1.Database.LD_ServerCode = "DB3";
			client2Lic2.Database.LD_ServerCode = "DB4";
			client3Lic.Database.LD_ServerCode = "DB5";

			Factory.Save();

			AssertEquals("build1.ReleaseClientCount", 0, build1.ReleaseClientCount);
			AssertEquals("build2.ReleaseClientCount", 4, build2.ReleaseClientCount);
			AssertEquals("build3.ReleaseClientCount", 1, build3.ReleaseClientCount);

			AssertEquals("build1.ReleaseLicences.Count", 4, build1.ReleaseLicences.Count);
			AssertEquals("build2.ReleaseLicences.Count", 4, build2.ReleaseLicences.Count);
			AssertEquals("build3.ReleaseLicences.Count", 1, build3.ReleaseLicences.Count);

			AssertEquals("build2.ReleaseLicences.Contains(client1Lic1)", true, build2.ReleaseLicences.Contains(client1Lic1));
			AssertEquals("build2.ReleaseLicences.Contains(client1Lic2)", true, build2.ReleaseLicences.Contains(client1Lic2));
			AssertEquals("build2.ReleaseLicences.Contains(client2Lic1)", true, build2.ReleaseLicences.Contains(client2Lic1));
			AssertEquals("build2.ReleaseLicences.Contains(client3Lic)", true, build2.ReleaseLicences.Contains(client3Lic));
			AssertEquals("build3.ReleaseLicences.Contains(client2Lic2)", true, build3.ReleaseLicences.Contains(client2Lic2));

			build1.HL_Superceded = false;
			AssertEquals("build1.ReleaseClientCount", 0, build1.ReleaseClientCount);
		}

		public void TestReleaseLicencesAndReleaseClientCount_CW1()
		{
			ReleaseBuild build1 = Factory.New<ReleaseBuild>();
			ReleaseBuild build2 = Factory.New<ReleaseBuild>();
			ReleaseBuild build3 = Factory.New<ReleaseBuild>();

			build1.HL_Superceded = true;
			build2.HL_Superceded = false;
			build3.HL_Superceded = false;

			build1.VersionNumber = new VersionNumber(14, 15, 1, 0);
			build2.VersionNumber = new VersionNumber(14, 16, 1, 1);
			build3.VersionNumber = new VersionNumber(14, 17, 2, 0);

			EDIOrgHeader client1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client3 = Factory.NewWithValidTestData<EDIOrgHeader>();

			client1.CreateAndLoadLicenceForOrg();
			client2.CreateAndLoadLicenceForOrg();
			client3.CreateAndLoadLicenceForOrg();

			client1.LicenceEnterpriseCode = "CL1";
			client2.LicenceEnterpriseCode = "CL2";
			client3.LicenceEnterpriseCode = "CL3";

			var client1Lic1 = client1.LicCompany.GetHeader(client1.LicCompany.LicDatabases.AddNew());
			var client1Lic2 = client1.LicCompany.GetHeader(client1.LicCompany.LicDatabases.AddNew());
			var client2Lic1 = client2.LicCompany.GetHeader(client2.LicCompany.LicDatabases.AddNew());
			var client2Lic2 = client2.LicCompany.GetHeader(client2.LicCompany.LicDatabases.AddNew());
			var client3Lic = client3.LicCompany.GetHeader(client3.LicCompany.LicDatabases.AddNew());

			client1Lic1.Database.LD_HL_CurrentRunningVersion = build1.PK;
			client1Lic2.Database.LD_HL_CurrentRunningVersion = build2.PK;
			client2Lic1.Database.LD_HL_CurrentRunningVersion = build1.PK;
			client2Lic2.Database.LD_HL_CurrentRunningVersion = build3.PK;
			client3Lic.Database.LD_HL_CurrentRunningVersion = build2.PK;

			client1Lic1.Database.LD_ServerCode = "DB1";
			client1Lic2.Database.LD_ServerCode = "DB2";
			client2Lic1.Database.LD_ServerCode = "DB3";
			client2Lic2.Database.LD_ServerCode = "DB4";
			client3Lic.Database.LD_ServerCode = "DB5";

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("build1.ReleaseLicences.Count", 2, build1.ReleaseLicences.Count);
				AssertEquals("build2.ReleaseLicences.Count", 2, build2.ReleaseLicences.Count);
				AssertEquals("build3.ReleaseLicences.Count", 1, build3.ReleaseLicences.Count);

				AssertEquals("build1.ReleaseLicences.Contains(client1Lic1)", true, build1.ReleaseLicences.Contains(client1Lic1));
				AssertEquals("build1.ReleaseLicences.Contains(client2Lic1)", true, build1.ReleaseLicences.Contains(client2Lic1));
				AssertEquals("build2.ReleaseLicences.Contains(client1Lic2)", true, build2.ReleaseLicences.Contains(client1Lic2));
				AssertEquals("build2.ReleaseLicences.Contains(client3Lic)", true, build2.ReleaseLicences.Contains(client3Lic));
				AssertEquals("build3.ReleaseLicences.Contains(client2Lic2)", true, build3.ReleaseLicences.Contains(client2Lic2));
			});
		}

		public void TestFilteredReleaseLicences()
		{
			ReleaseBuild build1 = Factory.New<ReleaseBuild>();
			ReleaseBuild build2 = Factory.New<ReleaseBuild>();
			ReleaseBuild build3 = Factory.New<ReleaseBuild>();

			build1.HL_Superceded = true;
			build2.HL_Superceded = false;
			build3.HL_Superceded = false;

			build1.VersionNumber = new VersionNumber(1, 1, 1, 0);
			build2.VersionNumber = new VersionNumber(1, 1, 1, 1);
			build3.VersionNumber = new VersionNumber(1, 1, 2, 0);

			EDIOrgHeader client1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client3 = Factory.NewWithValidTestData<EDIOrgHeader>();

			client1.CreateAndLoadLicenceForOrg();
			client2.CreateAndLoadLicenceForOrg();
			client3.CreateAndLoadLicenceForOrg();

			client1.LicenceEnterpriseCode = "CL1";
			client2.LicenceEnterpriseCode = "CL2";
			client3.LicenceEnterpriseCode = "CL3";

			var client1Lic1 = client1.LicCompany.GetHeader(client1.LicCompany.LicDatabases.AddNew());
			var client1Lic2 = client1.LicCompany.GetHeader(client1.LicCompany.LicDatabases.AddNew());
			var client2Lic1 = client2.LicCompany.GetHeader(client2.LicCompany.LicDatabases.AddNew());
			var client2Lic2 = client2.LicCompany.GetHeader(client2.LicCompany.LicDatabases.AddNew());
			var client3Lic = client3.LicCompany.GetHeader(client3.LicCompany.LicDatabases.AddNew());

			client1Lic1.Database.LD_HL_CurrentRunningVersion = build1.PK;
			client1Lic2.Database.LD_HL_CurrentRunningVersion = build2.PK;
			client2Lic1.Database.LD_HL_CurrentRunningVersion = build1.PK;
			client2Lic2.Database.LD_HL_CurrentRunningVersion = build3.PK;
			client3Lic.Database.LD_HL_CurrentRunningVersion = build2.PK;

			client1Lic1.Database.LD_ServerCode = "DB1";
			client1Lic2.Database.LD_ServerCode = "DB2";
			client2Lic1.Database.LD_ServerCode = "DB3";
			client2Lic2.Database.LD_ServerCode = "DB4";
			client3Lic.Database.LD_ServerCode = "DB5";

			Factory.Save();

			InternalIncidentLicenceSettings settings = new InternalIncidentLicenceSettings();
			LicenceEnterpriseKey key = new LicenceEnterpriseKey();
			key.LE_PK = client2.LicEnterprise.PK;
			settings.LicenceEnterpriseKeys.Add(key);
			settings.EdiProd_LicencePK = client2Lic1.PK;
			settings.UAT_ALP_LicencePK = client2Lic1.PK;
			settings.UAT_DPR_LicencePK = client2Lic1.PK;
			settings.UAT_GPC_LicencePK = client2Lic1.PK;
			settings.UAT_GPR_LicencePK = client2Lic1.PK;
			settings.UAT_STD_LicencePK = client2Lic1.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			AssertEquals("build1.ReleaseLicences.Count", 3, build1.FilteredReleaseLicences.Count);
			AssertEquals("build2.ReleaseLicences.Count", 3, build2.FilteredReleaseLicences.Count);
			AssertEquals("build3.ReleaseLicences.Count", 0, build3.FilteredReleaseLicences.Count);

			AssertEquals("build2.ReleaseLicences.Contains(client1Lic1)", true, build2.FilteredReleaseLicences.Contains(client1Lic1));
			AssertEquals("build2.ReleaseLicences.Contains(client1Lic2)", true, build2.FilteredReleaseLicences.Contains(client1Lic2));
			AssertEquals("build2.ReleaseLicences.Contains(client2Lic1)", false, build2.FilteredReleaseLicences.Contains(client2Lic1));
			AssertEquals("build2.ReleaseLicences.Contains(client3Lic)", true, build2.FilteredReleaseLicences.Contains(client3Lic));
			AssertEquals("build3.ReleaseLicences.Contains(client2Lic2)", false, build3.FilteredReleaseLicences.Contains(client2Lic2));

			build1.ShowCargoWiseLicences = true;
			build2.ShowCargoWiseLicences = true;
			build3.ShowCargoWiseLicences = true;

			AssertEquals("build1.ReleaseLicences.Count", 4, build1.FilteredReleaseLicences.Count);
			AssertEquals("build2.ReleaseLicences.Count", 4, build2.FilteredReleaseLicences.Count);
			AssertEquals("build3.ReleaseLicences.Count", 1, build3.FilteredReleaseLicences.Count);

			AssertEquals("build2.ReleaseLicences.Contains(client1Lic1)", true, build2.FilteredReleaseLicences.Contains(client1Lic1));
			AssertEquals("build2.ReleaseLicences.Contains(client1Lic2)", true, build2.FilteredReleaseLicences.Contains(client1Lic2));
			AssertEquals("build2.ReleaseLicences.Contains(client2Lic1)", true, build2.FilteredReleaseLicences.Contains(client2Lic1));
			AssertEquals("build2.ReleaseLicences.Contains(client3Lic)", true, build2.FilteredReleaseLicences.Contains(client3Lic));
			AssertEquals("build3.ReleaseLicences.Contains(client2Lic2)", true, build3.FilteredReleaseLicences.Contains(client2Lic2));
		}

		public void TestShowCargoWiseLicences()
		{
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			AssertEquals(false, build.FilteredReleaseLicences.IncludeAllItems);
			AssertEquals(false, build.ShowCargoWiseLicences);

			build.ShowCargoWiseLicences = true;
			AssertEquals(true, build.FilteredReleaseLicences.IncludeAllItems);

			build = Factory.New<ReleaseBuild>();
			AssertEquals(false, build.FilteredPatchLicences.IncludeAllItems);
			AssertEquals(false, build.ShowCargoWiseLicences);

			build.ShowCargoWiseLicences = true;
			AssertEquals(true, build.FilteredPatchLicences.IncludeAllItems);
		}

		public void TestVersionNumber()
		{
			AssertEquals("VersionNumber", new VersionNumber(), Build.VersionNumber);
			Build.VersionNumber = new VersionNumber(1, 2, 3, 4);
			AssertEquals("VersionNumber", new VersionNumber(1, 2, 3, 4), Build.VersionNumber);
			AssertEquals("HL_MajorVersion", 1, Build.HL_MajorVersion);
			AssertEquals("HL_MinorVersion", 2, Build.HL_MinorVersion);
			AssertEquals("HL_Release", 3, Build.HL_Release);
			AssertEquals("HL_Patch", 4, Build.HL_Patch);
		}

		public void TestReleaseDisplayText()
		{
			Build.HL_Product = ProductTypes.Codes.Enterprise;
			Build.VersionNumber = new VersionNumber(1, 4, 4667, 12);
			AssertEquals("2012 Oct 11 patch 12", Build.ReleaseDisplayText);
			var build = Factory.New<ReleaseBuild>();
			build.HL_Product = ProductTypes.Codes.Enterprise;
			build.VersionNumber = new VersionNumber(2, 0, 3, 0);
			AssertEquals("2013 Aug 11", build.ReleaseDisplayText);
		}

		#region Patch Client Count

		public void TestPatchClientCountWithParentCollection()
		{
			ReleaseBuildCollection collection = new ReleaseBuildCollection(Factory);

			ReleaseBuild build1 = collection.AddNew();
			ReleaseBuild build2 = collection.AddNew();
			ReleaseBuild build3 = collection.AddNew();

			build1.FillWithValidTestData();
			build2.FillWithValidTestData();
			build3.FillWithValidTestData();

			TestPatchClientCount(build1, build2, build3, 0, 3);
		}

		public void TestPatchClientCountWithoutParentCollection()
		{
			ReleaseBuild build1 = Factory.NewWithValidTestData<ReleaseBuild>();
			ReleaseBuild build2 = Factory.NewWithValidTestData<ReleaseBuild>();
			ReleaseBuild build3 = Factory.NewWithValidTestData<ReleaseBuild>();

			TestPatchClientCount(build1, build2, build3, 3, 0);
		}

		public void TestPatchClientCountWithInvalidParentCollection()
		{
			DummyBusinessObjectCollection collection = new DummyBusinessObjectCollection(Factory);

			ReleaseBuild build1 = Factory.NewWithValidTestData<ReleaseBuild>();
			ReleaseBuild build2 = Factory.NewWithValidTestData<ReleaseBuild>();
			ReleaseBuild build3 = Factory.NewWithValidTestData<ReleaseBuild>();

			collection.Add(build1);
			collection.Add(build2);
			collection.Add(build3);

			TestPatchClientCount(build1, build2, build3, 3, 0);
		}

		void TestPatchClientCount(ReleaseBuild build1, ReleaseBuild build2, ReleaseBuild build3, int expectedIncrementAfterAccessingClientCount, int expectedIncrementAfterAccessingPatchLicences)
		{
			EDIOrgHeader client1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client3 = Factory.NewWithValidTestData<EDIOrgHeader>();

			client1.CreateAndLoadLicenceForOrg();
			client2.CreateAndLoadLicenceForOrg();
			client3.CreateAndLoadLicenceForOrg();

			client1.LicenceEnterpriseCode = "CL1";
			client2.LicenceEnterpriseCode = "CL2";
			client3.LicenceEnterpriseCode = "CL3";

			LicenceDatabase database1 = client1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database2 = client1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database3 = client2.LicCompany.LicDatabases.AddNew();

			database1.LD_HL_CurrentRunningVersion = build1.PK;
			database2.LD_HL_CurrentRunningVersion = build1.PK;
			database3.LD_HL_CurrentRunningVersion = build2.PK;

			database1.LD_ServerCode = "DB1";
			database2.LD_ServerCode = "DB2";
			database3.LD_ServerCode = "DB3";

			Factory.Save();

			int initialLoadCount = Factory.DatabaseLoadCount;

			AssertEquals("build1.PatchClientCount", 2, build1.PatchClientCount);
			AssertEquals("build2.PatchClientCount", 1, build2.PatchClientCount);
			AssertEquals("build3.PatchClientCount", 0, build3.PatchClientCount);
			AssertEquals("Factory.DatabaseLoadCount", initialLoadCount + expectedIncrementAfterAccessingClientCount, Factory.DatabaseLoadCount);

			initialLoadCount = Factory.DatabaseLoadCount;

			AssertEquals("build1.PatchLicences.Count", build1.PatchClientCount, build1.PatchLicences.Count);
			AssertEquals("build2.PatchLicences.Count", build2.PatchClientCount, build2.PatchLicences.Count);
			AssertEquals("build3.PatchLicences.Count", build3.PatchClientCount, build3.PatchLicences.Count);
			AssertEquals("Factory.DatabaseLoadCount", initialLoadCount + expectedIncrementAfterAccessingPatchLicences, Factory.DatabaseLoadCount);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ReleaseBuild result = (ReleaseBuild)base.GetNewBusinessObject();
			result.HL_ExeVersionDate = ZDateTime.Now;
			result.HL_PackagePath = TestBuildPackagePath;
			return result;
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				Dictionary<string, IZType> result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result[ReleaseBuild.Schema.HL_ExeVersionDate] = new ZDateTime(2007, 2, 13, 2, 3, 3);
				return result;
			}
		}

		ReleaseBuild Build
		{
			get
			{
				if (build == null)
				{
					build = Factory.New<ReleaseBuild>();
				}
				return build;
			}
		}

		ReleaseBuild build;

		EmbeddedResourceRetriever resourceRetriever;
		string TestBuildPackagePath;
		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			TestBuildPackagePath = resourceRetriever.SaveResourceToFile("ZClientEDI.Business.Test.DummyUpgradePackageCS.zip");
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}

		#endregion
	}

	public class ReleaseBuildTestWithoutTransaction : TestCase
	{
		[UseSnapshotProtection]
		public void TestClientSpecificCodes_MemoryUsage()
		{
			new ClientDbSchemaCreationForTesting().RunClientDbCreateScripts(false);
			var factory = new BusinessObjectFactory();
			ReleaseBuild build = factory.New<ReleaseBuild>();
			build.HL_ExeVersionDate = ZDateTime.Now;
			build.HL_ReleaseStatus = ReleaseRings.Codes.ETL;

			using (var tempFile = TempFile.New())
			{
				bool haveEnoughDiskSpace = false;
				try
				{
					using (var fileStream = File.OpenWrite(tempFile.Filename))
					{
						fileStream.SetLength(2000000000L);
					}
					haveEnoughDiskSpace = true;
				}
				catch { }

				if (haveEnoughDiskSpace)
				{
					using (var fileStream = File.OpenWrite(tempFile.Filename))
					{
						using (var zipArchive = new System.IO.Compression.ZipArchive(fileStream, System.IO.Compression.ZipArchiveMode.Create, true))
						{
							var zipEntry = zipArchive.CreateEntry("one", System.IO.Compression.CompressionLevel.NoCompression);
							using (var entryStream = zipEntry.Open())
							{
								var chunk = new byte[1000 * 1000];
								for (int i = 0; i < chunk.Length; ++i)
								{
									chunk[i] = 0;
								}

								for (int i = 0; i < 2000; ++i)
								{
									entryStream.Write(chunk, 0, chunk.Length);
								}
							}
						}
						Assert("we have a huge file", fileStream.Length > 2000000000L);
					}

					build.HL_PackagePath = tempFile.Filename;
					AssertEquals("Count", 0, build.ClientSpecificCodes.Count);
				}
				else
				{
					Assert("this system doesn't have enough disk space for this test", true);
				}
			}
		}
	}

	#region class MockReleaseBuild

	public class MockReleaseBuild : ReleaseBuild
	{
		public MockReleaseBuild(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override string PackageName
		{
			get { return "Package20041229_135600_1_2_3_4.txt"; }
		}

		protected override string FTPClientSpecificDirectoryName
		{
			get { return Path.Combine(Env.TempPath, "ediEnterpriseUpgrades\\ClientSpecific"); }
		}

		protected override string FTPGenericDirectoryName
		{
			get { return Path.Combine(Env.TempPath, "ediEnterpriseUpgrades\\Generic"); }
		}

		protected override void DeleteFile(string path)
		{
			if (FtpDeleteShouldFail)
			{
				throw new Exception("Something happened");
			}
			else
			{
				base.DeleteFile(path);
			}
		}

		public void SetClientSpecificCodes(params string[] value)
		{
			clientSpecificCodes = new ReadOnlyCollection<string>(value);
		}

		public bool FtpDeleteShouldFail
		{
			get { return ftpDeleteShouldFail; }
			set { ftpDeleteShouldFail = value; }
		}

		public new ReadOnlyCollection<string> ClientSpecificCodes
		{
			get { return base.ClientSpecificCodes; }
		}

		protected override ReadOnlyCollection<string> CalculateClientSpecificCodes()
		{
			return clientSpecificCodes ?? base.CalculateClientSpecificCodes();
		}

		public void ThrowDuringGetCodes(Exception ex)
		{
			getCodesException = ex;
		}

		Exception getCodesException;
		protected override void GetCodesFromPackage(List<string> result, string zClientWebFilePattern, string zClientFilePattern, string documentsXmlFilePattern)
		{
			RetryCount++;

			if (getCodesException != null)
			{
				throw getCodesException;
			}
			else if (!SkipGetCodes)
			{
				base.GetCodesFromPackage(result, zClientWebFilePattern, zClientFilePattern, documentsXmlFilePattern);
			}
		}

		bool ftpDeleteShouldFail;
		ReadOnlyCollection<string> clientSpecificCodes;
		public int RetryCount;
		public bool SkipGetCodes;
	}

	#endregion
}
