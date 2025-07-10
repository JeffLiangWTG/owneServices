using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.StmNote;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.StmNote.Testing
{
	[TestedType(typeof(TG_CheckDuplicateNote))]
	class TG_CheckDuplicateNoteTest : DbCreateScriptTest
	{
	}

	class Trigger_TG_CheckDuplicateNoteTest : TransactionedTestCase
	{
		public void TestFailedInsertDoesNotRollBackTheTransaction()
		{
			using (var cmd = Db.Connection.Command(string.Format(@"INSERT INTO dbo.StmNote (ST_PK, ST_ParentId, ST_Table, ST_Description, ST_NoteType, ST_NoteContext, ST_IsCustomDescription) Values ('{0}','{1}','','{2}','','AAA',0)", Guid.Empty, Guid.Empty, "")))
			{
				cmd.ExecuteNonQuery();
				try
				{
					cmd.ExecuteNonQuery();
				}
				catch (SqlException)
				{
				}
				AssertEquals(true, Db.Connection.IsInTransaction);
			}
		}

		public void TestDuplicateNoteCannotBeInsertedIntoDatabase()
		{
			string uniqueNoteDescription = "Container Release Note";

			Guid orgHeaderPK = Guid.Empty;
			using (var cmd = Db.Connection.Command("SELECT top 1 OH_PK FROM dbo.OrgHeader"))
			{
				orgHeaderPK = (Guid)cmd.ExecuteScalar();
			}

			Guid note1PK = Guid.NewGuid();
			using (var cmd = Db.Connection.Command(ConstructInsertSql(note1PK, orgHeaderPK, uniqueNoteDescription)))
			{
				cmd.ExecuteNonQuery();
				AssertEquals("Note is inserted into database", 1, (int)Db.Connection.ExecuteScalar($"SELECT Count(0) FROM dbo.StmNote Where ST_PK = '{note1PK}'"));
			}

			Guid note2PK = Guid.NewGuid();
			using (var cmd = Db.Connection.Command(ConstructInsertSql(note2PK, orgHeaderPK, uniqueNoteDescription)))
			{
				var exceptionSql = AssertExceptionThrown<SqlException>(() => cmd.ExecuteNonQuery());
				AssertContains("Failed to save a note into the database because there is already another note existing with the same description and Context Module for this job.", exceptionSql.Message);
				AssertEquals("Duplicate note is not inserted into database", 0, (int)Db.Connection.ExecuteScalar($"SELECT Count(0) FROM dbo.StmNote Where ST_PK = '{note2PK}'"));
			}
		}

		string ConstructInsertSql(Guid notePK, Guid parentPK, string description)
		{
			return string.Format(@"INSERT INTO dbo.StmNote
(ST_PK, ST_ParentId,ST_Table,ST_Description,ST_NoteType,ST_NoteContext,ST_IsCustomDescription)
Values
('{0}','{1}','OrgHeader','{2}','PUB','AAA',0)", notePK, parentPK, description);
		}
	}
}

