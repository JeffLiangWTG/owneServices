using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace CargoWise.Data
{
	// This the class that keep all version information. Once Sql server needs a bump, update this class
	// and change the date in SqlCutOverHelper.
	public sealed partial class SqlServerVersionNumber : IComparable<SqlServerVersionNumber>
	{
		public SqlServerVersionNumber(string productVersionText)
		{
			Argument.NotNull(productVersionText, nameof(productVersionText));
			version = Version.Parse(productVersionText);
		}

		public bool IsAboveMaximumSupportedSqlServerGeneration
		{
			get
			{
				var max = SupportedVersions.Max().Generation;
				return version.Major > max.Major || (version.Major == max.Major && version.Minor > max.Minor);
			}
		}

		public string SqlServerGeneration
		{
			get
			{
				var sqlVersion = SqlGeneration.Find(version);
				return sqlVersion != null
					? sqlVersion.Name
					: $"SQL VERSION:{SqlVersion.GetCanonicalVersionString(version)}";
			}
		}

		public string FormalSqlServerGeneration
		{
			get
			{
				var sqlVersion = SqlGeneration.Find(version);
				return sqlVersion != null
					? sqlVersion.FormalName
					: $"Microsoft SQL Server version:{SqlVersion.GetCanonicalVersionString(version)}";
			}
		}

		public int CompatibilityLevel
		{
			get { return version.Major * 10; }
		}

		public bool IsMinimumRequiredVersionOrAbove
		{
			get
			{
				if (IsAboveMaximumSupportedSqlServerGeneration)
				{
					return true;
				}

				var supportedVersion = GetSupportedVersion(version.Major, version.Minor);
				return supportedVersion != null && supportedVersion.Build <= version.Build;
			}
		}

		public bool IsSupported(out string failureMessage)
		{
			failureMessage = null;
			if (!IsMinimumRequiredVersionOrAbove)
			{
				failureMessage = GetBelowMinimumVersionError();
				return false;
			}

			var sqlVersion = GetSupportedVersion(this.version.Major, this.version.Minor);
			var maxSupported = SupportedVersions.Max();
			if (sqlVersion != null && sqlVersion != maxSupported)
			{
				failureMessage =
					$"It is recommended to update your version of SQL Server to {maxSupported.GetLongDescription()} or higher.\r\n" +
					"WiseTech Global intends to increase the minimum required version of SQL Server, 12 months after the RTM.\r\n" +
					"To plan for this, upgrade your SQL Server version as soon as is practical.";
			}

			return true;
		}

		string GetBelowMinimumVersionError()
		{
			var supportedVersion = GetSupportedVersion(version.Major, version.Minor);
			if (supportedVersion != null)
			{
				var min = supportedVersion;

				return $"The SQL Server version [{ToString()}] does not meet the minimum required Cumulative Update [{min}] ({min.Description}).";
			}
			else
			{
				var min = SupportedVersions.Min();

				return $"The SQL Server version [{ToString()}] does not meet the minimum required version [{min}] ({min.Generation.FormalName} {min.Description}).";
			}
		}

		public override string ToString() => SqlVersion.GetCanonicalVersionString(version);

		readonly Version version;

		public static string SqlMinimumSupportedGenerationEdition
		{
			get
			{
				var minVersion = SupportedVersions.Min();
				return $"{minVersion.Generation.Name} {minVersion.Description}";
			}
		}

		static SqlVersion GetSupportedVersion(int major, int minor)
		{
			return SupportedVersions.FirstOrDefault(s => s.Generation.Major == major && s.Generation.Minor == minor);
		}

		/// <summary>
		///  Contains the list of all supported versions
		///  This is the part where should be updated when supporting new versions of SQL Server
		///  See https://learn.microsoft.com/en-us/troubleshoot/sql/releases/download-and-install-latest-updates for builds
		/// </summary>
		public static SqlVersion[] SupportedVersions { get; } =
		{
			new SqlVersion(SqlGeneration.Sql2019, 1200, "CTP 2.2"),
			new SqlVersion(SqlGeneration.Sql2022, 1000, "RTM"),
		};

		public int CompareTo(SqlServerVersionNumber serverVersionNumber)
		{
			return version.CompareTo(serverVersionNumber.version);
		}

		public bool IsEqualOrAboveSqlGeneration(SqlGeneration sqlGeneration)
		{
			return version.Major >= sqlGeneration.Major;
		}

		public class SqlVersion : IComparable<SqlVersion>
		{
			public SqlVersion(SqlGeneration generation, int build, string description)
			{
				Generation = generation ?? throw new ArgumentNullException(nameof(generation));
				Description = description ?? throw new ArgumentNullException(nameof(description));
				Build = build;
			}

			public SqlGeneration Generation { get; }
			public int Major => Generation.Major;
			public int Minor => Generation.Minor;
			public int Build { get; }
			public string Description { get; }

			public int CompareTo(SqlVersion other)
			{
				if (other == null)
				{
					throw new ArgumentNullException(nameof(other));
				}

				var generationComparison = Generation.CompareTo(other.Generation);
				return generationComparison != 0 ? generationComparison : Build.CompareTo(other.Build);
			}

			public string GetLongDescription() => $"{Generation.FormalName} - {Description}";

			public Version AsVersion() => new Version(Generation.Major, Generation.Minor, Build, 0);

			public override string ToString() => GetCanonicalVersionString(Generation.Major, Generation.Minor, Build);

			public static string GetCanonicalVersionString(Version v) => GetCanonicalVersionString(v.Major, v.Minor, v.Build, v.Revision);
			static string GetCanonicalVersionString(int major, int minor, int build, int revision = 0) => $"{major:d1}.{minor:d2}.{build:d4}.{revision:d2}";
		}

		public class SqlGeneration : IComparable<SqlGeneration>
		{
			SqlGeneration(int major, int minor, string name, string formalName)
			{
				Major = major;
				Minor = minor;
				Name = name;
				FormalName = formalName;
			}

			public int Major { get; }
			public int Minor { get; }
			public string Name { get; }
			public string FormalName { get; }

			public int CompareTo(SqlGeneration other)
			{
				if (other == null)
				{
					throw new ArgumentNullException(nameof(other));
				}

				var majorComparison = Major.CompareTo(other.Major);
				return majorComparison != 0 ? majorComparison : Minor.CompareTo(other.Minor);
			}

			public Version AsVersion() => new Version(Major, Minor, 0, 0);

			public static SqlGeneration Sql2000 { get; } = new SqlGeneration(8, 0, "SQL2000", "Microsoft SQL Server 2000");
			public static SqlGeneration Sql2005 { get; } = new SqlGeneration(9, 0, "SQL2005", "Microsoft SQL Server 2005");
			public static SqlGeneration Sql2008 { get; } = new SqlGeneration(10, 0, "SQL2008", "Microsoft SQL Server 2008");
			public static SqlGeneration Sql2008R2 { get; } = new SqlGeneration(10, 50, "SQL2008R2", "Microsoft SQL Server 2008R2");
			public static SqlGeneration Sql2012 { get; } = new SqlGeneration(11, 0, "SQL2012", "Microsoft SQL Server 2012");
			public static SqlGeneration Sql2014 { get; } = new SqlGeneration(12, 0, "SQL2014", "Microsoft SQL Server 2014");
			public static SqlGeneration Sql2016 { get; } = new SqlGeneration(13, 0, "SQL2016", "Microsoft SQL Server 2016");
			public static SqlGeneration Sql2017 { get; } = new SqlGeneration(14, 0, "SQL2017", "Microsoft SQL Server 2017");
			public static SqlGeneration Sql2019 { get; } = new SqlGeneration(15, 0, "SQL2019", "Microsoft SQL Server 2019");
			public static SqlGeneration Sql2022 { get; } = new SqlGeneration(16, 0, "SQL2022", "Microsoft SQL Server 2022");

			public static IEnumerable<SqlGeneration> All => new[] { Sql2000, Sql2005, Sql2008, Sql2008R2, Sql2012, Sql2014, Sql2016, Sql2017, Sql2019, Sql2022 };

			public static SqlGeneration Find(Version v) => All.FirstOrDefault(s => s.Major == v.Major && s.Minor == v.Minor);
		}
	}
}
