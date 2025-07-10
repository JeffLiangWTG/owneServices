using System.Diagnostics;
using System.IO;
using CargoWise.BuildTools;
using Enterprise.ZArchitecture.Environment;
using WTG.DevTools.Common;

namespace CargoWise.Bi.Development.Common
{
	public static class BiSolutionCompiler
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal tool only")]
		public static void CompileRegistrationProject()
		{
			var solutionPath = BuildConstants.GetLocalPath(@"BusinessIntelligence/BiIntegration/Deployment/CargoWiseBiDeployment/CargoWiseBiDeployment.sln");
			var solutionName = "CargoWiseBiDeployment";
			var projectName = "Registration";

			CompileProject(solutionPath, solutionName, projectName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal tool only")]
		static void CompileProject(string solutionPath, string solutionName, string projectName)
		{
			if (!Globals.IsTest)
			{
				var outputLogName = BuildConstants.GetLocalPath($"BuildLog.{solutionName}.{projectName}.Resource.txt");

				BiLogger.StartTask($"Compile {solutionName}.{projectName} Project - Start");

				if (!InvokeBuildProject(solutionPath, projectName, "DEBUG", outputLogName))
				{
					OpenOutputLogFileIfExists(outputLogName);
					BiLogger.Fail("ERROR compiling " + outputLogName);
				}
				else
				{
					BiLogger.Complete($"Compile {solutionName}.{projectName} Solution - Complete");
				}
			}
		}

		static bool InvokeBuildProject(string solutionPath, string projectName, string buildMode, string outputLogName)
		{
			return SolutionCompiler.Create(VisualStudioVersion.VisualStudio2022).BuildProjectAsync(solutionPath, projectName, buildMode, outputLogName).Result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		static void OpenOutputLogFileIfExists(string outputLogFullPath)
		{
			if (File.Exists(outputLogFullPath))
			{
				Process.Start(outputLogFullPath);
			}
		}
	}
}
