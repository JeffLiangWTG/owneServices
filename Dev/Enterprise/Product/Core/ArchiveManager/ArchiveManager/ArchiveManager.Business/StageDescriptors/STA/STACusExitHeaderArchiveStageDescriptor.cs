using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STACusExitHeaderArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("78BB7F03-7F24-4A76-9BD2-AA5B21CD8219", "Standalone Customs Exit Header Archive");

		public override SchemaColumn MainArchivePKColumn
			=> CusExitHeaderSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> CusExitHeaderSchema.CXH_JobReference;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> CusExitHeaderSchema.CXH_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> CusExitHeaderSchema.CXH_ParentID;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> CustomsArchiveRelationships.SetupCusExitHeaderRelationships(systemSetup);
	}
}
