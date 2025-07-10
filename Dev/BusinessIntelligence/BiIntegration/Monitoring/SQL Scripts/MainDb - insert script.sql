IF OBJECT_ID('tempdb..#DbListWithDwString', 'U') IS NOT NULL
 DROP TABLE #DbListWithDwString;
IF OBJECT_ID('tempdb..#DbList', 'U') IS NOT NULL
 DROP TABLE #DbList;
IF OBJECT_ID('tempdb..#CdcDbDetails', 'U') IS NOT NULL
 DROP TABLE #CdcDbDetails;
IF OBJECT_ID('tempdb..#DbListWithAppDetails', 'U') IS NOT NULL
 DROP TABLE #DbListWithAppDetails;
IF OBJECT_ID('tempdb..#StmData', 'U') IS NOT NULL
 DROP TABLE #StmData; 
IF OBJECT_ID('tempdb..#ServiceTaskData', 'U') IS NOT NULL
 DROP TABLE #ServiceTaskData; 


set nocount on

create table #DbListWithDwString (MainDbName VARCHAR(128) PRIMARY KEY, DwServerName VARCHAR(128), AuditServerName VARCHAR(128), DbSchemaVerion VARCHAR(128), DbMinorSchemaVerion VARCHAR(128), RetentionPeriod INT, BIRESETCHANGEDATACAPTURE VARCHAR(128)) 
create table #DbListWithAppDetails (MainDbName VARCHAR(128) PRIMARY KEY, AppVersion NVARCHAR(128), PackageBuiltDate DATETIME)
create table #DbList (MainDbName VARCHAR(128), IsCdcEnabled BIT, IPAddress nvarchar(48), DataSizeReservedGb NUMERIC(12,2), LogSizeReservedGb NUMERIC(12,2), DataSizeUsedGb NUMERIC(12,2), LogSizeUsedGb NUMERIC(12,2), EdwEnabledDataSizeUsedGb NUMERIC(12,2))
create table #CdcDbDetails (MainDbName VARCHAR(128), CdcEnabledUtcDate DATETIME, EarliestCapturedLsnUtcDateTime DATETIME, LatestCapturedLsnUtcDateTime DATETIME, LtmMaxLsn VARCHAR(22))
create table #StmData (MainDbName VARCHAR(128), KeyName VARCHAR(300), KeyValue NVARCHAR(MAX))
create table #ServiceTaskData (MainDbName VARCHAR(128), XmlData XML)

DECLARE @IPAddress nvarchar(48)
DECLARE @ServerName as sysname

DECLARE @EdwEnabledTableList VARCHAR(MAX) = ',AccChargeCode,AccGLHeader,AccGroups,AccInvMsg,AccTaxOverrideGroup,AccTaxRate,AccTransactionHeader,AccTransactionLines,AccWithholding,CusContainer,CusEntryHeader,CusEntryNum,CusHAWB,CusMAWB,CusSCAHouse,CusSCAOceanBill,CusStatementHeader,CusUnderbond,CYBooking,CYContainer,CYContainerBooking,DtbBooking,DtbBookingConsolidation,ExportCustomsManifestHeader,ExportCustomsManifestLines,GlbBranch,GlbCapability,GlbCompany,GlbDepartment,GlbDevice,GlbDeviceAssignmentDivot,GlbDeviceBattery,GlbDeviceExternalVoltage,GlbDeviceIgnition,GlbDeviceLocation,GlbDeviceLog,GlbDeviceOdometer,GlbDeviceOnboardMass,GlbDeviceTemperature,GlbDeviceTyreAlert,GlbDeviceTyreReport,GlbGroup,GlbGroupLink,GlbPerson,GlbStaff,JobCartage,JobChargeRevRecognition,JobComInvoiceLine,JobConShipLink,JobConsol,JobConsolTransport,JobContainer,JobContainerDetention,JobContainerLegs,JobContainerMove,JobContainerPackPivot,JobDeclaration,JobDocAddress,JobDocsAndCartage,JobHeader,JobMawb,JobOrderContainer,JobOrderHeader,JobOrderItem,JobOrderLine,JobOrderLineDeliverContainer,JobOrderLineDelivery,JobPackLines,JobSailing,JobShipment,JobShipmentPreplanning,JobSlotAllocation,JobStorage,JobTradeLaneVoyage,JobVoyAccount,JobVoyage,JobVoyDestination,JobVoyOrigin,LocalCartageJobLegType,LocalCartageJobOrg,LocalCartageJobType,OrgAddress,OrgAddressCapability,OrgContact,OrgCusCode,OrgDepartment,OrgHeader,OrgRelatedParty,OrgSupBuyLinkTrnMode,ProcessHeader,ProcessHeaderLink,ProcessTaskIterationLink,ProcessTasks,RefCarrierConsortium,RefContainer,RefContainerStock,RefCountry,RefCountryStates,RefCurrency,RefEquipment,RefServiceLevel,RefUNLOCO,RefDatabase_RefUNLOCOUtcOffset,RefVessel,RefZoneHeader,TelSubEquipment,UNDGDataItem,ZZUNDGSubstance,WhsArea,WhsDocket,WhsLocation,WhsLocationType,WhsStocktake,WhsWarehouse,WorkItem,'

