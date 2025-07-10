using System.IO;
using System.Reflection;

namespace Enterprise.Dash.Business.Tests
{
	public static class TestHelper
	{
		public static string ReadEmbeddedFile(string fileName)
		{
			var testFileName = $"TestFiles.{fileName}";
			var fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + testFileName;

			using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName))
			using (var streamReader = new StreamReader(resource))
			{
				var testFileContent = streamReader.ReadToEnd();
				return testFileContent;
			}
		}
	}
}
