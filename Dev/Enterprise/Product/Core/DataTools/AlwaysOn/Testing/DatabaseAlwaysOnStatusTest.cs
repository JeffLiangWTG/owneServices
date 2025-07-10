using System;
using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	class DatabaseAlwaysOnStatusTest : TestCase
	{
		public void TestAttributes()
		{
			Guid testGroupId = Guid.NewGuid();
			var dbStatus = new DatabaseAlwaysOnStatus("SomeDatabase", testGroupId, true, false);
			AssertEquals("Name", "SomeDatabase", dbStatus.Name);
			AssertEquals("GroupId", testGroupId, dbStatus.GroupId);
			AssertEquals("HasFullBackup", true, dbStatus.HasFullBackup);
			AssertEquals("Data Movement Suspended", false, dbStatus.IsDataMovementSuspended);
		}
	}
}
