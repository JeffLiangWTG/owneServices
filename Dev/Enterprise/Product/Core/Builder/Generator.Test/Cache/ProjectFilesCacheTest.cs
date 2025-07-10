using NUnit.Framework;

namespace Enterprise.Builder.Generator.Cache.Testing
{
	sealed class ProjectFilesCacheTest : TestCase
	{
		public void TestGetFilesInProject()
		{
			//Arrange
			var cache = new ProjectFilesCacheForTest();

			//Act
			var result = cache.GetFilesInProject("test");

			//Assert
			AssertEquals(4, result.Count);
		}
	}
}
