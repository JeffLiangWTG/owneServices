using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STACusCAeMHMasterArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("33D46324-54CC-4F2F-ADE2-1A0F19A942A7", "Standalone Canadian Customs eManifest Master Archive");

		public override SchemaColumn MainArchivePKColumn
			=> CusCAeMHMasterSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> CusCAeMHMasterSchema.BP_MasterBill;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> CusCAeMHMasterSchema.BP_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> CusCAeMHMasterSchema.BP_ParentID;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> CustomsArchiveRelationships.SetupCusCAeManifestHeaderRelationships(systemSetup);
	}
}
