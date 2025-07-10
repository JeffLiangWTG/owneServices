using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	public class ApplicationLoggingTest : TestCase
	{
		public void TestAppLogJsonExists()
		{
			// Arrange
			var file = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)!, "applog.json");

			// Act & Assert
			Assert(File.Exists(file));
		}
	}
}
