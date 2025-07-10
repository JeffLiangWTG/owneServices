using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using Enterprise.DocumentScanning.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(EDocsRegularContentToExternalStorageProcessingTask.ServiceTaskCode,
	EDocsRegularContentToExternalStorageProcessingTask.ServiceTaskDescription,
	"DOC",
	typeof(EDocsRegularContentToExternalStorageProcessingTask),
	MinimumPeriod = "10minutes",
	MaximumPeriod = "1day",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "10minutes",
	ActiveByDefault = true)]
[assembly: HostedServiceQueueProvider(EDocsRegularContentToExternalStorageProcessingTask.ServiceTaskCode, "eDocs External Storage", typeof(EDocsExternalStorageDataQueue))]

namespace Enterprise.DocumentScanning.ServiceTasks
{
	public class EDocsExternalStorageDataQueue : IHostedServiceQueueProvider
	{
		QueueResult IHostedServiceQueueProvider.QueueResult
		{
			get
			{
				if (SystemDataRegistry.Instance.EDocsStorageProvider.Value != Core.Constants.EDocsStorageProviders.Code.DB)
				{
					try
					{
						var result = QueueResult.Error;
						var storageDays = SystemDataRegistry.Instance.EDocsDBMinimumStorageDays.Value;
						var query = $@"SELECT COUNT(*), ISNULL(MAX(DATEDIFF(second, SC_Date, GETUTCDATE() - {storageDays})), 0) FROM {Db.DatabaseName}_SD001.dbo.StorageDocs WHERE SC_ImageDataHasValue = 1";
						if (storageDays > 0)
						{
							query += $" AND SC_Date < GETUTCDATE() - {storageDays}";
						}
						Db.Connection.ExecuteReader(query, reader => result = new QueueResult(reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1))));
						return result;
					}
					catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName)
					{
						return QueueResult.Error;
					}
				}
				else
				{
					return QueueResult.Zero;
				}
			}
		}
	}

	public class EDocsRegularContentToExternalStorageProcessingTask : BaseEDocsContentExternalStorageTask
	{
		public const string ServiceTaskCode = "DER";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string for the task description.")]
		public const string ServiceTaskDescription = "Regular External Storage Processing Task";

		[HostedServiceRequirement]
		public static string CheckEDocsStorageProvider() => HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(SystemDataRegistry.Instance.EDocsStorageProvider, Core.Constants.EDocsStorageProviders.Code.DB);

		protected override void RunTaskCore(CancellationToken token)
		{
			if (!CanAccessExternalStorage)
			{
				ServiceLogger.Error(ServiceStoppedNoAccessToExternalStorageMessage);
				return;
			}

			var branch = GlbBranch.GetOneActiveBranchPerCompany().FirstOrDefault();

			if (branch != null)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					new EDocsRegularContentToExternalStorageProcessor().Run(ServiceLogger, token);
				}
			}
		}
	}
}
