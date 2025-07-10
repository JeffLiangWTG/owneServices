using System.Collections.Immutable;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Database.Shared;

namespace Enterprise.Client.JAS
{
	class JASClientDbSchemaUpgradeInfo : IExtensionObjects
	{
		#region DbObjectNames constants

		public static class DbObjectNames
		{
			// FUNCTION
			public const string ClientConvertLocalToForeignAmount = "ClientConvertLocalToForeignAmount";
			public const string ClientGetGLAutoJournalAmount = "ClientGetGLAutoJournalAmount";
			public const string ClientGetFirstPeriodForCurrentYear = "ClientGetFirstPeriodForCurrentYear";
			public const string ClientGetCognosAgeFromDate = "ClientGetCognosAgeFromDate";

			// STORED PROCEDURE
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts";
			public const string ClientInvertCognosRawAggregateSignageIfApplicable = "ClientInvertCognosRawAggregateSignageIfApplicable";
			public const string ClientUpdateCognosRawAggregateRecordsWithALTAccounts = "ClientUpdateCognosRawAggregateRecordsWithALTAccounts";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromTTLAccounts = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromTTLAccounts";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts";
			public const string ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts = "ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts";
			public const string ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts = "ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts";
			public const string ClientInsertCognosAccountsIntoCognosExportTempTable = "ClientInsertCognosAccountsIntoCognosExportTempTable";

			// TABLE
			public const string ClientCognosAccGLAccountDescriptorExtraInfo = "ClientCognosAccGLAccountDescriptorExtraInfo";
			public const string ClientCognosSubClassificationAccountDebtorMapping = "ClientCognosSubClassificationAccountDebtorMapping";
			public const string ClientCognosSubClassificationAccountCreditorMapping = "ClientCognosSubClassificationAccountCreditorMapping";
			public const string ClientCognosGroupingFlags = "ClientCognosGroupingFlags";

			// VIEW
			public const string ClientAccTransactionHeaderWithJobInfo = "ClientAccTransactionHeaderWithJobInfo";
		}

		#endregion

		ImmutableArray<DatabaseObjectCreateScript> IExtensionObjects.TableCreationScripts => TableCreationScripts;
		ImmutableArray<DatabaseViewAndRoutineCreateScript> IExtensionObjects.ViewAndRoutineCreationScripts => ViewAndRoutinesCreationScripts;

		static ImmutableArray<DatabaseObjectCreateScript> TableCreationScripts => ImmutableArray.Create(
			ClientCognosAccGLAccountDescriptorExtraInfo,
			ClientCognosSubClassificationAccountDebtorMapping,
			ClientCognosSubClassificationAccountCreditorMapping,
			ClientCognosGroupingFlags
		);

		static ImmutableArray<DatabaseViewAndRoutineCreateScript> ViewAndRoutinesCreationScripts => ImmutableArray.Create(
			// FUNCTION
			ClientConvertLocalToForeignAmount,
			ClientGetGLAutoJournalAmount,
			ClientGetFirstPeriodForCurrentYear,
			ClientGetCognosAgeFromDate,
			ClientGetMaturityDateForNetting,

			// STORED PROCEDURE
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts,
			ClientInvertCognosRawAggregateSignageIfApplicable,
			ClientUpdateCognosRawAggregateRecordsWithALTAccounts,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromTTLAccounts,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts,
			ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts,
			ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts,
			ClientInsertCognosAccountsIntoCognosExportTempTable,

			// VIEW
			ClientAccTransactionHeaderWithJobInfo
		);

		#region FUNCTION ClientConvertLocalToForeignAmount

		static DatabaseViewAndRoutineCreateScript ClientConvertLocalToForeignAmount
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientConvertLocalToForeignAmount", @"
CREATE FUNCTION ClientConvertLocalToForeignAmount(@LocalAmount as money, @ExchangeRate as decimal(18,9), @CompanyPK as uniqueidentifier, @ForeignCurrencyNK as varchar(3))
RETURNS money
AS
BEGIN

DECLARE @Result money
DECLARE @IsReciprocal BIT
DECLARE @SubUnitRatio int
DECLARE @Decimals int

SET @IsReciprocal = 
(
    SELECT  GC_IsReciprocal 
    FROM    dbo.GlbCompany
    WHERE   GC_PK = @CompanyPK
)

SET @SubUnitRatio =
(
	SELECT	RX_SubUnitRatio
	FROM	dbo.RefCurrency
	WHERE	RX_Code = @ForeignCurrencyNK
)

SET @Decimals = LOG10(@SubUnitRatio)
IF (@Decimals < 0)
BEGIN
	SET @Decimals = 0
END

IF (@ExchangeRate != 0)
BEGIN 
    IF (@IsReciprocal = 1)
	BEGIN
        SET @Result = ROUND(@LocalAmount / @ExchangeRate, @Decimals) 
	END
    ELSE
	BEGIN		
        SET @Result = ROUND(@LocalAmount * @ExchangeRate, @Decimals)
	END
END
ELSE
BEGIN
    SET @Result = 0
END

RETURN @Result

END"
							, "DROP FUNCTION ClientConvertLocalToForeignAmount"
							, DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientGetGLAutoJournalAmount

		static DatabaseViewAndRoutineCreateScript ClientGetGLAutoJournalAmount
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientGetGLAutoJournalAmount", @"
CREATE FUNCTION ClientGetGLAutoJournalAmount(@PostDate as smalldatetime, @ReverseDate as smalldatetime, @ExportStartDate as smalldatetime, @LineAmount as money, @CompanyPK as uniqueidentifier)
RETURNS money
AS
BEGIN

DECLARE @Result money

DECLARE @EndDate smalldatetime
IF (@ExportStartDate >= @ReverseDate)
BEGIN
    SET @EndDate = @ReverseDate
END
ELSE
BEGIN
    SET @EndDate = @ExportStartDate
END

DECLARE @NumberOfPeriods int
SELECT  @NumberOfPeriods = COUNT(*)
FROM    dbo.AccPeriodManagement
WHERE   AM_Period >= dbo.GetPeriodFromDate(@PostDate, @CompanyPK)
        AND AM_Period <= dbo.GetPeriodFromDate(@EndDate, @CompanyPK)
        AND AM_GC_Company = @CompanyPK

SET @Result = @NumberOfPeriods * @LineAmount

RETURN @Result

END"
							, "DROP FUNCTION ClientGetGLAutoJournalAmount"
							, DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientGetFirstPeriodForCurrentYear

		static DatabaseViewAndRoutineCreateScript ClientGetFirstPeriodForCurrentYear
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientGetFirstPeriodForCurrentYear", @"
CREATE FUNCTION ClientGetFirstPeriodForCurrentYear(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
RETURNS int
AS
BEGIN

DECLARE @Result int

SELECT	    TOP 1  @Result = AM_Period
FROM    	dbo.AccPeriodManagement
WHERE	    AM_GC_Company = @CompanyPK
		    AND AM_Year = DATEPART(year, @ExportStartDate)
ORDER BY    AM_Period

SET @Result = CASE WHEN @Result IS NULL THEN 0 ELSE @Result END
RETURN @Result

END"
							, "DROP FUNCTION ClientGetFirstPeriodForCurrentYear"
							, DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientGetCognosAgeFromDate

		static DatabaseViewAndRoutineCreateScript ClientGetCognosAgeFromDate
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientGetCognosAgeFromDate", @"
CREATE FUNCTION ClientGetCognosAgeFromDate(@TransactionDate as smalldatetime, @ExportStartDate as smalldatetime)
RETURNS varchar(2)
AS
BEGIN

DECLARE @Result varchar(2)

SET @Result =
    CASE
        WHEN @TransactionDate BETWEEN DATEADD(day, -30, @ExportStartDate) AND @ExportStartDate THEN '30'
        WHEN @TransactionDate BETWEEN DATEADD(day, -60, @ExportStartDate) AND DATEADD(day, -31, @ExportStartDate) THEN '60'
        WHEN @TransactionDate BETWEEN DATEADD(day, -90, @ExportStartDate) AND DATEADD(day, -61, @ExportStartDate) THEN '90'
        ELSE '++'
    END

RETURN @Result

END"
							, "DROP FUNCTION ClientGetCognosAgeFromDate"
							, DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region FUNCTION ClientGetMaturityDateForNetting

		static DatabaseViewAndRoutineCreateScript ClientGetMaturityDateForNetting
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientGetMaturityDateForNetting", @"
CREATE FUNCTION ClientGetMaturityDateForNetting(@ShipmentPK as uniqueidentifier, @ShipmentETD as smalldatetime, @ConsolPK as uniqueidentifier)
RETURNS smalldatetime
AS
BEGIN
	
DECLARE @Result smalldatetime

DECLARE @PaymentTerms int
DECLARE @ETD smalldatetime

IF (@ShipmentETD IS NULL)
BEGIN	
	SELECT	TOP 1 @ETD = JW_ETD
	FROM	dbo.JobConsol
			JOIN 
			(
				SELECT
					JW_JK = JW_ParentGUID,
					JW_ETD
				FROM
					dbo.JobConsolTransport
				WHERE
					JW_ParentType = 'CON'
			) as ConsolTransports ON JK_PK = JW_JK
	WHERE	 JK_PK = @ConsolPK
	ORDER BY JW_ETD
END
ELSE
BEGIN
	SET @ETD = @ShipmentETD	
END

IF (@ETD IS NULL)
BEGIN
	SELECT	@ETD = MIN(SL_EventTime)
	FROM	dbo.StmALog
	WHERE	SL_Parent = @ShipmentPK
END

SELECT	@PaymentTerms = ISNULL(CAST(CAST(CAST(SD_BinaryValue AS varbinary) as nvarchar)as int), 45)
FROM	dbo.StmData
WHERE	SD_Name = 'NettingPaymentTerms'

SET @PaymentTerms = ISNULL(@PaymentTerms, 45)
SET @Result = DATEADD(day, @PaymentTerms, DATEADD(mm, DATEDIFF(mm, -1, @ETD), -1))

RETURN @Result

END"
					, "DROP FUNCTION ClientGetMaturityDateForNetting"
					, DbRoutineType.SqlFunctionScalarTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @PnLStartPeriod int
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
		    SUM(-AH_InvoiceAmount) AS Amount,
		    RX_Code AS TransactionCurrency,
		    SUM(dbo.ClientConvertLocalToForeignAmount(-AH_InvoiceAmount, AH_ExchangeRate, @CompanyPK, AH_RX_NKTransactionCurrency)) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AH_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AH_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionHeader) TransactionHeader
			INNER JOIN dbo.AccGLHeader ON AH_AG = AG_PK
			INNER JOIN dbo.GlbBranch ON AH_GB = GB_PK
			INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
			INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
			LEFT OUTER JOIN #CounterCompanies ON AH_OH = T2_OH
			LEFT OUTER JOIN #CognosModes ON AH_GE = T3_GE

WHERE 	    AH_Ledger IN ('AR', 'AP')
            AND AH_TransactionType = 'JNL'
            AND AH_PostToGL = 'Y'
		    AND GB_GC = @CompanyPK
            AND AH_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @PnLStartPeriod int
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
			T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
			T3_Mode AS Mode,
			GB_Code AS Branch,
			T2_BusinessType AS BusinessType,
			SUM(CASE WHEN AH_Ledger = 'CB' THEN AH_InvoiceAmount ELSE -AH_InvoiceAmount END) AS Amount,
			RX_Code AS TransactionCurrency,				
			SUM(CASE 
				    WHEN AH_Ledger = 'CB' THEN dbo.ClientConvertLocalToForeignAmount(AH_InvoiceAmount, AH_ExchangeRate, @CompanyPK, AH_RX_NKTransactionCurrency)
				    ELSE dbo.ClientConvertLocalToForeignAmount(-AH_InvoiceAmount, AH_ExchangeRate, @CompanyPK, AH_RX_NKTransactionCurrency)
			    END) AS TransactionAmount,
			T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AH_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AH_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionHeader) TransactionHeader
            INNER JOIN dbo.AccBankAccount ON AH_AB = AB_PK
            INNER JOIN dbo.AccGLHeader ON AB_AG = AG_PK
			INNER JOIN dbo.GlbBranch ON AH_GB = GB_PK
			INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code			
			INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
			LEFT OUTER JOIN #CounterCompanies ON AH_OH = T2_OH			
			LEFT OUTER JOIN #CognosModes ON AH_GE = T3_GE

WHERE 	    (
	    		(AH_Ledger IN ('AR', 'AP') AND AH_TransactionType IN ('PAY', 'REC')) 
	    		OR (AH_Ledger = 'CB' AND AH_TransactionType IN ('TRF', 'EXX'))
			)
			AND AH_PostToGL = 'Y'
			AND GB_GC = @CompanyPK
			AND AH_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END
"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @PnLStartPeriod int
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
		    SUM(-AL_LineAmount) AS Amount,
		    RX_Code AS TransactionCurrency,
		    SUM(dbo.ClientConvertLocalToForeignAmount(-AL_LineAmount, AL_ExchangeRate, @CompanyPK, AL_RX_NKTransactionCurrency)) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AH_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AH_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionHeader) TransactionHeader
            INNER JOIN dbo.AccTransactionLines ON AL_AH = AH_PK
            INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
		    INNER JOIN dbo.GlbBranch ON AH_GB = GB_PK
		    INNER JOIN dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
		    INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
		    LEFT OUTER JOIN #CounterCompanies ON AH_OH = T2_OH
		    LEFT OUTER JOIN #CognosModes ON AH_GE = T3_GE

WHERE 	    AH_Ledger = 'CB'
            AND AH_TransactionType IN ('DRC', 'DPY')		    
            AND AH_PostToGL = 'Y'
		    AND GB_GC = @CompanyPK
            AND AH_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END
"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @PnLStartPeriod int
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,	
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
		    SUM(AH_InvoiceAmount + AH_GSTAmount) AS Amount,
		    RX_Code AS TransactionCurrency,
		    SUM(AH_OSTotal) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AH_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AH_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionHeader) TransactionHeader
            INNER JOIN dbo.AccBankAccount ON AH_AB = AB_PK
            INNER JOIN dbo.AccGLHeader ON AB_AG = AG_PK
		    INNER JOIN dbo.GlbBranch ON AH_GB = GB_PK
            INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code		    
		    INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
		    LEFT OUTER JOIN #CounterCompanies ON AH_OH = T2_OH
		    LEFT OUTER JOIN #CognosModes ON AH_GE = T3_GE			

WHERE	    AH_Ledger = 'CB'
            AND AH_TransactionType in ('DRC', 'DPY')
            AND AH_PostToGL = 'Y'
		    AND GB_GC = @CompanyPK
            AND AH_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END
