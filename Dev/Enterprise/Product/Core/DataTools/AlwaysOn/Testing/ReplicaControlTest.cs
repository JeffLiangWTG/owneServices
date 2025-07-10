using System;
using System.Windows.Forms;
using Enterprise.AlwaysOn.Setup;
using Enterprise.AlwaysOn.Setup.GUI;
using Enterprise.AlwaysOn.Setup.Properties;
using Moq;

namespace Enterprise.AlwaysOn.Testing
{
	public class ReplicaControlTest : AlwaysOnTestFixture
	{
		public void TestRoleLabel()
		{
			replicaMock.Setup(r => r.Role).Returns(ReplicaRole.Primary);
			replicaMock.Setup(r => r.JoinState).Returns(JoinState.JOINED_STANDALONE);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("PRIMARY", replicaControl.roleLabel_Exposed.Text);

			replicaMock.Setup(r => r.Role).Returns(ReplicaRole.Secondary);
			replicaMock.Setup(r => r.JoinState).Returns(JoinState.NOT_JOINED);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("SECONDARY", replicaControl.roleLabel_Exposed.Text);

			replicaMock.Setup(r => r.Role).Returns(ReplicaRole.Primary);
			replicaMock.Setup(r => r.JoinState).Returns(JoinState.JOINED_FAILOVER_CLUSTER_INSTANCE);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("PRIMARY (FCI)", replicaControl.roleLabel_Exposed.Text);

			replicaMock.Setup(r => r.Role).Returns(ReplicaRole.Secondary);
			replicaMock.Setup(r => r.JoinState).Returns(JoinState.JOINED_FAILOVER_CLUSTER_INSTANCE);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("SECONDARY (FCI)", replicaControl.roleLabel_Exposed.Text);
		}

		public void TestServerLabel()
		{
			var serverInfo = new SqlServerInfo(alias: "Qwert!", serverMachineName: "SomeNode", "SomeInstance", "SomeDomain.net", "SomeNode.SomeDomain.net", default);
			replicaMock.Setup(r => r.ServerInfo).Returns(serverInfo);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("Qwert!", replicaControl.serverLabel_Exposed.Text);
		}

		public void TestNodeLabelWhenPopulated()
		{
			var serverInfo = new SqlServerInfo("SomeNode\\SomeInstance", serverMachineName: "Qwerty!", "SomeInstance", "SomeDomain.net", "SomeNode.SomeDomain.net", default);
			replicaMock.Setup(r => r.ServerInfo).Returns(serverInfo);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("Node: Qwerty!", replicaControl.nodeLabel_Exposed.Text);
		}

		public void TestNodeLabelWhenEmptyAndFCI()
		{
			var serverInfo = new SqlServerInfo("SomeNode\\SomeInstance", serverMachineName: string.Empty, "SomeInstance", "SomeDomain.net", "SomeNode.SomeDomain.net", default);
			replicaMock.Setup(r => r.ServerInfo).Returns(serverInfo);
			replicaMock.Setup(r => r.JoinState).Returns(JoinState.JOINED_FAILOVER_CLUSTER_INSTANCE);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("Failover Cluster Instance", replicaControl.nodeLabel_Exposed.Text);
		}

		public void TestNodeLabelWhenEmptyAndNotFCI()
		{
			var serverInfo = new SqlServerInfo("SomeNode\\SomeInstance", serverMachineName: string.Empty, "SomeInstance", "SomeDomain.net", "SomeNode.SomeDomain.net", default);
			replicaMock.Setup(r => r.ServerInfo).Returns(serverInfo);
			replicaMock.Setup(r => r.JoinState).Returns(JoinState.NOT_JOINED);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("", replicaControl.nodeLabel_Exposed.Text);
		}

		public void TestAvailabilityModeLabel()
		{
			replicaOptionsMock.Setup(o => o.CommitMode).Returns(AvailabilityMode.ASYNCHRONOUS_COMMIT);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("ASYNCHRONOUS_COMMIT", replicaControl.availabilityModeLabel_Exposed.Text);

			replicaOptionsMock.Setup(o => o.CommitMode).Returns(AvailabilityMode.SYNCHRONOUS_COMMIT);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("SYNCHRONOUS_COMMIT", replicaControl.availabilityModeLabel_Exposed.Text);
		}

