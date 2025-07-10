#define SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public partial class AddNewSecondaryReplicaControl : UserControl, IParentFormHook // Cannot use ZUserControl because this is an external tool
	{
		public AddNewSecondaryReplicaControl()
		{
			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string ValidationErrorPrefix = "Failed to add secondary replica";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string ConfirmButtonText = "Add Replica";

		protected IPrimaryServerInstance primaryServer;
		protected IAlwaysOnReplica primaryReplica;

		public Task Reset(IPrimaryServerInstance primaryServer, IAlwaysOnReplica primaryReplica)
		{
			InitialiseInternalAttributes(primaryServer, primaryReplica);
			InitialiseUserInterface();

			if (uiControlForm == null)
			{
				throw new InvalidOperationException("uiControlForm should not be null");
			}

			return Mediator.LoadPrimaryReplicaDatabaseInfoAsync(primaryReplica);
		}

		SecondaryReplicaActionMediator Mediator
		{
			get
			{
				if (mediator == null)
				{
					mediator = new SecondaryReplicaActionMediator(
						uiControlForm.ShowMessage,
						uiControlForm.ShowError,
						uiControlForm.RunDatabaseProcess,
						ValidationErrorPrefix,
						secondaryReplicaExists: false);
				}

				return mediator;
			}
		}
		SecondaryReplicaActionMediator mediator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const strings")]
		void AddSecondaryReplica(IReplicaOptions newReplicaOptions)
		{
			if (Mediator.DbPreJoinStatuses != null)
			{
				if (Mediator.SecondaryServer == null)
				{
					throw new InvalidOperationException("SecondaryServer should not be null");
				}

				uiControlForm.ShowMessage("Ensure secondary replica has the same database owner login.");
				var havingSameOdysseyAdminLoginAsPrimary = Mediator.SecondaryServer.EnsureOdysseyAdminLogin(Mediator.DbPreJoinStatuses);
				uiControlForm.AppendMessage(Invariant($"\r\nSecondary replica: [{Mediator.SecondaryServer.ServerInfo.ServerAlias}] has {(havingSameOdysseyAdminLoginAsPrimary ? "the same" : "different")} database owner login."));
				if (!CheckAndDisplaySecondaryReplicaActionProgress(Mediator.SecondaryServer))
				{
					return;
				}

				uiControlForm.AppendMessage(Invariant($"\r\nAdding secondary replica on [{Mediator.SecondaryServer.ServerInfo.ServerAlias}]..."));
				uiControlForm.AppendMessage("\r\nCheck and ensure high availability endpoint security.");
				Mediator.SecondaryServer.EnsureAlwaysOnEndpoint(primaryServer.AlwaysOnEndpointPort, primaryServer.SqlServiceAccount);

				if (!CheckAndDisplaySecondaryReplicaActionProgress(Mediator.SecondaryServer))
				{
					return;
				}

				uiControlForm.AppendMessage("\r\nAdd new group replica.");
				var secondaryReplica = primaryReplica.ParentGroup.AddSecondaryReplica(Mediator.SecondaryServer, newReplicaOptions);
				if (secondaryReplica == null)
				{
					uiControlForm.AppendMessage($"\r\n{primaryReplica.ParentGroup.LastErrorMessage}");
					return;
				}

				if (!Mediator.CheckAndDisplaySecondaryReplicaActionProgress(primaryReplica.ParentGroup))
				{
					return;
				}

				uiControlForm.AppendMessage("\r\nJoin secondary server instance to group as the newly created replica.");
				Mediator.SecondaryServer.JoinGroup(secondaryReplica.ParentGroup);

				if (!CheckAndDisplaySecondaryReplicaActionProgress(Mediator.SecondaryServer))
				{
					return;
				}

				uiControlForm.AppendMessage("\r\nRestore and join secondary databases.");
				Mediator.SecondaryServer.JoinDatabases(secondaryReplica.ParentGroup, Mediator.DbPreJoinStatuses);

				if (!CheckAndDisplaySecondaryReplicaActionProgress(Mediator.SecondaryServer))
				{
					return;
				}

				UpdateSecondaryReplicaHealth(secondaryReplica);

				uiControlForm.AppendMessage("\r\nPropagate database logins.");

				if (Mediator.PrimaryReplicaDbLogins == null)
				{
					throw new InvalidOperationException("PrimaryReplicaDbLogins cannot be null");
				}

				Mediator.SecondaryServer.EnsureDatabaseLogins(Mediator.PrimaryReplicaDbLogins);

				if (!CheckAndDisplaySecondaryReplicaActionProgress(Mediator.SecondaryServer))
				{
					return;
				}

				uiControlForm.AppendMessage("\r\n\r\nSuccessfully added secondary replica.");

				if (!havingSameOdysseyAdminLoginAsPrimary)
				{
					uiControlForm.AppendMessage("\r\n\r\nEnsure admin login on the new added secondary replica.");
					_ = Mediator.SecondaryServer.EnsureOdysseyAdminLogin(Mediator.DbPreJoinStatuses, true);
				}
			}
		}

		static void UpdateSecondaryReplicaHealth(IAlwaysOnReplica secondaryReplica)
		{
			// It takes some time for the join to take so we will give the server a chance to finish.
			var count = 0;
			while (secondaryReplica.Health == SyncronisationHealth.NOT_HEALTHY && count < 40)
			{
				Thread.Sleep(250);
				secondaryReplica.RefreshHealthState();
				++count;
			}
		}

		bool CheckAndDisplaySecondaryReplicaActionProgress(ISecondaryServerInstance server)
		{
			return Mediator.CheckAndDisplaySecondaryReplicaActionProgress(server);
		}

		bool ValidateReplicaOptions(out IReplicaOptions replicaOptions)
		{
			replicaOptions = ReplicaOptions.NewFromTextOptions(
				commitModeComboBox.SelectedItem.ToString(),
				failoverModeComboBox.SelectedItem.ToString(),
				allowConnectionsComboBox.SelectedItem.ToString(),
				secReadOnlyRoutingUrl: "");

			if (!replicaOptions.Validate(out var validationError))
			{
				uiControlForm.ShowError(validationError);
				return false;
			}

			return true;
		}

		#region Initialisation

		void InitialiseInternalAttributes(IPrimaryServerInstance primaryServer, IAlwaysOnReplica primaryReplica)
		{
			this.primaryServer = primaryServer;
			this.primaryReplica = primaryReplica;
			portTextBox.Text = primaryServer.ServerInfo.PortNumber.ToString();
		}

		void InitialiseUserInterface()
		{
			confirmButton.Enabled = false;
			groupDatabaseListView.Items.Clear();
			serverInstanceNameTextBox.Text = serverInstanceNameTextBox.Tag.ToString();
			commitModeComboBox.SelectedIndex = failoverModeComboBox.SelectedIndex = 0;
			allowConnectionsComboBox.SelectedIndex = 2;
			InitialiseReplicaNodeDropDown();
		}

		void InitialiseReplicaNodeDropDown()
		{
			var groupNodes = primaryReplica.ParentGroup.Replicas.Select(r => r.ServerInfo.ServerMachineName);
			var availableNodes = new List<NodeAvailabilityInfo> { defaultNode };
			availableNodes.AddRange(primaryServer.FailoverCluster.Nodes.Select(n => new NodeAvailabilityInfo(n, groupNodes.Contains(n, StringComparer.OrdinalIgnoreCase))));
			replicaNodeDropDown.DataSource = availableNodes.ToArray();
			replicaNodeDropDown.Enabled = (replicaNodeDropDown.Items.Count > 0);
		}

		#endregion // Initialisation

		#region NodeAvailabilityInfo

		struct NodeAvailabilityInfo
		{
			public NodeAvailabilityInfo(string nodeName, bool alreadyHostsReplica)
			{
				this.NodeName = nodeName;
				this.AlreadyHostsReplica = alreadyHostsReplica;
			}

			public string NodeName { get; }

			public bool AlreadyHostsReplica { get; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
			public override string ToString()
			{
				return NodeName + (AlreadyHostsReplica ? " (Already hosts a replica)" : "");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		readonly NodeAvailabilityInfo defaultNode = new NodeAvailabilityInfo("(Please select from the available nodes or enter your server name here)", false);

		#endregion // NodeAvailabilityInfo

		#region IParentFormHook members

		public void HookUiControlForm(IUiControlForm uiControlForm)
		{
			this.uiControlForm = uiControlForm;
		}
		IUiControlForm uiControlForm;

		#endregion // IParentFormHook members

		#region Events

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void allowConnectionsComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
#pragma warning disable CW1046 // Do Not Specify Tooltips Manually Rule
			if (!string.Equals(allowConnectionsComboBox.Text, "All", StringComparison.OrdinalIgnoreCase))
			{
				allowConnectionsComboBox.Enabled = true;
				toolTipService.SetToolTip(allowConnectionsComboBox, allowAllConnectionsToSecondaryReplica);
			}
			else
			{
				allowConnectionsComboBox.Enabled = false;
				toolTipService.SetToolTip(allowConnectionsComboBox, null);
			}
#pragma warning restore CW1046 // Do Not Specify Tooltips Manually Rule

		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			Mediator.DisposeAsyncTasks();
			uiControlForm.OnActionCancelled();
		}

		void confirmButton_Click(object sender, EventArgs e)
		{
			using var scope = Program.SqlContextManager.NewExecutionScope();

			IReplicaOptions replicaOptions;

			if (ValidateReplicaOptions(out replicaOptions))
			{
				Mediator.DisposeAsyncTasks();

				if (primaryReplica == null || primaryServer == null)
				{
					throw new InvalidOperationException("primaryReplica or primaryServer should not be null");
				}

				AddSecondaryReplica(replicaOptions);
				uiControlForm.OnActionConfirmed(ContextEnum.AddSecondaryReplica);
			}
		}

		void SecondaryReplicaActionControl_Load(object sender, EventArgs e)
		{
			confirmButton.Text = ConfirmButtonText;
		}

		void connectButton_Click(object sender, EventArgs e)
		{
			var executionScope = Program.SqlContextManager.NewExecutionScope();
			var task = Mediator.ConnectLoadAndDisplay(GetServerName(), primaryServer, primaryReplica, groupDatabaseListView, confirmButton);
			task.ContinueWith(t => executionScope.Dispose());
		}

		private protected string GetServerName()
		{
			var serverName = replicaNodeDropDown.Text;
			if (replicaNodeDropDown.SelectedItem != null)
			{
				serverName = ((NodeAvailabilityInfo)replicaNodeDropDown.SelectedItem).NodeName;
			}

			if (!string.IsNullOrWhiteSpace(serverInstanceNameTextBox.Text) && serverInstanceNameTextBox.Text != serverInstanceNameTextBox.Tag.ToString())
			{
				serverName += "\\" + serverInstanceNameTextBox.Text.Trim();
			}

			return serverName;
		}

		void replicaNodeDropDown_Changed(object sender, EventArgs e)
		{
			var isValidNode = true;
			if (sender is ComboBox dropdown)
			{
				if (dropdown.Parent == null || string.IsNullOrEmpty(dropdown.Text) || dropdown.Text == defaultNode.NodeName || (dropdown.SelectedItem is NodeAvailabilityInfo info && info.AlreadyHostsReplica))
				{
					dropdown.BackColor = MainForm.TextBoxErrorColor;
					isValidNode = false;
				}
				else
				{
					dropdown.BackColor = dropdown.Parent.BackColor;
				}
			}

			connectButton.Enabled = serverInstanceNameTextBox.Enabled = isValidNode;
		}

		void serverInstanceNameTextBox_Enter(object sender, EventArgs e)
		{
			if (serverInstanceNameTextBox.Text == serverInstanceNameTextBox.Tag.ToString())
			{
				serverInstanceNameTextBox.Text = "";
			}
		}

		void serverInstanceNameTextBox_Leave(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(serverInstanceNameTextBox.Text))
			{
				serverInstanceNameTextBox.Text = serverInstanceNameTextBox.Tag.ToString();
			}
		}

		#endregion // Events

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string allowAllConnectionsToSecondaryReplica = "Connections to secondary replica must be set to 'All'";
	}
}
