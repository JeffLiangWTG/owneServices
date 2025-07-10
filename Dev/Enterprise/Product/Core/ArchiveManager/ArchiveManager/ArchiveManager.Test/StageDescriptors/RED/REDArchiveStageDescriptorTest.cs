using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Actions;
using Enterprise.ArchiveManager.Business.StageDescriptors;
using Enterprise.ArchiveManager.Business.SystemDescriptors;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.RED
{
	class REDArchiveStageDescriptorTest : CommonArchiveStageDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor => new REDArchiveStageDescriptor();
		protected override IArchiveSystemDescriptor SystemDescriptor => new REDArchiveSystemDescriptor();

		protected override string ExpectedName =>
			"Purge Expired Rates";

		protected override SchemaColumn ExpectedMainArchivePKColumn =>
			RateEntrySchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn =>
			null;

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
		{
			get
			{
				var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);

				var query = new ZDBOnlyQuery(typeof(RateEntry));
				query.AddToFilter(RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.LessThanOrEqualTo, config.ArchiveJobsOnOrBeforeThisDate);

				var subquery = new ZDBOnlySubQuery(typeof(RatingHeader), RatingHeaderSchema.PK);
				subquery.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.ClientRate);
				subquery.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Tariff);
				subquery.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Costing);
				subquery.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.IntercompanyTariff);
				query.AddSubQuery(RateEntrySchema.TI_TH, subquery, JoinCondition.And);

				return query;
			}
		}

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations =>
			ExpectedMainArchiveFilterWithoutDeclarations;

		protected override Type[] ExpectedPreparationActions => new Type[] { typeof(ArchiveImageDeletionAction) };

		protected override Type[] ExpectedArchiveActions => Array.Empty<Type>();

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = RateEntrySchema.TI_SystemCreateTimeUtc;

		[TestDate(2023, 01, 01, 12, 0, 0, 0)]
		public override void TestMainArchiveableFilter()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);
			_config = config;
			var mainArchiveableFilter = StageDescriptor.GetMainArchiveableFilter(config).LiteralTextSqlFormatted;
			AssertEquals(ExpectedMainArchiveFilterWithoutDeclarations.LiteralTextSqlFormatted, mainArchiveableFilter);

			config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			_config = config;
			mainArchiveableFilter = StageDescriptor.GetMainArchiveableFilter(config).LiteralTextSqlFormatted;
			AssertEquals(ExpectedMainArchiveFilterWithDeclarations.LiteralTextSqlFormatted, mainArchiveableFilter);
		}
	}
}
