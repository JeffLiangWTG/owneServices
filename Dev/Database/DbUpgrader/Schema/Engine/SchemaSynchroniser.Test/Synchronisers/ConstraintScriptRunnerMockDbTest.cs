using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ConstraintScriptRunnerMockDbTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		public void TestDropCheckConstraints()
		{
			// Arrange
			RunActionOnMockMainDb(() =>
			{
				var testScriptRunner = new ConstraintScriptRunner(TestConnection, mockMainDb, mockTemplateDb);

				// Act
				testScriptRunner.DropCheckConstraints();

				// Assert
				AssertCheckConstraintExists(mockMainDb, false, "MatchTable01", "CK_MatchTable01");
				AssertCheckConstraintExists(mockMainDb, false, "MatchTable02", "CK_MatchTable02");
				AssertCheckConstraintExists(mockMainDb, true, "UnmatchTable01", "CK_UnmatchTable01");
			});
		}

		public void TestCreateCheckConstraints()
		{
			// Arrange
			RunActionOnMockMainDb(() =>
			{
				var testScriptRunner = new ConstraintScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
				testScriptRunner.DropCheckConstraints();

				// Act
				testScriptRunner.CreateCheckConstraints();

				// Assert
				AssertCheckConstraintExists(mockMainDb, false, "MatchTable01", "CK_MatchTable01");
				AssertCheckConstraintExists(mockMainDb, true, "MatchTable02", "CK_MatchTable02");
				AssertCheckConstraintExists(mockMainDb, true, "MatchTable02", "CK_MatchTable03");
				AssertCheckConstraintExists(mockMainDb, true, "MatchTable02", "CK_MatchTable02_NoCheck");
				AssertCheckConstraintExists(mockMainDb, true, "UnmatchTable01", "CK_UnmatchTable01");

				ConstraintScriptRunnerTransactionalTest.AssertConstraintIsEnabledAndIsTrustedStatus(TestConnection, "CK_MatchTable02", true, true);
				ConstraintScriptRunnerTransactionalTest.AssertConstraintIsEnabledAndIsTrustedStatus(TestConnection, "CK_MatchTable03", true, true);
				ConstraintScriptRunnerTransactionalTest.AssertConstraintIsEnabledAndIsTrustedStatus(TestConnection, "CK_MatchTable02_NoCheck", true, false);
				ConstraintScriptRunnerTransactionalTest.AssertConstraintIsEnabledAndIsTrustedStatus(TestConnection, "CK_UnmatchTable01", true, true);
			});
		}

		void AssertCheckConstraintExists(string dbName, bool expected, string tableName, string checkName)
		{
			string sqlText = String.Format(@"
				SELECT count(*) FROM [{0}].information_schema.table_constraints
				WHERE table_name = '{1}'
				AND CONSTRAINT_NAME = '{2}'
				AND CONSTRAINT_TYPE = 'CHECK'",
				dbName, tableName, checkName);
			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			string assertMessage = String.Format("Check Constraint: [{0}].[{1}] exists?", tableName, checkName);
			AssertEquals(assertMessage, expected, qtyRows == 1);
		}

		public void TestSynchroniseAllDefaultConstraints()
		{
			AssertDefaultConstraintExistInMainDb(true, "MatchTable01", "Col2", "Y");
			AssertDefaultConstraintExistInMainDb(true, "MatchTable02", "Col2", "A");
			AssertDefaultConstraintExistInMainDb(false, "MatchTable02", "Col2", "B");
			AssertDefaultConstraintExistInMainDb(true, "UnmatchTable01", "Col3", "");

			using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
			{
				ConstraintScriptRunner testScriptRunner = new ConstraintScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
				testScriptRunner.SynchroniseAllDefaultConstraints();
			}

			AssertDefaultConstraintExistInMainDb(false, "MatchTable01", "Col2", "Y");
			AssertDefaultConstraintExistInMainDb(false, "MatchTable02", "Col2", "A");
			AssertDefaultConstraintExistInMainDb(true, "MatchTable02", "Col2", "B");
			AssertDefaultConstraintExistInMainDb(true, "UnmatchTable01", "Col3", "");
		}

		void AssertDefaultConstraintExistInMainDb(bool expected, string tableName, string columnName, string defaultValue)
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM [{0}].INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}'
				AND COLUMN_DEFAULT = '(''{3}'')'",
				mockMainDb, tableName, columnName, defaultValue);
			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			string assertMessage = String.Format("Default: [{0}].[{1}] = '{2}' exists?", tableName, columnName, defaultValue);
			AssertEquals(assertMessage, expected, qtyRows == 1);
		}

		public void TestSynchroniseAllForeignKeys()
		{
			AssertForeignKeyExistInMainDb(true, "FK_MatchTable02_TO_MatchTable01", "PK_MatchTable01", "NO ACTION");
			AssertForeignKeyExistInMainDb(true, "FK_MatchTable01_TO_MatchTable01", "PK_MatchTable01", "NO ACTION");
			AssertForeignKeyExistInMainDb(true, "FK_UnmatchTable01_TO_MatchTable01", "PK_MatchTable01", "NO ACTION");
			AssertForeignKeyExistInMainDb(false, "FK_MatchTable02_TO_MatchTable02", "PK_MatchTable02", null);
			AssertContainsExactElementsInAnyOrder(new[] { ("Col4", "Col4"), ("Col5", "Col5") }, GetReferenceKeys("FK_MatchTable02_TO_MatchTable01_Composite"));

			using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
			{
				ConstraintScriptRunner testScriptRunner = new ConstraintScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
				testScriptRunner.SynchroniseAllForeignKeys();
			}

			AssertForeignKeyExistInMainDb(true, "FK_MatchTable02_TO_MatchTable01", "PK_MatchTable01", "CASCADE");
			AssertForeignKeyExistInMainDb(false, "FK_MatchTable01_TO_MatchTable01", "PK_MatchTable01", null);
			AssertForeignKeyExistInMainDb(false, "FK_UnmatchTable01_TO_MatchTable01", "PK_MatchTable01", null);
			AssertForeignKeyExistInMainDb(true, "FK_MatchTable02_TO_MatchTable02", "PK_MatchTable02", "NO ACTION");
			AssertForeignKeyExistInMainDb(false, "FK_MatchTable02_TO_MatchTable01_Composite", "PK_MatchTable02", null);

			var selfReferencingForeignKey = GetReferenceKeys("TrickyForeignKey_FK2_TrickyForeignKey_RRR_120N");
			AssertContainsExactElementsInAnyOrder(new[] { ("Col3", "Col3"), ("Col4", "Col2") }, selfReferencingForeignKey);
		}

		IEnumerable<(string from, string to)> GetReferenceKeys(string constraintName)
		{
			string sql = $@"
				SELECT fkCol.name, rkCol.name
				FROM [{mockMainDb}].sys.foreign_key_columns cnstCol1
				INNER JOIN [{mockMainDb}].sys.columns fkCol
					ON fkCol.object_id = cnstCol1.parent_object_id
					AND fkCol.column_id = cnstCol1.parent_column_id
				INNER JOIN [{mockMainDb}].sys.columns rkCol
					ON rkCol.object_id = cnstCol1.referenced_object_id
					AND rkCol.column_id = cnstCol1.referenced_column_id
				JOIN [{mockMainDb}].sys.foreign_keys cnst on cnstCol1.constraint_object_id=cnst.object_id
				WHERE cnst.name='{constraintName}'
				";

			var result = new List<(string, string)>();
			TestConnection.ExecuteReader(sql, r => result.Add((r.GetString(0), r.GetString(1))));

			return result;
		}

		void AssertForeignKeyExistInMainDb(bool expected, string fkName, string pkName, string deleteRule)
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM [{0}].INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
				WHERE CONSTRAINT_NAME = '{1}'",
				mockMainDb, fkName);

			if (expected)
			{
				sqlText += " AND UNIQUE_CONSTRAINT_NAME = '" + pkName + "'";

				if (deleteRule != null)
				{
					sqlText += " AND UPPER(DELETE_RULE) = '" + deleteRule + "'";
				}
			}

			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			string assertMessage = String.Format("Foreign Key: {0} exists?", fkName);
			AssertEquals(assertMessage, expected, qtyRows == 1);
		}

		public void TestEnsureCheckConstraintsAreEnabledAndTrusted()
		{
			RunActionOnMockMainDb(() =>
				ConstraintScriptRunnerTransactionalTest.TestEnsureCheckConstraintsAreEnabledAndTrusted(
					TestConnection,
					expectedIsTrusted: true));
		}

		protected override void SetUp()
		{
			base.SetUp();

			AssertCheckConstraintExists(mockTemplateDb, false, "MatchTable01", "CK_MatchTable01");
			AssertCheckConstraintExists(mockMainDb, true, "MatchTable01", "CK_MatchTable01");

			AssertCheckConstraintExists(mockTemplateDb, true, "MatchTable02", "CK_MatchTable02");
			AssertCheckConstraintExists(mockMainDb, false, "MatchTable02", "CK_MatchTable02");

			AssertCheckConstraintExists(mockMainDb, true, "UnmatchTable01", "CK_UnmatchTable01");
		}

		#region Test DB Creation Scripts

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, createTestMainDbObjectsScript);
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, createTestTemplateDbObjectsScript);
		}

		const string createTestMainDbObjectsScript = @"
			-- Matching Table 01
			--   Check constraint removed - CK_MatchTable01
			--   Default constraint removed (Col2)
			CREATE TABLE MatchTable01
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL DEFAULT ('Y'),
				Col3 VARCHAR(30) NULL,
				Col4 INT         NULL,
				Col5 INT         NULL
			)
			;
			ALTER TABLE MatchTable01
				ADD CONSTRAINT PK_MatchTable01 PRIMARY KEY NONCLUSTERED (Col1)
			;
			ALTER TABLE MatchTable01
				ADD CONSTRAINT UQ_MatchTable01 UNIQUE NONCLUSTERED (Col2)
			;
			ALTER TABLE MatchTable01
				ADD CONSTRAINT CK_MatchTable01 CHECK (Col3 != '')
			;

			CREATE UNIQUE NONCLUSTERED INDEX UX_MatchTable01_ForCompositeKeyTest ON MatchTable01 (Col4, Col5)
			;

			-- Matching Table 02
			--   Default constraint changed - default on Col2 to 'B'
			--   Check constraint added - CK_MatchTable02
			CREATE TABLE MatchTable02
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL DEFAULT ('A'),
				Col3 INT         NULL,
				Col4 INT         NULL,
				Col5 INT         NULL
			)
			;
			ALTER TABLE MatchTable02
				ADD CONSTRAINT PK_MatchTable02 PRIMARY KEY NONCLUSTERED (Col1)
			;

			-- Table with self-referencing multi-column foreign key
			CREATE TABLE TrickyForeignKey
			(
				Col1 int not null,
				Col2 int not null,
				Col3 char(3) not null,
				Col4 int null,
			)
			;

			CREATE UNIQUE CLUSTERED INDEX [FK_UX_Col2_Col3] ON TrickyForeignKey (Col3 ASC, Col2 ASC)
			;

			-- Unmatching Table 01 (old)
			CREATE TABLE UnmatchTable01
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL,
				Col3 VARCHAR(30) NULL DEFAULT ('')
			)
			;
			ALTER TABLE UnmatchTable01
				ADD CONSTRAINT PK_UnmatchTable01 PRIMARY KEY NONCLUSTERED (Col1)
			;
			ALTER TABLE UnmatchTable01
				ADD CONSTRAINT CK_UnmatchTable01 CHECK (Col2 != '')
			;

			-- Foreign Keys

			-- This FK will be modified to cascade on delete
			ALTER TABLE MatchTable02
				ADD CONSTRAINT FK_MatchTable02_TO_MatchTable01 FOREIGN KEY (Col4)
						REFERENCES MatchTable01 (Col1)
			;

			-- This FK will no longer exist in the new schema
			ALTER TABLE MatchTable01
				ADD CONSTRAINT FK_MatchTable01_TO_MatchTable01 FOREIGN KEY (Col4)
						REFERENCES MatchTable01 (Col1)
			;

			-- This FK will no longer exist in the new schema
			ALTER TABLE UnmatchTable01
				ADD CONSTRAINT FK_UnmatchTable01_TO_MatchTable01 FOREIGN KEY (Col1)
						REFERENCES MatchTable01 (Col1)

			-- This composite FK will no longer exist in the new schema
			ALTER TABLE MatchTable02
				ADD CONSTRAINT FK_MatchTable02_TO_MatchTable01_Composite FOREIGN KEY (Col4, Col5)
						REFERENCES MatchTable01 (Col4, Col5)
			;
			";

		const string createTestTemplateDbObjectsScript = @"
			-- Matching Table 01
			--   A check constraint removed - CK_MatchTable01
			--   A default constraint removed (Col2)
			CREATE TABLE MatchTable01
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL,
				Col3 VARCHAR(30) NULL,
				Col4 INT         NULL,
				Col5 INT         NULL
			)
			;
			ALTER TABLE MatchTable01
				ADD CONSTRAINT PK_MatchTable01 PRIMARY KEY NONCLUSTERED (Col1)
			;
			ALTER TABLE MatchTable01
				ADD CONSTRAINT UQ_MatchTable01 UNIQUE NONCLUSTERED (Col2)
			;

			CREATE UNIQUE NONCLUSTERED INDEX UX_MatchTable01_ForCompositeKeyTest ON MatchTable01 (Col4, Col5)
			;

			-- Matching Table 02
			--   Default constraint changed - (Col2 to 'B')
			--   Check constraint added - CK_MatchTable02
			CREATE TABLE MatchTable02
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL DEFAULT ('B'),
				Col3 INT         NULL,
				Col4 INT         NULL,
				Col5 INT         NULL
			)
			;
			ALTER TABLE MatchTable02
				ADD CONSTRAINT PK_MatchTable02 PRIMARY KEY NONCLUSTERED (Col1)
			;
			ALTER TABLE MatchTable02
				ADD CONSTRAINT CK_MatchTable02 CHECK (Col2 != '')
			;
			ALTER TABLE MatchTable02
				ADD CONSTRAINT CK_MatchTable03 CHECK (Col2 != '')
			;
			ALTER TABLE MatchTable02
				ADD CONSTRAINT CK_MatchTable02_NoCheck CHECK (Col2 != '')
			;

			-- Unmatching Table 02 (new)
			CREATE TABLE UnmatchTable02
			( 
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL,
				Col3 VARCHAR(30) NULL
			)
			;

			-- Table with self-referencing multi-column foreign key
			CREATE TABLE TrickyForeignKey
			(
				Col1 int not null,
				Col2 int not null,
				Col3 char(3) not null,
				Col4 int null,
			)
			;

			CREATE UNIQUE CLUSTERED INDEX [FK_UX_Col2_Col3] ON TrickyForeignKey (Col3 ASC, Col2 ASC)
			;

			-- Foreign Keys

			-- Modified to cascade on delete
			ALTER TABLE MatchTable02
				ADD CONSTRAINT FK_MatchTable02_TO_MatchTable01 FOREIGN KEY (Col4)
						REFERENCES MatchTable01 (Col1) ON DELETE CASCADE
			;

			-- New FK
			ALTER TABLE MatchTable02
				ADD CONSTRAINT FK_MatchTable02_TO_MatchTable02 FOREIGN KEY (Col3)
						REFERENCES MatchTable02 (Col1)
			;

			-- New self referencing composite FK (to be ADDED)
			ALTER TABLE TrickyForeignKey
			ADD CONSTRAINT TrickyForeignKey_FK2_TrickyForeignKey_RRR_120N FOREIGN KEY
				( Col3, Col4 )
				REFERENCES TrickyForeignKey
				( Col3, Col2 )
			;
			";

		#endregion
	}
}
