using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport.FEC
{
	public class FECDataProvider
	{
		public FECDataProvider(AccComplianceReport complianceReport)
		{
			ComplianceReport = Argument.NotNull(complianceReport, nameof(complianceReport));
		}

		const int CommandTimeout = 1800;  // half an hour
		readonly AccComplianceReport ComplianceReport;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "No BusinessObject for this data")]
		public DbCommand OpenTransactionsDbCommand(ZDate startDate, ZDate endDatePlusOne, DbConnection connection)
		{
			// 1st SELECT in UNION: all transactions having a header but maybe no lines (e.g. AR/REC, AP/PAY, ...)
			// 2nd SELECT in UNION: all transactions having header and lines
			// 3rd SELECT in UNION: all transaction lines linked to a job header (e.g. WIPs and ACRs) but not to a transaction header
			const string transactionsSQL = @"DECLARE @ReportType CHAR(3) = 'FEC'

	DECLARE @reportqueue_header TABLE
	(
		[ACQ_ParentID] [uniqueidentifier] ,
		[ACQ_ReportSubCode] [varchar](20) ,
		[ACQ_Date] [date],
		ag_link char(1),
		INDEX header_order CLUSTERED (ACQ_ParentID)
	) 

	DECLARE @reportqueue_line TABLE
	(
		[ACQ_ParentID] [uniqueidentifier] NOT NULL,
		[ACQ_ReportSubCode] [varchar](20) NOT NULL,
		[ACQ_Date] [date] NOT NULL,
		INDEX line_order CLUSTERED (ACQ_ParentID)
	) 

	INSERT INTO @reportqueue_header
		SELECT ACQ_ParentID,ACQ_ReportSubCode,ACQ_Date,iif(ACQ_ReportSubCode like '%bank%','B','H')
		FROM dbo.AccTransactionComplianceReportQueue
		WHERE ACQ_GC_Company = @CompanyPK
		AND ACQ_ReportType = @ReportType
		AND ACQ_ParentTableCode = 'AH'
		AND ACQ_Date >= @StartDate AND ACQ_Date < @EndDate

	INSERT INTO @reportqueue_line
		SELECT ACQ_ParentID,ACQ_ReportSubCode,ACQ_Date
		FROM dbo.AccTransactionComplianceReportQueue
		WHERE ACQ_GC_Company = @CompanyPK
		AND ACQ_ReportType = @ReportType
		AND ACQ_ParentTableCode = 'AL'
		AND ACQ_Date >= @StartDate AND ACQ_Date < @EndDate

	--header data
	SELECT AH_Ledger,
			AH_TransactionType,
			ACQ_Date AS EcritureDate,
			OH_Code AS CompAuxNum,
			OH_FullName AS CompAuxLib,
			AH_InvoiceAmount AS NetAmountLocal,
			ACQ_Date AS ValidDate,
			AH_GSTAmount AS TaxAmountLocal,
			ACQ_ReportSubCode,
			AH_TransactionType AS LineType,
			NULL AS VATBasis,						-- this is how we identify the header
			NULL AS InputVatRecoverable,			-- this is how we identify the header
			AG_PK AS AccountPK,
			AG_AccountNum AS AccountNum,
			AH_InvoiceDate AS PieceDate,
			AH_TransactionNum AS PieceRef,
			AH_Desc AS EcritureLib,
			AH_OSTotal AS GrossAmountForeign,
			AH_RX_NKTransactionCurrency AS IDevise,
			AH_ExchangeRate AS ExchangeRate,
			CAST(0 AS smallint) AS AL_Sequence,
			NULL AS AL_PK,
			AH_ConsolidatedInvoiceRef AS InternalReference,
			IIF(AH_Ledger IN ('AR', 'AP') AND AH_TransactionType <> 'JNL' AND AH_InvoiceAmount <> 0 AND ACQ_ReportSubCode LIKE '%Ctrl%', (SELECT string_agg(AP_MatchGroupNum, ',') FROM AccTransactionMatchLink WHERE AP_AH = AH_PK), NULL) AS EcritureLet,
			IIF(AH_Ledger IN ('AR', 'AP') AND AH_TransactionType <> 'JNL' AND AH_InvoiceAmount <> 0 AND ACQ_ReportSubCode LIKE '%Ctrl%', AH_FullyPaidDate, NULL) AS DateLet
		FROM @reportqueue_header
			INNER JOIN dbo.AccTransactionHeader Header ON AH_PK = ACQ_ParentID
			LEFT OUTER JOIN dbo.OrgHeader ON AH_OH = OH_PK
			OUTER APPLY (SELECT AG_PK,AG_AccountNum FROM dbo.AccGLHeader, AccBankAccount WHERE AG_PK = AB_AG AND AB_PK = AH_AB AND (AH_AG IS NULL OR ag_link = 'B')
							UNION ALL
							SELECT AG_PK, AG_AccountNum FROM dbo.AccGLHeader WHERE AG_PK = AH_AG AND ag_link = 'H') ag

	UNION ALL

	--line with header link
	SELECT AH_Ledger,
			AH_TransactionType,
			ACQ_Date AS EcritureDate,
			OH_Code AS CompAuxNum,
			OH_FullName AS CompAuxLib,
			AL_LineAmount AS NetAmountLocal,
			ACQ_Date AS ValidDate,
			AL_GSTVAT AS TaxAmountLocal,
			ACQ_ReportSubCode,
			AL_LineType AS LineType,
			AL_GSTVATBasis AS VATBasis,
			AL_InputGSTVATRecoverable AS InputVatRecoverable,
			AG_PK AS AccountPK,
			AG_AccountNum AS AccountNum,
			IsNull(AH_InvoiceDate, AL_PostDate) AS PieceDate,
			AH_TransactionNum AS PieceRef,
			IsNull(AH_Desc, AL_Desc) AS EcritureLib,
			AL_OSAmount AS GrossAmountForeign,
			AL_RX_NKTransactionCurrency AS IDevise,
			AL_ExchangeRate AS ExchangeRate,
			AL_Sequence,
			AL_PK,
			AH_ConsolidatedInvoiceRef AS InternalReference,
			NULL AS EcritureLet,
			NULL AS DateLet
		FROM @reportqueue_line 
			INNER JOIN dbo.AccTransactionLines Lines WITH(FORCESEEK, INDEX(PK_UX__AL_PK)) ON AL_PK = ACQ_ParentID		
			INNER JOIN dbo.AccTransactionHeader Header ON AH_PK = AL_AH 
			LEFT OUTER JOIN dbo.OrgHeader ON IsNull(AH_OH, AL_OH) = OH_PK
			LEFT OUTER JOIN dbo.AccChargeCode ChargeCode ON AL_AC = AC_PK
			LEFT OUTER JOIN dbo.AccGLHeader Account ON AL_AG = Account.AG_PK
		WHERE IsNull(AC_ChargeType, '') <> 'CMT'
		
	UNION ALL

	--line with job link
	SELECT 'JC' AS AH_Ledger,
			AL_LineType AS AH_TransactionType,
			ACQ_Date AS EcritureDate,
			OH_Code AS CompAuxNum,
			OH_FullName AS CompAuxLib,
			AL_LineAmount AS NetAmountLocal,
			ACQ_Date AS ValidDate,
			AL_GSTVAT AS TaxAmountLocal,
			ACQ_ReportSubCode,
			AL_LineType AS LineType,
			AL_GSTVATBasis AS VATBasis,
			AL_InputGSTVATRecoverable AS InputVatRecoverable,
			AG_PK AS AccountPK,
			AG_AccountNum AS AccountNum,
			AL_PostDate AS PieceDate,
			JH_JobNum AS PieceRef,
			AL_Desc AS EcritureLib,
			AL_OSAmount AS GrossAmountForeign,
			AL_RX_NKTransactionCurrency AS IDevise,
			AL_ExchangeRate AS ExchangeRate,
			AL_Sequence,
			AL_PK,
			NULL AS InternalReference,
			NULL AS EcritureLet,
			NULL AS DateLet
		FROM @reportqueue_line
			INNER JOIN dbo.AccTransactionLines Lines WITH(FORCESEEK, INDEX(PK_UX__AL_PK)) ON AL_PK = ACQ_ParentID		
			INNER JOIN dbo.JobHeader Header ON JH_PK = AL_JH
			LEFT OUTER JOIN dbo.OrgHeader ON AL_OH = OH_PK
			LEFT OUTER JOIN dbo.AccChargeCode ChargeCode ON AL_AC = AC_PK
			LEFT OUTER JOIN dbo.AccGLHeader Account ON AL_AG = Account.AG_PK
		WHERE IsNull(AC_ChargeType, '') <> 'CMT'
		AND AL_AH IS NULL

	ORDER BY EcritureDate, AH_Ledger, AH_TransactionType, InternalReference, PieceRef, LineType, AL_Sequence, AL_PK, ACQ_ReportSubCode";
			// This ORDER BY is useful because WIP and ACR transaction lines can have the same JH_JobNum but all have AL_Sequence = 1. 
			// Without AL_PK in the ORDER BY the result is ordered by ACQ_ReportSubCode and not be AL_Sequence because this is always = 1.
			// With AL_PK in the ORDER BY the result is ordered by AL_PK and then by ACQ_ReportSubCode with the result that the postings on different accounts for one
			// transaction line are followed one after the other. First all postings for one transation line, afterwards the postings for the next transaction line.

			var command = connection.Command(transactionsSQL, CommandTimeout);
			command.AddParameterBasedOnDbColumn("@CompanyPK", ComplianceReport.ACR_GC_Company.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_GC_Company);
			command.AddParameterBasedOnDbColumn("@StartDate", startDate.ToDateTime(), AccTransactionComplianceReportQueueSchema.ACQ_Date);
			command.AddParameterBasedOnDbColumn("@EndDate", endDatePlusOne.ToDateTime(), AccTransactionComplianceReportQueueSchema.ACQ_Date);

			return command;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "No BusinessObject for this data")]
		internal List<(ZGuid AccountPK, ZDecimal Balance)> RetrieveOpenBalances()
		{
			var result = new List<(ZGuid, ZDecimal)>();
			var retainedEarningsPK = new Guid(AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value.ToString());

			// @StartPeriod relates to the ReportStartDate
			// First select returns the opening balances of all BSH accounts 
			// Second select sums up all amounts for P&L accounts as opening balance for Retained Earnings account
			// -> excluding those having an open balance of 0.00
			const string openingBalanceSQL = @"SELECT * FROM
											   (SELECT AA_AG,
													SUM(AA_Amount) AS OpeningBalance,
													AG_AccountNum
													FROM dbo.AccGLAggregate INNER JOIN dbo.AccGLHeader ON AA_AG = AG_PK
													WHERE AA_GC = @Company
													AND AA_Period < @StartPeriod
													AND AG_AccountType <> 'P&L'
													GROUP BY AA_AG, AG_AccountNum

												UNION ALL

													SELECT @RetainedEarningsPK,
													SUM(AA_Amount) AS OpeningBalance,
													(SELECT AG_AccountNum FROM dbo.AccGLHeader WHERE AG_PK = @RetainedEarningsPK)
													FROM dbo.AccGLAggregate INNER JOIN dbo.AccGLHeader ON AA_AG = AG_PK
													WHERE AA_GC = @Company
													AND AA_Period < @StartPeriod
													AND AA_TransactionCategory = ''
													AND AG_AccountType = 'P&L'
												) AS innerTable
												WHERE OpeningBalance <> 0
												ORDER BY AG_AccountNum";

			// retrieve the opening balances per GL account
			var periodCalculator = new AccountingPeriodCalculator(ComplianceReport.Factory);
			var reportStartPeriod = (int)periodCalculator.GetPeriodFromDate(ComplianceReport.ACR_DateFrom.ToDateTime(), ComplianceReport.ACR_GC_Company);

			var command = ((IDbConnected)ComplianceReport.Factory).Connection.Command(openingBalanceSQL, CommandTimeout);
			command.AddParameterBasedOnDbColumn("@Company", ComplianceReport.ACR_GC_Company.ToGuid(), AccGLAggregateSchema.AA_GC);
			command.AddParameterBasedOnDbColumn("@StartPeriod", reportStartPeriod, AccGLAggregateSchema.AA_Period);
			command.AddParameterBasedOnDbColumn("@RetainedEarningsPK", retainedEarningsPK, AccGLHeaderSchema.PK);

			using (command)
			using (var openingBalancesDataReader = command.ExecuteReader())
			{
				while (openingBalancesDataReader.Read())
				{
					var accountPK = new ZGuid(openingBalancesDataReader["AA_AG"]);
					result.Add((accountPK, (decimal)openingBalancesDataReader["OpeningBalance"]));
				}
			}

			return result;
		}
	}
}
