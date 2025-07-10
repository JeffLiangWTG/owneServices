using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using NUnit.Framework;
using ServiceManager.Host.CW;
using WTG.NUnit;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ServiceManager.Host.Testing;

class ControllerServiceInstallerTest : TransactionedTestCase
{
	[ExpectNoExceptions]
	public void TestInstall_InstallerList_WithSecurityLauncher()
	{
		var expectedSecurityIdentifier = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(true);
		CheckInstallersWithSecLauncher(controllerServiceInstaller, expectedSecurityIdentifier);
	}

	[ExpectNoExceptions]
	public void TestInstall_InstallerList_WithoutSecurityLauncher()
	{
		var expectedSecurityIdentifier = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(false);
		CheckInstallersWithoutSecLauncher(controllerServiceInstaller, expectedSecurityIdentifier);
	}

	[ExpectNoExceptions]
	public void TestInstall_LogFilesAccessInstaller()
	{
		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(true);

		var logFilesAccessInstaller = controllerServiceInstaller.Installers.OfType<LogFilesAccessInstaller>().Single();
		var expectedLogFilesDirectory = ServiceManagerHelper.GetLogFilesDirectory("localhost", "Odyssey");
		NUnit.Framework.Assert.Multiple(() =>
		{
			NUnit.Framework.Assert.That(logFilesAccessInstaller.LogDirectoryName, Does.EndWith(@"\CargoWise edi\Process Controller\localhost\Odyssey"), $"Expected {expectedLogFilesDirectory}");
			NUnit.Framework.Assert.That(logFilesAccessInstaller.LogDirectoryName, Is.EqualTo(expectedLogFilesDirectory));
			NUnit.Framework.Assert.That(logFilesAccessInstaller.Sid, Is.EqualTo(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null)), "Expected SECURITY_LOCAL_SYSTEM_RID (S-1-5-18)");
		});
	}

	[ExpectNoExceptions]
	public void TestInstall_HttpAccessInstallers()
	{
		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(true);

		var installerProperties = controllerServiceInstaller.Installers.OfType<HttpAccessInstaller>().Select(i => (i.Sid, i.Url));
		NUnit.Framework.Assert.That(installerProperties, Is.EqualTo(new[]
			{
				(localSystemSid: LocalSystemSid, "http://+:7070/cargowise/processController/WTLSYD/"),
				(localSystemSid: LocalSystemSid, "http://+:7070/cargowise/processController/WTLSYD/security/"),
			}).Using(CustomComparers.TypeComparison));
	}

	[ExpectNoExceptions]
	public void TestInstall_CustomServiceInstallers()
	{
		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(true);

		var customServiceInstallers = controllerServiceInstaller.Installers
			.OfType<CustomServiceInstaller>()
			.ToList();
		NUnit.Framework.Assert.That(customServiceInstallers.Select(i => i.ServiceName), Is.EqualTo(new[] { "ediEnterpriseProcessController_localhost_Odyssey", "ediEnterpriseLauncherSecurity_localhost_Odyssey", }));

		var prcInstaller = customServiceInstallers[0];
		var secInstaller = customServiceInstallers[1];
		NUnit.Framework.Assert.Multiple(() =>
		{
			NUnit.Framework.Assert.That(prcInstaller.Description, Is.EqualTo("Manages and controls background tasks such as Email Processing, Report Scheduling, Database Consistency checks and backups."));
			NUnit.Framework.Assert.That(prcInstaller.DisplayName, Is.EqualTo("Process Controller (localhost Odyssey)"));
			NUnit.Framework.Assert.That(prcInstaller.ServiceName, Is.EqualTo("ediEnterpriseProcessController_localhost_Odyssey"));
			NUnit.Framework.Assert.That(prcInstaller.ExecutablePath, Does.EndWith(@"\CargoWise.ServiceManager.Host.Test.dll . Odyssey"), $"Invalid launch parameters: '{prcInstaller.ExecutablePath}'");
			NUnit.Framework.Assert.That(prcInstaller.Account, Is.EqualTo(ServiceAccount.LocalSystem));
			NUnit.Framework.Assert.That(prcInstaller.Username, Is.EqualTo(default(string)), "prcInstaller.Username - should be [null]");
			NUnit.Framework.Assert.That(prcInstaller.Password, Is.EqualTo(default(string)), "prcInstaller.Password - should be [null]");
			NUnit.Framework.Assert.That(prcInstaller.StartType, Is.EqualTo(ServiceStartMode.Manual));
			NUnit.Framework.Assert.That(prcInstaller.DelayedAutoStart, Is.EqualTo(false), "prcInstaller.DelayedAutoStart");
			NUnit.Framework.Assert.That(prcInstaller.ServicesDependedOn, Is.EqualTo(default(string[])), "prcInstaller.ServicesDependedOn - should be [null]");

			NUnit.Framework.Assert.That(secInstaller.Description, Is.EqualTo("Manages and controls background tasks such as access token generation."));
			NUnit.Framework.Assert.That(secInstaller.DisplayName, Is.EqualTo("Process Launcher - Security (localhost Odyssey)"));
			NUnit.Framework.Assert.That(secInstaller.ServiceName, Is.EqualTo("ediEnterpriseLauncherSecurity_localhost_Odyssey"));
			NUnit.Framework.Assert.That(secInstaller.ExecutablePath, Does.EndWith(@"\net8.0\CargoWise.ServiceManager.Next.Launcher.exe -EnterpriseCode:WTL -ServerCode:SYD . Odyssey"), "Invalid launch parameters");
			NUnit.Framework.Assert.That(secInstaller.Account, Is.EqualTo(ServiceAccount.LocalSystem));
			NUnit.Framework.Assert.That(secInstaller.Username, Is.EqualTo(default(string)), "secInstaller.Username - should be [null]");
			NUnit.Framework.Assert.That(secInstaller.Password, Is.EqualTo(default(string)), "secInstaller.Password - should be [null]");
			NUnit.Framework.Assert.That(secInstaller.StartType, Is.EqualTo(ServiceStartMode.Manual));
			NUnit.Framework.Assert.That(secInstaller.DelayedAutoStart, Is.EqualTo(false), "secInstaller.DelayedAutoStart");
			NUnit.Framework.Assert.That(secInstaller.ServicesDependedOn, Is.EqualTo(default(string[])), "secInstaller.ServicesDependedOn - should be [null]");
		});
	}

	[ExpectNoExceptions]
	public void TestInstall_CustomServiceInstallersAutomatic()
	{
		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(true, "-Automatic");

		var customServiceInstallers = controllerServiceInstaller.Installers
			.OfType<CustomServiceInstaller>()
			.ToList();

		NUnit.Framework.Assert.That(customServiceInstallers.Select(i => (i.StartType, i.DelayedAutoStart)), Is.EqualTo(new[] { (ServiceStartMode.Automatic, true), (ServiceStartMode.Automatic, true), }).Using(CustomComparers.TypeComparison));
	}

	[ExpectNoExceptions]
	public void TestInstall_CustomServiceInstallersNotLocalhost()
	{
		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(true, "-ServerName=Server1.wtg.zone");

		var customServiceInstallers = controllerServiceInstaller.Installers
			.OfType<CustomServiceInstaller>()
			.ToList();

		NUnit.Framework.Assert.That(customServiceInstallers.Select(i => i.ServiceName), Is.EqualTo(new[] { "ediEnterpriseProcessController_Server1.wtg.zone_Odyssey", "ediEnterpriseLauncherSecurity_Server1.wtg.zone_Odyssey", }));

		var prcInstaller = customServiceInstallers[0];
		var secInstaller = customServiceInstallers[1];
		NUnit.Framework.Assert.Multiple(() =>
		{
			NUnit.Framework.Assert.That(prcInstaller.Description, Is.EqualTo("Manages and controls background tasks such as Email Processing, Report Scheduling, Database Consistency checks and backups."));
			NUnit.Framework.Assert.That(prcInstaller.DisplayName, Is.EqualTo("Process Controller (Server1.wtg.zone Odyssey)"));
			NUnit.Framework.Assert.That(prcInstaller.ServiceName, Is.EqualTo("ediEnterpriseProcessController_Server1.wtg.zone_Odyssey"));
			NUnit.Framework.Assert.That(prcInstaller.ExecutablePath, Does.EndWith(@"\CargoWise.ServiceManager.Host.Test.dll Server1.wtg.zone Odyssey"), $"Invalid launch parameters: '{prcInstaller.ExecutablePath}'");
			NUnit.Framework.Assert.That(prcInstaller.ServicesDependedOn, Is.EqualTo(default(string[])), "prcInstaller.ServicesDependedOn - should be [null]");

			NUnit.Framework.Assert.That(secInstaller.Description, Is.EqualTo("Manages and controls background tasks such as access token generation."));
			NUnit.Framework.Assert.That(secInstaller.DisplayName, Is.EqualTo("Process Launcher - Security (Server1.wtg.zone Odyssey)"));
			NUnit.Framework.Assert.That(secInstaller.ServiceName, Is.EqualTo("ediEnterpriseLauncherSecurity_Server1.wtg.zone_Odyssey"));
			NUnit.Framework.Assert.That(secInstaller.ExecutablePath, Does.EndWith(@"\net8.0\CargoWise.ServiceManager.Next.Launcher.exe -EnterpriseCode:WTL -ServerCode:SYD Server1.wtg.zone Odyssey"), "Invalid launch parameters");
			NUnit.Framework.Assert.That(secInstaller.ServicesDependedOn, Is.EqualTo(default(string[])), "secInstaller.ServicesDependedOn - should be [null]");

			NUnit.Framework.Assert.That(customServiceInstallers.Select(i => (i.Account, i.Username, i.Password)), Is.EqualTo(new (ServiceAccount, string, string)[] { (ServiceAccount.LocalSystem, null, null), (ServiceAccount.LocalSystem, null, null), }).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(customServiceInstallers.Select(i => (i.StartType, i.DelayedAutoStart)), Is.EqualTo(new[] { (ServiceStartMode.Manual, false), (ServiceStartMode.Manual, false), }).Using(CustomComparers.TypeComparison));
		});
	}

	[ExpectNoExceptions]
	public void TestInstall_PrcUserNoPassword()
	{
		var userName = System.Environment.UserName;
		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(true, $"-ProcessControllerUsername={userName}");

		var customServiceInstallers = controllerServiceInstaller.Installers.OfType<CustomServiceInstaller>().ToList();
		var httpAccessInstallerProperties = controllerServiceInstaller.Installers.OfType<HttpAccessInstaller>().Select(i => (i.Sid, i.Url));
		var logFilesAccessInstaller = controllerServiceInstaller.Installers.OfType<LogFilesAccessInstaller>().Single();

		NUnit.Framework.Assert.Multiple(() =>
		{
			NUnit.Framework.Assert.That(httpAccessInstallerProperties, Is.EqualTo(new[]
				{
					(CurrentUserSid, "http://+:7070/cargowise/processController/WTLSYD/"),
					(LocalSystemSid, "http://+:7070/cargowise/processController/WTLSYD/security/"),
				}).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(customServiceInstallers.Select(i => (i.Account, i.Username, i.Password)), Is.EqualTo(new (ServiceAccount, string, string)[] { (ServiceAccount.User, userName, null), (ServiceAccount.LocalSystem, null, null), }).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(logFilesAccessInstaller.Sid, Is.EqualTo(CurrentUserSid));
		});
	}

	[ExpectNoExceptions]
	public void TestInstall_PrcUserWithPassword()
	{
		var userName = System.Environment.UserName;
		var password = Guid.NewGuid().ToString();
		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(true, $"-ProcessControllerUsername={userName}", $"-ProcessControllerPassword={EncryptPassword(password)}");

		var customServiceInstallers = controllerServiceInstaller.Installers.OfType<CustomServiceInstaller>().ToList();
		var httpAccessInstallerProperties = controllerServiceInstaller.Installers.OfType<HttpAccessInstaller>().Select(i => (i.Sid, i.Url));
		var logFilesAccessInstaller = controllerServiceInstaller.Installers.OfType<LogFilesAccessInstaller>().Single();

		NUnit.Framework.Assert.Multiple(() =>
		{
			NUnit.Framework.Assert.That(httpAccessInstallerProperties, Is.EqualTo(new[]
				{
					(CurrentUserSid, "http://+:7070/cargowise/processController/WTLSYD/"),
					(LocalSystemSid, "http://+:7070/cargowise/processController/WTLSYD/security/"),
				}).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(customServiceInstallers.Select(i => (i.Account, i.Username, i.Password)), Is.EqualTo(new (ServiceAccount, string, string)[] { (ServiceAccount.User, userName, password), (ServiceAccount.LocalSystem, null, null), }).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(logFilesAccessInstaller.Sid, Is.EqualTo(CurrentUserSid));
		});
	}

	public void TestInstall_NoPrcUser_ThrowException()
	{
		IDictionary stateSaver = new Hashtable();
		using var controllerServiceInstaller = new ControllerServiceInstallerForTest("-ProcessControllerUsername=", "-LauncherSecurityUsername=System");

		var e = AssertExceptionThrown<InvalidOperationException>(() => controllerServiceInstaller.Install(stateSaver));

		NUnit.Framework.Assert.That(e.Message, Does.StartWith("An exception occurred in the OnBeforeInstall event handler"), $"Unexpected exception: {e}");
		NUnit.Framework.Assert.That(e.InnerException, Is.TypeOf<InvalidOperationException>());
		NUnit.Framework.Assert.That(e.InnerException?.Message, Is.EqualTo("Username not found with ProcessControllerUsername"));
	}

	[ExpectNoExceptions]
	public void TestInstall_SecUserNoPassword()
	{
		var userName = System.Environment.UserName;
		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(true, $"-LauncherSecurityUsername={userName}");

		var customServiceInstallers = controllerServiceInstaller.Installers.OfType<CustomServiceInstaller>().ToList();
		var httpAccessInstallerProperties = controllerServiceInstaller.Installers.OfType<HttpAccessInstaller>().Select(i => (i.Sid, i.Url));
		var logFilesAccessInstaller = controllerServiceInstaller.Installers.OfType<LogFilesAccessInstaller>().Single();

		NUnit.Framework.Assert.Multiple(() =>
		{
			NUnit.Framework.Assert.That(httpAccessInstallerProperties, Is.EqualTo(new[]
				{
					(LocalSystemSid, "http://+:7070/cargowise/processController/WTLSYD/"),
					(CurrentUserSid, "http://+:7070/cargowise/processController/WTLSYD/security/"),
				}).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(customServiceInstallers.Select(i => (i.Account, i.Username, i.Password)), Is.EqualTo(new (ServiceAccount, string, string)[] { (ServiceAccount.LocalSystem, null, null), (ServiceAccount.User, userName, null), }).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(logFilesAccessInstaller.Sid, Is.EqualTo(LocalSystemSid));
		});
	}

	[ExpectNoExceptions]
	public void TestInstall_SecUserWithPassword()
	{
		var userName = System.Environment.UserName;
		var password = Guid.NewGuid().ToString();
		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(true, $"-LauncherSecurityUsername={userName}", $"-LauncherSecurityPassword={EncryptPassword(password)}");

		var customServiceInstallers = controllerServiceInstaller.Installers.OfType<CustomServiceInstaller>().ToList();
		var httpAccessInstallerProperties = controllerServiceInstaller.Installers.OfType<HttpAccessInstaller>().Select(i => (i.Sid, i.Url));
		var logFilesAccessInstaller = controllerServiceInstaller.Installers.OfType<LogFilesAccessInstaller>().Single();

		NUnit.Framework.Assert.Multiple(() =>
		{
			NUnit.Framework.Assert.That(httpAccessInstallerProperties, Is.EqualTo(new[]
				{
					(LocalSystemSid, "http://+:7070/cargowise/processController/WTLSYD/"),
					(CurrentUserSid, "http://+:7070/cargowise/processController/WTLSYD/security/"),
				}).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(customServiceInstallers.Select(i => (i.Account, i.Username, i.Password)), Is.EqualTo(new (ServiceAccount, string, string)[] { (ServiceAccount.LocalSystem, null, null), (ServiceAccount.User, userName, password), }).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(logFilesAccessInstaller.Sid, Is.EqualTo(LocalSystemSid));
		});
	}

	public void TestInstall_InvalidSecUser_DoNotBlockPrcInstall()
	{
		var userName = System.Environment.UserName;
		var dummyUsername = "dummyUsername";
		var dummyAccount = new NTAccount(dummyUsername);
		var e = AssertExceptionThrown<IdentityNotMappedException>(() => _ = dummyAccount.Translate(typeof(SecurityIdentifier)));
		NUnit.Framework.Assert.That(e.Message, Is.EqualTo("Some or all identity references could not be translated."));

		using var controllerServiceInstaller = SetupControllerServiceInstallerForInstallTest(false, $"-ProcessControllerUsername={userName}", $"-LauncherSecurityUsername={dummyUsername}");

		var customServiceInstallers = controllerServiceInstaller.Installers.OfType<CustomServiceInstaller>().ToList();
		var httpAccessInstallerProperties = controllerServiceInstaller.Installers.OfType<HttpAccessInstaller>().Select(i => (i.Sid, i.Url));
		var logFilesAccessInstaller = controllerServiceInstaller.Installers.OfType<LogFilesAccessInstaller>().Single();

		NUnit.Framework.Assert.Multiple(() =>
		{
			NUnit.Framework.Assert.That(httpAccessInstallerProperties, Is.EqualTo(new[]
				{
					(CurrentUserSid, "http://+:7070/cargowise/processController/WTLSYD/"),
				}).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(customServiceInstallers.Select(i => (i.Account, i.Username, i.Password)), Is.EqualTo(new (ServiceAccount, string, string)[] { (ServiceAccount.User, userName, null), }).Using(CustomComparers.TypeComparison));

			NUnit.Framework.Assert.That(logFilesAccessInstaller.Sid, Is.EqualTo(CurrentUserSid));
		});
	}

	public void TestInstall_NoSecUser_ThrowException()
	{
		IDictionary stateSaver = new Hashtable();
		var userName = System.Environment.UserName;
		using var controllerServiceInstaller = new ControllerServiceInstallerForTest($"-ProcessControllerUsername={userName}", "-LauncherSecurityUsername=");

		var e = AssertExceptionThrown<InvalidOperationException>(() => controllerServiceInstaller.Install(stateSaver));

		NUnit.Framework.Assert.That(e.Message, Does.StartWith("An exception occurred in the OnBeforeInstall event handler"), $"Unexpected exception: {e}");
		NUnit.Framework.Assert.That(e.InnerException, Is.TypeOf<InvalidOperationException>());
		NUnit.Framework.Assert.That(e.InnerException?.Message, Is.EqualTo("Username not found with LauncherSecurityUsername"));
	}

	[ExpectNoExceptions]
	public void TestUninstall_InstallerList_WithSecurityLauncherAndNoState()
	{
		using var controllerServiceInstaller = SetupControllerServiceInstallerForUninstallTest(true, null);
		CheckInstallersWithSecLauncher(controllerServiceInstaller, expectedSecurityIdentifier: null);
	}

	[ExpectNoExceptions]
	public void TestUninstall_InstallerList_WithSecurityLauncherAndNoSecLauncherInState()
	{
		var serviceTypes = Array.Empty<ServiceType>();
		using var controllerServiceInstaller = SetupControllerServiceInstallerForUninstallTest(true, serviceTypes);
		CheckInstallersWithoutSecLauncher(controllerServiceInstaller, expectedSecurityIdentifier: null);
	}

	[ExpectNoExceptions]
	public void TestUninstall_InstallerList_WithSecurityLauncherAndSecLauncherInState()
	{
		var serviceTypes = new[] { ServiceType.LauncherSecurity, };
		using var controllerServiceInstaller = SetupControllerServiceInstallerForUninstallTest(true, serviceTypes);
		CheckInstallersWithSecLauncher(controllerServiceInstaller, expectedSecurityIdentifier: null);
	}

	[ExpectNoExceptions]
	public void TestUninstall_InstallerList_WithoutSecurityLauncherAndNoState()
	{
		using var controllerServiceInstaller = SetupControllerServiceInstallerForUninstallTest(false, null);
		CheckInstallersWithSecLauncher(controllerServiceInstaller, expectedSecurityIdentifier: null);
	}

	[ExpectNoExceptions]
	public void TestUninstall_InstallerList_WithoutSecurityLauncherAndNoSecLauncherInState()
	{
		var serviceTypes = Array.Empty<ServiceType>();
		using var controllerServiceInstaller = SetupControllerServiceInstallerForUninstallTest(false, serviceTypes);
		CheckInstallersWithoutSecLauncher(controllerServiceInstaller, expectedSecurityIdentifier: null);
	}

	[ExpectNoExceptions]
	public void TestUninstall_InstallerList_WithoutSecurityLauncherAndSecLauncherInState()
	{
		var serviceTypes = new[] { ServiceType.LauncherSecurity, };
		using var controllerServiceInstaller = SetupControllerServiceInstallerForUninstallTest(false, serviceTypes);
		CheckInstallersWithSecLauncher(controllerServiceInstaller, expectedSecurityIdentifier: null);
	}

	void CheckInstallersWithSecLauncher(ControllerServiceInstaller controllerServiceInstaller, SecurityIdentifier expectedSecurityIdentifier)
	{
		var expectedTypes = new List<Type>()
		{
			typeof(LogFilesAccessInstaller),
			typeof(HttpAccessInstaller),
			typeof(CustomServiceInstaller),
			typeof(HttpAccessInstaller),
			typeof(CustomServiceInstaller),
			typeof(TcpIpRegistrySettingsInstaller),
		};
		var actualTypes = controllerServiceInstaller.Installers.OfType<Installer>().Select(x => x.GetType());

		NUnit.Framework.Assert.That(actualTypes, Is.EqualTo(expectedTypes));

		var installerProperties = controllerServiceInstaller.Installers.OfType<HttpAccessInstaller>().Select(i => (i.Sid, i.Url));
		NUnit.Framework.Assert.That(installerProperties, Is.EqualTo(new (SecurityIdentifier localSystemSid, string Url)[]
			{
				(expectedSecurityIdentifier, "http://+:7070/cargowise/processController/WTLSYD/"),
				(expectedSecurityIdentifier, "http://+:7070/cargowise/processController/WTLSYD/security/"),
			}).Using(CustomComparers.TypeComparison));

		var customServiceInstaller = controllerServiceInstaller.Installers.OfType<CustomServiceInstaller>().ToList();
		var customServiceInstallerNames = customServiceInstaller.Select(i => i.ServiceName);
		NUnit.Framework.Assert.That(customServiceInstallerNames, Is.EqualTo(new[] { "ediEnterpriseProcessController_localhost_Odyssey", "ediEnterpriseLauncherSecurity_localhost_Odyssey", }));
	}

	void CheckInstallersWithoutSecLauncher(ControllerServiceInstaller controllerServiceInstaller, SecurityIdentifier expectedSecurityIdentifier)
	{
		var expectedTypes = new List<Type>()
		{
			typeof(LogFilesAccessInstaller),
			typeof(HttpAccessInstaller),
			typeof(CustomServiceInstaller),
			typeof(TcpIpRegistrySettingsInstaller),
		};
		var actualTypes = controllerServiceInstaller.Installers.OfType<Installer>().Select(x => x.GetType());

		NUnit.Framework.Assert.That(actualTypes, Is.EquivalentTo(expectedTypes));

		var installerProperties = controllerServiceInstaller.Installers.OfType<HttpAccessInstaller>().Select(i => (i.Sid, i.Url));
		NUnit.Framework.Assert.That(installerProperties, Is.EquivalentTo(new (SecurityIdentifier localSystemSid, string Url)[]
			{
				(expectedSecurityIdentifier, "http://+:7070/cargowise/processController/WTLSYD/"),
			}).Using(CustomComparers.TypeComparison));

		var customServiceInstallerNames = controllerServiceInstaller.Installers
			.OfType<CustomServiceInstaller>()
			.Select(x => x.ServiceName)
			.ToList();
		NUnit.Framework.Assert.That(customServiceInstallerNames, Is.EquivalentTo(new[] { "ediEnterpriseProcessController_localhost_Odyssey", }));
	}

	static string EncryptPassword(string password)
	{
		var encryptedPassword = ProtectedData.Protect(Encoding.Unicode.GetBytes(password), optionalEntropy: null, DataProtectionScope.CurrentUser);
		return Convert.ToBase64String(encryptedPassword);
	}

	static ControllerServiceInstaller SetupControllerServiceInstallerForInstallTest(bool withSecLauncher, params string[] extraArgs)
	{
		using var mechanismOverride = SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism.SetTemporaryValue(
			Guid.Empty,
			Guid.Empty,
			Guid.Empty,
			withSecLauncher ? DataProtectionMechanisms.Codes.ActiveDirectory : DataProtectionMechanisms.Codes.None);
		var controllerServiceInstaller = new ControllerServiceInstallerForTest(extraArgs);

		IDictionary stateSaver = new Hashtable();
		var e = AssertExceptionThrown<InvalidOperationException>(() => controllerServiceInstaller.Install(stateSaver));
		NUnit.Framework.Assert.Multiple(() =>
		{
			NUnit.Framework.Assert.That(e.InnerException, Is.TypeOf<NotImplementedException>());
			NUnit.Framework.Assert.That(e.InnerException?.Message, Is.EqualTo("Blocking install for unit tests"));
			NUnit.Framework.Assert.That(stateSaver.Keys, Is.EquivalentTo(new[] { "serviceTypes" }).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(stateSaver["serviceTypes"], Is.TypeOf<ServiceType[]>());
		});
		var expectedServiceTypes = withSecLauncher ? new[] { ServiceType.LauncherSecurity, } : Array.Empty<ServiceType>();
		NUnit.Framework.Assert.That((ServiceType[])stateSaver["serviceTypes"], Is.EquivalentTo(expectedServiceTypes));
		return controllerServiceInstaller;
	}

	static ControllerServiceInstaller SetupControllerServiceInstallerForUninstallTest(bool withSecLauncher, ServiceType[] serviceTypes, params string[] extraArgs)
	{
		using var mechanismOverride = SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism.SetTemporaryValue(
			Guid.Empty,
			Guid.Empty,
			Guid.Empty,
			withSecLauncher ? DataProtectionMechanisms.Codes.ActiveDirectory : DataProtectionMechanisms.Codes.None);
		var controllerServiceInstaller = new ControllerServiceInstallerForTest(extraArgs);

		IDictionary stateSaver = new Hashtable();
		if (serviceTypes != null)
		{
			stateSaver["serviceTypes"] = serviceTypes;
		}
		var e = AssertExceptionThrown<NotImplementedException>(() => controllerServiceInstaller.Uninstall(stateSaver));
		NUnit.Framework.Assert.That(e.Message, Is.EqualTo("Blocking uninstall for unit tests"));
		return controllerServiceInstaller;
	}

	class ControllerServiceInstallerForTest : ControllerServiceInstaller
	{
		internal ControllerServiceInstallerForTest(params string[] extraArgs)
		{
			var commandLine = new List<string>() {
				"-ServerName=.",
				"-DatabaseName=Odyssey",
				"-EnterpriseCode=WTL",
				"-ServerCode=SYD",
				$"-LauncherSecurityExe={Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "net8.0", "CargoWise.ServiceManager.Next.Launcher.exe")}",
				$"-ProcessControllerExe={Assembly.GetExecutingAssembly().Location}",
				"-LauncherSecurityUsername=System",
				"-ProcessControllerUsername=System",
			};
			commandLine.AddRange(extraArgs);
			Context = new InstallContext(Path.GetRandomFileName(), commandLine.ToArray());
		}

		protected override void OnBeforeInstall(IDictionary savedState)
		{
			base.OnBeforeInstall(savedState);
			throw new NotImplementedException("Blocking install for unit tests");
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.OnBeforeUninstall(savedState);
			throw new NotImplementedException("Blocking uninstall for unit tests");
		}
	}

	static readonly SecurityIdentifier LocalSystemSid = new (WellKnownSidType.LocalSystemSid, null); // SECURITY_LOCAL_SYSTEM_RID (S-1-5-18)
	static readonly SecurityIdentifier CurrentUserSid = WindowsIdentity.GetCurrent()?.User;
}
