using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	sealed class DataTransformationHelperTest : TransactionedTestCase
	{
		public void TestSuspendFkIfExists()
		{
			TestConnection.ExecuteNonQuery("CREATE TABLE [~T1] (Col1 INT PRIMARY KEY, Col2 INT, CONSTRAINT [~FK_T1] FOREIGN KEY (Col2) REFERENCES [~T1] (Col1));");

			AssertEquals("[PRE-CONDITION] FK exists", true, DbObjectCreator.ForeignKeyExists(TestConnection, "~T1", "~FK_T1"));

			using (DataTransformationHelper.SuspendFkIfExists(Db.SqlDbOwnerSchema, "~T1", "~FK_T1"))
			{
				AssertEquals("Is FK enabled after being suspended?", false, IsFkEnabled(TestConnection, "~FK_T1"));
			}

			AssertEquals("FK is enabled", true, IsFkEnabled(TestConnection, "~FK_T1"));
		}

		[ExpectNoExceptions]
		public void TestSuspendFkIfExists_WhenFkDoesNotExist()
		{
			using (DataTransformationHelper.SuspendFkIfExists("IRRELEVANT", "IRRELEVANT", "NON-EXISTING"))
			{ }
		}

		public static bool IsFkEnabled(DbConnection connection, string fkName) => !connection.ExecuteScalar<bool>($"SELECT is_disabled FROM sys.foreign_keys WHERE name = '{fkName}'");

		#region TestSuspendConstraintIfExists

		public void TestSuspendConstraintIfExists()
		{
			const string suspendConstraintName = "~Constraint_T1";
			const string suspendTestTable = "~TC1";

			TestConnection.ExecuteNonQuery($@"
			CREATE TABLE [{suspendTestTable}] (Col1 INT PRIMARY KEY);
			ALTER TABLE [{suspendTestTable}] WITH NOCHECK
			ADD CONSTRAINT [{suspendConstraintName}] CHECK (Col1 IN (1, 2, 3))
			");

			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, Db.SqlDbOwnerSchema, suspendTestTable, suspendConstraintName))
			{
				AssertEquals("Is Constraint enabled after being suspended?", false, IsConstraintEnabled(TestConnection, suspendConstraintName));
			}

			AssertEquals("Constraint is enabled", true, IsConstraintEnabled(TestConnection, suspendConstraintName));
		}

		[ExpectNoExceptions]
		public void TestSuspendConstraintIfExists_WhenFkDoesNotExist()
		{
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, "IRRELEVANT", "IRRELEVANT", "NON-EXISTING"))
			{ }
		}

		public static bool IsConstraintEnabled(DbConnection connection, string constraintName) => !connection.ExecuteScalar<bool>($"SELECT is_disabled FROM sys.check_constraints WHERE name = '{constraintName}'");

		#endregion

		public void TestGetLeafTableShrinkingSQL_NumericColumn()
		{
			AssertGetLeafTableShrinkingSQL("DedicatedGuaranteeAmount", true);
		}

		public void TestGetLeafTableShrinkingSQL_StringColumn()
		{
			AssertGetLeafTableShrinkingSQL("ExportUnionSecretaryCode", false);
		}

		void AssertGetLeafTableShrinkingSQL(string columnName, bool isColumnNumeric)
		{
			var whereFilter = isColumnNumeric ? "> 0" : "<> ''";
			var expectedSQLString = $@"
UPDATE dbo.CusEntryInstruction
SET
	CEI_SystemLastEditTimeUtc = GETUTCDATE(),
	CEI_SystemLastEditUser = '~BP',
	CEI_AddInfo = CASE
		WHEN LEN(CEI_AddInfo)>0 THEN CONCAT(CEI_AddInfo,'*','{columnName}=',child_table.EUE_{columnName})
		ELSE CONCAT('{columnName}=',child_table.EUE_{columnName})
	END
FROM dbo.CusEntryInstruction
	INNER JOIN dbo.CusEuEntryInstruction AS child_table ON EUE_CEI=CEI_PK AND EUE_ClusterKey=CEI_ClusterKey
	CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInlineToReturnEmptyIfNull(CEI_AddInfo, '{columnName}') AS {columnName}
WHERE child_table.EUE_{columnName} {whereFilter}
AND {columnName}.Value = ''";

			var actualSQLString = DataTransformationHelper.GetLeafTableShrinkingSQL("dbo", "CusEuEntryInstruction", $"EUE_{columnName}", isColumnNumeric, "CusEntryInstruction", "CEI_AddInfo");

			AssertEquals(expectedSQLString, actualSQLString);
		}
	}
}
