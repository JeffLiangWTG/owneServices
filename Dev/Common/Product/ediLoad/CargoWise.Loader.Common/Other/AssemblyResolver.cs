using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using Microsoft.Win32;

namespace CargoWise.Loader.Common
{
	public static class AssemblyResolver
	{
		public static void Initialize()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			Assembly result = null;
			string assemblyName = new AssemblyName(args.Name).Name;

			//This program must run as a self-contained .exe. Therefore any assemblies it depends on must be added
			//as embedded resources to CargoWise.Loader.Common
			string[] assemblies = new[]
			{
				"System.Resources.Extensions",
				"System.Buffers",
				"System.Memory",
				"System.Runtime.CompilerServices.Unsafe",
				"System.Numerics.Vectors",
			};

			if (assemblyName == ApplicationManagerCommonAssemblyName)
			{
				result = LoadApplicationManagerCommon();
			}
			else if (assemblies.Contains(assemblyName))
			{
				result = LoadFromResource(assemblyName);
			}
			else if (assemblyName == URLHandlerIntegrationAssemblyName)
			{
				result = LoadURLHandlerIntegrationAssembly();
			}
			return result;
		}

		static Assembly LoadApplicationManagerCommon()
			=> LoadApplicationManagerFromServicePath()
				?? LoadApplicationManagerFromProgramDataPath();

		static Assembly LoadApplicationManagerFromServicePath()
		{
			Assembly result = null;
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(ApplicationManagerServiceRegistryKeyName, false))
			{
				if (registryKey != null)
				{
					string imagePath = (string)registryKey.GetValue("ImagePath");
					if (imagePath != null)
					{
						imagePath = imagePath.Trim('"');
						var dllPath = Path.Combine(Path.GetDirectoryName(imagePath), ApplicationManagerCommonAssemblyName + ".dll");
						var version = FileVersionInfo.GetVersionInfo(dllPath).FileVersion;
						if (File.Exists(dllPath) && new Version(version) >= MinimumCompatibleApplicationManagerVersion)
						{
							result = Assembly.LoadFrom(dllPath);
						}
					}
				}
			}
			return result;
		}

		static Version MinimumCompatibleApplicationManagerVersion
		{
			get { return new Version(15, 11, 18, 90); }
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Path")]
		static Assembly LoadApplicationManagerFromProgramDataPath()
		{
			string targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WiseTech Global", Process.GetCurrentProcess().Id.ToString());
			return LoadFromResource(ApplicationManagerCommonAssemblyName, targetDirectory);
		}

		[SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods")]
		internal static Assembly LoadURLHandlerIntegrationAssembly()
		{
			return LoadFromResource(URLHandlerIntegrationAssemblyName)
				?? Directory.EnumerateFiles(
						Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
						URLHandlerIntegrationAssemblyName + ".dll",
						SearchOption.AllDirectories)
					.Where(file => !string.IsNullOrEmpty(file))
					.Select(file => Assembly.LoadFrom(file))
					.FirstOrDefault();
		}

		static Assembly LoadFromResource(string assemblyName)
		{
			return LoadFromResource(Assembly.GetExecutingAssembly(), assemblyName);
		}

		internal static Assembly LoadFromResource(Assembly containerAssembly, string assemblyName)
		{
			using (var resourceStream = containerAssembly.GetManifestResourceStream("CargoWise.Loader.Common.Resources." + assemblyName + ".dll"))
			{
				if (resourceStream == null)
				{
					return null;
				}

				using (var memoryStream = new MemoryStream())
				{
					resourceStream.CopyTo(memoryStream);
					return Assembly.Load(memoryStream.ToArray());
				}
			}
		}

		static Assembly LoadFromResource(string assemblyName, string targetDirectory)
		{
			Argument.NotNullOrEmpty(targetDirectory, nameof(targetDirectory));
			var path = Path.Combine(targetDirectory, assemblyName + ".dll");
			if (!File.Exists(path))
			{
				string directoryName = Path.GetDirectoryName(path);
				Directory.CreateDirectory(directoryName);
				using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.Loader.Common.Resources." + assemblyName + ".dll"))
				using (var fileStream = File.Create(path))
				{
					resourceStream.CopyTo(fileStream);
				}
			}

			return Assembly.LoadFrom(path);
		}

		public static bool IsApplicationManagerInstalled()
		{
			using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(ApplicationManagerServiceRegistryKeyName, false))
			{
				return registryKey != null;
			}
		}

		const string ApplicationManagerServiceRegistryKeyName = @"System\CurrentControlSet\Services\ediAppMgr";
		internal const string ApplicationManagerCommonAssemblyName = "CargoWise.ApplicationManager.Common";
		internal const string URLHandlerIntegrationAssemblyName = "Enterprise.URLHandler.Integration";
	}
}
