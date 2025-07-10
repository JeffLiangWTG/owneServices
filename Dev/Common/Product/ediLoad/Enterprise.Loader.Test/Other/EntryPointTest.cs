using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using AppDomainWrappers.Net;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	class EntryPointTest : TestCase
	{
		#region TestAppManagerTypesCanBeLoaded
		public void TestAppManagerTypesCanBeLoaded()
		{
			var appDomainWrapper = new AppDomainWrapper("TestAppManagerTypesCanBeLoaded");
			var config = new ProcessConfig
			{
				NamespacePath = "Enterprise.Loader.Testing",
				ClassName = nameof(EntryPointTest),
				MethodName = nameof(TestAppManagerTypesCanBeLoadedStatic),
			};

			config.AssemblyFile = Path.Combine(config.BinFolder, "CargoWise.Start.Unmerged.Test.dll");

			var result = appDomainWrapper.RunMethodInProcess48(config);
			AssertEquals(string.Empty, result);
		}

		static void TestAppManagerTypesCanBeLoadedStatic()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var assembly = Assembly.LoadFrom(Path.Combine(binPath, "CargoWise.Start.exe"));
			var type = assembly.GetType("Enterprise.Upgrades.InstallationDirectoryCopier");
			AssertNotNull(type);
			AssertEquals("Enterprise.Upgrades.InstallationDirectoryCopier", type.FullName);
			AssertEquals("CargoWise.ApplicationManager.Common.IAppManagerInvocable, CargoWise.ApplicationManager.Common, Version=2.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350", type.GetInterface("CargoWise.ApplicationManager.Common.IAppManagerInvocable").AssemblyQualifiedName);
		}
		#endregion TestAppManagerTypesCanBeLoaded

		[GuiTest]
		public void TestRunAlone()
		{
			AssertRunAlone(System.Environment.MachineName + " OdysseyDat -SelectCurrentVersion -RunFromThisLocation", "Installation Results");
		}

		[GuiTest]
		[RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestRunAloneWithAdminConnection()
		{
			Application.ConfigureApplicationServices();
			var helper = new EnterpriseDatabaseHelper(new EnterpriseConfiguration());
			helper.OpenConnection(true);
			string databaseName = "cw1start_" + Guid.NewGuid().ToString();
			using (var cmd = new SqlCommand($"CREATE DATABASE [{databaseName}]", helper.Connection))
			{
				cmd.ExecuteNonQuery();
			}
			using (var cmd = new SqlCommand($@"
CREATE TABLE[{databaseName}].dbo.StmUpgrade(
	[SZ_PK][uniqueidentifier] NOT NULL,
	[SZ_Type][varchar](3) NOT NULL,
	[SZ_ExeVersionDate][smalldatetime] NULL,
	[SZ_MajorVersion][int] NOT NULL,
	[SZ_MinorVersion][int] NOT NULL,
	[SZ_Release][int] NOT NULL,
	[SZ_Patch][int] NOT NULL,
	[SZ_MajorDataVersion][int] NOT NULL,
	[SZ_MinorDataVersion][int] NOT NULL,
	[SZ_WorkingVersion][char](1) NOT NULL,
	[SZ_UpgradeNotes][varchar](max) NOT NULL,
	[SZ_UpgradeData_Compressed][varbinary](max) NULL,
	[SZ_SplitCount][smallint] NOT NULL,
	[SZ_SplitLastItem][char](1) NOT NULL,
	[SZ_Reference][varchar](128) NOT NULL,
	[SZ_Cancelled][char](1) NOT NULL,
	[SZ_CancelledReason][varchar](128) NOT NULL,
	[SZ_Status][varchar](3) NOT NULL,
	[SZ_StatusTime][smalldatetime] NULL,
	[SZ_StatusComment][varchar](128) NOT NULL)", helper.Connection))
			{
				cmd.ExecuteNonQuery();
			}
			using (var cmd = new SqlCommand($@"INSERT [{databaseName}].dbo.StmUpgrade
	Values(NewID(), 'EDP', '2022-12-22 18:06:00', 50, 1, 1, 1, 50, 1, 1, 1, NULL, 0, 0, '', 0, '', 'RDY', '2022-12-22 18:06:00', ' ')", helper.Connection))
			{
				cmd.ExecuteNonQuery();
			}
			try
			{
				AssertRunAlone(System.Environment.MachineName + " " + databaseName + " -SelectCurrentVersion -RunFromThisLocation", "Select Software Version");
			}
			finally
			{
				using (var cmd = new SqlCommand($"DROP DATABASE [{databaseName}]", helper.Connection))
				{
					cmd.ExecuteNonQuery();
				}
				helper.Connection.Dispose();
			}
		}

		[GuiTest]
		public void TestRunAloneWithResultsDialog()
		{
			AssertRunAlone("-Instance:" + Guid.NewGuid().ToString() + " -RunFromThisLocation", "Installation Results");
		}

		[GuiTest]
		public void TestRunAloneWithUninstall()
		{
			var tempDirectory = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			var versionFolder = Path.Combine(tempDirectory, "22.1.1.1");
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var targetPath = Path.Combine(tempDirectory, "CargoWise.Start.exe");
			try
			{
				Directory.CreateDirectory(tempDirectory);
				Directory.CreateDirectory(versionFolder);
				File.Copy(Path.Combine(binPath, "CargoWise.Start.exe"), Path.Combine(targetPath));
				File.Copy(Path.Combine(binPath, "CargoWise.ApplicationManager.Common.dll"), Path.Combine(tempDirectory, "CargoWise.ApplicationManager.Common.dll"));
				AssertEquals(true, Directory.Exists(versionFolder));

				var processStartInfo = new ProcessStartInfo(targetPath, $" -Uninstall -path={tempDirectory} -NoUI")
				{
					RedirectStandardError = true,
					UseShellExecute = false
				};

				var process = Process.Start(processStartInfo);
				process.WaitForExit();

				AssertEquals(process.StandardError.ReadToEnd(), 0, process.ExitCode);
				AssertEquals(false, Directory.Exists(versionFolder));
			}
			finally
			{
				Directory.Delete(tempDirectory, true);
			}
		}

		void AssertRunAlone(string commandLineOptions, string windowTitle)
		{
			var tempDirectory = Path.Combine(TempForTest.TempPath, Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempDirectory);
			try
			{
				var targetPath = Path.Combine(tempDirectory, "CargoWise.Start.exe");
				string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				File.Copy(Path.Combine(binPath, "CargoWise.Start.exe"), targetPath);
				var process = Process.Start(targetPath, commandLineOptions);
				try
				{
					var stopWatch = new Stopwatch();
					stopWatch.Start();
					do
					{
						AssertEquals(false, process.WaitForExit(1000));
						process.Refresh();
					}
					while (process.MainWindowTitle != windowTitle && stopWatch.Elapsed < TimeSpan.FromMinutes(1));
					process.Refresh();
					string screenMessage = string.Empty;
					if (windowTitle != process.MainWindowTitle)
					{
						screenMessage = "Screenshot of window has been saved here: " + CaptureScreenshot(process);
					}
					AssertEquals(screenMessage, windowTitle, process.MainWindowTitle);
				}
				finally
				{
					if (!process.HasExited)
					{
						process.Kill();
						process.WaitForExit();
					}
				}
			}
			finally
			{
				Directory.Delete(tempDirectory, true);
			}
		}

		public string CaptureScreenshot(Process process)
		{
			var rect = new User32.RECT();
			User32.GetWindowRect(process.MainWindowHandle, ref rect);

			int width = rect.right - rect.left;
			int height = rect.bottom - rect.top;

			var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			Graphics graphics = Graphics.FromImage(bmp);
			graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

			var file = Path.Combine(GetTestFilesPath(), Guid.NewGuid().ToString() + ".bmp");
			bmp.Save(file, ImageFormat.Png);
			return file;
		}

		class User32
		{
			[StructLayout(LayoutKind.Sequential)]
			public struct RECT
			{
				public int left;
				public int top;
				public int right;
				public int bottom;
			}

			[DllImport("user32.dll")]
			internal static extern int GetWindowRect(IntPtr hWnd, ref RECT rect);
		}
	}
}