"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @PnLStartPeriod int
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
		    SUM(-AL_LineAmount) AS Amount,
		    RX_Code AS TransactionCurrency,
		    SUM(dbo.ClientConvertLocalToForeignAmount(-AL_LineAmount, AL_ExchangeRate, @CompanyPK, AL_RX_NKTransactionCurrency)) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM        ( SELECT *, dbo.GetPeriodFromDate(AH_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AH_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionHeader) TransactionHeader
		    INNER JOIN		dbo.AccTransactionLines ON AL_AH = AH_PK
		    INNER JOIN		dbo.GlbBranch ON AL_GB = GB_PK
            INNER JOIN		dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
            LEFT OUTER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
		    INNER JOIN      dbo.AccGLHeader ON AG_PK =
                            CASE
                                WHEN AL_AC IS NULL THEN AL_AG
                                WHEN AL_LineType = 'REV' THEN AC_AG_RevenueAccount
		    					WHEN AL_LineType = 'CST' THEN AC_AG_CostAccount
		    					ELSE AL_AG
                            END
			INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
		    LEFT OUTER JOIN #CounterCompanies ON AH_OH = T2_OH
		    LEFT OUTER JOIN #CognosModes ON AL_GE = T3_GE

WHERE 	    AH_Ledger IN ('AP', 'AR')
		    AND AH_TransactionType IN ('INV', 'CRD', 'ADJ')
		    AND AH_PostToGL = 'Y'
		    AND GB_GC = @CompanyPK
		    AND AH_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA' 
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END
"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @PnLStartPeriod int
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
			T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
			T3_Mode AS Mode,
			GB_Code AS Branch,
			T2_BusinessType AS BusinessType,
			SUM(LineAmount) AS Amount,
			RX_Code AS TransactionCurrency,
			SUM(dbo.ClientConvertLocalToForeignAmount(LineAmount, AL_ExchangeRate, @CompanyPK, AL_RX_NKTransactionCurrency)) AS TransactionAmount,
			T2_Geographical AS Geographical

FROM	    (
                SELECT  AG_PK, -AL_LineAmount AS LineAmount, AccTransactionLines.*
                FROM    dbo.AccTransactionLines
			            INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
			            INNER JOIN dbo.AccGLHeader ON AG_PK =
	    					        CASE
	    					            WHEN AL_LineType = 'REV' THEN AC_AG_RevenueAccount
	    						        WHEN AL_LineType = 'WIP' THEN AC_AG_WIPAccount
	    						        WHEN AL_LineType = 'CST' THEN AC_AG_CostAccount
	    						        ELSE AC_AG_AccrualAccount
	    					        END

                UNION

                SELECT  AG_PK, AL_LineAmount AS LineAmount, AccTransactionLines.*
                FROM    dbo.AccTransactionLines
			            INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
            ) TransactionLines
            INNER JOIN      ( SELECT *, dbo.GetPeriodFromDate(AH_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AH_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionHeader) TransactionHeader ON AL_AH = AH_PK
            INNER JOIN		dbo.GlbBranch ON AL_GB = GB_PK
			INNER JOIN		dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
			INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
			LEFT OUTER JOIN #CounterCompanies ON AH_OH = T2_OH
			LEFT OUTER JOIN #CognosModes ON AL_GE = T3_GE

WHERE 	    AH_Ledger = 'JC'
			AND AH_TransactionType = 'JNL'
			AND AH_PostToGL = 'Y'
			AND GB_GC = @CompanyPK
			AND AH_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END
"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @PnLStartPeriod int
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

-- General and Reverse Journals
INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
            SUM(AL_LineAmount) AS Amount,
		    RX_Code AS TransactionCurrency,
            SUM(dbo.ClientConvertLocalToForeignAmount(AL_LineAmount, AL_ExchangeRate, @CompanyPK, AL_RX_NKTransactionCurrency)) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AH_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AH_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionHeader) TransactionHeader
		    INNER JOIN dbo.AccTransactionLines ON AH_PK = AL_AH
            INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
		    INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
		    INNER JOIN dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
			INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
		    LEFT OUTER JOIN #CounterCompanies ON AH_OH = T2_OH
		    LEFT OUTER JOIN #CognosModes ON AL_GE = T3_GE

WHERE	    AH_Ledger  = 'GL'
		    AND (AH_TransactionType = 'GJL' OR (AH_TransactionType = 'RJL' AND AH_DueDate > @ExportStartDate))
            AND AH_PostToGL = 'Y'
		    AND GB_GC = @CompanyPK
            AND AH_PostDate <= @ExportStartDate AND 
			AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

-- Auto Journals
INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
            SUM(AL_LineAmount) AS Amount,
			RX_Code AS TransactionCurrency,
            SUM(dbo.ClientConvertLocalToForeignAmount(AL_LineAmount, AL_ExchangeRate, @CompanyPK, AL_RX_NKTransactionCurrency)) AS TransactionAmount,                    
		    T2_Geographical AS Geographical

FROM	    dbo.AccTransactionHeader
		    INNER JOIN dbo.AccTransactionLines ON AH_PK = AL_AH		    
			INNER JOIN dbo.AccGLHeader ON AL_AG = AG_PK
		    INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
		    INNER JOIN dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
		    INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
			INNER JOIN (SELECT AM_Period AS Period, dbo.ClientGetCognosAgeFromDate(AM_StartDate, @ExportStartDate) AS Age, * FROM dbo.AccPeriodManagement) PeriodManagement ON 
						AM_GC_Company = GB_GC
						AND AM_Period >= dbo.GetPeriodFromDate(AH_PostDate, @CompanyPK)
						AND AM_Period <= dbo.GetPeriodFromDate(AH_DueDate, @CompanyPK)
			LEFT OUTER JOIN #CounterCompanies ON AH_OH = T2_OH
		    LEFT OUTER JOIN #CognosModes ON AL_GE = T3_GE					 

WHERE	    AH_Ledger  = 'GL'
		    AND AH_TransactionType = 'AJL'
            AND AH_PostToGL = 'Y'
		    AND GB_GC = @CompanyPK
            AND AH_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
			AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END
"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @PnLStartPeriod int
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

-- Posted
INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
            SUM(AL_LineAmount) AS Amount,
		    RX_Code AS TransactionCurrency,
		    SUM(dbo.ClientConvertLocalToForeignAmount(AL_LineAmount, AL_ExchangeRate, @CompanyPK, AL_RX_NKTransactionCurrency)) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AL_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AL_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionLines) TransactionLines
		    INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
		    INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
		    INNER JOIN dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
            INNER JOIN dbo.AccGLHeader ON AG_PK = CASE WHEN AL_LineType = 'WIP' THEN AC_AG_WIPAccount ELSE AC_AG_AccrualAccount END
            INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
            LEFT OUTER JOIN #CounterCompanies ON AL_OH = T2_OH
            LEFT OUTER JOIN #CognosModes ON AL_GE = T3_GE

WHERE 	    AL_PostToGL = 'Y'
			AND GB_GC = @CompanyPK
		    AND AL_LineType IN ('WIP', 'ACR')
            AND AL_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

-- Reversed
INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
            SUM(-AL_LineAmount) AS Amount,
		    RX_Code AS TransactionCurrency,
		    SUM(dbo.ClientConvertLocalToForeignAmount(-AL_LineAmount, AL_ExchangeRate, @CompanyPK, AL_RX_NKTransactionCurrency)) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AL_ReverseDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AL_ReverseDate, @ExportStartDate) AS Age FROM dbo.AccTransactionLines) TransactionLines
		    INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
		    INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
		    INNER JOIN dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
            INNER JOIN dbo.AccGLHeader ON AG_PK = CASE WHEN AL_LineType = 'WIP' THEN AC_AG_WIPAccount ELSE AC_AG_AccrualAccount END
            INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
            LEFT OUTER JOIN #CounterCompanies ON AL_OH = T2_OH
            LEFT OUTER JOIN #CognosModes ON AL_GE = T3_GE

WHERE 	    AL_ReverseToGL = 'Y'
			AND GB_GC = @CompanyPK
		    AND AL_LineType IN ('WIP', 'ACR')
            AND AL_ReverseDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END
"
		, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR"
		, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @ARAccountGuid      uniqueidentifier
