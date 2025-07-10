using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using WTG.NUnit;

namespace Enterprise.ServiceManager.Host.Testing.Common
{
	class CustomServiceInstallerTest
	{
		[SetUp]
		public void SetUp()
		{
			processWrapperMock = new Mock<IProcess>();
			processWrapperFactoryMock = new Mock<IProcessFactory>();
			processWrapperFactoryMock
				.Setup(x => x.Create(It.IsAny<ProcessStartInfo>(), It.IsAny<bool>(), It.IsAny<ProcessPriorityClass>()))
				.Callback<ProcessStartInfo, bool, ProcessPriorityClass>((startInfo, _, __) =>
				{
					capturedStartInfoArguments = startInfo.Arguments;
				})
				.Returns(processWrapperMock.Object);
		}

		CustomServiceInstaller CreateDefaultInstaller()
		{
			var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);
			// Remove event log installer since we don't want to actually install the service in test
			installer.Installers.RemoveAt(0);
			installer.ServiceName = "ServiceName";
			installer.ExecutablePath = "ExecutablePath";
			installer.StartType = ServiceStartMode.Manual;
			installer.DelayedAutoStart = false;
			installer.DisplayName = "DisplayName";
			installer.Account = ServiceAccount.LocalSystem;

			return installer;
		}

