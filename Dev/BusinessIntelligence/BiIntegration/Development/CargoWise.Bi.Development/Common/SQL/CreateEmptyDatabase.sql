IF EXISTS (SELECT name FROM sys.databases WHERE name = '{0}')
BEGIN
	IF EXISTS(SELECT NULL FROM sys.databases WHERE name = '{0}' AND is_auto_update_stats_async_on = 1)
	BEGIN
		ALTER DATABASE [{0}] SET AUTO_UPDATE_STATISTICS_ASYNC OFF

		DECLARE @killCmd nvarchar(800) = ''
		SELECT @killCmd = @killCmd + 'KILL STATS JOB' + STR(job_id)
			FROM sys.dm_exec_background_job_queue j
			INNER JOIN sys.databases d
			ON j.database_id = d.database_id
			WHERE d.name = '{0}'
		EXEC sp_executesql @killCmd
	END

	ALTER DATABASE [{0}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE

	DROP DATABASE [{0}];
END

CREATE DATABASE [{0}];
ALTER DATABASE [{0}] COLLATE {1};
ALTER DATABASE [{0}] SET COMPATIBILITY_LEVEL = 130;
ALTER DATABASE [{0}] SET RECOVERY SIMPLE;
