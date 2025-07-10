using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.StabilityChecker;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DbHealth.Check.Checkers
{
	sealed class DbVersionStabilityCheckerTest : TestCase
	{
		[DatCapabilityRequirementLatestAvailableSqlServer]
		public void TestLatestAvailableVersion()
		{
			CombineAssertions(() =>
			{
				Test(true);
				Test(false);
			});

			void Test(bool isHosted)
			{
				// Arrange
				SetIsHostedWithCargowise(isHosted);
				var checker = new DbVersionStabilityChecker();

				// Act
				var results = checker.Check();

				// Assert
				AssertContainsExactElementsInAnyOrder(Array.Empty<StabilityResult>(), results);
			}
		}

		public void TestRecommendedVersionProvidesNoMessages()
		{
			CombineAssertions(() =>
			{
				var product = SqlServerVersionNumber.SupportedVersions.Max();
				Test(product.ToString(), true);
				Test(product.ToString(), false);
			});

			void Test(string version, bool isHosted)
			{
				// Arrange
				SetIsHostedWithCargowise(isHosted);
				using (Db.Connection.SetSqlServerVersionForTest(version))
				{
					var checker = new DbVersionStabilityChecker();

					// Act
					var results = checker.Check();

					// Assert
					AssertContainsExactElementsInAnyOrder(Array.Empty<StabilityResult>(), results);
				}
			}
		}

		public void TestNotYetTestedProductWorksAsValidVersion()
		{
			CombineAssertions(() =>
			{
				Test(true);
				Test(false);
			});

			void Test(bool isHosted)
			{
				// Arrange
				SetIsHostedWithCargowise(isHosted);

				using (Db.Connection.SetSqlServerVersionForTest("99.00.1000.00"))
				{
					var checker = new DbVersionStabilityChecker();

					// Act
					var results = checker.Check();

					// Assert
					AssertContainsExactElementsInAnyOrder(Array.Empty<StabilityResult>(), results);
				}
			}
		}

		public void TestWarningSuppressedOnCw1HostedSystems()
		{
			// Arrange
			var version = SqlServerVersionNumber.SupportedVersions.Min();

			SetIsHostedWithCargowise(true);

			using (Db.Connection.SetSqlServerVersionForTest(new Version(version.Major - 1, 0, 0, 0).ToString()))
			{
				var checker = new DbVersionStabilityChecker();

				// Act
				var results = checker.Check();

				// Assert
				AssertContainsExactElementsInAnyOrder(Array.Empty<StabilityResult>(), results);
			}
		}

		public void TestServicePackAboveMinimumRequiredVersionsProvidesNoMessages()
		{
			CombineAssertions(() =>
			{
				Test(false, false);
				Test(false, true);
				Test(true, false);
				Test(true, true);
			});

			void Test(bool isHosted, bool isGlobal)
			{
				// Arrange
				SetIsHostedWithCargowise(isHosted);
				SetIsWiseTechGlobalDatabaseServer(isGlobal, Db.Connection);

				var version = SqlServerVersionNumber.SupportedVersions.Max();

				using (Db.Connection.SetSqlServerVersionForTest($"{version.Major:d1}.{version.Minor:d2}.{version.Build + 1:d4}.00"))
				{
					var checker = new DbVersionStabilityChecker();

					// Act
					var results = checker.Check();

					// Assert
					AssertContainsExactElementsInAnyOrder(Array.Empty<StabilityResult>(), results);
				}
			}
		}

		public void TestSupportedMajorVersionLowerThanLatestSupportedVersionProvidesWarning()
		{
			var max = SqlServerVersionNumber.SupportedVersions.Max();
			CombineAssertions(() =>
			{
				foreach (var version in SqlServerVersionNumber.SupportedVersions)
				{
					if (version == max)
					{
						continue;
					}

					Test(version.ToString());
				}
			});

			void Test(string version)
			{
				// Arrange
				SetIsHostedWithCargowise(false);
				using (Db.Connection.SetSqlServerVersionForTest(version))
				{
					var checker = new DbVersionStabilityChecker();

					// Act
					var results = checker.Check();

					// Assert
					AssertContainsExactElementsInAnyOrder(new[]
					{
						$"Warning - It is recommended to update your version of SQL Server to {max.GetLongDescription()} or higher.\r\n" +
						"WiseTech Global intends to increase the minimum required version of SQL Server, 12 months after the RTM.\r\n" +
						"To plan for this, upgrade your SQL Server version as soon as is practical.",
					}, results.Select(n => n.ToString()));
				}
			}
		}

		static void SetIsHostedWithCargowise(bool hosted)
		{
			EnvProxy.SetHostedLocationForTest(hosted ? "SYD" : string.Empty);
			AssertEquals(hosted, EnvProxy.IsHostedWithCargowise);
		}

		static void SetIsWiseTechGlobalDatabaseServer(bool isGlobal, DbConnection connectionTestedOn)
		{
			DataUtils.IsWiseTechGlobalDatabaseServerForTest = isGlobal;
			AssertEquals(isGlobal, DataUtils.IsWiseTechGlobalDatabaseServer(connectionTestedOn));
		}
	}
}
