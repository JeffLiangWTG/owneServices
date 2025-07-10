using NUnit.Framework;

namespace Enterprise.DbUpgrader.Resource.Version.Tests
{
	class ScriptVersionTest : TestCase
	{
		public void TestApplicationVersion()
		{
			AssertEquals(Resource.ScriptVersion.Major, ScriptVersion.Application.Major);
			AssertEquals(Resource.ScriptVersion.Minor, ScriptVersion.Application.Minor);
		}
	}
}
