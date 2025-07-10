using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class VersionHistoryTest : TestCaseWithFactory
	{
		[TestDate(2022, 5, 1)]
		public void TestGetVersionSurchargePercent_Registry_L9Override()
		{
			var billingDate = new DateTime(2022, 4, 1);
			var db1 = BillingTestHelper.CreateLicence(Factory, "DB1", false).Database;
			db1.LD_ReleaseRing = "GP1";
			CreateBuildsAndSetDatabaseToNonCurrentVersion(db1, billingDate, 5);
			Factory.Save();

			AssertEquals(2m, EDIDataRegistry.Instance.VersionSurchargePercent.Value);
			AssertEquals(1m, EDIDataRegistry.Instance.VersionSurchargeAdditionalPercent.Value);
			AssertEquals(2m + 1m + 1m + 1m, new VersionHistory().GetVersionSurchargePercent(db1, null, billingDate));

			EDIDataRegistry.Instance.VersionSurchargePercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			EDIDataRegistry.Instance.VersionSurchargeAdditionalPercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
			AssertEquals(5m + 3m + 3m + 3m, new VersionHistory().GetVersionSurchargePercent(db1, null, billingDate));

			var setting = Factory.New<VersionSurchargeLicenceSetting>();
			setting.LS9_Percent = 7m;
			setting.AdditionalPercent = 9m;
			AssertEquals(7m + 9m + 9m + 9m, new VersionHistory().GetVersionSurchargePercent(db1, setting, billingDate));

			setting.LS9_Percent = 0m;
			setting.AdditionalPercent = 0m;
			AssertEquals(0m, new VersionHistory().GetVersionSurchargePercent(db1, setting, billingDate));

			EDIDataRegistry.Instance.VersionSurchargePercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			EDIDataRegistry.Instance.VersionSurchargeAdditionalPercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertEquals(0m, new VersionHistory().GetVersionSurchargePercent(db1, null, billingDate));
		}

		[TestDate(2022, 5, 1)]
		public void TestParseVersionFromLogReference()
		{
			var billingDate = new DateTime(2022, 1, 1);
			var db1 = BillingTestHelper.CreateLicence(Factory, "STD", false).Database;
			db1.LD_ReleaseRing = "STD";
			CreateBuildsAndSetDatabaseToNonCurrentVersion(db1, new ZDateTime(2021, 12, 1), 10);
			db1.LD_CurrentVersionFirstReportUtc = billingDate.AddMonths(3);
			Factory.Save();
			AssertEquals(2m + (10 - 3) * 1m, new VersionHistory().GetVersionSurchargePercent(db1, null, billingDate));
			var versionLog = string.Join("\r\n", db1.Logs.Find(new ZQuery()).Where(x => x.SL_Reference.Contains("Version"))
				.OrderBy(x => x.SL_PostedTimeUtc).Select(x => x.SL_Reference));
			AssertEquals(@"Server STD - Version: 1.1.1.0()
Server STD - Upgrade Method: HTP() Version: 20.12.6.500(1.1.1.0)", versionLog);
		}

		[TestDate(2022, 5, 1)]
		public void TestGetVersionSurchargePercent_STD_DPR_ALP()
		{
			var billingDate = new DateTime(2022, 4, 1);
			var db1 = BillingTestHelper.CreateLicence(Factory, "STD", false).Database;
			db1.LD_ReleaseRing = "STD";
			CreateBuildsAndSetDatabaseToNonCurrentVersion(db1, billingDate, 10);

			var db2 = BillingTestHelper.CreateLicence(Factory, "DPR", false).Database;
			db2.LD_ReleaseRing = "DPR";
			CreateBuildsAndSetDatabaseToNonCurrentVersion(db2, billingDate, 10);

			var db3 = BillingTestHelper.CreateLicence(Factory, "ALP", false).Database;
			db3.LD_ReleaseRing = "ALP";
			CreateBuildsAndSetDatabaseToNonCurrentVersion(db3, billingDate, 10);

			Factory.Save();

			AssertEquals(2m, EDIDataRegistry.Instance.VersionSurchargePercent.Value);
			AssertEquals(1m, EDIDataRegistry.Instance.VersionSurchargeAdditionalPercent.Value);

			AssertEquals(2m + (10 - 3) * 1m, new VersionHistory().GetVersionSurchargePercent(db1, null, billingDate));
			AssertEquals(2m + (10 - 4) * 1m, new VersionHistory().GetVersionSurchargePercent(db2, null, billingDate));
			AssertEquals(0m, new VersionHistory().GetVersionSurchargePercent(db3, null, billingDate));
		}

		[TestDate(2025, 5, 1)]
		public void TestGetVersionSurchargePercent_STD_CW1toCWN()
		{
			/*  
				the final CW1, 24.10.30.x
				the first CWN, 24.11.1.0
			*/

			var cw1_0 = CreateReleaseBuild(24, 06, 01, 01, new DateTime(2024, 06, 15), "STD");
			var cw1_1 = CreateReleaseBuild(24, 07, 01, 01, new DateTime(2024, 07, 15), "STD");
			var cw1_2 = CreateReleaseBuild(24, 08, 01, 01, new DateTime(2024, 08, 15), "STD");
			var cw1_3 = CreateReleaseBuild(24, 09, 01, 01, new DateTime(2024, 09, 15), "STD");
			var cw1_4 = CreateReleaseBuild(24, 10, 01, 01, new DateTime(2024, 10, 15), "STD");
			var cw1_5 = CreateReleaseBuild(24, 10, 30, 100, new DateTime(2024, 10, 30), "STD"); //final cw1.

			//only patches for cw1
			var cw1_5_1 = CreateReleaseBuild(24, 10, 30, 200, new DateTime(2024, 11, 15), "STD");
			var cw1_5_2 = CreateReleaseBuild(24, 10, 30, 300, new DateTime(2024, 12, 15), "STD");
			var cw1_5_3 =  CreateReleaseBuild(24, 10, 30, 400, new DateTime(2025, 01, 15), "STD");
			var cw1_5_4 = CreateReleaseBuild(24, 10, 30, 500, new DateTime(2025, 02, 15), "STD");
			var cw1_5_5 = CreateReleaseBuild(24, 10, 30, 600, new DateTime(2025, 03, 15), "STD");

			var cwn_1 = CreateReleaseBuild(24, 11, 01, 01, new DateTime(2024, 11, 15), "STD", "CWN"); //first CWN
			var cwn_2 = CreateReleaseBuild(24, 12, 01, 01, new DateTime(2024, 12, 15), "STD", "CWN");
			var cwn_3 = CreateReleaseBuild(25, 01, 01, 01, new DateTime(2025, 01, 15), "STD", "CWN");
			var cwn_4 = CreateReleaseBuild(25, 02, 01, 01, new DateTime(2025, 02, 15), "STD", "CWN");
			var cwn_5 = CreateReleaseBuild(25, 03, 01, 01, new DateTime(2025, 03, 15), "STD", "CWN");
			var cwn_6 = CreateReleaseBuild(25, 04, 01, 01, new DateTime(2025, 04, 15), "STD", "CWN");
			Factory.Save();

			AssertEquals(2m, EDIDataRegistry.Instance.VersionSurchargePercent.Value);
			AssertEquals(1m, EDIDataRegistry.Instance.VersionSurchargeAdditionalPercent.Value);

			var billingDate = new DateTime(2025, 4, 1);

			//old cw1
			var cw1Old = BillingTestHelper.CreateLicence(Factory, "E01", createClientCompany: false).Database;
			cw1Old.LD_ReleaseRing = "STD";
			Upgrade(cw1Old, cw1_0, new DateTime(2024, 6, 20));
			AssertVersionSurchargePercent(2m + (5 - 3) * 1m, cw1Old, billingDate); //5 versions old

			//last cw1
			var cw1LastCW1 = BillingTestHelper.CreateLicence(Factory, "E02", createClientCompany: false).Database;
			cw1LastCW1.LD_ReleaseRing = "STD";
			Upgrade(cw1LastCW1, cw1_1, new DateTime(2024, 7, 20));
			Upgrade(cw1LastCW1, cw1_5, new DateTime(2024, 10, 30));
			AssertVersionSurchargePercent(0m, cw1LastCW1, billingDate); //latest cw1

			//recent cw1 patch
			var cw1RecentPatch = BillingTestHelper.CreateLicence(Factory, "E03", createClientCompany: false).Database;
			cw1RecentPatch.LD_ReleaseRing = "STD";
			Upgrade(cw1RecentPatch, cw1_1, new DateTime(2024, 7, 20));
			Upgrade(cw1RecentPatch, cw1_5, new DateTime(2024, 10, 30));
			Upgrade(cw1RecentPatch, cw1_5_3, new DateTime(2025, 01, 20));
			AssertVersionSurchargePercent(0m, cw1RecentPatch, billingDate); //latest cw1

			//old cwn
			var cwnOld = BillingTestHelper.CreateLicence(Factory, "E04", createClientCompany: false).Database;
			cwnOld.LD_Product = "CWN";
			cwnOld.LD_ReleaseRing = "STD";
			Upgrade(cwnOld, cwn_2, new DateTime(2024, 12, 28));
			AssertVersionSurchargePercent(2m + (4 - 3) * 1m, cwnOld, billingDate);  //4 versions old

			//recent cwn
			var cwnRecentCWN = BillingTestHelper.CreateLicence(Factory, "E05", createClientCompany: false).Database;
			cwnRecentCWN.LD_Product = "CWN";
			cwnRecentCWN.LD_ReleaseRing = "STD";
			Upgrade(cwnRecentCWN, cwn_1, new DateTime(2024, 11, 28));
			Upgrade(cwnRecentCWN, cwn_3, new DateTime(2025, 01, 20));
			AssertVersionSurchargePercent(2m + (3 - 3) * 1m, cwnRecentCWN, billingDate); //3 versions old

			//latest cwn
			var cwnLatestCWN = BillingTestHelper.CreateLicence(Factory, "E06", createClientCompany: false).Database;
			cwnLatestCWN.LD_Product = "CWN";
			cwnLatestCWN.LD_ReleaseRing = "STD";
			Upgrade(cwnLatestCWN, cwn_1, new DateTime(2024, 11, 28));
			Upgrade(cwnLatestCWN, cwn_5, new DateTime(2025, 03, 10));
			Upgrade(cwnLatestCWN, cwn_6, new DateTime(2025, 04, 20));
			AssertVersionSurchargePercent(0m, cwnLatestCWN, billingDate); //latest cwn

			//old cw1 -> old cwn
			var cw1Old2OldCWN = BillingTestHelper.CreateLicence(Factory, "E07", createClientCompany: false).Database;
			cw1Old2OldCWN.LD_ReleaseRing = "STD";
			Upgrade(cw1Old2OldCWN, cw1_0, new DateTime(2024, 6, 20));
			cw1Old2OldCWN.LD_Product = "CWN";
			Factory.Save();
			Upgrade(cw1Old2OldCWN, cwn_2, new DateTime(2024, 12, 28));
			AssertVersionSurchargePercent(2m + (4 - 3) * 1m, cw1Old2OldCWN, billingDate);  //4 versions old

			//old cw1 -> latest cwn
			var cw1Old2NewCWN = BillingTestHelper.CreateLicence(Factory, "E08", createClientCompany: false).Database;
			cw1Old2NewCWN.LD_ReleaseRing = "STD";
			Upgrade(cw1Old2NewCWN, cw1_0, new DateTime(2024, 6, 20));
			cw1Old2NewCWN.LD_Product = "CWN";
			Factory.Save();
			Upgrade(cw1Old2NewCWN, cwn_6, new DateTime(2025, 04, 20));
			AssertVersionSurchargePercent(0m, cw1Old2NewCWN, billingDate); //latest cwn

			//new cw1 -> old cwn
			var cw1New2OldCWN = BillingTestHelper.CreateLicence(Factory, "E09", createClientCompany: false).Database;
			cw1New2OldCWN.LD_ReleaseRing = "STD";
			Upgrade(cw1New2OldCWN, cw1_5, new DateTime(2024, 10, 30));
			cw1New2OldCWN.LD_Product = "CWN";
			Factory.Save();
			Upgrade(cw1New2OldCWN, cwn_2, new DateTime(2024, 12, 28));
			AssertVersionSurchargePercent(2m + (4 - 3) * 1m, cw1New2OldCWN, billingDate);  //4 versions old

			//new cw1 -> new cwn
			var cw1New2NewCWN = BillingTestHelper.CreateLicence(Factory, "E10", createClientCompany: false).Database;
			cw1New2NewCWN.LD_ReleaseRing = "STD";
			Upgrade(cw1New2NewCWN, cw1_5, new DateTime(2024, 10, 28));
			cw1New2NewCWN.LD_Product = "CWN";
			Factory.Save();
			Upgrade(cw1New2NewCWN, cwn_6, new DateTime(2025, 04, 20));
			AssertVersionSurchargePercent(0m, cw1New2NewCWN, billingDate); //latest cwn
		}

		[TestDate(2025, 5, 1)]
		public void TestGetVersionSurchargePercent_STD_CW1toCGW()
		{
			/*  
				the final CW1, 24.10.30.x
				the first CGW, 25.4.7.0
			*/

			var cw1_0 = CreateReleaseBuild(24, 06, 01, 01, new DateTime(2024, 06, 15), "STD");
			var cw1_1 = CreateReleaseBuild(24, 07, 01, 01, new DateTime(2024, 07, 15), "STD");
			var cw1_2 = CreateReleaseBuild(24, 08, 01, 01, new DateTime(2024, 08, 15), "STD");
			var cw1_3 = CreateReleaseBuild(24, 09, 01, 01, new DateTime(2024, 09, 15), "STD");
			var cw1_4 = CreateReleaseBuild(24, 10, 01, 01, new DateTime(2024, 10, 15), "STD");
			var cw1_5 = CreateReleaseBuild(24, 10, 30, 100, new DateTime(2024, 10, 30), "STD"); //final cw1.

			//only patches for cw1
			var cw1_5_1 = CreateReleaseBuild(24, 10, 30, 200, new DateTime(2024, 11, 15), "STD");
			var cw1_5_2 = CreateReleaseBuild(24, 10, 30, 300, new DateTime(2024, 12, 15), "STD");
			var cw1_5_3 = CreateReleaseBuild(24, 10, 30, 400, new DateTime(2025, 01, 15), "STD");
			var cw1_5_4 = CreateReleaseBuild(24, 10, 30, 500, new DateTime(2025, 02, 15), "STD");
			var cw1_5_5 = CreateReleaseBuild(24, 10, 30, 600, new DateTime(2025, 03, 15), "STD");

			var cgw_1 = CreateReleaseBuild(25, 4, 7, 1, new DateTime(2025, 4, 15), "STD", "CGW"); //first CGW
			var cgw_2 = CreateReleaseBuild(25, 5, 7, 1, new DateTime(2025, 5, 15), "STD", "CGW");
			var cgw_3 = CreateReleaseBuild(25, 6, 7, 1, new DateTime(2025, 6, 15), "STD", "CGW");
			var cgw_4 = CreateReleaseBuild(25, 7, 7, 1, new DateTime(2025, 7, 15), "STD", "CGW");
			var cgw_5 = CreateReleaseBuild(25, 8, 7, 1, new DateTime(2025, 8, 15), "STD", "CGW");
			var cgw_6 = CreateReleaseBuild(25, 9, 7, 1, new DateTime(2025, 9, 15), "STD", "CGW");
			Factory.Save();

			AssertEquals(2m, EDIDataRegistry.Instance.VersionSurchargePercent.Value);
			AssertEquals(1m, EDIDataRegistry.Instance.VersionSurchargeAdditionalPercent.Value);

			var billingDate = new DateTime(2025, 9, 1);

			//old cw1
			var cw1Old = BillingTestHelper.CreateLicence(Factory, "E01", createClientCompany: false).Database;
			cw1Old.LD_ReleaseRing = "STD";
			Upgrade(cw1Old, cw1_0, new DateTime(2024, 6, 20));
			AssertVersionSurchargePercent(2m + (5 - 3) * 1m, cw1Old, billingDate); //5 versions old

			//last cw1
			var cw1LastCW1 = BillingTestHelper.CreateLicence(Factory, "E02", createClientCompany: false).Database;
			cw1LastCW1.LD_ReleaseRing = "STD";
			Upgrade(cw1LastCW1, cw1_1, new DateTime(2024, 7, 20));
			Upgrade(cw1LastCW1, cw1_5, new DateTime(2024, 10, 30));
			AssertVersionSurchargePercent(0m, cw1LastCW1, billingDate); //latest cw1

			//recent cw1 patch
			var cw1RecentPatch = BillingTestHelper.CreateLicence(Factory, "E03", createClientCompany: false).Database;
			cw1RecentPatch.LD_ReleaseRing = "STD";
			Upgrade(cw1RecentPatch, cw1_1, new DateTime(2024, 7, 20));
			Upgrade(cw1RecentPatch, cw1_5, new DateTime(2024, 10, 30));
			Upgrade(cw1RecentPatch, cw1_5_3, new DateTime(2025, 01, 20));
			AssertVersionSurchargePercent(0m, cw1RecentPatch, billingDate); //latest cw1

			//old cgw
			var cgwOld = BillingTestHelper.CreateLicence(Factory, "E04", createClientCompany: false).Database;
			cgwOld.LD_ReleaseRing = "STD";
			cgwOld.LD_Product = "CGW";
			Upgrade(cgwOld, cgw_2, new DateTime(2025, 5, 28));
			AssertVersionSurchargePercent(2m + (4 - 3) * 1m, cgwOld, billingDate);  //4 versions old

			//recent cwn
			var cgwRecentCGW = BillingTestHelper.CreateLicence(Factory, "E05", createClientCompany: false).Database;
			cgwRecentCGW.LD_Product = "CGW";
			cgwRecentCGW.LD_ReleaseRing = "STD";
			Upgrade(cgwRecentCGW, cgw_1, new DateTime(2025, 4, 28));
			Upgrade(cgwRecentCGW, cgw_3, new DateTime(2025, 6, 20));
			AssertVersionSurchargePercent(2m + (3 - 3) * 1m, cgwRecentCGW, billingDate); //3 versions old

			//latest cgw
			var cgwLatestCGW = BillingTestHelper.CreateLicence(Factory, "E06", createClientCompany: false).Database;
			cgwLatestCGW.LD_Product = "CGW";
			cgwLatestCGW.LD_ReleaseRing = "STD";
			Upgrade(cgwLatestCGW, cgw_1, new DateTime(2025, 4, 28));
			Upgrade(cgwLatestCGW, cgw_5, new DateTime(2025, 8, 10));
			Upgrade(cgwLatestCGW, cgw_6, new DateTime(2025, 9, 20));
			AssertVersionSurchargePercent(0m, cgwLatestCGW, billingDate); //latest cgw

			//old cw1 -> old cgw
			var cw1Old2OldCGW = BillingTestHelper.CreateLicence(Factory, "E07", createClientCompany: false).Database;
			cw1Old2OldCGW.LD_ReleaseRing = "STD";
			Upgrade(cw1Old2OldCGW, cw1_0, new DateTime(2024, 6, 20));
			cw1Old2OldCGW.LD_Product = "CGW";
			Factory.Save();
			Upgrade(cw1Old2OldCGW, cgw_2, new DateTime(2024, 12, 28));
			AssertVersionSurchargePercent(2m + (4 - 3) * 1m, cw1Old2OldCGW, billingDate);  //4 versions old

			//old cw1 -> latest cgw
			var cw1Old2NewCGW = BillingTestHelper.CreateLicence(Factory, "E08", createClientCompany: false).Database;
			cw1Old2NewCGW.LD_ReleaseRing = "STD";
			Upgrade(cw1Old2NewCGW, cw1_0, new DateTime(2024, 6, 20));
			cw1Old2NewCGW.LD_Product = "CGW";
			Factory.Save();
			Upgrade(cw1Old2NewCGW, cgw_6, new DateTime(2025, 04, 20));
			AssertVersionSurchargePercent(0m, cw1Old2NewCGW, billingDate); //latest cgw

			//new cw1 -> old cgw
			var cw1New2OldCGW = BillingTestHelper.CreateLicence(Factory, "E09", createClientCompany: false).Database;
			cw1New2OldCGW.LD_ReleaseRing = "STD";
			Upgrade(cw1New2OldCGW, cw1_5, new DateTime(2024, 10, 30));
			cw1New2OldCGW.LD_Product = "CGW";
			Factory.Save();
			Upgrade(cw1New2OldCGW, cgw_2, new DateTime(2024, 12, 28));
			AssertVersionSurchargePercent(2m + (4 - 3) * 1m, cw1New2OldCGW, billingDate);  //4 versions old

			//new cw1 -> new cgw
			var cw1New2NewCGW = BillingTestHelper.CreateLicence(Factory, "E10", createClientCompany: false).Database;
			cw1New2NewCGW.LD_ReleaseRing = "STD";
			Upgrade(cw1New2NewCGW, cw1_5, new DateTime(2024, 10, 28));
			cw1New2NewCGW.LD_Product = "CGW";
			Factory.Save();
			Upgrade(cw1New2NewCGW, cgw_6, new DateTime(2025, 04, 20));
			AssertVersionSurchargePercent(0m, cw1New2NewCGW, billingDate); //latest cgw
		}

		void Upgrade(LicenceDatabase db, ReleaseBuild build, DateTime upgradeDate)
		{
			var currentTestDate = TestDateAttribute.Date;

			using (new DisposableAction(() =>
			{
				TestDateAttribute.Date = currentTestDate;
			}))
			{
				db.LD_HL_CurrentRunningVersion = build.PK;
				db.LD_CurrentVersionFirstReportUtc = upgradeDate;
				TestDateAttribute.Date = db.LD_CurrentVersionFirstReportUtc.ToDateTime();
				db.Factory.Save();
			}
		}

		static void AssertVersionSurchargePercent(decimal expectedVersionSurchargePercent, LicenceDatabase db, DateTime billingDate)
		{
			AssertEquals(expectedVersionSurchargePercent, new VersionHistory().GetVersionSurchargePercent(db, null, billingDate));
		}

		[TestDate(2022, 5, 1)]
		public void TestGetVersionSurchargePercent_GP1()
		{
			AssertGetVersionSurchargePercentGPx("GP1");
		}

		[TestDate(2022, 5, 1)]
		public void TestGetVersionSurchargePercent_GP2()
		{
			AssertGetVersionSurchargePercentGPx("GP2");
		}

		void AssertGetVersionSurchargePercentGPx(string releaseRing)
		{
			/*
Release Ring	Version	Exe Date	
GPx	22.2.17.X	2022-03-30	Current Version
GPx	21.11.27.X	2022-01-05	Immediately Prior Version
GPx	21.8.25.X	2021-10-06	Unsupported
GPx	21.5.19.X	2021-07-01	Unsupported + 1
			 */

			var v22_2_17_0 = CreateReleaseBuild(22, 2, 17, 0, new DateTime(2022, 03, 30), releaseRing);
			var v21_11_27_0 = CreateReleaseBuild(21, 11, 27, 0, new DateTime(2022, 01, 05), releaseRing);
			var v21_8_25_0 = CreateReleaseBuild(21, 8, 25, 0, new DateTime(2021, 10, 06), releaseRing);
			var v21_5_19_0 = CreateReleaseBuild(21, 5, 19, 0, new DateTime(2021, 07, 01), releaseRing);

			Factory.Save();

			var versionHistory = new VersionHistory();

			/* Case 1
															|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on current version at start of billing period		|	Apr-22			|	2022-04-01	|	22.2.17.X	|	Current Version
CW1 is on current version on the last day of billing period	|					|	2022-04-30	|	22.2.17.X	|	Current Version
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db1 = BillingTestHelper.CreateLicence(Factory, "DB1", false).Database;
			db1.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db1.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db1.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 30);
			db1.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db1.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 25);
			Factory.Save();

			AssertEquals(0m, versionHistory.GetVersionSurchargePercent(db1, null, new DateTime(2022, 4, 1)));

			/* Case 2
																|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on imediately prior version at start of billing period	|	Apr-22			|	2022-04-01	|	21.11.27.X	|	Immediately Prior Version
CW1 is upgraded to current version before grace period end		|					|	2022-04-28	|	22.2.17.X	|	Current Version
CW1 is on current version on the last day of billing period		|					|	2022-04-30	|	22.2.17.X	|	Current Version
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db2 = BillingTestHelper.CreateLicence(Factory, "DB2", false).Database;
			db2.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db2.LD_HL_CurrentRunningVersion = v21_11_27_0.PK;
			db2.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 28);
			db2.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db2.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 25);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 30);
			db2.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db2.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 29);
			Factory.Save();

			AssertEquals(0m, versionHistory.GetVersionSurchargePercent(db2, null, new DateTime(2022, 4, 1)));

			/* Case 3
																|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on imediately prior version at start of billing period	|	Apr-22			|	2022-04-01	|	21.11.27.X	|	Immediately Prior Version
CW1 is upgraded to current version after grace period end		|					|	2022-04-30	|	22.2.17.X	|	Current Version
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db3 = BillingTestHelper.CreateLicence(Factory, "DB3", false).Database;
			db3.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db3.LD_HL_CurrentRunningVersion = v21_11_27_0.PK;
			db3.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 30);
			db3.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db3.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 25);
			Factory.Save();

			AssertEquals(0m, versionHistory.GetVersionSurchargePercent(db3, null, new DateTime(2022, 4, 1)));

			/* Case 4
																			|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on imediately prior version at start of billing period				|	Apr-22			|	2022-04-01	|	21.11.27.X	|	Immediately Prior Version
CW1 is on imediately prior version on the last day of billing period		|					|	2022-04-30	|	21.11.27.X	|	Immediately Prior Version
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db4 = BillingTestHelper.CreateLicence(Factory, "DB4", false).Database;
			db4.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db4.LD_HL_CurrentRunningVersion = v21_11_27_0.PK;
			db4.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 30);
			db4.LD_HL_CurrentRunningVersion = v21_11_27_0.PK;
			db4.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 25);
			Factory.Save();

			AssertEquals(0m, versionHistory.GetVersionSurchargePercent(db4, null, new DateTime(2022, 4, 1)));

			/* Case 5
															|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on unsupported version at start of billing period	|	Apr-22			|	2022-04-01	|	21.8.25.X	|	Unsupported
CW1 is upgraded to current version before grace period end	|					|	2022-04-15	|	22.2.17.X	|	Current Version
CW1 is on current version on the last day of billing period	|					|	2022-04-30	|	22.2.17.X	|	Current Version
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db5 = BillingTestHelper.CreateLicence(Factory, "DB5", false).Database;
			db5.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db5.LD_HL_CurrentRunningVersion = v21_8_25_0.PK;
			db5.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 15);
			db5.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db5.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 15);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 30);
			db5.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db5.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 30);
			Factory.Save();

			AssertEquals(0m, versionHistory.GetVersionSurchargePercent(db5, null, new DateTime(2022, 4, 1)));

			/* Case 6
															|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on unsupported version at start of billing period	|	Apr-22			|	2022-04-01	|	21.8.25.X	|	Unsupported
CW1 is upgraded to current version after grace period end	|					|	2022-04-30	|	22.2.17.X	|	Current Version
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db6 = BillingTestHelper.CreateLicence(Factory, "DB6", false).Database;
			db6.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db6.LD_HL_CurrentRunningVersion = v21_8_25_0.PK;
			db6.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 30);
			db6.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db6.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 30);
			Factory.Save();

			AssertEquals(0m, versionHistory.GetVersionSurchargePercent(db6, null, new DateTime(2022, 4, 1)));

			/* Case 7
																		|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on unsupported version at start of billing period				|	Apr-22			|	2022-04-01	|	21.8.25.X	|	Unsupported
CW1 is upgraded to imediately prior version after grace period end		|					|	2022-04-15	|	21.11.27.X	|	Immediately Prior Version
CW1 is on imediately prior version on the last day of billing period	|					|	2022-04-30	|	21.11.27.X	|	Immediately Prior Version
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db7 = BillingTestHelper.CreateLicence(Factory, "DB7", false).Database;
			db7.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db7.LD_HL_CurrentRunningVersion = v21_8_25_0.PK;
			db7.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 15);
			db7.LD_HL_CurrentRunningVersion = v21_11_27_0.PK;
			db7.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 15);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 30);
			db7.LD_HL_CurrentRunningVersion = v21_11_27_0.PK;
			db7.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 30);
			Factory.Save();

			AssertEquals(0m, versionHistory.GetVersionSurchargePercent(db7, null, new DateTime(2022, 4, 1)));

			/* Case 8
																	|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on unsupported version at start of billing period			|	Apr-22			|	2022-04-01	|	21.8.25.X	|	Unsupported
CW1 is upgraded to imediately prior version after grace period end	|					|	2022-04-15	|	21.11.27.X	|	Immediately Prior Version
CW1 is upgraded to current version after grace period end			|					|	2022-04-30	|	22.2.17.X	|	Current Version
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db8 = BillingTestHelper.CreateLicence(Factory, "DB8", false).Database;
			db8.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db8.LD_HL_CurrentRunningVersion = v21_8_25_0.PK;
			db8.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 15);
			db8.LD_HL_CurrentRunningVersion = v21_11_27_0.PK;
			db8.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 15);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 30);
			db8.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db8.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 30);
			Factory.Save();

			AssertEquals(0m, versionHistory.GetVersionSurchargePercent(db8, null, new DateTime(2022, 4, 1)));

			/* Case 9
																	|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on unsupported version at start of billing period			|	Apr-22			|	2022-04-01	|	21.8.25.X	|	Unsupported
CW1 is upgraded to imediately prior version after grace period end	|					|	2022-04-15	|	21.11.27.X	|	Immediately Prior Version
CW1 is upgraded to current version before grace period end			|					|	2022-04-20	|	22.2.17.X	|	Current Version
CW1 is on current version on the last day of billing period			|					|	2022-04-30	|	22.2.17.X	|	Current Version
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db9 = BillingTestHelper.CreateLicence(Factory, "DB9", false).Database;
			db9.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db9.LD_HL_CurrentRunningVersion = v21_8_25_0.PK;
			db9.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 15);
			db9.LD_HL_CurrentRunningVersion = v21_11_27_0.PK;
			db9.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 15);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 20);
			db9.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db9.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 30);
			db9.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db9.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 30);
			Factory.Save();

			AssertEquals(0m, versionHistory.GetVersionSurchargePercent(db9, null, new DateTime(2022, 4, 1)));

			/* Case 10
																	|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on unsupported+1 version at start of billing period			|	Apr-22			|	2022-04-01	|	21.5.19.X	|	Unsupported+1
CW1 is upgraded to unsupported version after grace period end		|					|	2022-04-07	|	21.8.25.X	|	Unsupported
CW1 is upgraded to imediately prior version after grace period end	|					|	2022-04-15	|	21.11.27.X	|	Immediately Prior Version
CW1 is upgraded to current version after grace period end			|					|	2022-04-30	|	22.2.17.X	|	Current Version
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db10 = BillingTestHelper.CreateLicence(Factory, "D10", false).Database;
			db10.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db10.LD_HL_CurrentRunningVersion = v21_5_19_0.PK;
			db10.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 7);
			db10.LD_HL_CurrentRunningVersion = v21_8_25_0.PK;
			db10.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 7);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 15);
			db10.LD_HL_CurrentRunningVersion = v21_11_27_0.PK;
			db10.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 15);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 30);
			db10.LD_HL_CurrentRunningVersion = v22_2_17_0.PK;
			db10.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 30);
			Factory.Save();

			AssertEquals(0m, versionHistory.GetVersionSurchargePercent(db10, null, new DateTime(2022, 4, 1)));

			/* Case 11
																|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on unsupported version at start of billing period		|	Apr-22			|	2022-04-01	|	21.8.25.X	|	Unsupported
CW1 is on unsupported version on the last day of billing period	|					|	2022-04-30	|	21.8.25.X	|	Unsupported
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db11 = BillingTestHelper.CreateLicence(Factory, "D11", false).Database;
			db11.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db11.LD_HL_CurrentRunningVersion = v21_8_25_0.PK;
			db11.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			AssertEquals(2m, versionHistory.GetVersionSurchargePercent(db11, null, new DateTime(2022, 4, 1)));

			/* Case 12
																|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on unsupported+1 version at start of billing period		|	Apr-22			|	2022-04-01	|	21.5.19.X	|	Unsupported+1
CW1 is upgraded to unsupported version after grace period end	|					|	2022-04-07	|	21.8.25.X	|	Unsupported
CW1 is on unsupported version on the last day of billing period	|					|	2022-04-30	|	21.8.25.X	|	Unsupported
			 */
			TestDateAttribute.Date = new DateTime(2022, 4, 1);
			var db12 = BillingTestHelper.CreateLicence(Factory, "D12", false).Database;
			db12.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db12.LD_HL_CurrentRunningVersion = v21_5_19_0.PK;
			db12.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 30);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 4, 07);
			db12.LD_HL_CurrentRunningVersion = v21_8_25_0.PK;
			db12.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 4, 07);
			Factory.Save();

			AssertEquals(2m, versionHistory.GetVersionSurchargePercent(db12, null, new DateTime(2022, 4, 1)));

			/* Case 13
																		|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on Immediately Prior Version at start of billing period			|	Mar-22			|	2022-03-01	|	21.8.25.X	|	Immediately Prior Version
New version 22.2.17.X is released, CW1 is now on an unsupported version	|					|	2022-03-30	|	21.8.25.X	|	Unsupported
Still in grace period of the current version, should be no surcharge	|					|	2022-03-31	|	21.8.25.X	|	Unsupported
			 */

			TestDateAttribute.Date = new DateTime(2022, 3, 1);
			var db13 = BillingTestHelper.CreateLicence(Factory, "D13", false).Database;
			db13.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db13.LD_HL_CurrentRunningVersion = v21_8_25_0.PK;
			db13.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 1);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 3, 30);
			v22_2_17_0.HL_ExeVersionDate = new DateTime(2022, 3, 30);
			Factory.Save();
			versionHistory = new VersionHistory();

			AssertEquals(0m, versionHistory.GetVersionSurchargePercent(db13, null, new DateTime(2022, 3, 1)));

			/* Case 14
																		|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on Immediately Prior Version at start of billing period			|	Mar-22			|	2022-03-01	|	21.8.25.X	|	Immediately Prior Version
New version 22.2.17.X is released, CW1 is now on an unsupported version	|					|	2022-03-01	|	21.8.25.X	|	Unsupported
30 day grace period ends												|					|	2022-03-30	|	21.8.25.X	|	Unsupported
CW1 is on unsupported version on the last day of billing period and grace period ended		|	2022-03-31	|	21.8.25.X	|	Unsupported
			 */
			TestDateAttribute.Date = new DateTime(2022, 3, 1);
			var db14 = BillingTestHelper.CreateLicence(Factory, "D14", false).Database;
			db14.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db14.LD_HL_CurrentRunningVersion = v21_8_25_0.PK;
			db14.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 1);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 3, 1);
			v22_2_17_0.HL_ExeVersionDate = new DateTime(2022, 3, 1);
			Factory.Save();
			versionHistory = new VersionHistory();

			AssertEquals(2m, versionHistory.GetVersionSurchargePercent(db14, null, new DateTime(2022, 3, 1)));

			/* Case 15
																			|	Billing Period	|	Date		|	CW1 Version	|	Version Support Status
CW1 is on unsupported version at start of billing period					|	Mar-22			|	2022-03-01	|	21.8.25.X	|	Unsupported
New version 22.3.17.X is released, CW1 is now on an unsupported+1 version	|					|	2022-03-30	|	21.8.25.X	|	Unsupported
CW1 is on unsupported+1 version on the last day of billing period			|					|	2022-03-31	|	21.8.25.X	|	Unsupported+1
			 */
			TestDateAttribute.Date = new DateTime(2022, 3, 1);
			var db15 = BillingTestHelper.CreateLicence(Factory, "D15", false).Database;
			db15.LD_ReleaseRing = releaseRing;
			Factory.Save();
			db15.LD_HL_CurrentRunningVersion = v21_8_25_0.PK;
			db15.LD_CurrentVersionFirstReportUtc = new ZDateTime(2022, 03, 1);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 3, 30);
			var v22_3_17_0 = CreateReleaseBuild(22, 3, 17, 0, new DateTime(2022, 03, 30), releaseRing);
			Factory.Save();
			versionHistory = new VersionHistory();

			AssertEquals(3m, versionHistory.GetVersionSurchargePercent(db15, null, new DateTime(2022, 3, 1)));
		}

		ReleaseBuild CreateReleaseBuild(int major, int minor, int release, int patch, DateTime date, string ring, string product = "ENT")
			=> CreateReleaseBuild(Factory, major, minor, release, patch, date, ring, product);

		internal static ReleaseBuild CreateReleaseBuild(BusinessObjectFactory factory, int major, int minor, int release, int patch, DateTime date, string ring, string product = "ENT")
		{
			var build = factory.New<ReleaseBuild>();
			build.HL_MajorVersion = major;
			build.HL_MinorVersion = minor;
			build.HL_Release = release;
			build.HL_Patch = patch;
			build.HL_ExeVersionDate = date;
			build.HL_ReleaseStatus = ring;
			build.HL_Product = product;
			return build;
		}

		internal static void CreateBuildsAndSetDatabaseToNonCurrentVersion(LicenceDatabase db, ZDateTime periodStart, int oldVersionsCount, bool isTransitToCWN = false)
		{
			var factory = db.Factory;
			var baseVersionDate = new DateTime(2001, 1, 1);
			var baseVersion = CreateReleaseBuild(factory, baseVersionDate.Year % 100, baseVersionDate.Month, baseVersionDate.Day, 0, baseVersionDate.AddDays(15), db.LD_ReleaseRing);
			var currentVersionDate = periodStart.AddMonths(-2).AddDays(5).ToDateTime();
			var currentVersion = CreateReleaseBuild(factory, currentVersionDate.Year % 100, currentVersionDate.Month, currentVersionDate.Day, 500, currentVersionDate.AddDays(15), db.LD_ReleaseRing);

			if (isTransitToCWN)
			{
				baseVersion.HL_Product = ProductTypes.Codes.CargoWiseNext;
				currentVersion.HL_Product = ProductTypes.Codes.CargoWiseNext;
			}

			factory.Save();

			for (var i = 1; i <= oldVersionsCount; i++)
			{
				var oldVersionDate = currentVersionDate.AddMonths(-i);
				var oldVersion = CreateReleaseBuild(factory, oldVersionDate.Year % 100, oldVersionDate.Month, oldVersionDate.Day, 500, oldVersionDate.AddDays(15), db.LD_ReleaseRing);

				if (i == oldVersionsCount)
				{
					db.LD_HL_CurrentRunningVersion = baseVersion.PK;
					db.LD_CurrentVersionFirstReportUtc = baseVersionDate.AddDays(2);
					TestDateAttribute.Date = db.LD_CurrentVersionFirstReportUtc.ToDateTime();
					factory.Save();

					db.LD_HL_CurrentRunningVersion = oldVersion.PK;
					db.LD_CurrentVersionFirstReportUtc = periodStart.AddDays(2);
					TestDateAttribute.Date = db.LD_CurrentVersionFirstReportUtc.ToDateTime();
					factory.Save();
				}
			}
		}
	}
}