DECLARE @APAccountGuid      uniqueidentifier
DECLARE @PnLStartPeriod     int

SELECT @ARAccountGuid = CAST(CAST(CAST(SD_BinaryValue AS binary(76)) AS nvarchar(38)) AS uniqueidentifier) FROM dbo.StmData WHERE SD_Name = 'GL_AR_CONTROL_ACCOUNT'
SELECT @APAccountGuid = CAST(CAST(CAST(SD_BinaryValue AS binary(76)) AS nvarchar(38)) AS uniqueidentifier) FROM dbo.StmData WHERE SD_Name = 'GL_AP_CONTROL_ACCOUNT'
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
            SUM(CASE 
                    WHEN AH_TransactionType IN ('JNL', 'PAY', 'REC', 'CTR', 'EXX', 'DSC', 'OVP', 'TRF') THEN AH_InvoiceAmount
                    WHEN AH_TransactionType IN ('INV', 'CRD', 'ADJ') THEN AH_InvoiceAmount + AH_GSTAmount
                    ELSE 0
                END) AS Amount,
		    RX_Code AS TransactionCurrency,
		    SUM(CASE 
                    WHEN AH_TransactionType IN ('JNL', 'PAY', 'REC', 'CTR', 'EXX', 'DSC', 'OVP', 'TRF') THEN dbo.ClientConvertLocalToForeignAmount(AH_InvoiceAmount, AH_ExchangeRate, @CompanyPK, AH_RX_NKTransactionCurrency)
                    WHEN AH_TransactionType IN ('INV', 'CRD', 'ADJ') THEN AH_OSTotal
                    ELSE 0
                END) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AH_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AH_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionHeader) TransactionHeader            
		    INNER JOIN dbo.GlbBranch ON AH_GB = GB_PK
		    INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
		    INNER JOIN dbo.AccGLHeader ON AG_PK = CASE AH_Ledger WHEN 'AR' THEN @ARAccountGuid ELSE @APAccountGuid END
            INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
		    LEFT OUTER JOIN #CounterCompanies ON AH_OH = T2_OH
            LEFT OUTER JOIN #CognosModes ON AH_GE = T3_GE			

WHERE       AH_Ledger IN ('AR', 'AP')
            AND AH_TransactionType IN ('JNL', 'PAY', 'REC', 'CTR', 'EXX', 'DSC', 'OVP', 'INV', 'ADJ', 'CRD', 'TRF')
		    AND AH_PostToGL = 'Y'
		    AND GB_GC = @CompanyPK
		    AND AH_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END
"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @OverpaymentAccountGuid         uniqueidentifier
DECLARE @PnLStartPeriod                 int

SELECT @OverpaymentAccountGuid = CAST(CAST(CAST(SD_BinaryValue AS binary(76)) AS nvarchar(38)) AS uniqueidentifier) FROM dbo.StmData WHERE SD_Name = 'GL_OVERPAYMENTS_ACCOUNT'
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
		    SUM(-AH_InvoiceAmount) AS Amount,                                
		    RX_Code AS TransactionCurrency,
		    SUM(dbo.ClientConvertLocalToForeignAmount(-AH_InvoiceAmount, AH_ExchangeRate, @CompanyPK, AH_RX_NKTransactionCurrency)) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AH_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AH_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionHeader) TransactionHeader
		    INNER JOIN      dbo.GlbBranch ON AH_GB = GB_PK
		    INNER JOIN      dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code            
		    INNER JOIN      dbo.AccGLHeader ON AG_PK =
                            CASE
                                WHEN AH_TransactionType IN ('EXX', 'DSC') THEN AH_AG
                                WHEN AH_TransactionType = 'OVP' THEN @OverpaymentAccountGuid
                            END
            INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
		    LEFT OUTER JOIN #CounterCompanies ON AH_OH = T2_OH
            LEFT OUTER JOIN #CognosModes ON AH_GE = T3_GE			

WHERE       AH_Ledger NOT IN ('JC', 'GL')
            AND AH_TransactionType IN ('EXX', 'DSC', 'OVP')
		    AND AH_PostToGL = 'Y'
		    AND GB_GC = @CompanyPK
		    AND AH_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END
"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @GSTInputAccountGuid            uniqueidentifier
DECLARE @GSTOutputAccountGuid           uniqueidentifier
DECLARE @PnLStartPeriod                 int

SELECT @GSTInputAccountGuid = CAST(CAST(CAST(SD_BinaryValue AS binary(76)) AS nvarchar(38)) AS uniqueidentifier) FROM dbo.StmData WHERE SD_Name = 'GL_GST_INPUT_ACCOUNT'
SELECT @GSTOutputAccountGuid = CAST(CAST(CAST(SD_BinaryValue AS binary(76)) AS nvarchar(38)) AS uniqueidentifier) FROM dbo.StmData WHERE SD_Name = 'GL_GST_OUTPUT_ACCOUNT'
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
		    SUM(-AL_GSTVAT) AS Amount,
		    RX_Code AS TransactionCurrency,
		    SUM(dbo.ClientConvertLocalToForeignAmount(-AL_GSTVAT, AL_ExchangeRate, @CompanyPK, AL_RX_NKTransactionCurrency)) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AH_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AH_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionHeader) TransactionHeader
		    INNER JOIN      dbo.GlbBranch ON AH_GB = GB_PK
		    INNER JOIN      dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
            INNER JOIN      dbo.AccTransactionLines ON AH_PK = AL_AH            
		    INNER JOIN      dbo.AccGLHeader ON AG_PK =
                            CASE
                                WHEN AH_TransactionType = 'DRC' THEN @GSTOutputAccountGuid
                                WHEN AH_TransactionType = 'DPY' THEN @GSTInputAccountGuid
                                WHEN AH_Ledger = 'AP' THEN @GSTInputAccountGuid
                                ELSE @GSTOutputAccountGuid
                            END
            INNER JOIN dbo.AccGLDescriptorPivot ON AG_PK = YJ_AG 
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
		    LEFT OUTER JOIN #CounterCompanies ON AH_OH = T2_OH
            LEFT OUTER JOIN #CognosModes ON AH_GE = T3_GE

WHERE       AH_Ledger NOT IN ('JC', 'GL')
            AND AH_TransactionType IN ('DRC', 'DPY', 'INV', 'CRD', 'ADJ')
		    AND AH_PostToGL = 'Y'
		    AND GB_GC = @CompanyPK
		    AND AH_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END
"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts(@CompanyPK as uniqueidentifier, @ExportStartDate as smalldatetime)
AS
BEGIN

DECLARE @WIPAccountGuid uniqueidentifier
DECLARE @ACRAccountGuid uniqueidentifier
DECLARE @PnLStartPeriod int

SELECT @WIPAccountGuid = CAST(CAST(CAST(SD_BinaryValue AS binary(76)) AS nvarchar(38)) AS uniqueidentifier) FROM dbo.StmData WHERE SD_Name = 'GL_ACCRUED_REVENUE_ACCOUNT'
SELECT @ACRAccountGuid = CAST(CAST(CAST(SD_BinaryValue AS binary(76)) AS nvarchar(38)) AS uniqueidentifier) FROM dbo.StmData WHERE SD_Name = 'GL_ACCRUED_COST_ACCOUNT'
SET @PnLStartPeriod = dbo.ClientGetFirstPeriodForCurrentYear(@CompanyPK, @ExportStartDate)

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
            SUM(-AL_LineAmount) AS Amount,
		    RX_Code AS TransactionCurrency,
		    SUM(dbo.ClientConvertLocalToForeignAmount(-AL_LineAmount, AL_ExchangeRate, @CompanyPK, AL_RX_NKTransactionCurrency)) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AL_PostDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AL_PostDate, @ExportStartDate) AS Age FROM dbo.AccTransactionLines) TransactionLines
		    INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
		    INNER JOIN dbo.AccGLHeader ON CASE WHEN AL_LineType = 'WIP' THEN AC_AG_WIPAccount ELSE AC_AG_AccrualAccount END = AG_PK
		    INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
		    INNER JOIN dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
            INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = CASE WHEN AL_LineType = 'WIP' THEN @WIPAccountGuid ELSE @ACRAccountGuid END
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ' 
		    LEFT OUTER JOIN #CognosModes ON AL_GE = T3_GE
		    LEFT OUTER JOIN #CounterCompanies ON AL_OH = T2_OH

