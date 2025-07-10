using System;
using System.Diagnostics;
using System.IO;
using CargoWise.Bi.Development.Automation;
using CargoWise.Bi.Development.Common;
using CargoWise.BuildTools;
using CargoWise.Common;

namespace Enterprise.Builder.GenerateDbUpgraderResources
{
	sealed class RegenEngine
	{
		public bool DoSetup(ILogger logger, RegenFlags regenFlags, string cwShared, out string errorMsg)
		{
			errorMsg = null;
			try
			{
				ReinitialiseSetupController(logger);

				SetupDbUpgrader(logger, regenFlags);

				if (regenFlags.HasFlag(RegenFlags.Compile))
				{
					CompileReferencedSolutions(logger, cwShared);
				}

				SynchroniseBiConfiguration(logger, cwShared);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				errorMsg = e.Message;
				logger.AddReportHeader(e.Message);
				return false;
			}
			return true;
		}

		public bool DoUndoCheckOut(ILogger logger)
		{
			logger.AddReportHeader("Undo check out - Start");

			if (setupUpg.UndoCheckOutUpgraderFiles())
			{
				logger.AddReportHeader("Undo check out - Complete");
			}
			else
			{
				logger.AddReportHeader("ERROR undoing check out");
				return false;
			}

			return true;
		}

		void ReinitialiseSetupController(ILogger logger)
		{
			setupUpg = new SetupController();
			setupUpg.OnTaskStarted += new SetupUpgraderEvent(logger.AddReportLine);
			setupUpg.OnTaskFailed += new SetupUpgraderEvent(logger.AddReportLine);
		}

		void SetupDbUpgrader(ILogger logger, RegenFlags regenFlags)
		{
			logger.AddReportHeader("Setup DbUpgrader - Start");

			if (!setupUpg.RunSetup(regenFlags.HasFlag(RegenFlags.MinorVersion), regenFlags.HasFlag(RegenFlags.BumpVersionOnly), regenFlags.HasFlag(RegenFlags.Merge)))
			{
				throw new RegenException("ERROR setting up DbUpgrader");
			}

			logger.AddReportHeader("Setup DbUpgrader - Complete");
		}

		void CompileReferencedSolutions(ILogger logger, string cwShared)
		{
			BuildWithDotNet(
				logger,
				Path.Combine(cwShared, @"CargoWise.DbUpgrader\src\Resource\DbUpgrader.Resource.sln"));

			File.Copy(
				Path.Combine(cwShared, @"CargoWise.DbUpgrader\Bin\netstandard2.0\Enterprise.DbUpgrader.Resource.dll"),
				BuildConstants.GetLocalPath(@"Bin\Enterprise.DbUpgrader.Resource.dll"),
				overwrite: true);

			BuildWithDotNet(logger, BuildConstants.GetLocalPath(@"Enterprise\Product\Core\Database\Build.Database.sln"));

			BuildWithDotNet(logger, BuildConstants.GetLocalPath(@"Database\BusinessIntelligence\ConfigLoader\ConfigLoader.sln"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Starting a dotnet process not to open a file or url")]
		static void BuildWithDotNet(ILogger logger, string solution)
		{
			logger.AddReportHeader("Compile " + solution + " Solution - Start");

			if (!File.Exists(solution))
			{
				throw new RegenException($"Building of {solution} the solution could not be found.");
			}

			try
			{
				var startInfo = new ProcessStartInfo("dotnet", "build \"" + solution + "\"");
				startInfo.RedirectStandardError = true;
				startInfo.RedirectStandardOutput = true;
				startInfo.UseShellExecute = false;
				startInfo.WorkingDirectory = Path.GetDirectoryName(solution);

				using (var process = Process.Start(startInfo))
				{
					DataReceivedEventHandler handler = Log;
					process.ErrorDataReceived += handler;
					process.OutputDataReceived += handler;
					process.BeginErrorReadLine();
					process.BeginOutputReadLine();
					process.WaitForExit();

					if (process.ExitCode != 0)
					{
						throw new RegenException($"ERROR compiling '{solution}'. (exit code: {process.ExitCode})");
					}
				}
			}
			catch (FileNotFoundException ex)
			{
				throw new RegenException($"Building of {solution} failed as either dotnet could not be found.", ex);
			}

			logger.AddReportHeader($"Compile {solution} Solution - Complete");

			void Log(object sender, DataReceivedEventArgs e) => logger.AddReportLine(e.Data);
		}

		static void SynchroniseBiConfiguration(ILogger logger, string cwShared)
		{
			try
			{
				BiFiles.CWSharedPath = cwShared;
				logger.AddReportHeader("Synchronise BI Configuration file - Start");
				new BiManager(logger.AddReportHeader).SchemaSynchronisation();
				logger.AddReportHeader("Synchronise BI Configuration file - Complete");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.AddReportHeader("ERROR Synchronising BI Configuration file");
				throw;
			}
		}

		SetupController setupUpg;
	}
}
