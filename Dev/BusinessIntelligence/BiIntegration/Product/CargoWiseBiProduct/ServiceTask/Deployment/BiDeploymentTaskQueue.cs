using System;
using CargoWise.Bi.Common;
using CargoWise.Data;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(
	CargoWise.Bi.Product.ServiceTask.BiDeploymentTask.ServiceTaskCode,
	CargoWise.Bi.Product.ServiceTask.BiDeploymentTask.ServiceTaskName,
	typeof(CargoWise.Bi.Product.ServiceTask.BiDeploymentTaskQueue))]

namespace CargoWise.Bi.Product.ServiceTask
{
	public class BiDeploymentTaskQueue : IHostedServiceQueueProvider
	{
		QueueResult IHostedServiceQueueProvider.QueueResult
		{
			get
			{
				try
				{
					return GetBidServiceTaskBacklog();
				}
				catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName)
				{
					return QueueResult.Error;
				}
			}
		}

		protected QueueResult GetBidServiceTaskBacklog()
		{
			var result = QueueResult.Error;
			var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
			if (!string.IsNullOrEmpty(dwServer))
			{
				using (var biConnection = Db.NewExtraConnectionWithMainDbCredentials(dwServer, Db.SqlMasterDb))
				{
					if (biConnection.DatabaseExists(Db.EdwDatabaseName))
					{
						using (((ICurrentDbControl)biConnection).UseDatabase(Db.EdwDatabaseName))
						{
							var sqlText = $"SELECT COUNT(*), ISNULL(MAX(DATEDIFF(second, CreateDate, GetUTCDate())), 0) FROM [{BiConstants.BiAdminSchemaName}].SsasPartitionUnprocessedDate with (nolock)";
							biConnection.ExecuteReader(sqlText, reader =>
							{
								result = new QueueResult(reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1)));
							});
						}
					}
				}
			}

			return result;
		}
	}
}
