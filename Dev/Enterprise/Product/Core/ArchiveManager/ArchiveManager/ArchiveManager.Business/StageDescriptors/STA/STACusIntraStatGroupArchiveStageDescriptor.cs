using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STACusIntraStatGroupArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("B2DA5F6E-241B-4464-AA06-BAA7D4690912", "Standalone Customs Intrastat Group Archive"); 

		public override SchemaColumn MainArchivePKColumn
			=> CusIntrastatGroupSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> null;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> CusIntrastatGroupSchema.CIG_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> null;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> CustomsArchiveRelationships.SetupCusIntrastatGroupRelationships(systemSetup);
	}
}
