using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Enterprise.Customs.BR.Business.Testing
{
	public static class BRMessageTestHelper
	{
		const string XmlFilesEmbeddedLocation = "Enterprise.Customs.BR.Business.Testing.Messaging.TestCases.";

		public static string GetEmbeddedResource(string resourceName, string embeddedLocation = XmlFilesEmbeddedLocation)
		{
			using (var stream = GetEmbeddedResourceAsStream(resourceName, embeddedLocation))
			{
				using (var reader = new StreamReader(stream))
				{
					var srtResult = reader.ReadToEnd();
					if (resourceName.EndsWith(".json"))
					{
						srtResult = JsonSerializer.Deserialize<object>(srtResult).ToJson();
					}
					return srtResult;
				}
			}
		}

		static Stream GetEmbeddedResourceAsStream(string resourceName, string embeddedLocation)
		{
			Assembly.GetExecutingAssembly().GetManifestResourceNames();
			var fileName = embeddedLocation + resourceName;
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName);
			return stream ?? throw new Exception($"Cannot find resource '{resourceName}'. Full name: {fileName}");
		}
	}
}
