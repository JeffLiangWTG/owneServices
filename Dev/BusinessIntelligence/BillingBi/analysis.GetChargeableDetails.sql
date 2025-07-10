CREATE PROCEDURE [analysis].[GetChargeableDetails]

@BillingPeriod VARCHAR(MAX),
@CodeFeature VARCHAR(MAX),
@PriceItemCode VARCHAR(MAX),
@Category VARCHAR(MAX),
@DatabaseNumber VARCHAR(MAX),
@ClientCompanyPK VARCHAR(MAX),
@IsoWeek VARCHAR(7) = NULL --YYYYWnn

AS

BEGIN

SET NOCOUNT ON;

DECLARE @BillingPeriodID INT = 0
DECLARE @PriceItemCodeID CHAR(3), @CategoryID CHAR(3), @DatabaseNumberID INT, @ClientCompanyID UNIQUEIDENTIFIER, @CompanyNumberID SMALLINT

IF ISNUMERIC(@BillingPeriod) = 1
	SET @BillingPeriodID = CAST(@BillingPeriod AS INT)

IF @CodeFeature <> '0'
BEGIN
	SELECT TOP 1 @PriceItemCodeID = v.PriceItemCode, @CategoryID = v.Category 
	FROM [analysis].[vwPriceItem] v
	WHERE v.CodeFeature = @CodeFeature
END ELSE
BEGIN
	SET @PriceItemCodeID = @PriceItemCode
	SET @CategoryID = @Category
END

IF @DatabaseNumber <> '0' AND ISNUMERIC(@DatabaseNumber) = 1
BEGIN
	SET @DatabaseNumberID = CAST(@DatabaseNumber AS INT)
END

IF TRY_CONVERT(UNIQUEIDENTIFIER, @ClientCompanyPK) IS NOT NULL
	SET @ClientCompanyID = CAST(@ClientCompanyPK AS UNIQUEIDENTIFIER)

SELECT TOP 1 @DatabaseNumberID = cc.DatabaseNumber, @CompanyNumberID = cc.CompanyNumber
FROM edi.ClientCompany cc
WHERE cc.LCC_PK = @ClientCompanyID


DECLARE @BillingPeriodFrom INT, @BillingPeriodTo INT
DECLARE @EndOfIsoWeekDate DATE, @StartOfIsoWeekDate DATE

SET @BillingPeriodFrom = @BillingPeriodID
SET @BillingPeriodTo = @BillingPeriodID
IF @IsoWeek = '0' SET @IsoWeek = ''
SET @IsoWeek = ISNULL(@IsoWeek, '')

IF @IsoWeek <> ''
BEGIN
	
	SELECT @EndOfIsoWeekDate = EndOfIsoWeekDate
	FROM [analysis].[BillingIsoWeekDimension]
	WHERE IsoWeek = @IsoWeek

	SET @StartOfIsoWeekDate = DATEADD(dd, -6, @EndOfIsoWeekDate)

	IF @BillingPeriod = '0'
	BEGIN
		SET @BillingPeriodFrom = (SELECT [Value] FROM [edi].[GetBillingPeriod] (@StartOfIsoWeekDate))
		SET @BillingPeriodTo = (SELECT [Value] FROM [edi].[GetBillingPeriod] (@EndOfIsoWeekDate))
	END

END

SELECT TOP 10000
	ch.[CH_Period] AS BillingPeriod,
	ch.[CH_Category] AS Category,
	ch.[CH_PriceItemCode] AS PriceItemCode,
	ch.[CH_ReportingSource] AS ReportingSource,
	ch.[CH_ClientStaffCode] AS ClientStaffCode,
	ch.[CH_ClientID] AS LicenceClientKey,
	ch.[CH_ClientNumber] AS ClientNumber,
	ch.[CH_BillableCount] AS BillableCount,
	ch.[CH_Reference1] AS Reference1,
	ch.[CH_Reference2] AS Reference2,
	ch.[CH_Reference3] AS Reference3,
	ch.[CH_Reference4] AS Reference4,
	ch.[CH_Reference5] AS Reference5,
	ch.[CH_Branch] AS Branch,
	CAST(AuSydServiceDateTime.Value AS DATE) AS DateServiceOccurred,
	CAST([CH_ServiceOccuredUtc] AS DATE) AS DateServiceOccurredUTC,
	CAST(CASE 
			WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) > 1 AND DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) < 52 THEN YEAR(ch.CH_ServiceOccuredUtc)
			WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) = 1 AND MONTH(ch.CH_ServiceOccuredUtc) = 1 THEN YEAR(ch.CH_ServiceOccuredUtc)
			WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) = 1 AND MONTH(ch.CH_ServiceOccuredUtc) = 12 THEN YEAR(ch.CH_ServiceOccuredUtc)+1
			WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) >= 52 AND MONTH(ch.CH_ServiceOccuredUtc) = 1 THEN YEAR(ch.CH_ServiceOccuredUtc)-1
			WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) >= 52 AND MONTH(ch.CH_ServiceOccuredUtc) = 12 THEN YEAR(ch.CH_ServiceOccuredUtc)
		END AS VARCHAR(4)) + 'W' + CASE WHEN DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) < 10 THEN '0' ELSE '' END + CAST(DATEPART(ISO_WEEK, ch.CH_ServiceOccuredUtc) AS VARCHAR(2)) AS IsoWeek

FROM [edi].[Chargeable] ch 
	CROSS APPLY [dbo].[AuSydTime](ch.CH_ServiceOccuredUTC) AS AuSydServiceDateTime
WHERE ch.CH_Period >= @BillingPeriodFrom AND ch.CH_Period <= @BillingPeriodTo
	AND (@IsoWeek =  '' OR ([CH_ServiceOccuredUtc] >= @StartOfIsoWeekDate AND [CH_ServiceOccuredUtc] < DATEADD(dd, 1, @EndOfIsoWeekDate)))
	AND ch.CH_Category = @CategoryID
	AND ch.CH_PriceItemCode = @PriceItemCodeID
	AND (@DatabaseNumberID IS NULL OR ch.CH_DatabaseNumber = @DatabaseNumberID)
	AND (@CompanyNumberID IS NULL OR ch.CH_CompanyNumber = @CompanyNumberID)

END