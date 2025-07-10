using System;
using System.Windows.Forms;
using CargoWise.Async;
using Enterprise.AlwaysOn.Setup;
using Enterprise.AlwaysOn.Setup.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	public class ChangeReplicaSettingControlTest : TestCase
	{
		public void TestChangingAllowConnectionsComboBoxIsEnabledWhenAllowConnectionsIsNotAll()
		{
			var allowConnectionsComboBox = (ComboBox)changeReplicaSettingControl.Controls.Find("allowConnectionsComboBox", false)[0];

			allowConnectionsComboBox.SelectedIndex = 0;
			AssertEquals("control should be enabled to users when allow connections is NOT All", true, allowConnectionsComboBox.Enabled);
			AssertNotEquals("All", allowConnectionsComboBox.Text);

			allowConnectionsComboBox.SelectedIndex = 1;
			AssertEquals("control should be disabled to users when allow connections is set to All", false, allowConnectionsComboBox.Enabled);
			AssertEquals("All", allowConnectionsComboBox.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var serverInfo = new SqlServerInfo("SomeNode", "SomeNode", "SomeInstance", "SomeDoamin", "SomeNode.SomeDomain", 1433);
			var replicaMock = new Mock<IAlwaysOnReplica>();
			_ = replicaMock.Setup(r => r.Role).Returns(ReplicaRole.Secondary);
			_ = replicaMock.Setup(r => r.Health).Returns(SyncronisationHealth.HEALTHY);
			_ = replicaMock.Setup(r => r.JoinState).Returns(JoinState.JOINED_FAILOVER_CLUSTER_INSTANCE);
			_ = replicaMock.Setup(r => r.ReplicaId).Returns(Guid.NewGuid());
			_ = replicaMock.Setup(r => r.ParentGroup).Returns(Mock.Of<IAvailabilityGroup>());
			_ = replicaMock.Setup(r => r.ServerInfo).Returns(serverInfo);
			_ = replicaMock.Setup(r => r.ServerAddressFromEndpointUrl).Returns("TCP://10.10.10.100:5024");
			_ = replicaMock.Setup(r => r.Options).Returns(Mock.Of<IReplicaOptions>());

			changeReplicaSettingControl = new ChangeReplicaSettingControl();
			dummyForm = new DummyForm(changeReplicaSettingControl);

			var replicaControl = new ReplicaControlForTesting();
			replicaControl.SetAlwaysOnReplica(replicaMock.Object);
			changeReplicaSettingControl.Reset(replicaControl);
		}

		protected override void TearDown()
		{
			dummyForm.Dispose();
			AsyncHelper.WaitAllActiveTasksForTest();
			base.TearDown();
		}

		DummyForm dummyForm;
		ChangeReplicaSettingControl changeReplicaSettingControl;

		class DummyForm : MainForm
		{
			public DummyForm(Control controlToTest)
			{
				Controls.Add(controlToTest);
				((IParentFormHook)controlToTest).HookUiControlForm(this);

				Controls.Add(controlToTest);
			}
		}
	}
}
