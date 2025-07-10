using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.IO;
#if NET
using System.Runtime.InteropServices;
using System.Text.Json;
#endif
using CargoWise.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Moq;
using Moq.Protected;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Shared.Testing
{
	public class LoggerFactoryTests : TestCase
	{
		public void TestLoggerFinalizer_CloseException()
		{
			//Arrange
			var expectException = new OutOfMemoryException();
			var targetMock = new Mock<Target>();

			targetMock
				.Protected()
				.Setup("CloseTarget")
				.Throws(expectException);
			targetMock.Object.Name = "TargetException";

			LogManager.Configuration = new LoggingConfiguration();
			LogManager.Configuration.AddTarget(targetMock.Object);
			LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", targetMock.Object));
			LogManager.ReconfigExistingLoggers();

			var factory = new LoggerFactory();

			//Act
			//Assert
			AssertExceptionThrown<OutOfMemoryException>(() => factory.ShutDownLog());

			targetMock
				.Protected()
				.Verify("FlushAsync", Times.Once(), ItExpr.IsAny<AsyncContinuation>());
			targetMock
				.Protected()
				.Verify("CloseTarget", Times.Once());
		}

		public void TestLoggerManagerShutsDownOnProcessExit()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var assemblyPath = Path.Combine(tempDir.DirectoryName, "test.exe");
				var buildErrors = OutputAssembly(assemblyPath);
				AssertNullOrEmpty(buildErrors);

#if NETFRAMEWORK
				var fileName = assemblyPath; //In .NET 4.8 we genenerate an standalone exe that can run against the system dlls 
				var arguments = $@"{Db.ServerName} {Db.DatabaseName} ""{AssemblyLoader.GetBinPath()}""";
#elif NET
				var fileName = "dotnet"; //In .NET 8.0 the generated exe cannot run without the .NET runtime, when building with Roslyn, as a result we need to run the dotnet runtime and pass in the exe.
				var arguments = $@"""{assemblyPath}"" {Db.ServerName} {Db.DatabaseName} ""{AssemblyLoader.GetBinPath()}""";
#endif

				var psi = new ProcessStartInfo()
				{
					FileName = fileName,
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardInput = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					Arguments = arguments
				};

				using var process = new Process { StartInfo = psi };
				var processOutputLogs = new StringBuilder();
				void DataReceivedHandler(object sender, DataReceivedEventArgs e)
				{
					if (e?.Data != null)
					{
						_ = processOutputLogs.AppendLine(e.Data);
					}
				}

				process.OutputDataReceived += DataReceivedHandler;
				process.ErrorDataReceived += DataReceivedHandler;

				// Act
				_ = process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				process.WaitForExit();

				// Assert
				AssertContains("targetMock FlushAsync", processOutputLogs.ToString());
			}
		}

		static string GetStandAloneSourceCodeForLogging()
		{
#if NET
			const string framework = "NET";
#elif NETFRAMEWORK
			const string framework = "NETFRAMEWORK";
#endif
			const string sourceCode = $$"""
			#define {{framework}} //When compiling with Roslyn the target framework directive is not populated https://github.com/dotnet/roslyn/discussions/57028#discussioncomment-1500743, this is the easiest way of doing it.
			using System;
			using System.IO;
			using System.Reflection;
			using CargoWise.Application;
			using CargoWise.Data;
			using Moq;
			using NLog.Config;
			using NLog.Targets;
			using NLog;
			using Moq.Protected;
			using System.Diagnostics;
			using Enterprise.Integration;
			using Enterprise.ServiceManager.Shared;
			using Enterprise.ServiceManager.Shared.Interfaces;
			using Enterprise.Integration.Licensing;
			using NLog.Common;
			using ServiceManager.Logging.CW;
			using ServiceManager.Shared.Abstractions;
			using ServiceManager.Shared.CW;

			namespace Enterprise.ServiceManager.Shared.Testing
			{
				class Program
				{
					static void Main(string[] args)
					{
						AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveHandler;
						AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionsHandler;

						int exitCode = 0;
						if (args != null && args.Length >= 3)
						{
							serverName = args[0];
							databaseName = args[1];
							binPath = args[2];
			
							Console.WriteLine(string.Format("ServerName: {0} DatabaseName: {1} BinPath: {2}.", serverName, databaseName, binPath));
			
							try
							{
								StartProcess();
							}
							catch (Exception ex)
							{
								Console.WriteLine(string.Format("Exception: {0}", ex));
							}
						}
						else
						{
							Console.WriteLine("No expected command line args received.");
							exitCode = -1;
						}
			
						Console.WriteLine("Exiting");
						System.Environment.Exit(exitCode);
					}
			
					static void StartProcess()
					{
						Db.InitializeDatabaseDetails(serverName, databaseName);
			
						var sharedRegistryMock = new Mock<ISharedRegistrySettings>();
			
						var loggerRegistryMock = sharedRegistryMock.As<ILoggerRegistrySettings>();
						loggerRegistryMock.Setup(x => x.ProcessControllerNLogInternalLoggingEnabled).Returns(true);
						loggerRegistryMock.Setup(x => x.FileSystemLoggingEnabled).Returns(true);
			
						SharedRegistry.Instance = sharedRegistryMock.Object;
			
						var targetMock = new Mock<Target>();
						targetMock.Object.Name = "targetMock";
						targetMock
							.Protected()
							.Setup("FlushAsync", ItExpr.IsAny<AsyncContinuation>())
							.Callback(() =>
							{
								Console.WriteLine("targetMock FlushAsync");
							});
			
						LogManager.Configuration = new LoggingConfiguration();
						LogManager.Configuration.AddTarget(targetMock.Object);
						LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", targetMock.Object));
						LogManager.ReconfigExistingLoggers();
						LogManager.AutoShutdown = false;
			
						using (ObjectFactory.Substitute(Mock.Of<IProductRegistrationKey>(key => key.EnterpriseCode == "ENT" && key.ServerCode == "TST")))
						{
							var dbUpgradeLogger = new LoggerFactory().NewScheduledUpgradeLogger(Db.ServerName, Db.DatabaseName);
							var stackTrace = new StackTrace();
							dbUpgradeLogger.Log(LogType.Information, string.Format("ScheduledUpgradeLog: {0}", stackTrace));
			
							Console.WriteLine(string.Format("Logged stackTrace to UPG log: {0}", stackTrace));
						}
					}
			
			#if NETFRAMEWORK
					static Assembly AssemblyResolveHandler(object sender, ResolveEventArgs args)
					{
						string assemblyPath = Path.Combine(binPath, new AssemblyName(args.Name).Name + ".dll");
						Console.WriteLine(string.Format("Loading assembly: {0}", assemblyPath));
						return Assembly.LoadFrom(assemblyPath);
					}
			#elif NET
					static Assembly AssemblyResolveHandler(object sender, ResolveEventArgs args)
					{
						string assemblyPath = ResolveAssemblyPath(args.Name);
						Console.WriteLine(string.Format("Loading assembly: {0}", assemblyPath));
						return Assembly.LoadFrom(assemblyPath);
					}

					static string ResolveAssemblyPath(string assemblyName)
					{
						var netDirectoryPath = binPath.Contains("net8.0") ? binPath : Path.Combine(binPath, "net8.0");
						var netFilePath = Path.Combine(netDirectoryPath, new AssemblyName(assemblyName).Name + ".dll");
						var frameworkFilePath = Path.Combine(binPath, new AssemblyName(assemblyName).Name + ".dll"); //If the DLL has a .NET 8 dll favor that over the framework dll

						var assemblyPath = File.Exists(netFilePath) ? netFilePath : frameworkFilePath;
						return assemblyPath;
					}
			#endif
			
					static void UnhandledExceptionsHandler(object sender, UnhandledExceptionEventArgs e)
					{
						Console.WriteLine(string.Format("Unhandled exception: {0}", e.ExceptionObject as Exception));
					}
			
					static string serverName;
					static string databaseName;
					static string binPath = "";
				}
			}
			""";

			return sourceCode;
		}

		static string OutputAssembly(string outputPath)
		{
			MetadataReference[] cw1Dlls = new[]
			{
				"CargoWise.ApplicationContext.dll",
				"CargoWise.Data.dll",
				"NLog.dll",
				"Moq.dll",
				"Enterprise.ServiceManager.Shared.dll",
				"ServiceManager.Shared.Abstractions.dll",
				"ServiceManager.Shared.CW.Abstractions.dll",
				"ServiceManager.Shared.CW.dll",
				"ServiceManager.Logging.Abstractions.dll",
				"ServiceManager.Logging.CW.dll",
				"ServiceManager.Integration.ServiceTasks.CW.dll",
				"Enterprise.Integration.dll",
				"Enterprise.Registry.Business.dll",
				"Enterprise.ZArchitecture.Core.dll",
				"CargoWise.EntityFramework.dll",
				"Enterprise.ZArchitecture.Modules.dll",
			}.Select(dll => MetadataReference.CreateFromFile(Path.Combine(AssemblyLoader.GetBinPath(), dll))).ToArray();

			var systemDllLocation = Path.GetDirectoryName(typeof(object).Assembly.Location);

			var systemDlls = new[]
			{
				"mscorlib.dll",
				"System.Runtime.dll",
				"System.Console.dll",
				"System.Core.dll",
				"System.Linq.Expressions.dll",
			}.Select(dll => MetadataReference.CreateFromFile(Path.Combine(systemDllLocation, dll))).ToArray();

			MetadataReference runtimeDll = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

			var compilation = CSharpCompilation
				.Create
				(
					assemblyName: "Test",
					syntaxTrees: new[]
					{
						CSharpSyntaxTree.ParseText(GetStandAloneSourceCodeForLogging()),
					},
					references:
					[
						.. systemDlls,
						.. cw1Dlls,
						runtimeDll
					]
				)
				.WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication));

			var result = compilation.Emit(outputPath);

