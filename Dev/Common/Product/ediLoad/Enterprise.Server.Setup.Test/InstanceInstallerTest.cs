using System;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	[GuiTest]
	public class InstanceInstallerTest : TestCase
	{
		public void TestNeedsToInstall()
		{
			config.InstallationSettings.RegisterInstance = false;
			AssertEquals(false, new InstanceInstaller(new Installation(config), config.InstallationSettings).NeedsToInstall());
			config.InstallationSettings.RegisterInstance = true;
			AssertEquals(true, new InstanceInstaller(new Installation(config), config.InstallationSettings).NeedsToInstall());
		}

		public void TestInstallWithCreateSchemaRequiredOk()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(false);
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema());
			Mock.Get(services.CargoWiseOneInstanceClass)
				.Setup(x => x.AddNewInstance("THEONE", config.InstallationSettings.ServerName, "thedatabase", null))
				.Returns(new Mock<ICargoWiseOneInstanceEntry>(MockBehavior.Strict).Object);

			var results = new InstallationResultCollection();
			new InstanceInstaller(new Installation(config), config.InstallationSettings).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.OKCount);
		}

		public void TestInstallWithCreateSchemaRequiredWithLoginOk()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(false);
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema()).Throws(new UnauthorizedAccessException("Access denied"));
			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(
				ShowDialogWithResult(form =>
				{
					((TextBox)form.Controls["textBoxUsername"]).Text = "admin";
					((TextBox)form.Controls["textBoxPassword"]).Text = "p@ssw0rd";
					((Button)form.Controls["buttonLogin"]).PerformClick();
				})
			);
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema("admin", "p@ssw0rd"));
			Mock.Get(services.CargoWiseOneInstanceClass)
				.Setup(x => x.AddNewInstance("THEONE", config.InstallationSettings.ServerName, "thedatabase", null))
				.Returns(new Mock<ICargoWiseOneInstanceEntry>(MockBehavior.Strict).Object);

			var results = new InstallationResultCollection();
			new InstanceInstaller(new Installation(config), config.InstallationSettings).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.OKCount);
		}

		public void TestInstallWithCreateSchemaRequiredWithLoginCancel()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(false);
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema()).Throws(new UnauthorizedAccessException("Access denied"));
			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(
				ShowDialogWithResult(form =>
				{
					((Button)form.Controls["buttonCancel"]).PerformClick();
				})
			);
			var results = new InstallationResultCollection();
			new InstanceInstaller(new Installation(config), config.InstallationSettings).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.WarningCount);
			Assert(results.GetWarningMessages().Contains("Access denied"));
		}

		public void TestInstallWithCreateSchemaRequiredWithLoginRetryOk()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(false);
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema()).Throws(new UnauthorizedAccessException("Access denied"));

			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(
				ShowDialogWithResult(form =>
				{
					((TextBox)form.Controls["textBoxUsername"]).Text = "user1";
					((TextBox)form.Controls["textBoxPassword"]).Text = "pwd1";
					((Button)form.Controls["buttonLogin"]).PerformClick();
				}));
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema("user1", "pwd1")).Throws(new UnauthorizedAccessException("Access denied"));

			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(
				ShowDialogWithResult(form =>
				{
					((TextBox)form.Controls["textBoxUsername"]).Text = "user2";
					((TextBox)form.Controls["textBoxPassword"]).Text = "pwd2";
					((Button)form.Controls["buttonLogin"]).PerformClick();
				}));
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema("user2", "pwd2")).Throws(new UnauthorizedAccessException("Access denied"));

			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(
				ShowDialogWithResult(form =>
				{
					((TextBox)form.Controls["textBoxUsername"]).Text = "user3";
					((TextBox)form.Controls["textBoxPassword"]).Text = "pwd3";
					((Button)form.Controls["buttonLogin"]).PerformClick();
				}));
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema("user3", "pwd3"));

			Mock.Get(services.CargoWiseOneInstanceClass)
				.Setup(x => x.AddNewInstance("THEONE", config.InstallationSettings.ServerName, "thedatabase", null))
				.Returns(new Mock<ICargoWiseOneInstanceEntry>(MockBehavior.Strict).Object);

			var results = new InstallationResultCollection();
			new InstanceInstaller(new Installation(config), config.InstallationSettings).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.OKCount);
		}

		public void TestInstallWithCreateSchemaRequiredWithLoginRetryCancel()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(false);
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema()).Throws(new UnauthorizedAccessException("Access denied"));

			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(
				ShowDialogWithResult(form =>
				{
					((TextBox)form.Controls["textBoxUsername"]).Text = "user1";
					((TextBox)form.Controls["textBoxPassword"]).Text = "pwd1";
					((Button)form.Controls["buttonLogin"]).PerformClick();
				}));
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema("user1", "pwd1")).Throws(new UnauthorizedAccessException("Access denied"));

			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(
				ShowDialogWithResult(form =>
				{
					((TextBox)form.Controls["textBoxUsername"]).Text = "user2";
					((TextBox)form.Controls["textBoxPassword"]).Text = "pwd2";
					((Button)form.Controls["buttonLogin"]).PerformClick();
				}));
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.CreateInCurrentSchema("user2", "pwd2")).Throws(new UnauthorizedAccessException("Access denied"));

			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(
				ShowDialogWithResult(form =>
				{
					((Button)form.Controls["buttonCancel"]).PerformClick();
				}));

			var results = new InstallationResultCollection();
			new InstanceInstaller(new Installation(config), config.InstallationSettings).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.WarningCount);
			Assert(results.GetWarningMessages().Contains("Access denied"));
		}

		public void TestInstallWithoutCreateSchemaRequiredOk()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(true);
			Mock.Get(services.CargoWiseOneInstanceClass)
				.Setup(x => x.AddNewInstance("THEONE", config.InstallationSettings.ServerName, "thedatabase", null))
				.Returns(new Mock<ICargoWiseOneInstanceEntry>(MockBehavior.Strict).Object);

			var results = new InstallationResultCollection();
			new InstanceInstaller(new Installation(config), config.InstallationSettings).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.OKCount);
		}

		public void TestInstallWithInstance()
		{
			config.InstallationSettings.SelectedDatabase = new DatabaseChoice("theinstance");
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(true);
			Mock.Get(services.CargoWiseOneInstanceClass)
				.Setup(x => x.AddNewInstance("THEONE", @"theserver\theinstance", "thedatabase", null))
				.Returns(new Mock<ICargoWiseOneInstanceEntry>(MockBehavior.Strict).Object);

			var results = new InstallationResultCollection();
			new InstanceInstaller(new Installation(config), config.InstallationSettings).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.OKCount);
		}

		public void TestInstallWithError()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(true);
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.AddNewInstance("THEONE", config.InstallationSettings.ServerName, "thedatabase", null)).Throws(new Exception("fail"));

			var results = new InstallationResultCollection();
			new InstanceInstaller(new Installation(config), config.InstallationSettings).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.WarningCount);
			AssertEquals(0, results.ErrorCount);
		}

		internal Func<Form, DialogResult> ShowDialogWithResult(Action<Form> action)
		{
			return new Func<Form, DialogResult>((Form form) =>
			{
				form.Load += (object sender, EventArgs e) =>
				{
					form.BeginInvoke(new Action(() => action(form)));
				};
				return form.ShowDialog();
			});
		}

		protected override void SetUp()
		{
			mocker = new MockRepository(MockBehavior.Loose);
			services = new MoqMockServiceContainer(mocker);
			config = new SetupConfiguration
			{
				Services = services,
				InstallationSettings =
				{
					RegisterInstance = true, InstanceName = "THEONE",
					SQLServerMachineName = "theserver",
					DbName = "thedatabase"
				}
			};
			services.CargoWiseOneInstanceClass = new Mock<ICargoWiseOneInstanceClass>(MockBehavior.Strict).Object;

			base.SetUp();
		}

		MockRepository mocker;
		MoqMockServiceContainer services;
		SetupConfiguration config;
	}
}
