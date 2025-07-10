using System;
using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client
{
	public class ClientOverride : ClientHook
	{
		protected ClientOverride()
		{
		}

		public static ClientOverride Instance
		{
			get { return instance ?? (instance = new ClientOverride()); }
		}
		[ThreadStatic]
		static ClientOverride instance;

		#region IClientHook Members

		public override Clients Client
		{
			get { return Clients.SIP; }
		}

		public override string ClientDisplayName
		{
			get { return "Stockwell International"; }
		}

		public override IExtensionObjects DbSchemaExtensionObjects => new ExtensionObjects(TableCreationScripts, ViewAndRoutinesCreationScripts);

		#endregion

		#region Reports SQL scripts

		static readonly ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts = ImmutableArray<DatabaseObjectCreateScript>.Empty;

		static readonly ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts = ImmutableArray.Create(
			new DatabaseViewAndRoutineCreateScript("ClientProfitAndLossReportStockWell", @"
CREATE PROCEDURE ClientProfitAndLossReportStockWell @Period INT, @CompanyPK UNIQUEIDENTIFIER, @DepartmentList AS VARCHAR(256),@BranchList AS VARCHAR(256)
, @InclZeroBal CHAR(1), @SummaryType CHAR(20) as

DECLARE @PeriodYearBegin as int, @PeriodYearEnd as int, @PeriodYear as int
DECLARE @PeriodLastYearBegin as int, @PeriodLastYearEnd as int, @PeriodLastYear as int, @LastYear as int
DECLARE @BranchCode as char(3), @BranchName as varchar(50), @DepartmentCode as char(3), @TTL as int
DECLARE @AccountNumber as VARCHAR(10), @LastTTLAccNumber as VARCHAR(10), @AccountPK UNIQUEIDENTIFIER
DECLARE @GrossProfitTotalAccount as VARCHAR(10), @NetProfitTotalAccount as VARCHAR(10), @OverHeadTotalAccount as VARCHAR(10)
DECLARE @BSHStartAccount as VARCHAR(10)

--------------------------------------------------------------------------------------------
-- Init Variables
--------------------------------------------------------------------------------------------
SELECT @BSHStartAccount = (SELECT AG_AccountNum FROM dbo.AccGlHeader
	WHERE AG_PK = (SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
	FROM dbo.stmdata WHERE sd_name = 'GL_BS_ACCOUNT_START'))

SELECT @GrossProfitTotalAccount = (SELECT AG_AccountNum FROM dbo.AccGlHeader
	WHERE AG_PK = (SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
	FROM dbo.stmdata WHERE sd_name = 'GL_GROSS_PROFIT_TOTAL_ACCOUNT'))

SELECT @NetProfitTotalAccount = (SELECT AG_AccountNum FROM dbo.AccGlHeader
	WHERE AG_PK = (SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
	FROM dbo.stmdata WHERE sd_name = 'GL_NET_PROFIT_TOTAL_ACCOUNT'))

SELECT @OverHeadTotalAccount = (SELECT AG_AccountNum FROM dbo.AccGlHeader
	WHERE AG_PK = (SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
	FROM dbo.stmdata WHERE sd_name = 'GL_OVERHEAD_TOTAL_ACCOUNT'))

SELECT @PeriodYear = CAST(LEFT(CAST(@Period AS CHAR(6)), 4) AS INT)
SELECT @PeriodLastYear = @Period - 100

SELECT @LastYear = @PeriodYear - 1
SELECT TOP 1 @PeriodYearBegin = AM_Period FROM dbo.AccPeriodManagement WHERE AM_Year = @PeriodYear ORDER BY AM_Period
SELECT TOP 1 @PeriodYearEnd = AM_Period FROM dbo.AccPeriodManagement WHERE AM_Year = @PeriodYear ORDER BY AM_Period DESC
SELECT TOP 1 @PeriodLastYearBegin = AM_Period FROM dbo.AccPeriodManagement WHERE AM_Year = @LastYear ORDER BY AM_Period
SELECT TOP 1 @PeriodLastYearEnd = AM_Period FROM dbo.AccPeriodManagement WHERE AM_Year = @LastYear ORDER BY AM_Period DESC

--------------------------------------------------------------------------------------------
-- Temp Tables
--------------------------------------------------------------------------------------------

CREATE TABLE #PL
(
	AccountPK UNIQUEIDENTIFIER,
	AccountNumber VARCHAR(10) COLLATE database_default,
	AccountName VARCHAR(35) COLLATE database_default,
	AccountType CHAR(3) COLLATE database_default,
	DebitCredit CHAR(2) COLLATE database_default,
	CurrentPeriod MONEY,
	YearToPeriod MONEY,
	PeriodLastYear MONEY,
	LastYearToPeriod MONEY,
	TTL INT,
	PercentageAccount VARCHAR(10) COLLATE database_default,
	ConsolidateAccount VARCHAR(10) COLLATE database_default,
	AlternateAccount VARCHAR(10) COLLATE database_default,
	BranchPK UNIQUEIDENTIFIER,
	BranchCode CHAR(3) COLLATE database_default,
	BranchName VARCHAR(50) COLLATE database_default,
	DepartmentPK UNIQUEIDENTIFIER,
	DepartmentCode CHAR(3) COLLATE database_default,
	DepartmentName VARCHAR(35) COLLATE database_default,
	PercentageAccountPK UNIQUEIDENTIFIER,
	ConsolidateAccountPK UNIQUEIDENTIFIER,
	AlternateAccountPK UNIQUEIDENTIFIER,
)

--------------------------------------------------------------------------------------------
-- ADD GL Accounts to #PL
--------------------------------------------------------------------------------------------

INSERT INTO #PL (AccountPK, AccountNumber, AccountName, AccountType, DebitCredit, TTL,
	PercentageAccountPK, ConsolidateAccountPK, AlternateAccountPK)
	SELECT AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_TotalLevel,
	AG_AG_PercentNum, AG_AG_ConsolidationNum, AG_AG_AlternateNum
	FROM dbo.AccGLHeader WHERE AG_AccountNum < @BSHStartAccount AND AG_AccountType NOT IN ('BSH', 'ALT')
	ORDER BY AG_AccountNum

--------------------------------------------------------------------------------------------
-- UPDATE #PL PercentageAccount, ConsolidateAccount, AlternateAccount
--------------------------------------------------------------------------------------------

UPDATE #PL SET PercentageAccount = (SELECT AG_AccountNum FROM dbo.AccGLHeader WHERE AG_PK = #PL.PercentageAccountPK)
UPDATE #PL SET ConsolidateAccount = (SELECT AG_AccountNum FROM dbo.AccGLHeader WHERE AG_PK = #PL.ConsolidateAccountPK)
UPDATE #PL SET AlternateAccount = (SELECT AG_AccountNum FROM dbo.AccGLHeader WHERE AG_PK = #PL.AlternateAccountPK)

--------------------------------------------------------------------------------------------
-- UPDATE #PL with CurrentPeriod, YearToPeriod, PeriodLastYear, LastYearToPeriod
--------------------------------------------------------------------------------------------

UPDATE #PL SET CurrentPeriod = (SELECT SUM(AccGLAggregate.AA_Amount) FROM dbo.AccGLAggregate
	INNER JOIN dbo.GlbBranch ON AccGLAggregate.AA_GB = GlbBranch.GB_PK
	INNER JOIN dbo.GlbDepartment ON AccGLAggregate.AA_GE = GlbDepartment.GE_PK
	WHERE AccGLAggregate.AA_Period = @Period AND AccGLAggregate.AA_AG = #PL.AccountPK
	AND GlbBranch.GB_GC = @CompanyPK
	AND (charindex(GB_Code, isnull(@BranchList,'')) > 0 OR isnull(@BranchList,'') = '')
	AND (charindex(GE_Code, isnull(@DepartmentList,'')) > 0 OR isnull(@DepartmentList,'') = '')
	)

UPDATE #PL SET YearToPeriod = (SELECT SUM(AccGLAggregate.AA_Amount) FROM dbo.AccGLAggregate
	INNER JOIN dbo.GlbBranch ON AccGLAggregate.AA_GB = GlbBranch.GB_PK
	INNER JOIN dbo.GlbDepartment ON AccGLAggregate.AA_GE = GlbDepartment.GE_PK
	WHERE (AccGLAggregate.AA_Period >= @PeriodYearBegin AND AccGLAggregate.AA_Period <= @Period)
	AND AccGLAggregate.AA_AG = #PL.AccountPK
	AND GlbBranch.GB_GC = @CompanyPK
	AND (charindex(GB_Code, isnull(@BranchList,'')) > 0 OR isnull(@BranchList,'') = '')
	AND (charindex(GE_Code, isnull(@DepartmentList,'')) > 0 OR isnull(@DepartmentList,'') = '')
	)

UPDATE #PL SET PeriodLastYear = (SELECT SUM(AccGLAggregate.AA_Amount) FROM dbo.AccGLAggregate
	INNER JOIN dbo.GlbBranch ON AccGLAggregate.AA_GB = GlbBranch.GB_PK
	INNER JOIN dbo.GlbDepartment ON AccGLAggregate.AA_GE = GlbDepartment.GE_PK
	WHERE AccGLAggregate.AA_Period = @PeriodLastYear AND AccGLAggregate.AA_AG = #PL.AccountPK
	AND GlbBranch.GB_GC = @CompanyPK
	AND (charindex(GB_Code, isnull(@BranchList,'')) > 0 OR isnull(@BranchList,'') = '')
	AND (charindex(GE_Code, isnull(@DepartmentList,'')) > 0 OR isnull(@DepartmentList,'') = '')
	)

