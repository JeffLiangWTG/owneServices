--------------------------------------------------
-- XT_GetPurgeOrderSpecific	                --
--                                              --
-- Cleans up data of a given table hierarchy    --
--------------------------------------------------

IF OBJECT_ID('dbo.GetPurgeOrderSpecific') IS NOT NULL DROP FUNCTION GetPurgeOrderSpecific
go

CREATE FUNCTION GetPurgeOrderSpecific (@RootTable varchar(128), @DependantFields varchar(1000))
RETURNS @OutputData TABLE (
      DeleteOrder   int,
      TableName   sysname  not null,
      LocalColumn sysname  not null,
      FkColumn    sysname  not null
    )
	WITH SCHEMABINDING
AS
BEGIN

-- -- DEBUG ONLY
--    SET NOCOUNT ON
--    declare @OutputData TABLE (
--       DeleteOrder   int,
--       TableName   sysname  not null,
--       LocalColumn sysname  not null,
--       FkColumn    sysname  not null
--    )
-- 
--    declare @RootTable varchar(128)
--    declare @DependantFields varchar(1000)
--    set @RootTable = 'OrgHeader'
--    set @DependantFields = 'OM_OH, OB_OH, OV_OH_OrgHeader, OA_OH, OC_OH, O9_OH, O8_OH, OX_OH, P1_OH, OS_OH, OW_OH, OW_OH_AirCompetitor, OW_OH_SeaCompetitor, OQ_OH, OL_OH_Buyer, OL_OH_Supplier, OO_OH, OK_OH, OT_OH, P8_OH, P7_OH, OC_OA_OrgAddress'

  DECLARE @Pk           int
  DECLARE @NodeLevel    int
  DECLARE @QtyChildren  int
  DECLARE @FkColumn     varchar(128)
  DECLARE @LocalColumn  varchar(128)
  DECLARE @TableName    varchar(128)
  DECLARE @Path         varchar(7000)
  DECLARE @Operation    varchar(50)
  DECLARE @ErrorMessage varchar(255)

  DECLARE @ReferenceList TABLE (
    Pk          int           not null identity(1, 1),
    NodeLevel   int           not null,
    Path        varchar(7500) not null,
    TableName   varchar(75)  not null,
    Operation   varchar(50)   null,
    Parent      varchar(75)  null,
    LocalColumn varchar(128)  null,
    FkColumn    varchar(128)  null,
    QtyChildren tinyint       null
  )

  DECLARE @Operations TABLE (SQL varchar(1000), TableName sysname)


  DECLARE @DependantFieldsTable TABLE (FieldName sysname, TableName sysname)
  DECLARE @comma integer, @Field sysname, @Table sysname
  WHILE (LEN(@DependantFields) > 0)
  BEGIN
	SET @comma = CHARINDEX(',', @DependantFields)
	IF @comma > 1
	BEGIN
		SET @Field = LEFT(@DependantFields, @comma-1)
		SET @DependantFields = LTRIM(SUBSTRING(@DependantFields, @comma+1, LEN(@DependantFields)))
	END
	ELSE
	BEGIN
		SET @Field = @DependantFields
		SET @DependantFields = ''
	END
SET @Table = (SELECT TOP 1 o.name FROM dbo.sys.columns c JOIN sys.objects o  ON c.object_id = o.object_id WHERE c.name = @Field) 
	IF (@Table IS NOT NULL)
	BEGIN
	    INSERT INTO @DependantFieldsTable VALUES (@Field, @Table)
	END

  END

  -- Check Root Table is valid
IF (not exists (SELECT name FROM sys.objects  WHERE type = 'U' AND name = @RootTable)) 
  BEGIN
    SET @ErrorMessage = '''' + @RootTable + ''' is not a valid table on database ''' + db_name() + '''.'
--    RAISERROR(@ErrorMessage, 16, 1)
    RETURN
  END
  
  -- Insert Root Table
  INSERT @ReferenceList (NodeLevel, Path, TableName)
    VALUES (1, @RootTable, @RootTable)

