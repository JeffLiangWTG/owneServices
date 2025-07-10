DECLARE @ExchangeDifferentControlGuid uniqueidentifier
DECLARE @CompanyPK uniqueidentifier

SELECT		AA_AccountName, AA_AccountCode, AA_CounterCompany, AA_Branch, AA_Business, SUM(AA_Amount) AS AA_Amount, AA_TransactionCurrency, SUM(AA_TransactionAmount) AS AA_TransactionAmount, AA_Geographical
FROM
(
	SELECT	T1_Name AS AA_AccountName,
			T1_Code AS AA_AccountCode,							
			CASE
				WHEN T4_Company = 'Y' THEN T2_CompanyCode
				ELSE ''
			END AS AA_CounterCompany,
			CASE
				WHEN T4_Mode > 0 THEN T3_Mode
				ELSE ''
			END AS AA_Mode,
			CASE
				WHEN T4_Branch > 0 THEN GB_Code
				ELSE ''
			END AS AA_Branch,
			CASE
				WHEN T4_BusinessType > 0 THEN T2_BusinessType
				ELSE ''
			END AS AA_Business,
			CASE
				WHEN AH_Ledger = 'CB' THEN -AH_InvoiceAmount
				ELSE AH_InvoiceAmount
			END AS AA_Amount,
			CASE
				WHEN T4_Currency = 'Y' THEN RX_Code
				ELSE ''
			END AS AA_TransactionCurrency,
			CASE
				WHEN T4_Currency = 'Y' AND AH_Ledger = 'CB' THEN -dbo.ClientConvertLocalToForeignAmount(AH_InvoiceAmount, AH_ExchangeRate, @CompanyPK, AH_RX)
				WHEN T4_Currency = 'Y' THEN dbo.ClientConvertLocalToForeignAmount(AH_InvoiceAmount, AH_ExchangeRate, @CompanyPK, AH_RX)
				ELSE 0
			END AS AA_TransactionAmount,
			CASE
				WHEN T4_Geographical > 0 THEN T2_Geographical
				ELSE ''
			END AS AA_Geographical

	FROM	dbo.AccTransactionHeader
			INNER JOIN dbo.GlbBranch ON AH_GB = GB_PK
			INNER JOIN dbo.RefCurrency ON AH_RX = RX_PK
			INNER JOIN #CognosAccounts ON @ExchangeDifferentControlGuid = T1_AG
			INNER JOIN #CounterCompanies ON AH_OH = T2_OH
			INNER JOIN #CognosModes ON AH_GE = T3_GE
			LEFT OUTER JOIN #Settings ON T1_Code = T4_Code

	WHERE	AH_TransactionType = 'EXX'
			AND AH_Ledger != 'JC'
) DerivedTbl
GROUP BY	AA_AccountName, AA_AccountCode, AA_CounterCompany, AA_Branch, AA_Business, AA_TransactionCurrency, AA_Geographical