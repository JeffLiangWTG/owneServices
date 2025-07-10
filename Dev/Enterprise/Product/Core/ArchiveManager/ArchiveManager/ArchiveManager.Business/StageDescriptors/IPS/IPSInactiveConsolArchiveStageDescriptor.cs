using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class IPSInactiveConsolArchiveStageDescriptor : IPSArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("244DC9C3-067D-417F-9BAE-4868606345AC", "Inactive Consol Archive");

		public override SchemaColumn MainArchivePKColumn
			=> JobConsolSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> JobConsolSchema.JK_UniqueConsignRef;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> JobConsolSchema.JK_SystemCreateTimeUtc;

		protected internal override SchemaBoolColumn IsCancelledSchemaColumn
			=> JobConsolSchema.JK_IsCancelled;

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet archiveSet, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			var imageAction = ObjectFactory.Get<IArchiveImageGenerationAction>();
			imageAction.Setup(logger, archiveSet, BusinessObjectProviderDictionary, cache);
			yield return imageAction;
		}

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = base.GetMainArchiveableFilter(config);

			var sql = @"
NOT EXISTS
(
	SELECT 1 FROM dbo.HVLVOuterPackage
	WHERE HVO_JK_LoadedOnConsol = JK_PK
)
";
			_ = query.AddFilterAndZSQLParameterCollection(sql, null);

			if (!config.ShouldIncludeDeclarations)
			{
				sql = @"
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
				_ = query.AddFilterAndZSQLParameterCollection(sql, null);
			}

			return query;
		}
	}
}
