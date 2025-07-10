using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Engine
{
	public class MAIOnlyArchiveStage : ArchiveStage
	{
		public MAIOnlyArchiveStage(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor)
			: base(descriptor, systemDescriptor)
		{
		}

		public virtual IEnumerable<IArchiveSet> GetNextArchiveSet(IArchiveSchedule schedule, IArchiveStage stage)
		{
			var archiveSets = new List<MAIOnlyArchiveSet>();
			var mainArchiveableTypeFilter = GetMainBusinessObjectFilter(_config);

			var set = new MAIOnlyArchiveSet(systemDescriptor, descriptor, StageNameForQueries, schedule, mainArchiveableType, mainArchiveableTypeFilter, BatchSize);
			archiveSets.Add(set);

			if (IncludeTimeTakenInTheARCLogs)
			{
				allLoadedArchiveSets.AddRange(archiveSets);
			}

			return archiveSets;
		}

		public override bool ExecuteStage(IArchiveSystem system, IArchiveStage stage, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSchedule schedule, CancellationToken token)
		{
			var getLockResult = Db.Connection.TryGetLock($"Run{system.Descriptor.Code}Lock", TimeSpan.FromMilliseconds(0), out var sqlLock);

			if (!getLockResult)
			{
				logger.LogInfo(systemDescriptor.Code, (NoResString)$"{system.Descriptor.Code} is already running in another instance of ARC.");
				Initialised = false;
				return false;
			}

			using (sqlLock)
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

						IArchiveSet archiveSet = null;
						var numberOfTimesToRetryBeforeGivingUp = 3;
						while (true)
						{
							try
							{
								archiveSet = GetNextArchiveSet(schedule, stage).FirstOrDefault();
								break;
							}
							catch (Exception e) when (!e.IsCriticalException())
							{
								if (numberOfTimesToRetryBeforeGivingUp <= 0)
								{
									throw;
								}
								else
								{
									Thread.Sleep(SleepSpanBeforeGetNextArchiveSetRetry);
									numberOfTimesToRetryBeforeGivingUp--;
								}
							}
						}

						var itemsLoaded = 0;

						if (archiveSet != null)
						{
							itemsLoaded = ExecuteForMAIArchiveSet(system, stage, config, logger, archiveSet);
						}

						if (itemsLoaded == 0 && state != StageStates.Running)
						{
							return state == StageStates.NoMoreRecords;
						}

						NumberOfMainRecordsLoaded += itemsLoaded;
					}
					while (true);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					state = StageStates.Error;

					var errorMessage = $"Error encountered during {system.Descriptor.PresentTenseVerb.GetUnresolvedString().ToLower()}. Run aborted.";
					logger.LogAndReportError("ArchiveStage.ExecuteStageException", systemDescriptor.Code, errorMessage, e);

					return false;
				}
				finally
				{
					EndRun();
				}
			}
		}

		public override PurgeAction GetPurgeAction(IArchiveSet archiveSet)
		{
			return new MAIOnlyPurgeAction(archiveSet, _logger);
		}

		public int ExecuteForMAIArchiveSet(IArchiveSystem system, IArchiveStage stage, IArchiveConfiguration config, IArchiveLogger logger, IArchiveSet archiveSet)
		{
			Stopwatch sw = null;
			if (IncludeTimeTakenInTheARCLogs)
			{
				sw = new Stopwatch();
				sw.Start();
			}

			var stepResult = archiveSet.Load(logger);

			if (stepResult.ErrorsEncountered.Count > 0)
			{
				if (archiveSet.Count == 0)
				{
					LogGeneralReasonForSkipping(system, logger, archiveSet, ReasonsForSkipping.LoadedNothing);
				}
				return 0;
			}

			var numberOfRowsLoaded = archiveSet.Count;
			if (IncludeTimeTakenInTheARCLogs)
			{
				sw.Stop();
				timeTakenToLoadAllArchiveSets += sw.ElapsedMilliseconds;
				totalNumberOfArchiveSetsLoaded += numberOfRowsLoaded;
				totalNumberOfBatchesLoaded += numberOfRowsLoaded != 0 ? 1 : 0;
			}

			LogLoadedArchiveSetDetails(system, config, logger, archiveSet);

			var archiveToImageResult = stage.ArchiveToImages(archiveSet);

			if (archiveToImageResult.ErrorsEncountered.Count > 0)
			{
				LogGeneralReasonForSkipping(system, logger, archiveSet, ReasonsForSkipping.ArchivingFailed);
			}
			else
			{
				descriptor.OnArchiveSetProcessed(archiveSet);
			}

			if (numberOfRowsLoaded == 0)
			{
				state = StageStates.NoMoreRecords;
			}

			return numberOfRowsLoaded;
		}

		protected override void LogDetails(IArchiveSystem system, IArchiveLogger logger, IArchiveSet archiveSet, Stopwatch sw, StringBuilder infoBuilder)
		{
			logger.LogInfo(system.Descriptor.Code, $"Loaded batch of {archiveSet.Count} {mainArchiveableType.PKColumn.TableName}{Helpers.GetTimeTaken(sw)}{System.Environment.NewLine}{infoBuilder}");
		}

		protected override void LogGeneralReasonForSkipping(IArchiveSystem system, IArchiveLogger logger, IArchiveSet archiveSet, ReasonsForSkipping reason)
		{
			if (archiveSet is MAIOnlyArchiveSet maiOnlyArchiveSet)
			{
				var reasonForSkipping = reason == ReasonsForSkipping.ArchivingFailed ? (NoResString)"archiving" : (NoResString)"loading";
				logger.LogInfo(system.Descriptor.Code, $"Skipped records dated between '{ConvertWatermarkDateToString(maiOnlyArchiveSet.DateRangeStartWatermark)}' and '{ConvertWatermarkDateToString(maiOnlyArchiveSet.DateRangeEndWatermark)}' because of errors with {reasonForSkipping} records.");
			}
		}

		string ConvertWatermarkDateToString(IArchiveWatermark watermark)
		{
			return watermark?.WatermarkDate.ToShortDateString() ?? (NoResString)"Earliest Possible Date";
		}
	}
}
