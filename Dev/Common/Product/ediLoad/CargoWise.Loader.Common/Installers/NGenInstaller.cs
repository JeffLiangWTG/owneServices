using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using CargoWise.ApplicationManager.Common;
using CargoWise.BrandManager;
using CargoWise.Common;
using WTG.Authenticode;

namespace CargoWise.Loader.Common
{
	public class NGenInstaller : AppManagerInvoker, IAppManagerInvocable
	{
		public NGenInstaller(Installation installation, IAuthenticodeVerificationService authenticodeVerificationService, string workingDirectory, string targetPath, Action action, int delayTimeInMsForTest = 0)
			: base(installation)
		{
			this.authenticodeVerificationService = authenticodeVerificationService;
			this.workingDirectory = workingDirectory;
			this.targetPath = targetPath;
			this.action = action;
			this.delayTimeInMsForTest = delayTimeInMsForTest;
		}

		public NGenInstaller(Installation installation, string workingDirectory, string targetPath, Action action, int delayTimeInMsForTest = 0)
			: this(installation, new AuthenticodeVerificationService(), workingDirectory, targetPath, action, delayTimeInMsForTest)
		{
		}

		NGenInstaller()
			: this(new Installation(new Configuration()), null, null, 0)
		{
		}

		public const string NGenRootsDllName = "CargoWise.NGenRoot.dll";
		const string ValidNGenPublicKeyToken = "T1cN8nBXY1A=";

		readonly IAuthenticodeVerificationService authenticodeVerificationService;
		readonly string workingDirectory;
		readonly string targetPath;
		readonly Action action;
		readonly int delayTimeInMsForTest;

		protected override bool NeedsToInstallCore()
		{
			return action == Action.Uninstall || NGenEnabled;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt")]
		bool NGenEnabled
		{
			get
			{
				// Ngen won't exist anymore because we will be on .NET 8, so needn't rename this registry item to CargoWise. Will delete obsolete NGen code in WI00884783
				return Installation.Configuration.Services.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WiseTech Global\CargoWise One", "nongen", null) == null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt")]
		public int? NGenTimeout
		{
			get
			{
				// Ngen won't exist anymore because we will be on .NET 8, so needn't rename this registry item to CargoWise. Will delete obsolete NGen code in WI00884783
				return (int?)Installation.Configuration.Services.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WiseTech Global\CargoWise One", "NGen_TimeoutSecondsPerExecution", null);
			}
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			var result = InvokeAppManager<NGenInstaller>(new object[] { workingDirectory, targetPath, (int)action, delayTimeInMsForTest }, null);
			if (result.IsError)
			{
				result = InstallationResult.Warning(
					"There was a problem " + (action == Action.Uninstall ? "un" : "") + "installing " + BrandingFactory.Instance.ProductName + " assemblies as native images (using NGen.exe for performance).\r\n" +
					"This is an informational message only, your system will still function correctly.\r\n" +
					"Please contact " + BrandingFactory.Instance.ProductSupportName + " and report the following error:\r\n" + result.Message
					);
			}
			return result;
		}

		public AppManagerResult Invoke(bool waitedForMutex, object state)
		{
			if (state == null || ((object[])state).Length < 3)
			{
				return new AppManagerResult(AppManagerResultStatus.Error, "Invalid arguments");
			}
			try
			{
				var workingDirectory = (string)((object[])state)[0];
				var targetPath = (string)((object[])state)[1];
				var action = (Action)((object[])state)[2];
				var delayTimeInMsForTest = (int)((object[])state)[3];

				return InstallOrUninstall(workingDirectory, targetPath, action, delayTimeInMsForTest, NGenTimeout?.ToString() ?? string.Empty);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return new AppManagerResult(AppManagerResultStatus.Error, ex.Message);
			}
		}

		void ValidateArguments(string nGenWorkingDirectory)
		{
			var assemblyPath = GetNGenInstallerPath(nGenWorkingDirectory);

			ValidateAssemblyPublicKey(assemblyPath);

			authenticodeVerificationService.VerifyAuthenticodeSignatureIsWhitelisted(assemblyPath);
		}

		static void ValidateAssemblyPublicKey(string assemblyPath)
		{
			Assembly assembly;
#if NETCOREAPP
			var resolver = new PathAssemblyResolver([assemblyPath, typeof(object).Assembly.Location]);
			using (var mlc = new MetadataLoadContext(resolver))
			{
				assembly = mlc.LoadFromAssemblyPath(assemblyPath);
			}
#else
			assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
#endif
			var publicKeyToken = assembly.GetName().GetPublicKeyToken() ?? throw new ArgumentException("Assembly returned null public key token.");

			var assemblyKeyToken = Convert.ToBase64String(publicKeyToken);

			if (assemblyKeyToken != ValidNGenPublicKeyToken)
			{
				throw new ArgumentException("The assembly does not contain the expected public key token.");
			}
		}

		string GetNGenInstallerPath(string workingDirectory)
			=> Path.Combine(workingDirectory, "CargoWise.NGenInstaller.exe");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		protected AppManagerResult InstallOrUninstall(string workingDirectory, string targetPath, Action action, int delayTimeInMsForTest, string executionTimeout)
		{
			Argument.NotNull(workingDirectory, nameof(workingDirectory));
			ValidateArguments(workingDirectory);
			var startInfo = new ProcessStartInfo(GetNGenInstallerPath(workingDirectory));
			startInfo.WorkingDirectory = workingDirectory;
			startInfo.Arguments = $"{action} \"{targetPath}\" \"{executionTimeout}\" \"{delayTimeInMsForTest}\"";
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			var process = Process.Start(startInfo);

			if (action == Action.Uninstall)
			{
				if (process == null)
				{
					return new AppManagerResult(AppManagerResultStatus.Error, "Failed to start the NGenInstaller process");
				}

				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					return new AppManagerResult(AppManagerResultStatus.Error, string.Format(CultureInfo.InvariantCulture, "NGenInstaller process exited with error code {0}", process.ExitCode));
				}
			}

			return new AppManagerResult(AppManagerResultStatus.Success);
		}

		public enum Action
		{
			Install,
			Uninstall
		}
	}
}