UPDATE #PL SET LastYearToPeriod = (SELECT SUM(AccGLAggregate.AA_Amount) FROM dbo.AccGLAggregate
	INNER JOIN dbo.GlbBranch ON AccGLAggregate.AA_GB = GlbBranch.GB_PK
	INNER JOIN dbo.GlbDepartment ON AccGLAggregate.AA_GE = GlbDepartment.GE_PK
	WHERE (AccGLAggregate.AA_Period >= @PeriodLastYearBegin AND AccGLAggregate.AA_Period <= @PeriodLastYear)
	AND AccGLAggregate.AA_AG = #PL.AccountPK
	AND GlbBranch.GB_GC = @CompanyPK
	AND (charindex(GB_Code, isnull(@BranchList,'')) > 0 OR isnull(@BranchList,'') = '')
	AND (charindex(GE_Code, isnull(@DepartmentList,'')) > 0 OR isnull(@DepartmentList,'') = '')
	)
UPDATE #PL SET CurrentPeriod = CurrentPeriod * -1, YearToPeriod = YearToPeriod * -1, LastYearToPeriod = LastYearToPeriod * -1, PeriodLastYear = PeriodLastYear * -1
	WHERE AccountType = 'P&L' AND DebitCredit = 'CR'
UPDATE #PL SET CurrentPeriod = CurrentPeriod * -1, YearToPeriod = YearToPeriod * -1, LastYearToPeriod = LastYearToPeriod * -1, PeriodLastYear = PeriodLastYear * -1
	WHERE AccountType = 'P&L' AND DebitCredit = 'DR'

