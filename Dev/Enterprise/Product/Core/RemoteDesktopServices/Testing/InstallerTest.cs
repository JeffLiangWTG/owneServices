using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.BuildTools;
using CargoWise.IO;
using NUnit.Framework;
using WTG.DevTools.Common;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class InstallerTest : TestCase
	{
		[TestRequiresAdministrativePrivileges("Installer app")]
		public void TestSetupFilesAreReleaseSigned()
		{
			var tempDir = Temp.GetNewTempSubdirectory();
			try
			{
				var process = Process.Start(new ProcessStartInfo(SetupFile, "/extract:\"" + tempDir + "\"") { UseShellExecute = false });
				process.WaitForExit(30000);
				AssertAllFilesAreSigned(tempDir);
			}
			finally
			{
				Directory.Delete(tempDir, true);
			}
		}

		void AssertAllFilesAreSigned(string directory)
		{
			foreach (string file in Directory.GetFiles(directory))
			{
				try
				{
					var result = new AuthenticodeVerification().RunAsync(file, TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
					AssertEquals(Path.GetFileName(file) + " must be authenticode signed. " + InstallerFileName + " must be created with binaries built from QGL -Release -DAT. " + result.ErrorMessage, SignToolResultType.Success, result.Result);
				}
				catch (BaseSourcePathNotFoundException)
				{
					// SignTool.exe not found on this computer
					Assert(true);
					return;
				}
			}
		}

		public void TestVersionConsistency()
		{
			var setupFileVersionInfo = FileVersionInfo.GetVersionInfo(SetupFile);
			AssertEquals("ClientVersion.Version must match the version of " + InstallerFileName, new Version(setupFileVersionInfo.ProductVersion), InstallerVersion);
			if (!string.IsNullOrEmpty(SharedXmlSerializers))
			{
				var xmlSerializersVersion = Assembly.LoadFile(SharedXmlSerializers).GetName().Version;
				xmlSerializersVersion = new Version(xmlSerializersVersion.Major, xmlSerializersVersion.Minor,
					xmlSerializersVersion.Build);
				AssertEquals(
					"Assembly version of Enterprise.RemoteDesktopServices.Shared and Enterprise.RemoteDesktopServices.Shared.XmlSerializers must match the version of " +
					InstallerFileName + ", because it must be installed to the GAC", InstallerVersion, xmlSerializersVersion);
			}
		}

		protected virtual Version InstallerVersion
		{
			get { return ClientVersion.Version; }
		}

		string SetupFile
		{
			get { return Path.Combine(BinPath, InstallerFileName); }
		}

		protected virtual string SharedXmlSerializers
		{
			get { return Path.Combine(BinPath, "Enterprise.RemoteDesktopServices.Shared.XmlSerializers.dll"); }
		}

		string BinPath
		{
			get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
		}

		protected virtual string InstallerFileName
		{
			get { return ClientVersion.InstallerExeName; }
		}
	}

	class CitrixInstallerTest : InstallerTest
	{
		protected override string SharedXmlSerializers { get; }

		protected override string InstallerFileName
		{
			get { return ClientCitrixVersion.InstallerExeName; }
		}

		protected override Version InstallerVersion
		{
			get { return ClientCitrixVersion.Version; }
		}
	}
}
