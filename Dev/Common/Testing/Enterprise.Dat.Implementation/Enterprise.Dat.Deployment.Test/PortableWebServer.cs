using System;
using System.IO;
using System.IO.Compression;
using CargoWise.IO;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed class PortableWebServer : IDisposable
	{
		readonly TempDirectory tempDir;
		public string InstallPath => tempDir.DirectoryName;

		public PortableWebServer()
		{
			tempDir = new TempDirectory();
			ZipFile.ExtractToDirectory(Path.Combine(Path.GetDirectoryName(typeof(PortableWebServer).Assembly.Location), "CargoWiseOneWebServerSetup.zip"), InstallPath);
		}

		public void Dispose()
		{
			tempDir.Dispose();
		}
	}
}