		[Test]
		public void TestWrongParamsCall()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => _ = new CustomServiceInstaller(null));
			Assert.That(exception.ParamName, NUnit.Framework.Is.EqualTo("processWrapperFactory"));
		}

		[Test]
		public void TestGenerateCreateCommandArgsEscapesParams_ExecutablePath()
		{
			// Assert
			using var installer = CreateDefaultInstaller();
			installer.ExecutablePath = "Execu\"tablePath";

			// Act
			installer.Install(new Hashtable());

			// Assert
			Assert.That(capturedStartInfoArguments, NUnit.Framework.Does.Contain(@"binpath= Execu\""tablePath"));
		}

		[Test]
		public void TestGenerateCreateCommandArgsEscapesParams_ServiceName()
		{
			// Assert
			using var installer = CreateDefaultInstaller();
			installer.ServiceName = "Service\"Name";

			// Act
			installer.Install(new Hashtable());

			// Assert
			Assert.That(capturedStartInfoArguments, NUnit.Framework.Does.Contain(@"create Service\""Name"));
		}

		[Test]
		public void TestGenerateCreateCommandArgsEscapesParams_DisplayName()
		{
			// Assert
			using var installer = CreateDefaultInstaller();
			installer.DisplayName = "Display\"Name";

			// Act
			installer.Install(new Hashtable());

			// Assert
			Assert.That(capturedStartInfoArguments, NUnit.Framework.Does.Contain(@"displayname= Display\""Name"));
		}

		[Test]
		public void TestGenerateCreateCommandArgsEscapesParams_Username()
		{
			// Assert
			using var installer = CreateDefaultInstaller();
			installer.Account = ServiceAccount.User;
			installer.Username = "User\"Name";

			// Act
			installer.Install(new Hashtable());

			// Assert
			Assert.That(capturedStartInfoArguments, NUnit.Framework.Does.Contain(@"obj= User\""Name"));
		}

		[Test]
		public void TestGenerateCreateCommandArgsQuotesParams_Username()
		{
			// Assert
			using var installer = CreateDefaultInstaller();
			installer.Account = ServiceAccount.User;
			installer.Username = "User Name";

			// Act
			installer.Install(new Hashtable());

			// Assert
			Assert.That(capturedStartInfoArguments, NUnit.Framework.Does.Contain(@"obj= ""User Name"""));
		}

		[Test]
		public void TestGenerateCreateCommandArgsEscapesParams_Password()
		{
			// Assert
			using var installer = CreateDefaultInstaller();
			installer.Account = ServiceAccount.User;
			installer.Username = "Username";
			installer.Password = "Pass\"Word";

			// Act
			installer.Install(new Hashtable());

			// Assert
			Assert.That(capturedStartInfoArguments, NUnit.Framework.Does.Contain(@"password= Pass\""Word"));
		}

		[Test]
		public void TestGenerateCreateCommandArgsCreatesExpectedParam_Depend()
		{
			void TestCase(string[] deps, string expected)
			{
				// Arrange
				using var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);
				// Remove event log installer since we don't want to actually install the service in test
				installer.Installers.RemoveAt(0);
				installer.ServiceName = "ServiceName";
				installer.ExecutablePath = "ExecutablePath";
				installer.StartType = ServiceStartMode.Manual;
				installer.DelayedAutoStart = false;
				installer.DisplayName = "DisplayName";
				installer.Account = ServiceAccount.LocalSystem;

				installer.ServicesDependedOn = deps;

				// Act
				installer.Install(new Hashtable());

				// Assert
				Assert.That(capturedStartInfoArguments, NUnit.Framework.Is.EqualTo(expected));
			}

			TestCase(new[] { "Service1" }, "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName depend= Service1");
			TestCase(new[] { "Service 1", "Service2" }, "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName depend= \"Service 1/Service2\"");
			TestCase(Array.Empty<string>(), "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName");
			TestCase(null, "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName");
		}

		[Test]
		public void TestGenerateCreateCommandArgsCreatesExpectedParam_BinPath()
		{
			void TestCase(string path, string expected)
			{
				// Arrange
				using var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);
				// Remove event log installer since we don't want to actually install the service in test
				installer.Installers.RemoveAt(0);
				installer.ServiceName = "ServiceName";
				installer.StartType = ServiceStartMode.Manual;
				installer.DelayedAutoStart = false;
				installer.DisplayName = "DisplayName";
				installer.Account = ServiceAccount.LocalSystem;

				installer.ExecutablePath = path;

				// Act
				installer.Install(new Hashtable());

				// Assert
				Assert.That(capturedStartInfoArguments, NUnit.Framework.Is.EqualTo(expected));
			}

			TestCase("C:\\Program Files\\Service\\Service.exe", "create ServiceName binpath= \"C:\\Program Files\\Service\\Service.exe\" type= own start= demand displayname= DisplayName");
			TestCase("C:\\ProgramFiles\\Service\\Service.exe", "create ServiceName binpath= C:\\ProgramFiles\\Service\\Service.exe type= own start= demand displayname= DisplayName");
		}

		[Test]
		public void TestGenerateCreateCommandArgsCreatesExpectedParam_Start()
		{
			void TestCase(ServiceStartMode start, bool delayed, string expected)
			{
				// Arrange
				using var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);
				// Remove event log installer since we don't want to actually install the service in test
				installer.Installers.RemoveAt(0);
				installer.ServiceName = "ServiceName";
				installer.ExecutablePath = "ExecutablePath";
				installer.DisplayName = "DisplayName";
				installer.Account = ServiceAccount.LocalSystem;

				installer.StartType = start;
				installer.DelayedAutoStart = delayed;

				// Act
				installer.Install(new Hashtable());

				// Assert
				Assert.That(capturedStartInfoArguments, NUnit.Framework.Is.EqualTo(expected));
			}

			TestCase(ServiceStartMode.Automatic, false, "create ServiceName binpath= ExecutablePath type= own start= auto displayname= DisplayName");
			TestCase(ServiceStartMode.Automatic, true, "create ServiceName binpath= ExecutablePath type= own start= delayed-auto displayname= DisplayName");
			TestCase(ServiceStartMode.Manual, false, "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName");
		}

		[Test]
		public void TestGenerateCreateCommandArgsCreatesExpectedParam_DisplayName()
		{
			void TestCase(string name, string expected)
			{
				// Arrange
				using var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);
				// Remove event log installer since we don't want to actually install the service in test
				installer.Installers.RemoveAt(0);
				installer.ServiceName = "ServiceName";
				installer.ExecutablePath = "ExecutablePath";
				installer.Account = ServiceAccount.LocalSystem;
				installer.StartType = ServiceStartMode.Manual;

				installer.DisplayName = name;

				//Act
				installer.Install(new Hashtable());

				// Assert
				Assert.That(capturedStartInfoArguments, NUnit.Framework.Is.EqualTo(expected));
			}

			TestCase("Display Name", "create ServiceName binpath= ExecutablePath type= own start= demand displayname= \"Display Name\"");
			TestCase("DisplayName", "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName");
		}

		[Test]
		public void TestGenerateCreateCommandArgsCreatesExpectedParam_AccountAndPwd()
		{
			void TestCase(ServiceAccount account, string username, string password, string expected)
			{
				// Arrange
				using var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);
				// Remove event log installer since we don't want to actually install the service in test
				installer.Installers.RemoveAt(0);
				installer.ServiceName = "ServiceName";
				installer.ExecutablePath = "ExecutablePath";
				installer.Account = ServiceAccount.LocalSystem;
				installer.StartType = ServiceStartMode.Manual;
				installer.DisplayName = "DisplayName";

				installer.Account = account;
				installer.Username = username;
				installer.Password = password;

				// Act
				installer.Install(new Hashtable());

				// Assert
				Assert.That(capturedStartInfoArguments, NUnit.Framework.Is.EqualTo(expected));
			}

			TestCase(ServiceAccount.LocalSystem, null, null, "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName");
			TestCase(ServiceAccount.LocalService, null, null, "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName obj= \"NT AUTHORITY\\LocalService\"");
			TestCase(ServiceAccount.NetworkService, null, null, "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName obj= \"NT AUTHORITY\\NetworkService\"");
			TestCase(ServiceAccount.User, "gMSA$@domain.com", null, "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName obj= gMSA$@domain.com");
			TestCase(ServiceAccount.User, "DOMAIN\\user", null, "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName obj= DOMAIN\\user");
			TestCase(ServiceAccount.User, "username", "password", "create ServiceName binpath= ExecutablePath type= own start= demand displayname= DisplayName obj= username password= password");
		}

		[Test]
		public void TestGenerateCreateCommandArgsThrowsExceptionIfExecutablePathIsInvalid()
		{
			void TestCase(string executablePath)
			{
				// Arrange
				using var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);
				// Remove event log installer since we don't want to actually install the service in test
				installer.Installers.RemoveAt(0);
				installer.ServiceName = "ServiceName";
				installer.Account = ServiceAccount.LocalSystem;
				installer.StartType = ServiceStartMode.Manual;
				installer.DisplayName = "DisplayName";

				installer.ExecutablePath = executablePath;

				// Act & Assert
				var exception = Assert.Throws<ArgumentException>(() => installer.Install(new Hashtable()));
				Assert.That(exception.ParamName, NUnit.Framework.Is.EqualTo("ExecutablePath"));
			}

			TestCase(null);
			TestCase("");
			TestCase("  ");
		}

		[Test]
		public void TestGenerateCreateCommandArgsThrowsExceptionIfDisplayNameIsInvalid()
		{
			void TestCase(string displayName)
			{
				// Arrange
				using var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);
				// Remove event log installer since we don't want to actually install the service in test
				installer.Installers.RemoveAt(0);
				installer.ServiceName = "ServiceName";
				installer.ExecutablePath = "ExecutablePath";
				installer.Account = ServiceAccount.LocalSystem;
				installer.StartType = ServiceStartMode.Manual;

				installer.DisplayName = displayName;

				// Act & Assert
				var exception = Assert.Throws<ArgumentException>(() => installer.Install(new Hashtable()));
				Assert.That(exception.ParamName, NUnit.Framework.Is.EqualTo("DisplayName"));
			}

			TestCase(null);
			TestCase("");
			TestCase("  ");
			TestCase(new string('a', 256));
		}

		[Test]
		public void TestServiceNameSetterThrowsExceptionIfValueIsInvalid()
		{
			void TestCase(string serviceName)
			{
				// Arrange
				using var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);
				// Remove event log installer since we don't want to actually install the service in test
				installer.Installers.RemoveAt(0);
				installer.DisplayName = "DisplayName";
				installer.ExecutablePath = "ExecutablePath";
				installer.Account = ServiceAccount.LocalSystem;
				installer.StartType = ServiceStartMode.Manual;

				// Act & Assert
				var exception = Assert.Throws<ArgumentException>(() => installer.ServiceName = serviceName);
				Assert.That(exception.ParamName, NUnit.Framework.Is.EqualTo("ServiceName"));
			}

			TestCase("Service/Name");
			TestCase(@"Service\Name");
			TestCase(new string('a', 81));
			TestCase(null);
			TestCase("");
			TestCase("  ");
		}

		[Test]
		[ExpectNoExceptions]
		public void TestServiceNameSetterChangesEventLogInstallerSource()
		{
			// Arrange
			using var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);

			// Act
			installer.ServiceName = "ServiceName";

			// Assert
			EventLogInstaller eventLogInstaller = (EventLogInstaller)installer.Installers[0];
			Assert.That(eventLogInstaller.Source, NUnit.Framework.Is.EqualTo("ServiceName"));
		}

		[Test]
		[ExpectNoExceptions]
		public void TestInstallSetsStateSaverInstalledToTrue()
		{
			// Arrange
			processWrapperMock.Setup(x => x.ExitCode).Returns(0);
			using var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);

			// Remove event log installer since we don't want to actually install the service in test
			installer.Installers.RemoveAt(0);

			installer.ServiceName = "ServiceName";
			installer.ExecutablePath = "C:\\Program Files\\Service\\Service.exe";
			installer.DisplayName = "DisplayName";

			var stateSaver = new Hashtable();

			// Act
			installer.Install(stateSaver);

			// Assert
			Assert.That(stateSaver["installed"], NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison));
		}

		[Test]
		[ExpectException(typeof(Win32Exception))]
		public void TestInstallThrowsExceptionIfExitCodeNotZero()
		{
			// Arrange
			processWrapperMock.Setup(x => x.ExitCode).Returns((RunnerExitCode)99);
			using var installer = new CustomServiceInstaller(processWrapperFactoryMock.Object);
			// Remove event log installer since we don't want to actually install the service in test
			installer.Installers.RemoveAt(0);
			installer.ServiceName = "ServiceName";
			installer.ExecutablePath = "C:\\Program Files\\Service\\Service.exe";
			installer.DisplayName = "DisplayName";

			// Act & Assert
			Assert.Throws<Win32Exception>(() => installer.Install(new Hashtable()));
		}

		Mock<IProcess> processWrapperMock;
		Mock<IProcessFactory> processWrapperFactoryMock;
		string capturedStartInfoArguments;
	}
}
