using System.IO;
using System.Reflection;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Dat.Implementation
{
	static class EmbeddedPowerShellScript
	{
		public static TempFile GetLocalTempFile(string fileName)
		{
			var localScriptFile = TempFile.NewWithExtension("ps1");
			using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Dat.Implementation." + fileName))
			using (var scriptFileStream = File.Create(localScriptFile.Filename))
			{
				resourceStream.CopyTo(scriptFileStream);
			}

			return localScriptFile;
		}
	}
}
