using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RegistryItemLogs))]
	sealed class RegistryItemLogsTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestLogs()
		{
			var parent = Factory.NewWithValidTestData<StmData>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			parent.GetLogs().AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			AssertEquals(1, ((IStmALogParent)new RegistryItemLogs(parent.PK, Factory)).Logs.GetAllLogs().Count);
			AssertEquals(0, ((IStmALogParent)new RegistryItemLogs(ZGuid.NewZGuid(), Factory)).Logs.GetAllLogs().Count);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RegistryItemLogs(ZGuid.NewZGuid(), Factory);
		}

		#endregion
	}
}
