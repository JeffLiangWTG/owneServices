using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.DocumentScanning.ServiceTasks
{
	abstract class BaseEDocsContentToExternalStorageProcessor
	{
		protected BaseEDocsContentToExternalStorageProcessor(int batchSize = 100)
		{
			BatchSize = batchSize;
		}

		public abstract void Run(ILogger serviceLogger, CancellationToken token);

		protected IExternalPersister ExternalPersister
		{
			get
			{
				if (externalPersister == null || externalPersisterType != SystemDataRegistry.Instance.EDocsStorageProvider.Value)
				{
					externalPersisterType = SystemDataRegistry.Instance.EDocsStorageProvider.Value;
					externalPersister = ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister(externalPersisterType);
				}

				return externalPersister;
			}
		}

		protected void HandleSaveToExternalStorageErrors(Exception ex, StorageDocsBase storageDoc, ILogger serviceLogger)
		{
			if (ex is ExternalStorageException)
			{
				throw new HostedServiceException(ex.Message, ex);
			}

			var errorMessage = FormattableString.Invariant($"Failed to move eDocs to external storage with last StorageDocs PK: {storageDoc.PK}, Name: {storageDoc.SC_FileNameWithExtension}");
			serviceLogger.Error(errorMessage, ex);
			ErrorReporter.ReportOnce("StorageDocs_SaveToExternalStorage_Error", errorMessage, ex);
		}

		protected virtual bool Validate(ILogger logger)
		{
			if (SystemDataRegistry.Instance.EDocsStorageProvider.Value != Core.Constants.EDocsStorageProviders.Code.S3)
			{
				logger.Error((NoResString)"S3 Storage is not enabled, the process will be stopped.");
				return false;
			}

			if (Db.Connection.GetDatabases(DatabaseType.SD).Any(db => !DocManagerUtils.IsDbWriteableForDocManager(db)))
			{
				logger.Error(DocumentDBReadOnlyMessage);
				UnattendedUserNotification.Instance.ShowErrorOnceADay(ServiceFailedKey, DocumentDBReadOnlyMessage, FailToExecuteNotificationSubject, sendToPostMaster: true);
				return false;
			}

			return true;
		}

		protected abstract string ServiceFailedKey { get; }

		protected abstract string FailToExecuteNotificationSubject { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string message to show in service task logs.")]
		public const string DocumentDBReadOnlyMessage = "Process cannot start as some of the eDocs databases are read-only. Please ensure all eDocs databases are writable.";

		protected readonly int BatchSize;
		IExternalPersister externalPersister;
		string externalPersisterType;
	}
}
