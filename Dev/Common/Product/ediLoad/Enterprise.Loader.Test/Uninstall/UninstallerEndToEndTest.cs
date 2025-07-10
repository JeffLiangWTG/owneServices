using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Loader.Common;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.Loader.Testing
{
	[TestRequiresAdministrativePrivileges(@"Requires write access to 'C:\Program Files\WiseTech Global\CargoWise' folder")]
	class UninstallerEndToEndTest : TestCase
	{
		[DeveloperOnlyTest()]
		public void TestUninstall_Removes_OrphanOldVersion()
		{
			// Arrange
			var output = new StringBuilder();
			var targetVersion = ReleaseInfo.Instance.VersionNumber;
			var targetVersionDirectory = InstallTargetVersion(targetVersion);

			Assert(Directory.Exists(targetVersionDirectory));
			Assert(Directory.GetFiles(targetVersionDirectory).Any());
			Assert(
				$"Old version '{targetVersion}' that does not appear in the CurrentVersion file.",
				!currentVersionFile.RetrieveAllVersionNumbers().Contains(targetVersion.ToString()));

			// Act
			var process = ExecuteCargoWiseOneStart(new[]
			{
				Db.ServerName,
				Db.DatabaseName,
				EnterpriseConfiguration.UninstallArgument,
				Configuration.NoUIArgument,
			}, output);
			process.Start();
			process.WaitForExit();

			// Assert
			Assert(
				$"Old version '{targetVersionDirectory}' that does not appear in the CurrentVersion file should have been removed {output}.",
				!Directory.Exists(targetVersionDirectory));
		}

		Process ExecuteCargoWiseOneStart(string[] arguments, StringBuilder processLogs)
		{
			var startInfo = new ProcessStartInfo()
			{
				WorkingDirectory = ExecutableDirectory,
				FileName = cargoWiseStartExePath,
				Arguments = string.Join(" ", arguments),
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};

			var process = new Process();
			process.StartInfo = startInfo;
			process.OutputDataReceived += (sender, e) => { processLogs.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}|{e?.Data}"); };
			process.ErrorDataReceived += (sender, e) => { processLogs.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}|Error: {e?.Data}"); };

			return process;
		}

		protected override void SetUp()
		{
			base.SetUp();

			configuration = new EnterpriseConfiguration();
			cargoWiseStartExePath = Path.Combine(configuration.BaseTargetPath, CargoWiseStartExeFileName);
			currentVersionFilePath = Path.Combine(configuration.BaseTargetPath, CurrentVersionFileName);

			BackupBasePathIfExistsAndCreateNew(currentVersionFilePath);
			CopyCargoWiseOneStartExeIfNotExists();

			currentVersionFile = new CurrentVersionFile(currentVersionFilePath);

			void BackupBasePathIfExistsAndCreateNew(string currentVersionFilePath)
			{
				baseDirectoryPath = Path.GetDirectoryName(currentVersionFilePath);
				var baseDirectoryName = Path.GetFileName(baseDirectoryPath);
				if (Directory.Exists(baseDirectoryPath))
				{
					baseDirectoryBackupPath = Path.Combine(
						Directory.GetParent(baseDirectoryPath).FullName,
						Invariant($"{baseDirectoryName}_{Guid.NewGuid()}"));

					Directory.Move(baseDirectoryPath, baseDirectoryBackupPath);
				}

				Directory.CreateDirectory(baseDirectoryPath);
				File.WriteAllText(currentVersionFilePath, "");
			}

			void CopyCargoWiseOneStartExeIfNotExists()
			{
				if (!File.Exists(cargoWiseStartExePath))
				{
					File.Copy(Path.Combine(ExecutableDirectory, CargoWiseStartExeFileName), cargoWiseStartExePath);
				}
			}
		}

		protected override void TearDown()
		{
			if (Directory.Exists(baseDirectoryPath))
			{
				Directory.Delete(baseDirectoryPath, true);
			}

			if (!string.IsNullOrEmpty(baseDirectoryBackupPath) && Directory.Exists(baseDirectoryBackupPath))
			{
				Directory.Move(baseDirectoryBackupPath, baseDirectoryPath);
			}

			base.TearDown();
		}

		string InstallTargetVersion(VersionNumber targetVersion)
		{
			if (!TryCreateVersionDirectory(configuration.BaseTargetPath, targetVersion, out var targetVersionDirectory))
			{
				if (currentVersionFile.RetrieveAllVersionNumbers().Contains(targetVersion.ToString()))
				{
					currentVersionFile.DeleteVersionRecord(Db.ServerName, Db.DatabaseName);
				}
			}

			Directory.GetFiles(ExecutableDirectory, "*.dll", SearchOption.TopDirectoryOnly)
				.Select(x => Path.GetFileName(x))
				.ForEach(fileName =>
					File.Copy(
						Path.Combine(ExecutableDirectory, fileName),
						Path.Combine(targetVersionDirectory, fileName)));

			return targetVersionDirectory;
		}

		static bool TryCreateVersionDirectory(string baseDirectory, VersionNumber version, out string versionDirectory)
		{
			versionDirectory = Path.Combine(baseDirectory, version.ToString());
			if (Directory.Exists(versionDirectory))
			{
				return false;
			}

			Directory.CreateDirectory(versionDirectory);
			return true;
		}

		const string CurrentVersionFileName = "CurrentVersion";
		const string CargoWiseStartExeFileName = "CargoWise.Start.exe";

		CurrentVersionFile currentVersionFile;
		Configuration configuration;
		string currentVersionFilePath;
		string cargoWiseStartExePath;
		string baseDirectoryPath;
		string baseDirectoryBackupPath;
	}
}
