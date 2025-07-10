using System;
using System.Threading;
using CargoWise.Data;
using Enterprise.DocumentScanning.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(EDocsDeleteContentFromExternalStorageProcessingTask.ServiceTaskCode,
	EDocsDeleteContentFromExternalStorageProcessingTask.ServiceTaskDescription,
	"DOC",
	typeof(EDocsDeleteContentFromExternalStorageProcessingTask),
	MinimumPeriod = "1day",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true
	)]
[assembly: HostedServiceQueueProvider(
	EDocsDeleteContentFromExternalStorageProcessingTask.ServiceTaskCode,
	nameof(EDocsDeleteContentFromExternalStorageProcessingTask),
	typeof(EDocsDeleteContentFromExternalStorageProcessingTaskQueue))]
[assembly: HostedServiceBusinessObjectBinding(EDocsDeleteContentFromExternalStorageProcessingTask.ServiceTaskCode, StorageDocsToDeleteSchema.Constants.TableName, new string[] { }, null)]

namespace Enterprise.DocumentScanning.ServiceTasks
{
	public class EDocsDeleteContentFromExternalStorageProcessingTaskQueue : IHostedServiceQueueProvider
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
						var query = $@"SELECT COUNT(*) FROM dbo.{StorageDocsToDeleteSchema.Constants.TableName}";
						Db.Connection.ExecuteReader(query, reader => result = new QueueResult(reader.GetInt32(0), TimeSpan.Zero));
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

	public class EDocsDeleteContentFromExternalStorageProcessingTask : BaseEDocsContentExternalStorageTask
	{
		public const string ServiceTaskCode = "DED";
		public const string ServiceTaskDescription = "Delete From External Storage Processing Task";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string message to show in service task logs.")]
		public const string ServiceNotRequiredMessage = "This service task is not required to run as it's hosted by WiseCloud.";

		protected override void RunTaskCore(CancellationToken token)
		{
			if (!CanAccessExternalStorage)
			{
				ServiceLogger.Error(ServiceStoppedNoAccessToExternalStorageMessage);
				return;
			}

			processor ??= new EDocsDeleteContentFromExternalStorageProcessor();
			processor.Run(ServiceLogger, token);
		}

		EDocsDeleteContentFromExternalStorageProcessor processor;

		[HostedServiceRequirement]
		public static string CheckEDocsStorageProvider() => HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(SystemDataRegistry.Instance.EDocsStorageProvider, Core.Constants.EDocsStorageProviders.Code.DB);

		[HostedServiceRequirement]
		public static string CheckIsNotHostedWithCargowise() => !EnvProxy.IsHostedWithCargowise ? string.Empty : ServiceNotRequiredMessage;
	}
}
