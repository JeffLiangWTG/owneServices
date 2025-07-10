CREATE FUNCTION RptDt_GlobalJobProfitReportCore  
(   
	 @TransactionFrom as datetime,  
	 @TransactionTo as datetime,  
	 @ReportCurrency uniqueidentifier,   
  
	 @TransactionDebtorList as varchar(8000),   
	 @TransactionDebtorNOTINList as varchar(8000),   
	 @TransactionCreditorList as varchar(8000), -- Do not use both Creditor and Debtor as they are mutually exclusive  
	 @TransactionCreditorNOTINList as varchar(8000),   
   
	 @ChargeCodeList as varchar(8000),   
	 @ChargeCodeNOTINList as varchar(8000),  
	 @ChargeGroup as varchar (4000),  
	 @SalesGroup as uniqueidentifier,  
	 @ExpenseGroup as uniqueidentifier,

	 @CompanyPK AS uniqueidentifier	
)  
RETURNS TABLE   
AS  
RETURN  
(  
  
	SELECT
		JCD_JH,
		GC_PK,
		GC_Code,
		JCD_ParentID, 
		GC_RX_NKLocalCurrency,
		ReportCurrencyCode,		
		SUM(REVAmount) AS LCTotalREVAmountByJob,
		SUM(WIPAmount) AS LCTotalWIPAmountByJob,
		SUM(REVAmount + WIPAmount) AS LCTotalIncomeByJob,
		SUM(CSTAmount) AS LCTotalCSTAmountByJob,
		SUM(ACRAmount)AS LCTotalACRAmountByJob,
		SUM(CSTAmount + ACRAmount) AS LCTotalExpenseByJob,
		SUM(Profit) AS LCTotalProfitByJob,		
		SUM(RCREVAmount) as RCTotalREVAmountByJob,  
		SUM(RCWIPAmount) as RCTotalWIPAmountByJob,  
		SUM(RCREVAmount + RCWIPAmount) AS RCTotalIncomeByJob,  
		SUM(RCCSTAmount) as RCTotalCSTAmountByJob,  
		SUM(RCACRAmount) as RCTotalACRAmountByJob,  
		SUM(RCCSTAmount + RCACRAmount) AS RCTotalExpenseByJob,  
		SUM(RCProfit) as RCTotalProfitAmountByJob,  
		IIF(ISNULL(dbo.CLRCssvAgg(DISTINCT MissingExchangeRateDate), '') <> '', '(' + GC_RX_NKLocalCurrency + '>' + ReportCurrencyCode + ' ' + dbo.CLRCssvAgg(DISTINCT MissingExchangeRateDate) + ')', '') AS MissingExchangeRateDatesByJob

	FROM
	(
		SELECT	
			JCD_JH, 
			GC_PK,
			GC_Code,
			JCD_ParentID,  
			GC_RX_NKLocalCurrency,
			REVAmount,
			WIPAmount,
			CSTAmount,
			ACRAmount,
			Profit,
			ReportCurrencyCode,  
			(SELECT OsAmount FROM dbo.GetOSAmountFromLocalAmount(REVAmount, GC_IsReciprocal, ReportCurrencyExchangeRate, NULL, RCSubUnitRatio, LCSubUnitRatio)) AS RCREVAmount,  
			(SELECT OsAmount FROM dbo.GetOSAmountFromLocalAmount(WIPAmount, GC_IsReciprocal, ReportCurrencyExchangeRate, NULL, RCSubUnitRatio, LCSubUnitRatio)) AS RCWIPAmount,  
			(SELECT OsAmount FROM dbo.GetOSAmountFromLocalAmount(CSTAmount, GC_IsReciprocal, ReportCurrencyExchangeRate, NULL, RCSubUnitRatio, LCSubUnitRatio)) AS RCCSTAmount,  
			(SELECT OsAmount FROM dbo.GetOSAmountFromLocalAmount(ACRAmount, GC_IsReciprocal, ReportCurrencyExchangeRate, NULL, RCSubUnitRatio, LCSubUnitRatio)) AS RCACRAmount,  
			(SELECT OsAmount FROM dbo.GetOSAmountFromLocalAmount(Profit, GC_IsReciprocal, ReportCurrencyExchangeRate, NULL, RCSubUnitRatio, LCSubUnitRatio)) AS RCProfit,  
			ReportCurrencyExchangeRate,  
			MissingExchangeRateDate  
		FROM
			(  
				SELECT
						JCD_JH,
						JCD_ParentID,
						JCD_GC AS GC_PK,
						ALCompany.GC_Code AS GC_Code,
						ALCompany.GC_RX_NKLocalCurrency AS GC_RX_NKLocalCurrency,
						ALCompany.GC_IsReciprocal AS GC_IsReciprocal,
						(CASE WHEN JCD_LineType = 'REV' THEN JCD_LineAmount ELSE 0 END) AS REVAmount,
						(CASE WHEN JCD_LineType = 'WIP' THEN JCD_LineAmount ELSE 0 END) AS WIPAmount,
						(CASE WHEN JCD_LineType = 'CST' THEN JCD_LineAmount ELSE 0 END) AS CSTAmount,
						(CASE WHEN JCD_LineType = 'ACR' THEN JCD_LineAmount ELSE 0 END) AS ACRAmount,
						JCD_LineAmount AS Profit,
						RC.RX_Code as ReportCurrencyCode,
						RC.RX_SubUnitRatio as RCSubUnitRatio,
						TLC.RX_SubUnitRatio as LCSubUnitRatio,
						(CASE WHEN JCD_RX_NKLocalCurrency = RC.RX_Code THEN 1 ELSE PeriodEndExchangeRate.ExchangeRate END) as ReportCurrencyExchangeRate,
						(CASE WHEN JCD_RX_NKLocalCurrency = RC.RX_Code THEN NULL ELSE PeriodEndExchangeRate.MissingExchangeRateDate END) as MissingExchangeRateDate
  
				FROM
						dbo.RptDtJobCostingData  
						INNER JOIN dbo.RptDt_GetPeriodKeys(@TransactionFrom , @Transactionto, @CompanyPK) t ON t.PeriodKey = jcd_periodcompanykey  
						INNER JOIN dbo.AccChargeCode ACOutter ON JCD_AC = AC_PK  
						INNER JOIN dbo.GlbCompany ALCompany ON JCD_GC = ALCompany.GC_PK   
						INNER JOIN dbo.RefCurrency TLC ON TLC.RX_Code = GC_RX_NKLocalCurrency  
						INNER JOIN dbo.RefCurrency RC ON RC.RX_PK = @ReportCurrency  
						LEFT JOIN dbo.OrgHeader CreditorOrDebtor ON CreditorOrDebtor.OH_PK = JCD_OH
						CROSS APPLY dbo.PeriodEndExchangeRate(RC.RX_Code, JCD_PostPeriod, JCD_GC)
  
				WHERE
						(ISNULL(@chargeCodeList, '') = '' OR AC_PK IN (select value from dbo.SplitStringToGuidTable(@chargeCodeList, ',', DEFAULT)))  
						AND (ISNULL(@ChargeCodeNOTINList, '') = '' OR AC_PK NOT IN (select value from dbo.SplitStringToGuidTable(@ChargeCodeNOTINList, ',', DEFAULT)))  
						AND (ISNULL(@ChargeGroup, '') = '' OR AC_ChargeGroup IN (select value from dbo.SplitStringToTable(@ChargeGroup, DEFAULT)))  
						AND (@SalesGroup IS NULL OR AC_AR_SalesGroup = @SalesGroup)  
						AND (@ExpenseGroup IS NULL OR AC_AR_ExpenseGroup = @ExpenseGroup)  
						AND (ISNULL(@TransactionDebtorList, '') = '' OR (JCD_LineType IN ('REV', 'WIP') AND CreditorOrDebtor.OH_PK IN (select value from dbo.SplitStringToGuidTable(@TransactionDebtorList, ',', DEFAULT))))  
						AND (ISNULL(@TransactionDebtorNOTINList, '') = '' OR ((JCD_LineType IN ('REV', 'WIP') AND CreditorOrDebtor.OH_PK NOT IN (select value from dbo.SplitStringToGuidTable(@TransactionDebtorNOTINList ,',', DEFAULT)))))  
						AND (ISNULL(@TransactionCreditorList, '') = '' OR (JCD_LineType IN ('CST', 'ACR') AND CreditorOrDebtor.OH_PK IN (select value from dbo.SplitStringToGuidTable(@TransactionCreditorList ,',', DEFAULT))))       
						AND (ISNULL(@TransactionCreditorNOTINList, '') = '' OR ((JCD_LineType IN ('CST', 'ACR') AND CreditorOrDebtor.OH_PK NOT IN (select value from dbo.SplitStringToGuidTable(@TransactionCreditorNOTINList ,',', DEFAULT)))))  
						AND JCD_PostDate BETWEEN IIF(@TransactionFrom < t.StartDate, t.StartDate, @TransactionFrom) AND IIF(@TransactionTo > t.EndDate, t.EndDate, @TransactionTo)  
  
			) AS AllTransactions  
		) AS TransactionAmountInReportCurrency  

	GROUP BY
		ReportCurrencyCode,
		GC_PK,
		GC_Code,
		GC_RX_NKLocalCurrency,
		JCD_ParentID,  
		JCD_JH
)