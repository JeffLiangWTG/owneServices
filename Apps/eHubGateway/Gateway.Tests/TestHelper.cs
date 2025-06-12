using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.eHub.Gateway.Tests
{
	public static class TestHelper
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

		public static string GetEmbeddedResourceAsString(string resourceName)
		{
			using (var stream = GetEmbeddedResource(resourceName))
			{
				return new StreamReader(stream).ReadToEnd();
			}
		}
    }
}
