using System;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class TableScriptRunnerLockEscalationTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		#region SynchroniseLockEscalationSettings

		public void TestSynchroniseLockEscalationSettings()
		{
			PrepareAndAssertLockEscalationTestData();

			var testSynchroniser = new TableScriptRunner(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.SynchroniseLockEscalationSettings);

			// Old table not changed as it doesn't exist on template database
			AssertLockEscalation(mockMainDb, "Old", 1);
			// Main database settings changed based on the template database
			AssertLockEscalation(mockMainDb, "Match_DefaultDiff", 2);
			AssertLockEscalation(mockMainDb, "Match_ColDiff", 0);
			AssertLockEscalation(mockMainDb, "Match_TableNameCaseDiff", 0);
			AssertLockEscalation(mockMainDb, "Match_ColCaseDiff", 1);
			AssertLockEscalation(mockMainDb, "Match_PKDiff", 1);
			AssertLockEscalation(mockMainDb, "Match_IndexDiff", 2);
			// No changes on template database
			AssertLockEscalation(mockTemplateDb, "Old", -1);
			AssertLockEscalation(mockTemplateDb, "Match_DefaultDiff", 2);
			AssertLockEscalation(mockTemplateDb, "Match_ColDiff", 0);
			AssertLockEscalation(mockTemplateDb, "Match_TableNameCaseDiff", 0);
			AssertLockEscalation(mockTemplateDb, "Match_ColCaseDiff", 1);
			AssertLockEscalation(mockTemplateDb, "Match_PKDiff", 1);
			AssertLockEscalation(mockTemplateDb, "Match_IndexDiff", 2);
		}

		void PrepareAndAssertLockEscalationTestData()
		{
			PrepareLockEscalationTestData();

			// Assert test data
			AssertLockEscalation(mockMainDb, "Old", 1);
			AssertLockEscalation(mockMainDb, "Match_DefaultDiff", 1);
			AssertLockEscalation(mockMainDb, "Match_ColDiff", 1);
			AssertLockEscalation(mockMainDb, "Match_TableNameCaseDiff", 2);
			AssertLockEscalation(mockMainDb, "Match_ColCaseDiff", 2);
			AssertLockEscalation(mockMainDb, "Match_PKDiff", 0);
			AssertLockEscalation(mockMainDb, "Match_IndexDiff", 0);
			AssertLockEscalation(mockTemplateDb, "Old", -1);
			AssertLockEscalation(mockTemplateDb, "Match_DefaultDiff", 2);
			AssertLockEscalation(mockTemplateDb, "Match_ColDiff", 0);
			AssertLockEscalation(mockTemplateDb, "Match_TableNameCaseDiff", 0);
			AssertLockEscalation(mockTemplateDb, "Match_ColCaseDiff", 1);
			AssertLockEscalation(mockTemplateDb, "Match_PKDiff", 1);
			AssertLockEscalation(mockTemplateDb, "Match_IndexDiff", 2);
		}

		void PrepareLockEscalationTestData()
		{
			string sqlText = String.Format(@"
				ALTER TABLE [{0}]..[Old] SET (LOCK_ESCALATION = DISABLE);
				ALTER TABLE [{0}]..[Match_DefaultDiff] SET (LOCK_ESCALATION = DISABLE);
				ALTER TABLE [{0}]..[Match_ColDiff] SET (LOCK_ESCALATION = DISABLE);
				ALTER TABLE [{0}]..[Match_TableNameCaseDiff] SET (LOCK_ESCALATION = AUTO);
				ALTER TABLE [{0}]..[Match_ColCaseDiff] SET (LOCK_ESCALATION = AUTO);
				ALTER TABLE [{1}]..[Match_DefaultDiff] SET (LOCK_ESCALATION = AUTO);
				ALTER TABLE [{1}]..[Match_PKDiff] SET (LOCK_ESCALATION = DISABLE);
				ALTER TABLE [{1}]..[Match_IndexDiff] SET (LOCK_ESCALATION = AUTO);
				ALTER TABLE [{1}]..[Match_ColCaseDiff] SET (LOCK_ESCALATION = DISABLE);",
				mockMainDb, mockTemplateDb);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		/// <summary>
		///	0 = TABLE
		///	1 = DISABLE
		///	2 = AUTO
		/// </summary>
		void AssertLockEscalation(string dbName, string tableName, int expectedValue)
		{
			string sqlText = String.Format("SELECT lock_escalation FROM [{0}].sys.tables WHERE name = '{1}'", dbName, tableName);
			object actualValueObj = TestConnection.ExecuteScalar(sqlText);
			int actualValue = (actualValueObj == null) ? -1 : Convert.ToInt32(actualValueObj);
			AssertEquals(String.Format("Table [{0}]..[{1}] lock escalation", dbName, tableName), expectedValue, actualValue);
		}

		#endregion
	}
}