		public void TestFailoverModeLabel()
		{
			replicaOptionsMock.Setup(o => o.Failover).Returns(FailoverMode.AUTOMATIC);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("Failover: AUTOMATIC", replicaControl.failoverModeLabel_Exposed.Text);

			replicaOptionsMock.Setup(o => o.Failover).Returns(FailoverMode.MANUAL);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("Failover: MANUAL", replicaControl.failoverModeLabel_Exposed.Text);
		}

		public void TestAllowConnectionLabel()
		{
			replicaMock.Setup(r => r.Role).Returns(ReplicaRole.Secondary);
			replicaOptionsMock.Setup(o => o.SecondaryAllowConnection).Returns(AllowConnection.READ_WRITE);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("Allow Connections: READ_WRITE", replicaControl.allowConnectionLabel_Exposed.Text);

			replicaMock.Setup(r => r.Role).Returns(ReplicaRole.Primary);
			replicaOptionsMock.Setup(o => o.SecondaryAllowConnection).Returns(AllowConnection.READ_WRITE);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("Allow Connections: ALL", replicaControl.allowConnectionLabel_Exposed.Text);

			replicaMock.Setup(r => r.Role).Returns(ReplicaRole.Resolving);
			replicaOptionsMock.Setup(o => o.SecondaryAllowConnection).Returns(AllowConnection.READ_WRITE);
			replicaControl.RefreshReplicaLabels();
			AssertEquals("Allow Connections: ALL", replicaControl.allowConnectionLabel_Exposed.Text);
		}

		public void TestHealthPanelBackgroundImage()
		{
			replicaMock.Setup(r => r.Health).Returns(SyncronisationHealth.HEALTHY);
			replicaControl.RefreshReplicaLabels();
			AssertEquals(Setup.Properties.Resources.SmileyHappy24.RawFormat, replicaControl.healthPanel_Exposed.BackgroundImage.RawFormat);

			replicaMock.Setup(r => r.Health).Returns(SyncronisationHealth.PARTIALLY_HEALTHY);
			replicaControl.RefreshReplicaLabels();
			AssertEquals(Setup.Properties.Resources.SmileyNeutral24.RawFormat, replicaControl.healthPanel_Exposed.BackgroundImage.RawFormat);

			replicaMock.Setup(r => r.Health).Returns(SyncronisationHealth.NOT_HEALTHY);
			replicaControl.RefreshReplicaLabels();
			AssertEquals(Setup.Properties.Resources.SmileySad24.RawFormat, replicaControl.healthPanel_Exposed.BackgroundImage.RawFormat);
		}

		public void TestReplicaControlShowSyncHealthStatus()
		{
			foreach (var role in new[] { ReplicaRole.Primary, ReplicaRole.Secondary })
			{
				foreach (var healthStatus in new[] { SyncronisationHealth.HEALTHY, SyncronisationHealth.NOT_HEALTHY, SyncronisationHealth.PARTIALLY_HEALTHY })
				{
					replicaMock.Invocations.Clear();

					replicaMock.Setup(x => x.IsPrimary).Returns(role == ReplicaRole.Primary);
					replicaMock.Setup(x => x.Role).Returns(role);
					replicaMock.Setup(x => x.Health).Returns(healthStatus);

					replicaControl.SetAlwaysOnReplica(replicaMock.Object);

					var expectedHealthIconImage = healthStatus == SyncronisationHealth.HEALTHY ? Resources.SmileyHappy24.RawFormat : Resources.SmileySad24.RawFormat;
					AssertEquals(replicaControl.healthPanel_Exposed.BackgroundImage.RawFormat, expectedHealthIconImage);
					AssertEquals(replicaControl.pendingActionsLink_Exposed.Visible, healthStatus != SyncronisationHealth.HEALTHY);
					AssertEquals(replicaControl.healthPanel_Exposed.Enabled, healthStatus != SyncronisationHealth.HEALTHY);

					var tooltip = replicaControl.toolTipService.GetToolTip(replicaControl.pendingActionsLink_Exposed);
					if (healthStatus == SyncronisationHealth.HEALTHY)
					{
						Assert(string.IsNullOrEmpty(tooltip));
					}
					else
					{
						Assert(tooltip.Contains(ReplicaControl.ReplicaHealthIssues));
						Assert(tooltip.Contains(ReplicaControl.CheckAddReplicaDatabasesAction));
					}
				}
			}
		}

		public void TestReplicaControlShowsLastError()
		{
			// Arrange
			replicaMock.Invocations.Clear();
			replicaMock.Setup(x => x.LastErrorMessage).Returns("Last Error!");
			replicaControl.SetAlwaysOnReplica(replicaMock.Object);

			// Act
			var tooltip = replicaControl.toolTipService.GetToolTip(replicaControl.pendingActionsLink_Exposed);

			// Assert
			Assert(tooltip.Contains("=> Last Error!"));
		}

