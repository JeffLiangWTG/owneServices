using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(TG_GenAddOnColumn_XA_Data_Date_SmallDateTime))]
	class TG_GenAddOnColumn_XA_Data_Date_SmallDateTimeTest : DbCreateScriptTest
	{
		const string TriggerMessage = "When XA_Type is 'DAT', then XA_Data must be in a range of 1900-01-01 00:00:00.000 to 2079-06-06 23:59:29.000, and in format 'yyyy-mm-dd hh:mi:ss.mmm'.  When XA_Type is 'DTE', then XA_Data must be in a range of 0001-01-01 to 9999-12-31, and in format 'yyyy-mm-dd'.";

		[ExpectNoExceptions]
		public void TestInsertDAT_Valid()
		{
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GenAddOnColumn([XA_PK], [XA_Name], [XA_Type], [XA_Data], [XA_ParentTableCode], [XA_ParentID])
VALUES(NEWID(), 'TEST', 'DAT', '2069-06-06 23:59:30.000', 'JE', NEWID())");
		}

		public void TestInsertDAT_EmptyData()
		{
			var errorMessage = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GenAddOnColumn([XA_PK], [XA_Name], [XA_Type], [XA_Data], [XA_ParentTableCode], [XA_ParentID])
VALUES(NEWID(), 'TEST', 'DAT', '', 'JE', NEWID())")).Message;
			AssertContains(TriggerMessage, errorMessage);
		}

		public void TestInsertDAT_InvalidData()
		{
			var errorMessage = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GenAddOnColumn([XA_PK], [XA_Name], [XA_Type], [XA_Data], [XA_ParentTableCode], [XA_ParentID])
VALUES(NEWID(), 'TEST', 'DAT', '1899-12-31 23:59:29.998', 'JE', NEWID())")).Message;
			AssertContains(TriggerMessage, errorMessage);
		}

		[ExpectNoExceptions]
		public void TestUpdateDAT_Valid()
		{
			TestConnection.ExecuteNonQuery(@"UPDATE dbo.GenAddOnColumn SET XA_Data = '2059-06-06 23:59:30.000', XA_SystemLastEditUser = 'E', XA_SystemLastEditTimeUtc = GetDate() WHERE XA_Name = 'TESTDAT'");
		}

		public void TestUpdateDAT_EmptyData()
		{
			var errorMessage = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(@"
UPDATE dbo.GenAddOnColumn SET XA_Data = '', XA_Type = 'DAT', XA_SystemLastEditUser = 'E', XA_SystemLastEditTimeUtc = GetDate() WHERE XA_Name = 'TESTDAT'")).Message;
			AssertContains(TriggerMessage, errorMessage);
		}

		public void TestUpdateDAT_InvalidData()
		{
			var errorMessage = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(@"
UPDATE dbo.GenAddOnColumn SET XA_Data = '1899-12-31 23:59:29.998', XA_SystemLastEditUser = 'E', XA_SystemLastEditTimeUtc = GetDate() WHERE XA_Name = 'TESTDAT'")).Message;
			AssertContains(TriggerMessage, errorMessage);
		}

		[ExpectNoExceptions]
		public void TestInsertDTE_Valid()
		{
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GenAddOnColumn([XA_PK], [XA_Name], [XA_Type], [XA_Data], [XA_ParentTableCode], [XA_ParentID])
VALUES(NEWID(), 'TEST', 'DTE', '2069-06-06', 'JE', NEWID())");
		}

		public void TestInsertDTE_EmptyData()
		{
			var errorMessage = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GenAddOnColumn([XA_PK], [XA_Name], [XA_Type], [XA_Data], [XA_ParentTableCode], [XA_ParentID])
VALUES(NEWID(), 'TEST', 'DTE', '', 'JE', NEWID())")).Message;
			AssertContains(TriggerMessage, errorMessage);
		}

		public void TestInsertDTE_InvalidData()
		{
			var errorMessage = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GenAddOnColumn([XA_PK], [XA_Name], [XA_Type], [XA_Data], [XA_ParentTableCode], [XA_ParentID])
VALUES(NEWID(), 'TEST', 'DTE', '1899-13-31', 'JE', NEWID())")).Message;
			AssertContains(TriggerMessage, errorMessage);
		}

		[ExpectNoExceptions]
		public void TestUpdateDTE_Valid()
		{
			TestConnection.ExecuteNonQuery(@"UPDATE dbo.GenAddOnColumn SET XA_Data = '2059-06-06', XA_SystemLastEditUser = 'E', XA_SystemLastEditTimeUtc = GetDate() WHERE XA_Name = 'TESTDTE'");
		}

		public void TestUpdateDTE_EmptyData()
		{
			var errorMessage = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(@"
UPDATE dbo.GenAddOnColumn SET XA_Data = '', XA_Type = 'DTE', XA_SystemLastEditUser = 'E', XA_SystemLastEditTimeUtc = GetDate() WHERE XA_Name = 'TESTDTE'")).Message;
			AssertContains(TriggerMessage, errorMessage);
		}

		public void TestUpdateDTE_InvalidData()
		{
			var errorMessage = AssertExceptionThrown<SqlException>(() => TestConnection.ExecuteNonQuery(@"
UPDATE dbo.GenAddOnColumn SET XA_Data = '1899-13-31', XA_SystemLastEditUser = 'E', XA_SystemLastEditTimeUtc = GetDate() WHERE XA_Name = 'TESTDTE'")).Message;
			AssertContains(TriggerMessage, errorMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GenAddOnColumn([XA_PK], [XA_Name], [XA_Type], [XA_Data], [XA_ParentTableCode], [XA_ParentID])
VALUES(NEWID(), 'TESTDAT', 'DAT', '2069-06-06 23:59:30.000', 'JE', NEWID())
INSERT INTO dbo.GenAddOnColumn([XA_PK], [XA_Name], [XA_Type], [XA_Data], [XA_ParentTableCode], [XA_ParentID])
VALUES(NEWID(), 'TESTDTE', 'DTE', '2069-06-06', 'JE', NEWID())");
		}
	}
}
