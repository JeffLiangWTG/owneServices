CREATE VIEW AccTransactionHeaderWithJobInfo WITH SCHEMABINDING AS
	SELECT NEWID() AS AH_PK, AH_OH, AH_InvoiceAmount, AH_TransactionType, AH_TransactionNum, AH_RX, AH_InvoiceDate, AH_DueDate, AH_Ledger, AH_Desc, AH_FullyPaidDate, JK_PK AS AH_JK, JS_PK AS AH_JS
	FROM dbo.AccTransactionHeader 
		INNER JOIN dbo.JobConsol ON AH_ConsolidatedInvoiceRef = JK_UniqueConsignRef OR 
				AH_ConsolidatedInvoiceRef LIKE JK_UniqueConsignRef + '/%' 
		INNER JOIN dbo.JobShipment ON JS_PK = (SELECT TOP 1 JN_JS FROM dbo.JobConShipLink WHERE JN_JK = JK_PK) 
	WHERE AH_JH IS NULL
	UNION
	SELECT NEWID() AS AH_PK, AH_OH, AH_InvoiceAmount, AH_TransactionType, AH_TransactionNum, AH_RX, AH_InvoiceDate, AH_DueDate, AH_Ledger, AH_Desc, AH_FullyPaidDate, JK_PK, JS_PK
	FROM dbo.AccTransactionHeader 
		INNER JOIN dbo.JobHeader ON AH_JH = JH_PK AND JH_ParentTableCode = 'JS' 
		INNER JOIN dbo.JobShipment ON JH_ParentID = JS_PK 
		INNER JOIN dbo.JobConsol ON JK_PK IN (SELECT JN_JK FROM dbo.JobConShipLink WHERE JN_JS = JS_PK) 
	WHERE AH_JH IS NOT NULL
	UNION
	SELECT NEWID() AS AH_PK, AH_OH, AH_InvoiceAmount, AH_TransactionType, AH_TransactionNum, AH_RX, AH_InvoiceDate, AH_DueDate, AH_Ledger, AH_Desc, AH_FullyPaidDate, NULL, NULL
	FROM dbo.AccTransactionHeader 
	WHERE AH_JH IS NULL 
	AND AH_ConsolidatedInvoiceRef IS NULL
GO

CREATE VIEW AccTransactionHeaderWithJobInfo WITH SCHEMABINDING AS
					SELECT 
						NEWID() AS AH_PK, 
						AH_OH,						
						AH_TransactionType, 
						AH_TransactionNum, 
						AH_RX, 
						AH_InvoiceDate, 
						AH_DueDate, 
						AH_Ledger,
						AH_Desc,				
						AH_InvoiceAmount,
						AH_GSTAmount,						
						AH_OSTotal,
						AH_OutstandingAmount,
						AH_OSOutstandingAmount,
						AH_IsOSOutstandingAmountApplicable,
						AH_ExchangeRate,
						JK_PK AS AH_JK, 
						JS_PK AS AH_JS						

					FROM 
						dbo.AccTransactionHeader 
						INNER JOIN dbo.JobConsol ON AH_ConsolidatedInvoiceRef = JK_UniqueConsignRef OR AH_ConsolidatedInvoiceRef LIKE JK_UniqueConsignRef + '/%' 
						INNER JOIN dbo.JobShipment ON JS_PK =	(SELECT TOP 1 JN_JS 
																FROM 
																	dbo.JobConShipLink 
																	INNER JOIN dbo.JobShipment ON JN_JS = JS_PK 
																WHERE JN_JK = JK_PK
																ORDER BY JS_UniqueConsignRef)

					WHERE AH_JH IS NULL

					UNION

					SELECT
						NEWID() AS AH_PK, 
						AH_OH,						
						AH_TransactionType, 
						AH_TransactionNum, 
						AH_RX, 
						AH_InvoiceDate, 
						AH_DueDate, 
						AH_Ledger,
						AH_Desc,				
						AH_InvoiceAmount,
						AH_GSTAmount,						
						AH_OSTotal,
						AH_OutstandingAmount,
						AH_OSOutstandingAmount,
						AH_IsOSOutstandingAmountApplicable,
						AH_ExchangeRate,
						JK_PK AS AH_JK, 
						JS_PK AS AH_JS		

					FROM 
						dbo.AccTransactionHeader 
						INNER JOIN dbo.JobHeader ON AH_JH = JH_PK AND JH_ParentTableCode = 'JS' 
						INNER JOIN dbo.JobShipment ON JH_ParentID = JS_PK 
						INNER JOIN dbo.JobConsol ON JK_PK IN	(SELECT JN_JK 
																FROM dbo.JobConShipLink 
																WHERE JN_JS = JS_PK)

					WHERE AH_JH IS NOT NULL

					UNION

					SELECT 
						NEWID() AS AH_PK, 
						AH_OH,						
						AH_TransactionType, 
						AH_TransactionNum, 
						AH_RX, 
						AH_InvoiceDate, 
						AH_DueDate, 
						AH_Ledger,
						AH_Desc,				
						AH_InvoiceAmount,
						AH_GSTAmount,						
						AH_OSTotal,
						AH_OutstandingAmount,
						AH_OSOutstandingAmount,
						AH_IsOSOutstandingAmountApplicable,
						AH_ExchangeRate						
					
					FROM dbo.AccTransactionHeader 
					WHERE AH_JH IS NULL 
					AND (AH_ConsolidatedInvoiceRef IS NULL
					OR AH_ConsolidatedInvoiceRef = '')