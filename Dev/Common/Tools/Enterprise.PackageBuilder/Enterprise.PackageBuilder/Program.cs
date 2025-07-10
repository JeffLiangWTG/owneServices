using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CargoWise.BuildTools;
using Enterprise.Client.EDI.AutoDeploy;
using Enterprise.Initialisation;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.PackageBuilder
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				var options = new Arguments(args);

				if (options.IsDevBuild)
				{
					PrepareReleaseBuildFromDev(options);
				}

				Initialiser.InitialiseWinForms();

				if (options.IsDvd)
				{
					Log(options, "Building generic DVD image, no longer contains a package");
					BuildDvd(options);
				}
				else
				{
					Log(options, "Building package");
					var packageBuilder = new RuntimePackageBuilder(options.BuildPath, options.TargetPath);
					if (options.IsBuildMaster)
					{
						packageBuilder.BuildMaster();
					}
					else
					{
						packageBuilder.Build(options.EnterpriseCode, true);
					}

					Log(options, "");
					Log(options, "Package built: {0}", packageBuilder.LastPackagePath);
					Log(options, "");
				}
			}
			catch (Exception e)
			{
				Console.Error.Write(new ExceptionDetails(e).GetStackTraceAndMessage());
				Console.Error.WriteLine();
			}
		}

		static void PrepareReleaseBuildFromDev(Arguments options)
		{
			options.BuildPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string localSourceDirectory = Path.GetDirectoryName(options.BuildPath);
			ImportableBuild.Prepare(localSourceDirectory, options.BuildPath);
		}

		static void BuildDvd(Arguments options)
		{
			string enterpriseExeFile = Path.Combine(options.BuildPath, ExeFileNames.CargoWiseOneExeForVersionInfo);
			var versionInfo = FileVersionInfo.GetVersionInfo(enterpriseExeFile);
			var dateTimePart = new VersionNumber(versionInfo).GetReleaseDate().ToString("yyyyMMdd_HHmm00_");
			var dvdName = "DVD" + dateTimePart + versionInfo.FileVersion.Replace('.', '_');
			string dvdTargetPath = Path.Combine(options.TargetPath, dvdName);

			if (Directory.Exists(dvdTargetPath))
			{
				Directory.Delete(dvdTargetPath, true);
			}
			Log(options, "Copying dvd template files");
			CopyDirectory(options.DvdTemplate, dvdTargetPath);

			Log(options, "Copying release files");
			File.Copy(Path.Combine(options.BuildPath, "CargoWiseServerSetup.exe"), Path.Combine(dvdTargetPath, "CargoWiseServerSetup.exe"));
			File.Copy(Path.Combine(options.BuildPath, "CargoWiseServerSetup.exe.config"), Path.Combine(dvdTargetPath, "CargoWiseServerSetup.exe.config"));
			File.Copy(Path.Combine(options.BuildPath, "CargoWise.Start.exe"), Path.Combine(dvdTargetPath, "CargoWise.Start.exe"));
			File.Copy(Path.Combine(options.BuildPath, "CargoWise.Start.exe.config"), Path.Combine(dvdTargetPath, "CargoWise.Start.exe.config"));
			File.Copy(Path.Combine(options.BuildPath, "CargoWiseOne.Start.exe"), Path.Combine(dvdTargetPath, "CargoWiseOne.Start.exe"));
			File.Copy(Path.Combine(options.BuildPath, "CargoWiseOne.Start.exe.config"), Path.Combine(dvdTargetPath, "CargoWiseOne.Start.exe.config"));
			File.Copy(Path.Combine(options.BuildPath, "applog.json"), Path.Combine(dvdTargetPath, "applog.json"));

			if (!string.IsNullOrEmpty(options.UseDbBackups))
			{
				Log(options, "Copying database backup files");
				CopyDirectory(options.UseDbBackups, Path.Combine(dvdTargetPath, @"Install\Databases"));
			}

			Log(options, "");
			Log(options, "Dvd built: {0}", dvdTargetPath);
			Log(options, "");
		}

		static void CopyDirectory(string sourceFolder, string targetFolder)
		{
			if (Directory.Exists(sourceFolder))
			{
				if (!Directory.Exists(targetFolder))
				{
					Directory.CreateDirectory(targetFolder);
				}

				foreach (string subSourceFolder in Directory.GetDirectories(sourceFolder))
				{
					string subSourceFolderName = Path.GetFileName(subSourceFolder);
					string subTargetFolder = Path.Combine(targetFolder, subSourceFolderName);
					CopyDirectory(subSourceFolder, subTargetFolder);
				}

				string[] sourceFiles = Directory.GetFiles(sourceFolder);
				foreach (string sourceFile in sourceFiles)
				{
					string fileName = Path.GetFileName(sourceFile);
					string targetFile = Path.Combine(targetFolder, fileName);
					File.Copy(sourceFile, targetFile, true);
				}
			}
			else
			{
				throw new ArgumentException(sourceFolder + " does not exist");
			}
		}

		static void Log(Arguments options, string message, params object[] messageParams)
		{
			Console.WriteLine(string.Format(message, messageParams));
		}
	}
}
