using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Client.Wow;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.MasterFiles.Business;
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
			get { return fInstance ?? (fInstance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride fInstance;
		#endregion

		#region IClientHook Members
		protected override void InitialiseCore()
		{
			WoolworthsOrderLine.RegisterThisSubTypeOverride();
			WoolworthsOrderLineDelivery.RegisterThisSubTypeOverride();
			WowDocDeclaration.RegisterThisSubTypeOverride();
			WowDocJobComInvoiceLine.RegisterThisSubTypeOverride();
			WowMessageSendingValidation.RegisterThisSubTypeOverride();
			WowDocCusContainer.RegisterThisSubTypeOverride();
			WowDeliveryInstructionsHelper.RegisterThisSubTypeOverride();
			WowDocOrder.RegisterThisSubTypeOverride();
		}

		public override Clients Client
		{
			get { return Clients.WOW; }
		}

		public override string ClientDisplayName
		{
			get { return "Woolworths"; }
		}

		public override IRegistryItemSet AdditionalRegistryItemSet
		{
			get { return WowDataRegistry.Instance; }
		}

		#region ControllerOverrides

		protected override ControllerOverrides GetControllerOverrides()
		{
			var controllerOverrides = new ControllerOverrides();
			ClientOverrideControllerID ordersID = new ClientOverrideControllerID(ControllerIDs.Orders);
			ClientOverrideControllerInfo ordersControllerInfo = new ClientOverrideControllerInfo(
				ordersID, typeof(WoolworthsOrdersControllerOverride).Assembly.FullName,
				typeof(WoolworthsOrdersControllerOverride).FullName);
			controllerOverrides.AddControllerOverride(ordersControllerInfo);

			ClientOverrideControllerID declarationID = new ClientOverrideControllerID(ControllerIDs.Customs.JobDeclaration);
			ClientOverrideControllerInfo declarationControllerInfo = new ClientOverrideControllerInfo(
				declarationID, typeof(WoolworthsJobDeclarationControllerOverride).Assembly.FullName,
				typeof(WoolworthsJobDeclarationControllerOverride).FullName,
				"AU");
			controllerOverrides.AddControllerOverride(declarationControllerInfo);

			ClientOverrideControllerID orderLineID = new ClientOverrideControllerID(ControllerIDs.OrderLine);
			ClientOverrideControllerInfo orderLineControllerInfo = new ClientOverrideControllerInfo(
				orderLineID, typeof(WoolworthsOrderLineControllerOverride).Assembly.FullName,
				typeof(WoolworthsOrderLineControllerOverride).FullName);
			controllerOverrides.AddControllerOverride(orderLineControllerInfo);

			ClientOverrideControllerID orderLineFromOrderID = new ClientOverrideControllerID(ControllerIDs.OrderLineFromOrder);
			ClientOverrideControllerInfo orderLineFromOrderControllerInfo = new ClientOverrideControllerInfo(
				orderLineFromOrderID, typeof(WoolworthsOrderLineFromOrderControllerOverride).Assembly.FullName,
				typeof(WoolworthsOrderLineFromOrderControllerOverride).FullName);
			controllerOverrides.AddControllerOverride(orderLineFromOrderControllerInfo);

			ClientOverrideControllerID supplierPartID = new ClientOverrideControllerID(ControllerIDs.Customs.SupplierPart);
			ClientOverrideControllerInfo orgSupplierPartControllerInfo = new ClientOverrideControllerInfo(
				supplierPartID, typeof(WoolworthsOrgSupplierPartControllerOverride).Assembly.FullName,
				typeof(WoolworthsOrgSupplierPartControllerOverride).FullName,
				"AU");
			controllerOverrides.AddControllerOverride(orgSupplierPartControllerInfo);
			return controllerOverrides;
		}

		#endregion

		#region ModuleOverrides

		protected override ModuleOverrides GetModuleOverrides()
		{
			var moduleOverrides = new ModuleOverrides();

			ClientOverrideModuleIdentifier ordersID = new ClientOverrideModuleIdentifier(ModuleIDs.Orders);
			ClientOverrideModuleInfo ordersModuleInfo = new ClientOverrideModuleInfo(
				ordersID, typeof(WoolworthsOrdersModuleOverride).Assembly.FullName,
				typeof(WoolworthsOrdersModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(ordersModuleInfo);

			ClientOverrideModuleIdentifier ordersReportID = new ClientOverrideModuleIdentifier(ModuleIDs.OrdersReport);
			ClientOverrideModuleInfo ordersReportModuleInfo = new ClientOverrideModuleInfo(
				ordersReportID, typeof(WoolworthsOrdersReportModuleOverride).Assembly.FullName,
				typeof(WoolworthsOrdersReportModuleOverride).FullName);
			moduleOverrides.AddModuleOverride(ordersReportModuleInfo);
			return moduleOverrides;
		}

		#endregion

		#region ClientTypeDeciders

		public override ITypeDeciderDictionary ClientTypeDeciders
		{
			get
			{
				if (fClientTypeDeciders == null)
				{
					Dictionary<Type, ITypeDecider> result = new Dictionary<Type, ITypeDecider>();
					result.Add(typeof(AUOrgSupplierPart), new TypeDeciderImpl(typeof(WoolworthsProduct)));
					result.Add(typeof(Order), new TypeDeciderImpl(typeof(WoolworthsOrder)));
					result.Add(typeof(OrderLine), new TypeDeciderImpl(typeof(WoolworthsOrderLine)));
					result.Add(typeof(OrderLineDelivery), new TypeDeciderImpl(typeof(WoolworthsOrderLineDelivery)));
					result.Add(typeof(OrderLineDeliverContainer), new TypeDeciderImpl(typeof(WoolworthsOrderLineDeliverContainer)));
					result.Add(typeof(OrdersFilterBusinessObject), new TypeDeciderImpl(typeof(WoolworthsOrdersFilterBusinessObject)));
					result.Add(typeof(OrgAddress), new TypeDeciderImpl(typeof(WoolworthsOrgAddress)));
					result.Add(typeof(Accounting.Business.JobInvoicing.ExchangeRate), new TypeDeciderImpl(typeof(WoolworthsJobExchangeRate)));
					result.Add(typeof(JobDeclaration), new TypeDeciderImpl(typeof(WoolworthsJobDeclaration)));
					result.Add(typeof(Freight.Forwarding.Business.ForwardingDocsAndCartage), new TypeDeciderImpl(typeof(WoolworthsJobDocsAndCartage)));
					result.Add(typeof(JobComInvoiceLine), new TypeDeciderImpl(typeof(WoolworthsJobComInvoiceLine)));
					result.Add(typeof(JobComInvoiceHeader), new TypeDeciderImpl(typeof(WoolworthsJobComInvoiceHeader)));
					result.Add(typeof(CusContainer), new TypeDeciderImpl(typeof(WoolworthsCusContainer)));
					fClientTypeDeciders = new TypeDeciderDictionary(result);
				}
				return fClientTypeDeciders;
			}
		}
		ITypeDeciderDictionary fClientTypeDeciders;

		#endregion

		public override IExtensionObjects DbSchemaExtensionObjects { get; } = new ExtensionObjects(TableCreationScripts, ViewAndRoutinesCreationScripts);

		#endregion

		#region Reports SQL scripts

		static readonly ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts = ImmutableArray.Create(
			Index_JobComInvoiceLine_JI_OrderHeader_JI_PartNo
		);

		#region Client Specific Indexes

		static DatabaseObjectCreateScript Index_JobComInvoiceLine_JI_OrderHeader_JI_PartNo
		{
			get
			{
				return new DatabaseObjectCreateScript(
					"NR_RX__JI_OrderNumber_JI_PartNo",
					"CREATE NONCLUSTERED INDEX NR_RX__JI_OrderNumber_JI_PartNo ON JobComInvoiceLine (JI_OrderNumber, JI_PartNo)",
					"DROP INDEX JobComInvoiceLine.NR_RX__JI_OrderNumber_JI_PartNo");
			}
		}

		#endregion

		static readonly ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts = ImmutableArray.Create(
			new DatabaseViewAndRoutineCreateScript("ClientGetAllClientVisibleNotes", Create_ClientGetAllClientVisibleNotes, Drop_ClientGetAllClientVisibleNotes, DbRoutineType.SqlFunctionScalarTypeDesc),
			new DatabaseViewAndRoutineCreateScript("vw_Report_ClientContainerVolume", Create_vw_Report_ClientContainerVolume, Drop_vw_Report_ClientContainerVolume, DbRoutineType.SqlViewTypeDesc),
			new DatabaseViewAndRoutineCreateScript("Client_WOW_DeclarationFreightPaymentReport", Create_Client_WOW_DeclarationFreightPaymentReport, Drop_Client_WOW_DeclarationFreightPaymentReport, DbRoutineType.SqlFunctionInlineTypeDesc),
			new DatabaseViewAndRoutineCreateScript("vw_Report_ClientInvoicePaymentNotify", Create_vw_Report_ClientInvoicePaymentNotify, Drop_vw_Report_ClientInvoicePaymentNotify, DbRoutineType.SqlViewTypeDesc),
			new DatabaseViewAndRoutineCreateScript("vw_Report_ClientOrderBuyersTripReport", Create_vw_Report_ClientOrderBuyersTripReport, Drop_vw_Report_ClientOrderBuyersTripReport, DbRoutineType.SqlViewTypeDesc),
			new DatabaseViewAndRoutineCreateScript("vw_Report_ClientOrderInvoiceDiscrepancy", Create_vw_Report_ClientOrderInvoiceDiscrepancy, Drop_vw_Report_ClientOrderInvoiceDiscrepancy, DbRoutineType.SqlViewTypeDesc),
			new DatabaseViewAndRoutineCreateScript("vw_Report_ClientOrdersSignOff", Create_vw_Report_ClientOrdersSignOff, Drop_vw_Report_ClientOrdersSignOff, DbRoutineType.SqlViewTypeDesc),
			new DatabaseViewAndRoutineCreateScript("Client_WOW_OutstandingOrdersReport", Create_Client_WOW_OutstandingOrdersReport, Drop_Client_WOW_OutstandingOrdersReport, DbRoutineType.SqlFunctionInlineTypeDesc),
			new DatabaseViewAndRoutineCreateScript("Client_WOW_Report_ClientOverseasConsolidatorOrderReport", Create_WOW_Report_ClientOverseasConsolidatorOrderReport, Drop_WOW_Report_ClientOverseasConsolidatorOrderReport, DbRoutineType.SqlFunctionTableTypeDesc),
			new DatabaseViewAndRoutineCreateScript("vw_Report_ClientOrderContainersInYard", Create_vw_Report_ClientOrderContainersInYard, Drop_vw_Report_ClientOrderContainersInYard, DbRoutineType.SqlViewTypeDesc),
			new DatabaseViewAndRoutineCreateScript("vw_Report_ClientContainerTracking", Create_vw_Report_ClientContainerTracking, Drop_vw_Report_ClientContainerTracking, DbRoutineType.SqlViewTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientGetLatestStmaLog", Create_ClientGetLatestStmaLog, Drop_ClientGetLatestStmaLog, DbRoutineType.SqlFunctionScalarTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientContainerTracking", Create_ClientContainerTracking, Drop_ClientContainerTracking, DbRoutineType.SqlFunctionInlineTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientCheckDeclarationJobService", Create_ClientCheckDeclarationJobService, Drop_ClientCheckDeclarationJobService, DbRoutineType.SqlFunctionScalarTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientGetEntryNumberList", Create_ClientGetEntryNumberList, Drop_ClientGetEntryNumberList, DbRoutineType.SqlFunctionScalarTypeDesc),
			new DatabaseViewAndRoutineCreateScript("ClientReportOrderCurrencyRequirement", Create_ClientReportOrderCurrencyRequirement, Drop_ClientReportOrderCurrencyRequirement, DbRoutineType.SqlFunctionInlineTypeDesc),
			new DatabaseViewAndRoutineCreateScript("Client_WOW_ContainerPriorityReport", Create_Client_WOW_ContainerPriorityReport, Drop_Client_WOW_ContainerPriorityReport, DbRoutineType.SqlFunctionInlineTypeDesc),
			new DatabaseViewAndRoutineCreateScript("Client_WOW_ContainerVolumeReport", Create_Client_WOW_ContainerVolumeReport, Drop_Client_WOW_ContainerVolumeReport, DbRoutineType.SqlFunctionInlineTypeDesc),
			new DatabaseViewAndRoutineCreateScript("Client_WOW_ProductOrderLinesReport", Create_Client_WOW_ProductOrderLinesReport, Drop_Client_WOW_ProductOrderLinesReport, DbRoutineType.SqlViewTypeDesc),
			new DatabaseViewAndRoutineCreateScript("Client_WOW_OrderContainerInYardReport", Create_Client_WOW_OrderContainerInYardReport, Drop_Client_WOW_OrderContainerInYardReport, DbRoutineType.SqlFunctionInlineTypeDesc)
		);

		#region Client_WOW_OrderContainerInYardReport

		const string Create_Client_WOW_OrderContainerInYardReport = @"

CREATE FUNCTION Client_WOW_OrderContainerInYardReport
(
   @ConsigneeList varchar(5000),
   @SupplierList varchar(5000),
   @OrderNumber varchar(35),
   @DeliverdFrom dateTime,
   @DeliveredTo dateTime,
   @NotYetDeliverOnly char(1)
)
RETURNS TABLE
AS

RETURN 
SELECT 
	Declaration.JE_DeclarationReference As DeclarationReference, 
	JE_GB as BranchPK, 
	min(CusContainer.CO_ContainerNumber) As ContainerNum,   
	min(ContainerType.RC_Code) As ContainerType,  
	max(Declaration.JE_DateAtOrigin) as ETD,
	max(Declaration.JE_DateAtFinalDestination) as ETA,
	isnull(JE_VesselName, '<empty>') As ArrivalVessel,  
	isnull(JE_VoyageFlightNo, '<empty>') As ArrivalVoyage,  
	min(DeclInvoiceLine.JI_PartNo) As Partno,   
	min(JI_Description) As Description,  
	min(Buyer.OH_Code) As BuyerCode,  
	min(Buyer.OH_FullName) As BuyerFullName, 
	min(Supplier.OH_Code) as SupplierCode, 
	OrderDelivery.J4_OA_NKDeliveryPoint As DeliveryAddress,  
	max(JobContainer.JC_FCLWharfGateOut) as ContainerInYardDate, 
	max(JC_ArrivalCartageComplete) as deliveredToWarehouse,
	sum(J5_PackCount) As TotalPacksInCartons,  
	min(Product.OP_VendorPackQty) As VendorPackQty,  
	max(OrderDelivery.J4_CustomAttribute1) As POMNum,  
	DeclInvoiceLine.JI_OrderNumber As OrderNumber ,
	max(JE_RL_NKPortOfLoading) as Loading,
	sum(J5_QuantityInStore) as QtyDelivered,
	max(JD_OrderDate) as OrderDate,
	max(JD_OrderStatus) as OrderStatus
FROM dbo.JobComInvoiceLine DeclInvoiceLine 
INNER JOIN dbo.JobComInvoiceHeader DeclInvoice ON JZ_PK = JI_JZ AND JZ_GroupInvoice = 0
INNER JOIN dbo.JobDeclaration Declaration ON JE_PK = JZ_JE
INNER JOIN dbo.CusContainer CusContainer ON CusContainer.CO_JE = Declaration.JE_PK and Declaration.JE_IsCancelled = 0 AND JI_CustomAttrib4 = CusContainer.CO_ContainerNumber
INNER JOIN dbo.GlbBranch on GB_PK = JE_GB
LEFT JOIN dbo.RefContainer ContainerType ON CusContainer.CO_RC = ContainerType.RC_PK
LEFT JOIN dbo.JobContainer ON JobContainer.JC_PK = CusContainer.CO_JC
LEFT JOIN dbo.OrgSupplierPart Product ON DeclInvoiceLine.JI_OP = Product.OP_PK  
LEFT JOIN dbo.JobOrderHeader OrderHeader ON JD_OrderNumber = JI_OrderNumber AND JD_OrderStatus != 'CAN'
LEFT JOIN dbo.OrgAddress BuyerAddress ON OrderHeader.JD_OA_BuyerAddress = BuyerAddress.OA_PK
LEFT JOIN dbo.OrgHeader Buyer ON BuyerAddress.OA_OH = Buyer.OH_PK  
LEFT JOIN dbo.OrgAddress SupplierAddress ON OrderHeader.JD_OA_SupplierAddress = SupplierAddress.OA_PK
LEFT JOIN dbo.OrgHeader Supplier ON SupplierAddress.OA_OH = Supplier.OH_PK  
LEFT JOIN dbo.JobOrderLine OrderLine ON OrderLine.JO_JD = OrderHeader.JD_PK AND DeclInvoiceLine.JI_PartNo = OrderLine.JO_Partno AND OrderLine.JO_LineStatus != 'CAN' AND JI_CustomDecimal1 = JO_LineNo
LEFT JOIN dbo.JobOrderLineDelivery OrderDelivery ON OrderLine.JO_PK = OrderDelivery.J4_JO AND Declaration.JE_RL_NKFinalDestination = OrderDelivery.J4_RL_NKDestinationPort  
LEFT JOIN dbo.JobOrderLineDeliverContainer OrderContainer ON OrderDelivery.J4_PK = OrderContainer.J5_J4 AND CusContainer.CO_ContainerNumber=OrderContainer.J5_ContainerNum  
WHERE (JE_DateOfArrival is null or JE_DateOfArrival > dateadd(month, -8, getdate()))  
AND (rtrim(Buyer.OH_Code) IN (SELECT value from SplitStringToTable(@ConsigneeList, DEFAULT)) OR @ConsigneeList = '' OR @ConsigneeList IS NULL)  
AND (rtrim(Supplier.OH_Code) IN (SELECT value from SplitStringToTable(@SupplierList, DEFAULT)) OR @SupplierList = '' OR @SupplierList IS NULL)  
AND (@OrderNumber IS NULL OR @OrderNumber = '' OR @OrderNumber = JI_OrderNumber)
AND (@DeliverdFrom IS NULL OR @DeliverdFrom = '' OR JC_ArrivalCartageComplete >= @DeliverdFrom)
AND (@DeliveredTo IS NULL OR @DeliveredTo = '' OR JC_ArrivalCartageComplete <= @DeliveredTo)
AND ( (JC_ArrivalCartageComplete IS NULL AND @NotYetDeliverOnly = 'Y') OR  @NotYetDeliverOnly <> 'Y')	
AND JE_IsCancelled <> 1		
GROUP BY 
JE_GB,
Declaration.JE_DeclarationReference,
DeclInvoiceLine.JI_PartNo, 
CO_PK, 
CO_ContainerNumber, 
JE_VesselName, 
JE_VoyageFlightNo, 
OrderDelivery.J4_OA_NKDeliveryPoint, 
JI_OrderNumber

";

		const string Drop_Client_WOW_OrderContainerInYardReport = @"
DROP FUNCTION Client_WOW_OrderContainerInYardReport
";
		#endregion

		#region  Client_WOW_ProductOrderLinesReport

		const string Create_Client_WOW_ProductOrderLinesReport = @"
CREATE VIEW Client_WOW_ProductOrderLinesReport
AS
SELECT JD_OrderNumber As OrderNumber,
	JD_OrderDate AS OrderDate,
	OP_PartNum As Partno,
	OP_Division As Division,
	OP_Department As Department,
	JO_Description As Description,
	JO_CustomDate2 As ArrivalDate,
	Currency.RX_Code As Currency,
	JO_InnerPacks As InnerPacks,
	JO_OuterPacks As OuterPacks,
	JO_PK As OrderLinePK,
	JO_CustomDecimal1 As EstimatedLandedCost,
	JO_ItemPrice As ItemPrice,
	Buyer.OH_FullName As Buyer,
	DeliverPoint.OA_Code As WarehouseNum,
	(case
	when isnull(Line.JO_InnerPacks, Product.OP_VendorPackQty) = 0 then 0
	else Line.JO_Quantity / isnull(Line.JO_InnerPacks, Product.OP_VendorPackQty)
	end) As TotalPacksInCartons

FROM dbo.JobOrderHeader Header
LEFT JOIN dbo.OrgAddress BuyerAddress ON Header.JD_OA_BuyerAddress = BuyerAddress.OA_PK
LEFT JOIN dbo.OrgAddress SupplierAddress ON Header.JD_OA_SupplierAddress = SupplierAddress.OA_PK
LEFT JOIN dbo.JobOrderLine Line ON Header.JD_PK = Line.JO_JD
LEFT JOIN dbo.JobOrderLineDelivery Delivery ON Line.JO_PK = Delivery.J4_JO
LEFT JOIN dbo.OrgSupplierPart Product ON Line.JO_Partno = Product.OP_PartNum  AND Product.OP_PK =
(
	  SELECT TOP 1 OP_PK
	  FROM dbo.OrgSupplierPart InnerPart
	  WHERE 	 
		(
			(OP_PK IN (SELECT OU_OP FROM dbo.OrgPartRelation WHERE OU_OH = BuyerAddress.OA_OH and OU_Relationship in ('OWN', 'BTH'))) 
			or 
			(OP_PK IN (SELECT OU_OP FROM dbo.OrgPartRelation WHERE OU_OH = SupplierAddress.OA_OH and OU_Relationship in ('SUP', 'BTH')))
		) and OP_IsActive = 1
	 AND InnerPart.OP_PartNum = Product.OP_PartNum
)
LEFT JOIN dbo.OrgHeader Buyer ON BuyerAddress.OA_OH = Buyer.OH_PK
LEFT JOIN dbo.OrgAddress DeliverPoint ON Delivery.J4_OA_NKDeliveryPoint = DeliverPoint.OA_Code AND Buyer.OH_PK = DeliverPoint.OA_OH
LEFT JOIN dbo.RefCurrency Currency ON Header.JD_RX_NKOrderCurrency = Currency.RX_Code
WHERE JO_CustomFlag4 = 1 
AND JD_OrderStatus != 'CAN'


";

		const string Drop_Client_WOW_ProductOrderLinesReport = @"
DROP VIEW Client_WOW_ProductOrderLinesReport
";
		#endregion

		#region Client_WOW_ContainerVolumeReport

		const string Drop_Client_WOW_ContainerVolumeReport = "Drop Function Client_WOW_ContainerVolumeReport";
		const string Create_Client_WOW_ContainerVolumeReport = @"
CREATE FUNCTION Client_WOW_ContainerVolumeReport  
(  
 @Year int,  
 @ExportDateFrom smalldatetime,  
 @ExportDateTo smalldatetime,  
 @PortOfLoading varchar(5),  
 @ShippingLine uniqueIdentifier,  
 @SendingAgent uniqueIdentifier  
)  
RETURNS TABLE AS  
RETURN  
  
SELECT     isnull(LoadingLocoZone.ZoneDescription, '<empty>') As Region,  
           min(JE_ExportDate) As MinExportDate,  
           max(JE_ExportDate) As MaxExportDate,  
           isnull(Buyer.OH_FullName, '<empty>') As Buyer,  
           isnull(JE_RL_NKPortOfLoading, '<empty>') + ';' + isnull(PortOfLoading.RL_PortName, '') As PortOfLoadingName,  
           Buyer.OH_FullName + '__' + JE_RL_NKPortOfLoading As BuyerAndPortOfLoading,  
           ShippingLine.OH_FullName + '__' + JE_RL_NKPortOfLoading As ShippingLineAndPortOfLoading,  
           isnull(ShippingLine.OH_FullName, '<empty>') As ShippingLine,  
           JE_OH_ShippingLine As ShippingLinePK,  
           isnull(SendingAgent.OH_FullName, '<empty>') As SendingAgent,  
           JE_OH_Forwarder As SendingAgentPK,  
           isnull(ContainerType.RC_Code, '<empty>') As ContainerType,  
           ContainerType.RC_TEU As ContainerTEU,  
           isnull(JE_RL_NKFinalDestination, '<empty>') + ';' + isnull(DestinationPort.RL_PortName, '') As DestinationPortName,  
           CO_CustomFlag1 As ConsolidatorOrFactoryPack,  
           sum(case when datepart(month,JE_DateOfArrival) = 1 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As JanTEU,  
           sum(case when datepart(month,JE_DateOfArrival) = 2 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As FebTEU,  
           sum(case when datepart(month,JE_DateOfArrival) = 3 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As MarTEU,  
           sum(case when datepart(month,JE_DateOfArrival) = 4 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As AprTEU,  
           sum(case when datepart(month,JE_DateOfArrival) = 5 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As MayTEU,  
           sum(case when datepart(month,JE_DateOfArrival) = 6 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As JunTEU,  
           sum(case when datepart(month,JE_DateOfArrival) = 7 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As JulTEU,  
           sum(case when datepart(month,JE_DateOfArrival) = 8 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As AugTEU,  
           sum(case when datepart(month,JE_DateOfArrival) = 9 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As SepTEU,  
           sum(case when datepart(month,JE_DateOfArrival) = 10 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As OctTEU,  
           sum(case when datepart(month,JE_DateOfArrival) = 11 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As NovTEU,  
           sum(case when datepart(month,JE_DateOfArrival) = 12 and datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As DecTEU,  
           sum(case when                                           datepart(year,JE_DateOfArrival) = (cast(@Year AS int) % 2000 + 2000) then ContainerType.RC_TEU else 0 end) As YearTotalTEU  
  
FROM       dbo.JobComInvoiceLine 
		   INNER JOIN dbo.JobComInvoiceHeader ON JZ_PK = JI_JZ
		   INNER JOIN dbo.JobDeclaration Declaration ON JE_PK = JZ_JE
           INNER JOIN dbo.CusContainer Container ON Declaration.JE_PK = Container.CO_JE  
           LEFT  JOIN dbo.JobDocAddress DepotDocAddress on Declaration.JE_PK = DepotDocAddress.E2_ParentID and DepotDocAddress.E2_AddressType = 'CDE'  
           LEFT  JOIN dbo.OrgAddress DepotAddress ON DepotDocAddress.E2_OA_Address = DepotAddress.OA_PK  
           LEFT  JOIN dbo.JobOrderHeader ON JD_OrderNumber = JI_OrderNumber		
           LEFT  JOIN dbo.OrgAddress BuyerAddress ON BuyerAddress.OA_PK = JobOrderHeader.JD_OA_BuyerAddress	
           LEFT  JOIN dbo.OrgHeader Buyer ON Buyer.OH_PK = BuyerAddress.OA_OH
           LEFT  JOIN dbo.OrgHeader Supplier ON Supplier.OH_PK = Declaration.JE_OH_Supplier  
           LEFT  JOIN dbo.OrgHeader ShippingLine ON ShippingLine.OH_PK = Declaration.JE_OH_ShippingLine  
           LEFT  JOIN dbo.OrgHeader SendingAgent ON SendingAgent.OH_PK = Declaration.JE_OH_Forwarder  
           LEFT  JOIN dbo.RefUNLOCO PortOfLoading ON Declaration.JE_RL_NKPortOfLoading = PortOfLoading.RL_Code  
           LEFT  JOIN dbo.RefUNLOCO DestinationPort ON Declaration.JE_RL_NKFinalDestination = DestinationPort.RL_Code  
           LEFT  JOIN csfn_LocoReportingZones(NULL) LoadingLocoZone ON LoadingLocoZone.LocoPK = PortOfLoading.RL_PK  
           LEFT  JOIN dbo.RefContainer ContainerType ON Container.CO_RC = ContainerType.RC_PK  
  
WHERE      (@ExportDateFrom is null OR @ExportDateFrom = '' OR JE_ExportDate >= @ExportDateFrom) AND  
           (@ExportDateTo is null OR @ExportDateTo = '' OR JE_ExportDate <= @ExportDateTo) AND  
           (@PortOfLoading is null OR @PortOfLoading = '' OR JE_RL_NKPortOfLoading like '%' + @PortOfLoading + '%') AND  
           (@ShippingLine is null OR JE_OH_ShippingLine = @ShippingLine) AND  
           (@SendingAgent is null OR JE_OH_Forwarder = @SendingAgent)  
  
GROUP BY   LoadingLocoZone.ZoneDescription,  
    Buyer.OH_FullName,   
    ShippingLine.OH_FullName,   
    Declaration.JE_OH_ShippingLine,   
    SendingAgent.OH_FullName,   
    Declaration.JE_OH_Forwarder,   
    Declaration.JE_RL_NKPortOfLoading,   
    PortOfLoading.RL_PortName,  
    JE_OH_ShippingLine,  
    ContainerType.RC_Code,   
    ContainerType.RC_TEU,   
    JE_RL_NKFinalDestination,  
    DestinationPort.RL_PortName,  
    CO_CustomFlag1  
  
";

		#endregion

		#region Create_Client_WOW_ContainerPriorityReport

		const string Create_Client_WOW_ContainerPriorityReport = @"CREATE 

   FUNCTION Client_WOW_ContainerPriorityReport
(
  @Department varchar(100),
  @PartNo varchar(1000),
  @DeliveryAddress varchar(300)
)

RETURNS TABLE
AS
RETURN


SELECT 
	JI_PartNo As Partno,
	JI_OrderNumber as OrderNumber,
	OrderDate,
	JI_InvoiceQuantity as LineInvoiceQuantity,
	CO_ContainerNumber As ContainerNum,
	JE_DateOfArrival as ETA,
	OP_Desc As ItemDescription,
	OP_Department As Department,
	OP_Division As Division,
	JE_VesselName as Vessel,
	DeclDeliveryAddress.OA_Code as WarehouseNum,
	JC_ArrivalCartageComplete as ContainerDeliveredDate,
	JI_PartNo + '__' + OP_Department + '__' + OP_Division As GroupByString,
	AdvertOrNewLineDate, 
	POMNum

from dbo.JobComInvoiceLine
inner join dbo.JobComInvoiceHeader on JI_JZ = JZ_PK and JZ_GroupInvoice = 0
inner join dbo.CusContainer on JI_CustomAttrib4 = CO_ContainerNumber and JZ_JE = CO_JE
inner join dbo.JobDeclaration on JZ_JE = JE_PK
left join dbo.JobContainer on JC_PK = CO_JC 
left join dbo.OrgSupplierPart on JI_OP = OP_PK
left join dbo.JobDocAddress DocAddress ON JE_PK = DocAddress.E2_ParentID AND DocAddress.E2_AddressType = 'IMG'
left join dbo.OrgAddress DeclDeliveryAddress ON DocAddress.E2_OA_Address = DeclDeliveryAddress.OA_PK
left join
(
	SELECT
		JI_PK as InvoiceLinePK,
		Max(J4_CustomAttribute1) as POMNum,
		Max(JD_OrderDate) as OrderDate,
		ISNULL(min(JO_CustomDate4), min(SL_EventTime)) as  AdvertOrNewLineDate
	FROM dbo.JobComInvoiceLine
	inner join dbo.JobOrderHeader ON JI_OrderNumber = JD_OrderNumber AND JD_OrderStatus != 'CAN'
	INNER JOIN dbo.JobOrderLine ON JD_PK=JO_JD AND JO_LineStatus != 'CAN' AND JO_Partno=JI_PartNo AND JI_CustomDecimal1 = JO_LineNo
	INNER JOIN dbo.JobOrderLineDelivery ON J4_JO=JO_PK
	INNER JOIN dbo.StmALog ON JO_PK=SL_Parent
	WHERE SL_SE_NKEvent = 'ADD' AND SL_IsCancelled = 'N'
	GROUP BY JI_PK
) AdvertEvent on AdvertEvent. InvoiceLinePK = JI_PK
WHERE (JE_IsCancelled is null or JE_IsCancelled != 1) 
AND (@Department = '' or OP_Department = @Department)
AND (@PartNo = '' or charindex(JI_PartNo, @PartNo) > 0)
AND (@DeliveryAddress = '' or DeclDeliveryAddress.OA_Code = @DeliveryAddress)
";

		#endregion

		#region

		const string Drop_Client_WOW_ContainerPriorityReport = "Drop Function Client_WOW_ContainerPriorityReport";

		#endregion

		#region ClientGetAllClientVisibleNotes

		public const string Drop_ClientGetAllClientVisibleNotes = "DROP FUNCTION ClientGetAllClientVisibleNotes";
		public const string Create_ClientGetAllClientVisibleNotes = @"
			CREATE FUNCTION ClientGetAllClientVisibleNotes(@PK uniqueidentifier)
			RETURNS varchar(8000)
			AS
			BEGIN
				DECLARE @RetVal varchar(8000); SET @RetVal = ''

				DECLARE @ST_NoteText varchar(8000)
				DECLARE Cur CURSOR FAST_FORWARD READ_ONLY FOR
					select convert(varchar(8000),ST_NoteText) from dbo.StmNote where ST_ParentID=@PK and ST_NoteType='PUB'
				OPEN Cur

				FETCH NEXT FROM Cur INTO @ST_NoteText
				WHILE @@FETCH_STATUS = 0
				BEGIN
					IF @RetVal <> ''
						SET @RetVal = @RetVal + CHAR(10) + CHAR(10)
					SET @RetVal = @RetVal + @ST_NoteText
					FETCH NEXT FROM Cur INTO @ST_NoteText
				END

				CLOSE Cur
				DEALLOCATE Cur

				RETURN @RetVal
			END";

		#endregion

		#region vw_Report_ClientContainerVolume

		public const string Drop_vw_Report_ClientContainerVolume = "DROP VIEW vw_Report_ClientContainerVolume";
		public const string Create_vw_Report_ClientContainerVolume = @"
			CREATE VIEW vw_Report_ClientContainerVolume
			AS
			SELECT isnull(LocoZone.ZoneDescription, '<empty>') As Region,
				min(JO_CustomDate1) As MinShipDate,
				max(JO_CustomDate1) As MaxShipDate,
				isnull(Buyer.OH_FullName, '<empty>') As Buyer,
				isnull(JD_RL_NKPortOfLoading, '<empty>') As PortOfLoading,
				Buyer.OH_FullName + '__' + JD_RL_NKPortOfLoading As BuyerAndPortOfLoading,
				isnull(Carrier.OH_FullName, '<empty>') As Carrier,
				JD_OH_Carrier As CarrierPK,
				isnull(SendingAgent.OH_FullName, '<empty>') As SendingAgent,
				JD_OH_SendingAgent As SendingAgentPK,
				isnull(J5_RC_NKContainerType, '<empty>') As ContainerType,
				ContainerType.RC_TEU As ContainerTEU,
				isnull(J4_RL_NKDestinationPort, '<empty>') As DestinationPort,
				JO_CustomFlag3 As ConsolidatorOrFactoryPack,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '01-01' AND cast(year(GetDate()) AS varchar) + '-' + '02-01' then ContainerType.RC_TEU else 0 end) As JanTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '02-02' AND cast(year(GetDate()) AS varchar) + '-' + '03-01' then ContainerType.RC_TEU else 0 end) As FebTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '03-03' AND cast(year(GetDate()) AS varchar) + '-' + '04-01' then ContainerType.RC_TEU else 0 end) As MarTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '04-04' AND cast(year(GetDate()) AS varchar) + '-' + '05-01' then ContainerType.RC_TEU else 0 end) As AprTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '05-05' AND cast(year(GetDate()) AS varchar) + '-' + '06-01' then ContainerType.RC_TEU else 0 end) As MayTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '06-06' AND cast(year(GetDate()) AS varchar) + '-' + '07-01' then ContainerType.RC_TEU else 0 end) As JunTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '07-07' AND cast(year(GetDate()) AS varchar) + '-' + '08-01' then ContainerType.RC_TEU else 0 end) As JulTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '08-08' AND cast(year(GetDate()) AS varchar) + '-' + '09-01' then ContainerType.RC_TEU else 0 end) As AugTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '09-09' AND cast(year(GetDate()) AS varchar) + '-' + '10-01' then ContainerType.RC_TEU else 0 end) As SepTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '10-10' AND cast(year(GetDate()) AS varchar) + '-' + '11-01' then ContainerType.RC_TEU else 0 end) As OctTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '11-11' AND cast(year(GetDate()) AS varchar) + '-' + '12-01' then ContainerType.RC_TEU else 0 end) As NovTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '12-12' AND cast(year(GetDate() + 1) AS varchar) + '-' + '01-01' then ContainerType.RC_TEU else 0 end) As DecTEU,
				sum(case when J5_ETA between cast(year(GetDate()) AS varchar) + '-' + '01-01' AND cast(year(GetDate() + 1) AS varchar) + '-' + '01-01' then ContainerType.RC_TEU else 0 end) As YearTotalTEU

			FROM dbo.JobOrderHeader Header
				INNER JOIN dbo.JobOrderLine Line ON Header.JD_PK = Line.JO_JD AND JO_LineStatus != 'CAN'
				INNER JOIN dbo.JobOrderLineDelivery Delivery ON Line.JO_PK = Delivery.J4_JO
				INNER JOIN dbo.JobOrderLineDeliverContainer Container ON Delivery.J4_PK = Container.J5_J4
				LEFT OUTER JOIN dbo.OrgAddress BuyerAddress ON BuyerAddress.OA_PK = Header.JD_OA_BuyerAddress
				LEFT OUTER JOIN dbo.OrgHeader Buyer ON Buyer.OH_PK = BuyerAddress.OA_OH
				LEFT OUTER JOIN dbo.OrgAddress DeliverPoint ON (DeliverPoint.OA_OH = Buyer.OH_PK AND Delivery.J4_OA_NKDeliveryPoint = DeliverPoint.OA_Code)
				LEFT OUTER JOIN dbo.OrgAddress SupplierAddress ON SupplierAddress.OA_PK = Header.JD_OA_SupplierAddress
				LEFT OUTER JOIN dbo.OrgHeader Supplier ON Supplier.OH_PK = SupplierAddress.OA_OH
				LEFT OUTER JOIN dbo.OrgHeader Carrier ON Carrier.OH_PK = Header.JD_OH_Carrier
				LEFT OUTER JOIN dbo.OrgHeader SendingAgent ON SendingAgent.OH_PK = Header.JD_OH_SendingAgent
				LEFT OUTER JOIN csfn_LocoReportingZones(NULL) LocoZone ON LocoZone.LocoCode = Header.JD_RL_NKPortOfLoading
				LEFT OUTER JOIN dbo.RefContainer ContainerType ON Container.J5_RC_NKContainerType = ContainerType.RC_Code

			WHERE JD_OrderStatus != 'CAN'
			GROUP BY LocoZone.ZoneDescription,
				Buyer.OH_FullName, 
				Carrier.OH_FullName, 
 				Header.JD_OH_Carrier, 
				SendingAgent.OH_FullName, 
				Header.JD_OH_SendingAgent, 
				Header.JD_RL_NKPortOfLoading, 
 				JD_OH_Carrier, 
				J5_RC_NKContainerType, 
				ContainerType.RC_TEU, 
				J4_RL_NKDestinationPort,
				JO_CustomFlag3";

		#endregion

		#region Client_WOW_DeclarationFreightPaymentReport

		public const string Drop_Client_WOW_DeclarationFreightPaymentReport = "DROP FUNCTION Client_WOW_DeclarationFreightPaymentReport";
		public const string Create_Client_WOW_DeclarationFreightPaymentReport = @"
		CREATE FUNCTION Client_WOW_DeclarationFreightPaymentReport  
(  
 @ExportStartDate as smalldatetime,  
 @ExportEndDate as smalldatetime,  
 @ShippingLinePK as uniqueidentifier,  
 @Vessel as varchar(100),  
 @Voyage as varchar(100)  
)  
RETURNS TABLE  
  
AS  
  
RETURN  
   
 SELECT     
    Decl.JE_DeclarationReference As DeclarationReference,  
    (ROW_NUMBER() over (partition by Decl.JE_DeclarationReference order by CO_ContainerNumber, Decl.JE_DeclarationReference)) as RowNum,  
    JE_MasterBill As MasterBill,  
    OrderedBy.OH_Code As OrderedByCode,  
    CO_ContainerNumber As ContainerNum,  
    JE_VesselName As Vessel,  
    JE_VoyageFlightNo As Voyage,  
    JE_RL_NKPortOfLoading As PortOfLoading,  
    JE_RL_NKPortOfArrival As PortOfDischarge,  
    ContainerType.RC_Code As ContainerType,  
    JE_ExportDate As ATD,  
    FreightCharge.JR_OSCostAmt As FreightCharges,  
    FreightChargeCurrency.RX_Code As FreightChargeCurrency,  
    PSCCharge.JR_OSCostAmt As PSCCharges,  
    JE_ExportDate As ExportDate,  
    JE_OH_ShippingLine As ShippingLinePK  
  
   FROM dbo.CusContainer Container  
    LEFT OUTER JOIN dbo.RefContainer ContainerType ON ContainerType.RC_PK = Container.CO_RC  
    LEFT OUTER JOIN dbo.JobDeclaration Decl ON Container.CO_JE = Decl.JE_PK  
    LEFT OUTER JOIN dbo.JobComInvoiceLine ON Container.CO_ContainerNumber = JI_CustomAttrib4  
                AND JobComInvoiceLine.JI_PK =  
                (  
                 SELECT top 1 JI_PK   
                 FROM dbo.JobComInvoiceLine InnerJobCom  
                 INNER JOIN dbo.JobComInvoiceHeader ON JZ_PK = JI_JZ and JZ_GroupInvoice = 0 and JZ_JE = Decl.JE_PK  
                 AND InnerJobCom.JI_CustomAttrib4 = Container.CO_ContainerNumber                                                       
                )      
    LEFT OUTER JOIN dbo.JobOrderHeader ON JD_OrderNumber = JI_OrderNumber
    LEFT OUTER JOIN dbo.OrgAddress BuyerAddress ON BuyerAddress.OA_PK = JobOrderHeader.JD_OA_BuyerAddress
    LEFT OUTER JOIN dbo.OrgHeader OrderedBy ON BuyerAddress.OA_OH = OrderedBy.OH_PK  
    LEFT OUTER JOIN dbo.JobHeader Job  ON Decl.JE_PK = Job.JH_ParentID  
    LEFT OUTER JOIN dbo.JobCharge FreightCharge ON Job.JH_PK = FreightCharge.JR_JH AND FreightCharge.JR_AC = (select AC_PK from dbo.AccChargeCode ChargeCode where ChargeCode.AC_Code='FRT')  
    LEFT OUTER JOIN dbo.RefCurrency FreightChargeCurrency ON FreightCharge.JR_RX_NKSellCurrency = FreightChargeCurrency.RX_Code  
    LEFT OUTER JOIN dbo.JobCharge PSCCharge ON Job.JH_PK = PSCCharge.JR_JH AND PSCCharge.JR_AC = (select AC_PK from dbo.AccChargeCode ChargeCode where ChargeCode.AC_Code='PSC')  
  
   WHERE (JE_IsCancelled is null or JE_IsCancelled != 1)  
   AND (@ExportStartDate IS NULL OR @ExportStartDate = '' OR JE_ExportDate >= @ExportStartDate)  
   AND (@ExportEndDate IS NULL OR @ExportEndDate = '' OR JE_ExportDate < @ExportEndDate)  
   AND (@ShippingLinePK IS NULL OR JE_OH_ShippingLine = @ShippingLinePK)  
   AND (@Vessel = '' OR @Vessel IS NULL OR JE_VesselName = @Vessel)  
   AND (@Voyage = '' OR @Voyage IS NULL OR JE_VoyageFlightNo = @Voyage)";

		#endregion

		#region vw_Report_ClientInvoicePaymentNotify

		public const string Drop_vw_Report_ClientInvoicePaymentNotify = "DROP VIEW vw_Report_ClientInvoicePaymentNotify";
		public const string Create_vw_Report_ClientInvoicePaymentNotify = @"
			CREATE VIEW vw_Report_ClientInvoicePaymentNotify
			AS
			SELECT Invoice.JZ_InvoiceNumber As InvoiceNo,
 				Invoice.JZ_InvoiceDate As InvoiceDate,
 				Invoice.JZ_PaymentNo As PaymentNo,
				Currency.RX_Code As Currency,
 				Invoice.JZ_PaymentAmount As PaymentAmount,
				Invoice.JZ_OH_Supplier As SupplierPK

			FROM dbo.JobDeclaration Decl 
 				LEFT OUTER JOIN dbo.JobComInvoiceHeader Invoice  ON Decl.JE_PK = Invoice.JZ_JE
 				LEFT OUTER JOIN dbo.RefCurrency Currency  ON Invoice.JZ_RX_NKInvoice_Currency = Currency.RX_Code
 				LEFT OUTER JOIN dbo.RefExchangeRate InvoiceExchangeRate  ON InvoiceExchangeRate.RE_ExRateType = 'CUS' AND Decl.JE_ExportDate >= InvoiceExchangeRate.RE_StartDate and Decl.JE_ExportDate <= InvoiceExchangeRate.RE_ExpiryDate AND InvoiceExchangeRate.RE_RX_NKExCurrency = Currency.RX_Code
 				LEFT OUTER JOIN dbo.RefExchangeRate AUDExchangeRate      ON AUDExchangeRate.RE_ExRateType     = 'CUS' AND Decl.JE_ExportDate >= AUDExchangeRate.RE_StartDate     and Decl.JE_ExportDate <= AUDExchangeRate.RE_ExpiryDate     AND AUDExchangeRate.RE_RX_NKExCurrency     in (select RX_Code from dbo.RefCurrency where RX_Code='AUD')

			WHERE Invoice.JZ_PaymentNo != ''
			AND (JE_IsCancelled is null or JE_IsCancelled != 1)";

		#endregion

		#region vw_Report_ClientOrderBuyersTripReport

		public const string Drop_vw_Report_ClientOrderBuyersTripReport = "DROP VIEW vw_Report_ClientOrderBuyersTripReport";
		public const string Create_vw_Report_ClientOrderBuyersTripReport = @"
			CREATE VIEW vw_Report_ClientOrderBuyersTripReport
			AS
					SELECT DISTINCT OP_Department As Department,
				OP_PartNum As Partno,
				OP_Desc As ItemDescription,
				JO_ItemPrice As ItemPrice,
				Currency.RX_Code As Currency,
				JO_CustomDecimal1 As LastLandedCost,
				OP_QtyInStock As QtyInStock,
				min(JO_CustomDate1) As MinEstDepartureDate,
				max(JO_CustomDate1) As MaxEstDepartureDate,
				Supplier.OH_PK As SupplierPK,
				Supplier.OH_FullName As Supplier,
				JD_CustomAttrib1 As PaymentType,
				Port.RL_PortName As PortOfLoadingName,
				Port.RL_Code As PortOfLoadingCode,
				RL_PK As PortOfLoadingPK,
				JD_IncoTerm As IncoTerm,
				(CASE WHEN JO_CustomAttrib6='' OR JO_CustomAttrib6 IS NULL 
					  THEN JD_FirstBuyerContact 
					  ELSE JO_CustomAttrib6 
				 END) As BusinessManager ,
				sum(JO_Quantity) As Quantity

			FROM dbo.OrgSupplierPart Product
				LEFT JOIN dbo.OrgPartRelation PartRelation ON Product.OP_PK = PartRelation.OU_OP
				INNER JOIN dbo.JobOrderLine Line ON Line.JO_Partno = Product.OP_PartNum AND Line.JO_LineStatus != 'CAN'
				LEFT JOIN dbo.JobOrderHeader Header ON Header.JD_PK = Line.JO_JD AND Header.JD_OrderStatus != 'CAN'
				LEFT JOIN dbo.OrgAddress SupplierAddress ON Header.JD_OA_SupplierAddress = SupplierAddress.OA_PK
				LEFT JOIN dbo.OrgHeader Supplier ON SupplierAddress.OA_OH = Supplier.OH_PK
				LEFT JOIN dbo.RefCurrency Currency ON Header.JD_RX_NKOrderCurrency = Currency.RX_Code
				LEFT JOIN dbo.RefUNLOCO Port ON JD_RL_NKPortOfLoading = RL_Code

			GROUP BY OP_PartNum, OP_Department, OP_Desc, JO_ItemPrice, OP_QtyInStock, Currency.RX_Code, JO_CustomDecimal1, Supplier.OH_PK, Supplier.OH_FullName, JD_CustomAttrib1, Port.RL_Code, Port.RL_PortName, JD_IncoTerm, JO_CustomDate1, RL_PK, JD_FirstBuyerContact,JO_CustomAttrib6";
		#endregion

		#region vw_Report_ClientOrderInvoiceDiscrepancy

		public const string Drop_vw_Report_ClientOrderInvoiceDiscrepancy = "DROP VIEW vw_Report_ClientOrderInvoiceDiscrepancy";
		public const string Create_vw_Report_ClientOrderInvoiceDiscrepancy = @"
			CREATE VIEW vw_Report_ClientOrderInvoiceDiscrepancy
			AS
						SELECT JE_SystemCreateTimeUtc As InvoiceCreationDate,
				JD_SecondBuyerContact As RebuyerName,
				Buyer.OH_PK As BuyerPK,
				OrderSupplier.OH_PK As OrderSupplierPK,
				OrderSupplier.OH_FullName As OrderSupplierFullName,
				DeclInvoiceSupplier.OH_PK As InvoiceSupplierPK,
				DeclInvoiceSupplier.OH_FullName As InvoiceSupplierFullName,
				DeclarationSupplier.OH_PK As DeclarationSupplierPK,
				DeclarationSupplier.OH_FullName As DeclarationSupplierFullName,
				JD_OrderNumber As OrderNumber,
				JO_Partno As ProductNo,
				JO_Description As ItemDescription,
				JZ_InvoiceNumber As InvoiceNo,
				JE_RL_NKFinalDestination As FinalDestination,
				J5_ContainerNum As ContainerNum,
				J4_Allocated As QuantityDelivered,
				J4_CustomDecimal5 As QuantityOrdered,
				JO_ItemPrice As OrderItemPrice,
				(case JI_InvoiceQuantity WHEN 0 THEN 0 ELSE (round(JI_LinePrice / JI_InvoiceQuantity, 4)) END) As InvoiceItemPrice,
				JO_F3_NKPackType As OrderUQ,
				JI_InvoiceUQ As InvoiceUQ,
				JO_LinePrice As OrderLinePrice,
				JI_LinePrice As InvoiceLinePrice,
				OrderCurrency.RX_Code As OrderCurrency,
				InvoiceCurrency.RX_Code As InvoiceCurrency,
				JD_RL_NKPortOfLoading As OrderPortOfLoading,
				JE_RL_NKPortOfLoading As DeclarationPortOfLoading,
				J4_OA_NKDeliveryPoint As OrderDeliverPoint,
				(CASE DocAddress.E2_OA_Address WHEN NULL THEN '' ELSE DeclDeliveryAddress.OA_Code END) As DeclarationDeliveryAddress
			FROM dbo.JobDeclaration Declaration
				INNER JOIN dbo.JobComInvoiceHeader DeclInvoice ON Declaration.JE_PK = DeclInvoice.JZ_JE AND DeclInvoice.JZ_GroupInvoice = 0
				INNER JOIN dbo.JobComInvoiceLine DeclInvoiceLine ON DeclInvoiceLine.JI_JZ = DeclInvoice.JZ_PK
				INNER JOIN dbo.JobOrderHeader OrderHeader ON DeclInvoiceLine.JI_OrderNumber = OrderHeader.JD_OrderNumber AND OrderHeader.JD_OrderStatus != 'CAN'
				INNER JOIN dbo.JobOrderLine OrderLine ON OrderLine.JO_JD = OrderHeader.JD_PK AND DeclInvoiceLine.JI_PartNo = OrderLine.JO_Partno AND OrderLine.JO_LineStatus != 'CAN' AND DeclInvoiceLine.JI_CustomDecimal1 = OrderLine.JO_LineNo 
				INNER JOIN dbo.JobOrderLineDelivery OrderDelivery ON OrderLine.JO_PK = OrderDelivery.J4_JO AND Declaration.JE_RL_NKFinalDestination = OrderDelivery.J4_RL_NKDestinationPort
				LEFT JOIN dbo.JobOrderLineDeliverContainer DeliverContainer ON OrderDelivery.J4_PK = DeliverContainer.J5_J4 AND DeliverContainer.J5_ContainerNum= DeclInvoiceLine.JI_CustomAttrib4
				LEFT JOIN dbo.OrgAddress OrderSupplierAddress ON OrderHeader.JD_OA_SupplierAddress = OrderSupplierAddress.OA_PK
				LEFT JOIN dbo.OrgHeader OrderSupplier ON OrderSupplierAddress.OA_OH = OrderSupplier.OH_PK
				LEFT JOIN dbo.OrgHeader DeclarationSupplier ON Declaration.JE_OH_Supplier = DeclarationSupplier.OH_PK
				LEFT JOIN dbo.OrgHeader DeclInvoiceSupplier ON DeclInvoice.JZ_OH_Supplier = DeclInvoiceSupplier.OH_PK
				LEFT JOIN dbo.OrgHeader InvoiceSupplier ON DeclInvoice.JZ_OH_Supplier = InvoiceSupplier.OH_PK
				LEFT JOIN dbo.OrgAddress BuyerAddress ON OrderHeader.JD_OA_BuyerAddress = BuyerAddress.OA_PK
				LEFT JOIN dbo.OrgHeader Buyer ON BuyerAddress.OA_OH = Buyer.OH_PK
				LEFT JOIN dbo.RefCurrency OrderCurrency ON OrderHeader.JD_RX_NKOrderCurrency = OrderCurrency.RX_Code
				LEFT JOIN dbo.RefCurrency InvoiceCurrency ON DeclInvoice.JZ_RX_NKInvoice_Currency = InvoiceCurrency.RX_Code
                LEFT JOIN dbo.JobDocAddress DocAddress ON Declaration.JE_PK = DocAddress.E2_ParentID AND DocAddress.E2_AddressType = 'IMG'
                LEFT JOIN dbo.OrgAddress DeclDeliveryAddress ON DocAddress.E2_OA_Address = DeclDeliveryAddress.OA_PK
				

			WHERE (JE_IsCancelled is null or JE_IsCancelled != 1) AND
				(
				OrderSupplier.OH_PK != DeclarationSupplier.OH_PK OR
				JO_ItemPrice != (case JI_InvoiceQuantity WHEN 0 THEN 0 ELSE (round(JI_LinePrice / JI_InvoiceQuantity, 4)) END) OR
				JO_F3_NKPackType != JI_InvoiceUQ OR
				JO_LinePrice != JI_LinePrice OR
				JD_RL_NKPortOfLoading != JE_RL_NKPortOfLoading OR
				J4_OA_NKDeliveryPoint != DeclDeliveryAddress.OA_Code
				)";

		#endregion

		#region vw_Report_ClientOrdersSignOff

		public const string Drop_vw_Report_ClientOrdersSignOff = "DROP VIEW vw_Report_ClientOrdersSignOff";
		public const string Create_vw_Report_ClientOrdersSignOff = @"
			CREATE VIEW vw_Report_ClientOrdersSignOff
			AS
			SELECT JD_PK As OrderPK,
				JD_OrderNumber As OrderNumber,
				JD_OrderNumber + ':' + convert(varchar(3), JD_OrderNumberSplit) As OrderAndSplitNumber,
				JD_OrderDate As OrderDate,
				(CASE WHEN JO_CustomAttrib6='' OR JO_CustomAttrib6 IS NULL 
					  THEN JD_FirstBuyerContact 
					  ELSE JO_CustomAttrib6 
				 END) As FirstBuyerContact,
				JD_SecondBuyerContact As SecondBuyerContact,
				Supplier.OH_FullName As SupplierFullName,
				Supplier.OH_PK As SupplierPK,
				JD_IncoTerm As IncoTerm,
				Currency.RX_Code As Currency,
				JD_RL_NKPortOfLoading As PortOfLoading,
				PortOfLoading.RL_PortName As PortOfLoadingName,
				JD_CustomAttrib1 As PaymentType,
				dbo.ClientGetAllClientVisibleNotes(JD_PK) As OrderNotes,

				JO_PK As OrderLinePK,
				JO_Partno As Partno,
				JO_Description As ItemDescription,
				OP_Department As Department,
				JO_OuterPacks As OuterPacks,
				JO_InnerPacks As InnerPacks,
				JO_ItemPrice As ItemPrice,
				JO_LinePrice As LinePrice,
				JO_F3_NKPackType As LineUnitOfQty,
				JO_CustomDate4 As AdvertDate,
				JO_CustomDate5 As NewLineDate,
				JO_CustomDecimal1 As EstLandedCost,
				JO_CustomDate1 As ShipDate,
				JO_CustomDate2 As PortArrivalDate,
				JO_Quantity As LineQuantityOrdered,

				J4_PK As DeliveryPK,
				convert(varchar(100), J4_PK) + '_' + DeliverPoint.OA_Code As DeliveryPKAndWarehouseNum,
				J4_RL_NKDestinationPort As DestinationPort,
				DeliverPoint.OA_Code As WarehouseNum,
				J4_CustomDecimal5 As QuantityOrdered,
				J4_Allocated As QuantityInvoiced,
				J4_CustomAttribute1 As POMNum

			FROM  dbo.JobOrderHeader Header
				INNER JOIN dbo.OrgAddress BuyerAddress ON Header.JD_OA_BuyerAddress = BuyerAddress.OA_PK
				INNER JOIN dbo.OrgHeader Buyer ON BuyerAddress.OA_OH = Buyer.OH_PK
				LEFT JOIN dbo.JobOrderLine Line ON Header.JD_PK = Line.JO_JD AND JO_LineStatus != 'CAN'
				LEFT JOIN dbo.JobOrderLineDelivery Delivery ON Line.JO_PK = Delivery.J4_JO
				LEFT JOIN dbo.RefCurrency Currency ON Header.JD_RX_NKOrderCurrency = Currency.RX_Code
				LEFT JOIN dbo.OrgAddress SupplierAddress ON Header.JD_OA_SupplierAddress = SupplierAddress.OA_PK
				LEFT JOIN dbo.OrgHeader Supplier ON SupplierAddress.OA_OH = Supplier.OH_PK
				LEFT JOIN dbo.OrgAddress DeliverPoint ON Delivery.J4_OA_NKDeliveryPoint = DeliverPoint.OA_Code AND Buyer.OH_PK = DeliverPoint.OA_OH
				LEFT JOIN dbo.RefUNLOCO PortOfLoading ON Header.JD_RL_NKPortOfLoading = PortOfLoading.RL_Code
				LEFT JOIN dbo.OrgSupplierPart Product ON Line.JO_Partno = Product.OP_PartNum  AND Product.OP_PK =
				(
					  SELECT TOP 1 OP_PK
					  FROM dbo.OrgSupplierPart InnerPart
					  WHERE 	 
						(
							(OP_PK IN (SELECT OU_OP FROM dbo.OrgPartRelation WHERE OU_OH = Buyer.OH_PK and OU_Relationship in ('OWN', 'BTH'))) 
							or 
							(OP_PK IN (SELECT OU_OP FROM dbo.OrgPartRelation WHERE OU_OH = Supplier.OH_PK and OU_Relationship in ('SUP', 'BTH')))
						) and OP_IsActive = 1
					 AND InnerPart.OP_PartNum = Product.OP_PartNum
				)
			WHERE JD_OrderStatus != 'CAN'";

		#endregion

		#region Client_WOW_OutstandingOrdersReport
		public const string Drop_Client_WOW_OutstandingOrdersReport = "DROP FUNCTION Client_WOW_OutstandingOrdersReport";
		public const string Create_Client_WOW_OutstandingOrdersReport = @"
CREATE FUNCTION Client_WOW_OutstandingOrdersReport(@IsConfirmed varchar(15))
			RETURNS TABLE
			AS
			RETURN
			SELECT JD_OrderDate As OrderDate,
				JD_BookingConfDate As ConfirmDate,
				JD_OrderNumber As OrderNumber,
				Supplier.OH_FullName As SupplierFullName,
				Buyer.OH_FullName As BuyerFullName,
				Buyer.OH_PK As BuyerPK,
				(CASE WHEN JO_CustomAttrib6='' OR JO_CustomAttrib6 IS NULL 
					  THEN JD_FirstBuyerContact 
					  ELSE JO_CustomAttrib6 
				 END) As BuyerContact,
				JD_SecondBuyerContact As RebuyerContact,
				Supplier.OH_PK As SupplierPK,
				JD_RL_NKPortOfLoading As PortOfLoading,
				PortOfLoading.RL_PortName As PortOfLoadingName,
				JO_CustomDate1 As ShipDate,
				JO_Partno As Partno,
				JO_Description As Description,
				OP_Department As Department,
				OP_Division As Division,
				Currency.RX_Code As Currency,
				JO_ItemPrice As LineItemPrice,
				J4_CustomDecimal5 As QuantityOrderedByPort,
				J4_Allocated As QuantityDeliveredByPort,
				J4_OA_NKDeliveryPoint As DeliveryPoint,
				J4_CustomAttribute1 As POMNum,
				JO_PK As OrderLinePK

			FROM dbo.JobOrderHeader Header 
				INNER JOIN dbo.OrgAddress BuyerAddress ON Header.JD_OA_BuyerAddress = BuyerAddress.OA_PK
				INNER JOIN dbo.OrgHeader Buyer ON BuyerAddress.OA_OH = Buyer.OH_PK
				LEFT OUTER JOIN dbo.JobOrderLine Line ON Header.JD_PK = Line.JO_JD AND JO_LineStatus != 'CAN'
				LEFT OUTER JOIN dbo.JobOrderLineDelivery Delivery ON Line.JO_PK = Delivery.J4_JO
				LEFT OUTER JOIN dbo.OrgAddress SupplierAddress ON Header.JD_OA_SupplierAddress = SupplierAddress.OA_PK
				LEFT OUTER JOIN dbo.OrgHeader Supplier ON SupplierAddress.OA_OH = Supplier.OH_PK
				LEFT OUTER JOIN dbo.RefCurrency Currency ON Header.JD_RX_NKOrderCurrency = Currency.RX_Code
				LEFT OUTER JOIN dbo.RefUNLOCO PortOfLoading ON Header.JD_RL_NKPortOfLoading = PortOfLoading.RL_Code
				LEFT JOIN dbo.OrgSupplierPart Product ON Line.JO_Partno = Product.OP_PartNum  AND Product.OP_PK =
				(
					  SELECT TOP 1 OP_PK
					  FROM dbo.OrgSupplierPart InnerPart
					  WHERE 	 
						(
							(OP_PK IN (SELECT OU_OP FROM dbo.OrgPartRelation WHERE OU_OH = Buyer.OH_PK and OU_Relationship in ('OWN', 'BTH'))) 
							or 
							(OP_PK IN (SELECT OU_OP FROM dbo.OrgPartRelation WHERE OU_OH = Supplier.OH_PK and OU_Relationship in ('SUP', 'BTH')))
						) and OP_IsActive = 1
					 AND InnerPart.OP_PartNum = Product.OP_PartNum
				)
			WHERE (Delivery.J4_CustomDecimal5 - Delivery.J4_Allocated) > 0 AND Header.JD_IsCancelled = 0 AND 
				((@IsConfirmed = 'CONFIRMED' AND JD_BookingConfDate IS NOT NULL) OR
				(@IsConfirmed = 'NOTCONFIRMED' AND JD_BookingConfDate IS NULL) OR 
				@IsConfirmed = '')";
		#endregion

		#region WOW_Report_ClientOverseasConsolidatorOrderReport

		public const string Drop_WOW_Report_ClientOverseasConsolidatorOrderReport = "DROP FUNCTION Client_WOW_Report_ClientOverseasConsolidatorOrderReport";
		public const string Create_WOW_Report_ClientOverseasConsolidatorOrderReport = @"
		CREATE FUNCTION Client_WOW_Report_ClientOverseasConsolidatorOrderReport
(
	@BuyerPKList varchar(1000),
	@LoadPort varchar(5),
	@LoadPortCountry varchar(2),
	@SupplierCountry varchar(2),
	@SendingAgentPK uniqueidentifier,
	@ShipDateFrom smalldatetime,
	@ShipDateTo smalldatetime
)
RETURNS @Result TABLE
(
	OrderLinePK uniqueidentifier,
	OrderDate smalldatetime,
	Supplier nvarchar(100),
	OrderNumber varchar(35),
	Partno varchar(80),
	PartDescription varchar(80),
	LoadPort varchar(5),
	ShipDate smalldatetime,
	Division varchar(15),
	TotalQuantity decimal,
	TotalQuantityInCartons decimal,
	TotalQtyP1 decimal,
	TotalQtyP2 decimal,
	TotalQtyP3 decimal,
	TotalQtyP4 decimal,
	TotalQtyP5 decimal,
	TotalQtyP6 decimal,
	TotalQtyP7 decimal,
	TotalQtyP8 decimal,
	TotalQtyP9 decimal,
	TotalQtyP10 decimal,
	HeaderP1 varchar(35),
	HeaderP2 varchar(35),
	HeaderP3 varchar(35),
	HeaderP4 varchar(35),
	HeaderP5 varchar(35),
	HeaderP6 varchar(35),
	HeaderP7 varchar(35),
	HeaderP8 varchar(35),
	HeaderP9 varchar(35),
	HeaderP10 varchar(35)
)
BEGIN
INSERT INTO @Result
(
	OrderLinePK,
	OrderDate,
	Supplier,
	OrderNumber,
	Partno,
	PartDescription,
	LoadPort,
	ShipDate,
	Division,
	TotalQuantity,
	TotalQuantityInCartons

)
SELECT 
	JO_PK,
	JD_OrderDate,
	Supplier.OH_FullName,
	JD_OrderNumber,
	JO_Partno,
	JO_Description,
	JD_RL_NKPortOfLoading,
	JO_CustomDate1 As ShipDate,
	OP_Division As Division,
	JO_Quantity As TotalQuantity,
	(case
		when JO_OuterPacks = 0 then 0
		else JO_Quantity / JO_OuterPacks
	end) As TotalQuantityInCartons				
	FROM dbo.JobOrderHeader Header 
		INNER JOIN dbo.OrgAddress BuyerAddress ON Header.JD_OA_BuyerAddress = BuyerAddress.OA_PK
		INNER JOIN dbo.OrgHeader Buyer ON BuyerAddress.OA_OH = Buyer.OH_PK
		LEFT OUTER JOIN dbo.JobOrderLine Line ON Header.JD_PK = Line.JO_JD AND Line.JO_LineStatus != 'CAN'
		LEFT OUTER JOIN dbo.OrgAddress SupplierAddress ON Header.JD_OA_SupplierAddress = SupplierAddress.OA_PK
		LEFT OUTER JOIN dbo.OrgHeader Supplier ON SupplierAddress.OA_OH = Supplier.OH_PK
		LEFT OUTER JOIN dbo.OrgHeader SendingAgent ON Header.JD_OH_SendingAgent=SendingAgent.OH_PK
		LEFT JOIN dbo.OrgSupplierPart Product ON Line.JO_Partno = Product.OP_PartNum  AND Product.OP_PK =
		(
			  SELECT TOP 1 OP_PK
			  FROM dbo.OrgSupplierPart InnerPart
			  WHERE 	 
				(
					(OP_PK IN (SELECT OU_OP FROM dbo.OrgPartRelation WHERE OU_OH = Buyer.OH_PK and OU_Relationship in ('OWN', 'BTH'))) 
					or 
					(OP_PK IN (SELECT OU_OP FROM dbo.OrgPartRelation WHERE OU_OH = Supplier.OH_PK and OU_Relationship in ('SUP', 'BTH')))
				) and OP_IsActive = 1
			 AND InnerPart.OP_PartNum = Product.OP_PartNum
		)
	WHERE JD_OrderStatus != 'CAN'
	AND ( ISNULL(@BuyerPKList, '') = '' OR CHARINDEX(CAST(Buyer.OH_PK AS VARCHAR(36)), @BuyerPKList) > 0)
	AND ( ISNULL(@LoadPort, '') = '' OR JD_RL_NKPortOfLoading = @LoadPort)
	AND ( ISNULL(@LoadPortCountry, '') ='' OR @LoadPortCountry = LEFT(JD_RL_NKPortOfLoading,2))
	AND ( ISNULL(@SupplierCountry, '') = '' OR @SupplierCountry = LEFT(Supplier.OH_RL_NKClosestPort,2))
	AND ( @SendingAgentPK IS NULL OR @SendingAgentPK = SendingAgent.OH_PK)
	AND ( @ShipDateFrom = '' OR @ShipDateFrom <=  JO_CustomDate1)
	AND ( @ShipDateTo = '' OR @ShipDateTo > JO_CustomDate1)
	AND JD_IsCancelled = 0
			
	DECLARE
	
	@Count int,
	@Code varchar(5),
	@PortName varchar(35),
	@Header1 varchar(5),   
	@Header2 varchar(5),   
	@Header3 varchar(5),   
	@Header4 varchar(5),   
	@Header5 varchar(5),   
	@Header6 varchar(5),   
	@Header7 varchar(5),   
	@Header8 varchar(5),
	@Header9 varchar(5),   
	@Header10 varchar(5)
	
	set @Count =1
	
	DECLARE CodeCursor CURSOR FAST_FORWARD READ_ONLY FOR 
	SELECT distinct J4_RL_NKDestinationPort 
	FROM dbo.JobOrderHeader Header 
		INNER JOIN dbo.JobOrderLine Line ON Header.JD_PK = Line.JO_JD AND Line.JO_LineStatus != 'CAN'
		INNER JOIN dbo.JobOrderLineDelivery ON J4_JO = JO_PK
		INNER JOIN dbo.OrgAddress BuyerAddress ON Header.JD_OA_BuyerAddress = BuyerAddress.OA_PK
		INNER JOIN dbo.OrgHeader Buyer ON BuyerAddress.OA_OH = Buyer.OH_PK
		LEFT JOIN dbo.OrgAddress SupplierAddress ON Header.JD_OA_SupplierAddress = SupplierAddress.OA_PK
		LEFT JOIN dbo.OrgHeader Supplier ON SupplierAddress.OA_OH = Supplier.OH_PK
		LEFT JOIN dbo.OrgHeader SendingAgent ON Header.JD_OH_SendingAgent=SendingAgent.OH_PK
		
	WHERE JD_OrderStatus != 'CAN'
	AND ( ISNULL(@BuyerPKList, '') = '' OR CHARINDEX(CAST(Buyer.OH_PK AS VARCHAR(36)), @BuyerPKList) > 0)
	AND ( ISNULL(@LoadPort, '') = '' OR JD_RL_NKPortOfLoading = @LoadPort)
	AND ( ISNULL(@LoadPortCountry, '') ='' OR @LoadPortCountry = LEFT(JD_RL_NKPortOfLoading,2))
	AND ( ISNULL(@SupplierCountry, '') = '' OR @SupplierCountry = LEFT(Supplier.OH_RL_NKClosestPort,2))
	AND ( @SendingAgentPK IS NULL OR @SendingAgentPK = SendingAgent.OH_PK)
	AND ( ISNULL(@ShipDateFrom, '') = '' OR @ShipDateFrom <=  JO_CustomDate1)
	AND ( ISNULL(@ShipDateTo, '') = '' OR @ShipDateTo > JO_CustomDate1)
	AND (J4_RL_NKDestinationPort IS NOT NULL OR LTRIM(J4_RL_NKDestinationPort) !='')
	AND (J4_CustomDecimal5 != 0)
	AND JD_IsCancelled = 0
	GROUP BY J4_RL_NKDestinationPort
	HAVING LEN(J4_RL_NKDestinationPort) > 0
	
	OPEN CodeCursor
	FETCH NEXT FROM CodeCursor INTO @Code
	WHILE @@FETCH_STATUS = 0 
	BEGIN
	
	SET @PortName = (SELECT Top 1 RL_PortName From dbo.RefUNLOCO WHERE RL_Code = @Code)
	
	if @Count = 1 
	BEGIN  
		UPDATE @Result SET HeaderP1 = ISNULL(@PortName, @Code)
		SET @Header1 = @Code	
	END
	
	if @Count = 2 
	BEGIN  
		UPDATE @Result SET HeaderP2 = ISNULL(@PortName, @Code)
		SET @Header2 = @Code
	END
	
	if @Count = 3 
	BEGIN  
		UPDATE @Result SET HeaderP3 = ISNULL(@PortName, @Code)
		SET @Header3 = @Code
	END
	
	if @Count = 4 
	BEGIN  
		UPDATE @Result SET HeaderP4 = ISNULL(@PortName, @Code)
		SET @Header4 = @Code
	END
	if @Count = 5 
	BEGIN  
		UPDATE @Result SET HeaderP5 = ISNULL(@PortName, @Code)
		SET @Header5 = @Code
	END
	
	if @Count = 6 
	BEGIN  
		if @Code IS NOT NULL OR LTRIM(@Code) != ''
		BEGIN
		UPDATE @Result SET HeaderP6 = ISNULL(@PortName, @Code)
		SET @Header6 = @Code
		END
	END
	
	if @Count = 7 
	BEGIN  
		UPDATE @Result SET HeaderP7 = ISNULL(@PortName, @Code)
		SET @Header7 = @Code 
	END
	
	if @Count = 8 
	BEGIN  
		UPDATE @Result SET HeaderP8 = ISNULL(@PortName, @Code)
		SET @Header8 = @Code
	END
	
	if @Count = 9
	BEGIN  
		UPDATE @Result SET HeaderP9 = ISNULL(@PortName, @Code)
		SET @Header8 = @Code
	END
	
	if @Count = 10 
	BEGIN  
		UPDATE @Result SET HeaderP10 = ISNULL(@PortName, @Code)
		SET @Header8 = @Code
	END
	
	SET @Count = @Count + 1  
	
	FETCH NEXT FROM CodeCursor INTO @Code  

	END
	
	CLOSE CodeCursor  
	DEALLOCATE CodeCursor  			
				
	UPDATE @Result
	SET TotalQtyP1 = (SELECT sum(J4_CustomDecimal5) FROM dbo.JobOrderLineDelivery WHERE J4_JO = OrderLinePK AND J4_RL_NKDestinationPort = @Header1),
		TotalQtyP2 = (SELECT sum(J4_CustomDecimal5) FROM dbo.JobOrderLineDelivery WHERE J4_JO = OrderLinePK AND J4_RL_NKDestinationPort = @Header2),
		TotalQtyP3 = (SELECT sum(J4_CustomDecimal5) FROM dbo.JobOrderLineDelivery WHERE J4_JO = OrderLinePK AND J4_RL_NKDestinationPort = @Header3),
		TotalQtyP4 = (SELECT sum(J4_CustomDecimal5) FROM dbo.JobOrderLineDelivery WHERE J4_JO = OrderLinePK AND J4_RL_NKDestinationPort = @Header4),
		TotalQtyP5 = (SELECT sum(J4_CustomDecimal5) FROM dbo.JobOrderLineDelivery WHERE J4_JO = OrderLinePK AND J4_RL_NKDestinationPort = @Header5),
		TotalQtyP6 = (SELECT sum(J4_CustomDecimal5) FROM dbo.JobOrderLineDelivery WHERE J4_JO = OrderLinePK AND J4_RL_NKDestinationPort = @Header6),
		TotalQtyP7 = (SELECT sum(J4_CustomDecimal5) FROM dbo.JobOrderLineDelivery WHERE J4_JO = OrderLinePK AND J4_RL_NKDestinationPort = @Header7),
		TotalQtyP8 = (SELECT sum(J4_CustomDecimal5) FROM dbo.JobOrderLineDelivery WHERE J4_JO = OrderLinePK AND J4_RL_NKDestinationPort = @Header8),
		TotalQtyP9 = (SELECT sum(J4_CustomDecimal5) FROM dbo.JobOrderLineDelivery WHERE J4_JO = OrderLinePK AND J4_RL_NKDestinationPort = @Header9),
		TotalQtyP10 = (SELECT sum(J4_CustomDecimal5) FROM dbo.JobOrderLineDelivery WHERE J4_JO = OrderLinePK AND J4_RL_NKDestinationPort = @Header10)		
	
			
RETURN

END";

		#endregion

		#region vw_Report_ClientOrderContainersInYard

		public const string Drop_vw_Report_ClientOrderContainersInYard = "DROP VIEW vw_Report_ClientOrderContainersInYard";
		public static string Create_vw_Report_ClientOrderContainersInYard
		{
			get
			{
				return @"
				CREATE VIEW vw_Report_ClientOrderContainersInYard  
AS 
SELECT 
	JE_PK as DeclarationPK,
	Declaration.JE_SystemCreateTimeUtc as DeclarationCreateTimeUtc,   
	CusContainer.CO_PK As CusContainerPK,   
	Declaration.JE_DeclarationReference As DeclarationReference,  
	min(CusContainer.CO_ContainerNumber) As ContainerNum,   
	min(ContainerType.RC_Code) As ContainerType,  
	isnull(JE_VesselName, '<empty>') As ArrivalVessel,  
	isnull(JE_VoyageFlightNo, '<empty>') As ArrivalVoyage,  
	min(DeclInvoiceLine.JI_Partno) As Partno,   
	min(JI_Description) As Description,  
	Buyer.OH_PK As BuyerPK,  
	min(Buyer.OH_Code) As BuyerCode,  
	min(Buyer.OH_FullName) As BuyerFullName,  
	OrderDelivery.J4_OA_NKDeliveryPoint As DeliveryAddress,  
	max(JobContainer.JC_FCLWharfGateOut) as ContainerInYardDate,
	sum(J5_PackCount) As TotalPacksInCartons,  
	min(Product.OP_VendorPackQty) As VendorPackQty,  
	min(DeclInvoiceCurrency.RX_Code) As InvoiceCurrencyCode,  
	max(OrderDelivery.J4_CustomAttribute1) As POMNum,  
	DeclInvoiceLine.JI_OrderNumber As OrderNumber ,
	max(InvoiceItemLink.totalLinePrice) As InvoiceLinePrice,
	max(InvoiceItemLink.InvoiceItemPrice) As InvoiceItemPrice,
	max(JC_ArrivalCartageComplete) As DLVEventTime,
	JE_GB AS BranchPK
	
FROM dbo.JobComInvoiceLine DeclInvoiceLine 
INNER JOIN dbo.JobComInvoiceHeader DeclInvoice ON JZ_PK = JI_JZ AND JZ_GroupInvoice = 0
INNER JOIN dbo.JobDeclaration Declaration ON JE_PK = JZ_JE
INNER JOIN dbo.CusContainer CusContainer ON CusContainer.CO_JE = Declaration.JE_PK and Declaration.JE_IsCancelled = 0 AND JI_CustomAttrib4 = CusContainer.CO_ContainerNumber
LEFT JOIN dbo.RefContainer ContainerType ON CusContainer.CO_RC = ContainerType.RC_PK
LEFT JOIN dbo.JobContainer ON JobContainer.JC_PK = CusContainer.CO_JC
LEFT JOIN dbo.RefCurrency DeclInvoiceCurrency ON DeclInvoice.JZ_RX_NKInvoice_Currency = DeclInvoiceCurrency.RX_Code  
LEFT JOIN dbo.OrgSupplierPart Product ON DeclInvoiceLine.JI_OP = Product.OP_PK  
LEFT JOIN dbo.JobOrderHeader OrderHeader ON JD_OrderNumber = JI_OrderNumber AND JD_OrderStatus != 'CAN'
LEFT JOIN dbo.OrgAddress BuyerAddress ON OrderHeader.JD_OA_BuyerAddress = BuyerAddress.OA_PK
LEFT JOIN dbo.OrgHeader Buyer ON BuyerAddress.OA_OH = Buyer.OH_PK  
LEFT JOIN dbo.JobOrderLine OrderLine ON OrderLine.JO_JD = OrderHeader.JD_PK AND DeclInvoiceLine.JI_PartNo = OrderLine.JO_Partno AND OrderLine.JO_LineStatus != 'CAN' AND DeclInvoiceLine.JI_CustomDecimal1 = OrderLine.JO_LineNo 
LEFT JOIN dbo.JobOrderLineDelivery OrderDelivery ON OrderLine.JO_PK = OrderDelivery.J4_JO AND Declaration.JE_RL_NKFinalDestination = OrderDelivery.J4_RL_NKDestinationPort  
LEFT JOIN dbo.JobOrderLineDeliverContainer OrderContainer ON OrderDelivery.J4_PK = OrderContainer.J5_J4 AND CusContainer.CO_ContainerNumber=OrderContainer.J5_ContainerNum  
LEFT JOIN
(
select JI_PartNo,
	   CO_JE DecPK,
	   CO_ContainerNumber ContNumber,	
	   sum(JI_LinePrice) as totalLinePrice,
	   sum(CASE WHEN JI_InvoiceQuantity != 0 AND JI_LinePrice != 0 THEN JI_LinePrice/JI_InvoiceQuantity ELSE 0 END) as InvoiceItemPrice
from dbo.JobComInvoiceLine
inner join dbo.JobcominvoiceHeader on JI_JZ = JZ_PK
left join dbo.JobDeclaration on JZ_JE = JE_PK
inner join dbo.CusContainer on JI_CustomAttrib4 = CO_ContainerNumber and CO_JE = JE_PK 
WHERE (JE_DateOfArrival is null or JE_DateOfArrival > dateadd(month, -8, getdate())) AND CO_ContainerNUmber = JI_CustomAttrib4
group by CO_JE, CO_ContainerNumber, JI_Partno
) as InvoiceItemLink on InvoiceItemLink.JI_PartNo =DeclInvoiceLine.JI_PartNo AND InvoiceItemLink.DecPK = JE_PK AND InvoiceItemLink.ContNumber= JI_CustomAttrib4  
WHERE (JE_DateOfArrival is null or JE_DateOfArrival > dateadd(month, -8, getdate()))  
GROUP BY Declaration.JE_PK, Declaration.JE_DeclarationReference, Declaration.JE_SystemCreateTimeUtc, DeclInvoiceLine.JI_PartNo, CO_PK, CO_ContainerNumber, Buyer.OH_PK, JE_VesselName, JE_VoyageFlightNo, OrderDelivery.J4_OA_NKDeliveryPoint, JI_OrderNumber, JE_GB  

					";
				// the primary key for each record here is (DeclarationReference, Product#, Container#).
				// Other fields included in the group should be consistent with the primary key, ie. a single container cannot be assigned to 2 delivery addresses.
				// If there is an inconsistency, there will be duplicate primary keys and a key constraint violation will be raised on the target system which is
				// desired so that further investigation can take place.
			}
		}

		#endregion

		#region vw_Report_ClientContainerTracking

		public const string Drop_vw_Report_ClientContainerTracking = "DROP VIEW vw_Report_ClientContainerTracking";
		public static string Create_vw_Report_ClientContainerTracking
		{
			get
			{
				string result = @"
						CREATE VIEW vw_Report_ClientContainerTracking
					AS
					SELECT
						CASE WHEN RTRIM(JE_RL_NKFinalDestination) != ''
							THEN JE_RL_NKFinalDestination
							ELSE JE_RL_NKPortOfArrival END		AS PortOfDestination,
						Buyer.Code                   	AS ConsigneeCode,
						ShippingLine.OH_PK                  	AS ShippingLinePK,
						ShippingLine.OH_FullName            	AS ShippingLineFullName,
						JE_MasterBill                       	AS OceanBillOfLading,
						CO_ContainerNumber                  	AS ContainerNo,
						RTRIM(RC_Code)                      	AS ContainerType,
						JE_VesselName	                      	AS Vessel,
						JE_VoyageFlightNo                   	AS Voyage,
						JE_DateAtFinalDestination           	AS ETA,
						JE_GoodsDescription                 	AS GoodsDescription,
						CO_CustomAttrib1                    	AS ReeferTemperature,
						JC_ArrivalCartageComplete				AS DLVEventTime,
						JE_GB									AS BranchPK,
						CASE RTRIM(JE_EntryStatus)"
							+ WowConstants.CustomsEntryStatusCaseBodyStatement + @"
						END												AS DeclarationStatus,
						CASE WHEN JE_MessageType = 'EXP'
							THEN JP_PickupCartageAdvised
							ELSE JP_DeliveryCartageAdvised END	AS DO_SentDate,
						CO_JE

					FROM dbo.CusContainer Container
						LEFT JOIN dbo.JobDeclaration ON JE_PK = CO_JE
						LEFT JOIN 
						(
							SELECT JZ_JE as Link,
								   MAX(JI_CustomAttrib3) as Code
							FROM dbo.JobComInvoiceLine
							INNER JOIN dbo.JobComInvoiceHeader ON JZ_PK = JI_JZ AND JZ_GroupInvoice = 0
							GROUP BY JZ_JE
							
						) Buyer ON Buyer.Link = JobDeclaration.JE_PK
						LEFT JOIN dbo.OrgHeader ShippingLine ON ShippingLine.OH_PK = JE_OH_ShippingLine
						LEFT JOIN dbo.RefContainer ON RC_PK = CO_RC
						LEFT JOIN dbo.JobDocsAndCartage ON JP_ParentID = JE_PK
						LEFT JOIN dbo.JobContainer ON CO_JC = JC_PK 
					WHERE ISNULL(JE_IsCancelled, '') != '1'
";

				return result;
			}
		}

		#endregion

		#region ClientReportOrderCurrencyRequirement

		public const string Drop_ClientReportOrderCurrencyRequirement = "DROP FUNCTION ClientReportOrderCurrencyRequirement";
		public static string Create_ClientReportOrderCurrencyRequirement
		{
			get
			{
				string result = @"
                CREATE FUNCTION ClientReportOrderCurrencyRequirement(@SelectionDateType varchar(3), @StartDate datetime, @EndDate datetime)
                RETURNS TABLE
                AS
                RETURN
                select * from
                (
                    SELECT 
	                    CASE 
	                        WHEN @SelectionDateType = 'PAD' then
		                        substring(datename(mm, JO_CustomDate2), 0, 4) + ' ' 
	                               + substring(cast(datepart(yy, JO_CustomDate2) as varchar), 3, 2)
	                        ELSE
	                            substring(datename(mm, JO_CustomDate3), 0, 4) + ' ' 
	                           + substring(cast(datepart(yy, JO_CustomDate3) as varchar), 3, 2)
	                    END as Period,
	                    CASE 
	                        WHEN @SelectionDateType = 'PAD' then
		                    cast(datepart(mm, JO_CustomDate2) as varchar) + cast(datepart(yy, JO_CustomDate2) as varchar) + '__' + Currency.RX_Code 
	                        ELSE
		                    cast(datepart(mm, JO_CustomDate3) as varchar) + cast(datepart(yy, JO_CustomDate3) as varchar) + '__' + Currency.RX_Code 
	                    END as PeriodAndCurrencyGroupBy,
	                    Currency.RX_Code As Currency,
                        JO_LinePrice As LinePrice,
	                    'SelectionDate' =
		                    CASE WHEN @SelectionDateType = 'PAD' then JO_CustomDate2
		                         ELSE JO_CustomDate3 END

                        FROM  dbo.JobOrderHeader Header
                                   LEFT OUTER JOIN dbo.JobOrderLine Line ON Header.JD_PK = Line.JO_JD
                                   LEFT OUTER JOIN dbo.RefCurrency Currency ON Header.JD_RX_NKOrderCurrency = Currency.RX_Code

                        where JO_Quantity - JO_QtyReceived > 0 AND
                           JD_RX_NKOrderCurrency <> ''
                ) OrderQuery

                where OrderQuery.SelectionDate BETWEEN @StartDate and @EndDate
                ";
				return result;
			}
		}
		#endregion

		#region ClientGetLatestStmaLog

		public const string Drop_ClientGetLatestStmaLog = "DROP FUNCTION ClientGetLatestStmaLog";
		public const string Create_ClientGetLatestStmaLog = @"
			CREATE FUNCTION ClientGetLatestStmaLog(@DeclarationPK uniqueIdentifier)
			RETURNS DateTime
			As
			BEGIN
				DECLARE @LogDate DateTime
				DECLARE @LatestDate DateTime

				SET @LatestDate = (SELECT MAX(SL_PostedTimeUtc)
				 						FROM dbo.StmAlog
										WHERE SL_Parent= @DeclarationPK
										AND SL_SE_NKEvent = 'DLV' 
										AND SL_IsCancelled = 'N' 
										AND (SL_Reference = 'FCL' OR SL_Reference = 'FTL'))	
				
				SET @LogDate = (SELECT Top 1 SL_EventTime
									FROM dbo.StmAlog
									WHERE SL_Parent= @DeclarationPK
									AND SL_SE_NKEvent = 'DLV' 
									AND SL_IsCancelled = 'N' 
									AND (SL_Reference = 'FCL' OR SL_Reference = 'FTL')					 
									AND SL_PostedTimeUtc = @LatestDate)
					
				RETURN @LogDate
			END";

		#endregion

		#region ClientContainerTracking

		public const string Drop_ClientContainerTracking = "DROP FUNCTION ClientContainerTracking";
		public const string Create_ClientContainerTracking = @"
			CREATE FUNCTION ClientContainerTracking(@Show VARCHAR(30))
			RETURNS TABLE 
			AS 
			RETURN 
				SELECT * FROM vw_Report_ClientContainerTracking 
				WHERE (ISNULL(@Show, '') = 'All Containers' OR DLVEventTime IS NULL)";

		#endregion

		#region ClientCheckDeclarationJobService

		public const string Drop_ClientCheckDeclarationJobService = "DROP FUNCTION ClientCheckDeclarationJobService";
		public const string Create_ClientCheckDeclarationJobService = @"
			CREATE FUNCTION ClientCheckDeclarationJobService(@DeclarationPK Uniqueidentifier, @serviceCode Varchar(3))
			RETURNS VARCHAR(11)
			BEGIN
			DECLARE @Result varchar(11)

			SELECT 
				@Result = Isnull('C ' + Left(CONVERT(VARCHAR, ES_Completed, 6), 2)
		    								+ '-'
		    								+ SubString(CONVERT(VARCHAR, ES_Completed, 6), 4, 3)
                    					+ '-'
		    								+ SubString(CONVERT(VARCHAR, ES_Completed , 6), 8, 2)
		    							, Isnull('B ' + Left(CONVERT(VARCHAR, ES_Booked, 6), 2)
		    											+ '-'
		    											+ SubString(CONVERT(VARCHAR, ES_Booked, 6), 4, 3)
                    								+ '-'
		    											+ SubString(CONVERT(VARCHAR, ES_Booked, 6), 8, 2), 'X')) 

							FROM dbo.JobService
								INNER JOIN dbo.jobdocsandcartage on ES_ParentID = JP_PK
								INNER JOIN dbo.jobdeclaration on JE_PK = JP_ParentID and JE_PK = @DeclarationPK
							WHERE ES_ServiceCode = @ServiceCode

			RETURN @Result
			END";

		#endregion

		#region ClientGetEntryNumberList

		public const string Drop_ClientGetEntryNumberList = "DROP FUNCTION ClientGetEntryNumberList";
		public const string Create_ClientGetEntryNumberList = @"
			CREATE FUNCTION ClientGetEntryNumberList(@DeclarationPK uniqueidentifier)
			RETURNS Varchar(1000)
			AS
			BEGIN
				DECLARE @Buffer varchar(1000)
				DECLARE @EntryNumRef varchar(1000)

				SET @Buffer = ''
				IF EXISTS(SELECT CE_EntryNum
							FROM dbo.CusEntryNum
							INNER JOIN dbo.CusEntryHeader ON CE_ParentID = CH_PK
							INNER JOIN dbo.jobdeclaration ON CH_JE = JE_PK
							WHERE JE_PK = @DeclarationPK)
				BEGIN
					SELECT @Buffer = @Buffer + CE_EntryNum + ', '
					FROM dbo.CusEntryNum
						INNER JOIN dbo.CusEntryHeader ON CE_ParentID = CH_PK
						INNER JOIN dbo.jobdeclaration ON CH_JE = JE_PK
					WHERE JE_PK = @DeclarationPK
			 	
					SET @Buffer = RTRIM(@Buffer)
					SET @Buffer = LEFT(@Buffer, len(@Buffer) - 1)
				END

				RETURN @Buffer
			END";

		#endregion

		#endregion
	}
}
