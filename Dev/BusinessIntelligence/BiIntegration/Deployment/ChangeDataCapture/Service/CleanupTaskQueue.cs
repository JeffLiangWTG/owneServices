using System;
using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	Enterprise.ChangeDataCapture.Service.CleanupTask.ServiceTaskCode,
	Enterprise.ChangeDataCapture.Service.CleanupTask.ServiceTaskName,
	typeof(Enterprise.ChangeDataCapture.Service.CleanupTaskQueue))]

namespace Enterprise.ChangeDataCapture.Service
{
	public class CleanupTaskQueue : IHostedServiceQueueProvider
	{
		public CleanupTaskQueue()
		{
		}

		QueueResult IHostedServiceQueueProvider.QueueResult
		{
			get
			{
				try
				{
					return GetCdnServiceTaskBacklog();
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName)
				{
					return QueueResult.Error;
				}
			}
		}

		protected QueueResult GetCdnServiceTaskBacklog()
		{
			if (CdcDatabase.IsEnabled(Db.Connection, Db.DatabaseName))
			{
				var sqlText = @"SELECT Count(*), ISNULL(MAX(DATEDIFF(second, tran_end_time, GetDate())), 0) FROM [cdc].[lsn_time_mapping] where tran_end_time < dateadd(day, -1, getdate())"; 
#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
				using (var cmd = Db.Connection.Command(sqlText))
				{
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var count = reader.GetInt32(0);
							var age = TimeSpan.FromSeconds(reader.GetInt32(1));
							return  new QueueResult(count, age);
						}
					}
				}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
			}
			return QueueResult.Zero;
		}
	}
}
