using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class SecondaryServerConnectionForModuleSearchDetailsProviderTest : TransactionedTestCase
	{
		public void TestIsSecondaryDbEnabled()
		{
			var replicaNamesForTest = new[] { "testReplica1", "testReplica2" };
			SystemDataRegistryForTest.Get().EnableModuleQueryFromSecondaryDbReplica = true;
			SystemDataRegistryForTest.Get().ModuleQueryDbServerNames = replicaNamesForTest;

			AssertEquals(true, SecondaryServerConnectionForModuleSearchDetailsProvider.IsSecondaryDbEnabled);
			SecondaryServerConnectionForModuleSearchDetailsProvider.ClearCache();
		}

		public void TestCurrrentSecondaryServerName()
		{
			var replicaNamesForTest = new[] { "testReplica1", "testReplica2" };
			SystemDataRegistryForTest.Get().ModuleQueryDbServerNames = replicaNamesForTest;

			Assert(replicaNamesForTest.Contains(SecondaryServerConnectionForModuleSearchDetailsProvider.CurrentSecondaryServerName));
			SecondaryServerConnectionForModuleSearchDetailsProvider.ClearCache();
		}

		public void TestIsSecondaryDbEnabled_UseAlwaysOn()
		{
			SystemDataRegistryForTest.Get().EnableModuleQueryFromSecondaryDbReplica = true;
			var replicaInfosForTest = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica1", AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica2", AvailabilityMode = 1 }
			};
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
			AlwaysOn.ReplicaNames_ForTest.Value = replicaInfosForTest;

			AssertEquals(true, SecondaryServerConnectionForModuleSearchDetailsProvider.IsSecondaryDbEnabled);
			SecondaryServerConnectionForModuleSearchDetailsProvider.ClearCache();
		}

		public void TestCurrrentSecondaryServerName_UseAlwaysOn()
		{
			var replicaInfosForTest = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica1", AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica2", AvailabilityMode = 1 }
			};
			AlwaysOn.ReplicaNames_ForTest.Value = replicaInfosForTest;

			Assert(replicaInfosForTest.Select(r => r.ReplicaServerName).Contains(SecondaryServerConnectionForModuleSearchDetailsProvider.CurrentSecondaryServerName));
			SecondaryServerConnectionForModuleSearchDetailsProvider.ClearCache();
		}

		public void TestAdd_Get_RemoveDbServer()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var replicaInfosForTest = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica1", AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica2", AvailabilityMode = 1 }
			};
				AlwaysOn.ReplicaNames_ForTest.Value = replicaInfosForTest;

				Assert(replicaInfosForTest.Select(r => r.ReplicaServerName).Contains(SecondaryServerConnectionForModuleSearchDetailsProvider.CurrentSecondaryServerName));

				SecondaryServerConnectionForModuleSearchDetailsProvider.AddDbServer("testReplica1", connection);
				SecondaryServerConnectionForModuleSearchDetailsProvider.AddDbServer("testReplica2", connection);

				AssertEquals(connection, SecondaryServerConnectionForModuleSearchDetailsProvider.GetDbServer("testReplica1"));
				AssertEquals(connection, SecondaryServerConnectionForModuleSearchDetailsProvider.GetDbServer("testReplica2"));

				SecondaryServerConnectionForModuleSearchDetailsProvider.RemoveDbServer("testReplica1");
				AssertEquals("testReplica2", SecondaryServerConnectionForModuleSearchDetailsProvider.CurrentSecondaryServerName);
				AssertEquals(null, SecondaryServerConnectionForModuleSearchDetailsProvider.GetDbServer("testReplica1"));

				SecondaryServerConnectionForModuleSearchDetailsProvider.RemoveDbServer("testReplica2");
				AssertEquals(string.Empty, SecondaryServerConnectionForModuleSearchDetailsProvider.CurrentSecondaryServerName);
				AssertEquals(null, SecondaryServerConnectionForModuleSearchDetailsProvider.GetDbServer("testReplica2"));
				SecondaryServerConnectionForModuleSearchDetailsProvider.ClearCache();
			}
		}
	}
}
