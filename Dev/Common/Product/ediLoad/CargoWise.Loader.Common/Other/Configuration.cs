using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CargoWise.ApplicationManager.Common;
using CargoWise.BrandManager;
using CargoWise.Common;
using Microsoft.Win32;

namespace CargoWise.Loader.Common
{
	public class Configuration
	{
		#region Fields

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public const string AutomatedArgument = "-Automated";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public const string CompanyName = "WiseTech Global";
		public const string NoUIArgument = "-NoUI";
		public const string ProgramFilesOverrideEnvironmentVariable = "Enterprise_Program_Files";
		public const string RelaunchedElevatedArgument = "-RelaunchedElevated";
		public const string StartBrandingArgument = "-StartBranding:";
		Notifier notifier;
		IServiceContainer services;
		string targetPath;

		#endregion

		[SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt")]
		public Configuration()
		{
			AllArguments = string.Empty;
			AppManagerDirectoryName = "CargoWise Application Manager";
			TargetDirectoryName = "CargoWise";
		}

		public bool ShowHelp { get; protected internal set; }

		string allArguments;
		public string AllArguments
		{
			get
			{
				return allArguments;
			}
			protected set
			{
				Argument.NotNull(value, nameof(value));
				allArguments = value;
			}
		}

		public virtual string ApplicationName
		{
			get
			{
				return (!String.IsNullOrEmpty(BrandingFactory.Instance.ProductName)) ? BrandingFactory.Instance.ProductName : "Current Application";
			}
		}

		string appManagerDirectoryName;
		public string AppManagerDirectoryName
		{
			get
			{
				return appManagerDirectoryName;
			}
			protected set
			{
				Argument.NotNullOrEmpty(value, nameof(value));
				appManagerDirectoryName = value;
			}
		}

		public Notifier Notifier
		{
			get
			{
				return notifier ?? (notifier = new Notifier(this));
			}
		}

		public static string ProgramFilesOverrideRegistry
		{
			get
			{
				object registryPath = null;
				using (RegistryKey hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
				{
					using (var key = hklm32.OpenSubKey(@"SOFTWARE\WiseTech Global"))
					{
						if (key != null)
						{
							registryPath = key.GetValue("InstallationLocation");
						}
					}
				}

				if (registryPath != null)
				{
					return (string)registryPath;
				}
				return null;
			}
		}

		protected string ProgramFilesPath
		{
			get
			{
				var registryPath = ProgramFilesOverrideRegistry;
				if (!string.IsNullOrEmpty(registryPath))
				{
					return registryPath;
				}
				var environmentPath = Services.Environment.GetEnvironmentVariable(ProgramFilesOverrideEnvironmentVariable);
				if (!string.IsNullOrEmpty(environmentPath))
				{
					return environmentPath;
				}
				return Services.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			}
		}

		public bool RelaunchedElevated { get; private set; }

		public BrandingFactory.BrandingType BrandingType { get; protected set; } = BrandingFactory.BrandingType.CW1LegacyBranding;

		public ReturnCode ReturnCode { get; set; }

		public IServiceContainer Services
		{
			get
			{
				return services ?? ServiceContainer.Instance;
			}
			set
			{
				if (value == null)
				{ throw new ArgumentNullException(nameof(value)); } //Should replaced with Argument.Null
				services = value;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		public virtual string StartupPath
		{
			get
			{
				if (!string.IsNullOrEmpty(StartupPathOverride))
				{
					return StartupPathOverride;
				}
				var assembly = Assembly.GetEntryAssembly();
				return StartupPathFromLocation(assembly.Location);
			}
		}

		internal virtual string StartupPathFromLocation(string location)
			=> Path.GetDirectoryName(location);

		public string StartupPathOverride { get; set; }

		string targetDirectoryName;
		public string TargetDirectoryName
		{
			get
			{
				return targetDirectoryName;
			}
			protected set
			{
				Argument.NotNull(value, nameof(value));
				targetDirectoryName = value;
			}
		}

		public string TargetPath
		{
			get
			{
				return BaseTargetPath;
			}
		}

		public string BaseTargetPath
		{
			get
			{
				return !string.IsNullOrEmpty(targetPath) ? targetPath : (targetPath = GetDefaultTargetPath());
			}
			set { targetPath = value; }
		}

		public virtual string CurrentPackage
		{
			get
			{
				return TargetVersion == null ? BaseTargetPath : Path.Combine(BaseTargetPath, TargetVersion.ToString());
			}
		}

		public virtual string GetInstallPathInCurrentPackage()
		{
			return Path.Combine(CurrentPackage, "Install");
		}

		public virtual string GetApplicationPathInCurrentPackage()
		{
			return CurrentPackage;
		}

		public Version TargetVersion { get; set; }

		public virtual UILevel UILevel { get; protected set; }

		protected static string EscapeArgument(string value)
		{
			string result = value;
			if ((value != null) && value.IndexOf(' ') >= 0)
			{
				if (value.EndsWith("\\"))
				{
					result += '\\';
				}
				result = '\"' + result + '\"';
			}
			return result;
		}

		protected string GetDefaultTargetPath()
		{
			return GetTargetPath(TargetDirectoryName);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public string GetTargetPath(string directoryName)
		{
			Argument.NotNull(directoryName, nameof(directoryName));
			return Path.Combine(Path.Combine(ProgramFilesPath, CompanyName), directoryName); //uses two combines because no contract exists on the three argument combine
		}

		public bool Initialize(string[] args)
		{
			UILevel = Environment.UserInteractive ? UILevel.Normal : UILevel.AutomatedWithNoUI;
			InitializeCore(args);
			return !ShowHelp;
		}

		protected virtual void InitializeCore(string[] args)
		{
			ParseCommandLine(args);
		}

		protected virtual void ParseCommandLineArgument(string arg)
		{
		}

		void ParseCommandLine(string[] value)
		{
			if (value != null)
			{
				List<string> unhandledArgs = new List<string>();
				foreach (string arg in value)
				{
					if (arg == NoUIArgument)
					{
						UILevel = UILevel.AutomatedWithNoUI;
					}
					else if (arg == AutomatedArgument)
					{
						if (UILevel == UILevel.Normal)
						{
							UILevel = UILevel.AutomatedWithUI;
						}
					}
					else if (arg == RelaunchedElevatedArgument)
					{
						RelaunchedElevated = true;
					}
					else if (arg.StartsWith(StartBrandingArgument))
					{
						var type = arg.Split(':')[1];
						if (type == "CWNext")
						{
							BrandingType = BrandingFactory.BrandingType.CargoWiseNext;
						}
					}
					else
					{
						unhandledArgs.Add(arg);
					}
					if (AllArguments.Length > 0)
					{
						AllArguments += ' ';
					}
					AllArguments += EscapeArgument(arg);
				}
				foreach (string arg in unhandledArgs)
				{
					ParseCommandLineArgument(arg);
				}
			}
		}

		#region OSVersion

		public OSVersion OSVersion
		{
			get
			{
				VersionHelper versionHelper = new VersionHelper();
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					if (versionHelper.IsWindowsVistaOrGreater())
					{
						return IsX64 ? OSVersion.WinVistaOr2008Or7x64 : OSVersion.WinVistaOr2008Or7x86;
					}
					else if (versionHelper.IsWindowsXPOrGreater())
					{
						if (versionHelper.IsWindowsServer())
						{
							return IsX64 ? OSVersion.WinServer2003x64 : OSVersion.WinServer2003x86;
						}
						else
						{
							return IsX64 ? OSVersion.WinXPx64 : OSVersion.WinXPx86;
						}
					}
				}

				return OSVersion.Unsupported;
			}
		}

		public bool IsX64
		{
			get
			{
				return IntPtr.Size == 8 || IsWow64ProcessInternal();
			}
		}

		bool IsWow64ProcessInternal()
		{
			bool isWow64 = false;
			IsWow64Process(Process.GetCurrentProcess().Handle, out isWow64);
			return isWow64;
		}

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool IsWow64Process(
			[In] IntPtr hProcess,
			[Out] out bool wow64Process
		);

		#endregion

		#region AppManager

		public virtual IAppManager GetNewAppManagerClient()
		{
			return new AppManagerClient();
		}

		#endregion
	}
}
