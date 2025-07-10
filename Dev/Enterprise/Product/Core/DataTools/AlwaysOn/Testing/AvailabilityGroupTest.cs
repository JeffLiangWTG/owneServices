using System;
using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	class AvailabilityGroupTest : TestCase
	{
		public void TestAttributes()
		{
			Guid groupId = Guid.NewGuid();
			var serverInfo = new SqlServerInfo("SomeServer");
			var alwaysOnDb = AlwaysOnDatabaseFactory.New("SomeDb", "SomeGroup", groupId);
			IAvailabilityGroup hadrGroup = new AvailabilityGroup(serverInfo, alwaysOnDb);

			AssertEquals("GroupName", "SomeGroup", hadrGroup.GroupName);
			AssertEquals("GroupId", groupId, hadrGroup.GroupId);
			AssertNotNull("Databases", hadrGroup.Databases);
			AssertNotNull("Replicas", hadrGroup.Replicas);
			AssertNull("PrimaryReplica", hadrGroup.PrimaryReplica);

			AssertEquals("IsLoaded?", false, hadrGroup.IsLoaded);
			AssertEquals("HasErrors?", false, hadrGroup.HasErrors);
			AssertEquals("LastErrorMessage", null, hadrGroup.LastErrorMessage);
		}
	}
}