		public void TestReplicaControlShowSecondaryIssuesAndPendingActions()
		{
			var messageBoxMock = new Mock<IMessageBox>();
			using (MessageDialog.OverrideMessageBox_ForTest(messageBoxMock.Object))
			{
				foreach (var issues in new[]
				{
					SecondaryReplicaIssues.None,
					SecondaryReplicaIssues.SynchronizationNotHealthy,
					SecondaryReplicaIssues.OdysseyAdminSidMismatch,
					SecondaryReplicaIssues.OdysseyAdminNotDbOwner,
					SecondaryReplicaIssues.SynchronizationNotHealthy | SecondaryReplicaIssues.OdysseyAdminSidMismatch,
					SecondaryReplicaIssues.SynchronizationNotHealthy | SecondaryReplicaIssues.OdysseyAdminNotDbOwner,
					SecondaryReplicaIssues.SynchronizationNotHealthy | SecondaryReplicaIssues.OdysseyAdminSidMismatch | SecondaryReplicaIssues.OdysseyAdminNotDbOwner,
					SecondaryReplicaIssues.OdysseyAdminSidMismatch | SecondaryReplicaIssues.OdysseyAdminNotDbOwner,
				})
				{
					messageBoxMock.Invocations.Clear();
					replicaMock.Invocations.Clear();

					replicaMock.Setup(x => x.IsPrimary).Returns(false);
					replicaMock.Setup(x => x.Role).Returns(ReplicaRole.Secondary);
					replicaMock.Setup(x => x.Health).Returns(
						issues == SecondaryReplicaIssues.None
						|| (issues & SecondaryReplicaIssues.SynchronizationNotHealthy) != SecondaryReplicaIssues.SynchronizationNotHealthy
							? SyncronisationHealth.HEALTHY
							: SyncronisationHealth.NOT_HEALTHY);
					replicaMock.Setup(x => x.IsOdysseyAdminLoginSidDifferentFromPrimary).Returns(
						(issues & SecondaryReplicaIssues.OdysseyAdminSidMismatch) == SecondaryReplicaIssues.OdysseyAdminSidMismatch);
					replicaMock.Setup(x => x.IsOdysseyAdminDbOwner).Returns(
						(issues & SecondaryReplicaIssues.OdysseyAdminNotDbOwner) != SecondaryReplicaIssues.OdysseyAdminNotDbOwner);
					replicaMock.Setup(x => x.RequiresManualFailOver).Returns(
						(issues & SecondaryReplicaIssues.OdysseyAdminNotDbOwner) == SecondaryReplicaIssues.OdysseyAdminNotDbOwner);
					replicaMock.Setup(x => x.IsAvailableForManualFailOver).Returns(
						issues == SecondaryReplicaIssues.OdysseyAdminNotDbOwner);

					replicaControl.SetAlwaysOnReplica(replicaMock.Object);

					var expectedHealthIconImage = issues == SecondaryReplicaIssues.None ? Resources.SmileyHappy24.RawFormat : Resources.SmileySad24.RawFormat;
					AssertEquals(replicaControl.healthPanel_Exposed.BackgroundImage.RawFormat, expectedHealthIconImage);

					AssertEquals(replicaControl.pendingActionsLink_Exposed.Visible, issues != SecondaryReplicaIssues.None);
					AssertEquals(replicaControl.healthPanel_Exposed.Enabled, issues != SecondaryReplicaIssues.None);

					var tooltip = replicaControl.toolTipService.GetToolTip(replicaControl.pendingActionsLink_Exposed);
					if (issues == SecondaryReplicaIssues.None)
					{
						Assert(string.IsNullOrEmpty(tooltip));
					}
					else
					{
						if ((issues & SecondaryReplicaIssues.SynchronizationNotHealthy) == SecondaryReplicaIssues.SynchronizationNotHealthy)
						{
							Assert(tooltip.Contains(ReplicaControl.ReplicaHealthIssues));
							Assert(tooltip.Contains(ReplicaControl.CheckAddReplicaDatabasesAction));

							Assert(!tooltip.Contains(ReplicaControl.OdysseyAdminLoginIsNotDbOwner));
							Assert(!tooltip.Contains(ReplicaControl.ManualFailoverAction));
						}

						if ((issues & SecondaryReplicaIssues.OdysseyAdminSidMismatch) == SecondaryReplicaIssues.OdysseyAdminSidMismatch)
						{
							Assert(tooltip.Contains(ReplicaControl.OdysseyAdminLoginDifferentFromPrimaryReplica));
							Assert(tooltip.Contains(ReplicaControl.CopyLoginsFromPrimaryReplicaAction));

							Assert(!tooltip.Contains(ReplicaControl.OdysseyAdminLoginIsNotDbOwner));
							Assert(!tooltip.Contains(ReplicaControl.ManualFailoverAction));
						}

						if (issues == SecondaryReplicaIssues.OdysseyAdminNotDbOwner)
						{
							Assert(tooltip.Contains(ReplicaControl.OdysseyAdminLoginIsNotDbOwner));
							Assert(tooltip.Contains(ReplicaControl.ManualFailoverAction));
						}

						replicaControl.PerformPendingActionsLinkClick(this, EventArgs.Empty);
						messageBoxMock.Verify(x =>
							x.Show(It.Is<string>(text => text.Contains(tooltip)), ReplicaControl.ReplicaHealthIssuesCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation),
							Times.Once);
					}
				}
			}
		}

