using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine.ArchiveStages
{
	public class PDOPurgeStage : ArchiveStage
	{
		public override int BatchSize
			=> SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.Value;

		public PDOPurgeStage(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor)
			: base(descriptor, systemDescriptor)
		{
		}

		protected override void LogSummaryOfNumberLoadedAndTimeTaken()
		{
			var totalTimeTakenToDeleteAllDocuments = allLoadedArchiveSets.Sum(x => x.TimeTakenToDeleteAllDocuments);
			var totalNumberOfDocumentsDeleted = allLoadedArchiveSets.Sum(set =>
				set.GetTotalDocumentsDeletedFromArchiveItems().ContainsKey(StorageDocsSchema.Constants.TableName) ? set.GetTotalDocumentsDeletedFromArchiveItems()[StorageDocsSchema.Constants.TableName].Count : 0);
			_logger.LogInfo(systemDescriptor.Code, $"Time taken to delete {totalNumberOfDocumentsDeleted} document(s): {totalTimeTakenToDeleteAllDocuments}ms.");
		}

		protected override void LogSummaryOfNumberDeletedAndTimeTaken()
		{
			return;
		}

		public override void LogLoadedArchiveSetDetails(IArchiveSystem system, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSet archiveSet, Stopwatch sw = null)
		{
			var archiveItemCode = archiveSet.MainArchiveItemNK ?? archiveSet.MainArchiveItem.PK.ToString();
			logger.LogInfo(system.Descriptor.Code, $"Loaded {archiveSet.MainArchiveItem.PKColumn.TableName} '{archiveItemCode}'{Helpers.GetTimeTaken(sw)}");
		}

		public override IArchiveStepResult ArchiveToImages(IArchiveSet archiveSet)
		{
			var executionErrors = descriptor
				.GetPreparationAction(_logger, archiveSet, archiveSystemCache, _config)
				.WhereNotNull()
				.Select(action => ExecuteAction(action))
				.WhereNotNull();

			var result = new ArchiveStepResult();
			result.ErrorsEncountered.AddRange(executionErrors);
			return result;
		}

		protected override void LogDetailsOfSkippedJobHeader(IArchiveSystem system, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSet archiveSet, ReasonsForSkipping reason)
		{
			LogGeneralReasonForSkipping(system, logger, archiveSet, reason);
		}

		protected override int NumberOfBatchesToLoadPerDriverRun => 1;

		protected override bool CheckForOverlap(IArchiveSet archiveSet)
		{
			return false;
		}

		public override bool ExecuteStage(IArchiveSystem system, IArchiveStage stage, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSchedule schedule, CancellationToken token)
		{
			var getLockResult = Db.Connection.TryGetLock("RunPDOLock", TimeSpan.FromMilliseconds(0), out var sqlLock);

			if (getLockResult)
			{
				using (sqlLock)
				{
					return base.ExecuteStage(system, stage, config, logger, schedule, token);
				}
			}
			else
			{
				logger.LogInfo(systemDescriptor.Code, (NoResString)"PDO is already running in another instance of ARC.");
				Initialised = false;
				return false;
			}
		}

		string ExecuteAction(IArchivePreparationAction action)
		{
			try
			{
				action.Execute();
				return null;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				var errorMessage = $"Failed to purge this set. A preparation action failed.";
				_logger.LogAndReportError("PDOPurgeStage.PurgeStageException", systemDescriptor.Code, errorMessage, e);
				return errorMessage;
			}
		}
	}
}
