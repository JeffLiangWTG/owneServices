using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STACusCAeMHHouseArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("9EF2C9C5-A868-4057-9E39-B289D7C0DDED", "Standalone Canadian Customs eManifest House Archive");

		public override SchemaColumn MainArchivePKColumn
			=> CusCAeMHHouseSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> CusCAeMHHouseSchema.BW_HouseBill;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> CusCAeMHHouseSchema.BW_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> CusCAeMHHouseSchema.BW_ParentID;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> CustomsArchiveRelationships.SetupCusCAeMHHouseRelationships(systemSetup);
	}
}
