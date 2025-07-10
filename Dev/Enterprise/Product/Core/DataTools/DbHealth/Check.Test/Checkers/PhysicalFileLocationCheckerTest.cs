using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DbHealth.Check
{
	[TestedType(typeof(PhysicalFileLocationChecker))]
	[UseSnapshotProtection]
	sealed class PhysicalFileLocationCheckerTest : CheckerTestCaseBase
	{
		public void TestCheckDataAndLogFileLocation()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				var bakReg = settings.SuppressDbFilesHealthCheckNotificationsForHostedSystems;
				var bakIsHosted = EnvProxy.HostedLocation;

				settings.SuppressDbFilesHealthCheckNotificationsForHostedSystems = true;
				EnvProxy.SetHostedLocationForTest("SYD");

				try
				{
					var testChecker = new PhysicalFileLocationChecker();
					var testWarningList = new DbHealthWarningList();
					testChecker.Check(Db.Connection, testWarningList, null);
					AssertEquals("Waring count", 0, testWarningList.Count);
				}
				finally
				{
					settings.SuppressDbFilesHealthCheckNotificationsForHostedSystems = bakReg;
					EnvProxy.SetHostedLocationForTest(bakIsHosted);
				}
			}

			using (var settings = TestEntityFrameworkSettings.Get())
			{
				var bakReg = settings.SuppressDbFilesHealthCheckNotificationsForHostedSystems;
				settings.SuppressDbFilesHealthCheckNotificationsForHostedSystems = false;

				try
				{
					var testChecker = new PhysicalFileLocationChecker();
					var testWarningList = new DbHealthWarningList();
					testChecker.Check(Db.Connection, testWarningList, null);
					CheckDataAndLogFileLocation(testWarningList);
				}
				finally
				{
					settings.SuppressDbFilesHealthCheckNotificationsForHostedSystems = bakReg;
				}
			}

			{
				var bakIsHosted = EnvProxy.HostedLocation;
				EnvProxy.SetHostedLocationForTest("");

				try
				{
					var testChecker = new PhysicalFileLocationChecker();
					var testWarningList = new DbHealthWarningList();
					testChecker.Check(Db.Connection, testWarningList, null);
					CheckDataAndLogFileLocation(testWarningList);
				}
				finally
				{
					EnvProxy.SetHostedLocationForTest(bakIsHosted);
				}
			}
		}

		void CheckDataAndLogFileLocation(DbHealthWarningList testWarningList)
		{
			List<string> expectedDbsWithWarning = new List<string>();

			string sqlText = String.Format("SELECT count(distinct(left(physical_name, 2))) FROM {0}.sys.database_files", Db.DatabaseName);
			int driveCount = (int)Db.Connection.ExecuteScalar(sqlText);

			if (driveCount == 1)
			{
				AssertEquals("Warning count > 0?", true, testWarningList.Count > 0);
				AssertEquals("Source Type", DatabaseWarning.DatabaseSourceType, testWarningList[0].SourceType);
				AssertEquals("Source", Db.DatabaseName, testWarningList[0].Source);
				AssertStartsWith("Description\r\n" + testWarningList[0].Description,
					$"The database [{Db.DatabaseName}] has data and log files located in the same disk volume.",
					testWarningList[0].Description);
				AssertStartsWith("Action\r\n" + testWarningList[0].Action,
					"Move the database files so data and log are located in 2 separate physical disks or RAIDs.",
					testWarningList[0].Action);
			}
			else
			{
				AssertEquals("Warning count", 0, testWarningList.Count);
			}
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new PhysicalFileLocationChecker();
		}
	}
}
