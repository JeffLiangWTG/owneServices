using System;
using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Definitions;
using Enterprise.Client.APL;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		protected ClientOverride()
		{
		}

		public static ClientOverride Instance
		{
			get { return instance ?? (instance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride instance;

		protected override void InitialiseCore()
		{
			APLDocAWB.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.APL; }
		}

		public override string ClientDisplayName
		{
			get { return "All Ports International Logistics P/L"; }
		}

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new ExtensionObjects(TableCreationScripts, ViewAndRoutinesCreationScripts);

		static readonly ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts = ImmutableArray<DatabaseObjectCreateScript>.Empty;

		static readonly ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts = ImmutableArray.Create(
			new DatabaseViewAndRoutineCreateScript("ClientGetOrderRefs", GetOrderRefs, DropGetOrderRefs, DbRoutineType.SqlFunctionScalarTypeDesc),
			new DatabaseViewAndRoutineCreateScript("Client_sfn_APLCartageImportDeliveryReport", Client_sfn_APLCartageImportDeliveryReport, Drop_Client_sfn_APLCartageImportDeliveryReport, DbRoutineType.SqlFunctionInlineTypeDesc)
		);

		const string Drop_Client_sfn_APLCartageImportDeliveryReport = "DROP FUNCTION Client_sfn_APLCartageImportDeliveryReport";
		const string Client_sfn_APLCartageImportDeliveryReport = @"
								CREATE FUNCTION Client_sfn_APLCartageImportDeliveryReport
							(
								  @Country as char(5),
								  @Company uniqueidentifier,
								  @Delivered as char(3),
								  @JobType as varchar(30),
								  @CartageType as varchar(9)
							)
							RETURNS TABLE
							AS
							RETURN
							SELECT
							  JS_UniqueConsignRef AS Shipment,
							  JC_ContainerNum AS Container,

							  ConsigneeOrgAddress.OA_OH AS ConsigneePK,
							  Cne.OH_FullName AS ConsigneeName,
							  RTRIM(Cne.OH_Code) AS ConsigneeCode,
							  ConsignorOrgAddress.OA_OH as ConsignorPK,

							  JW_Vessel AS Vessel,
							  RV_PK     AS VesselPK,
							  JW_VoyageFlight AS Voyage,
							  JS_E_ARV AS ETA,
							  JS_RL_NKDestination AS Destination,
							  JS_HouseBill As HouseBill,
							  dbo.ClientGetOrderRefs(JP_PK, ISNULL(JS_PK, JE_PK)) AS OrderNo,
							  EQ_DateReceived AS OriginalBillReceived,

							  ST_NoteText As NoteText,

							  JP_DeliveryCartageCompleted AS DeliveredDate,
							  JP_DeliveryRequiredBy AS DeliveryReq,
							  RC_Code AS ContainerType,

							  Cartage.OH_FullName AS CartageName,
							  RTRIM(Cartage.OH_Code) AS CartageCode,

							  CASE
								  WHEN JC_ContainerMode IN ('FCL', 'BCN') THEN
									 (CASE WHEN JC_FCLAvailable IS NULL THEN JP_FCLAvailable
												 ELSE JC_FCLAvailable END)
								  ELSE
									 (CASE WHEN JC_LCLAvailable IS NULL THEN JP_LCLAvailable
												 ELSE JC_LCLAvailable END)  END AS AvailableFrom,
							  CASE
								  WHEN JC_ContainerMode IN ('FCL', 'BCN') THEN
									 (CASE WHEN JC_ArrivalCTOStorageStartDate IS NULL THEN JP_FCLStorageCommences
												 ELSE JC_ArrivalCTOStorageStartDate END)
								  ELSE
									 (CASE WHEN JC_LCLStorageCommences IS NULL THEN JP_LCLStorageCommences
												 ELSE JC_LCLStorageCommences END) END AS StorageStart,
							  CASE
								  WHEN JC_ContainerMode IN ('FCL', 'BCN') THEN
									 JC_ArrivalSlotDateTime END AS SlotDate,
							  CASE
								  WHEN JC_ArrivalEstimatedDelivery IS NULL THEN JP_EstimatedDelivery
								  ELSE JC_ArrivalEstimatedDelivery END AS EstDelivery,

							  Cartage.OH_PK AS CartagePK,

							  CASE
								  WHEN JC_ArrivalCartageAdvised IS NULL THEN JP_DeliveryCartageAdvised
								  ELSE JC_ArrivalCartageAdvised END AS CartageAdvised,
							  CASE
								   WHEN JC_ArrivalCartageComplete IS NULL THEN JP_DeliveryCartageCompleted
								   ELSE JC_ArrivalCartageComplete END AS CartageComplete

							FROM dbo.JobShipment
								  LEFT JOIN
										(
											  SELECT
													J6_JC,
													JL_JS
											  FROM
													dbo.JobPackLines
													INNER JOIN dbo.JobContainerPackPivot ON J6_JL = JL_PK
											  GROUP BY
													J6_JC,
													JL_JS
										) AS JSJC
										ON
										JSJC.JL_JS = JS_PK  AND (JS_PackingMode = 'FCL' OR (JS_PackingMode = 'BCN' AND JS_ShipmentType = 'BCN'))
							  LEFT JOIN dbo.JobDocAddress as ConsigneeDocAddress ON ConsigneeDocAddress.E2_ParentID = JS_PK and ConsigneeDocAddress.E2_AddressType = 'CED'
							  LEFT JOIN dbo.JobDocAddress as ConsignorDocAddress ON ConsignorDocAddress.E2_ParentID = JS_PK and ConsignorDocAddress.E2_AddressType = 'CRD'
							  LEFT JOIN dbo.OrgAddress as ConsigneeOrgAddress ON ConsigneeDocAddress.E2_OA_Address = ConsigneeOrgAddress.OA_PK
							  LEFT JOIN dbo.OrgAddress as ConsignorOrgAddress ON ConsignorDocAddress.E2_OA_Address = ConsignorOrgAddress.OA_PK
							  LEFT JOIN dbo.JobContainer ON (J6_JC = JC_PK AND JC_ContainerMode = 'FCL')
							  LEFT JOIN dbo.RefContainer ON JC_RC = RC_PK
							  LEFT JOIN dbo.OrgHeader as Cne ON ConsigneeOrgAddress.OA_OH = Cne.OH_PK
							  LEFT JOIN dbo.JobDocsAndCartage ON JS_PK = JP_ParentID
							  LEFT Join dbo.JobRequiredDocument ON JP_PK = EQ_ParentID AND EQ_DocType = 'OBL'
							  LEFT JOIN dbo.JobConShipLink ON JN_JS = JS_PK
							  LEFT JOIN dbo.JobConsol ON JN_JK = JK_PK
							  LEFT JOIN csfn_mainconsoltransport(@Country) MainTransport ON MainTransport.JW_JK = JobConsol.JK_PK
							  LEFT JOIN dbo.RefVessel ON RV_Code = MainTransport.JW_Vessel
							  LEFT JOIN dbo.OrgAddress As CartageAddress ON JP_OA_DeliveryCartageCoAddr = CartageAddress.OA_PK
							  LEFT JOIN dbo.OrgHeader AS Cartage ON CartageAddress.OA_OH = Cartage.OH_PK
							  LEFT JOIN dbo.JobDeclaration ON (JE_JS = JS_PK AND JE_MessageType != 'EXP')
							  LEFT JOIN dbo.StmNote ON (StmNote.ST_ParentID = JS_PK and StmNote.ST_Description = 'Cartage History Notes')

							WHERE
							  left(JS_RL_NKDestination, 2) = @Country
							  AND JS_IsForwardRegistered = 1
							  AND
								  (
										(
											  SELECT
													COUNT(*) AS JS_ColoadCount
											  FROM
													dbo.JobShipment JSSubs
											  WHERE
													JSSubs.JS_JS_ColoadMasterShipment = JobShipment.JS_PK
										) = 0 AND
										NOT JobShipment.JS_ShipmentType = 'CLD'
								  )
							  AND (@Delivered = 'All'
										OR (@Delivered = 'OPN' AND JP_DeliveryCartageCompleted IS NULL AND JC_ArrivalCartageComplete IS NULL)
										OR (@Delivered = 'DLV' AND (JP_DeliveryCartageCompleted IS NOT NULL OR JC_ArrivalCartageComplete IS NOT NULL))
										)

							  AND (@JobType = 'All' OR @JobType = '' OR (@JobType = 'Shipments'  AND JE_JS IS NULL)
										OR (@JobType = 'Declarations'  AND JE_JS IS NOT NULL)
									 )

							  AND (@CartageType = 'Container' AND (JS_PackingMode = 'FCL' OR (JS_ShipmentType = 'BCN' AND JS_PackingMode = 'BCN'))
											  OR (@CartageType = 'Other'  AND (JS_PackingMode != 'FCL' AND NOT (JS_ShipmentType = 'BCN' AND JS_PackingMode = 'BCN')))
										)

							UNION ALL

							SELECT
							  JE_DeclarationReference AS Shipment,
							  CO_ContainerNumber AS Container,

							  JE_OH_Importer AS ConsigneePK,
							  Import.OH_FullName AS ConsigneeName,
							  RTRIM(Import.OH_Code) AS ConsigneeCode,
							  JE_OH_Supplier as ConsignorPK,

							  JE_VesselName AS Vessel,
							  RV_PK AS VesselPK,
							  JE_VoyageFlightNo AS Voyage,
							  JE_DateAtFinalDestination AS ETA,
							  JE_RL_NKFinalDestination AS Destination,
							  JE_HouseBill AS HouseBill,
							  dbo.ClientGetOrderRefs(JP_PK, JE_PK) AS OrderNo,
							  EQ_DateReceived AS OriginalBillReceived,

							  ST_NoteText AS NoteText,

							  JP_DeliveryCartageCompleted AS DeliveredDate,
							  JP_DeliveryRequiredBy AS DeliveryReq,
							  RC_Code AS ContainerType,

							  Cartage.OH_FullName AS CartageName,
							  RTRIM(Cartage.OH_Code) AS CartageCode,

							  CASE
								  WHEN CO_FCL_LCL_AIR IN ('FCL', 'FCX') THEN
									 (CASE WHEN JC_FCLAvailable IS NULL THEN JP_FCLAvailable
												 ELSE JC_FCLAvailable END)
								  ELSE
									 (CASE WHEN JC_LCLAvailable IS NULL THEN JP_LCLAvailable
												 ELSE JC_LCLAvailable END) END AS AvailableFrom,
							  CASE
								  WHEN CO_FCL_LCL_AIR IN ('FCL', 'FCX') THEN
									 (CASE WHEN JC_ArrivalCTOStorageStartDate IS NULL THEN JP_FCLStorageCommences
												 ELSE JC_ArrivalCTOStorageStartDate END)
								  ELSE
									 (CASE WHEN JC_LCLStorageCommences IS NULL THEN JP_LCLStorageCommences
												 ELSE JC_LCLStorageCommences END) END AS StorageStart,
							  CASE
								  WHEN CO_FCL_LCL_AIR IN ('FCL', 'FCX') THEN JC_ArrivalSlotDateTime END AS SlotDate,
							  CASE
								  WHEN JC_ArrivalEstimatedDelivery IS NULL THEN JP_EstimatedDelivery
								  ELSE JC_ArrivalEstimatedDelivery END AS EstDelivery,

							  Cartage.OH_PK CartagePK,

							  CASE
								  WHEN JC_ArrivalCartageAdvised IS NULL THEN JP_DeliveryCartageAdvised
								  ELSE JC_ArrivalCartageAdvised END AS CartageAdvised,
							  CASE
								   WHEN JC_ArrivalCartageComplete IS NULL THEN JP_DeliveryCartageCompleted
								   ELSE JC_ArrivalCartageComplete END AS CartageComplete

							FROM dbo.JobDeclaration
							  LEFT JOIN dbo.CusContainer ON (CO_JE = JE_PK AND @CartageType = 'Container')
							  LEFT JOIN dbo.RefContainer ON CO_RC = RC_PK
							  LEFT JOIN dbo.JobDocsAndCartage ON JE_PK = JP_ParentID
							  LEFT Join dbo.JobRequiredDocument ON JP_PK = EQ_ParentID AND EQ_DocType = 'OBL'
							  LEFT JOIN dbo.JobContainer ON CO_JC = JC_PK
							  LEFT JOIN dbo.OrgHeader AS Import ON Import.OH_PK = JE_OH_Importer
							  LEFT JOIN dbo.OrgAddress As CartageAddress ON JP_OA_DeliveryCartageCoAddr = CartageAddress.OA_PK
							  LEFT JOIN dbo.OrgHeader AS Cartage ON CartageAddress.OA_OH = Cartage.OH_PK
							  LEFT JOIN dbo.RefVessel ON JE_VesselName = RV_Code
							  LEFT JOIN dbo.StmNote ON (StmNote.ST_ParentID = JE_PK and StmNote.ST_Description = 'Cartage History Notes')
							WHERE
							  left(JE_RL_NKFinalDestination, 2) = @Country
							  AND (@Delivered = 'All'
										OR (@Delivered = 'OPN' AND JP_DeliveryCartageCompleted IS NULL AND JC_ArrivalCartageComplete IS NULL)
										OR (@Delivered = 'DLV' AND (JP_DeliveryCartageCompleted IS NOT NULL OR JC_ArrivalCartageComplete IS NOT NULL))
										)

							  AND (JE_JS IS NULL AND @JobType != 'Shipments')

							  AND (@CartageType = 'Container' AND JE_ContainerMode IN ('FCL','FCX')
											  OR (@CartageType = 'Other' AND JE_ContainerMode  NOT IN ('FCL','FCX','BCN') )
							)";

		const string DropGetOrderRefs = "DROP FUNCTION ClientGetOrderRefs";
		const string GetOrderRefs = @"
										CREATE FUNCTION ClientGetOrderRefs(@JP_PK AS UNIQUEIDENTIFIER, @Parent AS UNIQUEIDENTIFIER)
										RETURNS VARCHAR(8000)
										AS
										BEGIN
											DECLARE @OrderNo VARCHAR(8000)

											SET @OrderNo = ''
											SELECT @OrderNo = @OrderNo + JT_OrderReference + ' ' FROM dbo.JobOrderItem WHERE JT_JP = @JP_PK

											IF (@OrderNo = '')
												SET @OrderNo = (SELECT Top 1 JD_OrderNumber FROM dbo.JobOrderHeader WHERE JD_JS = @Parent OR JD_JE = @Parent)

											RETURN RTRIM(@OrderNo)
										END";
	}
}
