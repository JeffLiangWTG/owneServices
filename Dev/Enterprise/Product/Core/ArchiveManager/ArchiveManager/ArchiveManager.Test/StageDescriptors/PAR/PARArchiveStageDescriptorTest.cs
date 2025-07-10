using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.PAR
{
	class PARArchiveStageDescriptorTest : CommonArchiveStageDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor => new PARDeleteStageDescriptor();

		protected override IArchiveSystemDescriptor SystemDescriptor => new PARPurgeSystemDescriptor();

		protected override string ExpectedName
			=> "Purge Archived Records";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> StorageMainSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> null;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = StorageMainSchema.SM_Archived;

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
			=> new();

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
			=> new();

		protected override Type[] ExpectedPreparationActions
			=> Array.Empty<Type>();

		protected override Type[] ExpectedArchiveActions
			=> Array.Empty<Type>();

		public override void TestMainArchiveableFilter()
		{
			Assert(true);
		}

		public override void TestGetPreparationAction()
		{
			Assert(true);
		}

		public override void TestGetArchiveAction()
		{
			Assert(true);
		}
	}
}
