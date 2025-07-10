using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business.StageDescriptors.PDO
{
	public class PDOWithoutJobsRatingHeaderDescriptor : PDOWithoutJobsBaseDescriptor
	{
		public override string Name
			=> Res.GetString("30C4CC4A-D504-487D-A41D-C4E1C75FD1EA", "Purge Documents of Operational Records without Jobs - Rating Header");

		public override SchemaColumn MainArchivePKColumn
			=> RatingHeaderSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> RatingHeaderSchema.TH_QuoteNumber;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> RatingHeaderSchema.TH_SystemCreateTimeUtc;

		public override Type TypeToArchive
			=> typeof(IRatingHeader);

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
		{
			ArchiveRelationships.SetupArchiveRelationshipsForPDORatingStage(systemSetup, config);
		}

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = new ZDBOnlyQuery(TypeToArchive);
			_ = query.AddToFilter(MainDateFilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, config.ArchiveJobsOnOrBeforeThisDate);
			_ = query.AddToFilter(RatingHeaderSchema.TH_OneTimeQuote, 1);
			var subquery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, true);
			query.AddSubQuery(subquery, JoinCondition.And);
			query.OrderBy = $"{MainDateFilterColumn.Name}, {MainArchiveNKColumn.Name}, {MainArchivePKColumn.Name}";
			return query;
		}
	}
}
