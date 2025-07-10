using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.IPS
{
	class IPSInactiveJobCartageArchiveStageDescriptorTest : IPSArchiveStageDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new IPSInactiveJobCartageArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Inactive Job Cartage Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> JobCartageSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> JobCartageSchema.JJ_ConsignmentID;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = JobCartageSchema.JJ_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedIsCancelledSchemaColumn
			=> JobCartageSchema.JJ_IsCancelled;

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
			=> ExpectedMainArchiveFilterWithDeclarations;
	}
}
