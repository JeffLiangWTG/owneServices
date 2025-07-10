using NUnit.Framework;

namespace CargoWise.Data.SqlServer.Testing
{
	public class AlwaysOnReplicaInfoTest : TestCase
	{
		public void TestAlwaysOnReplicaInfoEquals()
		{
			foreach (var tuple in new[]
					{
						(new AlwaysOnReplicaInfo() { ReplicaServerName = "test1", AvailabilityMode = 1 },new AlwaysOnReplicaInfo() { ReplicaServerName = "test1", AvailabilityMode = 1 },true),
						(new AlwaysOnReplicaInfo() { ReplicaServerName = "test1", AvailabilityMode = 1 },new AlwaysOnReplicaInfo() { ReplicaServerName = "test1", AvailabilityMode = 0 },false),
						(new AlwaysOnReplicaInfo() { ReplicaServerName = "test1", AvailabilityMode = 1 },new AlwaysOnReplicaInfo() { ReplicaServerName = "test2", AvailabilityMode = 1 },false),
						(new AlwaysOnReplicaInfo() { ReplicaServerName = "test1", AvailabilityMode = 1 },new AlwaysOnReplicaInfo() { ReplicaServerName = "TEST1", AvailabilityMode = 1 },true),
						(new AlwaysOnReplicaInfo() { ReplicaServerName = "test1", AvailabilityMode = 1 },new AlwaysOnReplicaInfo() { ReplicaServerName = "TEST1", AvailabilityMode = 0 },false),
					})
			{
				AssertEquals("AlwaysOnReplicaInfo Equals", tuple.Item3, tuple.Item1.Equals(tuple.Item2));
				AssertEquals("AlwaysOnReplicaInfo GetHashCode", tuple.Item3, tuple.Item1.GetHashCode() == tuple.Item2.GetHashCode());
				AssertEquals("AlwaysOnReplicaInfo ==", tuple.Item3, tuple.Item1 == tuple.Item2);
				AssertEquals("AlwaysOnReplicaInfo !=", !tuple.Item3, tuple.Item1 != tuple.Item2);
			}
		}
	}
}
