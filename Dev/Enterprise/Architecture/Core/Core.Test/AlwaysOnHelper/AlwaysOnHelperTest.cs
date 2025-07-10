using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.AlwaysOnHelper.Testing
{
	sealed class AlwaysOnHelperTest : TransactionedTestCase
	{
		public void TestGetAlwaysOnSecondaryReplicaNames()
		{
			CombineAssertions(() =>
			{
				var primaryReplicaServerNameFromDB = GetPrimaryReplicaServerName(isNamedInstance: false);
				AssertGetAlwaysOnSecondaryReplicaNamesCore((databaseName, useCache) => AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(databaseName, useCache), primaryReplicaServerNameFromDB);

				var primaryReplicaServerNameWillBeEscaped = GetPrimaryReplicaServerName(isNamedInstance: true);
				AssertGetAlwaysOnSecondaryReplicaNamesCore((databaseName, useCache) => AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(databaseName, useCache), primaryReplicaServerNameWillBeEscaped);
			});
		}

		public void TestGetAlwaysOnSecondaryReplicaNamesWithAdminConnection()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				CombineAssertions(() =>
				{
					var primaryReplicaServerNameFromDB = GetPrimaryReplicaServerName(isNamedInstance: false);
					AssertGetAlwaysOnSecondaryReplicaNamesCore((databaseName, useCache) => AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(adminConnection, databaseName, useCache), primaryReplicaServerNameFromDB);

					var primaryReplicaServerNameWillBeEscaped = GetPrimaryReplicaServerName(isNamedInstance: true);
					AssertGetAlwaysOnSecondaryReplicaNamesCore((databaseName, useCache) => AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(adminConnection, databaseName, useCache), primaryReplicaServerNameWillBeEscaped);
				});
			}
		}

		public void TestGetAlwaysOnSecondaryReplicaNamesFromNotInListInstance()
		{
			CombineAssertions(() =>
			{
				var primaryReplicaServerNameFromDB = GetPrimaryReplicaServerName(isNamedInstance: false);
				AssertGetAlwaysOnSecondaryReplicaNamesFromNotInListInstanceCore(primaryReplicaServerNameFromDB);

				var primaryReplicaServerNameWillBeEscaped = GetPrimaryReplicaServerName(isNamedInstance: true);
				AssertGetAlwaysOnSecondaryReplicaNamesFromNotInListInstanceCore(primaryReplicaServerNameWillBeEscaped);
			});
		}

		void AssertGetAlwaysOnSecondaryReplicaNamesFromNotInListInstanceCore(string primaryReplicaServerName)
		{
			// Arrange
			var registry = EnvProxy.Instance.Registry;
			registry.UseAlwaysOnReplicaCache = true;
			var allReplicaInfosCacheForTest = new[]
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica11", AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica12", AvailabilityMode = 1 },
			};
			AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = primaryReplicaServerName, AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica11", AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica12", AvailabilityMode = 1 },
			};
			registry.AlwaysOnReplicaCachedInfos = allReplicaInfosCacheForTest;

			// Act
			var secondaryReplicaNames = AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(Db.DatabaseName, true);

			// Assert
			Assert(!secondaryReplicaNames.Contains(primaryReplicaServerName));
			Assert(registry.AlwaysOnReplicaCachedInfos
				.Any(info => info.ReplicaServerName.Equals(primaryReplicaServerName, StringComparison.OrdinalIgnoreCase)));
		}

		void AssertGetAlwaysOnSecondaryReplicaNamesCore(Func<string, bool, string[]> getAlwaysOnSecondaryReplicaNames, string primaryReplicaServerName)
		{
			// Arrange
			var registry = EnvProxy.Instance.Registry;
			registry.AlwaysOnReplicaCachedInfos = Array.Empty<AlwaysOnReplicaInfo>();
			var allReplicaInfosForTest1 = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica1", AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = primaryReplicaServerName, AvailabilityMode = 1 },
			};
			var allReplicaInfosForTest2 = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica2", AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = primaryReplicaServerName, AvailabilityMode = 1 },
			};
			var allReplicaInfosForTest3 = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica3", AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = primaryReplicaServerName, AvailabilityMode = 1 },
			};
			var allReplicaInfosForTest3Reverse = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = primaryReplicaServerName, AvailabilityMode = 1 },
				new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica3", AvailabilityMode = 1 },
			};

			// Arrange
			registry.UseAlwaysOnReplicaCache = false;
			AlwaysOn.ReplicaNames_ForTest.Value = allReplicaInfosForTest1;

			// Act
			var secondaryReplicaNames = getAlwaysOnSecondaryReplicaNames(Db.DatabaseName, true);

			// Assert cache not enabled
			AssertArrayEqualsByElements(new string[] { "testReplica1" }, secondaryReplicaNames);
			AssertArrayEqualsByElements(Array.Empty<AlwaysOnReplicaInfo>(), registry.AlwaysOnReplicaCachedInfos);

			// Arrange
			registry.UseAlwaysOnReplicaCache = true;
			AlwaysOn.ReplicaNames_ForTest.Value = allReplicaInfosForTest2;

			// Act
			secondaryReplicaNames = getAlwaysOnSecondaryReplicaNames(Db.DatabaseName, true);

			// Assert cache first time enabled
			AssertArrayEqualsByElements(new string[] { "testReplica2" }, secondaryReplicaNames);
			AssertArrayEqualsByElements(allReplicaInfosForTest2.ToArray(), registry.AlwaysOnReplicaCachedInfos);

			// Arrange
			AlwaysOn.ReplicaNames_ForTest.Value = allReplicaInfosForTest3;

			// Act
			secondaryReplicaNames = getAlwaysOnSecondaryReplicaNames(Db.DatabaseName, true);

			// Assert cache enabled and used
			AssertArrayEqualsByElements(new string[] { "testReplica2" }, secondaryReplicaNames);
			AssertArrayEqualsByElements(allReplicaInfosForTest2.ToArray(), registry.AlwaysOnReplicaCachedInfos);

			// Act
			secondaryReplicaNames = getAlwaysOnSecondaryReplicaNames(Db.DatabaseName, false);

			// Assert cache enabled but not used (will be refreshed)
			AssertArrayEqualsByElements(new string[] { "testReplica3" }, secondaryReplicaNames);
			AssertArrayEqualsByElements(allReplicaInfosForTest3.ToArray(), registry.AlwaysOnReplicaCachedInfos);

			// Arrange
			AlwaysOn.ReplicaNames_ForTest.Value = allReplicaInfosForTest3Reverse;

			// Act
			secondaryReplicaNames = getAlwaysOnSecondaryReplicaNames(Db.DatabaseName, false);

			// Assert cache stays the same in order
			AssertArrayEqualsByElements(new string[] { "testReplica3" }, secondaryReplicaNames);
			AssertArrayEqualsByElements(allReplicaInfosForTest3.ToArray(), registry.AlwaysOnReplicaCachedInfos);
		}

		string GetPrimaryReplicaServerName(bool isNamedInstance = false)
		{
			if (!isNamedInstance)
			{
				return DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(TestConnection.ServerNameReportedByDatabase);
			}
			var primaryReplicaServerName = "ABC.sand.wtg.zone\\MSSQLSERVER2022";
			AlwaysOnHelper.OverridablePrimaryReplicaFullName.Value = _ => primaryReplicaServerName;
			return primaryReplicaServerName;
		}

		protected override void TearDown()
		{
			base.TearDown();
			AlwaysOnHelper.OverridablePrimaryReplicaFullName.ResetValue();
		}
	}
}
