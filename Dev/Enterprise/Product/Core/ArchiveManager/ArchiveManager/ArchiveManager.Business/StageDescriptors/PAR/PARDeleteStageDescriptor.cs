using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	public class PARDeleteStageDescriptor : CommonArchiveStageDescriptor
	{
		public override string Name
			=> Res.GetString("FA8162F0-EABE-49D2-9E74-6B0350C8FEFF", "Purge Archived Records");

		public override SchemaColumn MainArchivePKColumn
			=> StorageMainSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> null;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> StorageMainSchema.SM_Archived;

		public override bool IsStageUsingTempTables
			=> false;
	}
}