--------------------------------------------------------------------------------------------
-- Update Consolidate Accounts
--------------------------------------------------------------------------------------------

UPDATE #PL SET CurrentPeriod = (SELECT SUM(PL2.CurrentPeriod) FROM #PL PL2 WHERE PL2.ConsolidateAccount = #PL.AccountNumber),
	YearToPeriod = (SELECT SUM(PL2.YearToPeriod) FROM #PL PL2 WHERE PL2.ConsolidateAccount = #PL.AccountNumber),
	PeriodLastYear = (SELECT SUM(PL2.PeriodLastYear) FROM #PL PL2 WHERE PL2.ConsolidateAccount = #PL.AccountNumber),
	LastYearToPeriod = (SELECT SUM(PL2.LastYearToPeriod) FROM #PL PL2 WHERE PL2.ConsolidateAccount = #PL.AccountNumber)
	WHERE AccountType = 'CLN'

--------------------------------------------------------------------------------------------
-- Update TTL
--------------------------------------------------------------------------------------------

DECLARE Cursor1 CURSOR FOR SELECT AccountPK, AccountNumber, TTL FROM #PL WHERE AccountType = 'TTL' ORDER BY AccountNumber
OPEN Cursor1
FETCH NEXT FROM Cursor1 INTO @AccountPK, @AccountNumber, @TTL
WHILE (@@FETCH_STATUS <> -1)
BEGIN
	SELECT @LastTTLAccNumber = (SELECT TOP 1 AccountNumber FROM #PL WHERE AccountType = 'TTL'
		AND AccountNumber < @AccountNumber AND TTL >= @TTL ORDER BY AccountNumber DESC)

	SELECT @LastTTLAccNumber = ISNULL(@LastTTLAccNumber, (SELECT TOP 1 AccountNumber FROM #PL ORDER BY AccountNumber))

	UPDATE #PL SET CurrentPeriod = (SELECT SUM(PL2.CurrentPeriod) FROM #PL PL2
		WHERE (PL2.AccountNumber >= @LastTTLAccNumber AND PL2.AccountNumber < @AccountNumber)
			AND PL2.AccountType = 'P&L'),
		YearToPeriod = (SELECT SUM(PL2.YearToPeriod) FROM #PL PL2
		WHERE (PL2.AccountNumber >= @LastTTLAccNumber AND PL2.AccountNumber < @AccountNumber)
			AND PL2.AccountType = 'P&L'),
		PeriodLastYear = (SELECT SUM(PL2.PeriodLastYear) FROM #PL PL2
		WHERE (PL2.AccountNumber >= @LastTTLAccNumber AND PL2.AccountNumber < @AccountNumber)
			AND PL2.AccountType = 'P&L'),
		LastYearToPeriod = (SELECT SUM(PL2.LastYearToPeriod) FROM #PL PL2
		WHERE (PL2.AccountNumber >= @LastTTLAccNumber AND PL2.AccountNumber < @AccountNumber)
			AND PL2.AccountType = 'P&L')
	    WHERE #PL.AccountPK = @AccountPK

	FETCH NEXT FROM Cursor1 INTO @AccountPK, @AccountNumber, @TTL
END
DEALLOCATE Cursor1

--------------------------------------------------------------------------------------------
-- APPLY Summary Type & InclZeroBal
--------------------------------------------------------------------------------------------

IF @SummaryType = 'TTLONLY'
BEGIN
	DELETE FROM #PL WHERE AccountType <> 'TTL'
END
ELSE IF @SummaryType = 'NETONLY'
BEGIN
	DELETE FROM #PL WHERE AccountNumber NOT IN (@GrossProfitTotalAccount, @NetProfitTotalAccount, @OverHeadTotalAccount)
END
ELSE IF @InclZeroBal <> 'Y'
BEGIN
	DELETE FROM #PL WHERE AccountType = 'P&L' AND (YearToPeriod IS NULL OR YearToPeriod = 0) AND (LastYearToPeriod IS NULL OR LastYearToPeriod = 0)
END

--------------------------------------------------------------------------------------------
-- REPLACE ZERO WITH NULL FOR P&L
--------------------------------------------------------------------------------------------

UPDATE #PL SET CurrentPeriod = NULL WHERE CurrentPeriod = 0 AND AccountType <> 'TTL'
UPDATE #PL SET YearToPeriod = NULL WHERE YearToPeriod = 0 AND AccountType <> 'TTL'
UPDATE #PL SET PeriodLastYear = NULL WHERE PeriodLastYear = 0 AND AccountType <> 'TTL'
UPDATE #PL SET LastYearToPeriod = NULL WHERE LastYearToPeriod = 0 AND AccountType <> 'TTL'

--------------------------------------------------------------------------------------------
-- REPLACE NULL WITH ZERO FOR TTL
--------------------------------------------------------------------------------------------

UPDATE #PL SET CurrentPeriod = 0 WHERE CurrentPeriod IS NULL AND AccountType = 'TTL'
UPDATE #PL SET YearToPeriod = 0 WHERE YearToPeriod IS NULL AND AccountType = 'TTL'
UPDATE #PL SET PeriodLastYear = 0 WHERE PeriodLastYear IS NULL AND AccountType = 'TTL'
UPDATE #PL SET LastYearToPeriod = 0 WHERE LastYearToPeriod IS NULL AND AccountType = 'TTL'

--------------------------------------------------------------------------------------------
-- Remove Accouts before @GrossProfitTotalAccount
--------------------------------------------------------------------------------------------
DELETE FROM #PL WHERE AccountNumber < @GrossProfitTotalAccount

--------------------------------------------------------------------------------------------
-- INSERT GE_Codes
--------------------------------------------------------------------------------------------
INSERT INTO #PL (AccountNumber, AccountName)
	SELECT ' ' + GE_Code, GE_Desc FROM dbo.AccGLAggregate
	INNER JOIN dbo.GlbBranch ON AccGLAggregate.AA_GB = GlbBranch.GB_PK
	INNER JOIN dbo.GlbDepartment ON AccGLAggregate.AA_GE = GlbDepartment.GE_PK
	WHERE GlbBranch.GB_GC = @CompanyPK
		AND (charindex(GE_Code, isnull(@DepartmentList,'')) > 0 OR isnull(@DepartmentList,'') = '')
	GROUP BY GlbDepartment.GE_Code, GlbDepartment.GE_Desc
	ORDER BY GE_CODE

--------------------------------------------------------------------------------------------
-- UPDATE Values by GE_Codes
--------------------------------------------------------------------------------------------
UPDATE #PL SET CurrentPeriod = (SELECT SUM(AccGLAggregate.AA_Amount) * -1 FROM dbo.AccGLAggregate
		INNER JOIN dbo.AccGLHeader ON AccGLHeader.AG_PK = AccGLAggregate.AA_AG
		INNER JOIN dbo.GlbBranch ON AccGLAggregate.AA_GB = GlbBranch.GB_PK
		INNER JOIN dbo.GlbDepartment ON AccGLAggregate.AA_GE = GlbDepartment.GE_PK
		WHERE AccGLAggregate.AA_Period = @Period AND GlbDepartment.GE_Code = LTRIM(#PL.AccountNumber)
			AND AccGLHeader.AG_AccountNum < @GrossProfitTotalAccount AND GlbBranch.GB_GC = @CompanyPK
			AND (charindex(GB_Code, isnull(@BranchList,'')) > 0 OR isnull(@BranchList,'') = '')
	) WHERE #PL.AccountPK is NULL

UPDATE #PL SET YearToPeriod = (SELECT SUM(AccGLAggregate.AA_Amount) * -1 FROM dbo.AccGLAggregate
		INNER JOIN dbo.AccGLHeader ON AccGLHeader.AG_PK = AccGLAggregate.AA_AG
		INNER JOIN dbo.GlbBranch ON AccGLAggregate.AA_GB = GlbBranch.GB_PK
		INNER JOIN dbo.GlbDepartment ON AccGLAggregate.AA_GE = GlbDepartment.GE_PK
		WHERE (AccGLAggregate.AA_Period >= @PeriodYearBegin AND AccGLAggregate.AA_Period <= @Period)
			AND GlbDepartment.GE_Code = LTRIM(#PL.AccountNumber)
			AND AccGLHeader.AG_AccountNum < @GrossProfitTotalAccount AND GlbBranch.GB_GC = @CompanyPK
			AND (charindex(GB_Code, isnull(@BranchList,'')) > 0 OR isnull(@BranchList,'') = '')
	) WHERE #PL.AccountPK is NULL

UPDATE #PL SET PeriodLastYear = (SELECT SUM(AccGLAggregate.AA_Amount) * -1 FROM dbo.AccGLAggregate
		INNER JOIN dbo.AccGLHeader ON AccGLHeader.AG_PK = AccGLAggregate.AA_AG
		INNER JOIN dbo.GlbBranch ON AccGLAggregate.AA_GB = GlbBranch.GB_PK
		INNER JOIN dbo.GlbDepartment ON AccGLAggregate.AA_GE = GlbDepartment.GE_PK
		WHERE AccGLAggregate.AA_Period = @PeriodLastYear
			AND GlbDepartment.GE_Code = LTRIM(#PL.AccountNumber)
			AND AccGLHeader.AG_AccountNum < @GrossProfitTotalAccount AND GlbBranch.GB_GC = @CompanyPK
			AND (charindex(GB_Code, isnull(@BranchList,'')) > 0 OR isnull(@BranchList,'') = '')
	) WHERE #PL.AccountPK is NULL

UPDATE #PL SET LastYearToPeriod = (SELECT SUM(AccGLAggregate.AA_Amount) * -1 FROM dbo.AccGLAggregate
		INNER JOIN dbo.AccGLHeader ON AccGLHeader.AG_PK = AccGLAggregate.AA_AG
		INNER JOIN dbo.GlbBranch ON AccGLAggregate.AA_GB = GlbBranch.GB_PK
		INNER JOIN dbo.GlbDepartment ON AccGLAggregate.AA_GE = GlbDepartment.GE_PK
		WHERE (AccGLAggregate.AA_Period >= @PeriodLastYearBegin AND AccGLAggregate.AA_Period <= @PeriodLastYear)
			AND GlbDepartment.GE_Code = LTRIM(#PL.AccountNumber)
			AND AccGLHeader.AG_AccountNum < @GrossProfitTotalAccount AND GlbBranch.GB_GC = @CompanyPK
			AND (charindex(GB_Code, isnull(@BranchList,'')) > 0 OR isnull(@BranchList,'') = '')
	) WHERE #PL.AccountPK is NULL

--------------------------------------------------------------------------------------------
-- Return #PL
--------------------------------------------------------------------------------------------
SELECT * FROM #PL ORDER BY AccountNumber",
"DROP PROCEDURE ClientProfitAndLossReportStockWell",
DbRoutineType.SqlProcedureTypeDesc));

		#endregion
	}
}
