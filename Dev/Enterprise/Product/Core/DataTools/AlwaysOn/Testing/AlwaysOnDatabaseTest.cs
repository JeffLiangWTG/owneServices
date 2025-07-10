using System;
using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Testing
{
	class AlwaysOnDatabaseTest : TestCase
	{
		public void TestAttributes()
		{
			Guid groupId = Guid.NewGuid();
			var alwaysOnDb = AlwaysOnDatabaseFactory.New("SomeDb", "SomeGroup", groupId);
			AssertEquals("Name", "SomeDb", alwaysOnDb.Name);
			AssertEquals("GroupName", "SomeGroup", alwaysOnDb.GroupName);
			AssertEquals("GroupId", groupId, alwaysOnDb.GroupId);
			AssertEquals("ToString", "SomeDb (group: SomeGroup)", alwaysOnDb.ToString());
		}

		public void TestSetGroupInfo()
		{
			var alwaysOnDb = AlwaysOnDatabaseFactory.New("SomeDb", null, Guid.Empty);
			AssertEquals("Name", "SomeDb", alwaysOnDb.Name);
			AssertNull("GroupName", alwaysOnDb.GroupName);
			AssertEquals("GroupId", Guid.Empty, alwaysOnDb.GroupId);

			Guid groupId = Guid.NewGuid();
			alwaysOnDb.SetGroupInfo("group", groupId);
			AssertEquals("GroupName", "group", alwaysOnDb.GroupName);
			AssertEquals("GroupId", groupId, alwaysOnDb.GroupId);

			AssertExceptionThrown(
				typeof(AlwaysOnException),
				Invariant($"Database [{alwaysOnDb.Name}] already has an availability group defined: ID [{alwaysOnDb.GroupName}], Name [{alwaysOnDb.GroupId.ToString()}]."),
				() => alwaysOnDb.SetGroupInfo("NewGroup", Guid.NewGuid()));
		}
	}
}
