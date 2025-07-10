using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ChangeDataCapture.Service
{
	public class CaptureTaskQueueTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestCdcServiceTaskBacklog()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				IHostedServiceQueueProvider provider = new CaptureTaskQueue();
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var captureTask = new CaptureTaskForTesting();

				string sqlText = @"
					CREATE TABLE dbo.CdcBacklogTest (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CdcBacklogTest',
						@role_name = cdc_admin;
				";
				testConnection.ExecuteNonQuery(sqlText);

				sqlText = "INSERT dbo.CdcBacklogTest VALUES (34); INSERT dbo.CdcBacklogTest VALUES(44);";
				testConnection.ExecuteNonQuery(sqlText);

				captureTask.CaptureChangesCore_Exposed();
				AssertEquals(provider.QueueResult, QueueResult.Zero);

				sqlText = @"INSERT dbo.CdcBacklogTest VALUES (314); INSERT dbo.CdcBacklogTest VALUES(42);";
				testConnection.ExecuteNonQuery(sqlText);
				AssertEquals(provider.QueueResult.QueueSize, 2);
				AssertEquals(provider.QueueResult.MaximumItemAge, TimeSpan.Zero);

				sqlText = @"INSERT dbo.CdcBacklogTest VALUES (94); INSERT dbo.CdcBacklogTest VALUES(99);";
				testConnection.ExecuteNonQuery(sqlText);
				AssertEquals(provider.QueueResult.QueueSize, 4);

				captureTask.CaptureChangesCore_Exposed();
				AssertEquals(provider.QueueResult, QueueResult.Zero);
			}
		}

		class CaptureTaskForTesting : CaptureTask
		{
			public long CaptureChangesCore_Exposed()
			{
				return new OnlineCdcScanner(new LoggerForTest()).ScanUntilNoTransactionsToProcess();
			}
		}
	}
}
