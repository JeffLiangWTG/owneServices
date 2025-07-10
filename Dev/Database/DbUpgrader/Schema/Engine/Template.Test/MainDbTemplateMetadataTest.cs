using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.DbUpgrader.Resource;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class MainDbTemplateMetadataTest : TestCase
	{
		/// <summary>
		/// Some prefixes are reserved for tables with specific purposes.
		///   * Client => Client-specific tables. They're ignored by DbUpgrader and never deleted. 
		///   * Dummy => Business Object Architecture Test Tables. 
		/// NO tables with these prefixes should be shipped with our product.
		/// </summary>
		public void TestNoCargoWiseOneTableNamesStartWithReservedPrefixes()
		{
			AssertNoTableNamesStartWithPrefix(new ScriptManager().MaindDbSchemaScript, "CLIENT");
			AssertNoTableNamesStartWithPrefix(new ScriptManager().MaindDbSchemaScript, "DUMMY");
		}

		public static void AssertNoTableNamesStartWithPrefix(string dbSchemaScript, string prefix)
		{
			var clientPrefixRegex = new Regex(@"CREATE\s+TABLE([^(]+?\b" + prefix + @".*?)\(", RegexOptions.IgnoreCase | RegexOptions.Singleline);
			var clientPrefixMatches = clientPrefixRegex.Matches(dbSchemaScript);

			Assert(
				String.Format(
					CultureInfo.InvariantCulture,
					"Found {0} tables starting with '{1}':\r\n\r\n{2}\r\n",
					clientPrefixMatches.Count,
					prefix,
					String.Join("\r\n\t", clientPrefixMatches.Cast<Match>().Select(m => m.Groups[1].Value.Trim()))
				),
				clientPrefixMatches.Count == 0
			);
		}
	}
}
