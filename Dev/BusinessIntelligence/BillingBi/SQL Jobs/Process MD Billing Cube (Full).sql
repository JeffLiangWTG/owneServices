USE [msdb]
GO

/****** Object:  Job [Process Billing Cube (Full, Multidimensional)]    Script Date: 2020-06-26 11:48:16 AM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 2020-06-26 11:48:17 AM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Process Billing Cube (Full, Multidimensional)', 
		@enabled=0, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'No description available.', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Check if current replica is primary]    Script Date: 2020-06-26 11:48:17 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Check if current replica is primary', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=4, 
		@on_success_step_id=2, 
		@on_fail_action=1, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'IF NOT EXISTS

(SELECT 1
FROM sys.databases d
	LEFT JOIN [sys].[dm_hadr_database_replica_states] dr with (nolock) ON dr.database_id = d.database_id AND dr.replica_id = d.replica_id
WHERE d.name = ''Billing''
	AND dr.is_primary_replica = 1)

	SELECT 1/0

ELSE

	SELECT 1', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Backup existing Billing Cube]    Script Date: 2020-06-26 11:48:17 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Backup existing Billing Cube', 
		@step_id=2, 
		@cmdexec_success_code=0, 
		@on_success_action=4, 
		@on_success_step_id=3, 
		@on_fail_action=2, 
		@on_fail_step_id=9, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'ANALYSISCOMMAND', 
		@command=N'<Backup xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
  <Object>
    <DatabaseID>Billing_Test2</DatabaseID>
  </Object>
  <File>Billing_Temp.abf</File>
  <AllowOverwrite>true</AllowOverwrite>
</Backup>', 
		@server=N'sydco-ssql-12b', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Restore Billing cube as BillingTemp cube]    Script Date: 2020-06-26 11:48:17 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Restore Billing cube as BillingTemp cube', 
		@step_id=3, 
		@cmdexec_success_code=0, 
		@on_success_action=4, 
		@on_success_step_id=4, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'ANALYSISCOMMAND', 
		@command=N'<Restore xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
  <File>F:\MSAS13.MSSQLSERVER\OLAP\Backup\Billing_Temp.abf</File>
  <DatabaseName>BillingTemp</DatabaseName>
  <AllowOverwrite>true</AllowOverwrite>
</Restore>', 
		@server=N'sydco-ssql-12b', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Delete all partitions in Temp Billing Cube]    Script Date: 2020-06-26 11:48:17 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete all partitions in Temp Billing Cube', 
		@step_id=4, 
		@cmdexec_success_code=0, 
		@on_success_action=4, 
		@on_success_step_id=5, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'SET NOCOUNT ON

DECLARE @CubeName VARCHAR(100), @CatalogName VARCHAR(100), @InstanceName VARCHAR(100), @MeasureGroup VARCHAR(100), @LinkedServerName VARCHAR(100)
DECLARE @GetPartitionsScript NVARCHAR(MAX), @DeletePartitionTemplate NVARCHAR(MAX), @DeletePartitionScript NVARCHAR(MAX), @CreatePartitionTemplate NVARCHAR(MAX), @CreatePartitionScript NVARCHAR(MAX)
DECLARE @PartitionBatchScript NVARCHAR(MAX)

SET @CubeName = ''BillingCube''
SET @InstanceName = ''sydco-ssql-12b''
SET @LinkedServerName = ''[SSAS2016]''
SET @CatalogName = ''BillingTemp''
SET @MeasureGroup = ''Billing Transactions''

SET @GetPartitionsScript = N''N''''

SELECT Object_ID
FROM $system.DISCOVER_OBJECT_MEMORY_USAGE
WHERE OBJECT_TYPE_ID = 100021 AND
	OBJECT_PARENT_PATH = '''''''''' + @InstanceName + ''.Databases.'' + @CatalogName + ''.Cubes.'' + @CubeName + ''.Measure Groups.'' + @MeasureGroup + ''.Partitions''''''''
''''
''

SET @GetPartitionsScript = REPLACE(REPLACE(''
SELECT Object_ID
FROM OPENQUERY(<ServerName>,<Query>)'', ''<ServerName>'', @LinkedServerName), ''<Query>'', @GetPartitionsScript)

CREATE TABLE #Partitions (IDC INT IDENTITY(1,1) PRIMARY KEY, PartitionName VARCHAR(250))
INSERT INTO #Partitions (PartitionName)
EXEC(@GetPartitionsScript)

/*
SELECT PartitionName
FROM #Partitions
*/

