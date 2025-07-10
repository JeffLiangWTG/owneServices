using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class DbIndexReaderTest : TestCaseWithFactory
	{
		public void TestReadingIndexes()
		{
			DbIndexReader reader = new DbIndexReader("StmALog");
			DbIndex[] indexes = reader.Indexes;
			AssertEquals("At least 2 indexes should be found", true, indexes.Length >= 2);

			AssertEquals(1, indexes[0].ColumnNames.Length);
			AssertEquals(StmALogSchema.SL_PostedTimeUtc.Name, indexes[0].ColumnNames[0]);
			AssertEquals(3, indexes[1].ColumnNames.Length);
			AssertEquals(StmALogSchema.SL_Parent.Name, indexes[1].ColumnNames[0]);
			AssertEquals(StmALogSchema.SL_SE_NKEvent.Name, indexes[1].ColumnNames[1]);
			AssertEquals(StmALogSchema.SL_EventTime.Name, indexes[1].ColumnNames[2]);
		}

		public void TestReadingIndexesUsingFullyQualifiedTableName()
		{
			var allDbs = TestConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW);
			string dbName = (allDbs.Count() > 1) ? allDbs.Skip(1).First() : TestConnection.CurrentDatabase;

			string sqlText = String.Format(@"
				CREATE TABLE [{0}].dbo.[~TestReadingIndexesFromDifferentDatabase~] (Col1 int);
				CREATE INDEX [~Test~] ON [{0}].dbo.[~TestReadingIndexesFromDifferentDatabase~] (Col1);",
				dbName);
			TestConnection.ExecuteNonQuery(sqlText);

			var reader = new DbIndexReader(dbName + ".dbo.~TestReadingIndexesFromDifferentDatabase~");
			var indexes = reader.Indexes;
			AssertEquals("Index count", 1, indexes.Length);
			AssertEquals("Index key count", 1, indexes[0].ColumnNames.Length);
			AssertEquals("Index column name", "Col1", indexes[0].ColumnNames[0]);
		}

		public void TestIsIndexed_WithSchemaColumn()
		{
			DbIndexReader reader = new DbIndexReader("StmALog");
			AssertEquals("SL_Parent belongs to an index", true, reader.IsIndexed(StmALogSchema.SL_Parent));
			AssertEquals("SL_PK is not indexed", false, reader.IsIndexed(StmALogSchema.PK));
		}

		public void TestIsIndexed_WithColumnNameArray()
		{
			DbIndexReader reader = new DbIndexReader("StmALog");
			AssertEquals("SL_PostedTimeUtc is indexed", true, reader.IsIndexed(new string[] { StmALogSchema.SL_PostedTimeUtc.Name }));
			AssertEquals("SL_PK is not indexed", false, reader.IsIndexed(new string[] { StmALogSchema.PK.Name }));
			AssertEquals("(SL_PostedTimeUtc, SL_PK) is not indexed", false, reader.IsIndexed(new string[] { StmALogSchema.SL_EventTime.Name, StmALogSchema.PK.Name }));
			AssertEquals("(SL_Parent, SL_SE_NKEvent, SL_EventTime) is indexed", true, reader.IsIndexed(new string[] { StmALogSchema.SL_Parent.Name, StmALogSchema.SL_SE_NKEvent.Name, StmALogSchema.SL_EventTime.Name }));
		}
	}
}
