using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.STA
{
	class STAHVLVOriginLoadListArchiveStageDescriptorTest : STAArchiveDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new STAHVLVOriginLoadListArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Standalone HVLV Origin Load List Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> HVLVOriginLoadListSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> HVLVOriginLoadListSchema.HVL_UniqueReference;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = HVLVOriginLoadListSchema.HVL_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedParentIDColumn
			=> null;

		protected override List<ArchiveRelationshipForSTATest> ExpectedRelationships
			=> new()
			{
				new ArchiveRelationshipForSTATest(HVLVOriginLoadListSchema.Constants.TableName, HVLVOriginLoadListSchema.PK, HVLVOuterPackageSchema.Constants.TableName, HVLVOuterPackageSchema.HVO_HVL_LoadList, isReversed: false),
			};

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
		{
			get
			{
				var query = base.ExpectedMainArchiveFilterWithoutDeclarations;
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
				return query.AddFilterAndZSQLParameterCollection(sql, null);
			}
		}
	}
}
