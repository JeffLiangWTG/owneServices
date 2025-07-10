using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STACusIntrastatHeaderArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("68108976-FD2C-4568-A2A5-DB13C5467B46", "Standalone Customs Intrastat Header Archive");

		public override SchemaColumn MainArchivePKColumn
			=> CusIntrastatHeaderSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> null;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> CusIntrastatHeaderSchema.CIH_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> null;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> CustomsArchiveRelationships.SetupCusIntrastatHeaderRelationships(systemSetup);
	}
}
