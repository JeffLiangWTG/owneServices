using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.AlwaysOn.Setup.Properties;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public partial class ReplicaControl : UserControl // Cannot use ZUserControl because this is an external tool
	{
		public ReplicaControl()
		{
			InitializeComponent();
			pendingActionsLink.Click += PendingActionsLinkOnClick;
		}

		public void HookDatabasePanelClickEvent(Action<ReplicaControl> onReplicaDbPanelClick)
		{
			databasePanel.Click += (sender, e) => onReplicaDbPanelClick(this);
		}

		public void SetAlwaysOnReplica(IAlwaysOnReplica replica)
		{
			if (AvailabilityReplica != null)
			{
				AvailabilityReplica.HealthStatusChanged -= Replica_HealthStatusChanged;
			}

			AvailabilityReplica = replica;
			RefreshReplicaLabels();
			RefreshReplicaHealthyStatus();

			if (AvailabilityReplica != null)
			{
				AvailabilityReplica.HealthStatusChanged += Replica_HealthStatusChanged;
			}
		}

		void Replica_HealthStatusChanged(object sender, EventArgs e)
		{
			if (sender == AvailabilityReplica)
			{
				RefreshReplicaHealthyStatus();
			}
		}

		public IAlwaysOnReplica AvailabilityReplica { get; private set; }

		public Point OptionsControlLocation
		{
			get { return availabilityModeLabel.Location; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is on the AON tool")]
		public void RefreshReplicaHealthyStatus()
		{
			var healthyIssues = GetReplicaHealthyStatus();
			if (!string.IsNullOrEmpty(healthyIssues))
			{
				healthPanel.BackgroundImage = Resources.SmileySad24;
				healthPanel.Enabled = true;
				pendingActionsLink.Visible = true;

				toolTipService.SetToolTip(healthPanel, healthyIssues);
				toolTipService.SetToolTip(pendingActionsLink, healthyIssues);
			}
			else
			{
				healthPanel.BackgroundImage = Resources.SmileyHappy24;
				healthPanel.Enabled = false;
				pendingActionsLink.Visible = false;

				toolTipService.SetToolTip(healthPanel, null);
				toolTipService.SetToolTip(pendingActionsLink, null);
			}
		}

		protected void PendingActionsLinkOnClick(object sender, EventArgs e)
		{
			var healthStatus = GetReplicaHealthyStatus();
			if (!string.IsNullOrEmpty(healthStatus))
			{
				var message = Invariant($"Replica [{AvailabilityReplica.ServerInfo.ServerAlias}] has the following issue(s) which require your action(s):{Environment.NewLine}{Environment.NewLine}");
				message += healthStatus;

				MessageDialog.Show(message, ReplicaHealthIssuesCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation); // AON tool is not localized yet
			}
		}

		string GetReplicaHealthyStatus()
		{
			var numberOfIssues = 0;
			var issues = new List<string>();
			if (AvailabilityReplica.Health != SyncronisationHealth.HEALTHY)
			{
				issues.Add(Invariant($"{++numberOfIssues}) {ReplicaHealthIssues}"));
				issues.Add(Invariant($"=> Please try {CheckAddReplicaDatabasesAction}{Environment.NewLine}"));
			}

			if (!AvailabilityReplica.IsPrimary)
			{
				if (AvailabilityReplica.IsOdysseyAdminLoginSidDifferentFromPrimary)
				{
					issues.Add(Invariant($"{++numberOfIssues}) {OdysseyAdminLoginDifferentFromPrimaryReplica}"));
					issues.Add(Invariant($"=> Please try {CopyLoginsFromPrimaryReplicaAction}{Environment.NewLine}"));
				}
				else if (AvailabilityReplica.RequiresManualFailOver && AvailabilityReplica.IsAvailableForManualFailOver)
				{
					issues.Add(Invariant($"{++numberOfIssues}) {OdysseyAdminLoginIsNotDbOwner}"));
					issues.Add(Invariant($"=> Please try {ManualFailoverAction}{Environment.NewLine}"));
				}

				if (AvailabilityReplica.Options.SecondaryAllowConnection != AllowConnection.ALL)
				{
					issues.Add(Invariant($"{++numberOfIssues}) {AllowConnectionsNotSetToAll}"));
					issues.Add(Invariant($"=> Please try {SetAllowConnectionsToAllAction}{Environment.NewLine}"));
				}
			}

			if (!string.IsNullOrWhiteSpace(AvailabilityReplica.LastErrorMessage))
			{
				issues.Add(Invariant($"{++numberOfIssues}) Last error"));
				issues.Add(Invariant($"=> {AvailabilityReplica.LastErrorMessage}{Environment.NewLine}"));
			}

			return string.Join(Environment.NewLine, issues);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const strings")]
		public void RefreshReplicaLabels()
		{
			if (AvailabilityReplica == null)
			{
				roleLabel.Text = serverLabel.Text = nodeLabel.Text = availabilityModeLabel.Text = failoverModeLabel.Text = allowConnectionLabel.Text = "";
				healthPanel.BackgroundImage = null;
			}
			else
			{
				roleLabel.Text = AvailabilityReplica.Role.ToString().ToUpper() + (AvailabilityReplica.JoinState == JoinState.JOINED_FAILOVER_CLUSTER_INSTANCE ? " (FCI)" : "");
				serverLabel.Text = AvailabilityReplica.ServerInfo.ServerAlias;
				nodeLabel.Text = !string.IsNullOrEmpty(AvailabilityReplica.ServerInfo.ServerMachineName)
					? "Node: " + AvailabilityReplica.ServerInfo.ServerMachineName
					: AvailabilityReplica.JoinState == JoinState.JOINED_FAILOVER_CLUSTER_INSTANCE
						? "Failover Cluster Instance"
						: string.Empty;
				availabilityModeLabel.Text = AvailabilityReplica.Options.CommitMode.ToString();
				failoverModeLabel.Text = "Failover: " + AvailabilityReplica.Options.Failover.ToString();
				allowConnectionLabel.Text = "Allow Connections: " + (
					(AvailabilityReplica.Role == ReplicaRole.Secondary)
						? AvailabilityReplica.Options.SecondaryAllowConnection.ToString()
						: nameof(AllowConnection.ALL));

				healthPanel.BackgroundImage =
					(AvailabilityReplica.Health == SyncronisationHealth.HEALTHY)
						? Properties.Resources.SmileyHappy24
						: (AvailabilityReplica.Health == SyncronisationHealth.PARTIALLY_HEALTHY)
							? Properties.Resources.SmileyNeutral24
							: Properties.Resources.SmileySad24;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal const string ReplicaHealthIssuesCaption = "Replica Health Issues";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal const string ReplicaHealthIssues = "Replica has synchronizing issues";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal const string CheckAddReplicaDatabasesAction = "right-click on the replica and select 'Check/Add replica databases'";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal const string OdysseyAdminLoginDifferentFromPrimaryReplica = "Admin login is different from primary replica";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal const string CopyLoginsFromPrimaryReplicaAction = "right-click on the replica and select 'Copy Logins from Primary Replica'";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal const string OdysseyAdminLoginIsNotDbOwner = "Database owner is of different identity than from the primary replica";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal const string ManualFailoverAction = "right-click on the replica and select 'Perform Planned Manual Failover'";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal const string AllowConnectionsNotSetToAll = "'Allow Connections' to secondary replica is not set to 'ALL'";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		internal const string SetAllowConnectionsToAllAction = "right-click on the replica and select 'Change Replica Settings'";
	}
}
