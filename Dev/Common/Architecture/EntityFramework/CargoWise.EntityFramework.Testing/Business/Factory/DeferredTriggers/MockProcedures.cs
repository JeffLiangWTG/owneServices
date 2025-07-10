using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	enum TriggerProcedure
	{
		SuspendTrigger,
		ResumeTrigger
	}

	class MockProcedures
	{
		static readonly string GuidToFilterResultsBy = "94224e76-55a7-48c8-86be-8275986db3d5";
		static readonly string GuidToFilterExtraParams = "5204532a-43ab-48a2-bd31-43f16ba83c58";

		public static IDisposable CreateNewMockProcedure(DbConnection connection, string procedureName, int extraParamsCount = 0)
		{
			var extraParamsWithType = ", @ExtraParam0 INT";
			var concatExtraParamsName = "@ExtraParam0";
			if (extraParamsCount > 1)
			{
				extraParamsWithType = "";
				concatExtraParamsName = "concat(";
				for (var i = 0; i < extraParamsCount; i++)
				{
					extraParamsWithType += $", @ExtraParam{i} INT";
					concatExtraParamsName += $"@ExtraParam{i},'|',";
				}
				concatExtraParamsName = concatExtraParamsName.Substring(0, concatExtraParamsName.Length - 5) + ")";
			}

			var mockProcedure = $@"
CREATE PROC {procedureName} (@ValuesToCheck dbo.TVP_uniqueidentifier READONLY {(extraParamsCount > 0 ? extraParamsWithType : "")}) AS
BEGIN
	DECLARE @result varchar(max) = (select dbo.CLRCssvAgg(Value) from @ValuesToCheck)
	INSERT INTO dbo.StmNote (ST_PK, ST_ParentID, ST_Table, ST_NoteText)
	VALUES (newID(), '{GuidToFilterResultsBy}', '{procedureName}', @result)

	{(extraParamsCount > 0 ? $"INSERT INTO dbo.StmNote (ST_PK, ST_ParentID, ST_Table, ST_NoteText) VALUES (newID(), '{GuidToFilterExtraParams}', '{procedureName}', {concatExtraParamsName})" : "")}

	RETURN 1
END";

			return new DisposableAction(
				() => connection.ExecuteNonQuery(mockProcedure),
				() => connection.ExecuteNonQuery(
					$@"DELETE FROM dbo.StmNote WHERE ST_ParentID in (@id1, @id2); DROP PROC {procedureName}",
					cmd =>
					{
						cmd.AddParameterBasedOnDbColumn("@id1", new Guid(GuidToFilterResultsBy), StmNoteSchema.ST_ParentID);
						cmd.AddParameterBasedOnDbColumn("@id2", new Guid(GuidToFilterExtraParams), StmNoteSchema.ST_ParentID);
					}));
		}

		public static IDisposable CreateFailingMockProcedure(DbConnection connection, string procedureName, string errorMessagePrefix = "")
		{
			var mockProcedure = $@"
CREATE PROC {procedureName} (@PKsToCheck dbo.TVP_uniqueidentifier READONLY) AS
BEGIN
	RAISERROR('{errorMessagePrefix}Dummy Error Message', 16, 1)
	RETURN 1
END";

			return new DisposableAction(
				() => connection.ExecuteNonQuery(mockProcedure),
				() => connection.ExecuteNonQuery($@"DROP PROC {procedureName}"));
		}

		public static IDisposable MockExistingProcedure(DbConnection connection, TriggerProcedure procedure)
		{
			var procedureName = GetProcedureName(procedure);

			var existingProcedure = connection.ExecuteScalar($@"
SELECT
    definition
FROM
    sys.sql_modules
WHERE
    objectproperty(OBJECT_ID, 'IsProcedure') = 1
	and OBJECT_NAME(OBJECT_ID) = '{procedureName}'").ToString();

			var mockProcedure = $@"
ALTER PROC {procedureName} (@TriggerName varchar(128)) AS
BEGIN
	INSERT INTO dbo.StmNote (ST_PK, ST_ParentID, ST_Table, ST_NoteText)
	VALUES (newID(), '{GuidToFilterResultsBy}', '{procedureName}', @TriggerName)
	RETURN 1
END";

			return new DisposableAction(
				() => connection.ExecuteNonQuery(mockProcedure),
				() =>
				{
					connection.ExecuteNonQuery(existingProcedure.Replace("CREATE PROC", "ALTER PROC"));
					connection.ExecuteNonQuery(
						$"DELETE FROM dbo.StmNote WHERE ST_ParentID = @pk and ST_Table = @table",
						cmd =>
						{
							cmd.AddParameterBasedOnDbColumn("@pk", new Guid(GuidToFilterResultsBy), StmNoteSchema.ST_ParentID);
							cmd.AddParameterBasedOnDbColumn("@table", procedureName, StmNoteSchema.ST_Table);
						});
				});
		}

		public static IDisposable CreateNewMockTrigger(DbConnection connection, string triggerName)
		{
			var mockedTrigger = $@"
CREATE TRIGGER {triggerName} on DummyBizo AFTER DELETE
AS
BEGIN
	RETURN
END";

			return new DisposableAction(
				() => connection.ExecuteNonQuery(mockedTrigger),
				() => connection.ExecuteNonQuery($@"DROP TRIGGER {triggerName}"));
		}

		static string GetProcedureName(TriggerProcedure procedure)
		{
			switch (procedure)
			{
				case TriggerProcedure.SuspendTrigger:
					return nameof(TriggerProcedure.SuspendTrigger);
				case TriggerProcedure.ResumeTrigger:
					return nameof(TriggerProcedure.ResumeTrigger);
				default:
					throw new InvalidOperationException($"Unknown TriggerProcedure Type: {procedure}.");
			}
		}

		public static IEnumerable<string> ReadValuesPassedToProcedure(DbConnection connection, TriggerProcedure procedure)
		{
			var procedureName = GetProcedureName(procedure);
			return ReadValuesPassedToProcedure(connection, procedureName);
		}

		public static IEnumerable<string> ReadValuesPassedToProcedure(DbConnection connection, string procedureName)
		{
			var cmd = connection.Command("SELECT ST_NoteText FROM dbo.StmNote WHERE ST_ParentID = @id AND ST_Table = @table");
			cmd.AddParameterBasedOnDbColumn("@id", new Guid(GuidToFilterResultsBy), StmNoteSchema.ST_ParentID);
			cmd.AddParameterBasedOnDbColumn("@table", procedureName, StmNoteSchema.ST_Table);

			var values = new List<string>();
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					values.Add((string)reader[StmNoteSchema.Constants.ST_NoteText]);
				}
			}

			connection.ExecuteNonQuery(
				$"DELETE FROM dbo.StmNote WHERE ST_ParentID = @pk and ST_Table = @table",
				cmd =>
				{
					cmd.AddParameterBasedOnDbColumn("@pk", new Guid(GuidToFilterResultsBy), StmNoteSchema.ST_ParentID);
					cmd.AddParameterBasedOnDbColumn("@table", procedureName, StmNoteSchema.ST_Table);
				});
			return values;
		}

		public static string ReadExtraParamsPassedToProcedure(DbConnection connection, string procedureName)
		{
			var cmd = connection.Command($"Select ST_NoteText from dbo.StmNote WHERE ST_ParentID = @pk and ST_Table = @table");
			cmd.AddParameterBasedOnDbColumn("@pk", new Guid(GuidToFilterExtraParams), StmNoteSchema.ST_ParentID);
			cmd.AddParameterBasedOnDbColumn("@table", procedureName, StmNoteSchema.ST_Table);
			string result = "";
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					result = (string)reader[StmNoteSchema.Constants.ST_NoteText];
				}
			}

			connection.ExecuteNonQuery(
				$"DELETE FROM dbo.StmNote WHERE ST_ParentID = @pk and ST_Table = @table",
				cmd => {
					cmd.AddParameterBasedOnDbColumn("@pk", new Guid(GuidToFilterExtraParams), StmNoteSchema.ST_ParentID);
					cmd.AddParameterBasedOnDbColumn("@table", procedureName, StmNoteSchema.ST_Table);
				});
			return string.Join(",", result.Split('|'));
		}
	}
}
