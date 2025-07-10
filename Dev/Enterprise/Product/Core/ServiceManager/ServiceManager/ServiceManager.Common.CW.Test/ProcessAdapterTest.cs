using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
#if NET
using System.Runtime.InteropServices;
using System.Text.Json;
#endif
using CargoWise.Common;
using CargoWise.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ServiceManager.Common;

namespace Enterprise.ServiceManager.Shared.Testing
{
	class ProcessAdapterTest : TestCase
	{
		const string AppName = "crasha.exe";

		public void TestStartWithPriority()
		{
			CombineAssertions(() =>
			{
				using var tempDir = new TempDirectory();
				var exePath = GenerateTestAssembly(tempDir.DirectoryName);

				Enum
					.GetValues(typeof(ProcessPriorityClass))
					.Cast<ProcessPriorityClass>()
					.ForEach(x => TestStartWithPriority(x, exePath));
			});

			static void TestStartWithPriority(ProcessPriorityClass priority, string exePath)
			{
				// Arrange
				var process = new Process()
				{
					StartInfo = new ProcessStartInfo(exePath),
				};
				var wrapper = new ProcessAdapter(process, priority);

				using (new DisposableAction(() =>
				{
					if (!wrapper.WaitForExit(15 * 1000))
					{
						wrapper.Kill();
					}
				}))
				{
					// Act
					wrapper.Start();
					var result = wrapper.Priority;

					// Assert
					AssertEquals(result, priority);
				}
			}

			static string GenerateTestAssembly(string dirPath)
			{
				const string assemblyCode = @"
using System;
using System.Threading;

namespace HelloWorld
{
	class HelloWorldClass
	{
		[STAThread]
		static int Main(string[] args)
		{
			Thread.Sleep(5 * 1000);
			return 0;
		}
	}
}
";

				return GenerateAssembly(dirPath, assemblyCode);
			}
		}

		[DeveloperOnlyTest]
		public void TestDoesNotChangePriorityIfTheSame()
		{
			using TempDirectoryManager tempDirectoryManager = new();
			var exePath = GenerateTestAssembly(tempDirectoryManager.GetTempDir().DirectoryName);

			using var process = new Process
			{
#if NET
				StartInfo = new ProcessStartInfo("dotnet", exePath)
#elif NETFRAMEWORK
				StartInfo = new ProcessStartInfo(exePath)
#endif
				{
					Verb = "runas",
#if NET
					UseShellExecute = true,
#endif
				},
			};
			using var processAdapter = new ProcessAdapter(process, Process.GetCurrentProcess().PriorityClass);

			using (new DisposableAction(() =>
			{
				if (!processAdapter.WaitForExit(15 * 1000))
				{
					processAdapter.Kill();
				}
			}))
			{
				// Act
				var result = processAdapter.Start();

				// Assert
				AssertEquals(true, result);
			}
		}

		string GenerateTestAssembly(string dirPath)
		{
			const string assemblyCode = @"
using System;

namespace HelloWorld
{
	class HelloWorldClass
	{
		[STAThread]
		static int Main(string[] args)
		{
			return 0;
		}
	}
}
";
			return GenerateAssembly(dirPath, assemblyCode);
		}

		static string GenerateAssembly(string dirPath, string sourceFile)
		{
			var filePath = Path.Combine(dirPath, AppName);

			var compilation = CSharpCompilation
				.Create
				(
					assemblyName: AppName,
					syntaxTrees: new[]
					{
						CSharpSyntaxTree.ParseText(sourceFile),
					},
					references: new[]
					{
						MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
					}
				)
				.WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication));

			var result = compilation.Emit(filePath);
#if NET
			File.WriteAllText(Path.ChangeExtension(filePath, "runtimeconfig.json"), GenerateRuntimeConfig());
#endif

			return filePath;
		}

#if NET
		static readonly JsonSerializerOptions jsonOptions = new ()
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		static string GenerateRuntimeConfig()
		{
			var runtimeConfig = new RuntimeConfig()
			{
				RuntimeOptions = new ()
				{
					Tfm = $"net{RuntimeInformation.FrameworkDescription.Split(" ")[1][..3]}",
					Framework = new ()
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
		public Framework Framework { get; set; } = new ();
	}

	class Framework
	{
		public string Name { get; set; } = string.Empty;
		public string Version { get; set; } = string.Empty;
	}
#endif

	class TempDirectoryManager : IDisposable
	{
		readonly TempDirectory tempDir;
		internal TempDirectoryManager()
		{
			tempDir = new TempDirectory(Temp.GetNewTempSubdirectory());
		}

		internal TempDirectory GetTempDir()
		{
			return tempDir;
		}

		static void Delete(string path, bool recursive, bool force)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException($"{nameof(path)} cannot be null or white space. ");
			}

			if (force)
			{
				EnsureDirectoryNotReadOnly(path);
				if (recursive)
				{
					var directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
					for (var i = 0; i < directories.Length; i++)
					{
						EnsureDirectoryNotReadOnly(directories[i]);
					}

					directories = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
					for (var i = 0; i < directories.Length; i++)
					{
						var fileInfo = new FileInfo(directories[i]);
						if (fileInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
						{
							fileInfo.Attributes &= ~FileAttributes.ReadOnly;
						}
					}
				}
			}

			Directory.Delete(path, recursive);

			static void EnsureDirectoryNotReadOnly(string path)
			{
				var directoryInfo = new DirectoryInfo(path);
				if (directoryInfo.Attributes.HasFlag(FileAttributes.ReadOnly))
				{
					directoryInfo.Attributes &= ~FileAttributes.ReadOnly;
				}
			}
		}

		public void Dispose()
		{
			Delete(tempDir.DirectoryName, recursive: true, force: true);
		}
	}
}
