using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.StageDescriptors.STA;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class STAHVLVOriginLoadListArchiveStageDescriptor : STAArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("615FCAF6-8D85-4D3D-A2DD-605B16537F35", "Standalone HVLV Origin Load List Archive");

		public override SchemaColumn MainArchivePKColumn
			=> HVLVOriginLoadListSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> HVLVOriginLoadListSchema.HVL_UniqueReference;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> HVLVOriginLoadListSchema.HVL_SystemCreateTimeUtc;

		public override SchemaColumn ParentIDColumn
			=> null;

		public override void SetupArchiveRelationships(IArchiveSystemSetup systemSetup, IArchiveConfiguration config)
			=> ArchiveRelationships.SetUpHVLVOriginLoadListRelationships(systemSetup);

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = base.GetMainArchiveableFilter(config);

			var sql = @"
NOT EXISTS
(
	SELECT 1 FROM dbo.HVLVItem
	WHERE HVI_HVL_LoadList = HVL_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.HVLVOuterPackage
	JOIN dbo.HVLVItem on HVI_HVO_OuterPackage = HVO_PK
	WHERE dbo.HVLVOuterPackage.HVO_HVL_LoadList = HVL_PK
)
";
			_ = query.AddFilterAndZSQLParameterCollection(sql, null);

			return query;
		}
	}
}
