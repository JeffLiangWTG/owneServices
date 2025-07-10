CREATE FUNCTION RptDt_Report_GlobalJobProfitSummaryByJob
(
	@ReportCurrency uniqueidentifier,--Never call these with null or empty values
	@TransactionFrom as datetime,--Never call these with null or empty values they are used in a between clause
	@TransactionTo as datetime,	--If you dont want to filter by them pass in a very small date for From and a really large one for to
	@chargeCodeList	as varchar(8000),
	@ChargeCodeNOTINList as varchar(8000),
	@ChargeGroup as varchar (4000),
	@SalesGroup as uniqueidentifier,
	@ExpenseGroup as uniqueidentifier,
	@TransactionDebtor as varchar(4000),
	@TransactionCreditor as varchar(4000),
	@ControllingCustomerList dbo.TVP_uniqueidentifier readonly,--controlling customer
	@ControllingCustomerListIsEmpty bit,
	@ControlingCustomerGroupList dbo.TVP_uniqueidentifier readonly,--controlling customer group
	@ControlingCustomerGroupListIsEmpty bit	
)
RETURNS TABLE AS 
RETURN

(
	SELECT  JH_JobNum,
			JH_JobLocalReference,
			JHBranch.GB_Code AS JH_BranchCode,
			JHBranch.GB_PK AS JH_BranchCodePK,
			JHDept.GE_Code AS JH_DepartmentCode,
			JHDept.GE_PK AS JH_DepartmentCodePK,
			JH_Status,
			JH_ProfitLossReasonCode,
			JHSalesRep.GS_PK AS JH_SalesRepPK,
			JHSalesRep.GS_Code AS JH_SalesRepInitials,
			JHOperator.GS_PK AS JH_OperatorPK,
			JHOperator.GS_Code AS JH_OperatorInitials,
			LocalClient.OH_Code AS JH_LocalClientCode,
			LocalClient.OH_FullName AS JH_LocalClientName,
			JH_SystemCreateTimeUtc AS JH_JobOpened,
			ISNULL(RecognitionDates.DateList, '') AS RecognitionDateList,
			JH_A_JCL AS JH_JobClosed,
			LocalClient.OH_PK AS JH_OH_LocalCharges,
			(SELECT OrgAddress FROM dbo.OrgAddressFormatter(dbo.GetAddressPksForOrg(LocalClient.OH_PK, 'OFC', 'N'))) AS LocalClientOFCAddress,
			CCB AS ControllingCustomerPK,
			CCBOrgName AS ControllingCustomerName,
			CAG AS ControllingCustomerAgentPK,
			CAGOrgName AS ControllingCustomerAgentName,
			MNGName AS ManagementGroupName,
			RelatedMNGName AS RelatedManagementGroupName,
			JCD_JH,
			GC_Code,
			GC_RX_NKLocalCurrency,
			LCTotalREVAmountByJob as REVAmountInLocalCurrency,
			LCTotalWIPAmountByJob as WIPAmountInLocalCurrency,
			LCTotalIncomeByJob as REVAndWIPAmountInLocalCurrency,
			LCTotalCSTAmountByJob as CSTAmountInLocalCurrency,
			LCTotalACRAmountByJob as ACRAmountInLocalCurrency,
			LCTotalExpenseByJob as CSTAndACRAmountInLocalCurrency,
			LCTotalProfitByJob as ProfitInLocalCurrency,
			RCTotalREVAmountByJob as REVAmountInReportCurrency,
			RCTotalWIPAmountByJob as WIPAmountInReportCurrency,
			RCTotalIncomeByJob as REVAndWIPAmountInReportCurrency,
			RCTotalCSTAmountByJob as CSTAmountInReportCurrency,
			RCTotalACRAmountByJob as ACRAmountInReportCurrency,
			RCTotalExpenseByJob as CSTAndACRAmountInReportCurrency,
			RCTotalProfitAmountByJob as ProfitInReportCurrency,
			MissingExchangeRateDatesByJob as MissingExchangeRateDates,
			ReportCurrencyCode
	FROM
			RptDt_GlobalJobProfitReportCore(@TransactionFrom, @TransactionTo, @ReportCurrency, @TransactionDebtor, NULL, @TransactionCreditor, NULL, @chargeCodeList, @ChargeCodeNOTINList, @ChargeGroup, @SalesGroup, @ExpenseGroup, NULL) AS AmountsByJob
			INNER JOIN dbo.JobHeader ON JCD_JH = JH_PK AND JH_IsActive = 1
			INNER JOIN dbo.GlbBranch JHBranch ON JH_GB = JHBranch.GB_PK 
			INNER JOIN dbo.GlbDepartment JHDept ON JH_GE = JHDept.GE_PK 
			LEFT JOIN dbo.JobRevRecognitionDates AS RecognitionDates ON RecognitionDates.JH = JobHeader.JH_PK
			LEFT JOIN dbo.GlbStaff JHOperator ON JH_GS_NKRepOps = JHOperator.GS_Code
			LEFT JOIN dbo.GlbStaff JHSalesRep ON JH_GS_NKRepSales = JHSalesRep.GS_Code
			LEFT JOIN dbo.OrgAddress AS LocalClientAddress ON JH_OA_LocalChargesAddr = LocalClientAddress.OA_PK
			LEFT JOIN dbo.OrgHeader LocalClient ON LocalClientAddress.OA_OH = LocalClient.OH_PK
			LEFT JOIN dbo.GetJobParentsByControllingCustomerAndAgent() RP ON JH_ParentID = E2_ParentID
			LEFT JOIN dbo.GetRootMNGNames(@ControlingCustomerGroupList, @ControlingCustomerGroupListIsEmpty) MNG ON MNG.OrgPK = RP.CCB
	WHERE	(@ControllingCustomerListIsEmpty = 1 OR (@ControllingCustomerListIsEmpty = 0 AND RP.CCB IN (SELECT VALUE FROM @ControllingCustomerList)))
			AND (@ControlingCustomerGroupListIsEmpty = 1 OR (@ControlingCustomerGroupListIsEmpty = 0 AND MNG.OrgPK IS NOT NULL))
)