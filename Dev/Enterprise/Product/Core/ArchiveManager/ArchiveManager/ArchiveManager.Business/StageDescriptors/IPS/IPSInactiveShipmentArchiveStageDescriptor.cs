using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Business
{
	class IPSInactiveShipmentArchiveStageDescriptor : IPSArchiveDescriptor
	{
		public override string Name
			=> Res.GetString("F2744AF6-3FE1-41D2-B69B-9E9796143247", "Inactive Shipment Archive");

		public override SchemaColumn MainArchivePKColumn
			=> JobShipmentSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> JobShipmentSchema.JS_UniqueConsignRef;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> JobShipmentSchema.JS_SystemCreateTimeUtc;

		protected internal override SchemaBoolColumn IsCancelledSchemaColumn
			=> JobShipmentSchema.JS_IsCancelled;

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = base.GetMainArchiveableFilter(config);

			var sql = @"
NOT EXISTS
(
	SELECT 1 FROM dbo.SupplierBookingLine
	WHERE DL_JS_ApprovedShipment = JS_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.ShipmentProfitShares
	WHERE PSS_JS = JS_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.HVLVConsignmentHeader
	WHERE HCH_JS_Shipment = JS_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.HVLVItem
	WHERE HVI_JS_LoadedOnShipment = JS_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.ELoadList
	WHERE DO_JS_MasterHouseShipment = JS_PK
)
AND NOT EXISTS
(
	SELECT 1 from dbo.RateAttachment
	JOIN dbo.RatingHeader ON TH_PK = TA_TH
	WHERE JS_TH_OneTimeQuote = TH_PK
)
";
			_ = query.AddFilterAndZSQLParameterCollection(sql, null);

			if (!config.ShouldIncludeDeclarations)
			{
				sql = @"
NOT EXISTS
(
	SELECT 1 FROM dbo.JobDeclaration
	WHERE JE_JS = JS_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.CusHAWB
	WHERE CS_JS = JS_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.CusSCAHouse 
	WHERE CA_JS = JS_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.CusSCAOceanBill 
	JOIN dbo.JobConShipLink on CB_ParentTableCode = 'JK' AND CB_ParentId = JN_JK
	WHERE JN_JS = JS_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.CusCAeMHMaster 
	JOIN dbo.JobConShipLink on BP_ParentTableCode = 'JK' AND BP_ParentId = JN_JK
	WHERE JN_JS = JS_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.AsycudaManifestHeader 
	JOIN dbo.JobConShipLink on AMA_ParentTableCode = 'JK' AND AMA_ParentId = JN_JK
	WHERE JN_JS = JS_PK

)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.CusSCADepotHouse
	WHERE CX_JS = JS_PK
)
AND NOT EXISTS
(
	SELECT 1 FROM dbo.CusDecHouseBill
	WHERE CU_JS = JS_PK
)
AND NOT EXISTS
(
	SELECT 1
	FROM dbo.CusOutturn
	WHERE C5_ParentTableCode = 'JS' AND C5_ParentID = JS_PK
)
";
				_ = query.AddFilterAndZSQLParameterCollection(sql, null);
			}

			return query;
		}

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet archiveSet, IArchiveSystemCache cache, IArchiveConfiguration config)
		{
			var imageAction = ObjectFactory.Get<IArchiveImageGenerationAction>();
			imageAction.Setup(logger, archiveSet, BusinessObjectProviderDictionary, cache);
			yield return imageAction;
		}
	}
}
