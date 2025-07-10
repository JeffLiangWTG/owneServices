USE [msdb]
GO

/****** Object:  Job [Process Billing Cube (Partial, Multidimensional)]    Script Date: 2020-06-26 11:49:58 AM ******/
BEGIN TRANSACTION
DECLARE @ReturnCode INT
SELECT @ReturnCode = 0
/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 2020-06-26 11:49:58 AM ******/
IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
BEGIN
EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

END

DECLARE @jobId BINARY(16)
EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'Process Billing Cube (Partial, Multidimensional)', 
		@enabled=0, 
		@notify_level_eventlog=0, 
		@notify_level_email=0, 
		@notify_level_netsend=0, 
		@notify_level_page=0, 
		@delete_level=0, 
		@description=N'Nudged by edi.ProcessStaging SP', 
		@category_name=N'[Uncategorized (Local)]', 
		@owner_login_name=N'sa', @job_id = @jobId OUTPUT
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Check if current replica is primary]    Script Date: 2020-06-26 11:49:59 AM ******/
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
/****** Object:  Step [Partial Cube Processing]    Script Date: 2020-06-26 11:49:59 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Partial Cube Processing', 
		@step_id=2, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=2, 
		@retry_interval=15, 
		@os_run_priority=0, @subsystem=N'ANALYSISCOMMAND', 
		@command=N'<Batch xmlns="http://schemas.microsoft.com/analysisservices/2003/engine" Transaction="true">
  <Parallel>
      <Process xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:ddl2="http://schemas.microsoft.com/analysisservices/2003/engine/2" xmlns:ddl2_2="http://schemas.microsoft.com/analysisservices/2003/engine/2/2" xmlns:ddl100_100="http://schemas.microsoft.com/analysisservices/2008/engine/100/100" xmlns:ddl200="http://schemas.microsoft.com/analysisservices/2010/engine/200" xmlns:ddl200_200="http://schemas.microsoft.com/analysisservices/2010/engine/200/200" xmlns:ddl300="http://schemas.microsoft.com/analysisservices/2011/engine/300" xmlns:ddl300_300="http://schemas.microsoft.com/analysisservices/2011/engine/300/300" xmlns:ddl400="http://schemas.microsoft.com/analysisservices/2012/engine/400" xmlns:ddl400_400="http://schemas.microsoft.com/analysisservices/2012/engine/400/400" xmlns:ddl500="http://schemas.microsoft.com/analysisservices/2013/engine/500" xmlns:ddl500_500="http://schemas.microsoft.com/analysisservices/2013/engine/500/500">
      <Object>
        <DatabaseID>Billing_Test2</DatabaseID>
        <DimensionID>Client</DimensionID>
      </Object>
      <Type>ProcessUpdate</Type>
      <WriteBackTableCreation>UseExisting</WriteBackTableCreation>
    </Process>
    <Process xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:ddl2="http://schemas.microsoft.com/analysisservices/2003/engine/2" xmlns:ddl2_2="http://schemas.microsoft.com/analysisservices/2003/engine/2/2" xmlns:ddl100_100="http://schemas.microsoft.com/analysisservices/2008/engine/100/100" xmlns:ddl200="http://schemas.microsoft.com/analysisservices/2010/engine/200" xmlns:ddl200_200="http://schemas.microsoft.com/analysisservices/2010/engine/200/200" xmlns:ddl300="http://schemas.microsoft.com/analysisservices/2011/engine/300" xmlns:ddl300_300="http://schemas.microsoft.com/analysisservices/2011/engine/300/300" xmlns:ddl400="http://schemas.microsoft.com/analysisservices/2012/engine/400" xmlns:ddl400_400="http://schemas.microsoft.com/analysisservices/2012/engine/400/400" xmlns:ddl500="http://schemas.microsoft.com/analysisservices/2013/engine/500" xmlns:ddl500_500="http://schemas.microsoft.com/analysisservices/2013/engine/500/500">
      <Object>
        <DatabaseID>Billing_Test2</DatabaseID>
        <DimensionID>Price Item</DimensionID>
      </Object>
      <Type>ProcessUpdate</Type>
      <WriteBackTableCreation>UseExisting</WriteBackTableCreation>
    </Process>
    <Process xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:ddl2="http://schemas.microsoft.com/analysisservices/2003/engine/2" xmlns:ddl2_2="http://schemas.microsoft.com/analysisservices/2003/engine/2/2" xmlns:ddl100_100="http://schemas.microsoft.com/analysisservices/2008/engine/100/100" xmlns:ddl200="http://schemas.microsoft.com/analysisservices/2010/engine/200" xmlns:ddl200_200="http://schemas.microsoft.com/analysisservices/2010/engine/200/200" xmlns:ddl300="http://schemas.microsoft.com/analysisservices/2011/engine/300" xmlns:ddl300_300="http://schemas.microsoft.com/analysisservices/2011/engine/300/300" xmlns:ddl400="http://schemas.microsoft.com/analysisservices/2012/engine/400" xmlns:ddl400_400="http://schemas.microsoft.com/analysisservices/2012/engine/400/400" xmlns:ddl500="http://schemas.microsoft.com/analysisservices/2013/engine/500" xmlns:ddl500_500="http://schemas.microsoft.com/analysisservices/2013/engine/500/500">
      <Object>
        <DatabaseID>Billing_Test2</DatabaseID>
        <CubeID>BillingCube</CubeID>
        <MeasureGroupID>Billing Transactions</MeasureGroupID>
        <PartitionID>BillingTransactionsCurrent</PartitionID>
      </Object>
      <Type>ProcessFull</Type>
      <WriteBackTableCreation>UseExisting</WriteBackTableCreation>
    </Process>
    <Process xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:ddl2="http://schemas.microsoft.com/analysisservices/2003/engine/2" xmlns:ddl2_2="http://schemas.microsoft.com/analysisservices/2003/engine/2/2" xmlns:ddl100_100="http://schemas.microsoft.com/analysisservices/2008/engine/100/100" xmlns:ddl200="http://schemas.microsoft.com/analysisservices/2010/engine/200" xmlns:ddl200_200="http://schemas.microsoft.com/analysisservices/2010/engine/200/200" xmlns:ddl300="http://schemas.microsoft.com/analysisservices/2011/engine/300" xmlns:ddl300_300="http://schemas.microsoft.com/analysisservices/2011/engine/300/300" xmlns:ddl400="http://schemas.microsoft.com/analysisservices/2012/engine/400" xmlns:ddl400_400="http://schemas.microsoft.com/analysisservices/2012/engine/400/400" xmlns:ddl500="http://schemas.microsoft.com/analysisservices/2013/engine/500" xmlns:ddl500_500="http://schemas.microsoft.com/analysisservices/2013/engine/500/500">
      <Object>
        <DatabaseID>Billing_Test2</DatabaseID>
        <CubeID>BillingCube</CubeID>
        <MeasureGroupID>Billing Transactions</MeasureGroupID>
        <PartitionID>BillingTransactionsPrevious</PartitionID>
      </Object>
      <Type>ProcessFull</Type>
      <WriteBackTableCreation>UseExisting</WriteBackTableCreation>
    </Process>
  </Parallel>
</Batch>', 
		@server=N'sydco-ssql-12b.wtg.zone', 
		@database_name=N'master', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Notify about failure]    Script Date: 2020-06-26 11:49:59 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Notify about failure', 
		@step_id=3, 
		@cmdexec_success_code=0, 
		@on_success_action=2, 
		@on_success_step_id=0, 
		@on_fail_action=2, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'declare @job_name sysname = ''Process Billing Cube (Partial, Multidimensional)''
declare @subject_text varchar(200) = ''[The job failed] SQL Server Job System: "'' + @job_name + ''" completed on '' + @@servername
declare @body_text varchar(max), @message varchar(max)
declare @job_date int, @job_time int, @duration int, @run_status int

select top 1
	@job_date = run_date,
	@job_time = run_time,
	@duration = run_duration,
	@run_status = run_status,
	@message = [message]
From msdb.dbo.sysjobs j 
	INNER JOIN msdb.dbo.sysjobhistory h ON j.job_id = h.job_id 
where j.name = @job_name
	and h.step_name = ''Partial Cube Processing''
order by [instance_id] desc

set @body_text = ''

TEST (PLEASE IGNORE)

JOB RUN:'' + CHAR(9) + CHAR(9) + ''"'' + @job_name + ''" was run on '' + convert(varchar(20), convert(date, convert(varchar(10), @job_date, 112))) + '' at '' + CONVERT(VARCHAR(10), TIMEFROMPARTS ((@job_time / 10000) % 100, (@job_time / 100) % 100, @job_time % 100, 0, 0 ), 120) + ''
DURATION:'' + CHAR(9) + CHAR(9) + CONVERT(VARCHAR(10), (@duration / 10000) % 100) + '' hour(s) '' + CONVERT(VARCHAR(10), (@duration / 100) % 100) + '' minute(s) '' + CONVERT(VARCHAR(10), @duration % 100) + '' second(s)'' + ''
STATUS:'' + CHAR(9) + CHAR(9) + CASE when @run_status = 1 then ''Succeeded'' else ''Failed'' END + ''
MESSAGES:

'' + @message 

exec msdb.dbo.sp_notify_operator  
	@name = ''BI Team (Failure)'', 
	@subject = @subject_text,
	@body = @body_text 
', 
		@database_name=N'msdb', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
/****** Object:  Step [Notify about success]    Script Date: 2020-06-26 11:49:59 AM ******/
EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Notify about success', 
		@step_id=4, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_success_step_id=0, 
		@on_fail_action=1, 
		@on_fail_step_id=0, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'declare @job_name sysname = ''Process Billing Cube (Partial, Multidimensional)''
