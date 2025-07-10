using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STACusInBondHeaderArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("C88DB7DB-44BC-4321-BB98-1C8F8C99A96D", "Standalone Customs In-Bond Header Archive");

		public override SchemaColumn MainArchivePKColumn
			=> CusInBondHeaderSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> CusInBondHeaderSchema.BH_VoyageNumber;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> CusInBondHeaderSchema.BH_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> CusInBondHeaderSchema.BH_ParentID;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> CustomsArchiveRelationships.SetupCusInBondHeaderRelationships(systemSetup);
	}
}
