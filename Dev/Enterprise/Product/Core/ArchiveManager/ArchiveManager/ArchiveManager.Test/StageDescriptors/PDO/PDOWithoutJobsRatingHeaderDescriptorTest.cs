using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.PDO;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.PDO
{
	class PDOWithoutJobsRatingHeaderDescriptorTest : PDOWithoutJobsBaseDescriptorTest
	{
		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
		{
			get
			{
				var query = new ZDBOnlyQuery(ExpectedTypeToArchive);
				_ = query.AddToFilter(ExpectedMainArchiveDateFilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, TestDate);
				_ = query.AddToFilter(RatingHeaderSchema.TH_OneTimeQuote, 1);
				var subquery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, true);
				query.AddSubQuery(subquery, JoinCondition.And);
				query.OrderBy = $"{ExpectedMainArchiveDateFilterColumn.Name}, {ExpectedMainArchiveNKColumn.Name}, {ExpectedMainArchivePKColumn.Name}";
				return query;
			}
		}

		protected override IArchiveStageDescriptor StageDescriptor
			=> new PDOWithoutJobsRatingHeaderDescriptor();

		protected override string ExpectedName
			=> "Purge Documents of Operational Records without Jobs - Rating Header";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> RatingHeaderSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> RatingHeaderSchema.TH_QuoteNumber;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn
			=> RatingHeaderSchema.TH_SystemCreateTimeUtc;

		public override Type ExpectedTypeToArchive
			=> typeof(IRatingHeader);
	}
}
