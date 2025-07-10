using System;
using System.Linq;
using NUnit.Framework;

namespace ResourceStrings.SpellCheck.TestRunner
{
	public abstract class SpellCheckBaseTest : TestCase
	{
		public void TestsAreGenerated()
		{
			const int staticTestNum = 500;
			var allMethods = GetType().GetMethods();
			var testMethodCount =
				allMethods.Count(method => method.Name.StartsWith("Test", StringComparison.OrdinalIgnoreCase));
			Assert("only " + testMethodCount + " methods are generated", testMethodCount > staticTestNum);
		}
	}
}
