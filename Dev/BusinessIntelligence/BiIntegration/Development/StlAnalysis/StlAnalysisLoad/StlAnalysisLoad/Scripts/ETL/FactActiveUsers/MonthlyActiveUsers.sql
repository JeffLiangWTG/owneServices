DECLARE @StartDateInclusive DATE = '2013-07-01';
DECLARE @EndDateExclusive DATE = '2013-08-01';

SELECT count(*)
FROM
	GlbStaff gs
	INNER JOIN (
		SELECT distinct SL_GS_NKUser
		FROM dbo.StmALog
		WHERE SL_PostedTimeUtc >= @StartDateInclusive
		AND SL_PostedTimeUtc < @EndDateExclusive
	) sl ON gs.GS_Code = sl.SL_GS_NKUser
WHERE
	cast(gs.GS_IsResource as char(1)) in ('0', 'N')
	AND cast(gs.GS_IsSystemAccount as char(1)) in ('0', 'N')
	-- AND GS_IsOperational = 1

/*
SELECT count(distinct gs.GS_Code)
FROM
	GlbStaff gs
	INNER JOIN dbo.StmALog sl ON sl.SL_GS_NKUser = gs.GS_Code
WHERE
	SL_PostedTimeUtc >= @StartDateInclusive
	AND SL_PostedTimeUtc < @EndDateExclusive
	AND cast(gs.GS_IsResource as char(1)) in ('0', 'N')
	AND cast(gs.GS_IsSystemAccount as char(1)) in ('0', 'N')
	-- AND GS_IsOperational = 1
*/