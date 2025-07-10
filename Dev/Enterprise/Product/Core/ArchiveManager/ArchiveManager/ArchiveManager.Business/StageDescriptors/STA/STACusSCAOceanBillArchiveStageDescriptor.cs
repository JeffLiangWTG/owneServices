using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STACusSCAOceanBillArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("1E03CBDD-08CB-4249-8E59-082AB3CAED3C", "Standalone Customs Sea Cargo Ocean Bill Archive");

		public override SchemaColumn MainArchivePKColumn
			=> CusSCAOceanBillSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> CusSCAOceanBillSchema.CB_MasterHouseBill;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> CusSCAOceanBillSchema.CB_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> CusSCAOceanBillSchema.CB_ParentId;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> CustomsArchiveRelationships.SetupSeaCargoRelationships(systemSetup);
	}
}
