using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
#if !NETFRAMEWORK
using Microsoft.Win32.SafeHandles;
#endif
using CargoWise.Loader.Common;
using CargoWise.RemoteDesktopServices.Upgrader.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RemoteDesktopServices.Upgrader.Test
{
	public class SessionKillerWarningTest : TestCase
	{
		public void TestInstallExcludingDependenciesErrorWhenOtherUsersConnectRDS()
		{
			// Arrange
			using (var mockProcess = Process.Start("mstsc"))
			{
				var services = new Mock<IServiceContainer>();
				services
					.SetupGet(s => s.MessageBox)
					.Returns(Mock.Of<IMessageBoxProxy>());
				var config = new Configuration()
				{
					Services = services.Object,
				};

				var installation = new Installation(config);
				var installationResult = new InstallationResultCollection();

				var killer = new SessionKillerWarning(installation, "mstsc", UpgraderStartupDirector.PluginProductName);

				using (var mockUser = new ImpersonateUser())
				{
					mockUser.Impersonate("sand", "WCATest_Admin", "p@ssw0rd");

					// Act
					killer.Install(installationResult);
				}

				// Assert
				var errorResult = installationResult[0];
				AssertEquals(true, errorResult.IsError);
				AssertEquals($"{UpgraderStartupDirector.PluginProductName} could not be updated because other user sessions are currently using it. CargoWise will open but you will not have the latest {UpgraderStartupDirector.PluginProductName} features available to you. Please contact your system administrator to organise the update to occur in a maintenance window.", errorResult.Message);
			}
		}
	}

	public class ImpersonateUser : IDisposable
	{
#if NETFRAMEWORK
		IntPtr tokenHandle = new IntPtr(0);
		WindowsImpersonationContext impersonatedUser;
#else
		SafeAccessTokenHandle tokenHandle;
		IDisposable impersonatedUser;
#endif

		public void Impersonate(string domainName, string userName, string password)
		{
			const int LOGON32_PROVIDER_DEFAULT = 0;
			const int LOGON32_LOGON_INTERACTIVE = 2;
#if NETFRAMEWORK
			tokenHandle = IntPtr.Zero;

			var returnValue = LogonUser(
			userName,
			domainName,
			password,
			LOGON32_LOGON_INTERACTIVE,
			LOGON32_PROVIDER_DEFAULT,
			ref tokenHandle);

			if (!returnValue)
			{
				int ret = Marshal.GetLastWin32Error();
				throw new System.ComponentModel.Win32Exception(ret);
			}

			WindowsIdentity newId = new WindowsIdentity(tokenHandle);
			impersonatedUser = newId.Impersonate();
#else
			if (!LogonUser(
				userName,
				domainName,
				password,
				LOGON32_LOGON_INTERACTIVE,
				LOGON32_PROVIDER_DEFAULT,
				out SafeAccessTokenHandle safeTokenHandle))
			{
				throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
			}

			tokenHandle = safeTokenHandle;
			impersonatedUser = WindowsIdentity.RunImpersonated(tokenHandle, () => new ImpersonationScope());
#endif
		}

		public void Dispose()
		{
#if NETFRAMEWORK
			impersonatedUser?.Undo();
			if (tokenHandle != IntPtr.Zero)
			{
				CloseHandle(tokenHandle);
			}
#else
			impersonatedUser?.Dispose();
			tokenHandle?.Dispose();
#endif
		}

#if !NETFRAMEWORK
	class ImpersonationScope : IDisposable
	{
		public void Dispose()
		{
		}
	}
#endif

#if NETFRAMEWORK
		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool LogonUser(
			String lpszUsername,
			String lpszDomain,
			String lpszPassword,
			int dwLogonType,
			int dwLogonProvider,
			ref IntPtr phToken);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public extern static bool CloseHandle(IntPtr handle);
#else
		[DllImport("advapi32.dll", SetLastError = true)]
		static extern bool LogonUser(
			string lpszUsername,
			string lpszDomain,
			string lpszPassword,
			int dwLogonType,
			int dwLogonProvider,
			out SafeAccessTokenHandle phToken);
#endif
	}
}
