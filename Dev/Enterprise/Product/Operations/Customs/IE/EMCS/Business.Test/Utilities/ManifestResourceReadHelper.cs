using System.IO;
using System.Reflection;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	public static class ManifestResourceReadHelper
	{
		public static string ReadManifestResourceContent(string resourceDetails)
		{
			var assembly = Assembly.GetCallingAssembly();
			using (var stream = assembly.GetManifestResourceStream(resourceDetails))
			{
				if (stream != null)
				{
					using (var streamReader = new StreamReader(stream))
					{
						return streamReader.ReadToEnd();
					}
				}
				else
				{
					var resourceNames = assembly.GetManifestResourceNames();
					var report = $"Missing resource: {resourceDetails} \r\nIn:\r\n";
					report += string.Join("\r\n", resourceNames);
					throw new IOException(report);
				}
			}
		}
	}
}
