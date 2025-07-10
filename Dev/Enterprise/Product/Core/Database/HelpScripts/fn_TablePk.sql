------------------------------------------------
-- SCALAR FUNCTION
--
-- Name: fn_TablePk
-- Description: Returns the primary key of table
------------------------------------------------
CREATE FUNCTION fn_TablePk
(
	@TableName sysname
)
RETURNS sysname WITH SCHEMABINDING AS
BEGIN
	DECLARE @PkColumn sysname
	
	SET @PkColumn = (
		SELECT TOP 1
			pkcol.name
		FROM
			sys.tables tab
			INNER JOIN sys.indexes pk
				ON pk.object_id = tab.object_id
				AND pk.is_primary_key = 1
			INNER JOIN sys.index_columns pkkey
				ON pkkey.object_id = pk.object_id
				AND pkkey.index_id = pk.index_id
				AND pkkey.index_column_id = 1
			INNER JOIN sys.columns pkcol
				ON pkcol.object_id = pkkey.object_id
				AND pkcol.column_id = pkkey.column_id
		WHERE
			tab.name = @TableName
	)
	
	RETURN @PkColumn
END