WHERE 	    AL_PostToGL = 'Y'
			AND GB_GC = @CompanyPK
		    AND AL_LineType IN ('WIP', 'ACR')
            AND AL_PostDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    AJ_PK AS AJ,
            Age,
		    T2_CompanyCode AS CompanyCode,
            T2_OG_CreditorGroup AS CreditorGroup,
            T2_OJ_DebtorGroup AS DebtorGroup,
		    T3_Mode AS Mode,
		    GB_Code AS Branch,
		    T2_BusinessType AS BusinessType,
            SUM(AL_LineAmount) AS Amount,
		    RX_Code AS TransactionCurrency,
		    SUM(dbo.ClientConvertLocalToForeignAmount(AL_LineAmount, AL_ExchangeRate, @CompanyPK, AL_RX_NKTransactionCurrency)) AS TransactionAmount,
		    T2_Geographical AS Geographical

FROM	    ( SELECT *, dbo.GetPeriodFromDate(AL_ReverseDate, @CompanyPK) AS Period, dbo.ClientGetCognosAgeFromDate(AL_ReverseDate, @ExportStartDate) AS Age FROM dbo.AccTransactionLines) TransactionLines
		    INNER JOIN dbo.AccChargeCode ON AL_AC = AC_PK
		    INNER JOIN dbo.AccGLHeader ON CASE WHEN AL_LineType = 'WIP' THEN AC_AG_WIPAccount ELSE AC_AG_AccrualAccount END = AG_PK
		    INNER JOIN dbo.GlbBranch ON AL_GB = GB_PK
		    INNER JOIN dbo.RefCurrency ON AL_RX_NKTransactionCurrency = RX_Code
            INNER JOIN dbo.AccGLDescriptorPivot ON YJ_AG = CASE WHEN AL_LineType = 'WIP' THEN @WIPAccountGuid ELSE @ACRAccountGuid END
			INNER JOIN dbo.AccGLAccountDescriptor ON AJ_PK = YJ_AJ AND AJ_Language = 'ZZZ'
		    LEFT OUTER JOIN #CognosModes ON AL_GE = T3_GE
		    LEFT OUTER JOIN #CounterCompanies ON AL_OH = T2_OH

WHERE 	    AL_ReverseToGL = 'Y'
			AND GB_GC = @CompanyPK
		    AND AL_LineType IN ('WIP', 'ACR')
            AND AL_ReverseDate <= @ExportStartDate
			AND AJ_ReportType = 'COA'  
            AND Period >= CASE WHEN AJ_ReportCategory = 'P&L' THEN @PnLStartPeriod ELSE 0 END

GROUP BY	AJ_PK, Age, T2_CompanyCode, T2_OG_CreditorGroup, T2_OJ_DebtorGroup, T3_Mode, GB_Code, T2_BusinessType, RX_Code, T2_Geographical

END
"
		, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts"
		, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInvertCognosRawAggregateSignageIfApplicable

		static DatabaseViewAndRoutineCreateScript ClientInvertCognosRawAggregateSignageIfApplicable
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInvertCognosRawAggregateSignageIfApplicable", @"
CREATE PROCEDURE ClientInvertCognosRawAggregateSignageIfApplicable
AS
BEGIN

UPDATE  #CognosRawAggregate
SET     T5_Amount = -T5_Amount,
        T5_TransactionAmount = -T5_TransactionAmount
FROM    #CognosRawAggregate
        INNER JOIN dbo.AccGLAccountDescriptor ON T5_AJ = AJ_PK AND AJ_ReportType = 'COA'
WHERE   AJ_DebitCredit = 'CR'        

END"
							, "DROP PROCEDURE ClientInvertCognosRawAggregateSignageIfApplicable"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientUpdateCognosRawAggregateRecordsWithALTAccounts

		static DatabaseViewAndRoutineCreateScript ClientUpdateCognosRawAggregateRecordsWithALTAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientUpdateCognosRawAggregateRecordsWithALTAccounts", @"
CREATE PROCEDURE ClientUpdateCognosRawAggregateRecordsWithALTAccounts
AS
BEGIN

UPDATE  #CognosRawAggregate
SET     T5_AJ = AlternativeAccount.AJ_PK,
        T5_Amount = CASE WHEN OriginalAccount.AJ_DebitCredit != AlternativeAccount.AJ_DebitCredit THEN -T5_Amount ELSE T5_Amount END,
        T5_TransactionAmount = CASE WHEN OriginalAccount.AJ_DebitCredit != AlternativeAccount.AJ_DebitCredit THEN -T5_TransactionAmount ELSE T5_TransactionAmount END
FROM    #CognosRawAggregate 
        INNER JOIN (
                        SELECT      T5_AJ AS AJ_AccountToBeUpdated
                        FROM        #CognosRawAggregate                        
                        GROUP BY    T5_AJ
                        HAVING      SUM(T5_Amount) < 0
                    ) ToBeUpdated ON T5_AJ = AJ_AccountToBeUpdated
        INNER JOIN dbo.AccGLAccountDescriptor OriginalAccount ON OriginalAccount.AJ_PK = T5_AJ AND OriginalAccount.AJ_ReportType = 'COA'
        INNER JOIN dbo.AccGLAccountDescriptor AlternativeAccount ON OriginalAccount.AJ_AJ_AlternativeNum = AlternativeAccount.AJ_PK

END"
							, "DROP PROCEDURE ClientUpdateCognosRawAggregateRecordsWithALTAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromTTLAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromTTLAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromTTLAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromTTLAccounts(@PnLStartAccount varchar(10), @BSHStartAccount varchar(10), @AccountType char(3))
AS
BEGIN

DECLARE @StartAccount varchar(10)
DECLARE @TotalEndAccount varchar(10)
DECLARE @AccountTypesToInclude TABLE
(
    AccountType char(3)
)

INSERT INTO @AccountTypesToInclude VALUES (@AccountType)
IF (@AccountType = 'P&L')
BEGIN
    SET @StartAccount = @PnLStartAccount
END
ELSE
BEGIN    
    SET @StartAccount = @BSHStartAccount    
    INSERT INTO @AccountTypesToInclude VALUES ('CFW')
    INSERT INTO @AccountTypesToInclude VALUES ('ALT')
END

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT      ToBeInserted.AJ_PK,
            T5_Age,
            T5_CompanyCode, 
            T5_OG_CreditorGroup, 
            T5_OJ_DebtorGroup, 
            T5_Mode,
            T5_Branch,
            T5_BusinessType,
            SUM(CASE
                    WHEN Main.AJ_ReportCategory = 'TTL' THEN 0
                    WHEN @AccountType = 'BSH' AND Main.AJ_DebitCredit != ToBeInserted.AJ_DebitCredit THEN -T5_Amount
                    ELSE T5_Amount
                END) AS Amount,
            T5_TransactionCurrency,
            SUM(CASE
                    WHEN Main.AJ_ReportCategory = 'TTL' THEN 0
                    WHEN @AccountType = 'BSH' AND Main.AJ_DebitCredit != ToBeInserted.AJ_DebitCredit THEN -T5_TransactionAmount
                    ELSE T5_TransactionAmount
                END) AS TransactionAmount,
            T5_Geographical

