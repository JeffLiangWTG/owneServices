using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STACusUnderbondArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("C23C69BC-3E1C-4497-A879-D1FC9F0B738B", "Standalone Customs Underbond Archive");

		public override SchemaColumn MainArchivePKColumn
			=> CusUnderbondSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> CusUnderbondSchema.C4_MovementReason;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> CusUnderbondSchema.C4_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> CusUnderbondSchema.C4_ParentID;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> CustomsArchiveRelationships.SetupCusUnderbondRelationships(systemSetup);
	}
}
