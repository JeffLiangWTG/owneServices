using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.MFI;
using Enterprise.Client.MFI.DocWrappers;
using Enterprise.Client.MFI.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Client.ClientOverride.ConsignmentNoteData),
	MFIConstants.AutoeDoc.BusinessRefType.ConNote,
	ClientSpecificCode = Clients.MFI)]

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		#region Instance

		public static ClientOverride Instance
		{
			get { return instance ?? (instance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride instance;

		public override Clients Client
		{
			get { return Clients.MFI; }
		}

		public override string ClientDisplayName
		{
			get { return "MFI"; }
		}

		public override string HelpWebPage
		{
			get { return string.Empty; }
		}

		#endregion

		#region IClientHook Members

		#region Core & Registry

		protected override void InitialiseCore()
		{
			MFIMenu.Initialise();
			DocMFIARInvoice.RegisterThisSubTypeOverride();
			MFIInvoicingBaseDocumentSupporter.RegisterThisSubTypeOverride();
			MFIStatement.RegisterThisSubTypeOverride();
			DocMFIPrintStatement.RegisterThisSubTypeOverride();
			MFIPrintStatementDocumentSupporter.RegisterThisSubTypeOverride();
			DocMFIBillofLadingContainer.RegisterThisSubTypeOverride();
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get
			{
				return MFIDataRegistry.Instance;
			}
		}

		#endregion

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();
			ClientOverrideModuleIdentifier consolID = new ClientOverrideModuleIdentifier(ModuleIDs.JobConsol);
			ClientOverrideModuleInfo consolModuleInfo = new ClientOverrideModuleInfo(consolID, typeof(MFIConsolModuleOverride).Assembly.FullName, typeof(MFIConsolModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(consolModuleInfo);

			ClientOverrideModuleIdentifier orderID = new ClientOverrideModuleIdentifier(ModuleIDs.Orders);
			ClientOverrideModuleInfo orderModuleInfo = new ClientOverrideModuleInfo(orderID, typeof(MFIOrderModuleOverride).Assembly.FullName, typeof(MFIOrderModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(orderModuleInfo);

			ClientOverrideModuleIdentifier transactionID = new ClientOverrideModuleIdentifier(ModuleIDs.APTransaction);
			ClientOverrideModuleInfo transactionModuleInfo = new ClientOverrideModuleInfo(transactionID, typeof(MFIPayableTransactionModuleOverride).Assembly.FullName, typeof(MFIPayableTransactionModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(transactionModuleInfo);

			ClientOverrideModuleIdentifier orgModuleID = new ClientOverrideModuleIdentifier(ModuleIDs.Organisation);
			ClientOverrideModuleInfo orgModuleIDInfo = new ClientOverrideModuleInfo(orgModuleID, typeof(MFIOrganisationModuleOverride).Assembly.FullName, typeof(MFIOrganisationModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(orgModuleIDInfo);
			return moduleOverrides;
		}

		#endregion

		#region DbSchemaUpgradeInfo

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new ExtensionObjects(
			ImmutableArray<DatabaseObjectCreateScript>.Empty,
			ViewAndRoutinesCreationScripts);

		#region ViewAndRoutinesCreationScripts

		protected static ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts { get; } = ImmutableArray.Create(
			new DatabaseViewAndRoutineCreateScript(
			"Client_ShipmentOrgs",
			Client_ShipmentOrgsScript,
			"Drop View Client_ShipmentOrgs",
			DbRoutineType.SqlViewTypeDesc
			),

			new DatabaseViewAndRoutineCreateScript(
			"Client_JobDecOrgs",
			Client_JobDecOrgsScript,
			"Drop View Client_JobDecOrgs",
			DbRoutineType.SqlViewTypeDesc
			),

			new DatabaseViewAndRoutineCreateScript(
			"Client_MFI_JobProfitForConsol",
			Client_MFI_JobProfitForConsolScript,
			"Drop Function Client_MFI_JobProfitForConsol",
			DbRoutineType.SqlFunctionTableTypeDesc
			),

			new DatabaseViewAndRoutineCreateScript(
			"Client_MFI_JobProfitForShipmentOrJobDec",
			Client_MFI_JobProfitForShipmentOrJobDecScript,
			"Drop Function Client_MFI_JobProfitForShipmentOrJobDec",
			DbRoutineType.SqlFunctionTableTypeDesc
			)
		);

		#region Client_ShipmentOrgsScript

		const string Client_ShipmentOrgsScript = @"
		CREATE View Client_ShipmentOrgs
AS
SELECT
		JS_PK,
		Consignor.OH_PK AS ConsignorPK,    
		Consignee.OH_PK AS ConsigneePK
FROM
(
	SELECT 
		 E2_ParentID AS JS_PK,
		 MAX( CASE E2_AddressType WHEN 'CRD' THEN OH_Code END ) AS JS_E2_OA_OH_NKConsignor,
		 MAX( CASE E2_AddressType WHEN 'CED' THEN OH_Code END ) AS JS_E2_OA_OH_NKConsignee
	FROM    
	   dbo.JobDocAddress        
	 LEFT JOIN     
	(     
	   OrgAddress     
	   INNER JOIN dbo.OrgHeader ON OA_OH = OH_PK    
	) ON E2_OA_Address = OA_PK  
	WHERE E2_AddressType in ('CED', 'CRD') 
	AND E2_ParentTableCode = 'JS'    
	GROUP BY E2_ParentID 	
) AS JobShipmentCNECNR
INNER JOIN dbo.OrgHeader Consignor ON JS_E2_OA_OH_NKConsignor = Consignor.OH_Code
INNER JOIN dbo.OrgHeader Consignee ON JS_E2_OA_OH_NKConsignee = Consignee.OH_Code
		";

		#endregion

		#region Client_JobDecOrgsScript

		const string Client_JobDecOrgsScript = @"
CREATE View Client_JobDecOrgs
AS
SELECT
		JE_PK,
		Supplier.OH_PK AS SupplierPK,    
		Importer.OH_PK AS ImporterPK
FROM
(
	SELECT 
		 E2_ParentID AS JE_PK,
		 MAX( CASE E2_AddressType WHEN 'SUD' THEN OH_Code END ) AS JS_E2_OA_OH_NKSupplier,
		 MAX( CASE E2_AddressType WHEN 'IMD' THEN OH_Code END ) AS JS_E2_OA_OH_NKImporter
	FROM    
	   dbo.JobDocAddress        
	 LEFT JOIN     
	(     
	   OrgAddress     
	   INNER JOIN dbo.OrgHeader ON OA_OH = OH_PK    
	) ON E2_OA_Address = OA_PK  
	WHERE E2_AddressType in ('IMD', 'SUD') 
	AND E2_ParentTableCode = 'JE'    
	GROUP BY E2_ParentID 	
) AS JobShipmentCNECNR
INNER JOIN dbo.OrgHeader Supplier ON JS_E2_OA_OH_NKSupplier = Supplier.OH_Code
INNER JOIN dbo.OrgHeader Importer ON JS_E2_OA_OH_NKImporter = Importer.OH_Code
";

		#endregion

		#region Client_MFI_JobProfitForConsolScript

		const string Client_MFI_JobProfitForConsolScript = @"
CREATE FUNCTION Client_MFI_JobProfitForConsol  
(    
  @JH_GC UNIQUEIDENTIFIER,      
  @AL_BranchPKList NVARCHAR(3899) = NULL, --Allows exactly 100 quote delimited comma separated PKs.  This also allows enough left over space on a string to properly format a where clause for any field in the system      
  @AL_DepartmentPKList NVARCHAR(3899) = NULL,        
  @AL_FromDate DATETIME,       
  @AL_ToDate DATETIME,      
  @CurrentCountry CHAR(2),    
  @TariffLvl CHAR(2),
  @AL_TransactionCreditorPKList NVARCHAR(3899) = NULL,
  @AL_TransactionDebtorPKList NVARCHAR(3899) = NULL,
  @AL_ChargeCodePKList NVARCHAR(3899) = NULL,
  @AC_ChargeGroup VARCHAR(3),
  @JS_PackingMode VARCHAR(3) = '', 
  @JS_RL_NKOrigin NVARCHAR(5) = NULL, 
  @JS_RL_NKDestination NVARCHAR(5) = NULL, 
  @JS_E_DEP_From DATETIME, 
  @JS_E_DEP_To DATETIME,
  @JS_E_ARV_From DATETIME, 
  @JS_E_ARV_To DATETIME,
  @JS_SystemCreateTime_From DATETIME, 
  @JS_SystemCreateTime_To DATETIME,  
  @JS_UniqueConsignRef NVARCHAR(20), 
  @JH_Status CHAR(3), 
  @JH_A_JCL_From DATETIME, 
  @JH_A_JCL_To DATETIME, 
  @JH_GS_SalesRep UNIQUEIDENTIFIER, 
  @JH_GS_OpsRep UNIQUEIDENTIFIER, 
  @JH_OH_LocalCharges NVARCHAR(3899), 
  @JH_OH_AgentCollect NVARCHAR(3899), 
  @JH_GB NVARCHAR(3899), 
  @JH_GE NVARCHAR(3899)
)    
RETURNS @JobProfit TABLE   
(  
 JK_UniqueConsignRef  varchar(35),  
 FW_TransportMode char(3),
 JK_TransportMode char(3),
 JK_ConsolMode varchar(3),  
 JK_AgentType varchar(4000),  
 JK_Load varchar(5),  
 JK_Discharge varchar(5),  
 JK_ETD smalldatetime,  
 JK_ETA smalldatetime,  
 AL_LineAmount decimal,  
 WIPAmount decimal(12,2),  
 REVAmount decimal(12,2),  
 CSTAmount decimal(12,2),  
 ACRAmount decimal(12,2),  
 JH_Profit decimal(12,2),
 SendingForwarderPK uniqueidentifier,
 ReceivingForwarderPK uniqueidentifier,
 CarrierPK uniqueidentifier,
 CreditorPK uniqueidentifier
)
BEGIN  
 SET @TariffLvl = (CASE WHEN ISNUMERIC(@TariffLvl) = 1 THEN @TariffLvl ELSE '' END)	   
 SET @AL_FromDate =  CONVERT(VARCHAR(100),@AL_FromDate, 101)  
 SET @AL_ToDate = dbo.GetValidEndDate(@AL_ToDate)  

  INSERT @JobProfit  
 SELECT   
   JK_UniqueConsignRef,    
   JK_TransportMode as FW_TransportMode,
   JK_TransportMode,     
   JK_ConsolMode,     
   JK_AgentType,     
   JW_RL_NKLoadPort as JK_Load,    
   JW_RL_NKDiscPort as JK_Discharge,    
   JW_TD as JK_ETD,    
   JW_TA as JK_ETA,
 SummedConsolProfit.AL_LineAmount,  
 SummedConsolProfit.WIPAmount,  
 SummedConsolProfit.REVAmount,  
 SummedConsolProfit.CSTAmount,  
 SummedConsolProfit.ACRAmount,  
 SummedConsolProfit.JH_Profit,  
 (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_SendingForwarderAddress) AS SendingForwarderPK,
 (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_ReceivingForwarderAddress) AS ReceivingForwarderPK,
 (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_ShippingLineAddress) AS CarrierPK,
 (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_CreditorAddress) AS CreditorPK
FROM dbo.JobConsol  
INNER JOIN  
(  
 SELECT    
     JN_JK as ConsolPK,
	 SUM(CASE WHEN AL_LineType IN ('WIP', 'ACR') THEN -AL_LineAmount ELSE AL_LineAmount END  * CASE WHEN AL_LineType in ('REV', 'CST') OR AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) as AL_LineAmount,  
       SUM(CASE WHEN AL_LineType = 'WIP' THEN -AL_LineAmount ELSE 0 END * CASE WHEN AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) as WIPAmount,       
       SUM(CASE WHEN AL_LineType = 'REV' THEN AL_LineAmount ELSE 0 END) as REVAmount,       
       SUM(CASE WHEN AL_LineType = 'CST' THEN AL_LineAmount ELSE 0 END) as CSTAmount,       
       SUM(CASE WHEN AL_LineType = 'ACR' THEN -AL_LineAmount ELSE 0 END * CASE WHEN AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) as ACRAmount,      
       SUM(CASE WHEN AL_LineType IN ('WIP', 'ACR') THEN -AL_LineAmount ELSE AL_LineAmount END  * CASE WHEN AL_LineType in ('REV', 'CST') OR AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) As JH_Profit      
 FROM dbo.JobHeader
 LEFT JOIN dbo.OrgAddress As LocalChargesAddress ON JH_OA_LocalChargesAddr = LocalChargesAddress.OA_PK
 LEFT JOIN dbo.OrgAddress As AgentCollectAddress ON JH_OA_AgentCollectAddr = AgentCollectAddress.OA_PK
 LEFT JOIN dbo.GLBStaff As RepSales ON JH_GS_NKRepSales = RepSales.GS_Code
 LEFT JOIN dbo.GLBStaff As Operator ON JH_GS_NKRepOps = Operator.GS_Code
 INNER JOIN dbo.JobConShipLink ON JN_JS = JH_ParentID  
 INNER JOIN  
 (  
  SELECT Shipment.JS_PK as LINK,  
    (  CASE WHEN OrgPK IS NULL THEN NULL  
        ELSE CASE WHEN DEF IS NOT NULL  
             THEN DEF  
             ELSE CASE WHEN FRT IS NOT NULL  
                  THEN FRT  
                  ELSE CASE WHEN ORG IS NOT NULL  
                       THEN ORG  
                       ELSE CASE WHEN DST IS NOT NULL  
                            THEN DST  
                            ELSE CASE WHEN DST IS NOT NULL  
                                 THEN DST  
                                 ELSE CASE WHEN CFS IS NOT NULL  
                                      THEN CFS  
                                      ELSE CASE WHEN WHS IS NOT NULL  
                                       THEN WHS  
                                       ELSE CASE WHEN TRN IS NOT NULL  
                                            THEN TRN  
                                            ELSE NULL  
                                           END   
                                       END  
                                  END   
                             END  
                        END  
                   END  
              END  
         END  
    END         
   ) AS OrgTariffLvl  
  FROM csfn_JobShipmentWithDirection(@CurrentCountry) Shipment  
  LEFT JOIN Client_ShipmentOrgs ON Client_ShipmentOrgs.JS_PK = Shipment.JS_PK    
  LEFT JOIN  
  (  
     SELECT   
      P7_OH As OrgPK,    
      MAX(CASE WHEN P7_TariffType = 'DEF' THEN P7_TariffLevel END) AS DEF,  
      MAX(CASE WHEN P7_TariffType = 'FRT' THEN P7_TariffLevel END) AS FRT,  
      MAX(CASE WHEN P7_TariffType = 'ORG' THEN P7_TariffLevel END) AS ORG,  
      MAX(CASE WHEN P7_TariffType = 'DST' THEN P7_TariffLevel END) AS DST,  
      MAX(CASE WHEN P7_TariffType = 'CFS' THEN P7_TariffLevel END) AS CFS,  
      MAX(CASE WHEN P7_TariffType = 'WHS' THEN P7_TariffLevel END) AS WHS,  
      MAX(CASE WHEN P7_TariffType = 'TRN' THEN P7_TariffLevel END) AS TRN  
     FROM dbo.OrgRateTariffLevel  
     WHERE  
     P7_GC = @JH_GC AND P7_TariffLevel != 0  
     Group by P7_OH  
   ) TariffLevel ON (CASE JS_Direction WHEN 'IMP' THEN ConsigneePK ELSE ConsignorPK END) = TariffLevel.OrgPK  
   WHERE JS_IsForwardRegistered = 1
	AND (@JS_PackingMode = '' OR JS_PackingMode = @JS_PackingMode)   
	AND (@JS_RL_NKOrigin = '' OR JS_RL_NKOrigin = @JS_RL_NKOrigin)
	AND (@JS_RL_NKDestination = '' OR JS_RL_NKDestination = @JS_RL_NKDestination)

	AND (@JS_E_DEP_From = '' OR JS_E_DEP >= @JS_E_DEP_From)
	AND (@JS_E_DEP_To = '' OR JS_E_DEP <= @JS_E_DEP_To)
	AND (@JS_E_ARV_From = ' ' OR JS_E_ARV >= @JS_E_ARV_From)
	AND (@JS_E_ARV_To = '' OR JS_E_ARV <= @JS_E_ARV_To)
	AND (@JS_SystemCreateTime_From = '' OR JS_SystemCreateTimeUtc >= @JS_SystemCreateTime_From)
	AND (@JS_SystemCreateTime_To = '' OR JS_SystemCreateTimeUtc <= @JS_SystemCreateTime_To)

	AND (@JS_UniqueConsignRef = '' OR JS_UniqueConsignRef = @JS_UniqueConsignRef)
   ) ShipmentLink on ShipmentLink.LINK = JN_JS  AND (OrgTariffLvl = @TariffLvl OR @TariffLvl  = '')   
   INNER JOIN dbo.AccTransactionLines ATLOutter WITH (index (NR_RX__AL_PostDate_AL_ReverseDate_AL_JH) )    
   ON AL_AC IS NOT NULL AND   
    JH_PK = AL_JH AND    
	(  
		(
			AL_LineType in ('ACR', 'WIP') 
			AND
			(
				(  
					AL_ReverseDate BETWEEN @AL_FromDate AND @AL_ToDate 
					AND (AL_PostDate < @AL_FromDate OR AL_PostDate >= @AL_ToDate )
				)
				OR  
				(  
					AL_PostDate BETWEEN @AL_FromDate AND @AL_ToDate AND  
					(AL_ReverseDate IS NULL OR AL_ReverseDate < @AL_FromDate OR AL_ReverseDate >= @AL_ToDate)
				)
			)
		)
		OR
		(
			AL_LineType in ('REV', 'CST') 
			AND AL_ReverseDate >= @AL_FromDate AND AL_ReverseDate < @AL_ToDate 
		)  
	)  
	LEFT JOIN dbo.AccChargeCode ON AL_AC = AC_PK
 WHERE JH_GC = @JH_GC  
 AND (@AL_BranchPKList = '' or CHARINDEX(CAST(AL_GB AS VARCHAR(50)),@AL_BranchPKList) > 0)    
 AND (@AL_DepartmentPKList = '' or CHARINDEX(CAST(AL_GE AS VARCHAR(50)),@AL_DepartmentPKList) > 0)      
 AND (@AL_TransactionCreditorPKList = '' or (CHARINDEX(CAST(AL_OH AS VARCHAR(50)),@AL_TransactionCreditorPKList) > 0 AND (AL_LineType = 'ACR' or AL_LineType = 'CST')))
 AND (@AL_TransactionDebtorPKList = '' or (CHARINDEX(CAST(AL_OH AS VARCHAR(50)),@AL_TransactionDebtorPKList) > 0 AND (AL_LineType = 'WIP' or AL_LineType = 'REV')))
 AND (@AL_ChargeCodePKList = '' or CHARINDEX(CAST(AL_AC AS VARCHAR(50)),@AL_ChargeCodePKList) > 0)
 AND (@AC_ChargeGroup = '' or AC_ChargeGroup = @AC_ChargeGroup)
 AND (@JH_Status = '' OR JH_Status = @JH_Status)
 AND (@JH_A_JCL_From = '' OR JH_A_JCL >= @JH_A_JCL_From)
 AND (@JH_A_JCL_To = '' OR JH_A_JCL <= @JH_A_JCL_To)
 AND (@JH_GS_SalesRep is NULL OR RepSales.GS_PK = @JH_GS_SalesRep)
 AND (@JH_GS_OpsRep is NULL OR Operator.GS_PK = @JH_GS_OpsRep)
 AND (@JH_OH_LocalCharges = '' OR CHARINDEX(CAST(LocalChargesAddress.OA_OH AS VARCHAR(50)),@JH_OH_LocalCharges) > 0)  
 AND (@JH_OH_AgentCollect = '' OR CHARINDEX(CAST(AgentCollectAddress.OA_OH AS VARCHAR(50)),@JH_OH_AgentCollect) > 0)
 AND (@JH_GB = '' OR CHARINDEX(CAST(JH_GB AS VARCHAR(50)),@JH_GB) > 0)
 AND (@JH_GE = '' OR CHARINDEX(CAST(JH_GE AS VARCHAR(50)),@JH_GE) > 0)
 Group by JN_JK
) SummedConsolProfit ON SummedConsolProfit.ConsolPK = JK_PK  
LEFT JOIN csfn_MainConsolTransport(@CurrentCountry) ON JW_JK = JobConsol.JK_PK    
ORDER BY JK_UniqueConsignRef, JK_TransportMode, JK_ConsolMode, JK_Load, JK_Discharge, JK_ETD, JK_ETA  

RETURN  
  
END
";

		#endregion

		#region Client_MFI_JobProfitForShipmentOrJobDecScript

		const string Client_MFI_JobProfitForShipmentOrJobDecScript = @"
CREATE FUNCTION Client_MFI_JobProfitForShipmentOrJobDec  
(  
  @JH_GC UNIQUEIDENTIFIER,    
  @AL_BranchPKList NVARCHAR(3899) = NULL, --Allows exactly 100 quote delimited comma separated PKs.  This also allows enough left over space on a string to properly format a where clause for any field in the system    
  @AL_DepartmentPKList NVARCHAR(3899) = NULL,      
  @AL_FromDate DATETIME,     
  @AL_ToDate DATETIME,    
  @CurrentCountry CHAR(2),  
  @TariffLvl CHAR(2),
  @AL_TransactionCreditorPKList NVARCHAR(3899) = NULL,
  @AL_TransactionDebtorPKList NVARCHAR(3899) = NULL,
  @AL_ChargeCodePKList NVARCHAR(3899) = NULL,
  @AC_ChargeGroup VARCHAR(3) = '',
  @JS_PackingMode VARCHAR(3) = '', 
  @JS_RL_NKOrigin NVARCHAR(5) = NULL, 
  @JS_RL_NKDestination NVARCHAR(5) = NULL, 
  @JS_E_DEP_From DATETIME, 
  @JS_E_DEP_To DATETIME,
  @JS_E_ARV_From DATETIME, 
  @JS_E_ARV_To DATETIME,
  @JS_SystemCreateTime_From DATETIME, 
  @JS_SystemCreateTime_To DATETIME,  
  @JS_UniqueConsignRef NVARCHAR(20), 
  @JH_Status CHAR(3), 
  @JH_A_JCL_From DATETIME, 
  @JH_A_JCL_To DATETIME, 
  @JH_GS_SalesRep UNIQUEIDENTIFIER, 
  @JH_GS_OpsRep UNIQUEIDENTIFIER, 
  @JH_OH_LocalCharges NVARCHAR(3899), 
  @JH_OH_AgentCollect NVARCHAR(3899), 
  @JH_GB NVARCHAR(3899), 
  @JH_GE NVARCHAR(3899)
)  
RETURNS @JobProfit TABLE 
(
	JobType char(1),
	FW_PK uniqueidentifier,
	FW_TransportMode varchar(3),
	FW_ContainerMode varchar(3),
	FW_Consols varchar(4000),
	OrgTariffLvl char(2),
	JH_JobNum varchar(35),
	JH_Status char(3),
	JH_BranchCode char(3),
	JH_DepartmentCode char(3),
	JH_OperatorInitials char(3),
	JH_SalesRepInitials char(3),
	AL_LineAmount decimal(12,2),
	WIPAmount decimal(12,2),
	REVAmount decimal(12,2),
	CSTAmount decimal(12,2),
	ACRAmount decimal(12,2),
	JH_Profit decimal(12,2),
	JH_SystemCreateTimeUtc smalldatetime,
	JH_A_JCL smalldatetime,
	JH_GS_SalesRep uniqueidentifier,
	JH_GS_OpsRep uniqueidentifier,
	JH_OH_LocalCharges uniqueidentifier,
	JH_OH_AgentCollect uniqueidentifier,
	SendingForwarderPK uniqueidentifier,
    ReceivingForwarderPK uniqueidentifier,
    CarrierPK uniqueidentifier,
    CreditorPK uniqueidentifier,
	JH_GB uniqueidentifier,
    JH_GE uniqueidentifier,
    JS_RL_NKOrigin varchar(5),
    JS_RL_NKDestination varchar(5),
	JS_ETD smalldatetime,
    JS_ETA smalldatetime,
    JS_SystemCreateTimeUtc smalldatetime,
	JS_UniqueConsignRef varchar(20),
	JK_ConsolMode varchar(3),
	JK_TransportMode varchar(3),
	JK_Load varchar(5),    
    JK_Discharge varchar(5),    
    JK_ETD smalldatetime,    
    JK_ETA smalldatetime,
	JK_UniqueConsignRef varchar(20),
	JK_AgentType varchar(3)
)

BEGIN
 SET @TariffLvl = (CASE WHEN ISNUMERIC(@TariffLvl) = 1 THEN @TariffLvl ELSE '' END)	     
 SET @AL_FromDate = 	CONVERT(VARCHAR(100),@AL_FromDate, 101)
 SET @AL_ToDate = dbo.GetValidEndDate(@AL_ToDate)

 INSERT @JobProfit
  SELECT     
   'S' AS JobType,  
   JS_PK as FW_PK,     
   JS_TransportMode as FW_TransportMode,    
   JS_PackingMode as FW_ContainerMode,     
   (SELECT UniqueConsignRef FROM dbo.getConsolsStringForShipment(JS_PK)) AS FW_Consols,   
   OrgTariffLvl,
       
   JH_JobNum,    
   JH_Status,    
   JHBranch.GB_Code as JH_BranchCode,    
   JHDept.GE_Code as JH_DepartmentCode,     
   JHOperator.GS_Code as JH_OperatorInitials,    
   JHSalesRep.GS_Code as JH_SalesRepInitials,    
   AL_LineAmount,    
   WIPAmount,     
   REVAmount,     
   CSTAmount,     
   ACRAmount,    
   JH_Profit,
   JH_SystemCreateTimeUtc,
   JH_A_JCL,
   JHSalesRep.GS_PK as JH_GS_SalesRep,
   JHOperator.GS_PK as JH_GS_OpsRep,
   LocalChargesAddress.OA_OH,
   AgentCollectAddress.OA_OH,
   (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_SendingForwarderAddress) AS SendingForwarderPK,
   (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_ReceivingForwarderAddress) AS ReceivingForwarderPK,
   (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_ShippingLineAddress) AS CarrierPK,
   (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_CreditorAddress) AS CreditorPK,
   JH_GB,
   JH_GE,
   JS_RL_NKOrigin,
   JS_RL_NKDestination,
   JS_E_DEP as JS_ETD,
   JS_E_ARV as JS_ETA,
   JS_SystemCreateTimeUtc,
   JS_UniqueConsignRef,
   JK_ConsolMode,
   JK_TransportMode,
   JW_RL_NKLoadPort as JK_Load,    
   JW_RL_NKDiscPort as JK_Discharge,    
   JW_TD as JK_ETD,    
   JW_TA as JK_ETA,
   JK_UniqueConsignRef,
   JK_AgentType
  FROM csfn_JobShipmentWithDirection(@CurrentCountry)   
  INNER JOIN dbo.JobHeader on JS_PK = JH_ParentID AND JH_GC = @JH_GC
  LEFT JOIN dbo.OrgAddress As LocalChargesAddress ON JH_OA_LocalChargesAddr = LocalChargesAddress.OA_PK
  LEFT JOIN dbo.OrgAddress As AgentCollectAddress ON JH_OA_AgentCollectAddr = AgentCollectAddress.OA_PK
  INNER JOIN dbo.GLBBranch JHBranch ON JH_GB = JHBranch.GB_PK    
  INNER JOIN dbo.GLBDepartment JHDept ON JH_GE = JHDept.GE_PK      
  LEFT JOIN dbo.GLBStaff JHOperator ON JH_GS_NKRepOps = JHOperator.GS_Code
  LEFT JOIN dbo.GLBStaff JHSalesRep ON JH_GS_NKRepSales = JHSalesRep.GS_Code    
  LEFT JOIN dbo.JobConshipLink ON JS_PK = JobConshipLink.JN_JS 
  LEFT JOIN dbo.JobConsol ON JN_JK = JobConsol.JK_PK
  LEFT JOIN csfn_MainConsolTransport(@CurrentCountry) ON JW_JK = JobConsol.JK_PK
  INNER JOIN  
  (  
    SELECT   
      JH_ParentID AS Link, 
      SUM(CASE WHEN AL_LineType IN ('WIP', 'ACR') THEN -AL_LineAmount ELSE AL_LineAmount END  * CASE WHEN AL_LineType in ('REV', 'CST') OR AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) as AL_LineAmount,
      SUM(CASE WHEN AL_LineType = 'WIP' THEN -AL_LineAmount ELSE 0 END * CASE WHEN AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) as WIPAmount,     
      SUM(CASE WHEN AL_LineType = 'REV' THEN AL_LineAmount ELSE 0 END ) as REVAmount,     
      SUM(CASE WHEN AL_LineType = 'CST' THEN AL_LineAmount ELSE 0 END ) as CSTAmount,     
      SUM(CASE WHEN AL_LineType = 'ACR' THEN -AL_LineAmount ELSE 0 END * CASE WHEN AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) as ACRAmount,    
      SUM(CASE WHEN AL_LineType IN ('WIP', 'ACR') THEN -AL_LineAmount ELSE AL_LineAmount END  * CASE WHEN AL_LineType in ('REV', 'CST') OR AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) As JH_Profit    
	FROM dbo.JobHeader
    LEFT JOIN dbo.OrgAddress As LocalChargesAddress ON JH_OA_LocalChargesAddr = LocalChargesAddress.OA_PK
    LEFT JOIN dbo.OrgAddress As AgentCollectAddress ON JH_OA_AgentCollectAddr = AgentCollectAddress.OA_PK
    LEFT JOIN dbo.GLBStaff Operator ON JH_GS_NKRepOps = Operator.GS_Code    
	LEFT JOIN dbo.GLBStaff SalesRep ON JH_GS_NKRepSales = SalesRep.GS_Code
    INNER JOIN dbo.AccTransactionLines ATLOutter WITH (index (NR_RX__AL_PostDate_AL_ReverseDate_AL_JH) )     
    ON 
	  JH_GC = @JH_GC
	  AND
	  AL_AC IS NOT NULL	
	 AND   
     JH_PK = AL_JH   
     AND    
	(  
		(
			AL_LineType in ('ACR', 'WIP') 
			AND
			(
				(  
					AL_ReverseDate BETWEEN @AL_FromDate AND @AL_ToDate 
					AND (AL_PostDate < @AL_FromDate OR AL_PostDate >= @AL_ToDate )
				)
				OR  
				(  
					AL_PostDate BETWEEN @AL_FromDate AND @AL_ToDate AND  
					(AL_ReverseDate IS NULL OR AL_ReverseDate < @AL_FromDate OR AL_ReverseDate >= @AL_ToDate)
				)
			)
		)
		OR
		(
			AL_LineType in ('REV', 'CST') 
			AND AL_ReverseDate >= @AL_FromDate AND AL_ReverseDate < @AL_ToDate 
		)  
	)  
	LEFT JOIN dbo.AccChargeCode ON AL_AC = AC_PK
	WHERE (@AL_BranchPKList = '' or CHARINDEX(CAST(AL_GB AS VARCHAR(50)),@AL_BranchPKList) > 0)  
    AND (@AL_DepartmentPKList = '' or CHARINDEX(CAST(AL_GE AS VARCHAR(50)),@AL_DepartmentPKList) > 0)  
    AND (@AL_TransactionCreditorPKList = '' or (CHARINDEX(CAST(AL_OH AS VARCHAR(50)),@AL_TransactionCreditorPKList) > 0 AND (AL_LineType = 'ACR' or AL_LineType = 'CST')))
	AND (@AL_TransactionDebtorPKList = '' or (CHARINDEX(CAST(AL_OH AS VARCHAR(50)),@AL_TransactionDebtorPKList) > 0 AND (AL_LineType = 'WIP' or AL_LineType = 'REV')))
	AND (@AL_ChargeCodePKList = '' or CHARINDEX(CAST(AL_AC AS VARCHAR(50)),@AL_ChargeCodePKList) > 0)
	AND (@AC_ChargeGroup = '' or AC_ChargeGroup = @AC_ChargeGroup)
    AND (@JH_Status = '' OR JH_Status = @JH_Status)
    AND (@JH_A_JCL_From = '' OR JH_A_JCL >= @JH_A_JCL_From)
    AND (@JH_A_JCL_To = '' OR JH_A_JCL <= @JH_A_JCL_To)
    AND (@JH_GS_SalesRep is NULL OR SalesRep.GS_PK = @JH_GS_SalesRep)
    AND (@JH_GS_OpsRep is NULL OR Operator.GS_PK = @JH_GS_OpsRep)
    AND (@JH_OH_LocalCharges = '' OR CHARINDEX(CAST(LocalChargesAddress.OA_OH AS VARCHAR(50)),@JH_OH_LocalCharges) > 0)  
    AND (@JH_OH_AgentCollect = '' OR CHARINDEX(CAST(AgentCollectAddress.OA_OH AS VARCHAR(50)),@JH_OH_AgentCollect) > 0)
    AND (@JH_GB = '' OR CHARINDEX(CAST(JH_GB AS VARCHAR(50)),@JH_GB) > 0)
    AND (@JH_GE = '' OR CHARINDEX(CAST(JH_GE AS VARCHAR(50)),@JH_GE) > 0)
   GROUP BY JH_ParentID    
   ) SummedJobProfit ON SummedJobProfit.Link = JS_PK
   LEFT JOIN  
   (  
   SELECT  
     Shipment.JS_PK AS LINK,   
     (  
      CASE WHEN OrgPK IS NULL THEN NULL  
                  ELSE CASE WHEN DEF IS NOT NULL  
                        THEN DEF  
                        ELSE CASE WHEN FRT IS NOT NULL  
                             THEN FRT  
                             ELSE CASE WHEN ORG IS NOT NULL  
                                THEN ORG  
                                ELSE CASE WHEN DST IS NOT NULL  
                                   THEN DST  
                                   ELSE CASE WHEN DST IS NOT NULL  
                                      THEN DST  
                                      ELSE CASE WHEN CFS IS NOT NULL  
                                           THEN CFS  
                                           ELSE CASE WHEN WHS IS NOT NULL  
                                            THEN WHS  
                                            ELSE CASE WHEN TRN IS NOT NULL  
                                                 THEN TRN  
                                                 ELSE NULL  
                                                END   
                                            END  
                                       END   
                                    END  
                                 END  
                            END  
                         END  
                      END  
        END          
   ) AS OrgTariffLvl  
   FROM  
   csfn_JobShipmentWithDirection(@CurrentCountry) Shipment  
   INNER JOIN Client_ShipmentOrgs ON Client_ShipmentOrgs.JS_PK = Shipment.JS_PK   
   LEFT JOIN  
    (  
     SELECT   
      P7_OH As OrgPK,    
      MAX(CASE WHEN P7_TariffType = 'DEF' THEN P7_TariffLevel END) AS DEF,  
      MAX(CASE WHEN P7_TariffType = 'FRT' THEN P7_TariffLevel END) AS FRT,  
      MAX(CASE WHEN P7_TariffType = 'ORG' THEN P7_TariffLevel END) AS ORG,  
      MAX(CASE WHEN P7_TariffType = 'DST' THEN P7_TariffLevel END) AS DST,  
      MAX(CASE WHEN P7_TariffType = 'CFS' THEN P7_TariffLevel END) AS CFS,  
      MAX(CASE WHEN P7_TariffType = 'WHS' THEN P7_TariffLevel END) AS WHS,  
      MAX(CASE WHEN P7_TariffType = 'TRN' THEN P7_TariffLevel END) AS TRN  
     FROM dbo.OrgRateTariffLevel  
     WHERE  
     P7_GC = @JH_GC AND P7_TariffLevel != 0  
     Group by P7_OH  
    ) TariffLevel ON (CASE JS_Direction WHEN 'IMP' THEN ConsigneePK ELSE ConsignorPK END) = TariffLevel.OrgPK )  TariffLink ON TariffLink.Link = JS_PK  
    WHERE JH_GC = @JH_GC  
    AND JS_IsForwardRegistered = 1    
    AND (OrgTariffLvl = @TariffLvl OR @TariffLvl  = '') 
	AND (@JS_PackingMode = '' OR JS_PackingMode = @JS_PackingMode)   
	AND (@JS_RL_NKOrigin = '' OR JS_RL_NKOrigin = @JS_RL_NKOrigin)
	AND (@JS_RL_NKDestination = '' OR JS_RL_NKDestination = @JS_RL_NKDestination)
	AND (@JS_E_DEP_From = '' OR JS_E_DEP >= @JS_E_DEP_From)
	AND (@JS_E_DEP_To = '' OR JS_E_DEP <= @JS_E_DEP_To)
	AND (@JS_E_ARV_From = ' ' OR JS_E_ARV >= @JS_E_ARV_From)
	AND (@JS_E_ARV_To = '' OR JS_E_ARV <= @JS_E_ARV_To)
	AND (@JS_SystemCreateTime_From = '' OR JS_SystemCreateTimeUtc >= @JS_SystemCreateTime_From)
	AND (@JS_SystemCreateTime_To = '' OR JS_SystemCreateTimeUtc <= @JS_SystemCreateTime_To)
	AND (@JS_UniqueConsignRef = '' OR JS_UniqueConsignRef = @JS_UniqueConsignRef) 
    
   UNION 
  
 SELECT   
   'B' AS JobType,    
   JE_PK as FW_PK,     
   JE_TransportMode as FW_TransportMode,    
   JE_ContainerMode as FW_ContainerMode,     
   NULL AS FW_Consols,    
   OrgTariffLvl,  
       
   JH_JobNum,    
   JH_Status,    
   JHBranch.GB_Code as JH_BranchCode,    
   JHDept.GE_Code as JH_DepartmentCode,     
   JHOperator.GS_Code as JH_OperatorInitials,    
   JHSalesRep.GS_Code as JH_SalesRepInitials,             
   AL_LineAmount,    
   WIPAmount,     
   REVAmount,     
   CSTAmount,     
   ACRAmount,    
   JH_Profit,
   JH_SystemCreateTimeUtc,
   JH_A_JCL,
   JHSalesRep.GS_PK as JH_GS_SalesRep,
   JHOperator.GS_PK as JH_GS_OpsRep,
   LocalChargesAddress.OA_OH,
   AgentCollectAddress.OA_OH,
   (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_SendingForwarderAddress) AS SendingForwarderPK,
   (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_ReceivingForwarderAddress) AS ReceivingForwarderPK,
   (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_ShippingLineAddress) AS CarrierPK,
   (select OA_OH from dbo.OrgAddress where OA_PK = JK_OA_CreditorAddress) AS CreditorPK,
   JH_GB,
   JH_GE,
   JE_RL_NKOrigin as JS_RL_NKOrigin,
   JE_RL_NKPortOfArrival as JS_RL_NKDestination,
   JE_DateAtOrigin as JS_ETD,
   JE_DateOfArrival as JS_ETA,
   JE_SystemCreateTimeUtc as JS_SystemCreateTimeUtc,
   JE_DeclarationReference as JS_UniqueConsignRef,
   JK_ConsolMode,
   JK_TransportMode,
   JW_RL_NKLoadPort as JK_Load,    
   JW_RL_NKDiscPort as JK_Discharge,    
   JW_TD as JK_ETD,    
   JW_TA as JK_ETA,
   NULL as JK_UniqueConsignRef,
   JK_AgentType
  FROM csfn_JobDeclarationWithDirection(@CurrentCountry)   
  INNER JOIN dbo.JobHeader on JE_PK = JH_ParentID AND JH_GC = @JH_GC    
  LEFT JOIN dbo.OrgAddress As LocalChargesAddress ON JH_OA_LocalChargesAddr = LocalChargesAddress.OA_PK
  LEFT JOIN dbo.OrgAddress As AgentCollectAddress ON JH_OA_AgentCollectAddr = AgentCollectAddress.OA_PK
  INNER JOIN dbo.GLBBranch JHBranch ON JH_GB = JHBranch.GB_PK    
  INNER JOIN dbo.GLBDepartment JHDept ON JH_GE = JHDept.GE_PK      
  LEFT JOIN dbo.GLBStaff JHOperator ON JH_GS_NKRepOps = JHOperator.GS_Code    
  LEFT JOIN dbo.GLBStaff JHSalesRep ON JH_GS_NKRepSales = JHSalesRep.GS_Code  
  LEFT JOIN dbo.JobShipment ON JS_PK = JE_JS
  LEFT JOIN dbo.JobConshipLink ON JE_JS = JobConshipLink.JN_JS 
  LEFT JOIN dbo.JobConsol ON JN_JK = JobConsol.JK_PK
  LEFT JOIN csfn_MainConsolTransport(@CurrentCountry) ON JW_JK = JobConsol.JK_PK
  INNER JOIN  
  (  
    SELECT   
      JH_ParentID AS Link,
	  SUM(CASE WHEN AL_LineType IN ('WIP', 'ACR') THEN -AL_LineAmount ELSE AL_LineAmount END  * CASE WHEN AL_LineType in ('REV', 'CST') OR AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) as AL_LineAmount,
      SUM(CASE WHEN AL_LineType = 'WIP' THEN -AL_LineAmount ELSE 0 END * CASE WHEN AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) as WIPAmount,     
      SUM(CASE WHEN AL_LineType = 'REV' THEN AL_LineAmount ELSE 0 END ) as REVAmount,     
      SUM(CASE WHEN AL_LineType = 'CST' THEN AL_LineAmount ELSE 0 END ) as CSTAmount,     
      SUM(CASE WHEN AL_LineType = 'ACR' THEN -AL_LineAmount ELSE 0 END * CASE WHEN AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) as ACRAmount,    
      SUM(CASE WHEN AL_LineType IN ('WIP', 'ACR') THEN -AL_LineAmount ELSE AL_LineAmount END  * CASE WHEN AL_LineType in ('REV', 'CST') OR AL_ReverseDate IS NULL  OR AL_ReverseDate > @AL_ToDate THEN 1 ELSE -1 END ) As JH_Profit    
    FROM dbo.JobHeader   
    LEFT JOIN dbo.OrgAddress As LocalChargesAddress ON JH_OA_LocalChargesAddr = LocalChargesAddress.OA_PK
    LEFT JOIN dbo.OrgAddress As AgentCollectAddress ON JH_OA_AgentCollectAddr = AgentCollectAddress.OA_PK
    LEFT JOIN dbo.GLBStaff JHSalesRep ON JHSalesRep.GS_Code = JH_GS_NKRepSales
	LEFT JOIN dbo.GLBStaff JHOperator ON JHOperator.GS_Code = JH_GS_NKRepOps
    INNER JOIN dbo.AccTransactionLines ATLOutter WITH (index (NR_RX__AL_PostDate_AL_ReverseDate_AL_JH) )     
    ON     
	JH_PK = AL_JH 
	AND 
	 AL_AC IS NOT NULL  
     AND    
	(  
		(
			AL_LineType in ('ACR', 'WIP') 
			AND
			(
				(  
					AL_ReverseDate BETWEEN @AL_FromDate AND @AL_ToDate 
					AND (AL_PostDate < @AL_FromDate OR AL_PostDate >= @AL_ToDate )
				)
				OR  
				(  
					AL_PostDate BETWEEN @AL_FromDate AND @AL_ToDate AND  
					(AL_ReverseDate IS NULL OR AL_ReverseDate < @AL_FromDate OR AL_ReverseDate >= @AL_ToDate)
				)
			)
		)
		OR
		(
			AL_LineType in ('REV', 'CST') 
			AND AL_ReverseDate >= @AL_FromDate AND AL_ReverseDate < @AL_ToDate 
		)  
	)  
	LEFT JOIN dbo.AccChargeCode ON AL_AC = AC_PK
  	WHERE JH_GC = @JH_GC
    AND (@AL_BranchPKList = '' or CHARINDEX(CAST(AL_GB AS VARCHAR(50)),@AL_BranchPKList) > 0)  
    AND (@AL_DepartmentPKList = '' or CHARINDEX(CAST(AL_GE AS VARCHAR(50)),@AL_DepartmentPKList) > 0) 
    AND (@AL_TransactionCreditorPKList = '' or (CHARINDEX(CAST(AL_OH AS VARCHAR(50)),@AL_TransactionCreditorPKList) > 0 AND (AL_LineType = 'ACR' or AL_LineType = 'CST')))
	AND (@AL_TransactionDebtorPKList = '' or (CHARINDEX(CAST(AL_OH AS VARCHAR(50)),@AL_TransactionDebtorPKList) > 0 AND (AL_LineType = 'WIP' or AL_LineType = 'REV')))
	AND (@AL_ChargeCodePKList = '' or CHARINDEX(CAST(AL_AC AS VARCHAR(50)),@AL_ChargeCodePKList) > 0)
	AND (@AC_ChargeGroup = '' or AC_ChargeGroup = @AC_ChargeGroup)
	AND (@JH_Status = '' OR JH_Status = @JH_Status)
	AND (@JH_A_JCL_From = '' OR JH_A_JCL >= @JH_A_JCL_From)
	AND (@JH_A_JCL_To = '' OR JH_A_JCL <= @JH_A_JCL_To)
	AND (@JH_GS_SalesRep is NULL OR JHSalesRep.GS_PK = @JH_GS_SalesRep)
	AND (@JH_GS_OpsRep is NULL OR JHOperator.GS_PK = @JH_GS_OpsRep)
	AND (@JH_OH_LocalCharges = '' OR CHARINDEX(CAST(LocalChargesAddress.OA_OH AS VARCHAR(50)),@JH_OH_LocalCharges) > 0)  
	AND (@JH_OH_AgentCollect = '' OR CHARINDEX(CAST(AgentCollectAddress.OA_OH AS VARCHAR(50)),@JH_OH_AgentCollect) > 0)
	AND (@JH_GB = '' OR CHARINDEX(CAST(JH_GB AS VARCHAR(50)),@JH_GB) > 0)
	AND (@JH_GE = '' OR CHARINDEX(CAST(JH_GE AS VARCHAR(50)),@JH_GE) > 0)
    GROUP BY JH_ParentID    
   ) SummedJobProfit ON SummedJobProfit.Link = JE_PK 
   LEFT JOIN  
   (  
   SELECT  
     JobDec.JE_PK AS LINK,   
     (  
      CASE WHEN OrgPK IS NULL THEN NULL  
                  ELSE CASE WHEN DEF IS NOT NULL  
                        THEN DEF  
                        ELSE CASE WHEN FRT IS NOT NULL  
                             THEN FRT  
                             ELSE CASE WHEN ORG IS NOT NULL  
                                THEN ORG  
                                ELSE CASE WHEN DST IS NOT NULL  
                                   THEN DST  
                                   ELSE CASE WHEN DST IS NOT NULL  
                                      THEN DST  
                                      ELSE CASE WHEN CFS IS NOT NULL  
                                           THEN CFS  
                                           ELSE CASE WHEN WHS IS NOT NULL  
                                            THEN WHS  
                                            ELSE CASE WHEN TRN IS NOT NULL  
                                                 THEN TRN  
                                                 ELSE NULL  
                                                END   
                                            END  
                                       END   
                                    END  
                                 END  
                            END  
                         END  
                      END  
        END          
   ) AS OrgTariffLvl  
   FROM  
   csfn_JobDeclarationWithDirection(@CurrentCountry) JobDec  
   INNER JOIN Client_JobDecOrgs ON Client_JobDecOrgs.JE_PK = JobDec.JE_PK   
   LEFT JOIN  
    (  
     SELECT   
      P7_OH As OrgPK,    
      MAX(CASE WHEN P7_TariffType = 'DEF' THEN P7_TariffLevel END) AS DEF,  
      MAX(CASE WHEN P7_TariffType = 'FRT' THEN P7_TariffLevel END) AS FRT,  
      MAX(CASE WHEN P7_TariffType = 'ORG' THEN P7_TariffLevel END) AS ORG,  
      MAX(CASE WHEN P7_TariffType = 'DST' THEN P7_TariffLevel END) AS DST,  
      MAX(CASE WHEN P7_TariffType = 'CFS' THEN P7_TariffLevel END) AS CFS,  
      MAX(CASE WHEN P7_TariffType = 'WHS' THEN P7_TariffLevel END) AS WHS,  
      MAX(CASE WHEN P7_TariffType = 'TRN' THEN P7_TariffLevel END) AS TRN  
     FROM dbo.OrgRateTariffLevel  
     WHERE  
     P7_GC = @JH_GC AND P7_TariffLevel != 0  
     Group by P7_OH  
    ) TariffLevel ON (CASE JE_Direction WHEN 'IMP' THEN ImporterPK ELSE SupplierPK END) = TariffLevel.OrgPK )  TariffLink ON TariffLink.Link = JE_PK  
    WHERE JH_GC = @JH_GC  
    AND (OrgTariffLvl = @TariffLvl OR @TariffLvl  = '')  
	AND (@JS_PackingMode = '' OR JS_PackingMode = @JS_PackingMode)   
	AND (@JS_RL_NKOrigin = '' OR JS_RL_NKOrigin = @JS_RL_NKOrigin)
	AND (@JS_RL_NKDestination = '' OR JS_RL_NKDestination = @JS_RL_NKDestination)
	AND (@JS_E_DEP_From = '' OR JS_E_DEP >= @JS_E_DEP_From)
	AND (@JS_E_DEP_To = '' OR JS_E_DEP <= @JS_E_DEP_To)
	AND (@JS_E_ARV_From = ' ' OR JS_E_ARV >= @JS_E_ARV_From)
	AND (@JS_E_ARV_To = '' OR JS_E_ARV <= @JS_E_ARV_To)
	AND (@JS_SystemCreateTime_From = '' OR JS_SystemCreateTimeUtc >= @JS_SystemCreateTime_From)
	AND (@JS_SystemCreateTime_To = '' OR JS_SystemCreateTimeUtc <= @JS_SystemCreateTime_To)
	AND (@JS_UniqueConsignRef = '' OR JS_UniqueConsignRef = @JS_UniqueConsignRef)
	AND JE_JS IS NULL
ORDER BY JH_JobNum, JH_Status, JH_BranchCode, JH_DepartmentCode, JH_OperatorInitials, JH_SalesRepInitials
RETURN

  END
";
		#endregion

		#endregion

		#endregion

		#region ClientTypeDeciders

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (fClientTypeDeciders == null)
				{
					Dictionary<Type, ITypeDecider> dictionary = new Dictionary<Type, ITypeDecider>();
					dictionary.Add(typeof(ForwardingShipment), new TypeDeciderImpl(typeof(MFIForwardingShipment)));
					fClientTypeDeciders = new TypeDeciderDictionary(dictionary);
				}
				return fClientTypeDeciders;
			}
		}

		TypeDeciderDictionary fClientTypeDeciders;

		#endregion

		#endregion

		public class ConsignmentNoteData : AssemblyData
		{
			public override Type BusinessObjectType { get { return typeof(ForwardingShipment); } }
			protected override Type CollectionType
			{
				get { return typeof(ForwardingShipmentCollection); }
			}
			public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
			{
				return new ForwardingShipmentCollection(factory);
			}
			public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
			public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("db16c464-cdda-4ee2-8506-16ec2c6f0edb", "Consignment Note"); } }
		}
	}
}
