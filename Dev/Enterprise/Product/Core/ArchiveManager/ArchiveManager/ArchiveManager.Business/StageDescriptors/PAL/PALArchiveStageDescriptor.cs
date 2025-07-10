using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.StageDescriptors
{
	public class PALArchiveStageDescriptor : CommonArchiveStageDescriptor
	{
		#region IArchiveStageDescriptor Members

		public override string Name
			=> Res.GetString("9E34EA58-EA28-4D17-8412-EE4C25695E09", "Purge Activity Logs");

		public override SchemaColumn MainArchivePKColumn
			=> StmActivityLogSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> null;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> StmActivityLogSchema.S7_OpenDateTimeUtc;

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var shipmentQuery = new ZDBOnlyQuery(typeof(StmActivityLog));
			_ = shipmentQuery.AddToFilter(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, config.ArchiveJobsOnOrBeforeThisDate);

			shipmentQuery.OrderBy = MainDateFilterColumn.Name;

			return shipmentQuery;
		}

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config) { }

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			return Enumerable.Empty<IArchivePreparationAction>();
		}

		public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
			=> Enumerable.Empty<IArchiveAction>();

		public override void OnArchiveSetProcessed(IArchiveSet set)
			=> OnArchiveSetProcessedHelpers.AddOrUpdateRecordsProcessedCount(set, ProcessingInfoPerTable);

		#endregion
	}
}
