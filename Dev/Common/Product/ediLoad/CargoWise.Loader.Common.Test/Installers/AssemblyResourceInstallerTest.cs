using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.ApplicationManager.Common;
using CargoWise.Loader.Common.Installers;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing.Installers
{
	public class AssemblyResourceInstallerTest : TestCase
	{
		InstallationResultCollection installResult;
		Mock<AssemblyResourceInstaller> installer;
		const string TestDll = "CargoWise.Loader.Common.Resources.Enterprise.URLHandler.Integration.dll";

		protected override void SetUp()
		{
			base.SetUp();
			var configuration = new MockConfiguration();

			var messageBoxProxy = new Mock<IMessageBoxProxy>();
			messageBoxProxy
				.Setup(x => x.Show(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<MessageBoxButtons>(),
					It.IsAny<MessageBoxIcon>()))
				.Returns(DialogResult.OK);

			var service = Mock.Of<IServiceContainer>(sc =>
				sc.MessageBox == messageBoxProxy.Object);
			configuration.Services = service;

			installResult = new InstallationResultCollection();
			installer = new Mock<AssemblyResourceInstaller>(new Installation(configuration));
			installer.Protected().SetupGet<string>("InstallerName").Returns("TestInstaller.exe");
			installer.Protected().SetupGet<string>("NameOfComponentBeingInstalled").Returns("TestComponent");
			installer.CallBase = true;
		}

		public void TestExtractsFileFromResource()
		{
			//Arrange
			installer.Protected()
				.Setup<Stream>("ResourceStream")
				.Returns(GetType().Assembly.GetManifestResourceStream(TestDll));

			installer.Protected()
				.Setup<InstallationResult>(
					"InvokeAppManager",
					ItExpr.IsAny<Type>(),
					ItExpr.IsAny<object>(),
					ItExpr.IsAny<MutexRequest>())
				.Callback(() =>
					{
						//Assert
						AssertEquals(true, File.Exists(installer.Object.InstallerAbsolutePath));
					}
				)
				.Returns(InstallationResult.OK());

			//Action
			installer.Object.Install(installResult);
		}

		public void TestSetupShouldBeCleaned_WhenInstallationSucceeds()
		{
			//Arrange
			installer.Protected()
				.Setup<Stream>("ResourceStream")
				.Returns(GetType().Assembly.GetManifestResourceStream(TestDll));

			installer.Protected()
				.Setup<InstallationResult>(
					"InvokeAppManager",
					ItExpr.IsAny<Type>(),
					ItExpr.IsAny<object>(),
					ItExpr.IsAny<MutexRequest>())
				.Returns(InstallationResult.OK());

			//Action
			installer.Object.Install(installResult);

			//Assert
			installer.Protected()
				.Verify(
					"InvokeAppManager",
					Times.Once(),
					ItExpr.IsAny<Type>(),
					ItExpr.IsAny<object>(),
					ItExpr.IsAny<MutexRequest>());

			AssertEquals(false, File.Exists(installer.Object.InstallerAbsolutePath));
		}

		public void TestSetupShouldBeCleaned_WhenInstallationFailed()
		{
			//Arrange
			installer.Protected()
				.Setup<Stream>("ResourceStream")
				.Returns(GetType().Assembly.GetManifestResourceStream(TestDll));

			installer.Protected()
				.Setup<InstallationResult>(
					"InvokeAppManager",
					ItExpr.IsAny<Type>(),
					ItExpr.IsAny<object>(),
					ItExpr.IsAny<MutexRequest>())
				.Returns(InstallationResult.Error("Failed"));

			//Action
			installer.Object.Install(installResult);

			//Assert
			installer.Protected()
				.Verify(
					"InvokeAppManager",
					Times.Once(),
					ItExpr.IsAny<Type>(),
					ItExpr.IsAny<object>(),
					ItExpr.IsAny<MutexRequest>());

			AssertEquals(false, File.Exists(installer.Object.InstallerAbsolutePath));
		}

		public void TestSetupShouldBeCleaned_WhenInstallationThrowsException()
		{
			//Arrange
			installer.Protected()
				.Setup<Stream>("ResourceStream")
				.Returns(GetType().Assembly.GetManifestResourceStream(TestDll));

			installer.Protected()
				.Setup<InstallationResult>(
					"InvokeAppManager",
					ItExpr.IsAny<Type>(),
					ItExpr.IsAny<object>(),
					ItExpr.IsAny<MutexRequest>())
				.Throws<Exception>();

			//Action
			installer.Object.Install(installResult);

			//Assert
			installer.Protected()
				.Verify(
					"InvokeAppManager",
					Times.Once(),
					ItExpr.IsAny<Type>(),
					ItExpr.IsAny<object>(),
					ItExpr.IsAny<MutexRequest>());

			AssertEquals(false, File.Exists(installer.Object.InstallerAbsolutePath));
		}

		public void TestSetupShouldBeCleaned_WhenInstallExtractFailed()
		{
			//Arrange
			installer.Protected()
				.Setup<Stream>("ResourceStream")
				.Returns(GetType().Assembly.GetManifestResourceStream("MissingResourceStream.DoesNotExist.dll"));

			//Action
			installer.Object.Install(installResult);
			var result = WindowsInstallerAppManagerInvoker.GetResult(installResult);

			//Assert
			AssertEquals(AppManagerResultStatus.Error, result.Status);
			AssertEquals(false, File.Exists(installer.Object.InstallerAbsolutePath));
		}
	}
}
