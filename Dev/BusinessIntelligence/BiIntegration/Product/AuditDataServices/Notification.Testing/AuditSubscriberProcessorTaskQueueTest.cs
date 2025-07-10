using System;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Bi.Common.Testing;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Notification.Testing
{
	class AuditSubscriberProcessorTaskQueueTest : TestCase
	{
		public void TestGetASPQueueNullServer()
		{
			using (BiServers.TemporarilySetAuditServerToNull())
			{
				var provider = new AuditSubscriberProcessorTaskQueue();
				AssertEquals("AuditSubscriberTaskForTest service task queue size should be 0 if BiServer is null.", 0, provider.QueueResult.QueueSize);
				AssertEquals("AuditSubscriberTaskForTest service task queue age should be 0 if BiServer is null.", TimeSpan.Zero, provider.QueueResult.MaximumItemAge);
			}
		}

		public void TestGetASPQueueNotOverdue()
		{
			var lastAuditMaintenance = DateTime.UtcNow.AddHours(-3).ToString(CultureInfo.InvariantCulture);

			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(connection, BiConstants.LastIndexRebuildUtcDt, lastAuditMaintenance))
			{
				var provider = new AuditSubscriberProcessorTaskQueue();
				AssertEquals("AuditSubscriberTaskForTest service task queue size.", 0, provider.QueueResult.QueueSize);
				AssertEquals(TimeSpan.Zero, provider.QueueResult.MaximumItemAge);
			}
		}

		public void TestGetASPQueueOverdue()
		{
			var lastAuditMaintenance = DateTime.UtcNow.Add(-AuditSubscriberProcessorTask.MaintenanceRunMinimumInterval).AddHours(-3).ToString(CultureInfo.InvariantCulture);

			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			using (BiTemporaryMasterState.SetParameterTemporaryValue(connection, BiConstants.LastIndexRebuildUtcDt, lastAuditMaintenance))
			{
				var provider = new AuditSubscriberProcessorTaskQueue();
				AssertEquals("AuditSubscriberTaskForTest service task queue size.", 1, provider.QueueResult.QueueSize);
				AssertEquals(TimeSpan.FromHours(3).TotalSeconds, provider.QueueResult.MaximumItemAge.TotalSeconds, 60);
			}
		}
	}
}