#if NET
			GenerateRuntimeConfig(outputPath);
#endif

			return result.Success ? string.Empty : string.Join(System.Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
		}

#if NET
		static readonly JsonSerializerOptions jsonOptions = new()
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		static void GenerateRuntimeConfig(string dllPath)
		{
			File.WriteAllText(Path.ChangeExtension(dllPath, "runtimeconfig.json"), GenerateRuntimeConfig());
		}

		static string GenerateRuntimeConfig()
		{
			var runtimeConfig = new RuntimeConfig()
			{
				RuntimeOptions = new()
				{
					Tfm = $"net{RuntimeInformation.FrameworkDescription.Split(" ")[1][..3]}",
					Framework = new()
					{
						Name = "Microsoft.NETCore.App",
						Version = RuntimeInformation.FrameworkDescription.Replace(".NET ", "")
					}
				}
			};
			return JsonSerializer.Serialize(runtimeConfig, jsonOptions);
		}
#endif
	}

#if NET
	class RuntimeConfig
	{
		public RuntimeOptions RuntimeOptions { get; set; } = new();
	}

	class RuntimeOptions
	{
		public string Tfm { get; set; } = string.Empty;
		public Framework Framework { get; set; } = new();
	}

	class Framework
	{
		public string Name { get; set; } = string.Empty;
		public string Version { get; set; } = string.Empty;
	}
#endif
}