IF charindex('\',@@servername) > 0
	SET @ServerName = left(@@servername, charindex('\',@@servername)-1)
--select @ServerName 
select @IPAddress = i.ip_address from sys.availability_group_listeners l
join sys.availability_group_listener_ip_addresses i on l.listener_id = i.listener_id
where dns_name like @ServerName + '%'

--Step 1. Get the list of all DBs
INSERT INTO #DbList (MainDbName, IsCdcEnabled, IPAddress, DataSizeReservedGb, LogSizeReservedGb)
SELECT 
	d.name, 
	d.is_cdc_enabled,
	case when aglip.ip_address is not null then aglip.ip_address
	else @IPAddress end as IPAddress, 
	CAST(SUM((CAST(CASE WHEN mf.type_desc = 'ROWS' THEN mf.Size ELSE 0.0 END AS decimal(10, 0)) / 128 / 1024)) AS decimal(12, 2)) AS DataSize,
	CAST(SUM((CAST(CASE WHEN mf.type_desc = 'LOG' THEN mf.Size ELSE 0.0 END AS decimal(10, 0)) / 128 / 1024)) AS decimal(12, 2)) AS LogSize
FROM sys.databases d
	INNER JOIN sys.master_files mf WITH (NOLOCK) ON d.database_id = mf.database_id
	LEFT JOIN [sys].[dm_hadr_database_replica_states] dr with (nolock) ON dr.database_id = d.database_id AND dr.replica_id = d.replica_id
	LEFT JOIN sys.availability_group_listeners agl with(nolock) on agl.group_id = dr.group_id
	LEFT JOIN sys.availability_group_listener_ip_addresses aglip with(nolock) on aglip.listener_id = agl.listener_id
WHERE 
	d.name like 'Odyssey______'  --'Odyssey%'--
	and not d.name like '%[_-]%'
	and d.state = 0	-- online
	and mf.type_desc IN ('ROWS', 'LOG')
	and ((dr.synchronization_health = 2  and dr.is_primary_replica = 1 ) OR dr.database_id IS NULL)
GROUP BY d.name, d.is_cdc_enabled, aglip.ip_address
ORDER BY d.is_cdc_enabled, d.name

--Step2. Delete databases not related to CW main DB
DECLARE @query NVARCHAR(MAX) = N'set nocount on' + char(13)+char(10)

SELECT 
	@query = @query + 
	N'if not exists (select 1 from [' + MainDbName + '].sys.tables with (nolock) where name in (''StmALog'', ''JobHeader'')) delete from #DbList where MainDbName = ''' + MainDbName + '''' + char(13) + char(10) --+ 'PRINT CONVERT(VARCHAR(25), GETDATE(), 121) + '': ' + MainDbName + '''' + char(13) + char(10)
FROM #DbList

EXEC(@query)

--Step 3.1 Update data / log usage
SET @query = N'set nocount on' + char(13)+char(10);
SELECT
	@query = @query + 
	N'USE [' + MainDbName + '];' + char(13) + char(10) + 'UPDATE #DbList SET DataSizeUsedGb = Q.DataSizeUsed, LogSizeUsedGb = Q.LogSizeUsed FROM (select SUM(case when df.type = 0 then FILEPROPERTY(name, ''SpaceUsed'') else 0 end) / 128.0 / 1024.0 as DataSizeUsed, SUM(case when df.type = 1 then FILEPROPERTY(name, ''SpaceUsed'') else 0 end) / 128.0 / 1024.0 as LogSizeUsed from sys.database_files df with (nolock)) Q WHERE MainDbName = ''' + MainDbName + '''' + char(13) + char(10) 
FROM #DbList

EXEC(@query)

--Step 3.2 Update data usage by tables enabled for EDW  (for performance reasons, run it on Sat/Sun only - Syd time)
DECLARE @DW INT = DATEPART(DW, GETDATE())
IF (@DW = 7 OR @DW = 1)
BEGIN
	SET @query = N'SET NOCOUNT ON' + char(13)+char(10)
	SET @query = @query + 'DECLARE @EdwEnabledTableList VARCHAR(MAX) = ''' + @EdwEnabledTableList + '''' + char(13)+char(10)
	SELECT
		@query = @query + 
		N'UPDATE #DbList ' + 
		'SET EdwEnabledDataSizeUsedGb = Q.Used_GB ' + 
		'FROM (SELECT CAST(ROUND(SUM(a.used_pages) / 128.00 / 1024, 2) AS NUMERIC(12, 2)) AS Used_GB ' +
		'FROM [' + MainDbName + '].sys.tables t with (nolock) ' +
		'	INNER JOIN [' + MainDbName + '].sys.indexes i with (nolock) ON t.OBJECT_ID = i.object_id ' +
		'	INNER JOIN [' + MainDbName + '].sys.partitions p with (nolock) ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id ' +
		'	INNER JOIN [' + MainDbName + '].sys.allocation_units a with (nolock) ON p.partition_id = a.container_id ' +
		'	INNER JOIN [' + MainDbName + '].sys.schemas s with (nolock) ON t.schema_id = s.schema_id ' +
		'WHERE @EdwEnabledTableList LIKE ''%,'' + t.name + '',%'' AND s.name = ''dbo'') Q ' +
		'WHERE MainDbName = ''' + MainDbName + '''' + char(13) + char(10) 
	FROM #DbList

	EXEC(@query)
END

--Step 4. Extract StmData to get DB Schema version and Audit Retention Period
SET @query = N'set nocount on' + char(13)+char(10);

SELECT
	@query = @query + 
	N'if exists (select 1 from [' + MainDbName + '].sys.tables with (nolock) where name = ''StmData'') insert into #StmData (MainDbName, KeyName, KeyValue) select ''' +
		MainDbName + ''', SD_Name, CONVERT(NVARCHAR(MAX), CAST(SD_BinaryValue AS VARBINARY(MAX))) from [' + 
		MainDbName + '].dbo.StmData with (nolock) where SD_Name IN (''BiServers'', ''DATABASE_SCHEMA_VERSION'', ''DATABASE_MINOR_SCHEMA_VERSION'', ''AuditRetentionPeriod'', ''BiDataWarehouseServer'', ''BiAuditServer'', ''BIRESETCHANGEDATACAPTURE'')' + char(13) + char(10)   
FROM #DbList

EXEC(@query)

--Step 5. Extract Service Task Data
SET @query = N'set nocount on' + char(13)+char(10);

SELECT
	@query = @query + 
	N'if exists (select 1 from [' + MainDbName + '].sys.tables with (nolock) where name = ''StmScheduleTask'') insert into #ServiceTaskData (MainDbName, XmlData) select ''' +
		MainDbName + ''', (select s5_scheduletype as "@ServiceTaskName", * from [' + 
		MainDbName + '].dbo.StmScheduleTask with (nolock) WHERE s5_typeofdocument = ''BI'' OR s5_scheduletype = ''UPG'' ' +
		' FOR XML PATH(''ServiceTask''))' + char(13) + char(10)   
FROM #DbList

EXEC(@query)

--Step 6. decode DW name
UPDATE #StmData
SET KeyValue = convert(XML, convert(NVARCHAR(max), KeyValue, 0)).value('(/BiServerInfo/DataWarehouseServer)[1]', 'NVARCHAR(max)')
Where KeyName = 'BiServers' AND KeyValue IS NOT NULL

--Step 7. Transpose StmData rows
INSERT INTO #DbListWithDwString (MainDbName, DbSchemaVerion, DbMinorSchemaVerion, DwServerName, RetentionPeriod, AuditServerName, BIRESETCHANGEDATACAPTURE)
SELECT MainDbName, [DATABASE_SCHEMA_VERSION],[DATABASE_MINOR_SCHEMA_VERSION],ISNULL([BiDataWarehouseServer], [BiServers]),[AuditRetentionPeriod], [BiAuditServer], [BIRESETCHANGEDATACAPTURE]
FROM #StmData
PIVOT(MIN(KeyValue) for KeyName in ([DATABASE_SCHEMA_VERSION],[DATABASE_MINOR_SCHEMA_VERSION],[BiServers],[AuditRetentionPeriod],[BiDataWarehouseServer],[BiAuditServer],[BIRESETCHANGEDATACAPTURE])) AS Q

--Step 8. Extract AppVersion and Package Built Date from dbo.StmUpgrade
SET @query = N'set nocount on' + char(13)+char(10)

SELECT 
	@query = @query + 
	N'if exists (select 1 from [' + MainDbName + '].sys.tables with (nolock) where name = ''StmUpgrade'') insert into #DbListWithAppDetails (MainDbName, AppVersion, PackageBuiltDate) select ''' +
		MainDbName + ''', CAST(SZ_MajorVersion AS NVARCHAR(10)) + N''.'' + CAST(SZ_MinorVersion AS NVARCHAR(10)) + N''.'' + CAST(SZ_Release AS NVARCHAR(10)) + ''.'' + CAST(SZ_Patch AS NVARCHAR(10)), ' + 
		'SZ_ExeVersionDate from [' + MainDbName + '].dbo.StmUpgrade with (nolock) where sz_status = ''CUR''' + char(13) + char(10)   
FROM #DbList

EXEC(@query)

--Step 9.  Extract CDC information (Cdc Enabled Utc Date, EarliestCapturedLsnUtcDateTime, LatestCapturedLsnUtcDateTime)
SET @query = N'set nocount on' + char(13)+char(10)

SELECT 
	@query = @query + 
	N'insert into #CdcDbDetails (MainDbName, CdcEnabledUtcDate, EarliestCapturedLsnUtcDateTime, LatestCapturedLsnUtcDateTime, LtmMaxLsn) ' +
	N'select ''' + MainDbName + ''', t.create_date, q.min_tran_end_time, q.max_tran_end_time, q.LtmMaxLsn ' +
	N'from [' + MainDbName + '].sys.tables t with (nolock) ' +
	N'inner join [' + MainDbName + '].sys.schemas s with (nolock) on s.schema_id = t.schema_id ' +
	N'cross join (select min(tran_end_time) as min_tran_end_time, max(tran_end_time) as max_tran_end_time, CONVERT(VARCHAR(22), max(start_lsn), 1) as LtmMaxLsn from [' + MainDbName + '].cdc.lsn_time_mapping with (nolock)) q ' +
	N'where t.name = ''change_tables'' and s.name = ''cdc''' + char(13) + char(10)
FROM #DbList
WHERE IsCdcEnabled = 1

EXEC(@query)

--Step 10. Populate / UPDATE global monitoring table (Table 1)
DECLARE @TimeOffset int = DATEDIFF(hh, GETUTCDATE(), GETDATE())

--DELETE FROM ....  WHERE ServerName = ....

--INSERT INTO ....


DECLARE @fqdn varchar(255);
	EXEC xp_getnetname @fqdn output, 1;
declare @RecordedTime datetime = getutcdate()

SELECT 
	CONVERT(varchar, @RecordedTime,120) as RecordedTime
	,@fqdn as FQDN 
	,@@SERVICENAME as InstanceName
	,SERVERPROPERTY('ProductBuild') 'ProductBuild',
	dl.MainDbName, 
	dl.IPAddress,
	dl.IsCdcEnabled, 
	dl.DataSizeReservedGb, 
	dl.LogSizeReservedGb,
	dl.DataSizeUsedGb, 
	dl.LogSizeUsedGb,
	dldw.DwServerName, 
	DATEADD(hh, -@TimeOffset, cd.CdcEnabledUtcDate) as CdcEnabledUtcDate,
	DATEADD(hh, -@TimeOffset, cd.EarliestCapturedLsnUtcDateTime) as EarliestCapturedLsnUtcDateTime,
	DATEADD(hh, -@TimeOffset, cd.LatestCapturedLsnUtcDateTime) as LatestCapturedLsnUtcDateTime,
	dldw.DbSchemaVerion + N'.' + ISNULL(dldw.DbMinorSchemaVerion, N'') AS DbVersion,
	dlad.AppVersion,
	dlad.PackageBuiltDate,
	dldw.RetentionPeriod as AuditRetentionPeriod,
	cd.LtmMaxLsn,
	dl.EdwEnabledDataSizeUsedGb,
	CASE
		WHEN CONVERT(VARCHAR(128), SERVERPROPERTY ('productversion')) like '13%' THEN 'SQL2016'
		WHEN CONVERT(VARCHAR(128), SERVERPROPERTY ('productversion')) like '14%' THEN 'SQL2017'
		WHEN CONVERT(VARCHAR(128), SERVERPROPERTY ('productversion')) like '15%' THEN 'SQL2019'
		ELSE 'Unknown'
	END + ' ' + CONVERT(VARCHAR(128), SERVERPROPERTY('ProductLevel')) + ' ' + CONVERT(VARCHAR(128), SERVERPROPERTY('ProductUpdateLevel')) AS SqlServerVersion,
	CONVERT(VARCHAR(128), SERVERPROPERTY('ProductVersion')) AS SqlServerProductVersion,
	CAST(std.XmlData AS NVARCHAR(MAX)) AS ServiceTaskData,
	dldw.AuditServerName,
	dldw.BIRESETCHANGEDATACAPTURE
FROM #DbList dl
	left join #DbListWithDwString dldw on dldw.MainDbName = dl.MainDbName
	left join #DbListWithAppDetails dlad on dlad.MainDbName = dl.MainDbName
	left join #CdcDbDetails cd on cd.MainDbName = dl.MainDbName
	left join #ServiceTaskData std on std.MainDbName = dl.MainDbName