SET @PartitionBatchScript = N''

<Batch xmlns="http://schemas.microsoft.com/analysisservices/2003/engine" Transaction="true">
''

SET @DeletePartitionTemplate = ''
<Delete xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
  <Object>
    <DatabaseID><Catalog></DatabaseID>
    <CubeID><Cube></CubeID>
    <MeasureGroupID><MeasureGroup></MeasureGroupID>
    <PartitionID><PartitionName></PartitionID>
  </Object>
</Delete>
''

SET @DeletePartitionTemplate = REPLACE(REPLACE(REPLACE(@DeletePartitionTemplate, ''<Catalog>'', @CatalogName), ''<Cube>'', @CubeName), ''<MeasureGroup>'', @MeasureGroup)

DECLARE @Counter INT = 1, @MaxRows INT, @PartitionName VARCHAR(100)
SELECT @MaxRows = COUNT(*) FROM #Partitions

WHILE @Counter <= @MaxRows
BEGIN
	SELECT @PartitionName = PartitionName FROM #Partitions WHERE IDC = @Counter

	SET @DeletePartitionScript = REPLACE(@DeletePartitionTemplate, ''<PartitionName>'', @PartitionName)
	SET @PartitionBatchScript = @PartitionBatchScript + @DeletePartitionScript

	SET @Counter = @Counter + 1

END

DROP TABLE #Partitions 

SET @PartitionBatchScript = @PartitionBatchScript + N''

</Batch>
''

