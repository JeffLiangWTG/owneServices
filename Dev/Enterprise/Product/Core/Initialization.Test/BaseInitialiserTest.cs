using System;
using System.IO;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using NUnit.Framework;

namespace Enterprise.Initialisation
{
	public class BaseInitialiserTest : TestCase
	{
#if !WINZOR
		protected void AssertEnvironmentProperties(string methodName, bool expectedIsUserInteractive, bool expectedIsPostableProcess, Type expectedExceptionReporterType, Type expectedEnvType, Type expectedDbEnvType)
		{
			using var appDomainWrapper = new WTG.AppDomainWrappers.Net.AppDomainWrapper($"AssertEnvironmentProperties_{methodName}");
			var config = new WTG.AppDomainWrappers.Net.ProcessConfig
			{
				NamespacePath = "Enterprise.Initialisation",
				ClassName = nameof(BaseInitialiserTest),
				MethodName = nameof(AssertEnvironmentPropertiesStatic),
				MethodParameters =
				[
						methodName,
						expectedIsUserInteractive.ToString(),
						expectedIsPostableProcess.ToString(),
						expectedExceptionReporterType?.AssemblyQualifiedName,
						expectedEnvType?.AssemblyQualifiedName,
						expectedDbEnvType?.AssemblyQualifiedName
				]
			};

			config.AssemblyFile = Path.Combine(config.BinFolder, "Enterprise.Initialisation.Test.dll");
			var result = appDomainWrapper.RunMethodInProcess(config);
			AssertEquals(string.Empty, result);
		}

		static void AssertEnvironmentPropertiesStatic(
			string methodName,
			string expectedIsUserInteractive,
			string expectedIsPostableProcess,
			string expectedExceptionReporterType,
			string expectedEnvType,
			string expectedDbEnvType)
		{
			switch (methodName)
			{
				case "InitialiseBatchProcessor":
					Initialiser.InitialiseBatchProcessor();
					break;
				case "InitialiseConsoleApp":
					Initialiser.InitialiseConsoleApp();
					break;
				case "InitialiseWinForms":
					Initialiser.InitialiseWinForms();
					break;
				case "InitialiseWebForms":
					Initialiser.InitialiseWebForms();
					break;
				case "InitialiseWeb":
					Initialiser.InitialiseWeb(enableErrorReport: false, null, null);
					break;
				case "InitialiseServiceManagerPooledConnection":
					Initialiser.InitialiseServiceManager(null, usePooledConnection: true);
					break;
				case "InitialiseServiceManager":
					Initialiser.InitialiseServiceManager(null, usePooledConnection: false);
					break;
				case "Initialise":
					WebInitialiser.Initialise();
					break;
				case "InitialiseErrorReport":
					WebInitialiser.Initialise(enableErrorReport: true);
					break;
				case "InitialiseNoErrorReport":
					WebInitialiser.Initialise(enableErrorReport: false);
					break;
				case "InitialiseErrorReportWithEnvProvider":
					WebInitialiser.Initialise(enableErrorReport: true, new WebEnvironmentProvider());
					break;
				default:
					break;
			}

			AssertEquals(methodName, bool.Parse(expectedIsUserInteractive), Globals.IsUserInteractive);
			AssertEquals(methodName, bool.Parse(expectedIsPostableProcess), ThreadSentry.IsPostableProcess);
			AssertEquals(methodName, expectedExceptionReporterType, ErrorReporter.Instance?.GetType().AssemblyQualifiedName);
			AssertEquals(methodName, expectedEnvType, Env.Instance?.GetType().AssemblyQualifiedName);
			AssertEquals(methodName, expectedDbEnvType, DbEnv.Instance?.GetType().AssemblyQualifiedName);
		}
#endif
	}
}