		public void TestReplicaControlHandlesHealthStatusChangedEvent()
		{
			var alwaysOnReplicaMock = new Mock<AlwaysOnReplica>(
				ReplicaRole.Secondary,
				SyncronisationHealth.HEALTHY,
				JoinState.JOINED_FAILOVER_CLUSTER_INSTANCE,
				replicaOptionsMock.Object,
				Guid.NewGuid(),
				new AvailabilityGroupForTesting(),
				new SqlServerInfo("SomeNode\\SomeInstance"),
				"someEndPointAddress")
			{ CallBase = true };

			var mock = alwaysOnReplicaMock.As<IAlwaysOnReplica>();

			mock.Setup(x => x.IsOdysseyAdminLoginSidDifferentFromPrimary).Returns(false).Verifiable();
			mock.Setup(x => x.Health).Returns(SyncronisationHealth.HEALTHY).Verifiable();
			mock.Setup(x => x.RequiresManualFailOver).Returns(true).Verifiable();
			mock.Setup(x => x.IsAvailableForManualFailOver).Returns(true).Verifiable();

			replicaControl.SetAlwaysOnReplica(mock.Object);
			mock.Raise(x => x.HealthStatusChanged += (sender, args) => { }, alwaysOnReplicaMock.Object, EventArgs.Empty);

			AssertNoExceptionThrown(() =>
			{
				mock.Verify();
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var serverInfo = new SqlServerInfo("SomeNode\\SomeInstance", "SomeNode", "SomeInstance", "SomeDomain.net", "SomeNode.SomeDomain.net", default);

			replicaMock = new Mock<IAlwaysOnReplica>();
			replicaOptionsMock = new Mock<IReplicaOptions>();
			replicaMock.Setup(r => r.Role).Returns(ReplicaRole.Primary);
			replicaMock.Setup(r => r.Health).Returns(SyncronisationHealth.HEALTHY);
			replicaMock.Setup(r => r.JoinState).Returns(JoinState.JOINED_FAILOVER_CLUSTER_INSTANCE);
			replicaMock.Setup(r => r.ReplicaId).Returns(Guid.NewGuid());
			replicaMock.Setup(r => r.ParentGroup).Returns(new AvailabilityGroupForTesting());
			replicaMock.Setup(r => r.ServerInfo).Returns(serverInfo);
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
	}

	[Flags]
	enum SecondaryReplicaIssues
	{
		None = 0x0,
		SynchronizationNotHealthy = 0x1,
		OdysseyAdminSidMismatch = 0x2,
		OdysseyAdminNotDbOwner = 0x4,
	}

	class ReplicaControlForTesting : ReplicaControl
	{
		public Panel databasePanel_Exposed => databasePanel;
		public Panel healthPanel_Exposed => healthPanel;
		public Label roleLabel_Exposed => roleLabel;
		public Label serverLabel_Exposed => serverLabel;
		public Label availabilityModeLabel_Exposed => availabilityModeLabel;
		public Label nodeLabel_Exposed => nodeLabel;
		public Label failoverModeLabel_Exposed => failoverModeLabel;
		public Label allowConnectionLabel_Exposed => allowConnectionLabel;
		public LinkLabel pendingActionsLink_Exposed => pendingActionsLink;

		public void PerformPendingActionsLinkClick(object sender, EventArgs e)
		{
			PendingActionsLinkOnClick(sender, e);
		}
	}
}
