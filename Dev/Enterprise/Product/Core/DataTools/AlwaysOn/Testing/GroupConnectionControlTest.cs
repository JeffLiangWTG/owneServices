using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.AlwaysOn.Setup;
using Enterprise.AlwaysOn.Setup.GUI;
using Moq;

namespace Enterprise.AlwaysOn.Testing
{
	public class GroupConnectionControlTest : AlwaysOnUITestFixture
	{
		public void TestServerNameIsCorrect()
		{
			var hostName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().HostName;
			var domainName = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;

			var failingDomainName = "wisecloud.zone";
			Assert(!domainName.EndsWith(failingDomainName));

			var serverTextBoxControl = grpConnControl.Controls.Find("dbServerTextBox", false)[0];

			// this first one will successfully make a connection, but will then fail because it's not a cluster
			// that's ok because we only need the connection to work
			serverTextBoxControl.Text = $"{hostName}.{domainName}";
			grpConnControl.connectButton_Click();
			if (testForm.errors.Count == 1)
			{
				Assert("The failure was not the type expected", testForm.errors[0].Contains("This database server instance is not enabled for AlwaysOn High Availability. Please refer to release notes for instructions on how to enable it."));
			}
			testForm.errors.Clear();

			// this one should fail to connect, but the server name it is trying to connect to should be correct
			// instead of haveing the previous domain name appended onto the end of it.
			serverTextBoxControl.Text = $@"fakeserver.{failingDomainName}\Instance1";
			grpConnControl.connectButton_Click();
			AssertEquals(1, testForm.errors.Count);
			var error = testForm.errors[0];
			AssertContains(serverTextBoxControl.Text, error, true);
		}

		public void TestLoadAvailabilityGroupStructureUpdateReplicaHealthStatus()
		{
			var secondaryReplicaMock = new Mock<IAlwaysOnReplica>();
			var availabilityGroupMock = new Mock<IAvailabilityGroup>();
			var availabilityGroupFactoryMock = new Mock<IAvailabilityGroupFactory>();
			var primaryServerMock = new Mock<IPrimaryServerInstance>();
			var alwaysOnDatabasesMock = new Mock<IAlwaysOnDatabase>();

			primaryServerMock.SetupGet(x => x.EligibleTopLevelDatabases).Returns(new [] { alwaysOnDatabasesMock.Object });
			alwaysOnDatabasesMock.Setup(x => x.GroupId).Returns(Guid.NewGuid);
			availabilityGroupMock.Setup(x => x.IsLoaded).Returns(true);
			availabilityGroupMock.Setup(x => x.Replicas).Returns(new [] { secondaryReplicaMock.Object });
			availabilityGroupFactoryMock.Setup(x => x
					.LoadAvailabilityGroupStructure(It.IsAny<SqlServerInfo>(), It.IsAny<IAlwaysOnDatabase>()))
				.Returns(availabilityGroupMock.Object);

			secondaryReplicaMock.Setup(x => x.Health).Returns(SyncronisationHealth.HEALTHY);
			secondaryReplicaMock.Setup(x => x.RefreshHealthState()).Verifiable();
			secondaryReplicaMock.Setup(x => x.GetOdysseyAdminLoginIfExists()).Verifiable();
			secondaryReplicaMock.Setup(x => x.CheckOdysseyAdminIsDbOwner(It.IsAny<IEnumerable<string>>())).Verifiable();

			AvailabilityGroupFactory.OverrideAvailabilityGroupFactory_ForTest(availabilityGroupFactoryMock.Object);

			grpConnControl.MockDbServer(primaryServerMock.Object);
			grpConnControl.MockSelectedDatabases(alwaysOnDatabasesMock.Object);

			grpConnControl.showReplicasButton_Click();

			AssertNoExceptionThrown(() =>
			{
				secondaryReplicaMock.Verify(x => x.RefreshHealthState(), Times.Once);
				secondaryReplicaMock.Verify(x => x.GetOdysseyAdminLoginIfExists(), Times.Once);
				secondaryReplicaMock.Verify(x => x.CheckOdysseyAdminIsDbOwner(It.IsAny<IEnumerable<string>>()), Times.Once);
			});
		}

