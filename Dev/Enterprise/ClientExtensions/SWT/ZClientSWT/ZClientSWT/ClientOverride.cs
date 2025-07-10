using System;
using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Definitions;
using Enterprise.Client.SWT;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		protected ClientOverride()
		{
		}

		#region Instance
		public static ClientOverride Instance
		{
			get { return instance ?? (instance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride instance;
		#endregion

		#region IClientHook Members

		public override Clients Client
		{
			get { return Clients.SWT; }
		}

		public override string ClientDisplayName
		{
			get { return "Savi World Transport Pty Ltd"; }
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return SWTDataRegistry.Instance; }
		}

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			ClientOverrideModuleIdentifier orderID = new ClientOverrideModuleIdentifier(ModuleIDs.Orders);
			ClientOverrideModuleInfo orderModuleInfo = new ClientOverrideModuleInfo(
				orderID, typeof(SWTOrderModuleOverride).Assembly.FullName,
				typeof(SWTOrderModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(orderModuleInfo);
			return moduleOverrides;
		}

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new ExtensionObjects(TableCreationScripts, ViewAndRoutinesCreationScripts);

		#region DatabaseObjectCreationScripts

		static readonly ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts = ImmutableArray<DatabaseObjectCreateScript>.Empty;

		static readonly ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts = ImmutableArray.Create(
			new DatabaseViewAndRoutineCreateScript("ClientContainerCountForShipment", ClientContainerCountForShipment, DropClientContainerCountForShipment, DbRoutineType.SqlFunctionScalarTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientGetOrderRefs", ClientGetOrderRefs, DropClientGetOrderRefs, DbRoutineType.SqlFunctionScalarTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientGetSuppliersList", ClientGetSuppliersList, DropClientGetSuppliersList, DbRoutineType.SqlFunctionScalarTypeDesc),
			new DatabaseViewAndRoutineCreateScript("Client_SWT_NotYetArrivedReport", Create_Client_SWT_NotYetArrivedReport, Drop_Client_SWT_NotYetArrivedReport, DbRoutineType.SqlProcedureTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientOrgsForNotYetArrived", ClientOrgsForNotYetArrived, DropClientOrgsForNotYetArrived, DbRoutineType.SqlFunctionInlineTypeDesc),

			new DatabaseViewAndRoutineCreateScript("ClientViewChargeCodesByPort", ClientViewChargeCodesByPort, DropClientViewChargeCodesByPort, DbRoutineType.SqlViewTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientFuncOrderedChargeCodes", ClientFuncOrderedChargeCodes, DropClientFuncOrderedChargeCodes, DbRoutineType.SqlFunctionInlineTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientViewChargeCodesWithRowNumbers", ClientViewChargeCodesWithRowNumbers, DropClientViewChargeCodesWithRowNumbers, DbRoutineType.SqlFunctionInlineTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientFunctionPerContainerUnitCalcRates", ClientFunctionPerContainerUnitCalcRates, DropClientFunctionPerContainerUnitCalcRates, DbRoutineType.SqlFunctionInlineTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientProcAgentDeclaredRates", ClientProcAgentDeclaredRates, DropClientProcAgentDeclaredRates, DbRoutineType.SqlProcedureTypeDesc)
		);

		#region ClientGetSuppliersList

		const string DropClientGetSuppliersList = @"DROP FUNCTION ClientGetSuppliersList";
		const string ClientGetSuppliersList = @"

CREATE FUNCTION ClientGetSuppliersList
(
	@ConsolPK  uniqueidentifier,
	@UseConsignor char(1)
)
RETURNS VARCHAR(4000)
AS
BEGIN
	DECLARE @OrgName VARCHAR(100)
	DECLARE @Result VARCHAR(4000)

	DECLARE SupplierNames CURSOR READ_ONLY FOR

	SELECT DISTINCT (CASE WHEN @UseConsignor = 'Y' THEN JS_E2_OA_OH_ConsignorFullName ELSE JS_E2_OA_OH_ConsigneeFullName END)
	FROM dbo.JobConsol
	JOIN dbo.JobConShipLink on JN_JK = @ConsolPK
	JOIN dbo.cvw_JobShipmentOrgs Orgs on Orgs.JS_PK = JN_JS

	OPEN SupplierNames

	SET @OrgName = ''
	SET @Result = ''

	FETCH NEXT FROM SupplierNames INTO @OrgName

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @Result = @Result +  ' ' + @OrgName + ','
		FETCH NEXT FROM SupplierNames INTO @OrgName
	END

	close SupplierNames;
	DEALLOCATE SupplierNames;

	IF @Result <> '' AND LEN(@Result) > 2
	BEGIN
		SET @Result = SUBSTRING(@Result, 2, len(@Result) - 2 );
	END

	RETURN @Result
END
		";

		#endregion

		#region ClientContainerCountForShipment

		const string DropClientContainerCountForShipment = "DROP FUNCTION ClientContainerCountForShipment";
		const string ClientContainerCountForShipment = @"
			CREATE FUNCTION ClientContainerCountForShipment(@ShipmentPK AS uniqueidentifier, @ConsolPK AS uniqueidentifier)
			RETURNS varchar(8000)
			AS
			BEGIN
				DECLARE @ContainerList varchar(8000); SET @ContainerList = ''

				IF @ShipmentPK IS NULL AND @ConsolPK IS NOT NULL
				BEGIN
						SELECT  @ContainerList = @ContainerList + CONVERT(varchar(5), Count(*))  + 'x '  +  SUBSTRING(RC_Code,1, 2)  + 'FT ' + SUBSTRING(RC_Code,3, 2) + ' + '
						FROM dbo.JobContainer
						JOIN dbo.Refcontainer on JC_RC = RC_PK
						WHERE JC_JK = @ConsolPK
						GROUP BY RC_Code
				END

				ELSE IF @ConsolPK IS NULL AND @ShipmentPK IS NOT NULL
				BEGIN
						SELECT
							@ContainerList = @ContainerList
							+ convert(varchar(5), sum(RCCount)) + 'x '
							+ SUBSTRING(RCCode, 1, 2) + 'FT '
							+ SUBSTRING(RCCode, 3, 2)
							+ ' + '
						FROM
						(
							SELECT
								COUNT(*) AS RCCount,
								RC_Code AS RCCode
							FROM
								dbo.JobPackLines
								LEFT JOIN dbo.JobContainerPackPivot on J6_JL = JL_PK
								LEFT JOIN dbo.JobContainer on J6_JC = JC_PK
								LEFT JOIN dbo.RefContainer on JC_RC = RC_PK
							WHERE
								JL_JS = @ShipmentPk
								AND JL_FreightMode = 'OUT'
							GROUP BY
								RC_Code
						) InnerSelect
						GROUP BY
							RCCode
				END

				IF (len(@ContainerList) > 0) SET @ContainerList = left(@ContainerList, len(@ContainerList) - 2)

				RETURN @ContainerList
			END
";

		#endregion

		#region ClientGetOrderRefs

		const string DropClientGetOrderRefs = "DROP FUNCTION ClientGetOrderRefs";
		const string ClientGetOrderRefs = @"

CREATE FUNCTION ClientGetOrderRefs
(
	@ShipmentPK  uniqueidentifier,
	@ConsolPK uniqueidentifier
)
RETURNS VARCHAR(5000)
AS
BEGIN

DECLARE @OrderNos VARCHAR(5000) = ''
DECLARE @JobDocsPKLists VARCHAR(8000) = ''
DECLARE @ShipmentPKLists VARCHAR(8000) = ''
DECLARE @OrderNo VARCHAR(100) = ''

IF @ShipmentPK IS NOT NULL AND @ConsolPK IS NULL
	BEGIN
		SELECT  @JobDocsPKLists = '''' + CAST(JP_PK as varchar(50)) + ''''
		FROM dbo.JobDocsAndCartage
		JOIN dbo.Jobshipment  on  JP_ParentTableCode = 'JS' AND JP_ParentID = Jobshipment.JS_PK
		WHERE JobShipment.JS_PK = @ShipmentPK
	END
ELSE IF @ConsolPK IS NOT NULL AND @ShipmentPK IS NULL
	BEGIN
		SELECT  @JobDocsPKLists = @JobDocsPKLists + ',''' + CAST(JP_PK as varchar(50)) + ''''
		FROM dbo.JobDocsAndCartage
		JOIN dbo.JobConshipLink ON JN_JS = JP_ParentID AND JP_ParentTableCode = 'JS'
		WHERE JN_JK = @ConsolPK
	END

DECLARE OrderItemFromJobDocs CURSOR READ_ONLY FOR
SELECT distinct RTRIM(JT_OrderReference )
FROM dbo.JobOrderItem
WHERE CHARINDEX( CAST(JT_JP AS VARCHAR(50)), @JobDocsPKLists) > 0

OPEN OrderItemFromJobDocs

	FETCH NEXT FROM OrderItemFromJobDocs INTO @OrderNo

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @OrderNos = @OrderNos + ',' + @OrderNo
		FETCH NEXT FROM OrderItemFromJobDocs INTO @OrderNo
	END

CLOSE OrderItemFromJobDocs;
DEALLOCATE OrderItemFromJobDocs;

IF @ShipmentPK IS NOT NULL AND @ConsolPK IS NULL
	BEGIN
		SELECT  @ShipmentPKLists = '''' + CAST(JS_PK as varchar(50)) + ''''
		FROM dbo.JobShipment
		WHERE JobShipment.JS_PK = @ShipmentPK
	END
ELSE  IF  @ConsolPK IS NOT NULL AND @ShipmentPK IS NULL
	BEGIN
		SELECT  @ShipmentPKLists = @ShipmentPKLists + ',''' + CAST(JS_PK as varchar(50)) + ''''
		FROM dbo.JobShipment
		JOIN dbo.JobConShipLink on JN_JS = JS_PK
		WHERE JobConShipLink.JN_JK = @ConsolPK
	END

DECLARE OrderItemFromOrderHeaders CURSOR READ_ONLY FOR
SELECT distinct RTRIM(JD_OrderNumber )
FROM dbo.JobOrderHeader
WHERE CHARINDEX( CAST(JD_JS AS VARCHAR(50)), @ShipmentPKLists) > 0

OPEN OrderItemFromOrderHeaders

	FETCH NEXT FROM OrderItemFromOrderHeaders INTO @OrderNo

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @OrderNos = @OrderNos + ',' + @OrderNo
		FETCH NEXT FROM OrderItemFromOrderHeaders INTO @OrderNo
	END

CLOSE OrderItemFromOrderHeaders;
DEALLOCATE OrderItemFromOrderHeaders;

IF @OrderNos != ''
BEGIN
	SET @OrderNos = SUBSTRING(@OrderNos, 2, len(@OrderNos)-1);
END

RETURN UPPER(@OrderNos)

END

";

		#endregion

		#region Client_SWT_NotYetArrivedReport

		const string Drop_Client_SWT_NotYetArrivedReport = "drop procedure Client_SWT_NotYetArrivedReport";
		const string Create_Client_SWT_NotYetArrivedReport = @"
CREATE procedure Client_SWT_NotYetArrivedReport
				@ImporterPK AS uniqueidentifier,
				@SupplierPK AS uniqueidentifier,
				@CountryCode AS char(2),
				@Today AS datetime
			   as SELECT
                Shipment.JS_UniqueConsignRef,
                (CASE WHEN FirstVessel.JW_Vessel <> LastVessel.JW_Vessel THEN
						(CASE WHEN LastVessel.JW_ATD IS NULL THEN
							FirstVessel.JW_Vessel + '/' + LastVessel.JW_Vessel
						ELSE LastVessel.JW_Vessel END)
					ELSE FirstVessel.JW_Vessel
                END) AS Vessel
                , JS_RL_NKDestination AS Discharge
                , LastVessel.JW_TA AS CurrentETA
                ,(CASE WHEN @ImporterPK is NULL
							THEN  JS_E2_OA_OH_ConsigneeFullName
							ELSE  JS_E2_OA_OH_ConsignorFullName
					END) AS Org
                , dbo.ClientGetOrderRefs(Shipment.JS_PK, NULL) AS OrderNumbers
                , (CASE JS_PackingMode
						WHEN 'LCL' THEN 'LCL Freight'
						WHEN 'BBK' THEN 'Break Bulk'
						WHEN 'BLK' THEN 'Break Bulk'
					    ELSE dbo.ClientContainerCountForShipment(Shipment.JS_PK, NULL)
					END
				) AS ContainerDetails

            FROM
	            dbo.JobConShipLink
	            JOIN dbo.JobShipment AS Shipment ON JN_JS = Shipment.JS_PK and JS_IsCancelled = 0
	            JOIN dbo.JobConsol AS Consol ON JN_JK = Consol.JK_PK
                LEFT JOIN dbo.cvw_JobShipmentOrgs AS Orgs ON Orgs.JS_PK = Shipment.JS_PK
                JOIN
				(
					SELECT
						JW_JK				= JW_ParentGUID,
						JW_RL_NKLoadPort,
						JW_Vessel,
						JW_ATD,
						JW_TA				= ISNULL(JW_ATA, JW_ETA)
					FROM
						dbo.JobConsolTransport
					WHERE
						JW_ParentType = 'CON'
				) AS FirstVessel ON FirstVessel.JW_JK = Consol.JK_PK AND Consol.JK_RL_NKLoadPort = FirstVessel.JW_RL_NKLoadPort
                JOIN
				(
					SELECT
						JW_JK				= JW_ParentGUID,
						JW_RL_NKLoadPort,
						JW_RL_NKDiscPort,
						JW_Vessel,
						JW_ATD,
						JW_TA				= ISNULL(JW_ATA, JW_ETA)
					FROM
						dbo.JobConsolTransport
					WHERE
						JW_ParentType = 'CON'
				) AS LastVessel ON LastVessel.JW_JK = Consol.JK_PK AND Consol.JK_RL_NKDischargePort = LastVessel.JW_RL_NKDiscPort

            WHERE
                ((Orgs.JS_E2_OA_OH_Consignee = @ImporterPK AND @SupplierPK is Null)
                OR (Orgs.JS_E2_OA_OH_Consignor = @SupplierPK AND @ImporterPK is Null))
                AND Shipment.JS_PK in (Select JN_JS from dbo.JobConShipLink)
                AND LastVessel.JW_TA >= @Today
				and FirstVessel.JW_ATD <= @Today
                AND FirstVessel.JW_ATD IS NOT NULL
--                AND JS_E_DEP <= @Today
--                AND JS_E_ARV >= @Today
                AND Shipment.JS_TransportMode = 'SEA'
				AND JK_ConsolMode != 'BCN'
				AND left(JK_RL_NKLoadPort, 2) != @CountryCode

			UNION ALL SELECT
                Shipment.JS_UniqueConsignRef,
                (CASE WHEN FirstVessel.JW_Vessel <> LastVessel.JW_Vessel THEN
						(CASE WHEN LastVessel.JW_ATD IS NULL THEN
							FirstVessel.JW_Vessel + '/' + LastVessel.JW_Vessel
						ELSE LastVessel.JW_Vessel END)
					ELSE FirstVessel.JW_Vessel
                END) AS Vessel
                , JS_RL_NKDestination AS Discharge
                , LastVessel.JW_TA AS CurrentETA
                ,(CASE WHEN @ImporterPK is NULL
							THEN  dbo.ClientGetSuppliersList(Consol.JK_PK, 'N')
							ELSE  dbo.ClientGetSuppliersList(Consol.JK_PK, 'Y')
					END) AS Org
                , dbo.ClientGetOrderRefs(NULL, Consol.JK_PK) AS OrderNumbers
                , (CASE JS_PackingMode
						WHEN 'LCL' THEN 'LCL Freight'
						WHEN 'BBK' THEN 'Break Bulk'
						WHEN 'BLK' THEN 'Break Bulk'
					    ELSE dbo.ClientContainerCountForShipment(NULL, Consol.JK_PK)
					END
				) AS ContainerDetails

            FROM
	            dbo.JobConsol Consol
				 LEFT JOIN dbo.Jobconshiplink on JN_PK =
				 (
				  SELECT TOP 1 JobConShipLink1.JN_PK
				  FROM dbo.JobConShipLink AS JobConShipLink1
				  WHERE JobConShipLink1.JN_JK = JK_PK
				  ORDER BY JobConShipLink1.JN_PK
				 )
				JOIN dbo.Jobshipment AS Shipment ON JN_JS = Shipment.JS_PK
	            LEFT JOIN dbo.cvw_JobShipmentOrgs AS Orgs ON Orgs.JS_PK = Shipment.JS_PK and JS_IsCancelled = 0
                JOIN
				(
					SELECT
						JW_JK				= JW_ParentGUID,
						JW_RL_NKLoadPort,
						JW_Vessel,
						JW_ATD,
						JW_TA				= ISNULL(JW_ATA, JW_ETA)
					FROM
						dbo.JobConsolTransport
					WHERE
						JW_ParentType = 'CON'
				) AS FirstVessel ON FirstVessel.JW_JK = Consol.JK_PK AND Consol.JK_RL_NKLoadPort = FirstVessel.JW_RL_NKLoadPort
                JOIN
				(
					SELECT
						JW_JK				= JW_ParentGUID,
						JW_RL_NKLoadPort,
						JW_RL_NKDiscPort,
						JW_Vessel,
						JW_ATD,
						JW_TA				= ISNULL(JW_ATA, JW_ETA)
					FROM
						dbo.JobConsolTransport
					WHERE
						JW_ParentType = 'CON'
				) AS LastVessel ON LastVessel.JW_JK = Consol.JK_PK AND Consol.JK_RL_NKDischargePort = LastVessel.JW_RL_NKDiscPort

            WHERE
               ((Orgs.JS_E2_OA_OH_Consignee = @ImporterPK AND @SupplierPK is Null)
               OR (Orgs.JS_E2_OA_OH_Consignor = @SupplierPK AND @ImporterPK is Null))
               AND Shipment.JS_PK in (Select JN_JS from dbo.JobConShipLink)
               AND LastVessel.JW_TA >= @Today and FirstVessel.JW_ATD <= @Today
               AND FirstVessel.JW_ATD IS NOT NULL
               AND Shipment.JS_TransportMode = 'SEA'
					AND JK_ConsolMode = 'BCN'
					AND left(JK_RL_NKLoadPort, 2) != @CountryCode

			ORDER BY CurrentETA
			";

		#endregion

		#region ClientOrgsForNotYetArrived

		const string DropClientOrgsForNotYetArrived = "DROP FUNCTION ClientOrgsForNotYetArrived";
		const string ClientOrgsForNotYetArrived = @"
		CREATE FUNCTION ClientOrgsForNotYetArrived
			(
                @CountryCode AS char(2),
                @Today AS datetime
            )
            RETURNS TABLE AS
            RETURN
		SELECT DISTINCT
			JS_E2_OA_OH_Consignor as ConsignorPK,
			JS_E2_OA_OH_Consignee as ConsigneePK
            FROM
	            dbo.JobConShipLink
	        JOIN dbo.JobShipment AS Shipment ON JN_JS = Shipment.JS_PK
	        JOIN dbo.JobConsol AS Consol ON JN_JK = Consol.JK_PK
		    LEFT JOIN dbo.cvw_JobShipmentOrgs AS Orgs ON Orgs.JS_PK = Shipment.JS_PK
 		    JOIN
				(
					SELECT
						JW_JK				= JW_ParentGUID,
						JW_RL_NKLoadPort,
						JW_Vessel,
						JW_ATD,
						JW_TA				= ISNULL(JW_ATA, JW_ETA)
					FROM
						dbo.JobConsolTransport
					WHERE
						JW_ParentType = 'CON'
				) AS FirstVessel ON FirstVessel.JW_JK = Consol.JK_PK AND Consol.JK_RL_NKLoadPort = FirstVessel.JW_RL_NKLoadPort
                JOIN
				(
					SELECT
						JW_JK				= JW_ParentGUID,
						JW_RL_NKLoadPort,
						JW_RL_NKDiscPort,
						JW_Vessel,
						JW_ATD,
						JW_TA				= ISNULL(JW_ATA, JW_ETA)
					FROM
						dbo.JobConsolTransport
					WHERE
						JW_ParentType = 'CON'
				) AS LastVessel ON LastVessel.JW_JK = Consol.JK_PK AND Consol.JK_RL_NKDischargePort = LastVessel.JW_RL_NKDiscPort
            WHERE
                 Shipment.JS_PK in (Select JN_JS from dbo.JobConShipLink)
                AND LastVessel.JW_TA >= @Today
                AND FirstVessel.JW_ATD <= @Today
                AND FirstVessel.JW_ATD IS NOT NULL
--                AND JS_E_DEP <= @Today
--                AND JS_E_ARV >= @Today
                AND Shipment.JS_TransportMode = 'SEA'
		AND left(JK_RL_NKLoadPort,2) != @CountryCode";

		#endregion

		#region Agent Rates Report

		#region ClientViewChargeCodesByPort

		const string DropClientViewChargeCodesByPort = "drop view ClientViewChargeCodesByPort";
		const string ClientViewChargeCodesByPort = @"
			create view ClientViewChargeCodesByPort
			as
				select distinct
					AC_Code as ChargeCode,
					TI_OriginLRC as Origin,
					TH_RateType as RateType
				from
					dbo.RatingHeader
				join dbo.RateEntry on TI_TH = TH_PK
				join dbo.RateLines on TL_TI = TI_PK and TL_RateCalculator = 'UNT' and TL_WeightVolume = 'CN'
				join dbo.AccChargeCode on TL_AC = AC_PK and AC_ChargeGroup in ('FRT', 'ORG')
				where
					TI_RateStartDate <= GETDATE()
					and
					(
						TI_RateEndDate >= GETDATE()
						or
						TI_RateEndDate is null
					)
					and
					(
						TI_Mode = 'FCL'
						or
						(
							TI_Mode = 'SEA'
							and
							TI_RateCategory = 'FCL'
						)
					)

			";

		#endregion

		#region ClientFuncOrderedChargeCodes

		const string DropClientFuncOrderedChargeCodes = "DROP FUNCTION ClientFuncOrderedChargeCodes";
		const string ClientFuncOrderedChargeCodes = @"
			create function ClientFuncOrderedChargeCodes(@RateType char(3), @Origin varchar(5))
			returns table as
			return
				select  * from
				(
					select
						ChargeCode,
						'RAT_' + CONVERT(varchar(10), ROW_NUMBER() over (order by ChargeCode)) as RowNum
					from
						ClientViewChargeCodesByPort
					where
						Origin like @Origin + '%'
					and
						RateType LIKE (case when @RateType is null then '%' else @RateType end)
					group by
						ChargeCode
				)
				as SourceTable

				PIVOT
				(
					MAX(ChargeCode)
					FOR RowNum IN
					(RAT_1, RAT_2, RAT_3, RAT_4, RAT_5, RAT_6, RAT_7, RAT_8, RAT_9, RAT_10, RAT_11, RAT_12, RAT_13, RAT_14, RAT_15)
				)
				AS PivotTable
			";

		#endregion

		#region ClientViewChargeCodesWithRowNumbers

		const string DropClientViewChargeCodesWithRowNumbers = "DROP FUNCTION ClientViewChargeCodesWithRowNumbers";
		const string ClientViewChargeCodesWithRowNumbers = @"
			create function ClientViewChargeCodesWithRowNumbers(@RateType char(3), @Origin varchar(5))
			returns table as
			return
				select ChargeCode, ROW_NUMBER() OVER (ORDER BY ChargeCode) as RowNum
				from ClientViewChargeCodesByPort where Origin like @Origin + '%' and RateType = @RateType

			";

		#endregion

		#region ClientFunctionPerContainerUnitCalcRates

		const string DropClientFunctionPerContainerUnitCalcRates = "drop function ClientFunctionPerContainerUnitCalcRates";
		const string ClientFunctionPerContainerUnitCalcRates = @"
				create function ClientFunctionPerContainerUnitCalcRates(@RateType char(3), @Origin varchar(5))
				returns table as
				return
					select
						TH_OH as ClientPK,
						Client.OH_Code as ClientCode,
						TI_OriginLRC as Origin,
						TI_DestinationLRC as Destination,
						Supplier.OH_Code as SupplierCode,
						Supplier.OH_FullName as SupplierName,
						Carrier.OH_Code as CarrierCode,
						Carrier.OH_FullName as CarrierName,
						RC_Code as Container,
						AC_Code as ChargeCode,
						TH_RateType + '_' + CONVERT(varchar(10), ClientViewChargeCodesWithRowNumbers.RowNum) as RowNumber,
						case when TM_AgentDeclaredRate != 0 then
							TL_RX_NKCurrency + ' ' + CONVERT(varchar(50), TM_AgentDeclaredRate)
						else
							TL_RX_NKCurrency + ' ' + CONVERT(varchar(50), TM_Value)
						end as RateWithCurrency,
						TH_RateType as RateType
					from dbo.RatingHeader
					join dbo.OrgHeader as Client on TH_OH = OH_PK
					join dbo.RateEntry on TI_TH = TH_PK
					left join dbo.RefContainer on TI_RC = RC_PK
					left join dbo.OrgHeader as Supplier on TI_OH_Consignor = Supplier.OH_PK
					left join dbo.OrgHeader as Carrier on TI_OH_TransportProvider = Carrier.OH_PK
					join dbo.RateLines on TL_TI = TI_PK
					join dbo.AccChargeCode on TL_AC = AC_PK
					join ClientViewChargeCodesWithRowNumbers(@RateType, @Origin) on AC_Code = ChargeCode
					join dbo.RateLineItems on TM_TL = TL_PK and TM_Type = 'UNT'
					where
						TH_RateType = @RateType
						and TI_OriginLRC like @Origin + '%'
						and TI_RateStartDate <= GETDATE()
						and
						(
							TI_RateEndDate >= GETDATE()
							or
							TI_RateEndDate is null
						)
						and
						(
							TI_Mode = 'FCL'
							or
							(
								TI_Mode = 'SEA'
								and
								TI_RateCategory = 'FCL'
							)
						)
			";

		#endregion

		#region ClientProcAgentDeclaredRates

		const string DropClientProcAgentDeclaredRates = "drop proc ClientProcAgentDeclaredRates";
		const string ClientProcAgentDeclaredRates = @"
			create proc ClientProcAgentDeclaredRates(@AgentPK uniqueidentifier, @OriginUNLOCOCode varchar(5))
			as
			begin
				SELECT ROW_NUMBER() over (order by ClientCode) as RowNumber ,
					*
					FROM
					(
						SELECT
							SellRates.ClientPK,
							Client.OH_Code as ClientCode,
							Client.OH_FullName as ClientName,
							Client.OH_RL_NKClosestPort as ClientUNLOCO,
							ClientMiscServ.OM_IMDefaultINCOTerm as IncoTerm,
							SellRates.CarrierName,
							SellRates.SupplierName,
							SellRates.Origin,
							SellRates.Destination,
							SellRates.Container,
							SellRates.RowNumber as SellRowNumber,
							MAX(SellRates.RateWithCurrency) as SellRate,
							CostRates.RowNumber as CostRowNumber,
							MAX(CostRates.RateWithCurrency) as CostRate
						FROM
							dbo.OrgHeader Client
							join dbo.OrgMiscServ ClientMiscServ on OM_OH = OH_PK
							join ClientFunctionPerContainerUnitCalcRates('SAL', @OriginUNLOCOCode) SellRates on SellRates.ClientPK = OH_PK
							join ClientFunctionPerContainerUnitCalcRates('COS', @OriginUNLOCOCode) CostRates on CostRates.ClientPK = @AgentPK
						GROUP BY
							SellRates.ClientPK,
							Client.OH_Code,
							Client.OH_FullName,
							Client.OH_RL_NKClosestPort,
							ClientMiscServ.OM_IMDefaultINCOTerm,
							SellRates.CarrierName,
							SellRates.SupplierName,
							SellRates.Origin,
							SellRates.Destination,
							SellRates.Container,
							SellRates.RowNumber,
							CostRates.RowNumber
					)
					AS SourceTable

					PIVOT
					(
						MAX(SellRate)
						FOR SellRowNumber IN
						(SAL_1, SAL_2, SAL_3, SAL_4, SAL_5, SAL_6, SAL_7, SAL_8, SAL_9, SAL_10, SAL_11, SAL_12, SAL_13, SAL_14, SAL_15)
					)
					AS SellPivotTable

					PIVOT
					(
						MAX(CostRate)
						FOR CostRowNumber IN
						(COS_1, COS_2, COS_3, COS_4, COS_5, COS_6, COS_7, COS_8, COS_9, COS_10, COS_11, COS_12, COS_13, COS_14, COS_15)
					)
					AS CostPivotTable
			end

		";

		#endregion

		#endregion

		#endregion

		#endregion
	}
}
