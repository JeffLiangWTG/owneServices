using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine.ArchiveStages
{
	public class PARArchiveStage : ArchiveStage
	{
		public PARArchiveStage(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor)
			: base(descriptor, systemDescriptor)
		{ }

		int totalNumberOfStorageMainsLoaded;
		long totalTimeTakenToDelete;

		protected override void LogSummaryOfNumberLoadedAndTimeTaken()
		{
			_logger.LogInfo(systemDescriptor.Code, $"Time taken to load {totalNumberOfStorageMainsLoaded} {mainArchiveableType.PKColumn.TableName} record(s) and their eDoc(s) in {totalNumberOfBatchesLoaded} batch(es): {timeTakenToLoadAllArchiveSets}ms.");
		}

		protected override void LogSummaryOfNumberDeletedAndTimeTaken()
		{
			_logger.LogInfo(systemDescriptor.Code, $"Time taken to {systemDescriptor.Noun.GetUnresolvedString().ToLower()} {totalNumberOfStorageMainsLoaded} {mainArchiveableType.PKColumn.TableName} record(s) " +
				$"and their eDoc(s): " +
				$"{totalTimeTakenToDelete}ms");
		}

		void ReadPKs(Dictionary<int, List<(Guid, Guid, string)>> storageMainPKs, IDataRecord reader)
		{
			var dbNumber = (int)reader[StorageMainSchema.Constants.SM_DB];

			if (!storageMainPKs.ContainsKey(dbNumber))
			{
				storageMainPKs.Add((int)reader[StorageMainSchema.Constants.SM_DB], new List<(Guid, Guid, string)>());
			}

			var archivedRecord = (
				(Guid)reader[StorageMainSchema.Constants.PK],
				(Guid)reader[StorageMainSchema.Constants.SM_ParentFK],
				(string)reader[StorageMainSchema.Constants.SM_Type]);

			storageMainPKs[dbNumber].Add(archivedRecord);
		}

		public override int BatchSize => SystemDataRegistry.Instance.PurgeArchivedRecordsBatchSizeControl.Value;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Need to send sql directly to minimise memory usage and roundtrips")]
		public new List<(Guid, Guid, string)> GetNextArchiveSet()
		{
			if (!Initialised)
			{
				throw new NotInitialisedException("Call BeginRun(..) first to initialise the ArchiveStage, before GetNextArchiveSet can be called.");
			}

			var sqlText = @"
SELECT DISTINCT TOP(@batchSize) SM_PK,
                         SM_TYPE,
                         SM_ParentFK,
                         SM_DB
FROM   dbo.StorageMain
       INNER JOIN dbo.StorageReference
               ON StorageMain.SM_PK = StorageReference.SR_SM
WHERE (1 = 1)
      AND (StorageMain.SM_Archived IS NOT NULL)
      AND (StorageMain.SM_OffLine IS NULL)
      AND SM_Archived < @archivedDate
";
			var storageMainPKs = new Dictionary<int, List<(Guid, Guid, string)>>();

			Stopwatch sw = null;
			if (IncludeTimeTakenInTheARCLogs)
			{
				sw = new Stopwatch();
				sw.Start();
			}

			using (var command = Db.Connection.Command(sqlText))
			{
				Db.Connection.ExecuteReader(command.CommandText, command =>
				{
					command.AddParameter("@archivedDate", SqlDbType.SmallDateTime, _config.ArchiveJobsOnOrBeforeThisDate.AddDays(1).ToSmallDateTime());
					command.AddParameter("@batchSize", SqlDbType.Int, BatchSize);
				},
				reader => ReadPKs(storageMainPKs, reader));
			}

			if (IncludeTimeTakenInTheARCLogs)
			{
				sw.Stop();
				totalTimeTakenToDelete += sw.ElapsedMilliseconds;
				totalNumberOfStorageMainsLoaded += storageMainPKs.Count;
				totalNumberOfBatchesLoaded += storageMainPKs.Count != 0 ? 1 : 0;
			}

			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			return storageMainPKs
				.Where(kv => documentFactory.GetDbWriteableState(kv.Key) != DbWriteableState.ReadOnly)
				.SelectMany(kv => kv.Value)
				.ToList();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log file not required to be translated, Log messages not required for translation, Log message does not need to be localised")]
		public override bool ExecuteStage(IArchiveSystem system, IArchiveStage stage, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSchedule schedule, CancellationToken token)
		{
			var result = true;
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			documentFactory.NameForDebugging = "Document Factory";
			documentFactory.RefreshEnabled = false;

			var lockResult = Db.Connection.TryGetLock("PARArchiveStageLock", TimeSpan.FromMilliseconds(0), out var parLock);
			if (lockResult)
			{
				using (parLock)
				{
					try
					{
						BeginRun(config, schedule, logger);
						do
						{
							if (CheckForStoppingCondition(token, logger))
							{
								return false;
							}

							var archiveSets = GetNextArchiveSet();
							NumberOfMainRecordsLoaded += archiveSets.Count;
							if (archiveSets.Count == 0)
							{
								result = true;
								break;
							}

							if (ZDateTime.UtcNow > config.StopRunTime)
							{
								logger.LogInfo(system.Descriptor.Code, string.Format(CultureInfo.InvariantCulture, (NoResString)"Max Run Duration reached for {0}", system.Descriptor.Name));
								result = false;
								return result;
							}

							foreach (var archiveSet in archiveSets)
							{
								var storageMain = documentFactory.RetrieveExistingOrCreateStorageMainForPK(archiveSet.Item2, archiveSet.Item3);
								logger.LogInfo(system.Descriptor.Code, $"Deleting record {storageMain.PK} of type {storageMain.SM_Type}. Archived on: {storageMain.SM_Archived}");
								logger.LogInfo(system.Descriptor.Code, $"eDocs: {storageMain.eDocs.Count}");

								// Unlike other archiving systems, PAR archiving system uses business logic (StorageMain's Delete() method) to perform archiving instead of PurgeAction
								// So we need to create ArchiveSet and load its ArchiveItems in order to calculate amount of records deleted
								// We'll be loading them in Delet method abyway, so as long as we do this in the same factory - they'll be cached

								var mainArchiveItem = new ArchiveItem(mainArchiveableType.PKColumn, storageMain.PK.ToGuid(), null, Guid.Empty, false, mainArchiveableType.PKColumn.ColumnPrefix);
								var set = new PARArchiveSet(systemDescriptor, stage.Name, schedule.SchedulePK, storageMain, mainArchiveItem, mainArchiveableType, null);

								var stepResult = set.Load(logger);

								if (stepResult.ErrorsEncountered.Count == 0 && set.Count > 0)
								{
									storageMain.Delete();
									documentFactory.Save();
									descriptor.OnArchiveSetProcessed(set);
								}
								else if (set.Count == 0)
								{
									logger.LogError(system.Descriptor.Code, $"Failed to load related records for {storageMain.PK} of type {storageMain.SM_Type}. Skipping for this run.");
								}
							}
						}
						while (true);
					}
					catch (SqlException e) when (new DbErrorMatch(e).ExceptionType == DbErrorType.CouldNotLocateDbInSysdatabases)
					{
						if (!Env.Instance.IsProductionSystem)
						{
							var dbHelper = new DocManagerDBHelper();
							var log = string.Format(@"Unable to purge documents due to an error retrieving data.
Please inform your System Administrator and have them ensure eDoc databases have been correctly restored.

Most likely the main database {0} has been renamed or copied from elsewhere and has existing document data in it.
To fix the error, rename or copy all other existing databases with the prefix {0}_SDXXX.

Please make sure the eDoc databases have been restored correctly.", dbHelper.GetDatabaseName(0));

							logger.LogError(system.Descriptor.Code, log);
						}
						var message = $"Error encountered during {system.Descriptor.PresentTenseVerb.GetUnresolvedString().ToLower()}. Run aborted.";
						logger.LogAndReportError("PARArchiveStage.ExceuteStageSQLException", systemDescriptor.Code, message, e);
						result = false;
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						var message = $"Error encountered during {system.Descriptor.PresentTenseVerb.GetUnresolvedString().ToLower()}. Run aborted.";
						logger.LogAndReportError("PARArchiveStage.ExecuteStageException", systemDescriptor.Code, message, e);
						result = false;
					}
					finally
					{
						EndRun();
					}
				}
			}
			else
			{
				logger.LogInfo(system.Descriptor.Code, (NoResString)"PAR is already running in another instance of ARC.");
				Initialised = false;
				return false;
			}

			return result;
		}
	}

	class PARArchiveSet : ArchiveSet
	{
		public PARArchiveSet(IArchiveSystemDescriptor descriptor, string stageName, Guid schedulePK, StorageMain storageMain, ArchiveItem mainItem, ArchiveableType mainArchiveableType, ZQuery mainArchiveableTypeFilter)
			: base(descriptor, stageName, schedulePK, mainItem, mainArchiveableType, mainArchiveableTypeFilter)
		{
			this.storageMain = storageMain;
		}

		public new IArchiveStepResult Load(IArchiveLogger logger)
		{
			var stepResult = new ArchiveStepResult();

			try
			{
				var count = storageMain.eDocs.Count + storageMain.StorageReferences.Length + 1;

				itemDictionary = new Dictionary<Guid, IArchiveItem>(count);
				itemDictionary.Add(MainArchiveItem.PK, MainArchiveItem);

				foreach (var eDoc in storageMain.eDocs.ToArray())
				{
					itemDictionary.Add(eDoc.PK.ToGuid(), new ArchiveItem(StorageDocsSchema.PK, eDoc.PK.ToGuid(), StorageMainSchema.PK, storageMain.PK.ToGuid(), false, StorageDocsSchema.PK.ColumnPrefix));
				}

				foreach (var storageReference in storageMain.StorageReferences)
				{
					itemDictionary.Add(storageReference.PK.ToGuid(), new ArchiveItem(StorageReferenceSchema.PK, storageReference.PK.ToGuid(), StorageMainSchema.PK, storageMain.PK.ToGuid(), false, StorageReferenceSchema.PK.ColumnPrefix));
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				itemDictionary.Clear();

				var builder = new StringBuilder(500);
				_ = builder.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"Unable to load ArchiveSet for main record type: {0}, table: {1}, pk: {2}", MainArchiveItem.PKColumn.TableName, StorageMainSchema.PK.TableName, MainArchiveItem.PK.ToString()));
				_ = builder.AppendLine((NoResString)"This set will be skipped. Full error below:");
				_ = builder.AppendLine();
				_ = builder.AppendLine(e.ToString());

				var errorMessage = builder.ToString();
				logger.LogAndReportError("ArchiveStepResult.LoadException", SystemDescriptor.Code, errorMessage, e);
				stepResult.ErrorsEncountered.Add(errorMessage);
			}

			return stepResult;
		}

		readonly StorageMain storageMain;
	}
}
