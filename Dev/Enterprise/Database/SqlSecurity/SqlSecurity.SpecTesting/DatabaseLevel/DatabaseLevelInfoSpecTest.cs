using System.Reflection;
using NUnit.Framework;
using WTG.TestHelpers.SpecTesting;

namespace Enterprise.SqlSecurity.SpecTesting
{
	internal class DatabaseLevelInfoSpecTest
	{
		[Test]
		[Property("DAT:CapabilityRequirements", "SQL2022")]
		public void TestSpecMatches()
		{
			SpecAssert.AssertAllSpecMatches(Assembly.GetExecutingAssembly(), new DatabaseLevelInfoSpecBuilder());
		}
	}
}
