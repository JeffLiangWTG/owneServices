using System;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Types;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.AuditDataServices.Notification.AuditSubscriberProcessorTask.ServiceTaskCode,
	Enterprise.AuditDataServices.Notification.AuditSubscriberProcessorTask.ServiceTaskDescription,
	typeof(Enterprise.AuditDataServices.Notification.AuditSubscriberProcessorTaskQueue))]

namespace Enterprise.AuditDataServices.Notification
{
	// This is the Queue for the ASP task, not the AuditSubscriberTasks
	public class AuditSubscriberProcessorTaskQueue : IHostedServiceQueueProvider
	{
		public QueueResult QueueResult => GetQueueResult();

		protected string BiServer => BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);

		QueueResult GetQueueResult()
		{
			if (!string.IsNullOrWhiteSpace(BiServer))
			{
				using (var connection = Db.NewExtraConnectionWithMainDbCredentials(BiServer, Db.AuditDatabaseName))
				{
					var lastMaintenanceTime = BiMasterState.GetParameterDate(connection, BiConstants.LastIndexRebuildUtcDt);
					if (lastMaintenanceTime.HasValue)
					{
						var timeSinceLastMaintenance = ZDateTime.UtcNow - lastMaintenanceTime.Value;
						var overdueTime = timeSinceLastMaintenance - AuditSubscriberProcessorTask.MaintenanceRunMinimumInterval;
						var isOverdue = overdueTime > TimeSpan.Zero;
						overdueTime = isOverdue ? overdueTime : TimeSpan.Zero;
						return new QueueResult(isOverdue ? 1 : 0, overdueTime);
					}
				}
			}
			return QueueResult.Zero;
		}
	}
}
