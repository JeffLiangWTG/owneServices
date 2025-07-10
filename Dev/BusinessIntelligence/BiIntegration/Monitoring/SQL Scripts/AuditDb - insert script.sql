IF OBJECT_ID('tempdb..#AuditPerformanceDetails', 'U') IS NOT NULL
 DROP TABLE #AuditPerformanceDetails;
IF OBJECT_ID('tempdb..#DbList', 'U') IS NOT NULL
 DROP TABLE #DbList;

set nocount on

create table #DbList (AuditDbName VARCHAR(128), IPAddress nvarchar(48), DataSizeReservedGb NUMERIC(12,2), LogSizeReservedGb NUMERIC(12,2), DataSizeUsedGb NUMERIC(12,2), LogSizeUsedGb NUMERIC(12,2), IsEmpty BIT, MainDbSchemaVersion VARCHAR(128), IsIndexReorganizeDurationMissing BIT)
create table #AuditPerformanceDetails (AuditDbName VARCHAR(128), MinTranEndTimeUtc DATETIME, MaxTranEndTimeUtc DATETIME, LastPartitioningDateUtc DATETIME, LastIndexRebuildDateUtc DATETIME, DataLossDetected BIT,
	PartitionCount INT, MinPartitionRangeValue INT, MaxPartitionRangeValue INT, NonPartitionedTableCount INT, IndexReorganizeDuration NUMERIC(9,3), LastLoadDuration NUMERIC(9,3), LastLoadRowCount INT, 
	LastMaxLsnProcessed VARCHAR(22), LtmMaxLsn VARCHAR(22))

DECLARE @IPAddress nvarchar(48)
DECLARE @ServerName as sysname = @@servername

