using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class ServerVersionNumberTest : TestCase
	{
		public void TestMinimumRequiredVersions()
		{
			var versionNumbers = SqlServerVersionNumber.SupportedVersions
				.Select(x => new SqlServerVersionNumber(x.ToString()));

			CombineAssertions(() =>
			{
				foreach (var versionNumber in versionNumbers)
				{
					Test(versionNumber);
				}
			});

			void Test(SqlServerVersionNumber versionNumber)
			{
				AssertEquals(false, versionNumber.IsAboveMaximumSupportedSqlServerGeneration);
				AssertMatch(
					$@"{nameof(versionNumber.SqlServerGeneration)} should match expectation
This test could fail when:
1. SQL Server generation changes its rule, e.g. SQL2008R2
2. New SQL Server is released but its version hasn't been maintained in code base",
					new Regex(@"^SQL\d{4}$"),
					versionNumber.SqlServerGeneration);
				AssertMatch(
					$@"{nameof(versionNumber.FormalSqlServerGeneration)} should match expectation
This test could fail when:
1. SQL Server generation changes its rule, e.g. Microsoft SQL Server 2008R2
2. New SQL Server is released but its version hasn't been maintained in code base",
					new Regex(@"^Microsoft SQL Server \d{4}$"),
					versionNumber.FormalSqlServerGeneration);
				AssertEquals(true, versionNumber.IsMinimumRequiredVersionOrAbove);
			}
		}

		public void TestObsoletedSqlServerGenerations()
		{
			var obsoleteVersionNumbers = ObsoletedGenerations
				.Select(x => new SqlServerVersionNumber(x.AsVersion().ToString()));

			CombineAssertions(() =>
			{
				foreach (var versionNumber in obsoleteVersionNumbers)
				{
					Test(versionNumber);
				}
			});

			void Test(SqlServerVersionNumber versionNumber)
			{
				AssertEquals(false, versionNumber.IsAboveMaximumSupportedSqlServerGeneration);
				AssertEquals(false, versionNumber.IsMinimumRequiredVersionOrAbove);
			}
		}

		public void TestIsMinimumRequiredVersionOrAbove()
		{
			AssertEquals("[14.0.3000.0] IsMinimumRequiredVersionOrAbove?", false, new SqlServerVersionNumber("14.0.3000.0").IsMinimumRequiredVersionOrAbove);
			AssertEquals("[14.0.3030.0] IsMinimumRequiredVersionOrAbove?", false, new SqlServerVersionNumber("14.0.3030.0").IsMinimumRequiredVersionOrAbove);
			AssertEquals("[14.7.3030.0] IsMinimumRequiredVersionOrAbove?", false, new SqlServerVersionNumber("14.7.3030.0").IsMinimumRequiredVersionOrAbove);

			AssertEquals("[15.0.1199.0] IsMinimumRequiredVersionOrAbove?", false, new SqlServerVersionNumber("15.0.1199.0").IsMinimumRequiredVersionOrAbove);
			AssertEquals("[16.0.999.0] IsMinimumRequiredVersionOrAbove?", false, new SqlServerVersionNumber("16.0.999.0").IsMinimumRequiredVersionOrAbove);

			AssertEquals("[15.0.1200.0] IsMinimumRequiredVersionOrAbove?", true, new SqlServerVersionNumber("15.0.1200.0").IsMinimumRequiredVersionOrAbove);
			AssertEquals("[16.0.1000.0] IsMinimumRequiredVersionOrAbove?", true, new SqlServerVersionNumber("16.0.1000.0").IsMinimumRequiredVersionOrAbove);

			AssertEquals("[15.1.0.0] IsMinimumRequiredVersionOrAbove?", false, new SqlServerVersionNumber("15.1.0.0").IsMinimumRequiredVersionOrAbove);
			AssertEquals("[16.1.0.0] IsMinimumRequiredVersionOrAbove?", true, new SqlServerVersionNumber("16.1.0.0").IsMinimumRequiredVersionOrAbove);
			AssertEquals("[99.0.0.0] IsMinimumRequiredVersionOrAbove?", true, new SqlServerVersionNumber("99.0.0.0").IsMinimumRequiredVersionOrAbove);
		}

		public void TestSqlServerGeneration()
		{
			// Arrange
			var sqlServerGenerations = new[]
			{
				("SQL2016", new[]
					{
						new SqlServerVersionNumber("13.00.0000.0"),
						new SqlServerVersionNumber("13.00.9999.0")
					}
				),
				("SQL2017" ,new[]
					{
						new SqlServerVersionNumber("14.00.0000.0"),
						new SqlServerVersionNumber("14.00.9999.0")
					}
				),
				("SQL2019" ,new[]
					{
						new SqlServerVersionNumber("15.00.0000.0"),
						new SqlServerVersionNumber("15.00.9999.0")
					}
				),
				("SQL2022" ,new[]
					{
						new SqlServerVersionNumber("16.00.0000.0"),
						new SqlServerVersionNumber("16.00.9999.0")
					}
				),
				("SQL2000" ,new[]
					{
						new SqlServerVersionNumber("8.00.0000.0"),
						new SqlServerVersionNumber("8.00.9999.0")
					}
				),
				("SQL2005" ,new[]
					{
						new SqlServerVersionNumber("9.00.0000.0"),
						new SqlServerVersionNumber("9.00.9999.0")
					}
				),
				("SQL2008" ,new[]
					{
						new SqlServerVersionNumber("10.00.0000.0"),
						new SqlServerVersionNumber("10.00.9999.0"),
					}
				),
				("SQL2008R2" ,new[]
					{
						new SqlServerVersionNumber("10.50.0000.0"),
						new SqlServerVersionNumber("10.50.9999.0")
					}
				),
				("SQL2012" ,new[]
					{
						new SqlServerVersionNumber("11.00.0000.0"),
						new SqlServerVersionNumber("11.00.9999.0")
					}
				),
				("SQL2014" ,new[]
					{
						new SqlServerVersionNumber("12.00.0000.0"),
						new SqlServerVersionNumber("12.00.9999.0")
					}
				),
				("SQL VERSION:1.00.0000.00" ,new[]
					{
						new SqlServerVersionNumber("1.00.0000.0"),
					}
				),
				("SQL VERSION:2.00.0000.00" ,new[]
					{
						new SqlServerVersionNumber("2.00.0000.0"),
					}
				),
				("SQL VERSION:99.00.9999.00" ,new[]
					{
						new SqlServerVersionNumber("99.00.9999.0"),
					}
				),
			};

			// Act
			// Assert
			CombineAssertions(() =>
			{
				sqlServerGenerations.ForEach(sqlServerGenerationSet
					=> sqlServerGenerationSet.Item2.ForEach(realVersion
					=> AssertEquals(sqlServerGenerationSet.Item1, realVersion.SqlServerGeneration)));
			});
		}

		public void TestFormalSqlServerGeneration()
		{
			// Arrange
			var sqlServerGenerations = new[]
			{
				("Microsoft SQL Server 2016", new[]
					{
						new SqlServerVersionNumber("13.00.0000.0"),
						new SqlServerVersionNumber("13.00.9999.0")
					}
				),
				("Microsoft SQL Server 2017" ,new[]
					{
						new SqlServerVersionNumber("14.00.0000.0"),
						new SqlServerVersionNumber("14.00.9999.0")
					}
				),
				("Microsoft SQL Server 2019" ,new[]
					{
						new SqlServerVersionNumber("15.00.0000.0"),
						new SqlServerVersionNumber("15.00.9999.0")
					}
				),
				("Microsoft SQL Server 2022" ,new[]
					{
						new SqlServerVersionNumber("16.00.0000.0"),
						new SqlServerVersionNumber("16.00.9999.0")
					}
				),
				("Microsoft SQL Server 2000" ,new[]
					{
						new SqlServerVersionNumber("8.00.0000.0"),
						new SqlServerVersionNumber("8.00.9999.0")
					}
				),
				("Microsoft SQL Server 2005" ,new[]
					{
						new SqlServerVersionNumber("9.00.0000.0"),
						new SqlServerVersionNumber("9.00.9999.0")
					}
				),
				("Microsoft SQL Server 2008" ,new[]
					{
						new SqlServerVersionNumber("10.00.0000.0"),
						new SqlServerVersionNumber("10.00.9999.0")
					}
				),
				("Microsoft SQL Server 2008R2" ,new[]
					{
						new SqlServerVersionNumber("10.50.0000.0"),
						new SqlServerVersionNumber("10.50.9999.0")
					}
				),
				("Microsoft SQL Server 2012" ,new[]
					{
						new SqlServerVersionNumber("11.00.0000.0"),
						new SqlServerVersionNumber("11.00.9999.0")
					}
				),
				("Microsoft SQL Server 2014" ,new[]
					{
						new SqlServerVersionNumber("12.00.0000.0"),
						new SqlServerVersionNumber("12.00.9999.0")
					}
				),
				("Microsoft SQL Server version:1.00.0000.00" ,new[]
					{
						new SqlServerVersionNumber("1.00.0000.0"),
					}
				),
				("Microsoft SQL Server version:2.00.0000.00" ,new[]
					{
						new SqlServerVersionNumber("2.00.0000.0"),
					}
				),
				("Microsoft SQL Server version:99.00.9999.00" ,new[]
					{
						new SqlServerVersionNumber("99.00.9999.0"),
					}
				),
			};

			// Act
			// Assert
			CombineAssertions(() =>
			{
				sqlServerGenerations.ForEach(sqlServerGenerationSet
					=> sqlServerGenerationSet.Item2.ForEach(realVersion
					=> AssertEquals(sqlServerGenerationSet.Item1, realVersion.FormalSqlServerGeneration)));
			});
		}

		public void TestCompatibilityLevel()
		{
			// Arrange
			var sqlServerVersions = new[]
			{
				(130 ,new SqlServerVersionNumber("13.00.0000.0")),
				(140 ,new SqlServerVersionNumber("14.00.0000.0")),
				(150 ,new SqlServerVersionNumber("15.00.0000.0")),
				(160 ,new SqlServerVersionNumber("16.00.0000.0")),
				(200 ,new SqlServerVersionNumber("20.00.0000.0")),
				(990 ,new SqlServerVersionNumber("99.00.0000.0")),
				(990 ,new SqlServerVersionNumber("99.10.0000.0")),
				(990 ,new SqlServerVersionNumber("99.50.0000.0")),
			};

			// Act
			// Assert
			sqlServerVersions.ForEach(x => AssertEquals(x.Item1, x.Item2.CompatibilityLevel));
		}

		public void TestSqlMinimumSupportedGenerationEditionIsSQL2019()
		{
			AssertEquals("SQL2019 CTP 2.2", SqlServerVersionNumber.SqlMinimumSupportedGenerationEdition);
		}

		public void TestSupportedSqlServerGenerations_List()
		{
			// Arrange
			var supportedSqlServerGenerations = new[]
			{
				new Version("15.00.0000.0"),
				new Version("16.00.0000.0"),
			};

			// Act
			// Assert
			AssertSequencesEqual(supportedSqlServerGenerations, SqlServerVersionNumber.SupportedVersions.Select(v => v.Generation.AsVersion()));
		}

		public void TestObsoletedSqlServerGenerations_List()
		{
			// Arrange
			var obsoletedSqlServerGenerations = new[]
			{
				new Version("8.00.0000.0"),
				new Version("9.00.0000.0"),
				new Version("10.00.0000.0"),
				new Version("10.50.0000.0"),
				new Version("11.00.0000.0"),
				new Version("12.00.0000.0"),
				new Version("13.00.0000.0"),
				new Version("14.00.0000.0"),
			};

			// Act
			// Assert
			AssertSequencesEqual(obsoletedSqlServerGenerations, ObsoletedGenerations.Select(v => v.AsVersion()));
		}

		public void TestIsAboveMaximumSupportedSqlServerGeneration()
		{
			CombineAssertions(() =>
			{
				var olderOrSupported = SqlServerVersionNumber.SupportedVersions.Select(v => v.Generation.AsVersion().ToString());

				foreach (var version in olderOrSupported)
				{
					AssertEquals(version, false, new SqlServerVersionNumber(version).IsAboveMaximumSupportedSqlServerGeneration);
				}

				var max = SqlServerVersionNumber.SupportedVersions.Max().Generation;

				AssertEquals("Up-to-date Version", false, new SqlServerVersionNumber($"{max.Major}.{max.Minor}.9999.99").IsAboveMaximumSupportedSqlServerGeneration);
				AssertEquals("Bigger Minor", true, new SqlServerVersionNumber($"{max.Major}.{max.Minor + 1}.0000.00").IsAboveMaximumSupportedSqlServerGeneration);
				AssertEquals("Bigger Major", true, new SqlServerVersionNumber($"{max.Major + 1}.00.0000.00").IsAboveMaximumSupportedSqlServerGeneration);
			});
		}

		public void TestIsSupported()
		{
			void Test(string version, bool expectedIsSupported, string expectedFailureMessage)
			{
				var sqlServerVersionNumber = new SqlServerVersionNumber(version);
				var isSupported = sqlServerVersionNumber.IsSupported(out var failureMessage);

				AssertEquals($"[{version}].{nameof(isSupported)}", expectedIsSupported, isSupported);
				AssertEquals($"[{version}].{nameof(failureMessage)}", expectedFailureMessage, failureMessage);
			}

			CombineAssertions(() =>
			{
				var recommendedVersion = SqlServerVersionNumber.SupportedVersions.Max(); // We recommend to use the latest version supported by us.

				var overallMin = SqlServerVersionNumber.SupportedVersions.Min();
				var lowerGeneration = new Version(overallMin.Major - 1, overallMin.Minor, 9999, 99);
				Test(lowerGeneration.ToString(),
					expectedIsSupported: false,
					expectedFailureMessage:
					$"The SQL Server version [{VersionString(lowerGeneration)}] does not meet the minimum required version " +
					$"[{overallMin}] ({overallMin.Generation.FormalName} {overallMin.Description}).");

				foreach (var min in SqlServerVersionNumber.SupportedVersions)
				{
					var expectedWarning = (string)null;
					if (min.CompareTo(recommendedVersion) < 0)
					{
						expectedWarning =
							$"It is recommended to update your version of SQL Server to {recommendedVersion.GetLongDescription()} or higher.\r\n" +
							"WiseTech Global intends to increase the minimum required version of SQL Server, 12 months after the RTM.\r\n" +
							"To plan for this, upgrade your SQL Server version as soon as is practical.";
					}

					Test(min.ToString(),
						expectedIsSupported: true,
						expectedFailureMessage: expectedWarning);

					var higherCumulativeUpdate = new Version(min.Major, min.Minor, min.Build + 1, 0);
					Test(higherCumulativeUpdate.ToString(),
						expectedIsSupported: true,
						expectedFailureMessage: expectedWarning);

					var lowerCumulativeUpdate = new Version(min.Major, min.Minor, min.Build - 1, 0);
					Test(lowerCumulativeUpdate.ToString(),
						expectedIsSupported: false,
						expectedFailureMessage:
						$"The SQL Server version [{VersionString(lowerCumulativeUpdate)}] does not meet the minimum required " +
						$"Cumulative Update [{min}] ({min.Description}).");
				}

				Test("99.99.9999.99",
					expectedIsSupported: true,
					expectedFailureMessage: null);

				Test("99.99.9999.99",
					expectedIsSupported: true,
					expectedFailureMessage: null);

				string VersionString(Version v) => $"{v.Major:d1}.{v.Minor:d2}.{v.Build:d4}.{v.Revision:d2}";
			});
		}

		public void TestCompareTo()
		{
			const string sql22Cu4 = "16.0.4035.4";
			const string sql22Cu5 = "16.0.4045.3";

			Test(sql22Cu4, sql22Cu4, 0);
			Test(sql22Cu5, sql22Cu5, 0);

			Test(sql22Cu4, sql22Cu5, -1);
			Test(sql22Cu5, sql22Cu4, 1);

			void Test(string version1, string version2, int expectedResult)
			{
				// Arrange
				var versionNumber1 = new SqlServerVersionNumber(version1);
				var versionNumber2 = new SqlServerVersionNumber(version2);

				// Act
				var actualResult = versionNumber1.CompareTo(versionNumber2);

				// Assert
				AssertEquals(expectedResult, actualResult);
			}
		}

		public void TestIsEqualOrAboveSqlGeneration()
		{
			Assert((new SqlServerVersionNumber("15.00.0000.0")).IsEqualOrAboveSqlGeneration(SqlServerVersionNumber.SqlGeneration.Sql2019));
			Assert(!(new SqlServerVersionNumber("15.00.0000.0")).IsEqualOrAboveSqlGeneration(SqlServerVersionNumber.SqlGeneration.Sql2022));
			Assert((new SqlServerVersionNumber("16.00.0000.0")).IsEqualOrAboveSqlGeneration(SqlServerVersionNumber.SqlGeneration.Sql2019));
			Assert((new SqlServerVersionNumber("16.00.0000.0")).IsEqualOrAboveSqlGeneration(SqlServerVersionNumber.SqlGeneration.Sql2022));
		}

		static IEnumerable<SqlServerVersionNumber.SqlGeneration> ObsoletedGenerations
			=> SqlServerVersionNumber.SqlGeneration.All.Except(SqlServerVersionNumber.SupportedVersions.Select(s => s.Generation));

		class CurrentSqlServerTest : TestCase
		{
			public void TestIsAboveMaximumSupportedSqlServerGeneration()
			{
				AssertEquals(false, CurrentVersionNumber.IsAboveMaximumSupportedSqlServerGeneration);
			}

			public void TestSqlServerGeneration()
			{
				AssertMatch(
					$@"{nameof(CurrentVersionNumber.SqlServerGeneration)} should match expectation
This test could fail when:
1. SQL Server generation changes its rule, e.g. SQL2008R2
2. New SQL Server is released but its version hasn't been maintained in code base",
					new Regex(@"^SQL\d{4}$"),
					CurrentVersionNumber.SqlServerGeneration);
			}

			public void TestFormalSqlServerGeneration()
			{
				AssertMatch(
					$@"{nameof(CurrentVersionNumber.FormalSqlServerGeneration)} should match expectation
This test could fail when:
1. SQL Server generation changes its rule, e.g. Microsoft SQL Server 2008R2
2. New SQL Server is released but its version hasn't been maintained in code base",
					new Regex(@"^Microsoft SQL Server \d{4}$"),
					CurrentVersionNumber.FormalSqlServerGeneration);
			}

			public void TestIsMinimumRequiredVersionOrAbove()
			{
				AssertEquals(true, CurrentVersionNumber.IsMinimumRequiredVersionOrAbove);
			}

			SqlServerVersionNumber CurrentVersionNumber => Db.Connection.ServerVersionNumber;
		}

		[DatCapabilityRequirementLatestAvailableSqlServer]
		sealed class LatestAvailableSqlServerTest : CurrentSqlServerTest
		{
		}
	}
}
