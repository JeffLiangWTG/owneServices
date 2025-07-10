using System.Windows.Forms;
using CargoWise.Async;
using Enterprise.AlwaysOn.Setup;
using Enterprise.AlwaysOn.Setup.GUI;
using Moq;

namespace Enterprise.AlwaysOn.Testing
{
	public class AddNewSecondaryReplicaControlTest : AlwaysOnTestFixture
	{
		public void TestInstanceNameCanBeBlank()
		{
			var serverInstanceNameTextBoxControl = (TextBox)addNewSecondaryReplicaControl.Controls.Find("serverInstanceNameTextBox", false)[0];
			var replicaNodeDropDownControl = (ComboBox)addNewSecondaryReplicaControl.Controls.Find("replicaNodeDropDown", false)[0];

			replicaNodeDropDownControl.SelectedIndex = 1;

			AssertEquals(serverInstanceNameTextBoxControl.Tag.ToString(), serverInstanceNameTextBoxControl.Text);
			AssertEquals(System.Drawing.SystemColors.GradientInactiveCaption, serverInstanceNameTextBoxControl.BackColor);

			serverInstanceNameTextBoxControl.Select();
			serverInstanceNameTextBoxControl.Text = "MyInstance";
			replicaNodeDropDownControl.Select();

			AssertEquals("MyInstance", serverInstanceNameTextBoxControl.Text);
			AssertEquals(System.Drawing.SystemColors.GradientInactiveCaption, serverInstanceNameTextBoxControl.BackColor);
			AssertEquals("MyServer\\MyInstance", addNewSecondaryReplicaControl.GetServerName_Exposed());

			serverInstanceNameTextBoxControl.Select();
			serverInstanceNameTextBoxControl.Text = "";
			replicaNodeDropDownControl.Select();

			AssertEquals(serverInstanceNameTextBoxControl.Tag.ToString(), serverInstanceNameTextBoxControl.Text);
			AssertEquals(System.Drawing.SystemColors.GradientInactiveCaption, serverInstanceNameTextBoxControl.BackColor);
			AssertEquals("MyServer", addNewSecondaryReplicaControl.GetServerName_Exposed());
		}

		public void TestAllowConnectionsComboBoxDefaultBehaviours()
		{
			var replicaOptionsGroupBox = (GroupBox)addNewSecondaryReplicaControl.Controls.Find("replicaOptionsGroupBox", false)[0];
			var allowConnectionsComboBox = (ComboBox)replicaOptionsGroupBox.Controls.Find("allowConnectionsComboBox", false)[0];

			CombineAssertions("Connections to secondary replicas would be allowed to 'All' and not configurable to users", () =>
			{
				AssertEquals("default allow all connections", "All", allowConnectionsComboBox.Text);
				AssertEquals("control should be disabled to users", false, allowConnectionsComboBox.Enabled);
			});
		}

		public void TestChangingAllowConnectionsComboBoxIsEnabledWhenAllowConnectionsIsNotAll()
		{
			var replicaOptionsGroupBox = (GroupBox)addNewSecondaryReplicaControl.Controls.Find("replicaOptionsGroupBox", false)[0];
			var allowConnectionsComboBox = (ComboBox)replicaOptionsGroupBox.Controls.Find("allowConnectionsComboBox", false)[0];
			allowConnectionsComboBox.SelectedIndex = 0;

			AssertEquals("control should be enabled to users when allow connections is NOT All", true, allowConnectionsComboBox.Enabled);
			AssertNotEquals("All", allowConnectionsComboBox.Text);

			allowConnectionsComboBox.SelectedIndex = 1;

			AssertEquals("control should be enabled to users when allow connections is NOT All", true, allowConnectionsComboBox.Enabled);
			AssertNotEquals("All", allowConnectionsComboBox.Text);

			allowConnectionsComboBox.SelectedIndex = 2;
			TestAllowConnectionsComboBoxDefaultBehaviours();
		}

		protected override void SetUp()
		{
			var primaryServerInfo = new SqlServerInfo(alias: "Node1\\SqlInstance", "Node1", "SqlInstance", "SomeDomain.net", "Node1.SomeDomain.net", default);
			var secondaryServerInfo = new SqlServerInfo(alias: "Node2\\SqlInstance", "Node2", "SqlInstance", "SomeDomain.net", "Node2.SomeDomain.net", default);

			base.SetUp();

			var primaryServerInstanceMock = new Mock<IPrimaryServerInstance>();
			var alwaysOnReplicaMock = new Mock<IAlwaysOnReplica>();
			var alwaysOnReplica2Mock = new Mock<IAlwaysOnReplica>();
			var availabilityGroupMock = new Mock<IAvailabilityGroup>();
			var failoverCluserMock = new Mock<IFailoverCluster>();

			_ = alwaysOnReplica2Mock.Setup(a2 => a2.ServerInfo).Returns(secondaryServerInfo);
			_ = availabilityGroupMock.Setup(aa => aa.Replicas).Returns(new[] { alwaysOnReplica2Mock.Object });
			_ = alwaysOnReplicaMock.Setup(a => a.ParentGroup).Returns(availabilityGroupMock.Object);
			_ = primaryServerInstanceMock.Setup(p => p.FailoverCluster).Returns(failoverCluserMock.Object);
			_ = primaryServerInstanceMock.Setup(p => p.ServerInfo).Returns(primaryServerInfo);
			_ = failoverCluserMock.Setup(f => f.Nodes).Returns(new[] { "MyServer" });

			addNewSecondaryReplicaControl = new AddNewSecondaryReplicaControlForTesting();
			testForm = new AddNewSecondaryReplicaControlForm(addNewSecondaryReplicaControl);
			testForm.Controls.Add(addNewSecondaryReplicaControl);
			addNewSecondaryReplicaControl.HookUiControlForm(testForm);

			_ = addNewSecondaryReplicaControl.Reset(primaryServerInstanceMock.Object, alwaysOnReplicaMock.Object);
		}

		protected override void TearDown()
		{
			AsyncHelper.WaitAllActiveTasksForTest();
			base.TearDown();
		}

		AddNewSecondaryReplicaControlForm testForm;
		AddNewSecondaryReplicaControlForTesting addNewSecondaryReplicaControl;
	}

	class AddNewSecondaryReplicaControlForTesting : AddNewSecondaryReplicaControl
	{
		public string GetServerName_Exposed()
		{
			return GetServerName();
		}
	}

	class AddNewSecondaryReplicaControlForm : MainForm
	{
		public AddNewSecondaryReplicaControlForm(Control controlToTest)
		{
			Controls.Add(controlToTest);
			((IParentFormHook)controlToTest).HookUiControlForm(this);
		}
	}
}
