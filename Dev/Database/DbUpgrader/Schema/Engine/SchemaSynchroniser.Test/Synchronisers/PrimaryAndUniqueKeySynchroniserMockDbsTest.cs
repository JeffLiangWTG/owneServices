using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class PrimaryAndUniqueKeySynchroniserMockDbsTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		public void TestSynchronisePrimaryAndUniqueConstraints()
		{
			AssertPreConditions();
			SynchroniseMockMainDbPksAndUqs();
			AssertPksAndUqsSynchronised();
		}

		public void TestSynchronisePrimaryAndUniqueConstraints_WithConflictingClientConstraints()
		{
			string sqlText = String.Format(@"
				CREATE TABLE [{0}]..ClientTest (Col1 int not null);
				ALTER TABLE [{0}]..ClientTest ADD CONSTRAINT PK_DiffConstraintNameNew PRIMARY KEY NONCLUSTERED (Col1);",
				mockMainDb);
			TestConnection.ExecuteNonQuery(sqlText);

			try
			{
				SynchroniseMockMainDbPksAndUqs();
				Fail("Should throw an exception");
			}
			catch (Exception ex)
			{
				string expectedMsg =
					"\r\nThe following constraints on client-defined tables conflict with our database schema." +
					"\r\nThey must be removed/renamed before an upgrade can be applied." +
					"\r\n\r\n\tClientTest.PK_DiffConstraintNameNew (PK) - conflicts with DiffConstraintName";
				AssertEquals("Caught Exception", expectedMsg, ex.Message);
			}
		}

		void SynchroniseMockMainDbPksAndUqs()
		{
			PrimaryAndUniqueKeySynchroniserForTesting testSynchroniser = new PrimaryAndUniqueKeySynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.SynchronisePrimaryAndUniqueConstraints);
		}

		/// <summary>
		/// PRE-CONDITION Assertions (Asserts schema before synchronisation)
		/// 
		/// DiffConstraintName
		///   - PK_DiffConstraintName, UQ_DiffConstraintName
		/// DiffConstraintType
		///   - XK_DiffConstraintType UNIQUE constraint
		/// DiffConstraintIndexType
		///   - PK_DiffConstraintIndexType CLUSTERED
		/// DiffKeyColumnName
		///   - PK_DiffKeyColumnName (Col1)
		/// DiffKeyColumnList
		///   - UQ_DiffKeyColumnList (Col1,Col2,Col3)
		/// </summary>
		void AssertPreConditions()
		{
			AssertConstraintExist(true, "DiffConstraintName", "PK_DiffConstraintName", "PK", false);
			AssertConstraintExist(true, "DiffConstraintName", "UQ_DiffConstraintName", "UQ", false);

			AssertConstraintExist(true, "DiffConstraintType", "XK_DiffConstraintType", "UQ", false);

			AssertConstraintExist(true, "DiffConstraintIndexType", "PK_DiffConstraintIndexType", "PK", true);

			AssertConstraintExist(true, "DiffKeyColumnName", "PK_DiffKeyColumnName", "PK", false);
			AssertEquals("PK_DiffKeyColumnName column list", "Col1 ASC", GetConstraintKeyList("PK_DiffKeyColumnName"));

			AssertConstraintExist(true, "DiffKeyColumnList", "UQ_DiffKeyColumnList", "UQ", false);
			AssertEquals("UQ_DiffKeyColumnList column list", "Col1 ASC,Col2 ASC,Col3 ASC", GetConstraintKeyList("UQ_DiffKeyColumnList"));

			AssertConstraintExist(false, "MultiKeyPrimary", "PK_MultiKeyNew", "UQ", false);

			AssertConstraintExist(true, "PK_Clustered", "PK_UX__PC_PK", "PK", isClustered: false);
			AssertIndexExists(true, "PK_Clustered", "NR_RC__PC_Col1", isClustered: true);

			AssertConstraintExist(true, "UQ_Clustered", "UQ_UX__UC_PK", "UQ", isClustered: false);
			AssertIndexExists(true, "UQ_Clustered", "NR_RC__UC_Col1", isClustered: true);
		}

		/// <summary>
		/// DiffConstraintName
		///   - PK_DiffConstraintName => PK_DiffConstraintNameNew
		///   - UQ_DiffConstraintName => UQ_DiffConstraintNameNew
		/// DiffConstraintType
		///   - XK_DiffConstraintType PRIMARY KEY constraint
		/// DiffConstraintIndexType
		///   - PK_DiffConstraintIndexType: CLUSTERED => NONCLUSTERED
		/// DiffKeyColumnName
		///   - PK_DiffKeyColumnName: Col1 => Col2
		/// DiffKeyColumnList
		///   - UQ_DiffKeyColumnList: Col1+Col2+Col3 => Col2
		/// </summary>
		void AssertPksAndUqsSynchronised()
		{
			AssertConstraintExist(false, "DiffConstraintName", "PK_DiffConstraintName", null, null);
			AssertConstraintExist(true, "DiffConstraintName", "PK_DiffConstraintNameNew", "PK", true);
			AssertConstraintExist(false, "DiffConstraintName", "UQ_DiffConstraintName", null, null);
			AssertConstraintExist(true, "DiffConstraintName", "UQ_DiffConstraintNameNew", "UQ", false);

			AssertConstraintExist(true, "DiffConstraintType", "XK_DiffConstraintType", "PK", false);

			AssertConstraintExist(true, "DiffConstraintIndexType", "PK_DiffConstraintIndexType", "PK", false);

			AssertConstraintExist(true, "DiffKeyColumnName", "PK_DiffKeyColumnName", "PK", false);
			AssertEquals("PK_DiffKeyColumnName column list", "Col2 ASC", GetConstraintKeyList("PK_DiffKeyColumnName"));

			AssertConstraintExist(true, "DiffKeyColumnList", "UQ_DiffKeyColumnList", "UQ", false);
			AssertEquals("UQ_DiffKeyColumnList column list", "Col2 ASC", GetConstraintKeyList("UQ_DiffKeyColumnList"));

			AssertConstraintExist(false, "PK_Clustered", "PK_UX__PC_PK", "PK", isClustered: false);
			AssertConstraintExist(true, "PK_Clustered", "PK_UC__PC_PK", "PK", isClustered: true);
			AssertIndexExists(false, "PK_Clustered", "NR_RC__PC_Col1", isClustered: true);

			AssertConstraintExist(false, "UQ_Clustered", "UQ_UX__UC_PK", "UQ", isClustered: false);
			AssertConstraintExist(true, "UQ_Clustered", "UQ_UC__UC_PK", "UQ", isClustered: true);
			AssertIndexExists(false, "UQ_Clustered", "NR_RC__UC_Col1", isClustered: true);

			AssertConstraintExist(true, "MultiKeyPrimary", "PK_MultiKeyNew", "PK", false);
			AssertEquals("PK_MultiKeyNew column list", "Col1 DESC,Col2 ASC", GetConstraintKeyList("PK_MultiKeyNew"));
		}

		void AssertConstraintExist(bool expected, string tableName, string constraintName, string constraintType, bool? isClustered)
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*)
				FROM [{0}].sys.tables tab
				INNER JOIN [{0}].sys.key_constraints const ON const.parent_object_id = tab.object_id
				INNER JOIN [{0}].sys.indexes ind ON ind.object_id = tab.object_id AND ind.index_id = const.unique_index_id
				WHERE tab.name = '{1}'
				AND const.name = '{2}'",
				mockMainDb, tableName, constraintName);

			if (expected)
			{
				sqlText += String.Format(" AND const.type = '{0}'", constraintType);
				sqlText += String.Format(" AND ind.type = {0}", (isClustered.Value) ? "1" : "2");
			}

			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			string assertMessage = String.Format("Constraint: {0}.{1} exists?", tableName, constraintName);

			AssertEquals(assertMessage, expected, (qtyRows == 1));
		}

		void AssertIndexExists(bool expected, string tableName, string indexName, bool isClustered)
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
			{
				var index = IndexLoader.LoadTop1(TestConnection, Db.SqlDbOwnerSchema, tableName, indexName);
				if (index == null)
				{
					AssertEquals(expected, false);
				}
				else
				{
					AssertEquals(expected, index.IsClustered == isClustered);
				}
			}
		}

		string GetConstraintKeyList(string constraintName)
		{
			string sqlText = String.Format(@"
				DECLARE @ColList varchar(1000);
				SELECT @ColList = isnull(@ColList + ',', '') + col.name + ' ' + case when ikey.is_descending_key = 1 then 'DESC' else 'ASC' end
					FROM
						[{0}].sys.key_constraints const
						INNER JOIN [{0}].sys.index_columns ikey ON ikey.object_id = const.parent_object_id AND ikey.index_id = const.unique_index_id
						INNER JOIN [{0}].sys.columns col ON col.object_id = ikey.object_id AND col.column_id = ikey.column_id
					WHERE
						const.name = '{1}'
					ORDER BY ikey.key_ordinal;
				SELECT @ColList;",
				mockMainDb, constraintName);

			return TestConnection.ExecuteScalar(sqlText).ToString();
		}

		#region Mock Databases

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, enableDbChangeTrackingScript, createTestMainDbObjectsScript);
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, createTestTemplateDbObjectsScript);
		}

		#region Scripts

		const string enableDbChangeTrackingScript = "ALTER DATABASE CURRENT SET CHANGE_TRACKING = ON";

		const string createTestMainDbObjectsScript = @"
			-- addition of multi key primary key
			CREATE TABLE MultiKeyPrimary
			(
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL
			)

