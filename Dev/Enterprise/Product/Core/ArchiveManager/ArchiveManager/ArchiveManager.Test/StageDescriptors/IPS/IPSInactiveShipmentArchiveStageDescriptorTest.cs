using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.IPS
{
	class IPSInactiveShipmentArchiveStageDescriptorTest : IPSArchiveStageDescriptorTest
	{
		protected override IArchiveStageDescriptor StageDescriptor
			=> new IPSInactiveShipmentArchiveStageDescriptor();

		protected override string ExpectedName
			=> "Inactive Shipment Archive";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> JobShipmentSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> JobShipmentSchema.JS_UniqueConsignRef;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = JobShipmentSchema.JS_SystemCreateTimeUtc;

		protected override SchemaColumn ExpectedIsCancelledSchemaColumn
			=> JobShipmentSchema.JS_IsCancelled;

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
		{
			get
			{
				var query = base.ExpectedMainArchiveFilterWithDeclarations;

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
				return ExpectedMainArchiveFilterWithDeclarations.AddFilterAndZSQLParameterCollection(sql, null);
			}
		}
	}
}
