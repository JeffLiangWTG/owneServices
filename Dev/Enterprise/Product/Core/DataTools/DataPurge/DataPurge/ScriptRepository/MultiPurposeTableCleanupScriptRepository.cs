using System;

namespace Enterprise.DataPurge
{
	class FinalSystemScriptRepository : ScriptRepository
	{
		protected override string[] PurgeScripts
		{
			get
			{
				return new string[] {
					StmNoteScript,
					StmLinkScript,
					GetGenericTablePurgeScript("JobDocAddress", "E2_ParentTableCode", "E2_ParentID"),
					GetGenericTablePurgeScript("GenCustomAddOnValue", "XV_ParentTableCode", "XV_ParentID"),
				};
			}
		}

		#region Purge Scripts

		#region StmNote

		const string StmNoteScript = @"
--StmNote
DECLARE @NoteParentTable sysname
DECLARE @NoteDeleteCommand nvarchar(1000)
DECLARE @PkColumn varchar(6)
DECLARE NoteParentTableCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
      SELECT DISTINCT sn.ST_Table, col.name
      FROM
            dbo.StmNote sn
            INNER JOIN sys.tables tab ON tab.name = sn.ST_Table
            INNER JOIN sys.columns col ON col.object_id = tab.object_id AND (col.name like '__[_]PK' OR col.name like '___[_]PK')
OPEN NoteParentTableCursor
FETCH NEXT FROM NoteParentTableCursor INTO @NoteParentTable, @PkColumn
WHILE @@FETCH_STATUS = 0
BEGIN
      SET @NoteDeleteCommand = 
            'DELETE dbo.StmNote ' +
            'WHERE ST_ParentID not in (SELECT ' + @PkColumn + ' FROM '+ @NoteParentTable + ') ' +
            'AND ST_Table = '''+ @NoteParentTable +''''

     EXEC (@NoteDeleteCommand)
     FETCH NEXT FROM NoteParentTableCursor INTO @NoteParentTable, @PkColumn
END
CLOSE NoteParentTableCursor
DEALLOCATE NoteParentTableCursor";

		const string StmLinkScript = @"
--StmLink
DELETE dbo.StmLink where not STL_ItemUrl = ''
";

		#endregion

		#region GenericTablePurgeScript

		string GetGenericTablePurgeScript(string tableName, string parentTablePrefixColumn, string parentIdColumn)
		{
			return String.Format(GenericTablePurgeScript, tableName, parentTablePrefixColumn, parentIdColumn);
		}

		const string GenericTablePurgeScript = @"
-- {0}
DECLARE @{0}ParentTableCode varchar(3)
DECLARE @{0}ParentTable sysname
DECLARE @{0}DeleteCommand nvarchar(1000)
DECLARE {0}TableCodeCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
	SELECT DISTINCT generictable.{1}, tab.name
	FROM {0} generictable
	INNER JOIN sys.columns col ON col.name = generictable.{1} + '_PK'
	INNER JOIN sys.tables tab ON tab.object_id = col.object_id AND tab.type = 'U' AND tab.name not like '%_CT'
OPEN {0}TableCodeCursor
FETCH NEXT FROM {0}TableCodeCursor INTO @{0}ParentTableCode, @{0}ParentTable
WHILE @@FETCH_STATUS = 0
BEGIN
	SET @{0}DeleteCommand = 
		'DELETE {0} ' +
		'WHERE {2} not in (SELECT ' + @{0}ParentTableCode + '_PK FROM '+ @{0}ParentTable + ') ' +
		'AND {1} = '''+ @{0}ParentTableCode +''''
	EXEC (@{0}DeleteCommand)
	FETCH NEXT FROM {0}TableCodeCursor INTO @{0}ParentTableCode, @{0}ParentTable
END
CLOSE {0}TableCodeCursor
DEALLOCATE {0}TableCodeCursor";

		#endregion

		#endregion
	}
}
