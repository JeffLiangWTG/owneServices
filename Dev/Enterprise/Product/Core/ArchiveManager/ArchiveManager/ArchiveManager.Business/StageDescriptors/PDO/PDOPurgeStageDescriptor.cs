using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.ArchiveEligibility;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.StageDescriptors.PDO
{
	public class PDOPurgeStageDescriptor : CommonArchiveStageDescriptor
	{
		public override string Name
			=> Res.GetString("D9C58CE4-C215-47A4-ADC1-2309F09FB962", "Purge Documents of Operational Records");

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> JobHeaderSchema.JH_SystemCreateTimeUtc;

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = new ZQuery();
			var filters = GetArchiveableFilters(config);

			foreach (var filter in filters)
			{
				filter.ApplyTo(query);
			}

			return query;
		}

		IEnumerable<JobHeaderArchiveableFilter> GetArchiveableFilters(IArchiveConfiguration config)
		{
			var filters = GetCommonArchiveableFiltersForConfig(config, MainDateFilterColumn);
			var excludedFilters = new[]
			{
				JobHeaderArchiveableFilter.JobIsClosed,
				JobHeaderArchiveableFilter.NoAssociatedRateAttachments,
				JobHeaderArchiveableFilter.NoAssociatedJobShipments
			};

			return filters.Except(excludedFilters);
		}

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			var imageDeleteAction = ObjectFactory.Get<IArchiveImageDeletionAction>();
			imageDeleteAction.Setup(logger, set, cache, config);
			yield return imageDeleteAction;
		}

		public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
			=> Enumerable.Empty<IArchiveAction>();

		public override void OnArchiveSetProcessed(IArchiveSet set)
		{
			OnArchiveSetProcessedHelpers.AddOrUpdateDocumentsDeletedCount(set, ProcessingInfoPerTable);
			OnArchiveSetProcessedHelpers.AddOrUpdateRecordsProcessedCount(set, ProcessingInfoPerTable, recordsPurged: false);
		}
	}
}
