using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ChangeDataCapture.Service.Testing
{
	class CleanupTaskQueueTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestCdnServiceTaskBacklog()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var cleanupTask = new CleanupTask();
				var cleanupTaskQueue = new CleanupTaskQueue();
				IHostedServiceQueueProvider provider = new CleanupTaskQueue();
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new OnlineCdcScanner(new LoggerForTest());

				string sqlText = @"INSERT INTO cdc.lsn_time_mapping (start_lsn, tran_end_time)
						VALUES (0x00002000000000000003, DATEADD(DAY, -60, GETDATE()))";
				testConnection.Command(sqlText).ExecuteNonQuery();

				int testCdnBacklogAge = 60 - (int)cleanupTask.CDCRetentionPeriod.TotalDays;
				AssertNotNull(provider.QueueResult.QueueSize);
				AssertNotNull(provider.QueueResult.MaximumItemAge);
				NUnit.Framework.Assert.That((int)Math.Round(provider.QueueResult.MaximumItemAge.TotalDays), NUnit.Framework.Is.EqualTo(testCdnBacklogAge).Within(1));

				sqlText = @"INSERT INTO cdc.lsn_time_mapping (start_lsn, tran_end_time)
						VALUES (0x00000000000200000003, DATEADD(DAY, -90, GETDATE()))";
				testConnection.Command(sqlText).ExecuteNonQuery();

				testCdnBacklogAge = 90 - (int)cleanupTask.CDCRetentionPeriod.TotalDays;
				AssertNotNull(provider.QueueResult.QueueSize);
				testCdnBacklogAge = 90;
				var result = provider.QueueResult;
				AssertNotNull(result.QueueSize);
				AssertNotNull(result.MaximumItemAge);
				NUnit.Framework.Assert.That((int)Math.Round(provider.QueueResult.MaximumItemAge.TotalDays), NUnit.Framework.Is.EqualTo(testCdnBacklogAge).Within(1));

				sqlText = @"DELETE FROM cdc.lsn_time_mapping";
				testConnection.Command(sqlText).ExecuteNonQuery();
				AssertEquals(0, provider.QueueResult.QueueSize);
				AssertEquals(TimeSpan.Zero, provider.QueueResult.MaximumItemAge);
			}
		}
	}
}
