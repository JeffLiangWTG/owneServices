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
			AL_LineAmount AS AA_Amount,
			CASE
				WHEN T4_Currency = 'Y' THEN RX_Code
				ELSE ''
			END AS AA_TransactionCurrency,
			CASE
				WHEN T4_Currency = 'Y' THEN dbo.ClientConvertLocalToForeignAmount(AL_LineAmount, AL_ExchangeRate, @CompanyPK, AL_RX)			
				ELSE 0
			END AS AA_TransactionAmount,
			CASE
				WHEN T4_Geographical > 0 THEN T2_Geographical
				ELSE ''
			END AS AA_Geographical

	FROM	(
				SELECT	AL_LineAmount,						
						AG_PK,
						GB_Code,
						AL_GE,
						AH_OH,
						AL_RX,
						AL_ExchangeRate
				FROM 	dbo.AccTransactionHeader
						INNER JOIN dbo.AccTransactionLines ON AH_PK = AL_AH
						INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
						INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
						INNER JOIN dbo.AccGLHeader ON AC_AG_RevenueAccount = AG_PK						
				WHERE 	AH_Ledger = 'JC'
						AND AH_TransactionType = 'JNL'
						AND AL_LineType = 'REV'
						AND GB_GC = @CompanyPK		

				UNION

				SELECT	AL_LineAmount,						
						AG_PK,
						GB_Code,
						AL_GE,
						AH_OH,
						AL_RX,
						AL_ExchangeRate
				FROM 	dbo.AccTransactionHeader
						INNER JOIN dbo.AccTransactionLines ON AH_PK = AL_AH
						INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
						INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
						INNER JOIN dbo.AccGLHeader ON AC_AG_WIPAccount = AG_PK						
				WHERE 	AH_Ledger = 'JC'
						AND AH_TransactionType = 'JNL'
						AND AL_LineType = 'WIP'
						AND GB_GC = @CompanyPK		

				UNION

				SELECT	AL_LineAmount,
						AG_PK,
						GB_Code,
						AL_GE,
						AH_OH,
						AL_RX,
						AL_ExchangeRate						
				FROM 	dbo.AccTransactionHeader
						INNER JOIN dbo.AccTransactionLines ON AH_PK = AL_AH
						INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
						INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
						INNER JOIN dbo.AccGLHeader ON AC_AG_CostAccount = AG_PK						
				WHERE 	AH_Ledger = 'JC'
						AND AH_TransactionType = 'JNL'
						AND AL_LineType = 'CST'						
						AND GB_GC = @CompanyPK

				UNION

				SELECT	AL_LineAmount,
						AG_PK,
						GB_Code,
						AL_GE,
						AH_OH,
						AL_RX,
						AL_ExchangeRate						
				FROM 	dbo.AccTransactionHeader
						INNER JOIN dbo.AccTransactionLines ON AH_PK = AL_AH
						INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
						INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
						INNER JOIN dbo.AccGLHeader ON AC_AG_AccrualAccount = AG_PK						
				WHERE 	AH_Ledger = 'JC'
						AND AH_TransactionType = 'JNL'
						AND AL_LineType = 'ACR'
						AND GB_GC = @CompanyPK

				UNION

				SELECT	AL_LineAmount,						
						AG_PK,
						GB_Code,
						AL_GE,
						AH_OH,
						AL_RX,
						AL_ExchangeRate
				FROM 	dbo.AccTransactionHeader
						INNER JOIN dbo.AccTransactionLines ON AH_PK = AL_AH
						INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
						INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK						
				WHERE 	AH_Ledger = 'JC'
						AND AH_TransactionType = 'JNL'
						AND GB_GC = @CompanyPK
			) InnerDerivedTbl
			INNER JOIN dbo.RefCurrency ON AL_RX = RX_PK
			INNER JOIN #CognosAccounts ON AG_PK = T1_AG
			INNER JOIN #CounterCompanies ON AH_OH = T2_OH
			INNER JOIN #CognosModes ON AL_GE = T3_GE
			LEFT OUTER JOIN #Settings ON T1_Code = T4_Code
) DerivedTbl

GROUP BY	AA_AccountName, AA_AccountCode, AA_CounterCompany, AA_Branch, AA_Business, AA_TransactionCurrency, AA_Geographical