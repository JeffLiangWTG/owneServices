using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ActiveDirectory;
using CargoWise.Loader.Common;
using Moq;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	[GuiTest]
	public class ConfigurationCheckerTest : TestCase
	{
		public void TestNeedsToInstall()
		{
			var config = new EnterpriseConfiguration();
			AssertEquals(true, new ConfigurationChecker(new Installation(config)).NeedsToInstall());

			config = new EnterpriseConfiguration();
			config.Initialize(new string[] { "-instance:instance" });
			AssertEquals(true, new ConfigurationChecker(new Installation(config)).NeedsToInstall());

			config = new EnterpriseConfiguration();
			config.Initialize(new string[] { "servername", "databasename" });
			AssertEquals(false, new ConfigurationChecker(new Installation(config)).NeedsToInstall());
		}

		public void TestInstanceResolved()
		{
			config.Initialize(new string[] { "-instance:mycw1" });
			var entry = new Mock<ICargoWiseOneInstanceSearchResult>();
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.FindInstance("mycw1", default)).Returns(entry.Object);
			entry.SetupGet(x => x.ServerName).Returns("myserver");
			entry.SetupGet(x => x.DatabaseName).Returns("mydb");
			var results = new InstallationResultCollection();
			new ConfigurationChecker(new Installation(config)).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.OKCount);
			AssertEquals("myserver", config.ServerName);
			AssertEquals("mydb", config.DatabaseName);
		}

		public void TestInstanceNotResolved()
		{
			config.Initialize(new string[] { "-instance:mycw1" });
			Mock.Get(services.CargoWiseOneInstanceClass)
				.Setup(x => x.FindInstance("mycw1", default))
				.Returns((ICargoWiseOneInstanceSearchResult)null);
			var results = new InstallationResultCollection();
			new ConfigurationChecker(new Installation(config)).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.ErrorCount);
			Assert(results.GetErrorMessages().Contains("The specified instance \"mycw1\" could not be found on the current domain."));
		}

		public void TestConfigurationDialogDatabaseOk()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(false);
			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(services.ShowDialogWithResult((Form form) =>
				{
					var dialog = (ConfigurationDialog)form;

					Assert(!dialog.serverNameLabel.Visible);
					Assert(!dialog.serverNameTextBox.Visible);
					Assert(!dialog.databaseNameLabel.Visible);
					Assert(!dialog.databaseNameTextBox.Visible);
					var comboBox = dialog.comboBoxInstance;
					comboBox.SelectedItem = comboBox.Items.Cast<object>().Single(item => item.ToString() == "Connect to Database...");
					Assert(dialog.serverNameLabel.Visible);
					Assert(dialog.serverNameTextBox.Visible);
					Assert(dialog.databaseNameLabel.Visible);
					Assert(dialog.databaseNameTextBox.Visible);

					dialog.serverNameTextBox.Text = "aserver";
					dialog.databaseNameTextBox.Text = "adatabase";
					dialog.connectButton.PerformClick();
				}));
			var results = new InstallationResultCollection();
			new ConfigurationChecker(new Installation(config)).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.OKCount);
			AssertEquals("aserver", config.ServerName);
			AssertEquals("adatabase", config.DatabaseName);
		}

		public void TestConfigurationDialogDatabaseValidation()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(false);
			Mock.Get(services.MessageBox).Setup(m => m.Show(null, "Server and database name values must be provided", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)).Returns(DialogResult.OK);
			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(services.ShowDialogWithResult((Form form) =>
			{
				var dialog = (ConfigurationDialog)form;

				Assert(!dialog.serverNameLabel.Visible);
				Assert(!dialog.serverNameTextBox.Visible);
				Assert(!dialog.databaseNameLabel.Visible);
				Assert(!dialog.databaseNameTextBox.Visible);
				var comboBox = dialog.comboBoxInstance;
				comboBox.SelectedItem = comboBox.Items.Cast<object>().Single(item => item.ToString() == "Connect to Database...");
				Assert(dialog.serverNameLabel.Visible);
				Assert(dialog.serverNameTextBox.Visible);
				Assert(dialog.databaseNameLabel.Visible);
				Assert(dialog.databaseNameTextBox.Visible);

				dialog.connectButton.PerformClick();
				form.Close();
			}));
			var results = new InstallationResultCollection();
			new ConfigurationChecker(new Installation(config)).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.ErrorCount);
		}

		public void TestConfigurationDialogInstanceOk()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(true);
			var productionInstance = new Mock<ICargoWiseOneInstanceSearchResult>();
			productionInstance.SetupGet(item => item.Name).Returns("prod");
			productionInstance.SetupGet(item => item.ServerName).Returns("prodserver");
			productionInstance.SetupGet(item => item.DatabaseName).Returns("proddb");
			var testInstance = new Mock<ICargoWiseOneInstanceSearchResult>();
			testInstance.SetupGet(item => item.Name).Returns("test");
			var instances = new Mock<IDirectorySearchResults<ICargoWiseOneInstanceSearchResult>>(MockBehavior.Strict);
			instances.Setup(x => x.GetEnumerator()).Returns(new List<ICargoWiseOneInstanceSearchResult>(new[] { productionInstance.Object, testInstance.Object }).GetEnumerator());
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.FindAllInstances(default)).Returns(instances.Object);
			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(services.ShowDialogWithResult((Form form) =>
			{
				var dialog = (ConfigurationDialog)form;

				Assert(!dialog.serverNameLabel.Visible);
				Assert(!dialog.serverNameTextBox.Visible);
				Assert(!dialog.databaseNameLabel.Visible);
				Assert(!dialog.databaseNameTextBox.Visible);
				var comboBox = dialog.comboBoxInstance;
				AssertEquals("prod", comboBox.Items[0].ToString());
				AssertEquals("test", comboBox.Items[1].ToString());
				comboBox.SelectedItem = comboBox.Items[0];
				dialog.connectButton.PerformClick();
			}));
			var results = new InstallationResultCollection();
			new ConfigurationChecker(new Installation(config)).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.OKCount);
			AssertEquals("prod", config.InstanceName);
			AssertEquals("prodserver", config.ServerName);
			AssertEquals("proddb", config.DatabaseName);
		}

		public void TestConfigurationDialogNoInstance()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(true);
			var instances = mocker.Create<IDirectorySearchResults<ICargoWiseOneInstanceSearchResult>>();
			instances.Setup(m => m.GetEnumerator()).Returns(new List<ICargoWiseOneInstanceSearchResult>().GetEnumerator());
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.FindAllInstances(default)).Returns(instances.Object);
			Mock.Get(services.MessageBox).Setup(m => m.Show(null, "Select a valid instance", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)).Returns(DialogResult.OK);
			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.IsAny<Form>())).Returns(services.ShowDialogWithResult((Form form) =>
			{
				var dialog = (ConfigurationDialog)form;

				var comboBox = dialog.comboBoxInstance;
				dialog.connectButton.PerformClick();
				form.Close();
			}));
			var results = new InstallationResultCollection();
			new ConfigurationChecker(new Installation(config)).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.ErrorCount);
		}

		public void TestConfigurationDialogManageAddInstance()
		{
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.ExistsInCurrentSchema()).Returns(true);
			var instances = mocker.Create<IDirectorySearchResults<ICargoWiseOneInstanceSearchResult>>();
			instances.Setup(m => m.GetEnumerator()).Returns(new List<ICargoWiseOneInstanceSearchResult>().GetEnumerator());
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.FindAllInstances(default)).Returns(instances.Object);
			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.Is<Form>(f => f.Name == "ConfigurationDialog")))
				.Returns(services.ShowDialogWithResult((Form form) =>
				{
					var dialog = (ConfigurationDialog)form;

					var comboBox = dialog.comboBoxInstance;
					comboBox.SelectedItem = comboBox.Items.Cast<object>().Single(item => item.ToString() == "Manage Instances...");
					AssertEquals("", comboBox.Text);
					AssertEquals("new", comboBox.Items[0].ToString());
					comboBox.SelectedItem = comboBox.Items[0];
					dialog.connectButton.PerformClick();
				}));

			var newInstance = new Mock<ICargoWiseOneInstanceEntry>(MockBehavior.Strict);
			newInstance.SetupGet(item => item.Name).Returns("new");
			newInstance.As<ICargoWiseOneInstanceSearchResult>().SetupGet(item => item.ServerName).Returns("newserver");
			newInstance.As<ICargoWiseOneInstanceSearchResult>().SetupGet(item => item.DatabaseName).Returns("newdatabase");
			Mock.Get(services.CargoWiseOneInstanceClass).Setup(x => x.AddNewInstance("new", "newserver", "newdatabase", null)).Returns(newInstance.Object);
			Mock.Get(services.MessageBox).Setup(m => m.ShowDialog(It.Is<Form>(f => f.Name == "InstanceManagerForm")))
				.Returns(services.ShowDialogWithResult((Form form) =>
				{
					System.Windows.Forms.Application.DoEvents();
					form.Controls["itemGroupBox"].Controls["textBoxName"].Text = "new";
					form.Controls["itemGroupBox"].Controls["textBoxServerName"].Text = "newserver";
					form.Controls["itemGroupBox"].Controls["textBoxDatabaseName"].Text = "newdatabase";
					((Button)form.Controls["itemGroupBox"].Controls["buttonSaveItem"]).PerformClick();
					form.Close();
				}));

			var results = new InstallationResultCollection();

			new ConfigurationChecker(new Installation(config)).Install(results);
			mocker.VerifyAll();
			AssertEquals(1, results.OKCount);
			AssertEquals("new", config.InstanceName);
			AssertEquals("newserver", config.ServerName);
			AssertEquals("newdatabase", config.DatabaseName);
		}

		protected override void SetUp()
		{
			mocker = new MockRepository(MockBehavior.Loose);
			services = new MockServiceContainer(mocker);
			services.PopulateWithRealServices();
			services.MessageBox = mocker.Create<IMessageBoxProxy>(MockBehavior.Strict).Object;
			services.CargoWiseOneInstanceClass = new Mock<ICargoWiseOneInstanceClass>(MockBehavior.Strict).Object;
			config = new EnterpriseConfiguration();
			config.Services = services;
			base.SetUp();
		}

		MockRepository mocker;
		MockServiceContainer services;
		EnterpriseConfiguration config;
	}
}