WHILE (exists(SELECT Pk FROM @ReferenceList WHERE Operation is null)) 
  BEGIN
    -- Currently Unprocessed Tables Cursor
    DECLARE UnprocessedTableCursor CURSOR FOR
SELECT Pk, NodeLevel, Path, TableName, FkColumn FROM @ReferenceList 
      WHERE Operation is null
    
    OPEN UnprocessedTableCursor
FETCH NEXT FROM UnprocessedTableCursor INTO @Pk, @NodeLevel, @Path, @TableName, @FkColumn 
    WHILE (@@FETCH_STATUS = 0)
    BEGIN
      -- If the table is found in a previous node along this path, can't delete the rows. 
      -- FK field should be set to null and
      -- QtyChildren set to 0(zero) to avoid infinite loops
      IF exists(SELECT @Pk
FROM @ReferenceList 
                WHERE @Path like Path + '%'
                AND   TableName = @TableName
                AND   NodeLevel < @NodeLevel)
      BEGIN
        SET @QtyChildren = 0
        SET @Operation = 'UPDATE'
      END
      ELSE
      BEGIN
        -- Insert Dependent Tables on ReferenceBoard table
        INSERT @ReferenceList (NodeLevel, Path, TableName, Parent, LocalColumn, FkColumn)
          SELECT 
            @NodeLevel + 1, 
            @Path + '-' + object_name(fkeyid),
            object_name(fkeyid),
            @TableName,
	    col_name(rkeyid, 1),
            col_name(fkeyid, fkey1)
-- if you need to fix this, figure it out, but there are no references to this function
FROM dbo.[used to be sys references]  JOIN @DependantFieldsTable DF on col_name(fkeyid, fkey1) = DF.FieldName 
          WHERE rkeyid = object_id(@TableName)
          AND rkeyid <> fkeyid
  
        -- Set number of children and operation
        SET @QtyChildren = @@rowcount
        SET @Operation = 'DELETE'
        SET @FkColumn = null
      END

      -- Updates operation and number of children (flag table as processed)
      UPDATE @ReferenceList
        SET Operation = @Operation, 
            QtyChildren = @QtyChildren,
            FkColumn = @FkColumn
        WHERE Pk = @Pk
    
FETCH NEXT FROM UnprocessedTableCursor INTO @Pk, @NodeLevel, @Path, @TableName, @FkColumn 
    END
    CLOSE UnprocessedTableCursor
    DEALLOCATE UnprocessedTableCursor
  END -- while exists null Operation

    DECLARE @ItemCount as int
    set @ItemCount = 0;
    -- Currently Unprocessed Tables Cursor
    DECLARE OutputData_cursor CURSOR FOR
      SELECT LocalColumn, FieldName, DF.TableName
FROM @ReferenceList RL JOIN @DependantFieldsTable DF on RL.TableName = DF.TableName 
      GROUP BY LocalColumn, DF.FieldName, DF.TableName
      ORDER BY MAX(NodeLevel) desc
    OPEN OutputData_cursor
FETCH NEXT FROM OutputData_cursor INTO @LocalColumn, @FkColumn, @TableName 
    WHILE (@@FETCH_STATUS = 0)
    BEGIN
      INSERT INTO @OutputData VALUES (@ItemCount, @TableName, @LocalColumn, @FkColumn)
      SET @ItemCount = @ItemCount + 1
FETCH NEXT FROM OutputData_cursor INTO @LocalColumn, @FkColumn, @TableName 
    END
    CLOSE OutputData_cursor
    DEALLOCATE OutputData_cursor

-- -- DEBUG ONLY
--select * from @OutputData 

  return
END

--sp_columns OrgContact

--select * from dbo.[GetPurgeOrderSpecific]('OrgHeader', 'OM_OH, OB_OH, OV_OH_OrgHeader, OA_OH, OC_OH, O9_OH, O8_OH, OX_OH, P1_OH, OS_OH, OW_OH, OW_OH_AirCompetitor, OW_OH_SeaCompetitor, OQ_OH, OL_OH_Buyer, OL_OH_Supplier, OO_OH, OK_OH, OT_OH, P8_OH, P7_OH') 

