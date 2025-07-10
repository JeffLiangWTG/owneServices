using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.DbHealth.Check.Checkers;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check.Test.Checkers
{
	[TestedType(typeof(CdcLatencyChecker))]
	sealed class CdcLatencyCheckerTest : CheckerTestCaseBase
	{
		[UseSnapshotProtection]
		public void TestCheckCdcLatencyIfCdcDisabled()
		{
			using (var conn = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(conn, Db.DatabaseName))
				{
					CdcDatabase.Disable(conn, Db.DatabaseName);
				}

				var checker = GetNewCheckerInstance();
				var logger = new TestServiceLogger();
				var warningList = new DbHealthWarningList();

				checker.Check(conn, warningList, logger);

				AssertEquals("Warning count", 0, warningList.Count);
			}
		}

		[UseSnapshotProtection]
		public void TestCheckCdcLatencyIfCdcDataIsEmpty()
		{
			using (var conn = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(conn, Db.DatabaseName))
				{
					CdcDatabase.Disable(conn, Db.DatabaseName);
				}
				CdcDatabase.Enable(conn, Db.DatabaseName);

				var checker = GetNewCheckerInstance();
				var logger = new TestServiceLogger();
				var warningList = new DbHealthWarningList();

				checker.Check(conn, warningList, logger);

				AssertEquals("Warning count", 1, warningList.Count);
				AssertEquals("Warning description", "Database does not contain any change data capture records.", warningList[0].Description);
				AssertEquals("Warning action", "Check if CDC scan service task is running.", warningList[0].Action);
			}
		}

		[UseSnapshotProtection]
		public void TestCheckCdcLatencyIfCdcDataIsOld()
		{
			using (var conn = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(conn, Db.DatabaseName))
				{
					CdcDatabase.Disable(conn, Db.DatabaseName);
				}
				CdcDatabase.Enable(conn, Db.DatabaseName);
				RunCdcScan(conn);
				Thread.Sleep(TimeSpan.FromSeconds(1)); // Add delay to prevent intermittent failures WI00845931

				var checker = new CdcLatencyCheckerForTest(TimeSpan.Zero);
				var logger = new TestServiceLogger();
				var warningList = new DbHealthWarningList();

				checker.Check(conn, warningList, logger);

				AssertEquals("Warning count", 1, warningList.Count);
				AssertEquals("Warning description", "Database has not scanned any change data capture records for more than 2 days.", warningList[0].Description);
				AssertEquals("Warning action", "Check if CDC scan service task is running.", warningList[0].Action);
			}
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new CdcLatencyChecker();
		}

		void RunCdcScan(DbConnection conn)
		{
			using (var cmd = conn.Command("sys.sp_cdc_scan"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@continuous", SqlDbType.Bit, 0);
				cmd.ExecuteNonQuery();
			}
		}

		class CdcLatencyCheckerForTest : CdcLatencyChecker
		{
			public CdcLatencyCheckerForTest(TimeSpan cdcLatencyOffset)
			{
				this.cdcLatencyOffset = cdcLatencyOffset;
			}
			readonly TimeSpan cdcLatencyOffset;

			protected override TimeSpan AcceptableCdcLatencyOffset => cdcLatencyOffset;
		}
	}
}
