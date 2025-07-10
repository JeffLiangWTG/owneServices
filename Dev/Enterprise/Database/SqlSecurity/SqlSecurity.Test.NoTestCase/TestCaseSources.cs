using System.Collections.Generic;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.SqlSecurity.Test
{
	static class TestCaseSources
	{
		public static IEnumerable<TestCaseData> DatabaseTestModeDataBaseTypeCombinations(DatabaseType databaseType)
		{
			yield return new TestCaseData(DatabaseTestMode.HostedInWiseCloudDedicatedServer, databaseType)
				.SetName($"{{m}}: Wise cloud dedicated, {databaseType}");

			yield return new TestCaseData(DatabaseTestMode.HostedInWiseCloudSharedServer, databaseType)
				.SetName($"{{m}}: Wise cloud hosted, {databaseType}");

			yield return new TestCaseData(DatabaseTestMode.SelfHostedOpen, databaseType)
				.SetName($"{{m}}: Self hosted open, {databaseType}");

			yield return new TestCaseData(DatabaseTestMode.SelfHostedLocked, databaseType)
				.SetName($"{{m}}: Self hosted locked, {databaseType}");
		}
	}
}