		public void TestLoadAvailabilityGroupStructureUpdateReplicaHealthStatus_ReplicaIsUnHealthy()
		{
			var secondaryReplicaMock = new Mock<IAlwaysOnReplica>();
			var availabilityGroupMock = new Mock<IAvailabilityGroup>();
			var availabilityGroupFactoryMock = new Mock<IAvailabilityGroupFactory>();
			var primaryServerMock = new Mock<IPrimaryServerInstance>();
			var alwaysOnDatabasesMock = new Mock<IAlwaysOnDatabase>();

			primaryServerMock.SetupGet(x => x.EligibleTopLevelDatabases).Returns(new[] { alwaysOnDatabasesMock.Object });
			alwaysOnDatabasesMock.Setup(x => x.GroupId).Returns(Guid.NewGuid);
			availabilityGroupMock.Setup(x => x.IsLoaded).Returns(true);
			availabilityGroupMock.Setup(x => x.Replicas).Returns(new[] { secondaryReplicaMock.Object });
			availabilityGroupFactoryMock.Setup(x => x
					.LoadAvailabilityGroupStructure(It.IsAny<SqlServerInfo>(), It.IsAny<IAlwaysOnDatabase>()))
				.Returns(availabilityGroupMock.Object);

			secondaryReplicaMock.Setup(x => x.Health).Returns(SyncronisationHealth.NOT_HEALTHY);
			AvailabilityGroupFactory.OverrideAvailabilityGroupFactory_ForTest(availabilityGroupFactoryMock.Object);

			grpConnControl.MockDbServer(primaryServerMock.Object);
			grpConnControl.MockSelectedDatabases(alwaysOnDatabasesMock.Object);

			grpConnControl.showReplicasButton_Click();

			AssertNoExceptionThrown(() =>
			{
				secondaryReplicaMock.Verify(x => x.RefreshHealthState(), Times.Once);
				secondaryReplicaMock.Verify(x => x.GetOdysseyAdminLoginIfExists(), Times.Never);
				secondaryReplicaMock.Verify(x => x.CheckOdysseyAdminIsDbOwner(It.IsAny<IEnumerable<string>>()), Times.Never);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			grpConnControl = new GroupConnectionControlForTesting();
			testForm = new FormForTesting();
			testForm.Controls.Add(this.grpConnControl);
			grpConnControl.HookUiControlForm(testForm);
		}

		FormForTesting testForm;
		GroupConnectionControlForTesting grpConnControl;
	}

	class GroupConnectionControlForTesting : GroupConnectionControl
	{
		public void connectButton_Click()
		{
			connectButton_Click(null, null);
		}

		public void showReplicasButton_Click()
		{
			showReplicasButton_Click(null, null);
		}

		public void MockSelectedDatabases(IAlwaysOnDatabase alwaysOnDatabase)
		{
			RefreshDatabaseComboBox();
			databaseComboBox.SelectedItem = alwaysOnDatabase;
		}

		public void MockDbServer(IPrimaryServerInstance primaryServerInstance)
		{
			dbServer = primaryServerInstance;
		}
	}

	class FormForTesting : Form, IUiControlForm
	{
		public List<string> messages = new List<string>();
		public List<string> errors = new List<string>();

		void IUiControlForm.AppendMessage(string message)
		{
			messages.Add(message);
		}

		void IUiControlForm.OnActionCancelled()
		{
		}

		void IUiControlForm.OnActionConfirmed(ContextEnum context, params object[] eventArgs)
		{
		}

		void IUiControlForm.OnTextBoxEnter(TextBox textBox)
		{
		}

		void IUiControlForm.OnTextBoxLeave(TextBox textBox)
		{
		}

		void IUiControlForm.OnTextBoxTextChanged(TextBox textBox)
		{
		}

		void IUiControlForm.RunDatabaseProcess(Action processToRun)
		{
			processToRun();
		}

		void IUiControlForm.ShowError(string message)
		{
			errors.Add(message);
		}

		void IUiControlForm.ShowMessage(string message)
		{
			messages.Add(message);
		}
	}
}
