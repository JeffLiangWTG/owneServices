using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.IPS
{
	class IPSInactiveRatingHeaderArchiveStageDescriptorTest : IPSArchiveStageDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new IPSInactiveRatingHeaderArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Inactive Rating Header Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> RatingHeaderSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> RatingHeaderSchema.TH_QuoteNumber;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = RatingHeaderSchema.TH_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedIsCancelledSchemaColumn
			=> RatingHeaderSchema.TH_IsCancelled;

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
		{
			get
			{
				var query = base.ExpectedMainArchiveFilterWithDeclarations;
				var sql = @"NOT EXISTS
(
	SELECT TA_TH FROM dbo.RateAttachment
	WHERE TA_TH = TH_PK
)";
				_ = query.AddFilterAndZSQLParameterCollection(sql, null);

				return query;
			}
		}

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
			=> ExpectedMainArchiveFilterWithDeclarations;
	}
}
