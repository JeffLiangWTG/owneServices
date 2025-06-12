using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace CargoWise.eHub.Products.TWCustoms.IntegrationTests
{
	public class TestBase
	{
		public static Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}

		public static string GetResourceAsString(string resourceName)
		{
			using (var stream = GetEmbeddedResource(resourceName))
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
