INSERT INTO dbo.RptDtJobCostingDataAmountByJob
	(JCA_JH, JCA_GC, JCA_OH_DebtorOrCreditor, JCA_PostPeriod, JCA_RX_NKLocalCurrency, JCA_Revenue, JCA_Cost, JCA_RowCount)
SELECT
	JCD_JH,
	JCD_GC,
	JCD_OH,
	JCD_PostPeriod,
	JCD_RX_NKLocalCurrency,
	Revenue = SUM(CASE WHEN JCD_LineType IN ('REV', 'WIP') THEN CONVERT(DECIMAL(28,4), JCD_LineAmount) ELSE 0 END),
	Cost = SUM(CASE WHEN JCD_LineType IN ('CST', 'ACR')  THEN CONVERT(DECIMAL(28,4), JCD_LineAmount) ELSE 0 END),
	RowCount_1 = COUNT_BIG(*)
FROM
	dbo.RptDtJobCostingData
GROUP BY
	JCD_JH,
	JCD_GC,
	JCD_OH,
	JCD_PostPeriod,
	JCD_RX_NKLocalCurrency
;
