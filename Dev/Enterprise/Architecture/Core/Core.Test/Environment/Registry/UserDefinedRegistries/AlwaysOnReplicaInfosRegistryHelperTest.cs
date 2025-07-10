using System;
using CargoWise.Data.SqlServer;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class AlwaysOnReplicaInfosRegistryHelperTest : TestCase
	{
		public void TestAlwaysOnReplicaInfosRegistryHelper()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected", () => AlwaysOnReplicaInfosRegistryHelper.Serialise(null));
			AssertExceptionThrown<ArgumentNullException>("Exception expected", () => AlwaysOnReplicaInfosRegistryHelper.Deserialise(null));

			var infos = new[]
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica1", AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica2", AvailabilityMode = 1 },
			};

			AssertArrayEqualsByElements(infos, AlwaysOnReplicaInfosRegistryHelper.Deserialise(AlwaysOnReplicaInfosRegistryHelper.Serialise(infos)));
		}
	}
}
