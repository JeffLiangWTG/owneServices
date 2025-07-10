-------------------------------------------------------------
-- STORED PROCEDURE
--
-- Name: ep_DeleteCascade
--
-- Description: Deletes records from a top level table,
--              and all dependent records in its child
--              table tree, based on the PKs contained
--              in the expected temporary table below.
--
-- Requires: #Temp_DeleteCascadeRows(RowPk uniqueidentifier)
--           fn_TablePk(@TableName)
--
-- Sample Usage:
--   SELECT JS_PK AS RowPk
--     INTO #Temp_DeleteCascadeRows
--     FROM dbo.JobShipment
--     WHERE <filter_criteria>;
--   EXEC ep_DeleteCascade 'JobShipment';
-------------------------------------------------------------
CREATE PROCEDURE ep_DeleteCascade
	@TableName sysname,
	@AuditLogDb sysname = null,
	@Level int = 0
AS
BEGIN
	SET NOCOUNT ON
	
	-- Copy row to delete to a local temp table
	-- and check guard clause (if no rows to delete => exit)
	SELECT * INTO #Temp_Local_DeleteCascadeRows FROM #Temp_DeleteCascadeRows
	IF (@@rowcount = 0) RETURN

	-- Check the Audit-Log database exists
	IF (@AuditLogDb is not null AND not exists(SELECT * FROM sys.databases WHERE [name] = @AuditLogDb))
	BEGIN
		DECLARE @ErrorMessage varchar(1000)
		SET @ErrorMessage = 'Must create DB [' + @AuditLogDb + '] to audit-log modified records.'
		RAISERROR(@ErrorMessage, 16, 1)
		RETURN -1
	END
	
	SET @Level = @Level + 1
	PRINT replicate('--', @Level) + '  ' + @TableName + ' - ' + convert(varchar(max), @Level)

	-- Declarations
	DECLARE @PkColumn sysname
	DECLARE @FkTable sysname
	DECLARE @FkColumn sysname
	DECLARE @FkTablePk sysname
	DECLARE @LogTableFullName nvarchar(300)
	DECLARE @SqlText nvarchar(2000)
	DECLARE @FkInfoTable TABLE (FkTable sysname, FkColumn sysname, IsSelfReference int)
	

	-- Get PK column name
	SET @PkColumn = dbo.fn_TablePk(@TableName)
	
	IF (@PkColumn is not null)
	BEGIN
		-- Populate FK info table
		INSERT @FkInfoTable (FkTable, FkColumn, IsSelfReference)
			SELECT FkTable, FkColumn, IsSelfReference 
			FROM vw_FkReferences WHERE PkTable = @TableName


		-- Self Reference FKs (set references of rows to be deleted to null)
		WHILE EXISTS(SELECT * FROM @FkInfoTable WHERE IsSelfReference = 1)
		BEGIN
			SELECT TOP 1 @FkColumn = FkColumn
				FROM @FkInfoTable WHERE IsSelfReference = 1

			SET @SqlText = 
				'UPDATE ' + @TableName +
				' SET ' + @FkColumn + ' = NULL' +
				' WHERE ' + @FkColumn + ' in (SELECT RowPk FROM #Temp_Local_DeleteCascadeRows)'

			IF (@AuditLogDb is not null)
			BEGIN
				SET @LogTableFullName = '[' + @AuditLogDb + ']..[' + @TableName + '-NULLIFY-' + @FkColumn + ']'
				SET @SqlText = 
					'IF (object_id(''' + @LogTableFullName + ''') is null)' +
					' BEGIN' +
					'  SELECT ' + @PkColumn + ',' + @FkColumn +
					'   INTO ' + @LogTableFullName +
					'   FROM ' + @TableName +
					'   WHERE ' + @FkColumn + ' in (SELECT RowPk FROM #Temp_Local_DeleteCascadeRows)' +
					' END ' + 
					' ELSE BEGIN' +
					'  INSERT ' + @LogTableFullName +
					'   SELECT ' + @PkColumn + ',' + @FkColumn +
					'   FROM ' + @TableName +
					'   WHERE ' + @FkColumn + ' in (SELECT RowPk FROM #Temp_Local_DeleteCascadeRows)' +
					' END  ' +
					@SqlText
			END
				
			EXEC sp_executesql @SqlText
			IF (@@rowcount > 0) PRINT 'Self-references (' + @TableName + '.' + @FkColumn + ') set to null'
			
			DELETE @FkInfoTable
				WHERE FkTable = @TableName
				AND FkColumn = @FkColumn
		END


		-- Delete Cascade from Child Tables
		WHILE EXISTS(SELECT * FROM @FkInfoTable)
		BEGIN
			SELECT TOP 1 @FkTable = FkTable, @FkColumn = FkColumn, @FkTablePk = dbo.fn_TablePk(FkTable)
				FROM @FkInfoTable

			-- Call itself recursively to delete a dependent record branch
			TRUNCATE TABLE #Temp_DeleteCascadeRows
			SET @SqlText = 
				'INSERT #Temp_DeleteCascadeRows' +
				' SELECT ' + @FkTablePk + ' FROM ' + @FkTable +
				' WHERE ' + @FkColumn + ' in (SELECT RowPk FROM #Temp_Local_DeleteCascadeRows)'
				
			EXEC sp_executesql @SqlText
			EXEC ep_DeleteCascade @FkTable, @AuditLogDb, @Level
		
			DELETE @FkInfoTable
				WHERE FkTable = @FkTable
				AND FkColumn = @FkColumn
		END


		-- Delete rows
		SET @SqlText =
			'DELETE ' + @TableName + 
			' WHERE ' + @PkColumn + ' in (SELECT RowPk FROM #Temp_Local_DeleteCascadeRows)'

		IF (@AuditLogDb is not null)
		BEGIN
			SET @LogTableFullName = '[' + @AuditLogDb + ']..[' + @TableName + '-DELETE]'
			SET @SqlText = 
				'IF (object_id(''' + @LogTableFullName + ''') is null)' +
				' BEGIN' +
				'  SELECT *' +
				'   INTO ' + @LogTableFullName +
				'   FROM ' + @TableName +
				'   WHERE ' + @PkColumn + ' in (SELECT RowPk FROM #Temp_Local_DeleteCascadeRows)' +
				' END ' + 
				' ELSE BEGIN' +
				'  INSERT ' + @LogTableFullName +
				'   SELECT *' +
				'   FROM ' + @TableName +
				'   WHERE ' + @PkColumn + ' in (SELECT RowPk FROM #Temp_Local_DeleteCascadeRows)' +
				' END  ' +
				@SqlText
		END
			
		EXEC sp_executesql @SqlText
		PRINT 'Deleted ' + convert(varchar(100), @@rowcount) + ' rows from ' + @TableName
	END
END
