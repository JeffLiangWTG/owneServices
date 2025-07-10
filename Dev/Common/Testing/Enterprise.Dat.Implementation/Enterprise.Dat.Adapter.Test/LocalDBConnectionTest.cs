using CargoWise.Data;
using Enterprise.Dat.Implementation;
using NUnit.Framework;

namespace Enterprise.Dat.Adapter.Testing
{
	abstract class LocalDBConnectionTest : TestCase
	{
		public void TestServerNameCanBeAssigned()
		{
			AssertNotNullOrEmpty(LocalDBConnection.GetServerName());
		}

		public void TestServerNameIsUsedInCw1()
		{
			AssertEquals(LocalDBConnection.GetServerName(), Db.ServerName);
		}

#if NETFRAMEWORK
		[TargetFrameworks(TargetFramework.NetFramework | TargetFramework.NetCore)]
#endif
		sealed class CurrentSqlServerServerNameTest : LocalDBConnectionTest
		{
		}

#if NETFRAMEWORK
		[TargetFrameworks(TargetFramework.NetFramework | TargetFramework.NetCore)]
#endif
		[DatCapabilityRequirementLatestAvailableSqlServer]
		sealed class LatestAvailableSqlServerServerNameTest : LocalDBConnectionTest
		{
		}
	}
}
