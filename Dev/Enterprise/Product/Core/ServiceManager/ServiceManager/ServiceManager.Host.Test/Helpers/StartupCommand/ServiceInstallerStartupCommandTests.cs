using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;
using ServiceManager.Integration.CW;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.StartupCommand
{
	[TestClass]
	public class ServiceInstallerStartupCommandTests : TestCase
	{
		public void TestWrongConstructorParams()
		{
			var exception = AssertExceptionThrown<ArgumentNullException>(() => new TestServiceInstallerStartupCommand(null, hostOptions.Object, managedInstallerHelper.Object, Mock.Of<IProductRegistration>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("hostLogger"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new TestServiceInstallerStartupCommand(hostLogger, null, managedInstallerHelper.Object, Mock.Of<IProductRegistration>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("hostOptions"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new TestServiceInstallerStartupCommand(hostLogger, hostOptions.Object, null, Mock.Of<IProductRegistration>()));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("managedInstallerHelper"));

			exception = AssertExceptionThrown<ArgumentNullException>(() => new TestServiceInstallerStartupCommand(hostLogger, hostOptions.Object, managedInstallerHelper.Object, null));
			NUnit.Framework.Assert.That(exception.ParamName, Is.EqualTo("productRegistration"));
		}

		[ExpectNoExceptions]
		public void TestExecuteWithSuccessfulInstallationReturnsZero()
		{
			// Arrange
			var command = CreateStartupCommand();

			// Act
			var result = command.Execute();

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(0));
		}

		public void TestExecuteWithFailedInstallationThrowsException()
		{
			// Arrange
			managedInstallerHelper
				.Setup(o => o.Install(It.IsAny<string[]>()))
				.Throws<Exception>();

			var command = CreateStartupCommand();

			// Act, Throw
			AssertExceptionThrown<Exception>(() => command.Execute());
		}

		[ExpectNoExceptions]
		public void TestExecuteWithWin32ExceptionReturnsHResult()
		{
			// Arrange
			managedInstallerHelper
				.Setup(o => o.Install(It.IsAny<string[]>()))
				.Throws(() => new SystemException("test", new Win32Exception("test")));

			var command = CreateStartupCommand();

			// Act
			var result = command.Execute();

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(-2147467259));
		}

		public void TestExecuteWithNoInnerExceptionThrowsException()
		{
			// Arrange
			managedInstallerHelper
				.Setup(o => o.Install(It.IsAny<string[]>()))
				.Throws(() => new SystemException("test"));

			var command = CreateStartupCommand();

			// Act, Throw
			AssertExceptionThrown<SystemException>(() => command.Execute());
		}

		[ExpectNoExceptions]
		public void TestExecuteWithIdentityExceptionReturnsNegativeResult()
		{
			// Arrange
			managedInstallerHelper
				.Setup(o => o.Install(It.IsAny<string[]>()))
				.Throws(() => new SystemException("test", new IdentityNotMappedException("test")));

			var command = CreateStartupCommand();

			// Act
			var result = command.Execute();

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(-1));
		}

		#region CreateArguments

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithServerNameArgumentExists()
		{
			// Arrange
			hostOptions.Setup(o => o.ServerName).Returns("server");
			var command = CreateStartupCommand();

			// Act
			var result = command.CreateArguments(hostOptions.Object);

			// Assert
			NUnit.Framework.Assert.That(result["-ServerName"], Is.EqualTo("server"));
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithDatabaseNameArgumentExists()
		{
			// Arrange
			hostOptions.Setup(o => o.DatabaseName).Returns("database");
			var command = CreateStartupCommand();

			// Act
			var result = command.CreateArguments(hostOptions.Object);

			// Assert
			NUnit.Framework.Assert.That(result["-DatabaseName"], Is.EqualTo("database"));
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithUsernameArgumentExists()
		{
			// Arrange
			hostOptions.Setup(o => o.Username).Returns("john.doe");
			var command = CreateStartupCommand();

			// Act
			var result = command.CreateArguments(hostOptions.Object);

			// Assert
			NUnit.Framework.Assert.That(result.Keys.Where(k => k.Contains("Username")), Is.EquivalentTo(new[] { "-ProcessControllerUsername", "-LauncherSecurityUsername", }));
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(result["-ProcessControllerUsername"], Is.EqualTo("john.doe"));
				NUnit.Framework.Assert.That(result["-LauncherSecurityUsername"], Is.EqualTo("s_EDI_security$"));
			});
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithoutUsernameArgumentExists()
		{
			// Arrange
			var command = CreateStartupCommand();

			// Act
			var result = command.CreateArguments(hostOptions.Object);

			// Assert
			NUnit.Framework.Assert.That(result.Keys.Where(k => k.Contains("Username")), Is.EquivalentTo(new[] { "-ProcessControllerUsername", "-LauncherSecurityUsername", }));
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(result["-ProcessControllerUsername"], Is.EqualTo("System"));
				NUnit.Framework.Assert.That(result["-LauncherSecurityUsername"], Is.EqualTo("System"));
			});
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithAutomaticArgumentExists()
		{
			// Arrange
			hostOptions.Setup(o => o.Automatic).Returns(true);
			var command = CreateStartupCommand();

			// Act
			var result = command.CreateArguments(hostOptions.Object);

			// Assert
			NUnit.Framework.Assert.That(result["-Automatic"], Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithConfigArgumentExists()
		{
			// Arrange
			hostOptions.Setup(o => o.Config).Returns("config");
			var command = CreateStartupCommand();

			// Act
			var result = command.CreateArguments(hostOptions.Object);

			// Assert
			NUnit.Framework.Assert.That(result["-Config"], Is.EqualTo("config"));
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithPasswordArgumentExists()
		{
			// Arrange
			hostOptions.Setup(o => o.Password).Returns("MyTestPassword");
			var command = CreateStartupCommand();

			// Act
			var result = command.CreateArguments(hostOptions.Object);

			// Assert
			var argPrefix = "-ProcessControllerPassword";
			NUnit.Framework.Assert.That(result.Keys.Where(k => k.Contains("Password")), Is.EquivalentTo(new[] { argPrefix, }));
			var encryptedPassword = result[argPrefix];
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(encryptedPassword, Is.Not.EqualTo("MyTestPassword"));
				NUnit.Framework.Assert.That(DecryptPassword(encryptedPassword), Is.EqualTo("MyTestPassword"));
			});
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithExeArgumentExists()
		{
			// Arrange
			var command = CreateStartupCommand();

			// Act
			var result = command.CreateArguments(hostOptions.Object);

			// Assert
			NUnit.Framework.Assert.That(result.Keys.Where(k => k.Contains("Exe")), Is.EquivalentTo(new[] { "-ProcessControllerExe", "-LauncherSecurityExe", }));
			var assemblyLocation = Assembly.GetExecutingAssembly().Location;
			var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(result["-ProcessControllerExe"], Is.EqualTo($@"{assemblyDirectory}\{ServiceManagerConstants.ServiceManagerHostExe}"));
				NUnit.Framework.Assert.That(result["-LauncherSecurityExe"], Is.EqualTo($@"{assemblyDirectory}\net8.0\CargoWise.ServiceManager.Next.Launcher.exe"));
			});
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithEnterpriseCodeArgumentExists()
		{
			// Arrange
			var command = CreateStartupCommand();

			// Act
			var result = command.CreateArguments(hostOptions.Object);

			// Assert
			NUnit.Framework.Assert.That(result["-EnterpriseCode"], Is.EqualTo("EDI"));
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithServerCodeArgumentExists()
		{
			// Arrange
			var command = CreateStartupCommand();

			// Act
			var result = command.CreateArguments(hostOptions.Object);

			// Assert
			NUnit.Framework.Assert.That(result["-ServerCode"], Is.EqualTo("DAT"));
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithLicenseOverride()
		{
			var username = "john.doe@sand.wtg.zone";
			var result = CreateArgumentsWithLicenseOverride(username, isUat: false);
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(result["-EnterpriseCode"], Is.EqualTo("ENT"));
				NUnit.Framework.Assert.That(result["-ServerCode"], Is.EqualTo("SER"));
				NUnit.Framework.Assert.That(result["-ProcessControllerUsername"], Is.EqualTo(username));
				NUnit.Framework.Assert.That(result["-LauncherSecurityUsername"], Is.EqualTo("s_ENT_security$@sand.wtg.zone"));
			});
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithLicenseOverrideForUat()
		{
			var username = "john.doe";
			var result = CreateArgumentsWithLicenseOverride(username, isUat: true);
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(result["-EnterpriseCode"], Is.EqualTo("ENT"));
				NUnit.Framework.Assert.That(result["-ServerCode"], Is.EqualTo("SER"));
				NUnit.Framework.Assert.That(result["-ProcessControllerUsername"], Is.EqualTo(username));
				NUnit.Framework.Assert.That(result["-LauncherSecurityUsername"], Is.EqualTo("s_gMSA_PRC1SAND$@sand.wtg.zone"));
			});
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithLicenseOverrideAndNoUsername()
		{
			var result = CreateArgumentsWithLicenseOverride(null, isUat: false);
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(result["-EnterpriseCode"], Is.EqualTo("ENT"));
				NUnit.Framework.Assert.That(result["-ServerCode"], Is.EqualTo("SER"));
				NUnit.Framework.Assert.That(result["-ProcessControllerUsername"], Is.EqualTo("System"));
				NUnit.Framework.Assert.That(result["-LauncherSecurityUsername"], Is.EqualTo("System"));
			});
		}

		[ExpectNoExceptions]
		public void TestCreateArgumentsWithLicenseOverrideForUatAndNoUsername()
		{
			var result = CreateArgumentsWithLicenseOverride(null, isUat: true);
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(result["-EnterpriseCode"], Is.EqualTo("ENT"));
				NUnit.Framework.Assert.That(result["-ServerCode"], Is.EqualTo("SER"));
				NUnit.Framework.Assert.That(result["-ProcessControllerUsername"], Is.EqualTo("System"));
				NUnit.Framework.Assert.That(result["-LauncherSecurityUsername"], Is.EqualTo("System"));
			});
		}

		IDictionary<string, string> CreateArgumentsWithLicenseOverride(string userName, bool isUat)
		{
			hostOptions.Setup(o => o.Username).Returns(userName);
			productRegistration = Mock.Of<IProductRegistration>(
				p => p.IsWiseTechGlobalInternalUATSystem() == isUat && p.Key == Mock.Of<IProductRegistrationKey>(
					k => k.ServerCode == "SER" && k.EnterpriseCode == "ENT"));
			using var x = ObjectFactory.Substitute(productRegistration);
			var command = CreateStartupCommand();

			return command.CreateArguments(hostOptions.Object);
		}

		#endregion CreateArguments

		TestServiceInstallerStartupCommand CreateStartupCommand()
		{
			return new TestServiceInstallerStartupCommand(hostLogger, hostOptions.Object, managedInstallerHelper.Object, productRegistration);
		}

		protected override void SetUp()
		{
			hostLogger = Mock.Of<IHostLogger>();
			hostOptions = new Mock<IServiceManagerHostOptions>();
			managedInstallerHelper = new Mock<IManagedInstallerAdapter>();
			productRegistration = ObjectFactory.Get<IProductRegistration>();
		}

		IHostLogger hostLogger;
		Mock<IServiceManagerHostOptions> hostOptions;
		Mock<IManagedInstallerAdapter> managedInstallerHelper;
		IProductRegistration productRegistration;

		class TestServiceInstallerStartupCommand : ServiceInstallerStartupCommand
		{
			public TestServiceInstallerStartupCommand(IHostLogger hostLogger, IServiceManagerHostOptions hostOptions, IManagedInstallerAdapter managedInstallerHelper, IProductRegistration productRegistration)
				: base(hostLogger, hostOptions, managedInstallerHelper, productRegistration)
			{
			}

			public IList<string> CreateArgumentsList(IServiceManagerHostOptions startupOptions)
			{
				return CreateServiceInstallerArguments(startupOptions);
			}

			public IDictionary<string, string> CreateArguments(IServiceManagerHostOptions startupOptions)
			{
				var args = CreateServiceInstallerArguments(startupOptions);
				return args.Select(x => x.Split(new[] { '=' }, 2)).ToDictionary(x => x[0], x => x.Length > 1 ? x[1] : null);
			}

			public override int Execute()
			{
				var installerArgs = CreateServiceInstallerArguments(hostOptions);

				return RunInstaller(installerArgs.ToArray());
			}
		}

		static string DecryptPassword(string encryptedPassword)
		{
			var password = ProtectedData.Unprotect(Convert.FromBase64String(encryptedPassword), null, DataProtectionScope.CurrentUser);
			return Encoding.Unicode.GetString(password);
		}
	}
}
