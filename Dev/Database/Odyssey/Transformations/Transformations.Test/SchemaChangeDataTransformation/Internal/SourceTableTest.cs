using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations
{
	sealed class SourceTableTest : TestWithTransformationDirectorCopyDb
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestSourceColumnConstructorThrowsExceptionIfExpectedTypeIsNull()
		{
			SourceColumn testColumn = new SourceColumn("AnyName", null);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestSourceColumnConstructorThrowsExceptionIfExpectedTypeIsBlank()
		{
			SourceColumn testColumn = new SourceColumn("AnyName", "     ");
		}

		public void TestFullName()
		{
			var testTable_dbo = new SourceTableForTesting(Db.DatabaseName, TestTableName, new SourceColumn[] { new SourceColumn("Col1", "int") }, null);
			var testTable_hrm = new SourceTableForTesting(Db.DatabaseName, "hrm", TestTableName, new SourceColumn[] { new SourceColumn("Col1", "int") }, null);

			AssertEquals("dbo table full name should follow expected pattern", true, Regex.IsMatch(testTable_dbo.FullName_Exposed, $"^{UpgUtils.UpgraderPrefix}T([0-9A-F]){{32}}_{testTable_dbo.OriginalName}$", RegexOptions.IgnoreCase));
			AssertEquals("hrm table full name should follow expected pattern", true, Regex.IsMatch(testTable_hrm.FullName_Exposed, $"^{UpgUtils.UpgraderPrefix}T([0-9A-F]){{32}}_{testTable_hrm.OriginalName}$", RegexOptions.IgnoreCase));
		}

		public void TestFullyQualifiedName()
		{
			SourceTableForTesting testTable = new SourceTableForTesting(Db.DatabaseName, TestTableName, new SourceColumn[] { new SourceColumn("Col1", "int") }, null);
			string expectedFullyQualifiedName = String.Format("[{0}]..[{1}]", testTable.DataCopyStorageDb_Exposed, testTable.FullName_Exposed);
			AssertEquals("FullyQualifiedName", expectedFullyQualifiedName, testTable.FullyQualifiedName);
		}

		public void TestOriginalFullyQualifiedName()
		{
			var testTable_dbo = new SourceTableForTesting(Db.DatabaseName, TestTableName, new SourceColumn[] { new SourceColumn("Col1", "int") }, null);
			var testTable_hrm = new SourceTableForTesting(Db.DatabaseName, "hrm", TestTableName, new SourceColumn[] { new SourceColumn("Col1", "int") }, null);

			AssertEquals("OriginalFullyQualifiedName for dbo table", $"[{Db.DatabaseName}].[dbo].[{TestTableName}]", testTable_dbo.OriginalFullyQualifiedName_Exposed);
			AssertEquals("OriginalFullyQualifiedName for hrm table", $"[{Db.DatabaseName}].[hrm].[{TestTableName}]", testTable_hrm.OriginalFullyQualifiedName_Exposed);
		}

		public void TestCreateAndPopulate()
		{
			SourceColumn codeColumn = new SourceColumn("SE_Code", "char(3)");
			SourceColumn descColumn = new SourceColumn("SE_Desc", "varchar(35)");
			SourceColumn inexistingColumn = new SourceColumn("InexistingColumnName", "int", "123");
			SourceTableForTesting sourceEventTable = new SourceTableForTesting(Db.DatabaseName, "StmEvent", new SourceColumn[] { codeColumn, descColumn, inexistingColumn }, codeColumn.Name + " in ('ADD','EDT','DEL')");

			bool doesTableCopyExist = DbObjectCreator.TableExists(Db.Connection, sourceEventTable.DataCopyStorageDb_Exposed, sourceEventTable.FullName_Exposed);
			AssertEquals("Copy of source table should NOT have been created yet", false, doesTableCopyExist);
			AssertEquals("CopyStatus (1)", SourceTable.CopyStatusEnum.NotCopied, sourceEventTable.CopyStatus);
			AssertEquals("IsTableCopied flag should NOT be set", false, sourceEventTable.IsTableCopied);

			sourceEventTable.Create();

			AssertEquals("CopyStatus (2)", SourceTable.CopyStatusEnum.CopiedAndPopulated, sourceEventTable.CopyStatus);
			AssertEquals("IsTableCopied flag should be set", true, sourceEventTable.IsTableCopied);
			AssertEquals("Table should have NO copy warnings", false, sourceEventTable.HasWarningsOnCopy);

			doesTableCopyExist = DbObjectCreator.TableExists(Db.Connection, sourceEventTable.DataCopyStorageDb_Exposed, sourceEventTable.FullName_Exposed);
			AssertEquals("Copy of source table should have been created", true, doesTableCopyExist);

			string sqlText = "SELECT count(*) FROM " + sourceEventTable.FullyQualifiedName;
			int rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of copied rows", 3, rowCount);

			sqlText = String.Format("SELECT {0} FROM {1} WHERE {2} = 'ADD'", descColumn.Name, sourceEventTable.FullyQualifiedName, codeColumn.Name);
			string addEventDescription = Db.Connection.ExecuteScalar(sqlText).ToString();
			AssertEquals("ADD Event Description", "Added a record to the system", addEventDescription);

			sqlText = String.Format("SELECT TOP 1 {0} FROM {1}", inexistingColumn.Name, sourceEventTable.FullyQualifiedName);
			int inexistingColumnValue = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Inexisting Column Value", 123, inexistingColumnValue);
		}

		[UseSnapshotProtection]
		public void TestCreateAndPopulate_HrmSchema()
		{
			PrepareHrmSchemaTestData();

			var reviewerColumn = new SourceColumn("GSV_GS_NKReviewer", "varchar(3)");
			var scoreColumn = new SourceColumn("GSV_Score", "tinyint");
			var commentsColumn = new SourceColumn("GSV_Comments", "nvarchar(MAX)");
			var inexistingColumn = new SourceColumn("InexistingColumnName", "int", "123");
			var sourceEventTable = new SourceTableForTesting(Db.DatabaseName, "hrm", "GlbStaffReview", new SourceColumn[] { reviewerColumn, scoreColumn, commentsColumn, inexistingColumn }, scoreColumn.Name + " >= (0)");

			var doesTableCopyExist = DbObjectCreator.TableExists(Db.Connection, sourceEventTable.DataCopyStorageDb_Exposed, sourceEventTable.FullName_Exposed);
			AssertEquals("Copy of source table should NOT have been created yet", false, doesTableCopyExist);
			AssertEquals("CopyStatus (1)", SourceTable.CopyStatusEnum.NotCopied, sourceEventTable.CopyStatus);
			AssertEquals("IsTableCopied flag should NOT be set", false, sourceEventTable.IsTableCopied);

			sourceEventTable.Create();

			AssertEquals("CopyStatus (2)", SourceTable.CopyStatusEnum.CopiedAndPopulated, sourceEventTable.CopyStatus);
			AssertEquals("IsTableCopied flag should be set", true, sourceEventTable.IsTableCopied);
			AssertEquals("Table should have NO copy warnings", false, sourceEventTable.HasWarningsOnCopy);

			doesTableCopyExist = DbObjectCreator.TableExists(Db.Connection, sourceEventTable.DataCopyStorageDb_Exposed, sourceEventTable.FullName_Exposed);
			AssertEquals("Copy of source table should have been created", true, doesTableCopyExist);

			var sqlText = "SELECT count(*) FROM " + sourceEventTable.FullyQualifiedName;
			var rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of copied rows", 4, rowCount);

			sqlText = string.Format("SELECT {0} FROM {1} WHERE {2} = 'RV1'", scoreColumn.Name, sourceEventTable.FullyQualifiedName, reviewerColumn.Name);
			var score = Db.Connection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Score is 11", 11, int.Parse(score));

			sqlText = string.Format("SELECT {0} FROM {1} WHERE {2} = 'RV4'", commentsColumn.Name, sourceEventTable.FullyQualifiedName, reviewerColumn.Name);
			var comment = Db.Connection.ExecuteScalar(sqlText).ToString();
			AssertEquals("Comment is \"Comment 4\"", "Comment 4", comment);

			sqlText = string.Format("SELECT TOP 1 {0} FROM {1}", inexistingColumn.Name, sourceEventTable.FullyQualifiedName);
			var inexistingColumnValue = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Inexisting Column Value", 123, inexistingColumnValue);
		}

		public void TestCreate_NoSourceTable()
		{
			var column1 = new SourceColumn("Column1", "int", dbReplacementValueIfNotExist: "NULL");
			var sourceTable = new SourceTableForTesting(Db.DatabaseName, TestTableName, new SourceColumn[] { column1 }, null);

			AssertEquals("[PRECONDITION] Original table exists?", false, DbObjectCreator.TableExists(Db.Connection, Db.DatabaseName, TestTableName));
			AssertEquals("[PRECONDITION] Original column exists?", false, DbObjectCreator.ColumnExists(Db.Connection, Db.DatabaseName, Db.SqlDbOwnerSchema, sourceTable.OriginalName, column1.Name));
			AssertEquals("[PRECONDITION] Copied table exists?", false, DbObjectCreator.TableExists(Db.Connection, sourceTable.DataCopyStorageDb_Exposed, sourceTable.FullName_Exposed));
			AssertEquals("[PRECONDITION] CopyStatus", SourceTable.CopyStatusEnum.NotCopied, sourceTable.CopyStatus);
			AssertEquals("[PRECONDITION] IsTableCopied", false, sourceTable.IsTableCopied);
			AssertEquals("[PRECONDITION] HasWarningsOnCopy", false, sourceTable.HasWarningsOnCopy);

			var e = AssertExceptionThrown<SqlException>(() => sourceTable.Create());
			AssertEquals(DbErrorType.InvalidObjectName, new DbErrorMatch(e).ExceptionType);

			AssertEquals("Original table exists?", false, DbObjectCreator.TableExists(Db.Connection, Db.DatabaseName, TestTableName));
			AssertEquals("Original column exists?", false, DbObjectCreator.ColumnExists(Db.Connection, Db.DatabaseName, Db.SqlDbOwnerSchema, sourceTable.OriginalName, column1.Name));
			AssertEquals("Copied table exists?", false, DbObjectCreator.TableExists(Db.Connection, sourceTable.DataCopyStorageDb_Exposed, sourceTable.FullName_Exposed));
			AssertEquals("CopyStatus", SourceTable.CopyStatusEnum.NotCopied, sourceTable.CopyStatus);
			AssertEquals("IsTableCopied", false, sourceTable.IsTableCopied);
			AssertEquals("HasWarningsOnCopy", false, sourceTable.HasWarningsOnCopy);
		}

		public void TestCreate_NoSourceColumnWithNoReplacement()
		{
			var column1 = new SourceColumn("Column1", "int", dbReplacementValueIfNotExist: null);
			var table = new SourceTableForTesting(Db.DatabaseName, "StmEvent", new SourceColumn[] { column1 }, null);

			AssertEquals("[PRECONDITION] Original table exists?", true, DbObjectCreator.TableExists(Db.Connection, Db.DatabaseName, table.OriginalName));
			AssertEquals("[PRECONDITION] Original column exists?", false, DbObjectCreator.ColumnExists(Db.Connection, Db.DatabaseName, Db.SqlDbOwnerSchema, table.OriginalName, column1.Name));
			AssertEquals("[PRECONDITION] Copied table exists?", false, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("[PRECONDITION] CopyStatus", SourceTable.CopyStatusEnum.NotCopied, table.CopyStatus);
			AssertEquals("[PRECONDITION] IsTableCopied", false, table.IsTableCopied);
			AssertEquals("[PRECONDITION] HasWarningsOnCopy", false, table.HasWarningsOnCopy);

			var e = AssertExceptionThrown<SqlException>(() => table.Create());
			AssertEquals(DbErrorType.InvalidColumnName, new DbErrorMatch(e).ExceptionType);

			AssertEquals("Original table exists?", true, DbObjectCreator.TableExists(Db.Connection, Db.DatabaseName, table.OriginalName));
			AssertEquals("Original column exists?", false, DbObjectCreator.ColumnExists(Db.Connection, Db.DatabaseName, Db.SqlDbOwnerSchema, table.OriginalName, column1.Name));
			AssertEquals("Copied table exists?", false, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("CopyStatus", SourceTable.CopyStatusEnum.NotCopied, table.CopyStatus);
			AssertEquals("IsTableCopied", false, table.IsTableCopied);
			AssertEquals("HasWarningsOnCopy", false, table.HasWarningsOnCopy);
		}

		public void TestCreate_NoSourceColumnWithReplacement()
		{
			var column1 = new SourceColumn("Column1", "int", dbReplacementValueIfNotExist: "NULL");
			var table = new SourceTableForTesting(Db.DatabaseName, "StmEvent", new SourceColumn[] { column1 }, null);

			AssertEquals("[PRECONDITION] Original table exists?", true, DbObjectCreator.TableExists(Db.Connection, Db.DatabaseName, table.OriginalName));
			AssertEquals("[PRECONDITION] Original column exists?", false, DbObjectCreator.ColumnExists(Db.Connection, Db.DatabaseName, Db.SqlDbOwnerSchema, table.OriginalName, column1.Name));
			AssertEquals("[PRECONDITION] Copied table exists?", false, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("[PRECONDITION] CopyStatus", SourceTable.CopyStatusEnum.NotCopied, table.CopyStatus);
			AssertEquals("[PRECONDITION] IsTableCopied", false, table.IsTableCopied);
			AssertEquals("[PRECONDITION] HasWarningsOnCopy", false, table.HasWarningsOnCopy);

			table.Create();

			AssertEquals("Original table exists?", true, DbObjectCreator.TableExists(Db.Connection, Db.DatabaseName, table.OriginalName));
			AssertEquals("Original column exists?", false, DbObjectCreator.ColumnExists(Db.Connection, Db.DatabaseName, Db.SqlDbOwnerSchema, table.OriginalName, column1.Name));
			AssertEquals("Copied table exists?", true, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("CopyStatus", SourceTable.CopyStatusEnum.CopiedAndPopulated, table.CopyStatus);
			AssertEquals("IsTableCopied", true, table.IsTableCopied);
			AssertEquals("HasWarningsOnCopy", false, table.HasWarningsOnCopy);
		}

		public void TestCreate_NoWhereClauseSourceColumn()
		{
			var column1 = new SourceColumn("Column1", "int", dbReplacementValueIfNotExist: "NULL");
			var table = new SourceTableForTesting(Db.DatabaseName, "StmEvent", new SourceColumn[] { column1 }, column1.Name + " > 0");

			AssertEquals("[PRECONDITION] Original table exists?", true, DbObjectCreator.TableExists(Db.Connection, Db.DatabaseName, table.OriginalName));
			AssertEquals("[PRECONDITION] Original column exists?", false, DbObjectCreator.ColumnExists(Db.Connection, Db.DatabaseName, Db.SqlDbOwnerSchema, table.OriginalName, column1.Name));
			AssertEquals("[PRECONDITION] Copied table exists?", false, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("[PRECONDITION] CopyStatus", SourceTable.CopyStatusEnum.NotCopied, table.CopyStatus);
			AssertEquals("[PRECONDITION] IsTableCopied", false, table.IsTableCopied);
			AssertEquals("[PRECONDITION] HasWarningsOnCopy", false, table.HasWarningsOnCopy);

			var e = AssertExceptionThrown<SqlException>(() => table.Create());
			AssertEquals(DbErrorType.InvalidColumnName, new DbErrorMatch(e).ExceptionType);

			AssertEquals("Original table exists?", true, DbObjectCreator.TableExists(Db.Connection, Db.DatabaseName, table.OriginalName));
			AssertEquals("Original column exists?", false, DbObjectCreator.ColumnExists(Db.Connection, Db.DatabaseName, Db.SqlDbOwnerSchema, table.OriginalName, column1.Name));
			AssertEquals("Copied table exists?", false, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("CopyStatus", SourceTable.CopyStatusEnum.NotCopied, table.CopyStatus);
			AssertEquals("IsTableCopied", false, table.IsTableCopied);
			AssertEquals("HasWarningsOnCopy", false, table.HasWarningsOnCopy);
		}

		public void TestCreateThrowsExceptionIfItIsAnSqlSyntaxError()
		{
			SourceColumn codeColumn = new SourceColumn("SE_Code", "char(3)");
			SourceColumn descColumn = new SourceColumn("SE_Desc", "varchar(35)");
			SourceTableForTesting sourceEventTable = new SourceTableForTesting(Db.DatabaseName, "StmEvent", new SourceColumn[] { codeColumn, descColumn }, "WhereClauseWithSyntaxError");

			bool doesTableCopyExist = DbObjectCreator.TableExists(Db.Connection, sourceEventTable.DataCopyStorageDb_Exposed, sourceEventTable.FullName_Exposed);
			AssertEquals("Copy of source table should NOT have been created yet", false, doesTableCopyExist);
			AssertEquals("CopyStatus (1)", SourceTable.CopyStatusEnum.NotCopied, sourceEventTable.CopyStatus);
			AssertEquals("IsTableCopied flag should NOT be set", false, sourceEventTable.IsTableCopied);

			try
			{
				sourceEventTable.Create();
				Fail("Should throw exception");
			}
			catch (SqlException e)
			{
				string expectedErrorMessage = "An expression of non-boolean type specified in a context where a condition is expected, near 'WhereClauseWithSyntaxError'.";

				AssertEquals("Wrong Exception caught - " + e.Message, expectedErrorMessage, e.Message);
			}

			AssertEquals("CopyStatus (2)", SourceTable.CopyStatusEnum.NotCopied, sourceEventTable.CopyStatus);
			AssertEquals("IsTableCopied flag should NOT be set", false, sourceEventTable.IsTableCopied);

			doesTableCopyExist = DbObjectCreator.TableExists(Db.Connection, sourceEventTable.DataCopyStorageDb_Exposed, sourceEventTable.FullName_Exposed);
			AssertEquals("Copy of source table should NOT have been created", false, doesTableCopyExist);
		}

		public void TestDrop()
		{
			var column1 = new SourceColumn("Column1", "int", dbReplacementValueIfNotExist: "NULL");
			var table = new SourceTableForTesting(Db.DatabaseName, "StmEvent", new SourceColumn[] { column1 }, null);

			AssertEquals("[PRECONDITION] Copied table exists?", false, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("[PRECONDITION] CopyStatus", SourceTable.CopyStatusEnum.NotCopied, table.CopyStatus);
			AssertEquals("[PRECONDITION] IsTableCopied", false, table.IsTableCopied);
			AssertEquals("[PRECONDITION] HasWarningsOnCopy", false, table.HasWarningsOnCopy);

			table.Create();

			AssertEquals("Copied table exists?", true, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("CopyStatus", SourceTable.CopyStatusEnum.CopiedAndPopulated, table.CopyStatus);
			AssertEquals("IsTableCopied", true, table.IsTableCopied);
			AssertEquals("HasWarningsOnCopy", false, table.HasWarningsOnCopy);

			table.Drop();

			AssertEquals("Copied table exists?", false, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("CopyStatus", SourceTable.CopyStatusEnum.NotCopied, table.CopyStatus);
			AssertEquals("IsTableCopied", false, table.IsTableCopied);
			AssertEquals("HasWarningsOnCopy", false, table.HasWarningsOnCopy);
		}

		public void TestDrop_HrmSchema()
		{
			var column1 = new SourceColumn("Column1", "int", dbReplacementValueIfNotExist: "NULL");
			var table = new SourceTableForTesting(Db.DatabaseName, "hrm", "GlbStaffClassification", new SourceColumn[] { column1 }, null);

			AssertEquals("[PRECONDITION] Copied table exists?", false, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("[PRECONDITION] CopyStatus", SourceTable.CopyStatusEnum.NotCopied, table.CopyStatus);
			AssertEquals("[PRECONDITION] IsTableCopied", false, table.IsTableCopied);
			AssertEquals("[PRECONDITION] HasWarningsOnCopy", false, table.HasWarningsOnCopy);

			table.Create();

			AssertEquals("Copied table exists?", true, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("CopyStatus", SourceTable.CopyStatusEnum.CopiedAndPopulated, table.CopyStatus);
			AssertEquals("IsTableCopied", true, table.IsTableCopied);
			AssertEquals("HasWarningsOnCopy", false, table.HasWarningsOnCopy);

			table.Drop();

			AssertEquals("Copied table exists?", false, DbObjectCreator.TableExists(Db.Connection, table.DataCopyStorageDb_Exposed, table.FullName_Exposed));
			AssertEquals("CopyStatus", SourceTable.CopyStatusEnum.NotCopied, table.CopyStatus);
			AssertEquals("IsTableCopied", false, table.IsTableCopied);
			AssertEquals("HasWarningsOnCopy", false, table.HasWarningsOnCopy);
		}

		public void TestCreateAndDropInexistingOriginalTable()
		{
			SourceColumn column1 = new SourceColumn("Column1", "int");
			SourceColumn column2 = new SourceColumn("Column2", "int");
			SourceTableForTesting testSourceTable = new SourceTableForTesting(Db.DatabaseName, TestTableName, new SourceColumn[] { column1, column2 }, null);

			AssertEquals("[PRE-CONDITION] Original table should NOT exist", false, DbObjectCreator.TableExists(TestConnection, testSourceTable.OriginalName));
			((ISourceTableTestHelper)testSourceTable).CreateOriginalTableAndOrColumnsIfNotExist();
			AssertEquals("Original table should have been created", true, DbObjectCreator.TableExists(TestConnection, testSourceTable.OriginalName));
			AssertEquals("column1 should have been created", true, DbObjectCreator.ColumnExists(Db.Connection, testSourceTable.OriginalName, column1.Name));
			AssertEquals("column2 should have been created", true, DbObjectCreator.ColumnExists(Db.Connection, testSourceTable.OriginalName, column2.Name));
			((ISourceTableTestHelper)testSourceTable).DropCreatedOriginalTableAndOrColumnsAfterCopyingData();
			AssertEquals("Original table should have been dropped", false, DbObjectCreator.TableExists(TestConnection, testSourceTable.OriginalName));
		}

		public void TestCreateAndDropExistingOriginalTableWithInexistingColumn()
		{
			SourceColumn column1 = new SourceColumn("SE_PK", "uniqueidentifier");
			SourceColumn column2 = new SourceColumn("Column2", "int");
			SourceTableForTesting testSourceTable = new SourceTableForTesting(Db.DatabaseName, "StmEvent", new SourceColumn[] { column1, column2 }, null);

			AssertEquals("[PRE-CONDITION] Original table should exist", true, DbObjectCreator.TableExists(TestConnection, testSourceTable.OriginalName));
			AssertEquals("[PRE-CONDITION] pkColumn should exist", true, DbObjectCreator.ColumnExists(Db.Connection, testSourceTable.OriginalName, column1.Name));
			AssertEquals("[PRE-CONDITION] inexistingColumn should NOT exist", false, DbObjectCreator.ColumnExists(Db.Connection, testSourceTable.OriginalName, column2.Name));
			((ISourceTableTestHelper)testSourceTable).CreateOriginalTableAndOrColumnsIfNotExist();
			AssertEquals("Original table should still exist (1)", true, DbObjectCreator.TableExists(TestConnection, testSourceTable.OriginalName));
			AssertEquals("column1 should still exist (1)", true, DbObjectCreator.ColumnExists(Db.Connection, testSourceTable.OriginalName, column1.Name));
			AssertEquals("column2 should have been created", true, DbObjectCreator.ColumnExists(Db.Connection, testSourceTable.OriginalName, column2.Name));
			((ISourceTableTestHelper)testSourceTable).DropCreatedOriginalTableAndOrColumnsAfterCopyingData();
			AssertEquals("Original table should still exist (2)", true, DbObjectCreator.TableExists(TestConnection, testSourceTable.OriginalName));
			AssertEquals("column1 should still exist (2)", true, DbObjectCreator.ColumnExists(Db.Connection, testSourceTable.OriginalName, column1.Name));
			AssertEquals("column2 should have been dropped", false, DbObjectCreator.ColumnExists(Db.Connection, testSourceTable.OriginalName, column2.Name));
		}

		void PrepareHrmSchemaTestData()
		{
			var staff1Pk = Guid.NewGuid();
			var staff2Pk = Guid.NewGuid();
			var staff3Pk = Guid.NewGuid();
			var staff4Pk = Guid.NewGuid();

			var sqlInsertTemplate = @"
  INSERT INTO dbo.GlbStaff([GS_PK], [GS_Code], [GS_LoginName], GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
  VALUES ('{0}', '{1}', '{2}', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

  INSERT INTO [hrm].[GlbStaffReview]([GSV_PK], [GSV_GS_Staff], [GSV_EffectiveDate], [GSV_GS_NKReviewer], [GSV_Score], [GSV_Comments], [GSV_SystemCreateTimeUtc], [GSV_SystemCreateUser], [GSV_SystemLastEditTimeUtc], [GSV_SystemLastEditUser]) 
  VALUES (NEWID(), '{0}', GETDATE(), '{3}', '{4}', '{5}', GETDATE(), 'TU1', GETDATE(), 'TU2');";

			var sql = new SqlQueryBuilder();
			sql.AppendLine(string.Format(CultureInfo.InvariantCulture, sqlInsertTemplate, staff1Pk, "TS1", "TestStaff1", "RV1", 11, "Comment 1"));
			sql.AppendLine(string.Format(CultureInfo.InvariantCulture, sqlInsertTemplate, staff2Pk, "TS2", "TestStaff2", "RV2", 22, "Comment 2"));
			sql.AppendLine(string.Format(CultureInfo.InvariantCulture, sqlInsertTemplate, staff3Pk, "TS3", "TestStaff3", "RV3", 33, "Comment 3"));
			sql.AppendLine(string.Format(CultureInfo.InvariantCulture, sqlInsertTemplate, staff4Pk, "TS4", "TestStaff4", "RV4", 44, "Comment 4"));

			TestConnection.Command(sql.ToStringWithNewLineBetweenAppends()).ExecuteNonQuery();
		}

		const string TestTableName = "SourceTableTest_TestTable";

		protected override void SetUp()
		{
			base.SetUp();

			disposableAdminConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();
		}

		protected override void TearDown()
		{
			disposableAdminConnection.Dispose();

			base.TearDown();
		}

		IDisposable disposableAdminConnection;
	}
}
