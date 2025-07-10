using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.UniversalDataBuss.ServiceTasks
{
	public abstract class StmQueueStateQueueProvider : IHostedServiceQueueProvider
	{
		readonly string status;
		protected StmQueueStateQueueProvider(string status)
		{
			Argument.NotNullOrEmpty(status, nameof(status));
			this.status = status;
		}

		public QueueResult QueueResult {
			get
			{
				var result = QueueResult.Error;
				var sqlText = string.Format(CultureInfo.InvariantCulture,
				@"
				SELECT COUNT(*),
				ISNULL(MAX(DATEDIFF(second, SQS_SystemCreateTimeUtc, GETUTCDATE())), 0)
				FROM dbo.StmQueueState
				WHERE SQS_Status = @status");

#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
				using (var cmd = Db.Connection.Command(sqlText))
				{
					cmd.AddParameterBasedOnDbColumn((NoResString)"@status", status, StmQueueStateSchema.SQS_Status);
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var count = reader.GetInt32(0);
							var age = TimeSpan.FromSeconds(reader.GetInt32(1));
							return new QueueResult(count, age);
						}
					}
				}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods
				return result;
			}
		}
	}
}
