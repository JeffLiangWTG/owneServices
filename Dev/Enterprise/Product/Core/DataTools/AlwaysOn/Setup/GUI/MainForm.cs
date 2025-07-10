#define SuppressResourceStringsCheckRegion
#region SuppressResourceStringsCheckRegion

using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public interface IUiControlForm
	{
		void ShowError(string message);
		void ShowMessage(string message);
		void AppendMessage(string message);
		void RunDatabaseProcess(Action processToRun);
		void OnActionCancelled();
		void OnActionConfirmed(ContextEnum context, params object[] eventArgs);
		void OnTextBoxEnter(TextBox textBox);
		void OnTextBoxLeave(TextBox textBox);
		void OnTextBoxTextChanged(TextBox textBox);
	}

	interface IParentFormHook
	{
		void HookUiControlForm(IUiControlForm uiControlForm);
	}

	public enum ContextEnum
	{
		DisplayGroupStructure,
		ConnectToGroup,
		AddSecondaryReplica,
		CheckPrimaryDatabases,
		CheckSecondaryDatabases,
		ChangeReplicaSettings,
		ListenerSettings,
		NOT_DEFINED,
	}

	public partial class MainForm : Form, IUiControlForm // Cannot use ODesignableForm because this is an external tool
	{
		public MainForm()
		{
			InitializeComponent();
			InitializeScreenAttributesAndEvents();
		}

		void InitializeScreenAttributesAndEvents()
		{
			InitialiseReplicaControls();
			addNewSecondaryReplicaControl.HookUiControlForm(this);
			checkPrimaryDatabasesControl.HookUiControlForm(this);
			checkSecondaryDatabasesControl.HookUiControlForm(this);
			groupConnectionControl.HookUiControlForm(this);
			changeReplicaSettingControl.HookUiControlForm(this);
			listenersControl.HookUiControlForm(this);
			SwitchFormContext(ContextEnum.ConnectToGroup);
		}

		void InitialiseReplicaControls()
		{
			replicaControls = new ReplicaControl[9]
			{
				primaryReplicaControl,
				secondary1ReplicaControl,
				secondary2ReplicaControl,
				secondary3ReplicaControl,
				secondary4ReplicaControl,
				secondary5ReplicaControl,
				secondary6ReplicaControl,
				secondary7ReplicaControl,
				secondary8ReplicaControl,
			};

			foreach (var replicaControl in replicaControls)
			{
				replicaControl.HookDatabasePanelClickEvent(ShowReplicaContextMenu);
			}
		}

		#region Implementation

		#region IUiControlForm members

		public static Color TextBoxErrorColor
		{
			get { return Color.Salmon; }
		}

		public void ShowError(string message)
		{
			ShowMessageCore(message, isError: true);
		}

		public void ShowMessage(string message)
		{
			ShowMessageCore(message, isError: false);
		}

		void ShowMessageCore(string message, bool isError)
		{
			try
			{
				outputTextBox.SuspendLayout();
				outputTextBox.Text = message;
				outputTextBox.ForeColor = (isError) ? Color.Red : Color.Black;
			}
			finally
			{
				outputTextBox.ResumeLayout();
				outputTextBox.ScrollToCaret();
			}
		}

		public void AppendMessage(string message)
		{
			try
			{
				outputTextBox.SuspendLayout();
				outputTextBox.AppendText(message);
			}
			finally
			{
				outputTextBox.ResumeLayout();
				outputTextBox.ScrollToCaret();
			}
		}

		DialogResult ShowDialogMessage(string message, string caption = "", MessageBoxButtons buttons = MessageBoxButtons.OK)
		{
			return MessageDialog.Show(message, caption, buttons); // Standalone Application. No external references.
		}

		void IUiControlForm.RunDatabaseProcess(Action processToRun)
		{
			using (this.SuspendUserInterface())
			{
				var previousCursor = Cursor;

				try
				{
					Cursor = Cursors.WaitCursor;
					processToRun();
				}
				finally
				{
					Cursor = previousCursor;
				}
			}
		}

		#endregion // IUiControlForm members

		#region Remove Availability Group / Secondary Replica

		void RemoveAvailabilityGroup()
		{
			var message = Invariant($"Do you want to remove availability group [{availabilityGroup.GroupName}]?");// no translation needed
			var userResponse = ShowDialogMessage(message, "Remove AlwaysOn Availability Group", MessageBoxButtons.YesNo);// no translation needed

			if (userResponse == DialogResult.Yes)
			{
				((IUiControlForm)this).RunDatabaseProcess(() =>
				{
					primaryDbServer.RemoveAvailabilityGroup(availabilityGroup.GroupName);
				});

				if (primaryDbServer != null && primaryDbServer.HasErrors)
				{
					ShowError(Invariant($"Unable to remove availability group:\r\n\r\n{primaryDbServer.LastErrorMessage}"));
				}
				else
				{
					groupConnectionControl.Reset();
					SwitchFormContext(ContextEnum.ConnectToGroup);
					ShowMessage("Successfully removed availability group.");// no translation needed
				}
			}
		}

		void RemoveSecondaryReplica(IAlwaysOnReplica replica)
		{
			var message = Invariant($"Do you want to remove replica on [{replica.ServerInfo.ServerAlias}]?"); // no translation needed
			var userResponse = ShowDialogMessage(message, "Remove Secondary AlwaysOn Replica", MessageBoxButtons.YesNo);

			if (userResponse == DialogResult.Yes)
			{
				((IUiControlForm)this).RunDatabaseProcess(() =>
				{
					availabilityGroup.RemoveSecondaryReplica(replica);
				});

				RefreshGroupReplicasPanel();

				if (primaryDbServer == null)
				{
					throw new InvalidOperationException("primaryDbServer was null");
				}

				if (primaryDbServer.HasErrors)
				{
					ShowError(Invariant($"Unable to remove secondary replica:\r\n\r\n{primaryDbServer.LastErrorMessage}"));
				}
				else
				{
					ShowMessage("Successfully removed secondary replica.");
				}
			}
		}

		#endregion // Remove Availability Group / Secondary Replica

		#region Refresh Group and GUI Controls

		void ChangeGroupStructure(IAvailabilityGroup group, IPrimaryServerInstance dbServer)
		{
			if (group == null || group.IsLoaded)
			{
				availabilityGroup = group;
				primaryDbServer = dbServer;
			}

			RefreshGroupStructure();
		}

		void RefreshGroupStructure()
		{
			RefreshGroupReplicasPanel();
			SwitchFormContext(ContextEnum.DisplayGroupStructure);
		}

		void RefreshGroupReplicasPanel()
		{
			groupDatabaseListView.Items.Clear();
			groupDatabaseListView.Visible = primaryReplicaControl.Visible =
				secondary1ReplicaControl.Visible = secondary2ReplicaControl.Visible =
				secondary3ReplicaControl.Visible = secondary4ReplicaControl.Visible =
				secondary5ReplicaControl.Visible = secondary6ReplicaControl.Visible =
				secondary7ReplicaControl.Visible = secondary8ReplicaControl.Visible = false;

			if (availabilityGroup != null)
			{
				// Database ListView
				RefreshGroupDatabaseListView();
				groupDatabaseListView.Visible = true;

				// Replica Controls
				var replicaIndex = 0;
				foreach (var replica in availabilityGroup.Replicas)
				{
					var index = replicaIndex++;

					if (replicaControls == null || replicaControls.Length <= index)
					{
						throw new InvalidOperationException("index cannot be greater than replicaControls.Length");
					}

					var control = replicaControls[index];

					DisplayReplicaControl(control, replica);
				}
			}
		}

		void RefreshGroupDatabaseListView()
		{
			groupDatabaseListView.Items.Clear();

			if (availabilityGroup != null)
			{
				foreach (var database in availabilityGroup.Databases)
				{
					var item = new ListViewItem(database);
					groupDatabaseListView.Items.Add(item);
				}
			}
		}

		void DisplayReplicaControl(ReplicaControl replicaControl, IAlwaysOnReplica replica)
		{
			replicaControl.SetAlwaysOnReplica(replica);
			replicaControl.Visible = true;
		}

		void SwitchFormContext(ContextEnum newContext)
		{
			SuspendLayout();

			try
			{
				addNewSecondaryReplicaControl.Visible = false;
				checkPrimaryDatabasesControl.Visible = false;
				checkSecondaryDatabasesControl.Visible = false;
				groupConnectionControl.Visible = false;
				changeReplicaSettingControl.Visible = false;
				listenersControl.Visible = false;
				groupStructurePanel.Enabled = false;
				selectGroupButton.Enabled = true;

				switch (newContext)
				{
					case ContextEnum.DisplayGroupStructure:
						if (primaryDbServer == null || primaryDbServer.FailoverCluster == null)
						{
							throw new InvalidOperationException("primaryDbServer cannot be null");
						}

						groupInfoLabel.Text = (availabilityGroup == null)
							? "No availability group selected"
							: Invariant($"Failover Cluster: {primaryDbServer.FailoverCluster.Name} - Availability Group: {availabilityGroup.GroupName}");
						groupStructurePanel.BringToFront();
						groupStructurePanel.Enabled = true;
						break;

					case ContextEnum.AddSecondaryReplica:
						groupInfoLabel.Text = "Add Secondary Replica";
						SetControlFocus(addNewSecondaryReplicaControl);
						break;

					case ContextEnum.CheckPrimaryDatabases:
						groupInfoLabel.Text = "Check/Add Group Databases";
						SetControlFocus(checkPrimaryDatabasesControl);
						break;

					case ContextEnum.CheckSecondaryDatabases:
						groupInfoLabel.Text = "Check/Add Secondary Replica Databases";
						SetControlFocus(checkSecondaryDatabasesControl);
						break;

					case ContextEnum.ConnectToGroup:
						groupInfoLabel.Text = "Select/Create Availability Group";
						selectGroupButton.Enabled = false;
						SetControlFocus(groupConnectionControl);
						break;

					case ContextEnum.ChangeReplicaSettings:
						groupInfoLabel.Text = "Change Replica Settings";
						SetControlFocus(changeReplicaSettingControl);
						break;

					case ContextEnum.ListenerSettings:
						groupInfoLabel.Text = "Listener Settings";
						SetControlFocus(listenersControl);
						break;

					default:
						throw new ArgumentException("Invalid form context " + newContext, nameof(newContext));
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		void SetControlFocus(Control controlToFocus)
		{
			controlToFocus.BringToFront();
			controlToFocus.Visible = true;
			controlToFocus.Focus();
		}

		#endregion // Refresh GUI Controls

		IPrimaryServerInstance primaryDbServer;
		IAvailabilityGroup availabilityGroup;

		ReplicaControl[] replicaControls;

		#endregion // Implementation

		#region Events

		void MainForm_Load(object sender, EventArgs e)
		{
			Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		}

		void AlwaysOnSetupForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			const string message = "Are you sure you want to exit?";
			var userResponse = ShowDialogMessage(message, "Exit Setup", MessageBoxButtons.YesNo);

			if (userResponse == DialogResult.No)
			{
				e.Cancel = true;
			}
		}

		void ExitButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void connectToGroupButton_Click(object sender, EventArgs e)
		{
			if (primaryDbServer == null)
			{
				groupConnectionControl.Reset();
			}

			SwitchFormContext(ContextEnum.ConnectToGroup);
		}

		#region Replica Events

		void ShowReplicaContextMenu(ReplicaControl replicaControl)
		{
			var replicaMenu = new ContextMenu();
			AddReplicaContextMenuOptions(replicaMenu, replicaControl);
			replicaMenu.Show(replicaControl, replicaControl.PointToClient(Cursor.Position));
		}

		void AddReplicaContextMenuOptions(ContextMenu menu, ReplicaControl replicaControl)
		{
			if (replicaControl.AvailabilityReplica.Role == ReplicaRole.Primary)
			{
				menu.MenuItems.Add(NewMenuItem("Add Secondary Replica", OnAddSecondaryReplicaMenuClick, replicaControl));
				menu.MenuItems.Add(NewMenuItem("Change Replica Settings", OnChangeReplicaSettingsMenuClick, replicaControl));
				menu.MenuItems.Add(NewMenuItem("Check/Add Group Databases", OnCheckGroupDatabasesMenuClick, replicaControl));
				menu.MenuItems.Add(NewMenuItem("Remove Availability Group", OnRemoveAvailabilityGroupMenuClick, replicaControl));
			}
			else if (replicaControl.AvailabilityReplica.Role == ReplicaRole.Secondary)
			{
				menu.MenuItems.Add(NewMenuItem("Change Replica Settings", OnChangeReplicaSettingsMenuClick, replicaControl));
				menu.MenuItems.Add(NewMenuItem("Check/Add Replica Databases", OnCheckSecondaryReplicaDatabasesMenuClick, replicaControl));
				menu.MenuItems.Add(NewMenuItem("Copy Logins from Primary Replica", OnCopyLoginsFromPrimaryReplicaMenuClick, replicaControl));

				var failoverMenuItem = NewMenuItem("Perform Planned Manual Failover", OnPerformPlannedManualFailoverMenuClik, replicaControl);
				failoverMenuItem.Enabled = replicaControl.AvailabilityReplica.IsAvailableForManualFailOver;
				menu.MenuItems.Add(failoverMenuItem);

				menu.MenuItems.Add(NewMenuItem("Remove Replica", OnRemoveReplicaMenuClick, replicaControl));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1100:DoNotUseMenuItemOrKMenuItem", Justification = "Baseline")]
		MenuItem NewMenuItem(string menuText, Action<ReplicaControl> menuAction, ReplicaControl replicaControl)
		{
			var menuHandler = new EventHandler((sender, e) => menuAction(replicaControl));
			return new MenuItem(menuText, menuHandler); // Standalone Application. No external references.
		}

		void OnAddSecondaryReplicaMenuClick(ReplicaControl replicaControl)
		{
			var executionScope = Program.SqlContextManager.NewExecutionScope();

			Task resetTask = null;
			try
			{
				resetTask = addNewSecondaryReplicaControl.Reset(primaryDbServer, replicaControl.AvailabilityReplica);
				SwitchFormContext(ContextEnum.AddSecondaryReplica);
			}
			finally
			{
				resetTask?.ContinueWith((t) => executionScope.Dispose());
			}
		}

		void OnRemoveAvailabilityGroupMenuClick(ReplicaControl replicaControl)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();

			if (availabilityGroup.Replicas.Any(r => r.ReplicaId != replicaControl.AvailabilityReplica.ReplicaId))
			{
				ShowDialogMessage(Invariant($"Availability Group [{availabilityGroup.GroupName}] cannot be removed as it contains at least one secondary replica."));
			}
			else
			{
				RemoveAvailabilityGroup();
			}
		}

		void OnRemoveReplicaMenuClick(ReplicaControl replicaControl)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();

			RemoveSecondaryReplica(replicaControl.AvailabilityReplica);
		}

		void OnChangeReplicaSettingsMenuClick(ReplicaControl replicaControl)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();

			changeReplicaSettingControl.Reset(replicaControl);
			SwitchFormContext(ContextEnum.ChangeReplicaSettings);
		}

		void OnCheckGroupDatabasesMenuClick(ReplicaControl replicaControl)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();

			if (replicaControl.AvailabilityReplica.Role == ReplicaRole.Primary)
			{
				checkPrimaryDatabasesControl.ResetControl(primaryDbServer, replicaControl.AvailabilityReplica);
				SwitchFormContext(ContextEnum.CheckPrimaryDatabases);
			}
			else
			{
				ShowDialogMessage("This option only applies to primary replicas.");
			}
		}

		void OnCheckSecondaryReplicaDatabasesMenuClick(ReplicaControl replicaControl)
		{
			if (replicaControl.AvailabilityReplica.Role == ReplicaRole.Secondary)
			{
				var executionScope = Program.SqlContextManager.NewExecutionScope();

				Task resetTask = null;
				try
				{
					resetTask = checkSecondaryDatabasesControl.Reset(primaryDbServer, primaryReplicaControl.AvailabilityReplica, replicaControl.AvailabilityReplica);
					SwitchFormContext(ContextEnum.CheckSecondaryDatabases);
					UpdateReplicaHealthStatus(replicaControl.AvailabilityReplica);
				}
				finally
				{
					resetTask?.ContinueWith((t) => executionScope.Dispose());
				}
			}
			else
			{
				ShowDialogMessage("This option only applies to secondary replicas.");
			}
		}

		void OnCopyLoginsFromPrimaryReplicaMenuClick(ReplicaControl replicaControl)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();

			PerformActionOnReplica(replicaControl, replicaControl.AvailabilityReplica.CopyDatabaseLoginsFromPrimaryReplica, "Failed to copy logins from primary replica", "Successfully copied logins from primary replica.");

			if (!replicaControl.AvailabilityReplica.HasErrors && replicaControl.AvailabilityReplica.RequiresManualFailOver && !replicaControl.AvailabilityReplica.IsOdysseyAdminLoginSidDifferentFromPrimary)
			{
				const string manualFailOverCaption = "Manual failover is required as soon as possible";
				var message = Invariant($@"Please right-click on the replica [{replicaControl.AvailabilityReplica.ServerInfo.ServerAlias}] and select 'Perform Planned Manual Failover' menu to finalize the synchronization of admin login with the primary replica."); // No localized string from AON tool

				MessageDialog.Show(message, manualFailOverCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation); // Cannot use Global.ShowMessage here as that requires Db.Connection which we are not using
			}

			UpdateReplicaHealthStatus(replicaControl.AvailabilityReplica);
		}

		void OnPerformPlannedManualFailoverMenuClik(ReplicaControl replicaControl)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();

			if (PerformActionOnReplica(replicaControl, replicaControl.AvailabilityReplica.PerformPlannedManualFailover, "Failed to perform a planned manual failover", "Successfully performed a planned manual failover."))
			{
				connectToGroupButton_Click(null, null);
			}
		}

		bool PerformActionOnReplica(ReplicaControl replicaControl, Action action, string failureMessage, string successMessage)
		{
			((IUiControlForm)this).RunDatabaseProcess(() =>
			{
				action?.Invoke();
			});

			if (replicaControl.AvailabilityReplica.HasErrors)
			{
				ShowError(Invariant($"{failureMessage}:\r\n\r\n{replicaControl.AvailabilityReplica.LastErrorMessage}"));
				return false;
			}

			ShowMessage(successMessage);

			return true;
		}

		protected void UpdateReplicaHealthStatus(IAlwaysOnReplica replica)
		{
			((IUiControlForm)this).RunDatabaseProcess(() =>
			{
				try
				{
					replica.RefreshHealthState();
					if (replica.Health != SyncronisationHealth.NOT_HEALTHY)
					{
						_ = replica.GetOdysseyAdminLoginIfExists();
						replica.CheckOdysseyAdminIsDbOwner(replica.ParentGroup.Databases);
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
				}
			});
		}

		#endregion // Replica Events

		#region Action Control Events

		void IUiControlForm.OnActionCancelled()
		{
			ShowMessage("");
			SwitchFormContext(ContextEnum.DisplayGroupStructure);
		}

		void IUiControlForm.OnActionConfirmed(ContextEnum context, params object[] eventArgs)
		{
			switch (context)
			{
				case ContextEnum.AddSecondaryReplica:
				case ContextEnum.CheckSecondaryDatabases:
					RefreshGroupStructure();
					break;

				case ContextEnum.CheckPrimaryDatabases:
					RefreshGroupDatabaseListView();
					SwitchFormContext(ContextEnum.DisplayGroupStructure);
					break;

				case ContextEnum.ConnectToGroup:
					if (eventArgs == null || eventArgs.Length < 2)
					{
						throw new InvalidOperationException("eventArgs should have at least 2 elements");
					}

					ChangeGroupStructure((IAvailabilityGroup)eventArgs[0], (IPrimaryServerInstance)eventArgs[1]);
					break;

				case ContextEnum.ChangeReplicaSettings:
					if (eventArgs == null || eventArgs.Length < 1)
					{
						throw new InvalidOperationException("eventArgs should have at least 1 element");
					}

					var eventArgObj = eventArgs[0] as ReplicaControl ?? throw new InvalidOperationException("eventArgs cannot be null");

					eventArgObj.RefreshReplicaLabels();
					SwitchFormContext(ContextEnum.DisplayGroupStructure);
					break;

				case ContextEnum.ListenerSettings:
					if (eventArgs == null || eventArgs.Length < 2)
					{
						throw new InvalidOperationException("eventArgs should be at least 2 elements");
					}

					listenersControl.DbServer = eventArgs[1] as IDbServerInstance;
					listenersControl.AvailabilityGroup = eventArgs[0] as IAvailabilityGroup;
					ChangeGroupStructure((IAvailabilityGroup)eventArgs[0], (IPrimaryServerInstance)eventArgs[1]);
					SwitchFormContext(ContextEnum.ListenerSettings);
					break;

				default:
					throw new ArgumentException("Invalid form context for this operation " + context.ToString(), nameof(context));
			}
		}

		void IUiControlForm.OnTextBoxEnter(TextBox textBox)
		{
		}

		void IUiControlForm.OnTextBoxLeave(TextBox textBox)
		{
			if (textBox != null)
			{
				if (textBox.Tag != null && (string.IsNullOrEmpty(textBox.Text) || textBox.Text == textBox.Tag.ToString()))
				{
					textBox.Text = textBox.Tag.ToString();
					textBox.BackColor = TextBoxErrorColor;
				}
				else if (textBox.Parent != null) // textBox will always have a parent but this will fix CC
				{
					textBox.BackColor = textBox.Parent.BackColor;
				}
			}
		}

		void IUiControlForm.OnTextBoxTextChanged(TextBox textBox)
		{
			if (textBox != null)
			{
				if (textBox.Parent != null && textBox.Tag != null && textBox.Text != textBox.Tag.ToString() && !string.IsNullOrEmpty(textBox.Text))
				{
					textBox.BackColor = textBox.Parent.BackColor;
				}
				else
				{
					textBox.BackColor = TextBoxErrorColor;
				}
			}
		}

		public static readonly Color FocusedColour = SystemColors.Info;

		#endregion // Action Control Events

		#endregion // Events
	}
}
#endregion

#region Test

#if DEBUG
namespace Enterprise.AlwaysOn.Setup.GUI
{
	public partial class MainForm
	{
		public void AddReplicaContextMenuOptions_ForTest(ContextMenu menu, ReplicaControl replicaControl)
		{
			AddReplicaContextMenuOptions(menu, replicaControl);
		}

		public void PerformCopyLoginsFromPrimaryReplicaMenuClick_ForTest(ReplicaControl replicaControl)
		{
			OnCopyLoginsFromPrimaryReplicaMenuClick(replicaControl);
		}
	}
}
#endif

#endregion
