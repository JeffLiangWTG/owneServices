using System.IO;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Client.EDI.ReleaseBuilds.Business;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	class PackageDownloaderForGlow
	{
		public string DownloadEDPToPath(ReleaseBuild releaseBuild, string edpDownloadFolderPath)
		{
			if (!Directory.Exists(edpDownloadFolderPath))
			{
				Directory.CreateDirectory(edpDownloadFolderPath);
			}

			// Ignore LicenceEnterprise (3-letter code) and force Glow enabled.
			// There should be no difference in Glow between different LicenceEnterprises since GLOW has no concept of ZClient.
			var builder = new RuntimePackageBuilder(releaseBuild, edpDownloadFolderPath);
			builder.Build();
			return builder.LastPackagePath;
		}
	}
}
