using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Actions;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.ArchiveEligibility;
using Enterprise.ArchiveManager.Business.StageDescriptors;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Billing.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	public abstract class CommonArchiveStageDescriptor : ICommonArchiveStageDescriptor
	{
		IArchiveableBusinessObjectProviderCache businessObjectProviderCache;

		public IArchiveStageStopwatch ArchiveStageStopWatch { get; private set; }
		public IArchiveableBusinessObjectProviderCache BusinessObjectProviderDictionary
		{
			get
			{
				if (businessObjectProviderCache == null)
				{
					businessObjectProviderCache = ObjectFactory.Get<IArchiveableBusinessObjectProviderCache>();
				}

				return businessObjectProviderCache;
			}
		}
		public ZDateTime ArchiveToDateAtBeginning { get; protected set; }
		public IArchiveWatermark WatermarkAtBeginning { get; protected set; }

		protected ConcurrentDictionary<string, ITableProcessingInfo> processingInfoPerTable = new();
		public ConcurrentDictionary<string, ITableProcessingInfo> ProcessingInfoPerTable
			=> processingInfoPerTable;

		#region IArchiveStageDescriptor Members

		public abstract string Name { get; }

		public virtual SchemaColumn MainArchivePKColumn
			=> JobHeaderSchema.PK;

		public virtual SchemaColumn MainArchiveNKColumn
			=> JobHeaderSchema.JH_JobNum;

		public virtual SchemaDateTimeColumn MainDateFilterColumn
			=> _config.IsFilteringByJobOpenDate ? JobHeaderSchema.JH_A_JOP : JobHeaderSchema.JH_A_JCL;

		public IArchiveConfiguration _config { get; protected set; }

		public virtual bool IsStageUsingTempTables
			=> true;

		public virtual void Setup(IArchiveSystemSetup systemSetup, IArchiveSchedule schedule, IArchiveConfiguration config)
		{
			SetupArchiveRelationships(systemSetup, config);

			ArchiveStageStopWatch = new ArchiveStageStopwatch();
			ArchiveStageStopWatch.Restart();
			WatermarkAtBeginning = schedule.GetWatermark(Name);
			ArchiveToDateAtBeginning = config.ArchiveJobsOnOrBeforeThisDate;
		}

		public virtual void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> ArchiveRelationships.SetupArchiveRelationships(systemSetup, config);

		public virtual ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			// Find only closed operational JobHeaders
			// that do not have any transactions in an open period
			// and that do not have any open hot cheques
			_config = config;
			var query = new ZQuery();
			var filters = GetCommonArchiveableFiltersForConfig(config, MainDateFilterColumn);

			filters.ForEach(f => f.ApplyTo(query));
			query.OrderBy = MainDateFilterColumn.Name;

			return query;
		}

		public static IEnumerable<JobHeaderArchiveableFilter> GetCommonArchiveableFiltersForConfig(IArchiveConfiguration config, SchemaDateTimeColumn mainDateFilterColumn)
		{
			yield return JobHeaderArchiveableFilter.JobIsClosed;
			yield return JobHeaderArchiveableFilter.GetDateColumnFilter(mainDateFilterColumn, config.ArchiveJobsOnOrBeforeThisDate);

			yield return config.ShouldIncludeDeclarations
				? JobHeaderArchiveableFilter.ParentTableCodeIsOperationalJobOrDeclaration
				: JobHeaderArchiveableFilter.ParentTableCodeIsOperationalJob;

			yield return JobHeaderArchiveableFilter.AccountingPeriod;
			yield return JobHeaderArchiveableFilter.NoOpenAssociatedHotCheques;
			yield return JobHeaderArchiveableFilter.NoAssociatedRateAttachments;
			yield return JobHeaderArchiveableFilter.NoAssociatedJobShipments;
			yield return JobHeaderArchiveableFilter.NoAssociatedHVLVScanningSummary;

			if (!config.ShouldIncludeDeclarations)
			{
				yield return JobHeaderArchiveableFilter.NoAssociatedCusUSLVConsignment;
				yield return JobHeaderArchiveableFilter.NoAssociatedJobDeclarations;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusSCAHouse;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusHawb;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusSCADepotHouse;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusSCAOceanBill;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusCAeMHMaster_JobConShipLink;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusCAeMHMaster_JobConsol;
				yield return JobHeaderArchiveableFilter.NoAssociatedAsycudaManifestHeader_JobConShipLink;
				yield return JobHeaderArchiveableFilter.NoAssociatedAsycudaManifestHeader_JobConsol;
				yield return JobHeaderArchiveableFilter.NoAssociatedAsycudaBill;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusDecHouseBill;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusOutturn;
				yield return JobHeaderArchiveableFilter.NoAssociatedJobConsolLinkedToJobDeclaration;
			}
		}

		public virtual IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
			=> throw new NotImplementedException();

		public virtual IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
		{
			var nullifyFKAction = new NullifyFKAction();
			var periodAction = new PeriodArchiveCommencedAction();

			foreach (var item in set.GetArchiveItems())
			{
				if (item.PKColumn.TableName == JobHeaderSchema.Constants.TableName)
				{
					NullifyRelationships.FkToJobHeader.ForEach(schemaGuidColumn => nullifyFKAction.Add(schemaGuidColumn, item.PK));

					periodAction.Add(item.PK);
				}
			}

			yield return periodAction;
			yield return nullifyFKAction;
		}

		public void Finalise(IArchiveSystemDescriptor descriptor, IArchiveLogger logger, IArchiveSchedule schedule, IArchiveConfiguration config)
		{
			try
			{
				ArchiveStageStopWatch.Stop();
				ReportAMUsageData(config);

				ReportGeneratorHelper.TryGenerateReport(this, descriptor, logger, schedule, config);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				var message = (NoResString)"Non-fatal error encountered during Finalise()";
				logger.LogAndReportError("CommonArchiveStageDescriptor.FinaliseException", descriptor.Code, message, e);
			}
		}

		protected virtual void ReportAMUsageData(IArchiveConfiguration config)
		{
			var valuesToReport = config.AMUsageReportValues.Select(kvpair => (kvpair.Key, kvpair.Value)).ToList();
			valuesToReport.AddRange(new (string, object)[]
			{
				(UsageProperties.ArchiveScheduleTotalRuntime, ArchiveStageStopWatch.Elapsed.Milliseconds),
				(UsageProperties.ArchiveStageName, Name),
				(UsageProperties.TotalRecordsDeletedFromAllTablesDuringArchiving, ProcessingInfoPerTable.Where(kvp => kvp.Value.Purged).Select(kvp => kvp.Value.Count).Sum()),
				(UsageProperties.TotalJobHeadersProcessedDuringArchiving, ReportGeneratorHelper.GetProcessedCountForTable(JobHeaderSchema.Constants.TableName, ProcessingInfoPerTable)),
				(UsageProperties.TotalDocumentsDeletedDuringArchiving, ReportGeneratorHelper.GetProcessedCountForTable(StorageDocsSchema.Constants.TableName, ProcessingInfoPerTable)),
			});
			UsageCollector.Report(UsageFeatures.Codes.ArchiveManager, valuesToReport.ToArray());
		}

		public virtual void OnArchiveSetProcessed(IArchiveSet set)
			=> OnArchiveSetProcessedHelpers.AddOrUpdateRecordsProcessedCount(set, ProcessingInfoPerTable, recordsPurged: true);

		#endregion
	}
}
