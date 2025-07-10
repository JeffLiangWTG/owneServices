using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class FactoryChangeSetTestCase : TestCaseWithFactory
	{
		public void TestGetChanges()
		{
			var dummyWithoutChanges = Factory.New<DummyEnterpriseBusinessObject>();
			var dummyWithChanges = Factory.New<DummyEnterpriseBusinessObject>();
			StmALog logWithChanges = dummyWithChanges.GetLogs().AddNew(Events.Authorised);

			dummyWithChanges.Z0_VarCharMax = "original";
			using (logWithChanges.LockForUpdatingKeyFieldsForTesting())
			{
				logWithChanges.SL_Reference = "original";
			}
			Factory.Save();
			dummyWithChanges.Z0_VarCharMax = "modified";
			using (logWithChanges.LockForUpdatingKeyFieldsForTesting())
			{
				logWithChanges.SL_Reference = "modified";
			}

			Db.Connection.ExecuteNonQuery("update dbo.DummyBizo set Z0_VarCharMax = 'db' where Z0_PK='" + dummyWithChanges.PK + "'");
			Db.Connection.ExecuteNonQuery("update dbo.StmALog set SL_Reference = 'db' where SL_PK='" + logWithChanges.PK + "'");

			IFactoryChangeSet changeSet = Factory.GetChanges();
			IObjectChangeSet[] objectChanges = changeSet.GetChangedObjects();
			AssertEquals(2, objectChanges.Length);
			IObjectChangeSet dummyChanges = objectChanges[0];
			IObjectChangeSet logChanges = objectChanges[1];

			AssertEquals("SessionInstance.GetType()", typeof(DummyEnterpriseBusinessObject), dummyChanges.SessionInstance.GetType());
			AssertEquals("SessionInstance", "modified", dummyChanges.SessionInstance["Z0_VarCharMax"]);
			AssertEquals("CanMerge", true, dummyChanges.CanMerge());
			AssertEquals("DatabaseInstance", "db", dummyChanges.DatabaseInstance["Z0_VarCharMax"]);
			AssertEquals("IsExistsInDatabase", true, dummyChanges.IsExistsInDatabase);
			AssertEquals("IsModifiedInDatabase", true, dummyChanges.IsModifiedInDatabase);
			AssertEquals("MergeableProperties", 1, dummyChanges.MergeableProperties.Count);
			AssertEquals("NonMergeableProperties", 0, dummyChanges.NonMergeableProperties.Count);

			AssertEquals("SessionInstance.GetType()", typeof(StmALog), logChanges.SessionInstance.GetType());
			AssertEquals("SessionInstance", "modified", logChanges.SessionInstance["SL_Reference"]);
			AssertEquals("CanMerge", true, logChanges.CanMerge());
			AssertEquals("DatabaseInstance", "db", logChanges.DatabaseInstance["SL_Reference"]);

			Regex regex = new Regex(".*SL_Parent = .* and SL_SE_NKEvent = .* and SL_EventTime =.*");
			AssertEquals("SL_Parent, SL_SE_NKEvent and SL_EventTime must be used to utilize the StmALog index", true, regex.IsMatch(SqlEventTracker.Instance.SqlEventDescription));
		}
	}
}
