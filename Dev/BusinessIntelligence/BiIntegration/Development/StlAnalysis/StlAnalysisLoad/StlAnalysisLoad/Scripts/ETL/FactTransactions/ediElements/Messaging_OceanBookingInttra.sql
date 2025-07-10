-- [Source] CLIENT
-- Transaction and Content Services
---- ediElements
------ Messaging and Electronic Submission
-------- Ocean Booking/SI/Status Messaging (via INTTRA/GTNexus/Other)

DECLARE @StartDateInclusive DATE = '2013-07-01';
DECLARE @EndDateExclusive DATE = '2013-08-01';

SELECT
	CompanyCode = gc.GC_Code,
	TransactionDate = convert(date, em.EM_SystemCreateTimeUtc),
	TransactionCount = count(*) -- TransactionReference = jk.JK_UniqueConsignRef + ' - ' + em.EM_MessageNum
FROM
	JobConsol jk
	INNER JOIN dbo.EDIMessage em on em.EM_LinkUniqueID = jk.JK_PK
	INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = em.EM_GB
	INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
WHERE
	em.EM_SystemCreateTimeUtc >= @StartDateInclusive
	AND em.EM_SystemCreateTimeUtc < @EndDateExclusive
	AND em.EM_ApplicationCode = 'INT'
	AND em.EM_ReceiveTransmit = 'TRX'
GROUP BY
	gc.GC_Code,
	convert(date, em.EM_SystemCreateTimeUtc)