FROM	    #CognosRawAggregate
            INNER JOIN dbo.AccGLAccountDescriptor Main ON T5_AJ = Main.AJ_PK            
            INNER JOIN dbo.AccGLAccountDescriptor ToBeInserted ON   
                            Main.AJ_LocalAccountNumber < ToBeInserted.AJ_LocalAccountNumber
                            AND Main.AJ_LocalAccountNumber >
                            (
                                ISNULL((SELECT TOP 1	Total.AJ_LocalAccountNumber 
                                        FROM			dbo.AccGLAccountDescriptor Total
                                        WHERE			Total.AJ_TotalLevel >= ToBeInserted.AJ_TotalLevel
	                                                    AND Total.AJ_LocalAccountNumber < ToBeInserted.AJ_LocalAccountNumber
                                        ORDER BY		Total.AJ_LocalAccountNumber DESC),
                                       @StartAccount)
                            )

WHERE	    ToBeInserted.AJ_Language = 'ZZZ' AND ToBeInserted.AJ_ReportType = 'COA'
		    AND ToBeInserted.AJ_ReportCategory = 'TTL'
            AND Main.AJ_ReportCategory COLLATE DATABASE_DEFAULT IN (SELECT AccountType FROM @AccountTypesToInclude)
            AND ((@AccountType = 'BSH' AND ((@BSHStartAccount < @PnLStartAccount AND ToBeInserted.AJ_LocalAccountNumber < @PnLStartAccount) OR  @PnLStartAccount < @BSHStartAccount))
                 OR 
                 (@AccountType = 'P&L' AND ((@PnLStartAccount < @BSHStartAccount AND ToBeInserted.AJ_LocalAccountNumber < @BSHStartAccount) OR  @BSHStartAccount < @PnLStartAccount)))

GROUP BY    ToBeInserted.AJ_PK, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_TransactionCurrency, T5_Geographical

END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromTTLAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts(@PnLStartAccount varchar(10), @BSHStartAccount varchar(10))
AS
BEGIN

-- TTL for P&L Accounts
EXEC ClientInsertIntoCognosRawAggregateTable_GLAggregationFromTTLAccounts @PnLStartAccount, @BSHStartAccount, 'P&L'

-- CFW Account
INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT      CarriedForwardAccount.AJ_PK, 
            T5_Age,
            T5_CompanyCode, 
            T5_OG_CreditorGroup, 
            T5_OJ_DebtorGroup, 
            T5_Mode,
            T5_Branch,
            T5_BusinessType,
            CASE WHEN CarriedForwardAccount.AJ_DebitCredit != MainAccount.AJ_DebitCredit THEN -T5_Amount ELSE T5_Amount END AS Amount,
            T5_TransactionCurrency,
            CASE WHEN CarriedForwardAccount.AJ_DebitCredit != MainAccount.AJ_DebitCredit THEN -T5_TransactionAmount ELSE T5_TransactionAmount END AS TransactionAmount,
            T5_Geographical
FROM	    #CognosRawAggregate
            INNER JOIN dbo.AccGLAccountDescriptor MainAccount ON T5_AJ = MainAccount.AJ_PK AND MainAccount.AJ_ReportType = 'COA'
            INNER JOIN dbo.AccGLAccountDescriptor CarriedForwardAccount ON MainAccount.AJ_AJ_CarriedForwardAccount = CarriedForwardAccount.AJ_PK

-- TTL for BSH Accounts
EXEC ClientInsertIntoCognosRawAggregateTable_GLAggregationFromTTLAccounts @PnLStartAccount, @BSHStartAccount, 'BSH'

END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts
AS
BEGIN

INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT	    ConsolidationAccount.AJ_PK AS AJ, 
            T5_Age,
		    T5_CompanyCode,
            T5_OG_CreditorGroup,
            T5_OJ_DebtorGroup,            
		    T5_Mode,
		    T5_Branch,
		    T5_BusinessType,
		    SUM(CASE WHEN ToBeConsolidated.AJ_DebitCredit != ConsolidationAccount.AJ_DebitCredit THEN -T5_Amount ELSE T5_Amount END) AS Amount,
		    T5_TransactionCurrency,
            SUM(CASE WHEN ToBeConsolidated.AJ_DebitCredit != ConsolidationAccount.AJ_DebitCredit THEN -T5_TransactionAmount ELSE T5_TransactionAmount END) AS TransactionAmount,
            T5_Geographical

FROM        #CognosRawAggregate
            INNER JOIN dbo.AccGLAccountDescriptor ToBeConsolidated ON T5_AJ = ToBeConsolidated.AJ_PK AND ToBeConsolidated.AJ_ReportType = 'COA'
            INNER JOIN dbo.AccGLAccountDescriptor ConsolidationAccount ON ToBeConsolidated.AJ_AJ_ConsolidationNum = ConsolidationAccount.AJ_PK

GROUP BY    ConsolidationAccount.AJ_PK, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_TransactionCurrency, T5_Geographical				

END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts

		static DatabaseViewAndRoutineCreateScript ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts", @"
CREATE PROCEDURE ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts
AS
BEGIN

-- Sub-Classified from Normal Accounts
INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT  T9_AJ, 
        T5_Age, 
        T5_CompanyCode, 
        T5_OG_CreditorGroup, 
        T5_OJ_DebtorGroup, 
        T5_Mode, 
        T5_Branch, 
        T5_BusinessType, 
        CASE WHEN SubClassification.AJ_DebitCredit != Main.AJ_DebitCredit THEN -T5_Amount ELSE T5_Amount END,
        T5_TransactionCurrency, 
        CASE WHEN SubClassification.AJ_DebitCredit != Main.AJ_DebitCredit THEN -T5_TransactionAmount ELSE T5_TransactionAmount END,
        T5_Geographical

FROM    #CognosRawAggregate
        INNER JOIN ClientCognosAccGLAccountDescriptorExtraInfo ON T9_AJ_AccountToBeSubClassified = T5_AJ
        INNER JOIN dbo.AccGLAccountDescriptor Main ON T5_AJ = Main.AJ_PK AND Main.AJ_ReportType = 'COA'
        INNER JOIN dbo.AccGLAccountDescriptor SubClassification ON T9_AJ = SubClassification.AJ_PK AND SubClassification.AJ_ReportType = 'COA'
        LEFT OUTER JOIN ClientCognosSubClassificationAccountDebtorMapping ON T8_T9 = T9_PK AND T8_OJ = T5_OJ_DebtorGroup AND T9_SubClassificationCode = 'DEB'
        LEFT OUTER JOIN ClientCognosSubClassificationAccountCreditorMapping ON T7_T9 = T9_PK AND T7_OG = T5_OG_CreditorGroup AND T9_SubClassificationCode = 'CRD'
WHERE   T8_T9 IS NOT NULL
        OR T7_T9 IS NOT NULL
        OR (T9_SubClassificationCode = 'AGE' AND T9_AccountAge = T5_Age COLLATE DATABASE_DEFAULT)

