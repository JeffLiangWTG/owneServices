using NUnit.Framework;

namespace Enterprise.DbUpgrader.Resource.Version.Tests
{
	class SchemaVersionTest : TestCase
	{
		public void TestApplicationVersion()
		{
			AssertEquals(Resource.SchemaVersion.ApplicationMajor, SchemaVersion.Application.Major);
			AssertEquals(Resource.SchemaVersion.ApplicationMinor, SchemaVersion.Application.Minor);
		}

		public void TestDocManagerVersion()
		{
			AssertEquals(Resource.SchemaVersion.DocumentMajor, SchemaVersion.DocManager.Major);
			AssertEquals(Resource.SchemaVersion.DocumentMinor, SchemaVersion.DocManager.Minor);
		}
	}
}