SET @PartitionBatchScript = REPLACE(CAST(N''EXEC(''''<Script>'''') AT '' + @LinkedServerName AS NVARCHAR(MAX)),''<Script>'', @PartitionBatchScript)	

EXEC (@PartitionBatchScript)
', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Create Partitions and Process Billing Temp cube]    Script Date: 2020-06-26 11:48:18 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Create Partitions and Process Billing Temp cube', 
		@step_id=5, 
		@cmdexec_success_code=0, 
		@on_success_action=4, 
		@on_success_step_id=6, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'SET NOCOUNT ON

DECLARE @CubeName VARCHAR(100), @CatalogName VARCHAR(100), @InstanceName VARCHAR(100), @MeasureGroup VARCHAR(100), @LinkedServerName VARCHAR(100)
DECLARE @GetPartitionsScript NVARCHAR(MAX), @DeletePartitionTemplate NVARCHAR(MAX), @DeletePartitionScript NVARCHAR(MAX), @CreatePartitionTemplate NVARCHAR(MAX), @CreatePartitionScript NVARCHAR(MAX)
DECLARE @PartitionBatchScript NVARCHAR(MAX)

SET @CubeName = ''BillingCube''
SET @InstanceName = ''sydco-ssql-12b''
SET @LinkedServerName = ''[SSAS2016]''
SET @CatalogName = ''BillingTemp''
SET @MeasureGroup = ''Billing Transactions''

SET @PartitionBatchScript = N''

<Batch xmlns="http://schemas.microsoft.com/analysisservices/2003/engine" Transaction="true">
''

SET @CreatePartitionTemplate = N''
<Create xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
    <ParentObject>
        <DatabaseID><Catalog></DatabaseID>
        <CubeID><Cube></CubeID>
        <MeasureGroupID><MeasureGroup></MeasureGroupID>
    </ParentObject>
    <ObjectDefinition>
        <Partition xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:ddl2="http://schemas.microsoft.com/analysisservices/2003/engine/2" xmlns:ddl2_2="http://schemas.microsoft.com/analysisservices/2003/engine/2/2" xmlns:ddl100_100="http://schemas.microsoft.com/analysisservices/2008/engine/100/100" xmlns:ddl200="http://schemas.microsoft.com/analysisservices/2010/engine/200" xmlns:ddl200_200="http://schemas.microsoft.com/analysisservices/2010/engine/200/200" xmlns:ddl300="http://schemas.microsoft.com/analysisservices/2011/engine/300" xmlns:ddl300_300="http://schemas.microsoft.com/analysisservices/2011/engine/300/300" xmlns:ddl400="http://schemas.microsoft.com/analysisservices/2012/engine/400" xmlns:ddl400_400="http://schemas.microsoft.com/analysisservices/2012/engine/400/400" xmlns:ddl500="http://schemas.microsoft.com/analysisservices/2013/engine/500" xmlns:ddl500_500="http://schemas.microsoft.com/analysisservices/2013/engine/500/500">
            <ID><PartitionID></ID>
            <Name><PartitionName></Name>
            <Source xsi:type="QueryBinding">
                <DataSourceID>Billing</DataSourceID>
                <QueryDefinition>SELECT * 
FROM [analysis].[BillingTransactions]
WHERE [BillingPeriod] &gt;= <StartPeriod> AND [BillingPeriod] &lt; <EndPeriod></QueryDefinition>
            </Source>
            <StorageMode>Molap</StorageMode>
            <ProcessingMode>Regular</ProcessingMode>
            <ProactiveCaching>
                <SilenceInterval>-PT1S</SilenceInterval>
                <Latency>-PT1S</Latency>
                <SilenceOverrideInterval>-PT1S</SilenceOverrideInterval>
                <ForceRebuildInterval>-PT1S</ForceRebuildInterval>
                <Source xsi:type="ProactiveCachingInheritedBinding" />
            </ProactiveCaching>
        </Partition>
    </ObjectDefinition>
</Create>
''

SET @CreatePartitionTemplate = REPLACE(REPLACE(REPLACE(@CreatePartitionTemplate, ''<Catalog>'', @CatalogName), ''<Cube>'', @CubeName), ''<MeasureGroup>'', @MeasureGroup)

--PRINT @CreatePartitionTemplate

CREATE TABLE #NewPartitions (IDC INT IDENTITY(1,1) PRIMARY KEY, PartitionName VARCHAR(250), PartitionID VARCHAR(100), StartPeriod VARCHAR(6), EndPeriod VARCHAR(6))

DECLARE @FirstDayForMonthPartition DATE

SET @FirstDayForMonthPartition = DATEADD(mm, - 1, GETDATE())
SET @FirstDayForMonthPartition = DATEFROMPARTS(YEAR(@FirstDayForMonthPartition), MONTH(@FirstDayForMonthPartition), 1)

INSERT INTO #NewPartitions (PartitionID, PartitionName, StartPeriod, EndPeriod)
VALUES
(''BillingTransactions'', ''Billing Transactions prior to '' + CONVERT(VARCHAR(8), @FirstDayForMonthPartition, 112), ''201001'', CONVERT(VARCHAR(6), @FirstDayForMonthPartition, 112)),
(''BillingTransactionsPrevious'', ''Billing Transactions Last Month'', CONVERT(VARCHAR(6), @FirstDayForMonthPartition, 112), CONVERT(VARCHAR(6), DATEADD(mm, 1, @FirstDayForMonthPartition), 112)),
(''BillingTransactionsCurrent'', ''Billing Transactions Current Month'', CONVERT(VARCHAR(6), DATEADD(mm, 1, @FirstDayForMonthPartition), 112), ''205001'')


DECLARE @MaxRows INT = 3, @Counter INT = 1
DECLARE @StartPeriod VARCHAR(6), @EndPeriod VARCHAR(6), @PartitionID VARCHAR(100), @PartitionName VARCHAR(128)

WHILE @Counter <= @MaxRows
BEGIN

	SELECT 
		@PartitionName = PartitionName,
		@PartitionID = PartitionID,
		@StartPeriod = StartPeriod,
		@EndPeriod = EndPeriod
	FROM #NewPartitions WHERE IDC = @Counter

	SET @CreatePartitionScript = REPLACE(@CreatePartitionTemplate, ''<PartitionName>'', @PartitionName)
	SET @CreatePartitionScript = REPLACE(@CreatePartitionScript, ''<PartitionID>'', @PartitionID)
	SET @CreatePartitionScript = REPLACE(@CreatePartitionScript, ''<StartPeriod>'', @StartPeriod)
	SET @CreatePartitionScript = REPLACE(@CreatePartitionScript, ''<EndPeriod>'', @EndPeriod)

	SET @PartitionBatchScript = @PartitionBatchScript + @CreatePartitionScript

	SET @Counter = @Counter + 1

END

DROP TABLE #NewPartitions

SET @PartitionBatchScript = @PartitionBatchScript + N''

<Parallel>
    <Process xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:ddl2="http://schemas.microsoft.com/analysisservices/2003/engine/2" xmlns:ddl2_2="http://schemas.microsoft.com/analysisservices/2003/engine/2/2" xmlns:ddl100_100="http://schemas.microsoft.com/analysisservices/2008/engine/100/100" xmlns:ddl200="http://schemas.microsoft.com/analysisservices/2010/engine/200" xmlns:ddl200_200="http://schemas.microsoft.com/analysisservices/2010/engine/200/200" xmlns:ddl300="http://schemas.microsoft.com/analysisservices/2011/engine/300" xmlns:ddl300_300="http://schemas.microsoft.com/analysisservices/2011/engine/300/300" xmlns:ddl400="http://schemas.microsoft.com/analysisservices/2012/engine/400" xmlns:ddl400_400="http://schemas.microsoft.com/analysisservices/2012/engine/400/400" xmlns:ddl500="http://schemas.microsoft.com/analysisservices/2013/engine/500" xmlns:ddl500_500="http://schemas.microsoft.com/analysisservices/2013/engine/500/500">
      <Object>
        <DatabaseID><Catalog></DatabaseID>
      </Object>
      <Type>ProcessFull</Type>
      <WriteBackTableCreation>UseExisting</WriteBackTableCreation>
    </Process>
  </Parallel>''

SET @PartitionBatchScript = REPLACE(@PartitionBatchScript, ''<Catalog>'', @CatalogName)

SET @PartitionBatchScript = @PartitionBatchScript + N''

</Batch>
''

SET @PartitionBatchScript = REPLACE(CAST(N''EXEC(''''<Script>'''') AT '' + @LinkedServerName AS NVARCHAR(MAX)),''<Script>'', @PartitionBatchScript)

EXEC (@PartitionBatchScript)
', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Backup Billing Temp cube]    Script Date: 2020-06-26 11:48:18 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Backup Billing Temp cube', 
		@step_id=6, 
		@cmdexec_success_code=0, 
		@on_success_action=4, 
		@on_success_step_id=7, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'ANALYSISCOMMAND', 
		@command=N'<Backup xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
  <Object>
    <DatabaseID>BillingTemp</DatabaseID>
  </Object>
  <File>Billing_New.abf</File>
  <AllowOverwrite>true</AllowOverwrite>
</Backup>', 
		@server=N'sydco-ssql-12b', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Delete Temp Billing Cube]    Script Date: 2020-06-26 11:48:18 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Delete Temp Billing Cube', 
		@step_id=7, 
		@cmdexec_success_code=0, 
		@on_success_action=4, 
		@on_success_step_id=8, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'ANALYSISCOMMAND', 
		@command=N'<Delete xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
	<Object>
		<DatabaseID>BillingTemp</DatabaseID>
	</Object>
</Delete>', 
		@server=N'sydco-ssql-12b', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Restore Billing Cube (final)]    Script Date: 2020-06-26 11:48:18 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Restore Billing Cube (final)', 
		@step_id=8, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'ANALYSISCOMMAND', 
		@command=N'<Restore xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
  <File>F:\MSAS13.MSSQLSERVER\OLAP\Backup\Billing_New.abf</File>
  <DatabaseName>Billing</DatabaseName>
  <DatabaseID>Billing_Test2</DatabaseID>
  <AllowOverwrite>true</AllowOverwrite>
</Restore>', 
		@server=N'sydco-ssql-12b', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Send Notification]    Script Date: 2020-06-26 11:48:18 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Send Notification', 
		@step_id=9, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'set nocount on

declare @job_id UNIQUEIDENTIFIER = CONVERT(uniqueidentifier, $(ESCAPE_NONE(JOBID))), @job_name sysname

select @job_name = j.name
from msdb.dbo.sysjobs j 
where j.job_id = @job_id

declare @subject_text varchar(200) = ''[The job <status>] SQL Server Job System: "'' + @job_name + ''" completed on '' + @@servername
declare @body_text varchar(max), @message varchar(max), @job_step_duration_text varchar(max)
declare @job_date int, @job_time int, @step_duration int, @run_status int

select top 1
	@run_status = run_status,
	@message = [message]
From msdb.dbo.sysjobs j 
	INNER JOIN msdb.dbo.sysjobhistory h ON j.job_id = h.job_id 
where j.job_id = @job_id
order by [instance_id] desc

declare @max_instance_id int, @max_outcome_instance_id int, @max_next_outcome_instance_id int

/*
select *
from msdb.dbo.sysjobhistory
where job_id = @job_id
order by 1 desc
*/

select @max_instance_id = max(instance_id)
from msdb.dbo.sysjobhistory
where job_id = @job_id

select @max_outcome_instance_id = max(instance_id)
from msdb.dbo.sysjobhistory
where job_id = @job_id and step_id = 0

if @max_instance_id <> @max_outcome_instance_id set @max_outcome_instance_id = @max_instance_id

select @max_next_outcome_instance_id = max(instance_id)
from msdb.dbo.sysjobhistory
where job_id = @job_id and step_id = 0 and instance_id < @max_outcome_instance_id

if @max_next_outcome_instance_id is null set @max_next_outcome_instance_id = 0

set @job_step_duration_text = ''''
set @step_duration = 0

select 
	@job_step_duration_text = @job_step_duration_text + ''Step '' + cast(step_id as varchar(10)) + CHAR(9) + '' (duration: '' + CONVERT(VARCHAR(10), (run_duration / 10000) % 100 + run_duration / 1000000 * 24) + '' hour(s) '' + CONVERT(VARCHAR(10), (run_duration / 100) % 100) + '' minute(s) '' + CONVERT(VARCHAR(10), run_duration % 100) + '' second(s))'' + '':'' + CHAR(9) + step_name + CHAR(13) + CHAR(10),
	@step_duration = @step_duration + run_duration / 1000000 * 3600 * 24 + ((run_duration / 10000) % 100) * 3600 + ((run_duration / 100) % 100) * 60 + run_duration % 100,
	@job_date = case when @job_date is null then run_date else @job_date end,
	@job_time = case when @job_time is null then run_time else @job_time end

from msdb.dbo.sysjobhistory
where job_id = @job_id and instance_id > @max_next_outcome_instance_id and instance_id <= @max_outcome_instance_id and step_id <> 0
order by step_id

set @body_text = ''

JOB RUN:'' + CHAR(9) + CHAR(9) + ''"'' + @job_name + ''" was run on '' + convert(varchar(20), convert(date, convert(varchar(10), @job_date, 112))) + '' at '' + CONVERT(VARCHAR(10), TIMEFROMPARTS ((@job_time / 10000) % 100, (@job_time / 100) % 100, @job_time % 100, 0, 0 ), 120) + ''
DURATION:'' + CHAR(9) + CHAR(9) + CONVERT(VARCHAR(10), @step_duration / 3600) + '' hour(s) '' + CONVERT(VARCHAR(10), @step_duration / 60 % 60) + '' minute(s) '' + CONVERT(VARCHAR(10), @step_duration % 60) + '' second(s)'' + ''
STATUS:'' + CHAR(9) + CHAR(9) + CASE when @run_status = 1 then ''Succeeded'' else ''Failed'' END + ''
MESSAGES:

'' + @message + ''

JOB STEP DETAILS:
'' + @job_step_duration_text

set @subject_text = CASE WHEN @run_status = 1 then REPLACE(@subject_text, ''<status>'', ''succeeded'') else REPLACE(@subject_text, ''<status>'', ''failed'') end

--PRINT @subject_text
--PRINT @body_text

IF @run_status = 1
BEGIN
	BEGIN TRY
		exec msdb.dbo.sp_notify_operator  
			@name = ''BI Team (Success)'', 
--			@name = ''Anton Vinokurov'', 
			@subject = @subject_text,
			@body = @body_text 
	END TRY
	BEGIN CATCH
		declare @Do_Nothing Int = 0
	END CATCH
END
ELSE 
BEGIN
	exec msdb.dbo.sp_notify_operator  
			@name = ''BI Team (Failure)'', 
--			@name = ''Anton Vinokurov'', 
			@subject = @subject_text,
			@body = @body_text 

	RAISERROR(''Job failed.'', 16, -1, @@servername)

END', 
		@database_name=N'msdb', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Weekly', 
		@enabled=1, 
		@freq_type=8, 
		@freq_interval=64, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=1, 
		@active_start_date=20170602, 
		@active_end_date=99991231, 
		@active_start_time=40000, 
		@active_end_time=235959, 
		@schedule_uid=N'3bf3bdff-e4ae-4a6d-b8f0-e85211a36d11'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:
GO

USE [msdb]
GO
CREATE USER [billing_admin] FOR LOGIN [billing_admin]
GO
USE [msdb]
GO
ALTER ROLE [SQLAgentOperatorRole] ADD MEMBER [billing_admin]
GO

