using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;

namespace Enterprise.Client.M1A
{
	public class M1AClientDbSchemaUpgradeInfo : IExtensionObjects
	{
		ImmutableArray<DatabaseObjectCreateScript> IExtensionObjects.TableCreationScripts => TableCreationScripts;
		ImmutableArray<DatabaseViewAndRoutineCreateScript> IExtensionObjects.ViewAndRoutineCreationScripts => ViewAndRoutinesCreationScripts;

		static ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts => ImmutableArray<DatabaseObjectCreateScript>.Empty;

		#region ViewAndRoutinesCreationScripts

		static ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts => ImmutableArray.Create(
			//FUNCTIONS
			Client_ctfn_JobShipmentOrgs,
			Client_Report_ShipmentProfileReport
		);

		#region FUNCTION Client_ctfn_JobShipmentOrgs

		static DatabaseViewAndRoutineCreateScript Client_ctfn_JobShipmentOrgs
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("Client_ctfn_JobShipmentOrgs", @"
CREATE FUNCTION Client_ctfn_JobShipmentOrgs  
(  
 @JS_PK uniqueidentifier  
)  
RETURNS @Result TABLE  
(  
 Consignor_PK uniqueidentifier,    
 Consignor_Code nvarchar(12),    
 Consignor_FullName nvarchar(200), 
 Consignor_City  nvarchar(100),
 Consignor_State nvarchar(50),
 Consignor_PostCode nvarchar(10),
 Consignor_Contact nvarchar(200),
 Consignor_Phone nvarchar(40),
 
 Consignee_PK uniqueidentifier,    
 Consignee_Code nvarchar(12),    
 Consignee_FullName nvarchar(200),
 Consignee_City  nvarchar(100),
 Consignee_State nvarchar(50),
 Consignee_PostCode nvarchar(10)
)  
AS  
BEGIN  
 INSERT INTO @Result   
  SELECT   
   OH_PK  
   ,OH_Code  
   ,CASE E2_AddressOverride  
    WHEN 1 THEN E2_CompanyName  
    ELSE OH_FullName  
   END
   ,CASE E2_AddressOverride  
    WHEN 1 THEN E2_City 
    ELSE OA_City 
   END
   ,CASE E2_AddressOverride  
    WHEN 1 THEN E2_State  
    ELSE OA_State  
   END
   ,CASE  E2_AddressOverride
    WHEN 1 THEN E2_Postcode
    ELSE OA_Postcode 
   END
   ,CASE  E2_Contact
    WHEN '' THEN (SELECT TOP 1 OC_ContactName FROM dbo.OrgContact WHERE OC_OH = OH_PK)
    ELSE E2_Contact
   END
   ,CASE  E2_AddressOverride
    WHEN 1 THEN E2_Phone
    ELSE OA_Phone
   END        
   ,NULL
   ,NULL
   ,NULL	
   ,NULL
   ,NULL
   ,NULL		  
  FROM    
   dbo.JobDocAddress    
   INNER JOIN dbo.OrgAddress ON E2_OA_Address = OA_PK      
   INNER JOIN dbo.OrgHeader ON OA_OH = OH_PK    
  WHERE  
   E2_ParentID = @JS_PK  
   AND  
   E2_AddressType = 'CRD'  
   AND  
   E2_ParentTableCode = 'JS'  
 IF @@ROWCOUNT > 0  
 BEGIN  
  UPDATE @Result   
     SET Consignee_PK = OH_PK,  
      Consignee_Code = OH_Code,  
      Consignee_FullName = CASE E2_AddressOverride  
             WHEN 1 THEN E2_CompanyName  
             ELSE OH_FullName  
             END , 
		Consignee_City = CASE E2_AddressOverride  
			WHEN 1 THEN E2_City 
			ELSE OA_City 
			END,
		Consignee_State = CASE E2_AddressOverride  
			WHEN 1 THEN E2_State  
			ELSE OA_State  
			END,
		Consignee_PostCode = CASE  E2_AddressOverride
			WHEN 1 THEN E2_Postcode
			ELSE OA_Postcode 
			END 
     FROM    
      dbo.JobDocAddress    
      INNER JOIN dbo.OrgAddress ON E2_OA_Address = OA_PK      
      INNER JOIN dbo.OrgHeader ON OA_OH = OH_PK    
     WHERE  
      E2_ParentID = @JS_PK  
      AND  
      E2_AddressType = 'CED'  
      AND  
      E2_ParentTableCode = 'JS'  
 END  
 ELSE  
 BEGIN  
   INSERT INTO @Result   
    SELECT   
		NULL  
		,NULL  
		,NULL  
		,NULL
		,NULL
		,NULL
		,NULL
		,NULL
		,OH_PK    
		,OH_Code  
		,CASE E2_AddressOverride  
		WHEN 1 THEN E2_CompanyName  
		ELSE OH_FullName  
		END    
		,CASE E2_AddressOverride  
		WHEN 1 THEN E2_City 
		ELSE OA_City 
		END
		,CASE E2_AddressOverride  
		WHEN 1 THEN E2_State  
		ELSE OA_State  
		END
		,CASE  E2_AddressOverride
		WHEN 1 THEN E2_Postcode
		ELSE OA_Postcode 
		END 
    FROM    
     dbo.JobDocAddress    
     INNER JOIN dbo.OrgAddress ON E2_OA_Address = OA_PK      
     INNER JOIN dbo.OrgHeader ON OA_OH = OH_PK    
    WHERE  
     E2_ParentID = @JS_PK  
     AND  
     E2_AddressType = 'CED'  
     AND  
     E2_ParentTableCode = 'JS'  
 END  
 RETURN    
END",
					"DROP FUNCTION Client_ctfn_JobShipmentOrgs", DbRoutineType.SqlFunctionTableTypeDesc);
			}
		}

		#endregion

		#region FUNCTION Client_Report_ShipmentProfileReport

		static DatabaseViewAndRoutineCreateScript Client_Report_ShipmentProfileReport
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("Client_Report_ShipmentProfileReport", @"
CREATE FUNCTION Client_Report_ShipmentProfileReport
(  
 @CurrentCountry char(2),  
 @CompanyPK AS uniqueidentifier,  
 @OrderedStorageClASsCodesList AS varchar(1000),  
 @Origin AS uniqueidentifier,  
 @DestinatiON AS uniqueidentifier  
)  
RETURNS TABLE  
AS  
RETURN  
--SHIPMENT DETAILS        
SELECT        
CASE WHEN JobHeader.JH_PK IS NOT NULL THEN 'Y' ELSE NULL END AS HeaderExists,         
JS.JS_UniqueCONsignRef AS ShipmentID,        
JS.JS_TransportMode    AS Type,        
JS.JS_PackingMode     AS Mode,        
JS.JS_RL_NKOrigin    AS Origin,  
JS.JS_INCO AS INCO,  
 CASE  
  WHEN JS.JS_INCO IN ('EXW', 'FCA', 'FAS', 'FOB', 'CLT', 'C3P', 'FCD') THEN 'CCX'  
  WHEN JS.JS_INCO IN ('CFR', 'CIF', 'CPT', 'CIP', 'DAF', 'DES', 'DEQ', 'DDU', 'DDP', 'PPD') THEN 'PPD'  
  ELSE ''  
 END AS Terms,        
JS.JS_RL_NKDestinatiON    AS Dest,        
ShipmentOrgs.Consignor_PK AS ConsignorPK  
,ShipmentOrgs.Consignee_PK AS ConsigneePK  
,ShipmentOrgs.Consignor_Code AS ConsignorCode 
,ShipmentOrgs.Consignor_FullName AS ConsignorName  
,ShipmentOrgs.Consignor_City AS ConsignorCity  
,ShipmentOrgs.Consignor_State AS ConsignorState 
,ShipmentOrgs.Consignor_PostCode AS ConsignorPostCode 
,ShipmentOrgs.Consignor_Contact AS ShipperContacts 
,ShipmentOrgs.Consignor_Phone AS ShipperPhone 
,ShipmentOrgs.Consignee_Code AS ConsigneeCode  
,ShipmentOrgs.Consignee_FullName AS ConsigneeName
,ShipmentOrgs.Consignee_City AS ConsigneeCity  
,ShipmentOrgs.Consignee_State AS ConsigneeState 
,ShipmentOrgs.Consignee_PostCode AS ConsigneePostCode,  
 
JS.JS_HouseBill    AS HouseBill,        
JS.JS_E_DEP     AS ETD,        
JS.JS_E_ARV     AS ETA,        
JS.JS_ActualWeight    AS Weight,        
JS.JS_ActualVolume    AS Volume,        
JS.JS_UnitOfWeight    AS WeightUnit,        
JS.JS_UnitOfVolume    AS VolumeUnit,        
JS.JS_ActualChargeable    AS Chargeable,        
CASE WHEN JS.JS_IsCancelled = 1 THEN 'Y' ELSE 'N' END AS IsCancelled,      
(        
 CASE        
  WHEN JS.JS_TransportMode IN ('AIR', 'MAI', 'FSA') THEN         
   'KG'        
  ELSE        
   'M3'        
 END        
)      AS ChargeableUnit,        
JS.JS_TotalPackageCount   AS Inners,        
JS.JS_F3_NKTotalCountPackType  AS InnersType,        
JS.JS_OuterPacks    AS Outers,        
JS.JS_F3_NKPackType   AS OutersType,        
JS.JS_SystemCreateTimeUtc   AS RegisteredDate,    
JS.JS_GoodsDescriptiON  AS JS_GoodsDescriptiON,      
CASE WHEN JS.JS_IsForwardRegistered = 1 THEN 'Y' ELSE 'N' END AS IsForward,      
CASE WHEN JE_JS IS NULL THEN 'N' ELSE 'Y' END AS DeclaratiON,        
CASE WHEN JJ_ParentID IS NULL THEN 'N' ELSE 'Y' END AS Transport,  
        
--CONSOL DETAILS        
SFC.CONsolID         AS CONsolID,        
SFC.CONsolCount       AS CONsolCount,        
  
MainCONTrans.JW_RL_NKLoadPort AS LoadPort,  
MainCONTrans.JW_RL_NKDiscPort AS DischargePort,  
JK_MASterBillNum    AS MASterBill,        
MainCONTrans.JW_Vessel AS Vessel,  
MainCONTrans.JW_VoyageFlight AS Voyage,  
MainCONTrans.JW_ETD,  
MainCONTrans.JW_ETA,  
SENDAgent_Code    AS SENDAgentCode,        
SENDAgent_FullName    AS SENDAgentName,        
RecvAgent_Code    AS RecvAgentCode,        
RecvAgent_FullName    AS RecvAgentName,        
CoLoad_Code     AS CoLoadCode,        
CoLoad_FullName    AS CoLoadName,        
        
----JOB INVOICE SUMMARY        
GLBBranch.GB_Code   AS Branch,              
GLBDepartment.GE_Code   AS Dept,            
JobHeader.JH_GB     AS BranchPK,         
JobHeader.JH_GE     AS DepartmentPK,         
LocalClient.OH_PK   AS LocalClientPK,         
LocalClient.OH_Code    AS LocalClient,         
LocalClient.OH_FullName   AS LocalClientFullName,        
AL_LineAmount   AS Amount, --aka JobProfit        
(CASE         
 WHEN REVAmount = .0000 THEN        
           NULL        
 ELSE        
    REVAmount        
END) AS REVAmount,        
(CASE         
 WHEN WIPAmount = .0000 THEN        
            NULL        
 ELSE        
    WIPAmount        
END) AS WIPAmount,        
(CASE         
 WHEN ACRAmount = .0000 THEN        
            NULL        
 ELSE        
    ACRAmount        
END) AS ACRAmount,        
(CASE         
 WHEN CSTAmount = .0000 THEN        
            NULL        
ELSE        
    CSTAmount        
END) AS CSTAmount,        
  
CONtainers.TotalTEU  AS TEU,        
CONtainers.CONtainerCount AS CONtainerCount,        
ColoadMASter.JS_UniqueCONsignRef AS ColoadMASter,        
(        
 CASE         
  WHEN         
  (        
   SELECT         
    (        
     CASE         
      WHEN COUNT(*) > 0 THEN        
       'Y'        
      ELSE        
       'N'        
     END        
    )        
   FROM         
    dbo.JobShipment JSSubs        
   WHERE        
    JSSubs.JS_JS_ColoadMASterShipment = JS.JS_PK        
  ) ='Y' OR         
  JS.JS_ShipmentType = 'CLD' THEN         
   'Y'        
  ELSE        
   'N'        
 END        
)AS IsColoadMASter,        
JH_Status AS JobStatus,         
JH_SystemCreateTimeUtc AS JobCreatedDate,         
SalesRep.GS_PK AS SalesRepPK,        
SalesRep.GS_FullName AS SalesRepName,         
Operator.GS_PK AS OperatorPK,        
Operator.GS_FullName AS OperatorName,        
Importer_Code AS ImportBrokerCode,        
Importer_FullName AS ImportBrokerName,        
Exporter_Code AS ExportBrokerCode,        
Exporter_FullName AS ExportBrokerName,        
        
CountOther,
Count1, 
Count2, 
Count3,         
Count4, 
Count5, 
Count6,        
Count7, 
Count8, 
Count9,         
Count10, 
Count11, 
Count12,         
Count13, 
Count14, 
Count15,   

FirstTransports.JW_ETD AS JW_ETDFirst,  
LastTransports.JW_ETA AS JW_ETALASt,  
FirstTransports.JW_RL_NKLoadPort AS JW_RL_NKLoadPortFirst,  
LastTransports.JW_RL_NKDiscPort AS JW_RL_NKDiscPortLASt,  
Carrier_Code AS CarrierCode,  
Carrier_FullName AS CarrierName,  
JP_FCLPickupEquipmentNeeded ,
JP_EstimatedPickup ,
JP_PickupRequiredBY ,
JP_PickupCartageAdvised ,
JP_PickupCartageCompleted ,
JS.JS_InterimReceipt ,
JS.JS_A_RCV ,
JP_FCLDeliveryEquipmentNeeded ,
JP_EstimatedDelivery ,
JP_DeliveryRequiredBY ,
JP_DeliveryCartageAdvised ,
JP_DeliveryCartageCompleted ,

--- added CBO
JobChargeFlat.LocalTotalInvoiced AS LocalTotalInvoiced,
JobChargeFlat.AC_Code_1			AS AC_Code_1,
JobChargeFlat.JR_LocalSellAmt_1 AS JR_LocalSellAmt_1,
JobChargeFlat.AC_Code_2			AS AC_Code_2,
JobChargeFlat.JR_LocalSellAmt_2 AS JR_LocalSellAmt_2,
JobChargeFlat.AC_Code_3			AS AC_Code_3,
JobChargeFlat.JR_LocalSellAmt_3 AS JR_LocalSellAmt_3,
JobChargeFlat.AC_Code_4			AS AC_Code_4,
JobChargeFlat.JR_LocalSellAmt_4 AS JR_LocalSellAmt_4,
JobChargeFlat.AC_Code_5			AS AC_Code_5,
JobChargeFlat.JR_LocalSellAmt_5 AS JR_LocalSellAmt_5,
JobChargeFlat.AC_Code_6			AS AC_Code_6,
JobChargeFlat.JR_LocalSellAmt_6 AS JR_LocalSellAmt_6,
JobChargeFlat.AC_Code_7			AS AC_Code_7,
JobChargeFlat.JR_LocalSellAmt_7 AS JR_LocalSellAmt_7,
JobChargeFlat.AC_Code_8			AS AC_Code_8,
JobChargeFlat.JR_LocalSellAmt_8 AS JR_LocalSellAmt_8,
JobChargeFlat.AC_Code_9			AS AC_Code_9,
JobChargeFlat.JR_LocalSellAmt_9 AS JR_LocalSellAmt_9,
JobChargeFlat.AC_Code_10		AS AC_Code_10,
JobChargeFlat.JR_LocalSellAmt_10 AS JR_LocalSellAmt_10,
JobChargeFlat.AC_Code_11		AS AC_Code_11,
JobChargeFlat.JR_LocalSellAmt_11 AS JR_LocalSellAmt_11,
JobChargeFlat.AC_Code_12		AS AC_Code_12,
JobChargeFlat.JR_LocalSellAmt_12 AS JR_LocalSellAmt_12,
JobChargeFlat.AC_Code_13		AS AC_Code_13,
JobChargeFlat.JR_LocalSellAmt_13 AS JR_LocalSellAmt_13,
JobChargeFlat.AC_Code_14		AS AC_Code_14,
JobChargeFlat.JR_LocalSellAmt_14 AS JR_LocalSellAmt_14,
JobChargeFlat.AC_Code_15		AS AC_Code_15,
JobChargeFlat.JR_LocalSellAmt_15 AS JR_LocalSellAmt_15,

(SELECT TOP 1 P9_actualDate FROM dbo.ProcessTasks WHERE JS.JS_PK = P9_ParentID AND P9_SE_NKMilestONeEvent = 'PUP'
AND P9_IsValid = 1
AND P9_Type <> 'EXC'
AND P9_ActualDate IS NOT NULL
AND P9_ParentTableCode = 'JS') AS Pickup_P9_ActualDate,

(SELECT TOP 1 P9_actualDate FROM dbo.ProcessTasks WHERE JS.JS_PK = P9_ParentID AND P9_SE_NKMilestONeEvent = 'DLV'
AND P9_IsValid = 1
AND P9_Type <> 'EXC'
AND P9_ActualDate IS NOT NULL
AND P9_ParentTableCode = 'JS')		AS Delivery_P9_ActualDate,

JS.JS_RS_NKServiceLevel		AS ServiceLevelCode,
JS.JS_bookingReference		AS ShippersReference,
JT.OrderReferenceString		AS OrderReference,
(SELECT TOP 1 EU_GoodsSignForBy FROM dbo.JobPickupDeliveryConfirm WHERE EU_PK IN 
	(SELECT J8_EU_PickupDeliverConfirm FROM dbo.JobTransportLegPackLineDivot WHERE J8_JL IN
		(SELECT JL_PK FROM dbo.JobPackLines WHERE JL_JS = JS.JS_PK)) AND EU_PickupDeliveryType = 'DLV') PODName
	

FROM        
 dbo.JobShipment JS        
 LEFT JOIN         
 (         
  SELECT         
   JN_JS,        
   MAX(JK_UniqueCONsignRef) AS CONsolID,        
   COUNT(*) AS CONsolCount        
  FROM        
   dbo.JobCONShipLink        
   INNER JOIN dbo.JobCONsol ON JN_JK = JK_PK        
  GROUP BY         
   JN_JS        
 ) AS SFC         
 ON JS.JS_PK = SFC.JN_JS        
 LEFT JOIN dbo.JobCONsol JK ON SFC.CONsolID = JK.JK_UniqueCONsignRef          
 OUTER APPLY Client_ctfn_JobShipmentOrgs(JS.JS_PK) AS ShipmentOrgs  
 LEFT JOIN   
  (SELECT   
    JE_JS   
   FROM   
    dbo.JobDeclaratiON   
   GROUP BY   
    JE_JS   
  ) AS JobDeclaratiON ON JE_JS = JS.JS_PK  
 OUTER APPLY ctfn_ShipmentCONsolOrgs(	(select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_ShippingLineAddress),
										(select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_SendingForwarderAddress),
										(select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_ReceivingForwarderAddress),
										CASE WHEN JK_AgentType = 'CLD' THEN (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_CreditorAddress) ELSE NULL END,
										JS_OH_ExportBroker,
										JS_OH_ImportBroker) AS Orgs
 LEFT JOIN csfn_MainCONsolTransport(@CurrentCountry) AS MainCONTrans ON MainCONTrans.JW_JK = JK.JK_PK AND JK_PK IS NOT NULL  
 LEFT JOIN dbo.JobHeader   
  ON  
  JH_ParentID = JS.JS_PK  
  AND  
  JH_GC = @CompanyPK  
  AND  
  JH_ParentTableCode = 'JS'
 LEFT JOIN dbo.OrgAddress As LocalChargesAddress ON JobHeader.JH_OA_LocalChargesAddr = LocalChargesAddress.OA_PK
 LEFT JOIN    
 (  
	SELECT  
		JH_PK  
		,SUM(AL_LineAmount) AS AL_LineAmount  
		,SUM(WIPAmount) AS WIPAmount  
		,SUM(ACRAmount) AS ACRAmount  
		,SUM(CSTAmount) AS CSTAmount  
		,SUM(REVAmount) AS REVAmount  
	FROM  
		dbo.JobHeader  
		INNER JOIN dbo.vw_ClASsifiedTransactiONLineAmountsIncludingCancelled   
		ON    
			JH_PK = AL_JH  
			AND JH_GC = @CompanyPK  
			AND JH_ParentTableCode = 'JS'  
			AND 
			(  
				AL_ReverseDate IS NULL      
				OR AL_LineType in ( 'CST', 'REV' )
			)
	GROUP BY  
		JH_PK  
 ) AS HeaderAmounts  
  ON    
   JobHeader.JH_PK = HeaderAmounts.JH_PK      
  LEFT JOIN dbo.GLBBranch ON JH_GB = GB_PK        
  LEFT JOIN dbo.GLBDepartment ON JH_GE = GE_PK        
  LEFT JOIN dbo.GLBStaff SalesRep ON JH_GS_NKRepSales = SalesRep.GS_Code        
  LEFT JOIN dbo.GLBStaff Operator ON JH_GS_NKRepOps = Operator.GS_Code
  LEFT JOIN dbo.OrgHeader AS LocalClient ON LocalChargesAddress.OA_OH = LocalClient.OH_PK        
  LEFT JOIN dbo.JobDocsANDCartage ON  JS.JS_PK = JP_ParentID        
  LEFT JOIN dbo.JobCartage ON  JJ_ParentID = JobDocsANDCartage.JP_PK  
  LEFT JOIN        
   (        
  SELECT         
    JobShipment.JS_PK AS ShipmentPK,   
    SUM(CASE WHEN JC_CONtainerCount < 1 THEN 1 ELSE JC_CONtainerCount END)  AS CONtainerCount,        
    SUM(RefCONtainer.RC_TEU * CASE WHEN JC_CONtainerCount < 1 THEN 1 ELSE JC_CONtainerCount END)AS TotalTEU,        
    SUM(CountOther) AS CountOther,
    SUM(Count1) AS Count1, SUM(Count2) AS Count2, SUM(Count3) AS Count3,         
    SUM(Count4) AS Count4, SUM(Count5) AS Count5, SUM(Count6) AS Count6,        
    SUM(Count7) AS Count7, SUM(Count8) AS Count8, SUM(Count9) AS Count9,         
    SUM(Count10) AS Count10, SUM(Count11) AS Count11, SUM(Count12) AS Count12,         
    SUM(Count13) AS Count13, SUM(Count14) AS Count14, SUM(Count15) AS Count15        
  FROM         
    dbo.JobShipment          
    LEFT JOIN   
    (  
     SELECT  
   JL_JS,   
   J6_JC  
     FROM  
   dbo.JobPackLines  
   LEFT JOIN  dbo.JobCONtainerPackPivot ON J6_JL = JL_PK        
     GROUP BY   
   JL_JS,   
   J6_JC  
    ) AS JSJC  
    ON JS_PK = JL_JS  
    LEFT JOIN dbo.JobCONtainer ON J6_JC = JobCONtainer.JC_PK        
    LEFT JOIN dbo.RefCONtainer ON JobCONtainer.JC_RC = RefCONtainer.RC_PK        
    LEFT JOIN ContainerSummaryFunction(@OrderedStorageClASsCodesList) ON ContainerSummaryFunction.JC_PK = JobCONtainer.JC_PK        
  WHERE   
   JS_PackingMode = 'FCL' OR (JS_PackingMode = 'BCN' AND JS_ShipmentType = 'BCN')
  GROUP BY   
   JobShipment.JS_PK  
  )AS CONtainers         
   ON CONtainers.ShipmentPK = JS.JS_PK         
  LEFT JOIN dbo.JobShipment ColoadMASter ON JS.JS_JS_ColoadMASterShipment = ColoadMASter.JS_PK    

	LEFT JOIN
	(
		SELECT
			JW_ParentGUID,
			FirstTransportPk = CONVERT(uniqueidentifier, RIGHT(MIN(
				FORMAT(JW_LegOrder,'000')
				+ CONVERT(varchar(36), JW_PK)
				), 36)),
			LastTransportPk = CONVERT(uniqueidentifier, RIGHT(MAX(
				FORMAT(JW_LegOrder,'000')
				+ CONVERT(varchar(36), JW_PK)
				), 36))
		FROM 
			dbo.JobConsolTransport
		WHERE 
			JW_ParentType = 'CON'
		GROUP BY 
			JW_ParentGUID
	) AS GroupedTransports ON JK_PK = GroupedTransports.JW_ParentGUID

	LEFT JOIN
	(
		SELECT
			JW_PK,
			JW_ETD,
			JW_RL_NKLoadPort,
			JW_IsActive = ISNULL(JV_IsActive, 1)
		FROM
			dbo.JobConsolTransport
			LEFT JOIN 
			(
				dbo.JobSailing			
				JOIN dbo.JobVoyDestination	ON JB_PK = JX_JB
				JOIN dbo.JobVoyage			ON JV_PK = JB_JV
			) ON JX_PK = JW_JX
		WHERE
			JW_ParentType = 'CON'
	) AS FirstTransports ON GroupedTransports.FirstTransportPk = FirstTransports.JW_PK

	LEFT JOIN
	(
		SELECT
			JW_PK,
			JW_ETA,
			JW_RL_NKDiscPort,
			JW_IsActive = ISNULL(JV_IsActive, 1)
		FROM
			dbo.JobConsolTransport
			LEFT JOIN 
			(
				dbo.JobSailing			
				JOIN dbo.JobVoyDestination	ON JB_PK = JX_JB
				JOIN dbo.JobVoyage			ON JV_PK = JB_JV
			) ON JX_PK = JW_JX
		WHERE
			JW_ParentType = 'CON'
	) AS LastTransports ON GroupedTransports.LastTransportPk = LastTransports.JW_PK

  LEFT JOIN dbo.RefUNLOCO Origin ON JS.JS_RL_NKOrigin = Origin.RL_Code AND @Origin IS NOT NULL  
  LEFT JOIN dbo.RefUNLOCO DestinatiON ON JS.JS_RL_NKDestinatiON = DestinatiON.RL_Code AND @DestinatiON IS NOT NULL
  LEFT JOIN dbo.RefCountry OriginCountry ON OriginCountry.RN_Code = Origin.RL_RN_NKCountryCode
  LEFT JOIN dbo.RefCountry DestinationCountry ON DestinationCountry.RN_Code = DestinatiON.RL_RN_NKCountryCode

-- ADDED BY CBO

	LEFT JOIN 
	(
		SELECT 
		JR_JH, 
		JR_OH_SellAccount,
		SUM(SubTotalAmt) AS LocalTotalInvoiced,
		SUM(CASE Number WHEN 1 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_1,
		SUM(CASE Number WHEN 2 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_2,
		SUM(CASE Number WHEN 3 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_3,
		SUM(CASE Number WHEN 4 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_4,
		SUM(CASE Number WHEN 5 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_5,
		SUM(CASE Number WHEN 6 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_6,
		SUM(CASE Number WHEN 7 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_7,
		SUM(CASE Number WHEN 8 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_8,
		SUM(CASE Number WHEN 9 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_9,
		SUM(CASE Number WHEN 10 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_10,
		SUM(CASE Number WHEN 11 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_11,
		SUM(CASE Number WHEN 12 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_12,
		SUM(CASE Number WHEN 13 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_13,
		SUM(CASE Number WHEN 14 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_14,
		SUM(CASE Number WHEN 15 THEN SubTotalAmt ELSE NULL END) AS JR_LocalSellAmt_15,
		MIN(CASE Number WHEN 1 THEN ChargeCode ELSE NULL END) AS AC_Code_1,
		MIN(CASE Number WHEN 2 THEN ChargeCode ELSE NULL END) AS AC_Code_2,
		MIN(CASE Number WHEN 3 THEN ChargeCode ELSE NULL END) AS AC_Code_3,
		MIN(CASE Number WHEN 4 THEN ChargeCode ELSE NULL END) AS AC_Code_4,
		MIN(CASE Number WHEN 5 THEN ChargeCode ELSE NULL END) AS AC_Code_5,
		MIN(CASE Number WHEN 6 THEN ChargeCode ELSE NULL END) AS AC_Code_6,	
		MIN(CASE Number WHEN 7 THEN ChargeCode ELSE NULL END) AS AC_Code_7,
		MIN(CASE Number WHEN 8 THEN ChargeCode ELSE NULL END) AS AC_Code_8,
		MIN(CASE Number WHEN 9 THEN ChargeCode ELSE NULL END) AS AC_Code_9,
		MIN(CASE Number WHEN 10 THEN ChargeCode ELSE NULL END) AS AC_Code_10,
		MIN(CASE Number WHEN 11 THEN ChargeCode ELSE NULL END) AS AC_Code_11,
		MIN(CASE Number WHEN 12 THEN ChargeCode ELSE NULL END) AS AC_Code_12,
		MIN(CASE Number WHEN 13 THEN ChargeCode ELSE NULL END) AS AC_Code_13,
		MIN(CASE Number WHEN 14 THEN ChargeCode ELSE NULL END) AS AC_Code_14,
		MIN(CASE Number WHEN 15 THEN ChargeCode ELSE NULL END) AS AC_Code_15
		FROM
		( 	
			SELECT 
			JR_JH, JR_OH_SellAccount, MIN(AC_Code) ChargeCode, SUM(JR_LocalSellAmt) AS SubTotalAmt, ROW_NUMBER() OVER(PARTITION BY JR_JH, JR_OH_SellAccount ORDER BY JR_OH_SellAccount, SUM(JR_LocalSellAmt)) AS Number
			FROM dbo.JobCharge
			INNER JOIN dbo.AccChargeCode ON AC_PK = JR_AC
			INNER JOIN dbo.AccTransactionLines ON AL_PK = JR_AL_ARLine
			INNER JOIN dbo.AccTransactionHeader ON AH_PK = AL_AH                  
			WHERE AccTransactionHeader.AH_TransactionNum IS NOT NULL-- AND JR_JH = '9FFDD838-6343-4ACA-A840-191C5E2D8A3D'
			GROUP BY JR_JH, JR_OH_SellAccount,JR_AC
		) U
		GROUP BY JR_JH, JR_OH_SellAccount
	 ) AS JobChargeFlat ON JobHeader.JH_PK = JobChargeFlat.JR_JH AND LocalChargesAddress.OA_OH = JobChargeFlat.JR_OH_SellAccount
	LEFT JOIN (	SELECT   
					JT_JP ,
					MAX( CASE 
						WHEN JT_Sequence = '1'
						THEN JT_OrderReference
						ELSE ''
					END) +
					MAX( CASE 
						WHEN JT_Sequence = '2'
						THEN ', ' + JT_OrderReference 
						ELSE ''
					END) + 
					MAX( CASE 
						WHEN JT_Sequence = '3'
						THEN ', ' + JT_OrderReference 
						ELSE ''
					END) +
					MAX( CASE 
						WHEN JT_Sequence = '4'
						THEN ', ' + JT_OrderReference 
						ELSE ''
					END) +
					MAX( CASE 
						WHEN JT_Sequence = '5'
						THEN ', ' + JT_OrderReference 
						ELSE ''
					END) +
					MAX( CASE 
						WHEN JT_Sequence = '6'
						THEN ', ' + JT_OrderReference 
						ELSE ''
					END) +
					MAX( CASE 
						WHEN JT_Sequence = '7'
						THEN ', ' + JT_OrderReference 
						ELSE ''
					END) +
					MAX( CASE 
						WHEN JT_Sequence = '8'
						THEN ', ' + JT_OrderReference 
						ELSE ''
					END) +
					MAX( CASE 
						WHEN JT_Sequence = '9'
						THEN ', ' + JT_OrderReference 
						ELSE ''
					END) +
					MAX( CASE 
						WHEN JT_Sequence = '10'
						THEN ', ' + JT_OrderReference 
						ELSE ''
					END) AS OrderReferenceString
				FROM dbo.JobOrderItem
				GROUP BY JT_JP 
			) AS JT ON JT.JT_JP = JobDocsAndCartage.JP_PK 


WHERE  
(
	@Origin IS NULL
	OR 
	@Origin IN (Origin.RL_PK, OriginCountry.RN_PK)
	OR
	Origin.RL_PK IN (SELECT F2_ParentID FROM dbo.RefZONePivot WHERE F2_FZ = @Origin AND F2_ParentTableCode = 'RL')
	OR
	(
		OriginCountry.RN_PK IN (SELECT F2_ParentID FROM dbo.RefZONePivot WHERE F2_FZ = @Origin AND F2_ParentTableCode = 'RN')
		AND
		Origin.RL_PK NOT IN (SELECT F2_ParentID 
							FROM 
								  dbo.RefZONePivot 
								  INNER JOIN dbo.RefZONeHeader ON FZ_PK = F2_FZ
							WHERE 
								  F2_ParentTableCode = 'RL'
								  AND
								  FZ_ZONeType IN ('ALL', 'RPT')
							)
     )
)
AND 
(
	@DestinatiON IS NULL
	OR 
	@DestinatiON IN (DestinatiON.RL_PK, DestinationCountry.RN_PK)
	OR
	DestinatiON.RL_PK IN (SELECT F2_ParentID FROM dbo.RefZONePivot WHERE F2_FZ = @DestinatiON AND F2_ParentTableCode = 'RL')
	OR
	(
		DestinationCountry.RN_PK IN (SELECT F2_ParentID FROM dbo.RefZONePivot WHERE F2_FZ = @DestinatiON AND F2_ParentTableCode = 'RN')
		AND
		DestinatiON.RL_PK NOT IN (SELECT F2_ParentID 
							FROM 
								  dbo.RefZONePivot
								  INNER JOIN dbo.RefZONeHeader ON FZ_PK = F2_FZ
							WHERE 
								  F2_ParentTableCode = 'RL'
								  AND
								  FZ_ZONeType IN ('ALL', 'RPT')
							)
     )
)",
				"DROP FUNCTION Client_Report_ShipmentProfileReport", DbRoutineType.SqlFunctionInlineTypeDesc);
			}
		}

		#endregion

		#endregion
	}
}