-- Sub-Classified from Another Sub-Classified Accounts (We're only interested in sub-classifying down to 1 level)
INSERT INTO #CognosRawAggregate (T5_AJ, T5_Age, T5_CompanyCode, T5_OG_CreditorGroup, T5_OJ_DebtorGroup, T5_Mode, T5_Branch, T5_BusinessType, T5_Amount, T5_TransactionCurrency, T5_TransactionAmount, T5_Geographical)
SELECT  T9_AJ,
        T5_Age,
        T5_CompanyCode,
        T5_OG_CreditorGroup,
        T5_OJ_DebtorGroup,
        T5_Mode,
        T5_Branch,
        T5_BusinessType,
        CASE WHEN SubClassification.AJ_DebitCredit != Main.AJ_DebitCredit THEN -T5_Amount ELSE T5_Amount END,
        T5_TransactionCurrency,
        CASE WHEN SubClassification.AJ_DebitCredit != Main.AJ_DebitCredit THEN -T5_TransactionAmount ELSE T5_TransactionAmount END,
        T5_Geographical

FROM    #CognosRawAggregate
        INNER JOIN dbo.AccGLAccountDescriptor Main ON T5_AJ = AJ_PK AND AJ_ReportCategory = 'CCC' AND AJ_ReportType = 'COA'
        INNER JOIN ClientCognosAccGLAccountDescriptorExtraInfo ON T9_AJ_AccountToBeSubClassified = T5_AJ
        INNER JOIN dbo.AccGLAccountDescriptor SubClassification ON T9_AJ = SubClassification.AJ_PK
        LEFT OUTER JOIN ClientCognosSubClassificationAccountDebtorMapping ON T8_T9 = T9_PK AND T8_OJ = T5_OJ_DebtorGroup AND T9_SubClassificationCode = 'DEB'
        LEFT OUTER JOIN ClientCognosSubClassificationAccountCreditorMapping ON T7_T9 = T9_PK AND T7_OG = T5_OG_CreditorGroup AND T9_SubClassificationCode = 'CRD'
WHERE   T8_T9 IS NOT NULL
        OR T7_T9 IS NOT NULL
        OR (T9_SubClassificationCode = 'AGE' AND T9_AccountAge = T5_Age COLLATE DATABASE_DEFAULT)

END"
							, "DROP PROCEDURE ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region PROCEDURE ClientInsertCognosAccountsIntoCognosExportTempTable

		static DatabaseViewAndRoutineCreateScript ClientInsertCognosAccountsIntoCognosExportTempTable
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientInsertCognosAccountsIntoCognosExportTempTable", @"
CREATE PROCEDURE ClientInsertCognosAccountsIntoCognosExportTempTable
AS
BEGIN

INSERT INTO #CognosExport (T6_AJ, T6_CompanyCode, T6_Mode, T6_Branch, T6_BusinessType, T6_Amount, T6_TransactionCurrency, T6_TransactionAmount, T6_Geographical)
SELECT      AJ, CompanyCode, Mode, Branch, BusinessType, SUM(Amount) AS T6_Amount, TransactionCurrency, SUM(TransactionAmount) AS T6_TransactionAmount, Geographical
FROM
(

    SELECT  T5_AJ AS AJ,
            CASE 
                WHEN T4_Company = 'ICT' THEN 'ICTOTA'
                WHEN T4_Company IN ('I/A', 'J') AND T5_CompanyCode IS NOT NULL THEN T5_CompanyCode 
                ELSE '' 
            END AS CompanyCode,
            CASE WHEN T5_Mode IS NOT NULL AND T4_Mode > 0 THEN T5_Mode ELSE '' END AS Mode,
			CASE WHEN T5_Branch IS NOT NULL AND T4_Branch > 0 THEN T5_Branch ELSE '' END AS Branch,
			CASE WHEN T5_BusinessType IS NOT NULL AND T4_BusinessType > 0 THEN T5_BusinessType ELSE '' END AS BusinessType,
            T5_Amount AS Amount,
			CASE WHEN T5_TransactionCurrency IS NOT NULL AND T4_Company = 'J' THEN T5_TransactionCurrency ELSE '' END AS TransactionCurrency,
			CASE WHEN T4_Company = 'J' THEN T5_TransactionAmount ELSE 0 END AS TransactionAmount,
			CASE WHEN T5_Geographical IS NOT NULL AND T4_Geographical > 0 THEN T5_Geographical ELSE '' END AS Geographical

    FROM    #CognosRawAggregate
            INNER JOIN dbo.AccGLAccountDescriptor ON T5_AJ = AJ_PK AND AJ_ReportType = 'COA'
            LEFT OUTER JOIN ClientCognosAccGLAccountDescriptorExtraInfo ON T5_AJ = T9_AJ
            LEFT OUTER JOIN ClientCognosGroupingFlags ON T5_AJ = T4_AJ

    WHERE   (T9_AJ IS NULL OR T9_IsPublished = 'Y')

) DerivedTbl
GROUP BY	AJ, CompanyCode, Mode, Branch, BusinessType, TransactionCurrency, Geographical

END"
							, "DROP PROCEDURE ClientInsertCognosAccountsIntoCognosExportTempTable"
							, DbRoutineType.SqlProcedureTypeDesc);
			}
		}

		#endregion

		#region TABLE ClientCognosAccGLAccountDescriptorExtraInfo

		static DatabaseObjectCreateScript ClientCognosAccGLAccountDescriptorExtraInfo
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientCognosAccGLAccountDescriptorExtraInfo", @"
CREATE TABLE ClientCognosAccGLAccountDescriptorExtraInfo
(
	T9_PK uniqueidentifier NOT NULL CONSTRAINT DF_T9_PK DEFAULT (newid()),
    T9_AJ uniqueidentifier NOT NULL,
    T9_IsPublished char(1) NOT NULL DEFAULT('N'),
    T9_ReconciliationTotalAccount nvarchar(15) NOT NULL DEFAULT(''),
    T9_AJ_AccountToBeSubClassified uniqueidentifier NULL,
    T9_SubClassificationCode varchar(3) NOT NULL DEFAULT(''),
    T9_AccountAge varchar(2) NOT NULL DEFAULT(''),

	CONSTRAINT PK_UX__T9_PK PRIMARY KEY  NONCLUSTERED
	( T9_PK ),
   
    CONSTRAINT ClientCognosAccGLAccountDescriptorExtraInfo_T9_AJ_FK2_AccGLAccountDescriptor_RRR_121 FOREIGN KEY 
	( T9_AJ ) REFERENCES AccGLAccountDescriptor ( AJ_PK ),

    CONSTRAINT ClientCognosAccGLAccountDescriptorExtraInfo_T9_AJ_AccountToBeSubClassified_FK2_AccGLAccountDescriptor_RRR_121 FOREIGN KEY 
	( T9_AJ_AccountToBeSubClassified ) REFERENCES AccGLAccountDescriptor ( AJ_PK ),
)
CREATE UNIQUE INDEX NR_UX__T9_AJ ON ClientCognosAccGLAccountDescriptorExtraInfo (T9_AJ)
",
						"DROP TABLE ClientCognosAccGLAccountDescriptorExtraInfo");
			}
		}

		#endregion

		#region TABLE ClientCognosSubClassificationAccountDebtorMapping

		static DatabaseObjectCreateScript ClientCognosSubClassificationAccountDebtorMapping
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientCognosSubClassificationAccountDebtorMapping", @"
CREATE TABLE ClientCognosSubClassificationAccountDebtorMapping
(
	T8_PK uniqueidentifier NOT NULL CONSTRAINT DF_T8_PK DEFAULT (newid()),
    T8_T9 uniqueidentifier NOT NULL,
    T8_OJ uniqueidentifier NOT NULL,

	CONSTRAINT PK_UX__T8_PK PRIMARY KEY  NONCLUSTERED 
	( T8_PK ),	
   
    CONSTRAINT ClientCognosSubClassificationAccountDebtorMapping_T8_T9_FK2_ClientCognosAccGLAccountDescriptorExtraInfo_RRR_121 FOREIGN KEY 
	( T8_T9 ) REFERENCES ClientCognosAccGLAccountDescriptorExtraInfo ( T9_PK ),

    CONSTRAINT ClientCognosSubClassificationAccountDebtorMapping_T8_OJ_FK2_OrgDebtorGroup_RRR_121 FOREIGN KEY 
	( T8_OJ ) REFERENCES OrgDebtorGroup ( OJ_PK ),
)
CREATE UNIQUE INDEX NR_UX__T8_T9_T8_OJ ON ClientCognosSubClassificationAccountDebtorMapping (T8_T9, T8_OJ)
",
						"DROP TABLE ClientCognosSubClassificationAccountDebtorMapping");
			}
		}

		#endregion

		#region TABLE ClientCognosSubClassificationAccountCreditorMapping

		static DatabaseObjectCreateScript ClientCognosSubClassificationAccountCreditorMapping
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientCognosSubClassificationAccountCreditorMapping", @"
CREATE TABLE ClientCognosSubClassificationAccountCreditorMapping
(
	T7_PK uniqueidentifier NOT NULL CONSTRAINT DF_T7_PK DEFAULT (newid()),
    T7_T9 uniqueidentifier NOT NULL,
    T7_OG uniqueidentifier NOT NULL,

	CONSTRAINT PK_UX__T7_PK PRIMARY KEY  NONCLUSTERED 
	( T7_PK ),	
   
    CONSTRAINT ClientCognosSubClassificationAccountCreditorMapping_T7_T9_FK2_ClientCognosAccGLAccountDescriptorExtraInfo_RRR_121 FOREIGN KEY 
	( T7_T9 ) REFERENCES ClientCognosAccGLAccountDescriptorExtraInfo ( T9_PK ),

    CONSTRAINT ClientCognosSubClassificationAccountCreditorMapping_T7_OG_FK2_OrgCreditorGroup_RRR_121 FOREIGN KEY 
	( T7_OG ) REFERENCES OrgCreditorGroup ( OG_PK ),
)
CREATE UNIQUE INDEX NR_UX__T7_T9_T7_OG ON ClientCognosSubClassificationAccountCreditorMapping (T7_T9, T7_OG)
",
						"DROP TABLE ClientCognosSubClassificationAccountCreditorMapping");
			}
		}

		#endregion

		#region TABLE ClientCognosGroupingFlags

		static DatabaseObjectCreateScript ClientCognosGroupingFlags
		{
			get
			{
				return new DatabaseObjectCreateScript("ClientCognosGroupingFlags", @"
CREATE TABLE ClientCognosGroupingFlags
(
	T4_PK uniqueidentifier NOT NULL CONSTRAINT DF_T4_PK DEFAULT (newid()),
    T4_AJ uniqueidentifier NOT NULL,
    T4_Company      varchar(3) NOT NULL DEFAULT(''),
    T4_Branch       tinyint NOT NULL DEFAULT(0),
    T4_Mode         tinyint NOT NULL DEFAULT(0),
    T4_BusinessType tinyint NOT NULL DEFAULT(0),
    T4_Geographical tinyint NOT NULL DEFAULT(0),
    
	CONSTRAINT PK_UX__T4_PK PRIMARY KEY  NONCLUSTERED
	( T4_PK ),
   
    CONSTRAINT ClientCognosGroupingFlags_T4_AJ_FK2_AccGLAccountDescriptor_RRR_121 FOREIGN KEY
	( T4_AJ ) REFERENCES AccGLAccountDescriptor ( AJ_PK ),
)
CREATE UNIQUE INDEX NR_UX__T4_AJ ON ClientCognosGroupingFlags (T4_AJ)
",
						"DROP TABLE ClientCognosGroupingFlags");
			}
		}

		#endregion

		#region VIEW ClientAccTransactionHeaderWithJobInfo

		static DatabaseViewAndRoutineCreateScript ClientAccTransactionHeaderWithJobInfo
		{
			get
			{
				return new DatabaseViewAndRoutineCreateScript("ClientAccTransactionHeaderWithJobInfo", @"
CREATE VIEW ClientAccTransactionHeaderWithJobInfo AS
SELECT 
	NEWID() AS AH_PK,
	AH_OH,						
	AH_TransactionType, 
	AH_TransactionNum, 
	AH_ConsolidatedInvoiceRef,
	RX_PK AS AH_RX, 
	AH_InvoiceDate,	
	ISNULL(dbo.ClientGetMaturityDateForNetting(JS_PK, JS_E_DEP, JK_PK), AH_DueDate) AS AH_DueDate,	
	AH_Ledger,
	AH_Desc,				
	AH_InvoiceAmount,
	AH_GSTAmount,						
	AH_OSTotal,
	AH_OutstandingAmount,
	AH_OSOutstandingAmount,
	AH_IsOSOutstandingAmountApplicable,
	AH_ExchangeRate,
	AH_GB,
	JK_PK AS AH_JK, 
	JS_PK AS AH_JS						

FROM 
	dbo.AccTransactionHeader 
	INNER JOIN dbo.JobConsol  ON JK_UniqueConsignRef = CASE WHEN CHARINDEX('/', AH_ConsolidatedInvoiceRef) > 0 THEN LEFT(AH_ConsolidatedInvoiceRef, CHARINDEX('/', AH_ConsolidatedInvoiceRef) - 1) ELSE AH_ConsolidatedInvoiceRef END
	INNER JOIN dbo.JobShipment  ON JS_PK = (SELECT TOP 1 JN_JS 
					FROM 
						dbo.JobConShipLink  
						INNER JOIN dbo.JobShipment  ON JN_JS = JS_PK
					WHERE JN_JK = JK_PK
					ORDER BY JS_UniqueConsignRef)
	INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code

WHERE AH_JH IS NULL

UNION ALL

SELECT
	NEWID() AS AH_PK, 
	AH_OH,						
	AH_TransactionType, 
	AH_TransactionNum, 
	AH_ConsolidatedInvoiceRef,
	RX_PK AS AH_RX, 
	AH_InvoiceDate, 
	ISNULL(dbo.ClientGetMaturityDateForNetting(JS_PK, JS_E_DEP, JK_PK), AH_DueDate) AS AH_DueDate,
	AH_Ledger,
	AH_Desc,				
	AH_InvoiceAmount,
	AH_GSTAmount,						
	AH_OSTotal,
	AH_OutstandingAmount,
	AH_OSOutstandingAmount,
	AH_IsOSOutstandingAmountApplicable,
	AH_ExchangeRate,
	AH_GB,
	JK_PK AS AH_JK,
	JS_PK AS AH_JS		

FROM 
	dbo.AccTransactionHeader 
	INNER JOIN dbo.JobHeader  ON AH_JH = JH_PK AND JH_ParentTableCode = 'JS'
	INNER JOIN dbo.JobShipment  ON JH_ParentID = JS_PK
	INNER JOIN dbo.JobConsol  ON JK_PK =	(SELECT TOP 1 JN_JK 
					FROM 
						dbo.JobConShipLink 
						INNER JOIN dbo.JobConsol  ON JN_JK = JK_PK
					WHERE JN_JS = JS_PK
					ORDER BY JK_UniqueConsignRef)
	INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code

UNION ALL

SELECT 
	NEWID() AS AH_PK, 
	AH_OH,						
	AH_TransactionType, 
	AH_TransactionNum, 
	AH_ConsolidatedInvoiceRef,
	RX_PK AS AH_RX, 
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
	AH_GB,
	NULL, NULL		

FROM dbo.AccTransactionHeader
INNER JOIN dbo.RefCurrency ON AH_RX_NKTransactionCurrency = RX_Code
WHERE AH_JH IS NULL 
AND (AH_ConsolidatedInvoiceRef IS NULL
OR AH_ConsolidatedInvoiceRef = '')", "DROP VIEW ClientAccTransactionHeaderWithJobInfo"
														 , DbRoutineType.SqlViewTypeDesc);
			}
		}

		#endregion
	}
}
