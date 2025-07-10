using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	sealed class ReferenceDbUpgraderStaticMemberTest : TransactionedTestCase
	{
		public void TestHasSharedDatabases()
		{
			AssertEquals("[PRE-CONDITION] Does system use shared reference databases?", false, ReferenceDbUpgrader.DoesDbHaveSynonymsToSharedDatabases(TestConnection));
			string sqlText = "CREATE SYNONYM [RefDbXxxYy] FOR [CW-RefDb-Xxx-Yy-000000]..[SomeTable]";
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("Does system use shared reference databases?", true, ReferenceDbUpgrader.DoesDbHaveSynonymsToSharedDatabases(TestConnection));
		}

		public void TestHasSharedAvailabilityGroupDatabases()
		{
			AssertEquals("[PRE-CONDITION] Does system use shared reference databases?", false, ReferenceDbUpgrader.DoesDbHaveSynonymsToSharedDatabases(TestConnection));
			TestConnection.ExecuteNonQuery("CREATE SYNONYM [RefDbXxxYy] FOR [CW-AG-RefDb-ORDWP4-CP1AS1-Xxx-Yy-000000]..[SomeTable]");
			AssertEquals("Does system use shared reference databases?", true, ReferenceDbUpgrader.DoesDbHaveSynonymsToSharedDatabases(TestConnection));
		}
	}
}
