using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.DbUpgrader.Scripts.Abstractions;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests
{
	abstract class EdwHashTest : DbCreateScriptTest
	{
		const string edwScriptFolderPath = "/CargoWise.DbUpgrader/src/Scripts/Scripts.Definitions/BusinessIntelligence/EDW/";
		protected abstract string edwScriptPath { get; }
		protected abstract string expectedMainDbFunctionHash { get; }
		protected abstract string expectedEdwDbFunctionHash { get; }

		protected abstract DbCreateScript GetMainDbFunction();
		protected abstract BiCreateScript GetEdwDbFunction();

		public void TestEdwHash_MainDbFunctionHasNotChanged()
		{
			var mainDbFunction = GetMainDbFunction();
			var actualHash = GetHashString(mainDbFunction.Text);
			var errorMessage = $"A version of this '{mainDbFunction.GetType().Name}' function exists in the EDW database (" +
									edwScriptFolderPath + edwScriptPath + "), " +
									"please modify the EDW script so that it has the same output. " +
									"If no changes are required, please just update the expectedMainDbFunctionHash field and recalculate the hash. " +
									"For any questions reach out to the Business Intelligence team";
			AssertEquals(errorMessage, expectedMainDbFunctionHash, actualHash);
		}

		public void TestEdwHash_EdwDbFunctionHasNotChanged()
		{
			var edwDbFunction = GetEdwDbFunction();
			var actualHash = GetHashString(edwDbFunction.Text);
			var errorMessage = $"A version of this '{edwDbFunction.GetType().Name}' function exists in the main database, " +
										"please modify the main DB script so that it has the same output. " +
										"If no changes are required, please just update the expectedEdwDbFunctionHash field and recalculate the hash. " +
										"For any questions reach out to the Business Intelligence team.";
			AssertEquals(errorMessage, expectedEdwDbFunctionHash, actualHash);
		}

		public void TestEdwHash_EdwFunctionConsistency()
		{
			var mainDbFunction = GetMainDbFunction();
			var edwDbFunction = GetEdwDbFunction();

			CombineAssertions("There is an inconsistency in your test", () =>
			{
				var edwDbFunctionName = edwDbFunction.GetType().Name;
				Assert($"EDW script path should have the same name as the EDW function {edwDbFunctionName} but was {edwScriptPath}", edwScriptPath.Contains(edwDbFunctionName));

				var mainDbFunctionName = mainDbFunction.GetType().Name;
				var testClassName = GetTestClassName();
				Assert($"This class is for the {testClassName} function but you are testing the {mainDbFunctionName} function",
					testClassName.Equals(mainDbFunctionName, StringComparison.OrdinalIgnoreCase)
					);

				var edwDbFunctionNameWithoutPrefix = RemoveEdwFunctionPrefixes(edwDbFunctionName);
				Assert($"Main DB function {mainDbFunctionName} should have the same name as the EDW function {edwDbFunctionNameWithoutPrefix}",
					 mainDbFunctionName.Equals(edwDbFunctionNameWithoutPrefix, StringComparison.OrdinalIgnoreCase));
			});
		}

		string GetTestClassName()
		{
			string pattern = "(?i)(EdwHashTest|Test)$";  // Case-insensitive, ensures removal of suffix at the end
			string result = Regex.Replace(this.GetType().Name, pattern, "");
			return result;
		}

		string RemoveEdwFunctionPrefixes(string edwDbFunctionName)
		{
			var allowedPrefixes = new List<string> { "vw_GRP__" };
			return edwDbFunctionName.Substring(
				 (allowedPrefixes.FirstOrDefault(prefix => edwDbFunctionName.StartsWith(prefix)) ?? string.Empty).Length
			);
		}
	}
}
