using System.IO;
using System.Reflection;
using CargoWise.IO;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Tests
{
	public static class ResourceManager
	{
		public static Stream GetFileResource(string resourceName)
		{
			var result = new VirtualMemoryStream();
			var writer = new StreamWriter(result, MessageEncoding.UTF8WithoutBOM);

			var assembly = Assembly.GetExecutingAssembly();
			using (var reader = new StreamReader(assembly.GetManifestResourceStream(assembly.GetName().Name + "." + resourceName), MessageEncoding.UTF8WithoutBOM))
			{
				reader.CopyTo(writer);
			}

			writer.Flush();
			result.Position = 0;
			return result;
		}

		public static string GetFileResourceString(string resourceName)
		{
			using (var fileResource = GetFileResource(resourceName))
			{
				return new StreamReader(fileResource, MessageEncoding.UTF8WithoutBOM).ReadToEnd();
			}
		}
	}
}
