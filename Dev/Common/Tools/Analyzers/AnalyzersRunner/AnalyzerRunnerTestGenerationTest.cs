using System;
using System.Linq;
using NUnit.Framework;

namespace AnalyzersRunner
{
	class AnalyzerRunnerTestGenerationTest : TestCase
	{
		public void TestRulesAreGenerated()
		{
			var count = typeof(RunAnalyzers).GetMethods().Where(method => method.Name.StartsWith("Test", StringComparison.OrdinalIgnoreCase)).Count();
			AssertGreaterThan("Only " + count + " methods found", count, 200);
		}
	}
}
