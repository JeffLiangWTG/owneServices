CREATE FUNCTION RptDt_Report_GlobalForwardingAndCustomsSummary  
(   
 @TransactionFrom as datetime,  
 @TransactionTo as datetime,  
 @ReportCurrency uniqueidentifier,   
  
 @TransactionDebtorList as varchar(8000),   
 @TransactionDebtorNOTINList as varchar(8000),   
 @TransactionCreditorList as varchar(8000), -- Do not use both Creditor and Debtor as they are mutually exclusive  
 @TransactionCreditorNOTINList as varchar(8000),   
   
 @SalesRepPK as uniqueidentifier, -- Job Sales Rep    
 @OperatorPK as uniqueidentifier, -- Job Operator  
 @LocalClientPK as uniqueidentifier, -- Job Local Client  
 @OverseasAgentPK as uniqueidentifier, -- Job Overseas Agtent   
   
 @ChargeCodeList as varchar(8000),   
 @ChargeCodeNOTINList as varchar(8000),  
 @ChargeGroup as varchar (4000),  
 @SalesGroup as uniqueidentifier,  
 @ExpenseGroup as uniqueidentifier,  
  
 @ControllingCustomerList dbo.TVP_uniqueidentifier readonly,--controlling customer  
 @ControllingCustomerListIsEmpty bit,  
 @ultimateMNGPKs dbo.TVP_uniqueidentifier readonly,  
 @MNGListIsEmpty bit,  
   
 @Origin as varchar(40),  
 @Destination as varchar(40),  
 @RegistrationDateFrom as datetime,   
 @RegistrationDateTo as datetime,  
  
 @OriginETDFrom as datetime,  
 @OriginETDTo as datetime,  
 @DestinationETAFrom as datetime,  
 @DestinationETATo as datetime,  
  
 @FirstLoadPort as varchar(40),  
 @LastDischargePort as varchar(40),  
 @ReceivingAgent as uniqueidentifier,  
 @SendingAgent as uniqueidentifier,  
 @Carrier as uniqueidentifier,  
 @CoLoader as uniqueidentifier,  
  
 @LoadETDFrom as datetime,  
 @LoadETDTo as datetime,  
 @DischargeETAFrom as datetime,  
 @DischargeETATo as datetime,

 @currentCountry char(2)
)  
RETURNS TABLE   
AS  
RETURN  
(

SELECT  
	 FW.HouseBillNumber as HouseBillNumber,  
	 FW.ShipmentType AS ShipmentType,  
	 FW.TransportMode as FW_TransportMode,  
	 FW.OpMode as FW_ContainerMode,  
   
	 FW.ConsignRefs, 
  
	 ISNULL(FW.Shipments, 0) AS TotalAttached,  
  
	 FW.Origin as NKOrigUNLOCOCode,  
	 FW.ETD as OriginETD,  
	 FW.Destination as NKDestUNLOCOCode,  
	 FW.ETA as DestinationETA,  
  
	 FW.DEP_JK_UniqueConsignRef as DepartureConsolNum,  
	 FW.DEP_JK_AgentType as DepartureAgentType,  
	 FW.DEP_Carrier as DepartureShippingLineCode,  
	 FW.DEP_Creditor as DepartureCreditorCode,  
	 FW.DEP_ReceivingAgent as DepartureReceivingAgentCode,  
	 FW.DEP_SendingAgent as DepartureSendingAgentCode,  
	 FW.DEP_JK_TransportMode as DepartureTransportMode,  
	 FW.DEP_FirstLoad_Port as DepartureConsolFirstLoad,  
	 FW.DEP_FirstLoad_ETD as DepartureConsolETD,  
	 FW.DEP_FirstLoad_ATD as DepartureConsolATD,  
  
	 FW.ARV_JK_UniqueConsignRef as ArrivalConsolNum,  
	 FW.ARV_JK_AgentType as ArrivalAgentType,  
	 FW.ARV_Carrier as ArrivalShippingLineCode,  
	 FW.ARV_Creditor as ArrivalCreditorCode,  
	 FW.ARV_ReceivingAgent as ArrivalReceivingAgentCode,  
	 FW.ARV_SendingAgent as ArrivalSendingAgentCode,  
	 FW.ARV_JK_TransportMode as ArrivalTransportMode,  
	 FW.ARV_LastDischarge_Port as ArrivalConsolLastDischarge,  
	 FW.ARV_LastDischarge_ETA as ArrivalConsolETA,  
	 FW.ARV_LastDischarge_ATA as ArrivalConsolATA,  
  
	 FW.ConsigneeImporterCode,  
	 FW.ConsigneeImporterFullName,  
	 FW.ConsignorShipperSupplierCode,  
	 FW.ConsignorShipperSupplierFullName,  
  
	 FW.ActualWeight as ActualWeight,  
	 FW.UnitOfWeight as UnitOfWeight,  
	 FW.ActualVolume as ActualVolume,  
	 FW.UnitOfVolume as UnitOfVolume,  
	 FW.ActualChargeable as ActualChargeable,  
	 FW.ChargeableUnit as ChargeableUnit,  
  
	 FW.TwentyFootEquivalentUnit,   
	 FW.FortyFootEquivalentUnit,  
	 FW.FW_FCLContainerCount,   
	 FW.FW_FCLContainerTEU,   
   
	 RP.CCB as ControllingCustomerPK,  
	 RP.CCBOrgName as ControllingCustomerName,  
	 RP.CAG as ControllingCustomerAgentPK,  
	 RP.CAGOrgName as ControllingCustomerAgentName,  
	 MNG.MNGName as ManagementGroupName,  
	 MNG.RelatedMNGName as RelatedManagementGroupName,  
  
	 AmountsByJobParentID.JCD_ParentID,  
	 AmountsByJobParentID.ReportCurrencyCode,  
	 AmountsByJobParentID.JH_JobNum,  
	 AmountsByJobParentID.REVAmountInReportCurrency,  
	 AmountsByJobParentID.WIPAmountInReportCurrency,  
	 AmountsByJobParentID.REVAndWIPAmountInReportCurrency,  
	 AmountsByJobParentID.CSTAmountInReportCurrency,  
	 AmountsByJobParentID.ACRAmountInReportCurrency,  
	 AmountsByJobParentID.CSTAndACRAmountInReportCurrency,  
	 AmountsByJobParentID.ProfitInReportCurrency,  
	 AmountsByJobParentID.ProfitRevenueMargin,  
	 AmountsByJobParentID.ProfitCostMargin,  
	 AmountsByJobParentID.MissingExchangeRateDates  
  
FROM  
	(
		SELECT	JCD_ParentID,
				ReportCurrencyCode,
				JH.JH_JobNum,
				SUM(RCTotalREVAmountByJob) as REVAmountInReportCurrency,  
				SUM(RCTotalWIPAmountByJob) as WIPAmountInReportCurrency,  
				SUM(RCTotalIncomeByJob) as REVAndWIPAmountInReportCurrency,  
				SUM(RCTotalCSTAmountByJob) as CSTAmountInReportCurrency,  
				SUM(RCTotalACRAmountByJob) as ACRAmountInReportCurrency,  
				SUM(RCTotalExpenseByJob) as CSTAndACRAmountInReportCurrency,  
				SUM(RCTotalProfitAmountByJob) as ProfitInReportCurrency,  
				IIF(ISNULL(SUM(RCTotalREVAmountByJob), 1) <> 0, (ISNULL(SUM(RCTotalProfitAmountByJob), 0) / SUM(RCTotalREVAmountByJob)), 0) as ProfitRevenueMargin,  
				IIF(ISNULL(SUM(RCTotalCSTAmountByJob), 1) <> 0, (ISNULL(SUM(RCTotalProfitAmountByJob), 0) / SUM(RCTotalCSTAmountByJob)), 0) as ProfitCostMargin,  
				dbo.CLRCssvAgg(DISTINCT MissingExchangeRateDatesByJob) AS MissingExchangeRateDates  
		FROM  

			RptDt_GlobalJobProfitReportCore(@TransactionFrom, @TransactionTo, @ReportCurrency, @TransactionDebtorList, @TransactionDebtorNOTINList, @TransactionCreditorList, @TransactionCreditorNOTINList, @ChargeCodeList, @ChargeCodeNOTINList, @ChargeGroup, @SalesGroup, @ExpenseGroup, NULL) AS AmountsByJob
			INNER JOIN dbo.JobHeader JH ON JH.JH_PK = JCD_JH  
			LEFT JOIN dbo.GlbStaff JHOperator ON JH_GS_NKRepOps = JHOperator.GS_Code  
			LEFT JOIN dbo.GlbStaff JHSalesRep ON JH_GS_NKRepSales = JHSalesRep.GS_Code  
			LEFT JOIN dbo.OrgAddress AS LocalClientAddress ON JH_OA_LocalChargesAddr = LocalClientAddress.OA_PK  
			LEFT JOIN dbo.OrgHeader LocalClient ON LocalClientAddress.OA_OH = LocalClient.OH_PK  
			LEFT JOIN dbo.OrgAddress AS OverseasAgentAddress ON JH_OA_AgentCollectAddr = OverseasAgentAddress.OA_PK  
			LEFT JOIN dbo.OrgHeader OverseasAgent ON OverseasAgentAddress.OA_OH = OverseasAgent.OH_PK   

		WHERE

			(@SalesRepPK IS NULL OR JHSalesRep.GS_PK = @SalesRepPK)  
			AND (@OperatorPK IS NULL OR JHOperator.GS_PK = @OperatorPK)  
			AND (@LocalClientPK IS NULL OR LocalClient.OH_PK = @LocalClientPK)  
			AND (@OverseasAgentPK IS NULL OR OverseasAgent.OH_PK = @OverseasAgentPK)  
			AND (JH_SystemCreateTimeUtc BETWEEN @RegistrationDateFrom AND @RegistrationDateTo)  

		GROUP BY 
			 ReportCurrencyCode,
			 JCD_ParentID,
			 JH.JH_JobNum  
	) AmountsByJobParentID  

	INNER JOIN dbo.csfn_GetForwardingAndCustomsSummaryInfo(@Origin, @Destination, @OriginETDFrom, @OriginETDTo, @DestinationETAFrom, @DestinationETATo, @FirstLoadPort, @LastDischargePort, @ReceivingAgent, @SendingAgent, @Carrier, @CoLoader, @LoadETDFrom, @LoadETDTo, @DischargeETAFrom, @DischargeETATo, @currentCountry) FW ON FW.JH_ParentID = AmountsByJobParentID.JCD_ParentID 
	LEFT JOIN dbo.GetJobParentsByControllingCustomerAndAgent() RP ON FW.JH_ParentID = E2_ParentID  
	LEFT JOIN dbo.GetRootMNGNames(@ultimateMNGPKs, @MNGListIsEmpty) MNG ON MNG.OrgPK = RP.CCB 

WHERE
 
	((@MNGListIsEmpty = 0 AND MNG.OrgPK IS NOT NULL) OR @MNGListIsEmpty = 1)  
	AND (@ControllingCustomerListIsEmpty = 1 OR (@ControllingCustomerListIsEmpty = 0 AND RP.CCB IN (SELECT Value from @ControllingCustomerList)))
)