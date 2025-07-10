-- [Source] EDIFAX
-- ediEnterprise Core Engine
---- ediCore Extension Pack 1
------ Communications Engine
-------- Fax Gateway

DECLARE @StartDateInclusive DATE = '2014-02-01';
DECLARE @EndDateExclusive DATE = '2014-03-01';
DECLARE @EnterpriseCode char(3) = 'MAL';
DECLARE @DbServerCode char(3) = 'MEM';

SELECT
	CompanyCode = FaxJobs.CompanyCode,
	TransactionDate = convert(date, FaxJobs.ReceivedDateTime),
	TransactionCount = sum (
		CASE 
			WHEN FaxRecipients.AckSuccess = 1 THEN FaxJobs.[PageCount]
			WHEN FaxBilling.[Status] = 0 AND FaxBilling.Price > 0 THEN FaxBilling.Pages 
			ELSE 0
		END)
FROM
	EDIFaxDB.dbo.FaxJobs WITH (NOLOCK)
	INNER JOIN EDIFaxDB.dbo.FaxRecipients WITH (NOLOCK) ON FaxJobs.FaxJobId = FaxRecipients.FaxJobId
	LEFT JOIN EDIFaxDB.dbo.FaxBilling WITH (NOLOCK) ON FaxRecipients.FaxRecipientID = FaxBilling.ChargeCode
WHERE
	FaxJobs.EnterpriseCode = @EnterpriseCode
	AND FaxJobs.ServerCode = @DbServerCode
	AND FaxJobs.ReceivedDateTime >= @StartDateInclusive
	AND FaxJobs.ReceivedDateTime < @EndDateExclusive
	AND FaxRecipients.IsAcknowledged = 1
	AND (FaxRecipients.AckSuccess = 1 OR FaxBilling.Price > 0)
GROUP BY
	FaxJobs.CompanyCode,
	convert(date, FaxJobs.ReceivedDateTime)
