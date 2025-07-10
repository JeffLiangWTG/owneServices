using System.IO;
using System.Reflection;

namespace Enterprise.Customs.IT.Business.Testing;

public static class ManifestResourceHelper
{
	public static string ReadManifestResourceContent(string resourceDetails)
	{
		var result = string.Empty;
		using (var streamReader = ManifestResourceContentStream(resourceDetails, Assembly.GetCallingAssembly()))
		{
			result = streamReader.ReadToEnd();
		}
		return result;
	}

	public static StreamReader ManifestResourceContentStream(string resourceDetails, Assembly assembly = null)
	{
		assembly = assembly ?? Assembly.GetCallingAssembly();
		var stream = assembly.GetManifestResourceStream(resourceDetails)
			?? throw new IOException($"Missing resource: {resourceDetails}");

		var result = new StreamReader(stream);
		return result;
	}
}
