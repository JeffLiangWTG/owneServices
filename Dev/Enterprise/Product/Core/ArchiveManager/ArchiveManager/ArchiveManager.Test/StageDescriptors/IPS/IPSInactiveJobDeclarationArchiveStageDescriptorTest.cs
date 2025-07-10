using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.IPS
{
	class IPSInactiveJobDeclarationArchiveStageDescriptorTest : IPSArchiveStageDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new IPSInactiveJobDeclarationArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Inactive Job Declaration Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> JobDeclarationSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> JobDeclarationSchema.JE_DeclarationReference;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = JobDeclarationSchema.JE_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedIsCancelledSchemaColumn
			=> JobDeclarationSchema.JE_IsCancelled;

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
			=> ExpectedMainArchiveFilterWithDeclarations;
	}
}
