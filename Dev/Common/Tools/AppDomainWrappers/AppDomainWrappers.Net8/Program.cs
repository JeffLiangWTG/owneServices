using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace AppDomainWrappers.Net8
{
	class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Lazy is thread-safe")]
		internal static readonly Lazy<string[]> separator = new(() => new string[] { "--codeToRun " });

		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: AppDomainWrapper.Net8.exe --config [Path to config file] --codeToRun [ActionCode]");
				return;
			}

			if (!File.Exists(args[1]))
			{
				Console.WriteLine("Config file not found");
				return;
			}

			ProcessConfigClient config;
			try
			{
				config = JsonSerializer.Deserialize<ProcessConfigClient>(File.ReadAllText(args[1]));
			}
			catch (JsonException jsonException)
			{
				Console.WriteLine(jsonException.InnerException.Message ?? jsonException.Message);
				return;
			}

			if (config == null)
			{
				Console.WriteLine("Config failed to deserialize");
				return;
			}

			var heartbeat = HeartbeatSetUp(config);
			try
			{
				LoadAssemblyDependancies(config.AssemblyDependancies, config.BinFolder);

				if (config.RunMode == AppDomainRunMode.Method)
				{
					try
					{
						var asm = Assembly.LoadFile(config.AssemblyFile);
						var type = asm.GetType($"{config.NamespacePath}.{config.ClassName}");
						if (type == null)
						{
							Console.WriteLine("Could not find type");
						}

						var staticMethodInfo = type.GetMethod(config.MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
						if (staticMethodInfo != null)
						{
							_ = staticMethodInfo.Invoke(null, config.MethodParameters);
						}
						else
						{
							var nonStaticMethodInfo = type.GetMethod(config.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							if (nonStaticMethodInfo != null)
							{
								var instance = Activator.CreateInstance(type);
								_ = nonStaticMethodInfo.Invoke(instance, config.MethodParameters);
							}
							else
							{
								Console.WriteLine("Could not find method");
							}
						}
					}
					catch (AggregateException ex)
					{
						Console.WriteLine(HandleAggregateException(ex));
					}
					catch (Exception ex)
					{
						Console.WriteLine(HandleException(ex));
					}
				}
				else if (config.RunMode == AppDomainRunMode.RawCode)
				{
					var argsString = string.Join(" ", args);
					var splitArgs = argsString.Split(separator.Value, StringSplitOptions.None);
					var codeToRun = splitArgs[1];
					var imports = config.UsingImports?.ToArray() ?? Array.Empty<string>();
					var dllFiles = config.AssemblyDependancies?.ToArray() ?? Array.Empty<string>();

					var dataStore = new DataStore();

					void customAction()
					{
						try
						{
							var options = ScriptOptions.Default
								.WithReferences(dllFiles)
								.WithReferences(AppDomain.CurrentDomain.GetAssemblies())
								.WithImports(imports);

							var result = CSharpScript.EvaluateAsync(codeToRun, options, globals: new Globals { DataStore = dataStore }).Result;
						}
						catch (AggregateException ex)
						{
							Console.WriteLine(HandleAggregateException(ex));
						}
						catch (Exception ex)
						{
							Console.WriteLine(HandleException(ex));
						}
					}

					customAction();
				}
				else
				{
					Console.WriteLine("Error: Invalid config");
					return;
				}
			}
			finally
			{
				HeartbeatTearDown(heartbeat);
			}
		}

		static void LoadAssemblyDependancies(List<string> assemblyDependancies, string binFolder)
		{
			if (assemblyDependancies == null)
			{
				AutoAssemblyLoading(binFolder);
			}
			else
			{
				foreach (var dllFile in assemblyDependancies)
				{
					var assemblyPath = Path.Combine(binFolder, dllFile);
					try
					{
						if (File.Exists(assemblyPath))
						{
							_ = Assembly.LoadFile(assemblyPath);
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error loading {dllFile}: {ex.Message}");
					}
				}
			}
		}

		static void AutoAssemblyLoading(string binFolder)
		{
			Globals.BinFolder = Path.GetFullPath(binFolder);
			//resolve any unknown assemblies by looking in the main cargowiseone folder for the assembly.
			AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
			{
				var assemblyName = new AssemblyName(args.Name);
				var path = Path.Combine(Globals.BinFolder, assemblyName.Name + ".dll");
				if (File.Exists(path))
				{
					return Assembly.LoadFile(path);
				}

				return null;
			};
		}

		static string HandleAggregateException(AggregateException ex)
		{
			if (ex?.InnerExceptions?.Count == 1)
			{
				return HandleException(ex?.InnerException);
			}

			var results = new StringBuilder();
			foreach (var innerEx in ex?.InnerExceptions)
			{
				_ = results.AppendLine(GetFailedAssertions(innerEx));
			}

			if (results.Length > 0)
			{
				return results.ToString();
			}

			return ex?.InnerException?.Message ?? ex?.Message;
		}

		static string HandleException(Exception ex)
		{
			var results = GetFailedAssertions(ex);
			if (!string.IsNullOrEmpty(results))
			{
				return results;
			}

			return ex?.InnerException?.Message ?? ex?.Message;
		}

		static string GetFailedAssertions(Exception ex)
		{
			if (ex?.InnerException?.GetType().FullName == "NUnit.Framework.AssertionFailedError")
			{
				if (ex.GetType() == typeof(TargetInvocationException))
				{
					return $"NUnit.Framework.AssertionFailedError:{ex.InnerException.StackTrace}";
				}

				return $"NUnit.Framework.AssertionFailedError:{ex.InnerException.Message}";
			}

			return string.Empty;
		}

		static HeartbeatClient HeartbeatSetUp(ProcessConfigClient processConfig)
		{
			var heartbeat = new HeartbeatClient(processConfig.ClientLockFileName);

			heartbeat.ClientStateChanged += (s, e) => Heartbeat_StateChanged(e, HeartbeatExecutionMode.Client);
			_ = heartbeat.InitialiseMyLockFile(processConfig.TempConfigDirectoryPath);
			_ = heartbeat.DoLockMyLockFile();

			heartbeat.HostStateChanged += (s, e) => Heartbeat_StateChanged(e, HeartbeatExecutionMode.Host);
			_ = heartbeat.DoMonitorLockFile(
					Path.Combine(processConfig.TempConfigDirectoryPath, processConfig.HostLockFileName),
					waitForConnection: false);  // Host should already exist.

			return heartbeat;
		}

		static void HeartbeatTearDown(HeartbeatClient heartbeat)
		{
			_ = heartbeat.DoCancelLockFileMonitor();
			_ = heartbeat.DoCancelMyLockFile();
			heartbeat.HostStateChanged -= (s, e) => Heartbeat_StateChanged(e, HeartbeatExecutionMode.Host);
			heartbeat.ClientStateChanged -= (s, e) => Heartbeat_StateChanged(e, HeartbeatExecutionMode.Client);
		}

		static void Heartbeat_StateChanged(LockFileStateChangedEventArgs e, HeartbeatExecutionMode mode)
		{
			if (e.State == LockFileState.Disconnected
					|| e.State == LockFileState.Failure)
			{
				if (!e.CancellationTokenSource.IsCancellationRequested)
				{
					// Failure
					if (mode == HeartbeatExecutionMode.Host)
					{
					}
					else
					{
					}
				}
			}
		}
	}
}
