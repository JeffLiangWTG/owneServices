using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Utils;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.DocumentScanning.ServiceTasks
{
	public abstract class BaseEDocsContentExternalStorageTask : ServiceProviderImpl
	{
		internal static readonly string S3ModificationLockName = "S3Modification";

		public override void RunTask(CancellationToken token)
		{
			var lockResult = Db.Connection.RunLocked(S3ModificationLockName, _ => RunTaskCore(token), lockMode: SqlApplicationLockMode.Shared);
			if (lockResult != LockedProcessResult.Completed)
			{
				ServiceLogger.Debug((NoResString)"Could not acquire shared S3 lock, skipping run.");
			}
		}

		protected abstract void RunTaskCore(CancellationToken token);

		protected bool CanAccessExternalStorage
		{
			get
			{
				try
				{
					return ObjectFactory.Get<IExternalPersisterProvider>().GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3).AllowWrite;
				}
				catch (Exception ex)
				{
					ServiceLogger.Error($"Exception: {ex.Message} Inner Exception: {ex.InnerException?.Message}");
					return false;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string message to show in service task logs.")]
		protected const string ServiceStoppedNoAccessToExternalStorageMessage = "This service task has stopped because it is unable to access external storage.";
	}
}