-- [DiffConstraintName] will have
			--   - Its PK name modified: PK_DiffConstraintName => PK_DiffConstraintNameNew
			--   - Its UQ name modified: UQ_DiffConstraintName => UQ_DiffConstraintNameNew
			CREATE TABLE DiffConstraintName
			( 
				Col1 INT         NOT NULL,
				Col2 INT         NULL,
			)
			;
			ALTER TABLE DiffConstraintName
				ADD CONSTRAINT PK_DiffConstraintName PRIMARY KEY NONCLUSTERED (Col1)
			;
			ALTER TABLE DiffConstraintName
				ADD CONSTRAINT UQ_DiffConstraintName UNIQUE NONCLUSTERED (Col2)
			;
			ALTER TABLE DiffConstraintName ENABLE CHANGE_TRACKING;

			-- [DiffConstraintType] will have
			--   - Its constraint type modified: UQ => PK
			CREATE TABLE DiffConstraintType
			( 
				Col1 INT         NOT NULL,
			)
			;
			ALTER TABLE DiffConstraintType
				ADD CONSTRAINT XK_DiffConstraintType UNIQUE NONCLUSTERED (Col1)
			;

			-- [DiffConstraintIndexType] will have
			--   - Its PK index type modified: CLUSTERED => NONCLUSTERED
			CREATE TABLE DiffConstraintIndexType
			( 
				Col1 INT         NOT NULL,
				Col2 XML         NULL,
			)
			;
			ALTER TABLE DiffConstraintIndexType
				ADD CONSTRAINT PK_DiffConstraintIndexType PRIMARY KEY CLUSTERED (Col1)
			;
			CREATE PRIMARY XML INDEX IX_DiffConstraintIndexType_Col2 ON DiffConstraintIndexType (Col2) 
			;
			ALTER TABLE DiffConstraintIndexType ENABLE CHANGE_TRACKING;

			-- [DiffKeyColumnName] will have
			--   - Its PK column modified: Col1 => Col2
			CREATE TABLE DiffKeyColumnName
			( 
				Col1 INT         NOT NULL,
				Col2 INT         NOT NULL,
			)
			;
			ALTER TABLE DiffKeyColumnName
				ADD CONSTRAINT PK_DiffKeyColumnName PRIMARY KEY NONCLUSTERED (Col1)
			;
			ALTER TABLE DiffKeyColumnName ENABLE CHANGE_TRACKING;

			-- [DiffKeyColumnList] will have
			--   - Its UQ modified to be single as opposed to composite key (Col1+Col2+Col3 => Col2)
			CREATE TABLE DiffKeyColumnList
			( 
				Col1 INT         NULL,
				Col2 INT         NULL,
				Col3 INT         NULL,
			)
			;
			ALTER TABLE DiffKeyColumnList
				ADD CONSTRAINT UQ_DiffKeyColumnList UNIQUE NONCLUSTERED (Col1, Col2, Col3)
			;

			-- Foreign Keys
			ALTER TABLE DiffKeyColumnName
				ADD CONSTRAINT FK_DiffKeyColumnName_TO_DiffConstraintName_01 FOREIGN KEY (Col2)
						REFERENCES DiffConstraintName (Col1)
			;

			-- PK from non-clustered to clustered with existing clustered index
			CREATE TABLE dbo.PK_Clustered
			(
				PC_PK   int NOT NULL,
				PC_Col1 int NOT NULL,

				CONSTRAINT PK_UX__PC_PK PRIMARY KEY NONCLUSTERED (PC_PK)
			)
			;
			CREATE CLUSTERED INDEX NR_RC__PC_Col1 ON dbo.PK_Clustered (PC_Col1)
			;

			-- UQ from non-clustered to clustered with existing clustered index
			CREATE TABLE dbo.UQ_Clustered
			(
				UC_PK   int NOT NULL,
				UC_Col1 int NOT NULL,

				CONSTRAINT UQ_UX__UC_PK UNIQUE NONCLUSTERED (UC_PK)
			)
			;
			CREATE CLUSTERED INDEX NR_RC__UC_Col1 ON dbo.UQ_Clustered (UC_Col1)
			;

			-- Insert Rows
			INSERT INTO DiffConstraintName       VALUES (1,null);
			INSERT INTO DiffConstraintType       VALUES (2);
			INSERT INTO DiffConstraintIndexType  VALUES (3,null);
			INSERT INTO DiffKeyColumnName        VALUES (4,1);
			INSERT INTO DiffKeyColumnList        VALUES (5,5,5);
			";

		const string createTestTemplateDbObjectsScript = @"
			CREATE TABLE MultiKeyPrimary
			(
				Col1 INT         NOT NULL,
				Col2 CHAR(1)     NOT NULL
			)
			;
			ALTER TABLE MultiKeyPrimary
				ADD CONSTRAINT PK_MultiKeyNew PRIMARY KEY NONCLUSTERED (Col1 DESC, Col2)
			;

			-- [DiffConstraintName] had
			--   - Its PK name modified: PK_DiffConstraintName => PK_DiffConstraintNameNew
			--   - Its UQ name modified: UQ_DiffConstraintName => UQ_DiffConstraintNameNew
			CREATE TABLE DiffConstraintName
			( 
				Col1 INT         NOT NULL,
				Col2 INT         NULL,
			)
			;
			ALTER TABLE DiffConstraintName
				ADD CONSTRAINT PK_DiffConstraintNameNew PRIMARY KEY CLUSTERED (Col1)
			;
			ALTER TABLE DiffConstraintName
				ADD CONSTRAINT UQ_DiffConstraintNameNew UNIQUE NONCLUSTERED (Col2)
			;

			-- [DiffConstraintType] had
			--   - Its constraint type modified: UQ => PK
			CREATE TABLE DiffConstraintType
			( 
				Col1 INT         NOT NULL,
			)
			;
			ALTER TABLE DiffConstraintType
				ADD CONSTRAINT XK_DiffConstraintType PRIMARY KEY NONCLUSTERED (Col1)
			;

			-- [DiffConstraintIndexType] had
			--   - Its PK type modified: CLUSTERED => NONCLUSTERED
			CREATE TABLE DiffConstraintIndexType
			( 
				Col1 INT         NOT NULL,
			)
			;
			ALTER TABLE DiffConstraintIndexType
				ADD CONSTRAINT PK_DiffConstraintIndexType PRIMARY KEY NONCLUSTERED (Col1)
			;

			-- [DiffKeyColumnName] had
			--   - Its PK column modified: Col1 => Col2
			CREATE TABLE DiffKeyColumnName
			( 
				Col1 INT         NOT NULL,
				Col2 INT         NOT NULL,
			)
			;
			ALTER TABLE DiffKeyColumnName
				ADD CONSTRAINT PK_DiffKeyColumnName PRIMARY KEY NONCLUSTERED (Col2)
			;

			-- [DiffKeyColumnList] had
			--   - Its UQ modified to be single as opposed to composite key (Col1+Col2+Col3 => Col2)
			CREATE TABLE DiffKeyColumnList
			( 
				Col1 INT         NULL,
				Col2 INT         NULL,
				Col3 INT         NULL,
			)
			;
			ALTER TABLE DiffKeyColumnList
				ADD CONSTRAINT UQ_DiffKeyColumnList UNIQUE NONCLUSTERED (Col2)
			;

			-- Foreign Keys
			ALTER TABLE DiffKeyColumnName
				ADD CONSTRAINT FK_DiffKeyColumnName_TO_DiffConstraintName_01 FOREIGN KEY (Col2)
						REFERENCES DiffConstraintName (Col1)
			;

			-- PK from non-clustered to clustered with existing clustered index
			CREATE TABLE dbo.PK_Clustered
			(
				PC_PK   int NOT NULL,
				PC_Col1 int NOT NULL,

				CONSTRAINT PK_UC__PC_PK PRIMARY KEY CLUSTERED (PC_PK)
			)
			;

			-- UQ from non-clustered to clustered with existing clustered index
			CREATE TABLE dbo.UQ_Clustered
			(
				UC_PK   int NOT NULL,
				UC_Col1 int NOT NULL,

				CONSTRAINT UQ_UC__UC_PK UNIQUE CLUSTERED (UC_PK)
			)
			;

			";

		#endregion

		#endregion

		class PrimaryAndUniqueKeySynchroniserForTesting : PrimaryAndUniqueKeySynchroniser
		{
			public PrimaryAndUniqueKeySynchroniserForTesting(DbConnection upgConnection, string dbBeingUpgraded, string templateDb)
				: base(upgConnection, dbBeingUpgraded, templateDb, new TableScriptRunner(upgConnection, dbBeingUpgraded, templateDb), new DummyUpgradeManager())
			{
			}
		}
	}
}
