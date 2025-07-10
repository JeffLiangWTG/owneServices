using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.ServiceTasks
{
	class EDocsAllContentToExternalStorageProcessor : BaseEDocsContentToExternalStorageProcessor
	{
		public EDocsAllContentToExternalStorageProcessor(int batchSize = 100) : base(batchSize)
		{
		}

		public override void Run(ILogger serviceLogger, CancellationToken token)
		{
			if (!Validate(serviceLogger))
			{
				return;
			}

			var dbHelper = new DocManagerDBHelper();
			var missingDbs = dbHelper.GetDBNamesMissing();
			if (missingDbs.Any())
			{
				var missingDbNames = string.Join(System.Environment.NewLine, missingDbs);
				serviceLogger.Error(FormattableString.Invariant($@"Moving eDocs to external storage has encountered an error due to missing Document Databases.
Please inform your System Administrator that the following document databases are missing:
{missingDbNames}"));
			}

			var sdDbNumbers = dbHelper.GetStorageDocDbNumbersIncludingMainDb().Where(n => n > DocManagerDBHelper.InitialStorageDocsDatabaseNumber);

			foreach (var sdDbNumber in sdDbNumbers)
			{
				var sdDbName = dbHelper.GetDatabaseName(sdDbNumber);
				using (var sdDbConnection = Db.NewExtraConnectionWithMainDbCredentials(Db.Connection.ServerName, sdDbName))
				{
					if (sdDbConnection.TryGetLock("DESProcessingLock", out var mutex, sdDbName))
					{
						using (mutex)
						{
							var lastProcessedSM_PK = ZGuid.Empty;
							var excludedSM_PKs = new List<ZGuid>();
							while (!token.IsCancellationRequested) // this is bit doggy, back to it later
							{
								var query = new ZDBOnlyQuery(typeof(StorageMain)) { MaximumRows = BatchSize };
								query.AddToFilter(StorageMainSchema.SM_DB, sdDbNumber);

								if (excludedSM_PKs.Any())
								{
									query.AddToFilter(StorageMainSchema.PK, SQLComparisonOperator.NotEqual, excludedSM_PKs);
								}

								var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory { RefreshEnabled = false });
								var storageMainBatch = masterFactory.Load<StorageMain>(query);

								if (!storageMainBatch.IsNullOrEmpty())
								{
									serviceLogger.Information(FormattableString.Invariant($"Started to process StorageMain batch with size {storageMainBatch.Length}"));

									foreach (StorageMain storageMain in storageMainBatch)
									{
										lastProcessedSM_PK = storageMain.PK;

										if (!ProcessStorageMain(storageMain, masterFactory, token, serviceLogger, sdDbConnection))
										{
											excludedSM_PKs.Add(lastProcessedSM_PK);
										}

										if (token.IsCancellationRequested)
										{
											serviceLogger.Information((NoResString)"Stopped processing because of cancellation");
											break;
										}
									}

									serviceLogger.Information(FormattableString.Invariant($"Finished processing StorageMain batch with size {storageMainBatch.Length}"));
								}
								else
								{
									break;
								}
							}

							if (token.IsCancellationRequested)
							{
								serviceLogger.Information((NoResString)"Stopped processing because of cancellation");
								break;
							}
						}
					}
				}
			}
		}

		protected override bool Validate(ILogger logger)
		{
			if (Db.Connection.GetDatabases(DatabaseType.SD).Count() == 1)
			{
				logger.Warning(ServiceNotRequiredMessage);
				return false;
			}

			return base.Validate(logger);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string message to show in service task logs.")]
		public const string ServiceNotRequiredMessage = "This service task is not required to run as there is only one eDocs database (SD001) present.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Auto generated baseline suppressions - WI00545660")]
		bool ProcessStorageMain(StorageMain storageMain, DocumentFactory masterFactory, CancellationToken token, ILogger serviceLogger, DbConnection sdDbConnection)
		{
			var allEDocsProcessed = false;
			var docManagerDBHelper = new DocManagerDBHelper();
			var initialStorageDocsDatabaseName = docManagerDBHelper.GetDatabaseName(DocManagerDBHelper.InitialStorageDocsDatabaseNumber);

			sdDbConnection.EnsureIsOpen();
			var excludedStorageDocs = new List<ZGuid>();

			ProcessStorageDocs(storageMain, sdDbConnection, masterFactory, token, serviceLogger, excludedStorageDocs);

			if (token.IsCancellationRequested)
			{
				serviceLogger.Information((NoResString)"Stopped processing because of cancellation");
				return false;
			}

			using (var transaction = sdDbConnection.BeginTransactionWithManager())
			{
				//range lock on parent
				using (var lockParentCommand = sdDbConnection.Command(FormattableString.Invariant($"SELECT COUNT(*) FROM {StorageDocsSchema.Constants.SqlSchemaName}.{StorageDocsSchema.Constants.TableName} WITH (UPDLOCK, HOLDLOCK) WHERE {StorageDocsSchema.Constants.SC_SM} = @StorageMainPK")))
				{
					lockParentCommand.AddParameter(ZSqlParameter.New("@StorageMainPK", storageMain.PK, StorageDocsSchema.SC_SM));
					lockParentCommand.ExecuteNonQuery();
				}

				//expect 0 rows here, done 'just in case'
				ProcessStorageDocs(storageMain, sdDbConnection, masterFactory, token, serviceLogger, excludedStorageDocs);

				if (!excludedStorageDocs.Any())
				{
					var storageDocsColumns = string.Join(", ", StorageDocsSchema.All.Select(x => x.Name));

					try
					{
						using (var mergeParentCommand = sdDbConnection.Command(FormattableString.Invariant($@"
BEGIN TRY
	DELETE {initialStorageDocsDatabaseName}..{StorageDocsSchema.Constants.TableName}
		WHERE {StorageDocsSchema.Constants.PK} in
			(SELECT {StorageDocsSchema.Constants.PK} FROM {StorageDocsSchema.Constants.SqlSchemaName}.{StorageDocsSchema.Constants.TableName}
			WHERE {StorageDocsSchema.Constants.SC_SM} = @StorageMainPK)

	INSERT INTO {initialStorageDocsDatabaseName}..{StorageDocsSchema.Constants.TableName} ({storageDocsColumns})
		SELECT {storageDocsColumns} FROM {StorageDocsSchema.Constants.SqlSchemaName}.{StorageDocsSchema.Constants.TableName}
		WHERE {StorageDocsSchema.Constants.SC_SM} = @StorageMainPK

	DELETE FROM {StorageDocsSchema.Constants.SqlSchemaName}.{StorageDocsSchema.Constants.TableName}
		WHERE {StorageDocsSchema.Constants.SC_SM} = @StorageMainPK

	UPDATE {Db.DatabaseName}..{StorageMainSchema.Constants.TableName}
		SET {StorageMainSchema.Constants.SM_DB} = {DocManagerDBHelper.InitialStorageDocsDatabaseNumber}
		WHERE {StorageMainSchema.Constants.PK} = @StorageMainPK
END TRY
BEGIN CATCH
	THROW
END CATCH
")))
						{
							mergeParentCommand.AddParameter(ZSqlParameter.New("@StorageMainPK", storageMain.PK, StorageDocsSchema.SC_SM));
							mergeParentCommand.ExecuteNonQuery();
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var errorMessage = $"Unable to copy StorageDocs with SM_PK '{storageMain.PK}' from {sdDbConnection.CurrentDatabase} to SD001";
						serviceLogger.Error(errorMessage, ex);
						ErrorReporter.ReportOnce("StorageDocs_Copy_Error", errorMessage, ex);

						throw;
					}
					allEDocsProcessed = true;
				}

				transaction.CommitTransaction();
			}
			return allEDocsProcessed;
		}

		void ProcessStorageDocs(StorageMain storageMain, DbConnection connectionToStorageDocs, DocumentFactory masterFactory, CancellationToken token, ILogger serviceLogger, List<ZGuid> excludedPKs)
		{
			var totalDocsProcessed = 0L;
			while (!token.IsCancellationRequested)
			{
				var query = new ZDBOnlyQuery(typeof(StorageDocsBase)) { MaximumRows = BatchSize };
				query.AddToFilter(StorageDocsSchema.SC_SM, storageMain.PK);
				query.AddFilterAndZSQLParameterCollection(FormattableString.Invariant($"{StorageDocsBase.Schema.SC_ImageDataHasValueSchemaName} = 1"), null);
				query.IncludeBlob(StorageDocsSchema.SC_ImageData);

				if (excludedPKs.Any())
				{
					query.AddToFilter(StorageDocsSchema.PK, SQLComparisonOperator.NotEqual, excludedPKs);
				}

				var factory = new DbBackendDocumentFactory(new BusinessObjectFactory(connectionToStorageDocs) { RefreshEnabled = false }, connectionToStorageDocs, masterFactory, true);
				var batch = factory.Load(typeof(StorageDocsBase), query);

				if (batch.Any())
				{
					foreach (StorageDocsBase storageDoc in batch)
					{
						try
						{
							if (storageDoc.ParentMain == null)
							{
								continue;
							}
							if (storageDoc.SaveToExternalStorage())
							{
								storageDoc.SC_ImageData = ZBlob.Empty;
								totalDocsProcessed++;
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							HandleSaveToExternalStorageErrors(ex, storageDoc, serviceLogger);
							excludedPKs.Add(storageDoc.PK);
						}
					}

					ZExceptionReporting.ProcessWithConcurrencyHandling(() => factory.Save(), null);

					if (token.IsCancellationRequested)
					{
						serviceLogger.Information((NoResString)"Stopped processing because of cancellation");
						break;
					}
				}
				else
				{
					break;
				}
			}

			serviceLogger.Information(FormattableString.Invariant($"Processed {totalDocsProcessed} StorageDocs in DB {connectionToStorageDocs.CurrentDatabase}."));
		}

		protected override string ServiceFailedKey => "DesExecutionFailed";

		protected override string FailToExecuteNotificationSubject => (NoResString)"DES service failed to execute";
	}
}
