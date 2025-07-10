using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbHealth.Check
{
	[TestedType(typeof(PhysicalFileSizeChecker))]
	sealed class PhysicalFileCheckerTest : CheckerTestCaseBase
	{
		public void TestCheckDesktopExpressDataSize()
		{
			PhysicalFileCheckerForTesting testChecker = new PhysicalFileCheckerForTesting();

			testChecker.SqlExpressDataPagesThresholdOverride = 0;
			testChecker.IsDesktopOrExpressSqlServerEditionOverride = true;
			DbHealthWarningList testWarningList1 = new DbHealthWarningList();
			testChecker.CheckDesktopExpressDataSize_Exposed(testWarningList1);

			AssertEquals("Warings", 1, testWarningList1.Count);
			AssertEquals("Source Type", DatabaseWarning.DatabaseSourceType, testWarningList1[0].SourceType);
			AssertEquals("Source", Db.DatabaseName, testWarningList1[0].Source);
			string expectedDescription = "Database size is getting close to the MS SQL Server Express Edition limit.";
			AssertEquals("Description", expectedDescription, testWarningList1[0].Description);
			AssertStartsWith("Action\r\n" + testWarningList1[0].Action, "Upgrade to Standard/Enterprise SQL Server licence ASAP.", testWarningList1[0].Action);

			testChecker.SqlExpressDataPagesThresholdOverride = int.MaxValue;
			testChecker.IsDesktopOrExpressSqlServerEditionOverride = true;
			DbHealthWarningList testWarningList2 = new DbHealthWarningList();
			testChecker.CheckDesktopExpressDataSize_Exposed(testWarningList2);

			AssertEquals("Should have NO warnings", 0, testWarningList2.Count);
		}

		public void TestCheckDiskSpace()
		{
			var dbNameDifferentCollation = Db.DatabaseName + "_RefDb_XXX_YY";
			AdoTestUtils.CreateDbDropExisting(dbNameDifferentCollation);
			var dbList = new string[] { Db.DatabaseName, dbNameDifferentCollation };

			try
			{
				using (DbConnection adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery($"ALTER DATABASE [{dbNameDifferentCollation}] COLLATE Albanian_CI_AI");
					adminConnection.BeginTransaction();

					PhysicalFileCheckerForTesting testChecker = new PhysicalFileCheckerForTesting();

					testChecker.FreeSpaceUsedSpaceMinRatioOverride = int.MaxValue;
					DbHealthWarningList testWarningList1 = new DbHealthWarningList();

					testChecker.CheckDiscSpace_Exposed(adminConnection, dbList, testWarningList1);

					AssertEquals("Should have at least 1 waring", true, testWarningList1.Count > 0);
					AssertEquals("Source Type", DiskWarning.DiskSourceType, testWarningList1[0].SourceType);

					AssertStartsWith(
						"Description start NOT as expected. Actual description:\r\n" + testWarningList1[0].Description,
						"The total free space on the disk",
						testWarningList1[0].Description);
					AssertContains(
						"Description should contain database name, but Actual description was:\r\n" + testWarningList1[0].Description,
						$"DB: {Db.DatabaseName}",
						testWarningList1[0].Description);
					AssertEquals(
						"Action",
						"Try freeing space on the existing disk or RAID or replace/upgrade the drive by a larger unit, RAID or storage unit.", testWarningList1[0].Action);

					testChecker.FreeSpaceUsedSpaceMinRatioOverride = 0;
					DbHealthWarningList testWarningList2 = new DbHealthWarningList();
					testChecker.CheckDiscSpace_Exposed(adminConnection, dbList, testWarningList2);

					AssertEquals("Should have NO warnings", 0, testWarningList2.Count);
				}
			}
			catch (Exception ex)
			{
				Fail(ex.ToString());
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(Db.NewAdminConnection(), dbNameDifferentCollation);
			}
		}

		public void TestGetDiskFreeSpace()
		{
			PhysicalFileCheckerForTesting testChecker = new PhysicalFileCheckerForTesting();

			string driveLetter = System.Environment.SystemDirectory.Substring(0, 1);
			using (DbConnection adminConnection = Db.NewAdminConnection())
			{
				adminConnection.BeginTransaction();
				long freeSpaceInMb = testChecker.GetDiskFreeSpaceInMb_Exposed(driveLetter, adminConnection);
				AssertEquals("Space Free should be positive, but was " + freeSpaceInMb.ToString(), true, freeSpaceInMb >= 0);
			}
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new PhysicalFileSizeChecker();
		}
	}
}
