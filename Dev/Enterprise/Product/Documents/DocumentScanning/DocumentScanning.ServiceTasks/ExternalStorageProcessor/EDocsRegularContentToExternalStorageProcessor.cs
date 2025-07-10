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
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.ServiceTasks
{
	class EDocsRegularContentToExternalStorageProcessor : BaseEDocsContentToExternalStorageProcessor
	{
		public override void Run(ILogger serviceLogger, CancellationToken token)
		{
			if (!Validate(serviceLogger))
			{
				return;
			}

			ProcessInitialStorageDocs(serviceLogger, token);
		}

		void ProcessInitialStorageDocs(ILogger serviceLogger, CancellationToken token)
		{
			var sd001DBName = new DocManagerDBHelper().GetDatabaseName(DocManagerDBHelper.InitialStorageDocsDatabaseNumber);

			using (var sd001Connection = Db.NewExtraConnectionWithMainDbCredentials(Db.Connection.ServerName, sd001DBName))
			{
				var lockName = "DERDoubleLock";
				if (sd001Connection.TryGetLock(lockName, out var mutex, sd001DBName))
				{
					using (mutex)
					{
						sd001Connection.EnsureIsOpen();
						serviceLogger.Information((NoResString)"Run Start.");

						var shouldLoop = true;
						var excludedPKs = new List<ZGuid>();
						var totalDocsProcessed = 0;
						var batchSize = BatchSize;

						while (shouldLoop)
						{
							serviceLogger.Information(FormattableString.Invariant($"Loading new batch of StorageDocs."));

							var processedPKs = new List<ZGuid>();
							var orphanPKs = new List<ZGuid>();

							var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory { RefreshEnabled = false });
							var factory = new DbBackendDocumentFactory(new BusinessObjectFactory(sd001Connection) { RefreshEnabled = false }, sd001Connection, masterFactory, true);

							var query = new ZDBOnlyQuery(typeof(StorageDocsBase)) { MaximumRows = batchSize };
							query.AddFilterAndZSQLParameterCollection(FormattableString.Invariant($"{StorageDocsBase.Schema.SC_ImageDataHasValueSchemaName} = 1"), null);
							if (SystemDataRegistry.Instance.EDocsDBMinimumStorageDays.Value > 0)
							{
								query.AddToFilter(StorageDocsSchema.SC_Date, SQLComparisonOperator.LessThan, ZDateTime.UtcNow.AddDays(-SystemDataRegistry.Instance.EDocsDBMinimumStorageDays.Value));
							}
							query.IncludeBlob(StorageDocsSchema.SC_ImageData);
							query.TableIndexHints.Add(new TableIndexHint(StorageDocsBase.Schema.Indexes.NR_RX__SC_ImageDataHasValue_SC_Date));

							if (excludedPKs.Count > 0)
							{
								// to avoid falling into infinity loop
								query.AddToFilter(StorageDocsSchema.PK, SQLComparisonOperator.NotEqual, excludedPKs);
							}

							StorageDocsBase[] batch;
							try
							{
								batch = factory.Load<StorageDocsBase>(query);
								batchSize = BatchSize;
							}
							catch (OutOfMemoryException) when (batchSize > 20)
							{
								batchSize /= 2;
								continue;
							}

							if (batch.Length > 0)
							{
								serviceLogger.Information(FormattableString.Invariant($"Started processing batch of {batch.Length} StorageDocs."));

								foreach (var storageDoc in batch)
								{
									try
									{
										if (storageDoc.ParentMain == null || storageDoc.ParentMain.SM_DB != DocManagerDBHelper.InitialStorageDocsDatabaseNumber)
										{
											orphanPKs.Add(storageDoc.PK);
											continue;
										}

										// We check the lock again to ensure this is the only instance uploading the document
										// If db restarted or connection lost, SqlLockLostException will be thrown and this instance will be terminated
										if (!sd001Connection.HasAquiredLock(lockName))
										{
											serviceLogger.Information((NoResString)"Process terminated due of lost of exclusive lock");
											shouldLoop = false;
											break;
										}

										if (storageDoc.SaveToExternalStorage())
										{
											storageDoc.SC_ImageData = ZBlob.Empty;
											processedPKs.Add(storageDoc.PK);
										}
									}
									catch (Exception ex) when (!ex.IsCriticalException())
									{
										HandleSaveToExternalStorageErrors(ex, storageDoc, serviceLogger);
										excludedPKs.Add(storageDoc.PK);
									}
								}

								serviceLogger.Information(FormattableString.Invariant($"Uploaded {processedPKs.Count} StorageDocs to external storage."));
								ZExceptionReporting.ProcessWithConcurrencyHandling(() => factory.Save(), null);

								if (orphanPKs.Count > 0)
								{
									DeleteOrphans(sd001DBName, orphanPKs.ToArray());
									serviceLogger.Information(FormattableString.Invariant($"{orphanPKs.Count} orphan StorageDocs found in the batch and deleted."));
								}

								if (token.IsCancellationRequested)
								{
									serviceLogger.Information((NoResString)"Stopped processing because of cancellation");
									shouldLoop = false;
								}

								totalDocsProcessed += processedPKs.Count;
								serviceLogger.Information(FormattableString.Invariant($"Processed {processedPKs.Count} StorageDocs. PKs are: {string.Join(",", processedPKs)}"));
							}
							else
							{
								serviceLogger.Information(FormattableString.Invariant($"No StorageDocs were loaded."));
								shouldLoop = false;
							}
						}

						serviceLogger.Information(FormattableString.Invariant($"Run Completed, total processed {totalDocsProcessed} StorageDocs."));
					}
				}
			}
		}

		protected override bool Validate(ILogger logger)
		{
			if (!Db.Connection.GetDatabases(DatabaseType.SD).Any())
			{
				logger.Warning(NoDocumentDatabaseMessage);
				return false;
			}

			return base.Validate(logger);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods",
			Justification = "SC_PK is not unique among SD dbs, using BizoFactory to delete could accidentally delete different eDoc with the same PK in other factories. It could also delete the S3 object with the same PK in some case.")]
		void DeleteOrphans(string sd001DbName, ZGuid[] orphanPKs)
		{
			using (var cmd = Db.Connection.Command(FormattableString.Invariant($@"
DELETE {sd001DbName}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.PK} IN (SELECT value FROM @OrphanPKs)
")))
			{
				cmd.AddParameter(ZSqlParameter.New("@OrphanPKs", orphanPKs, StorageDocsSchema.PK, isTableValued: true));
				cmd.ExecuteNonQuery();
			}
		}
		protected override string ServiceFailedKey => "DerExecutionFailed";

		protected override string FailToExecuteNotificationSubject => (NoResString)"DER service failed to execute";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string message to show in service task logs.")]
		public const string NoDocumentDatabaseMessage = "Process cannot start as there is no document database.";
	}
}
