using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.StageDescriptors
{
	class REDArchiveStageDescriptor : CommonArchiveStageDescriptor
	{
		public override string Name
			=> Res.GetString("4224AEA7-655C-4A2D-BE6C-AC02BE4A88FB", "Purge Expired Rates");

		public override SchemaColumn MainArchivePKColumn
			=> RateEntrySchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> null;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> RateEntrySchema.TI_SystemCreateTimeUtc;

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = new ZDBOnlyQuery(typeof(RateEntry));
			query.AddToFilter(RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.LessThanOrEqualTo ,config.ArchiveJobsOnOrBeforeThisDate);

			var subquery = new ZDBOnlySubQuery(typeof(RatingHeader), RatingHeaderSchema.PK);
			subquery.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.ClientRate);
			subquery.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Tariff);
			subquery.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Costing);
			subquery.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.IntercompanyTariff);
			query.AddSubQuery(RateEntrySchema.TI_TH, subquery, JoinCondition.And);

			return query;
		}

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config) { }

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			var imageDeleteAction = ObjectFactory.Get<IArchiveImageDeletionAction>();
			imageDeleteAction.Setup(logger, set, cache, config);
			yield return imageDeleteAction;
		}

		public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
			=> Enumerable.Empty<IArchiveAction>();

		public override void OnArchiveSetProcessed(IArchiveSet set)
			=> OnArchiveSetProcessedHelpers.AddOrUpdateRecordsProcessedCount(set, ProcessingInfoPerTable);
	}
}
