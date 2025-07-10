using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(EDocsExternalStorageSizeCalculationTask.ServiceTaskCode,
	EDocsExternalStorageSizeCalculationTask.ServiceTaskDescription,
	"DOC",
	typeof(EDocsExternalStorageSizeCalculationTask),
	IsScheduleReadOnly = true,
	MinimumPeriod = "10minutes",
	MaximumPeriod = "1day",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day",
	ActiveByDefault = true,
	DefaultScheduleStartAtLocal = "1hour")]

namespace Enterprise.DocumentScanning.ServiceTasks
{
	public class EDocsExternalStorageSizeCalculationTask : ServiceProviderImpl
	{
		public const string ServiceTaskCode = "DSC";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
		public const string ServiceTaskDescription = "eDocs External Storage Size Calculation Task";
		public const string ExternalStorageSizeRetrievalStartAfter = "ExternalStorageSize_StartAfter";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Log String")]
		public const string ExternalStorageSizeRetrievalNotFinishedMessage = "Process cannot start because external storage size retrieval transformation has not finished yet.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Log String")]
		public const string NoDocumentDatabaseMessage = "Process cannot start because there is no document database.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Log String")]
		public const string ServiceNotRequiredMessage = "This service task is not required to run for self-hosted system.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		public const string NoFilesUploadedToS3Message = "No eDocs are moved to S3 yet. Cannot get the start date for calculation. EDocsExternalStorageSize is set to 0.";

		[HostedServiceRequirement]
		public static string CheckEDocsStorageProvider() => HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(SystemDataRegistry.Instance.EDocsStorageProvider, Core.Constants.EDocsStorageProviders.Code.DB);

		[HostedServiceRequirement]
		public static string CheckIsHostedWithCargeWise() => (EnvProxy.IsHostedWithCargowise || ClientHookLoader.Instance.Client == Clients.EDI) ? string.Empty : ServiceNotRequiredMessage;

		public const int ZeroExternalStorageSizeEDocsCountThreshold = 10;

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Information((NoResString)"Started calculating eDocs external storage size");

			long s3BucketSize = -1;
			try
			{
				s3BucketSize = new AWSPersister().GetBucketSizeFromInternalAPI();
			}
			catch (BucketSizeWebException ex)
			{
				ServiceLogger.Error(ex.Message);
			}

			if (!Db.Connection.GetDatabases(DatabaseType.SD).Any())
			{
				UseS3BucketSize(NoDocumentDatabaseMessage);
				return;
			}

			var sizeJson = Business.DocManagerRegistry.Instance.EDocsExternalStorageSize.Value;
			if (string.IsNullOrEmpty(sizeJson) && !HasExternalStorageSizeBackFillFinished())
			{
				// Return if no calculation was done before, and back fill tranformation has not finished.
				UseS3BucketSize(ExternalStorageSizeRetrievalNotFinishedMessage);
				return;
			}

			var startDate = GetStartDate();
			if (startDate == ZDate.Empty)
			{
				UseS3BucketSize(NoFilesUploadedToS3Message);
				return;
			}

			long totalSize = 0;
			var backFillFailedCount = 0;
			const int dbNumber = 1;

			while (startDate < ZDateTime.Now)
			{
				var endDate = startDate.AddMonths(1);
				ServiceLogger.Information(FormattableString.Invariant($"Loading new batch of StorageDocs between {startDate} and {endDate}."));

				var query = new ZDBOnlyQuery(typeof(StorageDocsBase));
				query.AddFilterAndZSQLParameterCollection(FormattableString.Invariant($"{StorageDocsBase.Schema.SC_ImageDataHasValueSchemaName} = 0"), null);
				query.AddToFilter(StorageDocsSchema.SC_Date, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate);
				query.AddToFilter(StorageDocsSchema.SC_Date, SQLComparisonOperator.LessThan, endDate);
				query.TableIndexHints.Add(new TableIndexHint(StorageDocsBase.Schema.Indexes.NR_RX__SC_ImageDataHasValue_SC_Date));

				var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory { RefreshEnabled = false });
				var batch = masterFactory.GetFactory(dbNumber).Load<StorageDocsBase>(query);
				if (batch.Length > 0)
				{
					ServiceLogger.Information(FormattableString.Invariant($"Started calculating size for batch of {batch.Length} StorageDocs."));

					foreach (var storageDoc in batch)
					{
						var size = storageDoc.SC_ExternalStorageSize;
						totalSize += size;
						if (size == 0 && storageDoc.SC_UncompressedSize != 0)
						{
							backFillFailedCount++;
							if (backFillFailedCount <= ZeroExternalStorageSizeEDocsCountThreshold)
							{
								ServiceLogger.Warning($"eDoc with PK {storageDoc.PK} doesn't have external storage size filled.");
							}
							else
							{
								SaveExternalStorageSize(totalSize, s3BucketSize, backFillFailedCount);
								return;
							}
						}
					}

					token.ThrowIfCancellationRequested();
				}

				startDate = endDate;
			}

			SaveExternalStorageSize(totalSize, s3BucketSize, backFillFailedCount);

			void UseS3BucketSize(string message)
			{
				var size = new AWSPersister.EDocsExternalStorageSize
				{
					CalculateDateTimeUtc = ZDateTime.UtcNow.ToDateTime(),
					CalculatedSize = 0,
					S3BucketSize = s3BucketSize,
					DoNotUseCalculatedSize = true,
				};
				Business.DocManagerRegistry.Instance.EDocsExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JsonConvert.SerializeObject(size));
				ServiceLogger.Warning(message);
				LogFinishedMessage();
			}
		}

		void SaveExternalStorageSize(long totalSize, long s3BucketSize, int backFillFailedCount)
		{
			var threshold = ZeroExternalStorageSizeEDocsCountThreshold;
			var doNotUseCalculatedSize = backFillFailedCount > threshold;
			var externalStorageSize = new AWSPersister.EDocsExternalStorageSize
			{
				CalculateDateTimeUtc = ZDateTime.UtcNow.ToDateTime(),
				CalculatedSize = totalSize,
				S3BucketSize = s3BucketSize,
				DoNotUseCalculatedSize = doNotUseCalculatedSize
			};
			Business.DocManagerRegistry.Instance.EDocsExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, JsonConvert.SerializeObject(externalStorageSize));

			ServiceLogger.Information((doNotUseCalculatedSize || !Business.DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.Value) ?
				(NoResString)"The S3 storage billing will be calculated based on the actual bucket size" :
				(NoResString)"The S3 storage billing will be calculated based on the actual sizes recorded in SC_ExternalStorageSize column");

			if (doNotUseCalculatedSize)
			{
				// Log and report
				var message = $"More than {threshold} documents don't have external storage size filled, please check service log to see some of the document PKs.";
				ServiceLogger.Warning(message);

				UnattendedUserNotification.Instance.ShowErrorOnceADay(
					"EDocsNoExternalStorageSize",
					message,
					(NoResString)"Documents with no external storage sizes",
					sendToPostMaster: true);
			}
			else
			{
				var totalSizeInMb = AWSPersister.SizeInByteToSizeInMb(totalSize);
				var s3BucketSizeInMb = AWSPersister.SizeInByteToSizeInMb(s3BucketSize);
				var caculateDateTime = externalStorageSize.CalculateDateTimeUtc.ToString("yyyy-MM-dd HH-mm-ss");
				ServiceLogger.Information($"End of calculating storage size. On {caculateDateTime} UTC the calculated external storage size is {totalSizeInMb} MB and S3 bucket size is {s3BucketSizeInMb} MB");
			}

			LogFinishedMessage();
		}

		void LogFinishedMessage()
		{
			ServiceLogger.Information((NoResString)"Finished calculating eDocs external storage size");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods")]
		ZDate GetStartDate()
		{
			var tableNameWithDBPrefix = new DocManagerDBHelper().GetTableNameWithDatabasePrefix(1, StorageDocsSchema.Constants.TableName);
			var sql = $"SELECT MIN(SC_Date) FROM {tableNameWithDBPrefix} WITH (FORCESEEK, Index(NR_RX__SC_ImageDataHasValue_SC_Date)) WHERE {StorageDocsSchema.Constants.SC_ImageDataHasValue} = 0";
			using var command = Db.Connection.Command(sql); // use direct sql  to avoid loading documents into memory
			var startDate = command.ExecuteScalar();
			return startDate == DBNull.Value ? ZDate.Empty : new ZDate(startDate, DateTimeKind.Utc);
		}

		static bool HasExternalStorageSizeBackFillFinished()
		{
			// When ExternalStorageSizeRetrieval is started, the StartAfter property is added to DB connection,
			// and it's removed after the transformation is done.
			var startAfter = ExtProperty.Database.Select(Db.Connection, ExternalStorageSizeRetrievalStartAfter);
			return startAfter == null;
		}
	}
}