IF charindex('\',@@servername) > 0
	SET @ServerName = left(@@servername, charindex('\',@@servername)-1)

--select @ServerName 
select @IPAddress = i.ip_address from sys.availability_group_listeners l
	join sys.availability_group_listener_ip_addresses i on l.listener_id = i.listener_id
where dns_name like @ServerName + '%'

--Step 1. Get the list of all Audit DBs
INSERT INTO #DbList (AuditDbName, IPAddress, DataSizeReservedGb, LogSizeReservedGb, IsEmpty, IsIndexReorganizeDurationMissing)
SELECT 
	d.name, 
	case when aglip.ip_address is not null then aglip.ip_address else @IPAddress end as IPAddress,
	CAST(SUM((CAST(CASE WHEN mf.type_desc = 'ROWS' THEN mf.Size ELSE 0.0 END AS decimal(10, 0)) / 128 / 1024)) AS decimal(12, 2)) AS DataSize,
	CAST(SUM((CAST(CASE WHEN mf.type_desc = 'LOG' THEN mf.Size ELSE 0.0 END AS decimal(10, 0)) / 128 / 1024)) AS decimal(12, 2)) AS LogSize,
	0 AS IsEmpty, 0 AS IsIndexReorganizeDurationMissing
FROM sys.databases d
	LEFT JOIN [sys].[dm_hadr_database_replica_states] dr with (nolock) ON dr.database_id = d.database_id AND dr.replica_id = d.replica_id
	LEFT JOIN sys.availability_group_listeners agl with(nolock) on agl.group_id = dr.group_id
	LEFT JOIN sys.availability_group_listener_ip_addresses aglip with(nolock) on aglip.listener_id = agl.listener_id
	INNER JOIN sys.master_files mf WITH (NOLOCK) ON d.database_id = mf.database_id
WHERE 
	d.name like '%[_]Audit%'
	and d.state = 0	-- online
	and mf.type_desc IN ('ROWS', 'LOG')
	and ((dr.synchronization_health = 2  and dr.is_primary_replica = 1 ) OR dr.database_id IS NULL)
GROUP BY d.name, d.is_cdc_enabled, aglip.ip_address
ORDER BY d.is_cdc_enabled, d.name

--Step2. Update IsEmpty for empty databases
DECLARE @query NVARCHAR(MAX) = N'set nocount on' + char(13)+char(10)

SELECT 
	@query = @query + 
	N'if not exists (select 1 from [' + AuditDbName + '].sys.tables t with (nolock) inner join [' + AuditDbName + '].sys.schemas s with (nolock) on t.schema_id = s.schema_id where t.name = ''MasterState'' and s.name = ''biadmin'') update #DbList set IsEmpty = 1 where AuditDbName = ''' + AuditDbName + '''' + char(13) + char(10) 
FROM #DbList

EXEC(@query)

--Step 3. Update data / log usage
SET @query = N'set nocount on' + char(13)+char(10);
SELECT
	@query = @query + 
	N'USE [' + AuditDbName + '];' + char(13) + char(10) + 'UPDATE #DbList SET DataSizeUsedGb = Q.DataSizeUsed, LogSizeUsedGb = Q.LogSizeUsed FROM (select SUM(case when df.type = 0 then FILEPROPERTY(name, ''SpaceUsed'') else 0 end) / 128.0 / 1024.0 as DataSizeUsed, SUM(case when df.type = 1 then FILEPROPERTY(name, ''SpaceUsed'') else 0 end) / 128.0 / 1024.0 as LogSizeUsed from sys.database_files df with (nolock)) Q WHERE AuditDbName = ''' + AuditDbName + '''' + char(13) + char(10) 
FROM #DbList

EXEC(@query)


--Step 4. Update MainDbSchemaVersion 

SET @query = N'set nocount on' + char(13)+char(10);
SELECT
	@query = @query + 
	N'UPDATE #DbList ' +
	'SET MainDbSchemaVersion = (SELECT TOP 1 CAST(e.[value] AS VARCHAR(128)) FROM [' + AuditDbName + '].sys.extended_properties e WITH (NOLOCK) WHERE e.class = 0 AND e.name = ''MainDbSchemaVersion'') ' +
	'WHERE AuditDbName = ''' + AuditDbName + '''' + char(13) + char(10) 
FROM #DbList
WHERE isEmpty = 0

EXEC(@query)

--Step 5. Extract Performance Details (Min/Max TranEndTime)
SET @query = N'set nocount on' + char(13)+char(10);

SELECT
	@query = @query + 
	N'if exists (select 1 from [' + AuditDbName + '].sys.tables with (nolock) where name = ''LsnTimeMapping'') ' +
	'insert into #AuditPerformanceDetails (AuditDbName, MinTranEndTimeUtc, MaxTranEndTimeUtc, LtmMaxLsn) ' +
	'select ''' + AuditDbName + ''', MIN(TranEndTimeUtc) AS MinTranEndTimeUtc, MAX(TranEndTimeUtc) AS MaxMinTranEndTimeUtc, CONVERT(VARCHAR(22), MAX(StartLsn), 1) AS LtmMaxLsn ' + 
	'from [' + AuditDbName + '].biadmin.LsnTimeMapping with (nolock)' +
	char(13) + char(10)   
FROM #DbList
WHERE isEmpty = 0

EXEC(@query)

--Step 6. Extract Performance Details (LastPartitioningDateUtc/LastIndexRebuildDateUtc)
SET @query = N'set nocount on' + char(13)+char(10);

SELECT
	@query = @query + 
	N'update #AuditPerformanceDetails ' + 
	'SET LastPartitioningDateUtc = (SELECT TOP 1 ParamValue FROM [' + AuditDbName + '].biadmin.MasterState with (nolock) WHERE ParamName = ''LAST_PARTITION_PURGE_UTC_DT''), ' + 
	'	LastIndexRebuildDateUtc = (SELECT TOP 1 ParamValue FROM [' + AuditDbName + '].biadmin.MasterState with (nolock) WHERE ParamName = ''LAST_INDEX_REBUILD_UTC_DT''), ' + 
	'	LastMaxLsnProcessed = (SELECT TOP 1 ParamValue FROM [' + AuditDbName + '].biadmin.MasterState with (nolock) WHERE ParamName = ''LAST_MAX_LSN_PROCESSED'') ' + 
	'where AuditDbName = ''' + AuditDbName + '''' +	char(13) + char(10)   
FROM #DbList
WHERE isEmpty = 0

EXEC(@query)

--Step 7. Extract Performance Details (DataLossDetected)
SET @query = N'set nocount on' + char(13)+char(10);

SELECT
	@query = @query + 
	N'if exists (select 1 from [' + AuditDbName + '].sys.tables with (nolock) where name = ''DataLossLog'') ' +
	'if exists(select 1 from [' + AuditDbName + '].biadmin.DataLossLog with (nolock)) ' +
	'update #AuditPerformanceDetails ' + 
	'SET DataLossDetected = 1 ' + 
	'where AuditDbName = ''' + AuditDbName + '''' +	char(13) + char(10)   
FROM #DbList
WHERE isEmpty = 0

EXEC(@query)

--Step 8. Extract Partitioning Details (PartitionCount, MinPartitionRangeValue, MaxPartitionRangeValue, NonPartitionedTableCount)
SET @query = N'set nocount on' + char(13)+char(10);

SELECT
	@query = @query + 
	N'update apd ' + 
	'SET PartitionCount = Q.NumberOfPartition, MinPartitionRangeValue = Q.MinPartitionRangeValue, MaxPartitionRangeValue = Q.MaxPartitionRangeValue, ' +
	'	NonPartitionedTableCount = NT.NonPartitionedTableCount ' + 
	'FROM #AuditPerformanceDetails apd ' +
	'	CROSS JOIN ' + 
	'	(SELECT NULLIF(COUNT(*), 0) + 2 as NumberOfPartition, MIN(CAST(prv.value AS INT)) AS MinPartitionRangeValue, MAX(CAST(prv.value AS INT)) AS MaxPartitionRangeValue ' +
	'	FROM [' + AuditDbName + '].sys.partition_functions f with (nolock) ' +
	'		INNER JOIN [' + AuditDbName + '].sys.partition_schemes s with (nolock) ON s.function_id = f.function_id ' +
	'		INNER JOIN [' + AuditDbName + '].sys.partition_range_values prv with (nolock) ON prv.function_id = f.function_id ' +
	'	WHERE f.name = ''PF_LsnPeriodFunction'' AND prv.value < 10000) Q ' +
	'	CROSS JOIN ' + 
	'	(SELECT COUNT(*) AS NonPartitionedTableCount ' + 
	'	FROM [' + AuditDbName + '].biadmin.TableConfiguration tc with (nolock) ' + 
	'	LEFT JOIN ' + 
	'		(SELECT s.name + ''.'' + t.name AS FullTableName ' + 
	'		FROM [' + AuditDbName + '].sys.TABLES t with (nolock) ' + 
	'			INNER JOIN [' + AuditDbName + '].sys.schemas s with (nolock) ON s.schema_id = t.schema_id ' + 
	'			INNER JOIN [' + AuditDbName + '].sys.indexes i with (nolock) ON t.object_id = i.object_id ' + 
	'			INNER JOIN [' + AuditDbName + '].sys.partition_schemes ps with (nolock) ON i.data_space_id = ps.data_space_id ' + 
	'		WHERE ps.name = ''PS_AuditPartitionScheme'' AND i.type = 5) Q ON Q.FullTableName = tc.SourceSchemaName + ''.'' + tc.SourceTableName ' + 
	'	WHERE Q.FullTableName IS NULL) NT ' +
	
	'where apd.AuditDbName = ''' + AuditDbName + '''' +	char(13) + char(10)   
FROM #DbList
WHERE isEmpty = 0

EXEC(@query)

--Step 9.1 Exclude DBs with missing IndexReorganizeDurationMs
SET @query = N'set nocount on' + char(13)+char(10);

SELECT
	@query = @query + 
	N'if not exists (SELECT 1 FROM [' + AuditDbName + '].sys.tables t with (nolock) ' +
	'		inner join [' + AuditDbName + '].sys.schemas s with (nolock) ON s.schema_id = t.schema_id AND s.name = ''biadmin'' ' +
	'		inner join [' + AuditDbName + '].sys.columns c with (nolock) ON c.object_id = t.object_id AND c.name = ''IndexReorganizeDurationMs'' '+
	'	where t.name = ''TableState'') ' +
	'update #DbList ' +
	'set IsIndexReorganizeDurationMissing = 1 ' +
	'where AuditDbName = ''' + AuditDbName + '''' +	char(13) + char(10)   
FROM #DbList
WHERE isEmpty = 0

EXEC(@query)


--Step 9.2 Extract Permormance details (IndexReorganizeDuration,  LastLoadDuration, LastLoadRowCount)
SET @query = N'set nocount on' + char(13)+char(10);

SELECT
	@query = @query + 
	N'update apd ' + 
	'SET IndexReorganizeDuration = IR.IndexReorganizeDuration, ' + 
	'	LastLoadDuration = IR.LastLoadDuration, ' + 
	'	LastLoadRowCount = IR.LastLoadRowCount ' + 
	'FROM #AuditPerformanceDetails apd ' +
	'	CROSS JOIN ' + 
	'	(SELECT CAST(SUM(ISNULL(IndexReorganizeDurationMs, 0)) / 1000.0 AS NUMERIC(9,3)) AS IndexReorganizeDuration, ' +
	'		CAST(SUM(ISNULL(LoadDurationMS, 0)) / 1000.0 AS NUMERIC(9,3)) AS LastLoadDuration, ' +
	'		SUM(ISNULL(LoadRecordCount, 0)) AS LastLoadRowCount ' +
	'	FROM [' + AuditDbName + '].[biadmin].[TableState] ts with (nolock)) IR ' +
	'where apd.AuditDbName = ''' + AuditDbName + '''' +	char(13) + char(10)
FROM #DbList
WHERE isEmpty = 0 and IsIndexReorganizeDurationMissing = 0

EXEC(@query)


--Results
DECLARE @fqdn varchar(255);
	EXEC xp_getnetname @fqdn output, 1;
declare @RecordedTime datetime = getutcdate()

SELECT 
	CONVERT(varchar, @RecordedTime,120) as RecordedTime
	,@fqdn as FQDN 
	,@@SERVICENAME as InstanceName
	,SERVERPROPERTY('ProductBuild') 'ProductBuild',
	dl.AuditDbName, 
	dl.IPAddress,
	dl.DataSizeReservedGb,
	dl.LogSizeReservedGb,
	dl.DataSizeUsedGb,
	dl.LogSizeUsedGb,
	dl.IsEmpty,
	dl.MainDbSchemaVersion,
	dl.IsIndexReorganizeDurationMissing,
	pd.MinTranEndTimeUtc,
	pd.MaxTranEndTimeUtc,
	pd.LastMaxLsnProcessed,
	pd.LtmMaxLsn,
	pd.LastPartitioningDateUtc,
	pd.LastIndexRebuildDateUtc,
	pd.DataLossDetected,
	pd.PartitionCount,
	pd.MinPartitionRangeValue,
	pd.MaxPartitionRangeValue,
	pd.NonPartitionedTableCount,
	pd.IndexReorganizeDuration,
	pd.LastLoadDuration,
	pd.LastLoadRowCount,
	CASE
		WHEN CONVERT(VARCHAR(128), SERVERPROPERTY ('productversion')) like '13%' THEN 'SQL2016'
		WHEN CONVERT(VARCHAR(128), SERVERPROPERTY ('productversion')) like '14%' THEN 'SQL2017'
		WHEN CONVERT(VARCHAR(128), SERVERPROPERTY ('productversion')) like '15%' THEN 'SQL2019'
		ELSE 'Unknown'
	END + ' ' + CONVERT(VARCHAR(128), SERVERPROPERTY('ProductLevel')) + ' ' + CONVERT(VARCHAR(128), SERVERPROPERTY('ProductUpdateLevel')) AS SqlServerVersion,
	CONVERT(VARCHAR(128), SERVERPROPERTY('ProductVersion')) AS SqlServerProductVersion
FROM #DbList dl
	LEFT JOIN #AuditPerformanceDetails pd ON pd.AuditDbName = dl.AuditDbName
