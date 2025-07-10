using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STACusTempStorageRegHeaderArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("B4F3B8FA-1FAA-487A-9703-E6ECB03B7EB8", "Standalone Customs Temporary Storage Register Header Archive");

		public override SchemaColumn MainArchivePKColumn
			=> CusTempStorageRegHeaderSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> CusTempStorageRegHeaderSchema.SRH_Reference;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> CusTempStorageRegHeaderSchema.SRH_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> null;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> CustomsArchiveRelationships.SetupCusTempStorageRegHeaderRelationships(systemSetup);
	}
}