declare @subject_text varchar(200) = ''[The job succeeded] SQL Server Job System: "'' + @job_name + ''" completed on '' + @@servername
declare @body_text varchar(max), @message varchar(max)
declare @job_date int, @job_time int, @duration int, @run_status int

select top 1
	@job_date = run_date,
	@job_time = run_time,
	@duration = run_duration,
	@run_status = run_status,
	@message = [message]
From msdb.dbo.sysjobs j 
	INNER JOIN msdb.dbo.sysjobhistory h ON j.job_id = h.job_id 
where j.name = @job_name
	and h.step_name = ''Partial Cube Processing''
order by [instance_id] desc

set @body_text = ''

JOB RUN:'' + CHAR(9) + CHAR(9) + ''"'' + @job_name + ''" was run on '' + convert(varchar(20), convert(date, convert(varchar(10), @job_date, 112))) + '' at '' + CONVERT(VARCHAR(10), TIMEFROMPARTS ((@job_time / 10000) % 100, (@job_time / 100) % 100, @job_time % 100, 0, 0 ), 120) + ''
DURATION:'' + CHAR(9) + CHAR(9) + CONVERT(VARCHAR(10), (@duration / 10000) % 100) + '' hour(s) '' + CONVERT(VARCHAR(10), (@duration / 100) % 100) + '' minute(s) '' + CONVERT(VARCHAR(10), @duration % 100) + '' second(s)'' + ''
STATUS:'' + CHAR(9) + CHAR(9) + CASE when @run_status = 1 then ''Succeeded'' else ''Failed'' END + ''
MESSAGES:

'' + @message 

exec msdb.dbo.sp_notify_operator  
	@name = ''BI Team (Success)'', 
	@subject = @subject_text,
	@body = @body_text 

', 
		@database_name=N'msdb', 
		@flags=0
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
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

