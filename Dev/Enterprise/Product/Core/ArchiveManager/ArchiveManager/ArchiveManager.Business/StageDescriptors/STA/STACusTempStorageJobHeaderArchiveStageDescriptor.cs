using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STACusTempStorageJobHeaderArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("580C7C9B-EA41-4CE9-9BAC-8E7FE6A59B2D", "Standalone Customs Temporary Storage Job Header Archive");

		public override SchemaColumn MainArchivePKColumn
			=> CusTempStorageJobHeaderSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> CusTempStorageJobHeaderSchema.SJH_JobReference;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> CusTempStorageJobHeaderSchema.SJH_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> null;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> CustomsArchiveRelationships.SetupCusTempStorageRelationships(systemSetup);
	}
}
