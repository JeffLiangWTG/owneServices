using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.IPS
{
	class IPSInactiveConsolArchiveStageDescriptorTest : IPSArchiveStageDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new IPSInactiveConsolArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Inactive Consol Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> JobConsolSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> JobConsolSchema.JK_UniqueConsignRef;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = JobConsolSchema.JK_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedIsCancelledSchemaColumn
			=> JobConsolSchema.JK_IsCancelled;

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
		{
			get
			{
				var query = base.ExpectedMainArchiveFilterWithDeclarations;

				var sql = @"
NOT EXISTS
(
	SELECT 1 FROM dbo.HVLVOuterPackage
	WHERE HVO_JK_LoadedOnConsol = JK_PK
)
";

				return query.AddFilterAndZSQLParameterCollection(sql, null);
			}
		}

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
		{
			get
			{
				var sql = @"
NOT EXISTS
(
	SELECT 1 FROM dbo.CusSCAOceanBill 
	WHERE CB_ParentTableCode = 'JK'
	AND CB_ParentId = JK_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.CusCAeMHMaster 
	WHERE BP_ParentTableCode = 'JK'
	AND BP_ParentId = JK_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.AsycudaManifestHeader 
	WHERE AMA_ParentTableCode = 'JK'
	AND AMA_ParentId = JK_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.JobDeclaration
	JOIN dbo.JobShipment ON JE_JS = JS_PK
	JOIN dbo.JobConShipLink ON JN_JS = JS_PK
	WHERE JN_JK = JK_PK
)
";
				return ExpectedMainArchiveFilterWithDeclarations.AddFilterAndZSQLParameterCollection(sql, null);
			}
		}
	}
}
