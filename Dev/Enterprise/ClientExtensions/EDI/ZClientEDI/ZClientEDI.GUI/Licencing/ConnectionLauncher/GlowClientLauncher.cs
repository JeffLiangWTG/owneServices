using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CargoWise.Definitions.Authentication;
using CargoWise.IO;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	public class GlowClientLauncher : IConnectionLauncher
	{
		public GlowClientLauncher(LicenceConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException(nameof(connection));
			}

			this.connection = connection;
			this.downloader = new PackageDownloaderForGlow();
			this.tokenProvider = new TokenizedAccessControl();
		}

		readonly PackageDownloaderForGlow downloader;
		readonly LicenceConnection connection;
		readonly ITokenizedAccessControl tokenProvider;

		public bool ShowProgressForm
		{
			get { return true; }
		}

		public void Launch(Progress progress)
		{
			var edisupportBinariesPath = Path.Combine(Temp.TempPath, "EDISupportBinaries", "Glow");
			var releaseBuild = connection.Database.CurrentVersion;
			if (releaseBuild == null)
			{
				Globals.Message.ShowError("Unable to locate related ReleaseBuild for this installation. It may have been deleted.");
				return;
			}

			var extractedEDPRootPath = Path.Combine(edisupportBinariesPath, releaseBuild.VersionNumber.ToString());
			var pathToGlow = Path.Combine(extractedEDPRootPath, "Distribution", "Application", "Glow", "CargoWiseOne.Desktop.exe");

			if (!File.Exists(pathToGlow))
			{
				DownloadAndExtractEDP(progress, edisupportBinariesPath, extractedEDPRootPath);
			}

			LaunchGlow(pathToGlow);
		}

		void DownloadAndExtractEDP(Progress progress, string edisupportBinariesPath, string extractedEDPRootPath)
		{
			var edpDownloadFolderPath = Path.Combine(edisupportBinariesPath, "EDPs");
			var edpFilePath = downloader.DownloadEDPToPath(connection.Database.CurrentVersion, edpDownloadFolderPath);

			EdpFile.Unpack(edpFilePath, extractedEDPRootPath, progress, createFilter());
			File.Delete(edpFilePath);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "We need to launch GLOW with all of it's support files and binaries. FileOpener won't cut it.")]
		void LaunchGlow(string pathToGlow)
		{
			var arguments = new List<string>
			{
				"-GlowServer:" + connection.LK_RemoteAccessAddress,
				"-SupportToken:" + tokenProvider.CreateLimitedToken(AccessTokenTypes.SupportIdentity, new AccessTokenInfo("", Env.CurrentUser.PK, GlbStaffSchema.Constants.Prefix))
				// TODO: Launch with a flag to disable Enterprise Integration because that will just bounce back to ediProd.
			};

			var argumentsAsString = string.Join(" ", arguments.Select(x => '"' + x + '"'));

			Process.Start(pathToGlow, argumentsAsString);
		}

		string createFilter() => "^Distribution/Application/Glow.*";
	}
}

