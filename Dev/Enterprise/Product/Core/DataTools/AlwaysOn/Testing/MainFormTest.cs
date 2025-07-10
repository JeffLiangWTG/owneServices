using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.AlwaysOn.Setup;
using Enterprise.AlwaysOn.Setup.GUI;
using Moq;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Testing
{
	class MainFormTest : AlwaysOnUITestFixture
	{
		public void TestManualFailOverMenuDisabled_WhenReplicaIsNotAvailableForManualFailOver()
		{
			foreach (var replicaIsAvailableForManualFailOver in new[] { true, false })
			{
				replicaMock.Invocations.Clear();
				replicaMock.Setup(x => x.Role).Returns(ReplicaRole.Secondary);
				replicaMock.Setup(x => x.IsAvailableForManualFailOver).Returns(replicaIsAvailableForManualFailOver);
				replicaControl.SetAlwaysOnReplica(replicaMock.Object);

				var replicaMenu = new ContextMenu();
				using (var mainForm = new MainForm())
				{
					mainForm.AddReplicaContextMenuOptions_ForTest(replicaMenu, replicaControl);
				}

				var failOverMenuItem = replicaMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text.Equals("Perform Planned Manual Failover", StringComparison.OrdinalIgnoreCase));
				AssertNotNull(failOverMenuItem);
				AssertEquals(failOverMenuItem?.Enabled, replicaIsAvailableForManualFailOver);
			}
		}

		public void TestCopyLoginsFromPrimaryReplicaMenuClick()
		{
			replicaMock.Invocations.Clear();

			var messageBoxMock = new Mock<IMessageBox>();
			var groupDatabases = new List<string> { "testDatabase" };
			var expectedMessage = Invariant($@"Please right-click on the replica [{replicaControl.AvailabilityReplica.ServerInfo.ServerAlias}] and select 'Perform Planned Manual Failover' menu to finalize the synchronization of admin login with the primary replica.");

			availabilityGroupMock.Setup(x => x.Databases).Returns(groupDatabases);
			replicaMock.Setup(x => x.HasErrors).Returns(false);
			replicaMock.Setup(x => x.RequiresManualFailOver).Returns(true);
			replicaMock.Setup(x => x.IsOdysseyAdminLoginSidDifferentFromPrimary).Returns(false);

			using (MessageDialog.OverrideMessageBox_ForTest(messageBoxMock.Object))
			using (var mainForm = new MainForm())
			{
				mainForm.PerformCopyLoginsFromPrimaryReplicaMenuClick_ForTest(replicaControl);
			}

			AssertNoExceptionThrown(() =>
			{
				replicaMock.Verify(x => x.RefreshHealthState(), Times.Once);
				replicaMock.Verify(x => x.GetOdysseyAdminLoginIfExists(), Times.Once);
				replicaMock.Verify(x => x.CheckOdysseyAdminIsDbOwner(groupDatabases), Times.Once);

				messageBoxMock.Verify(x =>
						x.Show(expectedMessage, "Manual failover is required as soon as possible", MessageBoxButtons.OK, MessageBoxIcon.Exclamation),
					Times.Once);
			});
		}

		public void TestUpdateReplicaHealthStatus()
		{
			replicaMock.Setup(x => x.RefreshHealthState()).Verifiable();
			replicaMock.Setup(x => x.GetOdysseyAdminLoginIfExists()).Verifiable();
			replicaMock.Setup(x => x.CheckOdysseyAdminIsDbOwner(It.IsAny<IEnumerable<string>>())).Verifiable();
			TestUpdateReplicaHealthStatusCore(() =>
			{
				replicaMock.Verify(x => x.RefreshHealthState(), Times.Once);
				replicaMock.Verify(x => x.GetOdysseyAdminLoginIfExists(), Times.Once);
				replicaMock.Verify(x => x.CheckOdysseyAdminIsDbOwner(It.IsAny<IEnumerable<string>>()), Times.Once);
			});
		}

		public void TestUpdateReplicaHealthStatus_Unhealthy()
		{
			replicaMock.Setup(x => x.Health).Returns(SyncronisationHealth.NOT_HEALTHY);
			replicaMock.Setup(x => x.RefreshHealthState()).Verifiable();
			replicaMock.Setup(x => x.GetOdysseyAdminLoginIfExists()).Verifiable();
			replicaMock.Setup(x => x.CheckOdysseyAdminIsDbOwner(It.IsAny<IEnumerable<string>>())).Verifiable();
			TestUpdateReplicaHealthStatusCore(() =>
			{
				replicaMock.Verify(x => x.RefreshHealthState(), Times.Once);
				replicaMock.Verify(x => x.GetOdysseyAdminLoginIfExists(), Times.Never);
				replicaMock.Verify(x => x.CheckOdysseyAdminIsDbOwner(It.IsAny<IEnumerable<string>>()), Times.Never);
			});
		}

		void TestUpdateReplicaHealthStatusCore(Action assertions)
		{
			var form = new MainFormForTest();
			form.UpdateReplicaHealthStatusForTest(replicaMock.Object);
			AssertNoExceptionThrown(() => assertions());
		}

		protected override void SetUp()
		{
			base.SetUp();

			var serverInfo = new SqlServerInfo("SomeNode\\SomeInstance", "SomeNode", "SomeInstance", "SomeDomain.net", "SomeNode.SomeDomain.net", default);

			replicaMock = new Mock<IAlwaysOnReplica>();
			replicaOptionsMock = new Mock<IReplicaOptions>();
			availabilityGroupMock = new Mock<IAvailabilityGroup>();

			replicaMock.Setup(r => r.Role).Returns(ReplicaRole.Primary);
			replicaMock.Setup(r => r.Health).Returns(SyncronisationHealth.HEALTHY);
			replicaMock.Setup(r => r.JoinState).Returns(JoinState.JOINED_FAILOVER_CLUSTER_INSTANCE);
			replicaMock.Setup(r => r.ReplicaId).Returns(Guid.NewGuid());
			replicaMock.Setup(r => r.ServerInfo).Returns(serverInfo);
			replicaMock.Setup(r => r.ParentGroup).Returns(availabilityGroupMock.Object);
			replicaMock.Setup(r => r.ServerAddressFromEndpointUrl).Returns("TCP://10.10.10.100:5024");

			replicaMock.Setup(r => r.Options).Returns(replicaOptionsMock.Object);
			replicaOptionsMock.Setup(o => o.CommitMode).Returns(AvailabilityMode.ASYNCHRONOUS_COMMIT);
			replicaOptionsMock.Setup(o => o.Failover).Returns(FailoverMode.AUTOMATIC);
			replicaOptionsMock.Setup(o => o.SecondaryAllowConnection).Returns(AllowConnection.ALL);
			replicaOptionsMock.Setup(o => o.SecondaryReadOnlyRoutingUrl).Returns("helloworld://hi");

			replicaControl = new ReplicaControlForTesting();
			replicaControl.SetAlwaysOnReplica(replicaMock.Object);
		}

		ReplicaControlForTesting replicaControl;
		Mock<IAlwaysOnReplica> replicaMock;
		Mock<IReplicaOptions> replicaOptionsMock;
		Mock<IAvailabilityGroup> availabilityGroupMock;

		class MainFormForTest : MainForm
		{
			public void UpdateReplicaHealthStatusForTest(IAlwaysOnReplica replica)
			{
				UpdateReplicaHealthStatus(replica);
			}
		}
	}
}
