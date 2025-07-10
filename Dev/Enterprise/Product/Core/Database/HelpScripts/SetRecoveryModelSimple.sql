----------------------------------------------------
-- SET RECOVERY MODEL SIMPLE
-- 
-- For all databases on the server:
--   1. Truncate and Shrink the log file
--   2. Sets the recovery model to simple 
----------------------------------------------------

  SET NOCOUNT ON
  DECLARE @DbName varchar(128)
  DECLARE @DbLogName varchar(128)
  DECLARE @SqlText nvarchar(1000)

  -- Loop through databases to set recovery model
  DECLARE DbCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
    SELECT name
    FROM   sys.databases
    WHERE  name not in ('master', 'tempdb')
    AND    recovery_model != 3

  OPEN DbCursor
  FETCH NEXT FROM DbCursor INTO @DbName
  WHILE (@@FETCH_STATUS = 0)
  BEGIN
    -- Sets Recovery Model
    SET @SqlText = 'ALTER DATABASE [' + @DbName + '] SET RECOVERY SIMPLE'
    EXEC sp_executesql @SqlText

    IF (DATABASEPROPERTYEX(@DbName, 'Updateability') = 'READ_WRITE')
    BEGIN
      -- Truncates unused log space
      BACKUP LOG @DbName WITH TRUNCATE_ONLY

      -- Shrinks log file
      SET @SqlText = 'SELECT TOP 1 @DbLogName = rtrim(name) FROM [' + @DbName + '].sys.database_files WHERE type = 1'
      EXEC sp_executesql @SqlText, N'@DbLogName varchar(128) OUTPUT', @DbLogName OUTPUT
      SET @SqlText = 'USE [' + @DbName + '] DBCC SHRINKFILE ([' + @DbLogName + '], 1) WITH NO_INFOMSGS'
      EXEC sp_executesql @SqlText
    END

    FETCH NEXT FROM DbCursor INTO @DbName
  END
  CLOSE DbCursor
  DEALLOCATE DbCursor
