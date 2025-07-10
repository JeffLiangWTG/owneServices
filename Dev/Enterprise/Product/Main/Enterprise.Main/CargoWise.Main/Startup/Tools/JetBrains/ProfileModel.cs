using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Environment = System.Environment;

namespace CargoWise.Main.Startup.Tools.JetBrains
{
	public abstract class ProfileModel : NonPersistentBusinessObject, IDisposable
	{
		public static MultilingualString ProfilerDialogTitle => ResString.GetMultilingualString("dd9ee215-84ad-4ca5-ac10-77113e59e555", "Profiler");

		/// <summary>
		///		If the user doesn't stop profiling before this threshold, the profiling will be stopped automatically.
		///		There is no valid reason to profile longer than 60 minutes.
		/// </summary>
		protected static TimeSpan MaxProfileDuration => TimeSpan.FromMinutes(60);

		protected ProfileModel(IDialogService dialogService)
		{
			DialogService = Argument.NotNull(dialogService, nameof(dialogService));
		}

		#region Properties

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "It is going to be used only by the main thread anyway")]
		public static bool IsSessionStarted { get; set; }

		#region Output

		public ZString Output
		{
			get => output;
			set => SetNonPersistentPropertyValue(OutputInfo, ref output, value);
		}

		public ZPropertyInfo OutputInfo => GetZPropertyInfo(nameof(Output), "Output");

		ZString output;

		#endregion

		#region IsStarted

		public ZBool IsStarted
		{
			get => isStarted;
			private set => SetNonPersistentPropertyValue(IsStartedInfo, ref isStarted, value);
		}

		public ZPropertyInfo IsStartedInfo => GetZPropertyInfo(nameof(IsStarted), "Is Started");

		ZBool isStarted;

		#endregion

		public string UserSnapshotsPath { get; private set; }
		string TempSnapshotsPath => Path.Combine(Temp.TempPath, "PerformanceSnapshots");

		/// <summary>
		///		The actual working snapshots path. It is going to be equal <see cref="UserSnapshotsPath"/> if CW1 is run locally, and
		///		some temporary folder based on <see cref="TempSnapshotsPath"/> if CW1 is run via RDP.
		/// </summary>
		protected string SnapshotsPath { get; private set; }

		protected IDialogService DialogService { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI011:TempPathRule", Justification = "The suggested one creates a path per process. We need something more permanent here.")]
		protected string PrerequisitePath
		{
			get
			{
				var uri = new Uri(PrerequisiteDownloadUrl);
				var fileName = Path.GetFileNameWithoutExtension(uri.LocalPath);

				return Path.Combine(Path.GetTempPath(), BrandingFactory.Instance.CompanyName, fileName);
			}
		}

		protected abstract string ProfilerName { get; }
		protected abstract string PrerequisiteDownloadUrl { get; }

		/// <summary>
		///		An expected SHA512 hash of the prerequisite zip package.
		/// </summary>
		protected abstract string PrerequisiteCheckSum { get; }

		#endregion

		public void Init()
		{
			// Trying to delete old snapshots from previous runs if we failed to delete them last time.
			Cleanup();

			var profilerExecutable = Path.Combine(PrerequisitePath, FormattableString.Invariant($"{ProfilerName}.exe"));
			var isPrerequisiteExist = File.Exists(profilerExecutable);
			if (!isPrerequisiteExist)
			{
				var license = GetLicense();

				if (!DialogService.AcceptLicenseAgreement(license))
				{
					return;
				}

				if (Directory.Exists(PrerequisitePath))
				{
					// To make sure we extract the the tooling to a clean folder which doesn't conflict with any existing installation
					Directory.Delete(PrerequisitePath, true);
				}

				var profilerZip = DialogService.Download(PrerequisiteDownloadUrl);
				if (string.IsNullOrEmpty(profilerZip))
				{
					Globals.Message.ShowError(
						Res.GetString("1992f3a5-030a-44e2-a2d1-2b4d97d880a2", "Unable to download the prerequisite."),
						ProfilerDialogTitle);
					return;
				}

				var checkSum = CalculateCheckSum(profilerZip);
				if (checkSum != PrerequisiteCheckSum)
				{
					Globals.Message.ShowError(
						Res.GetString("ae1ea11c-825a-4a2f-b79b-d6e8c82c58fe", "The downloaded package has checksum mismatch. Please try downloading again."),
						ProfilerDialogTitle);
					return;
				}

				if (!ExtractZip(profilerZip, PrerequisitePath))
				{
					Globals.Message.ShowError(
						Res.GetString("59c4f4fa-4c90-46e0-8f91-892b826d0a74", "Unable to install the prerequisite. The downloaded package may be corrupted. Please try again and if the problem persists then please contact WTG support."),
						ProfilerDialogTitle);
					return;
				}
			}

			// Prerequisite must be already installed by this point, so, it should execute fast.
			// We call it just to force jetbrains to initialize the prerequisite path internally so that
			// it knew where to look for cli.
			EnsurePrerequisite();

			var defaultUserSnapshotsPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				BrandingFactory.Instance.CompanyName,
				"PerformanceSnapshots");

			Directory.CreateDirectory(defaultUserSnapshotsPath);

#if !WINZOR

			UserSnapshotsPath = DialogService.SelectFolder(defaultUserSnapshotsPath, Res.GetString("f5aaa4f0-b118-4225-8ee3-ae70e86ba750", "Please select a folder for snapshots"));
			if (string.IsNullOrEmpty(UserSnapshotsPath))
			{
				return;
			}

#endif

			// In case if CW1 is run using RDP, dotTrace and dotMemory cannot save snapshots to the mapped remote path.
			// So, as a workaround we save snapshots to local temp folder and then copy them to the remote one.
			SnapshotsPath = ZOpenFileDialog.IsRemote
				? Path.Combine(TempSnapshotsPath, Guid.NewGuid().ToString("N"))
				: UserSnapshotsPath;

			if (!Directory.Exists(SnapshotsPath))
			{
				Directory.CreateDirectory(SnapshotsPath);
			}

			Log(Res.GetString("f5930d59-367b-4ed3-a308-6a1cc5e745e2", "Click Start to begin profiling"));
			ShowForm();
		}

		public void StartProfiling()
		{
			if (IsStarted)
			{
				throw new InvalidOperationException("The profiling session is already started");
			}

			IsStarted = DoStartProfiling();
			if (IsStarted)
			{
				timeoutTask?.Dispose();
				timeoutTask = new CancellationTokenSource(MaxProfileDuration);
				timeoutTask.Token.Register(() =>
				{
					Log(Res.GetString("cc6276d3-41d8-43e2-883c-cf6bba82862b", "The profiling session wasn't stopped in {0}. The session will be stopped automatically.", MaxProfileDuration));
					StopProfiling();
				});
			}
		}

		public void StopProfiling()
		{
			if (!IsStarted)
			{
				throw new InvalidOperationException("The profiling session is not started");
			}

			OpenSnapshotsFolder();
			timeoutTask?.Dispose();

			IsStarted = false;
		}

		public void Cleanup()
		{
			if (!ZOpenFileDialog.IsRemote)
			{
				// Nothing to clean up. We write directly to the user selected folder.
				return;
			}

			try
			{
				Directory.Delete(TempSnapshotsPath, true);
			}
			catch (IOException)
			{
				// We don't care about errors. There is nothing we can do.
				// It is temp folder anyway, the user or system will clean up it anyway sometimes.
			}
		}

		#region IDisposable

		public void Dispose()
		{
			timeoutTask?.Dispose();
		}

		#endregion

		protected void OpenSnapshotsFolder()
		{
#if !WINZOR

			if (!DoStopProfiling())
			{
				return;
			}

			if (ZOpenFileDialog.IsRemote)
			{
				RunSafe(
					() => CopyFiles(SnapshotsPath, UserSnapshotsPath),
					Res.GetString("83deb66e-7626-4af2-8099-ce0d6130990f", "Copying snapshots to the selected folder ({0})", UserSnapshotsPath));
			}
			else
			{
				new ProgramLauncher().Launch("explorer.exe", UserSnapshotsPath);
			}

#else

			using var fileStream = DialogService.SelectSaveAs("Profile.zip");

			if (fileStream == default)
			{
				return;
			}

			if (!DoStopProfiling())
			{
				return;
			}

			Log(Res.GetString("f10fdd08-64bd-424e-b4bc-5f1c0bd07a39", "Creating profile zip file..."), newLine: false);

			var zipPath = Path.Combine(TempSnapshotsPath, $"{Guid.NewGuid().ToString("N")}.zip");
			ZipFile.CreateFromDirectory(SnapshotsPath, zipPath);

			Log(Res.GetString("74a07998-fb28-4561-8862-83f481221685", "OK"), newLine: true);

			using var zipStream = new FileStream(zipPath, FileMode.Open);

			zipStream.CopyTo(fileStream);

			Log(Res.GetString("21778d07-de10-4e6d-9162-d6674e38f147", "Profile zip file saved"));

#endif
		}

		protected abstract bool DoStartProfiling();
		protected abstract bool DoStopProfiling();
		protected abstract void ShowForm();
		protected abstract void EnsurePrerequisite();

		protected bool RunSafe(Action action, string actionName)
		{
			Log(FormattableString.Invariant($"{actionName}... "), newLine: false);

			try
			{
				action();

				Log(Res.GetString("74a07998-fb28-4561-8862-83f481221685", "OK"), newLine: true);
				return true;
			}
			catch (Exception ex)
			{
				Log(Res.GetString("e33e9688-9ec8-4902-8713-c74c142b3c4b", "FAILED"), newLine: true);
				Log(ex.ToString());
				return false;
			}
		}

		protected void Log(string text, bool newLine = true)
		{
			Output += text;

			if (newLine)
			{
				Output += Environment.NewLine;
			}

			OutputInfo.RefreshBinding();
		}

		static string CalculateCheckSum(string zipFile)
		{
			using (var sha = SHA512.Create())
			{
				using (var stream = new FileStream(zipFile, FileMode.Open))
				{
					var hash = sha.ComputeHash(stream);
					return Convert.ToBase64String(hash);
				}
			}
		}

		static bool ExtractZip(string zipFile, string destinationPath)
		{
			try
			{
				using (var zipArchive = new ZipArchive(new FileStream(zipFile, FileMode.Open, FileAccess.Read), ZipArchiveMode.Read))
				{
					zipArchive.ExtractToDirectory(destinationPath);
					return true;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}

#if !WINZOR
		void CopyFiles(string sourcePath, string targetPath)
		{
			if (!Directory.Exists(sourcePath))
			{
				return;
			}

			var files = new DirectoryInfo(sourcePath).GetFiles();

			foreach (var fileInfo in files)
			{
				var completeTargetPath = Path.Combine(targetPath, fileInfo.Name);
				using (var sourceStream = fileInfo.OpenRead())
				using (var targetStream = ZSaveFileDialog.OpenFile(completeTargetPath))
				{
					sourceStream.CopyTo(targetStream);
				}
			}
		}
#endif

		static string GetLicense()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "CargoWise.Main.Startup.Tools.JetBrains.license.txt";

			using (var stream = assembly.GetManifestResourceStream(resourceName))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		CancellationTokenSource timeoutTask;
	}
}
