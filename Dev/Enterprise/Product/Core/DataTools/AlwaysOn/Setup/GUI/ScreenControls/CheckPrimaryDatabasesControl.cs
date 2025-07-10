#define SuppressResourceStringsCheckRegion
#region SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using static System.FormattableString;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public partial class CheckPrimaryDatabasesControl : UserControl, IParentFormHook // Cannot use ZUserControl because external tool
	{
		public CheckPrimaryDatabasesControl()
		{
			InitializeComponent();
		}

		public void ResetControl(IPrimaryServerInstance primaryServer, IAlwaysOnReplica replica)
		{
			this.primaryServer = primaryServer;
			this.replica = replica;
			dbAlwaysOnStatuses = null;

			if (uiControlForm == null)
			{
				throw new InvalidOperationException("uiControlForm should not be null");
			}

			ReloadControls();
		}

		void ReloadControls()
		{
			addToGroupButton.Enabled = false;
			groupDatabaseListView.Items.Clear();
			groupLabel.Text = Invariant($"Group: {replica.ParentGroup.GroupName}");// no translation needed

			LoadDatabaseList();

			DisplayDatabaseStatus();
			SetConfirmButtonEnableState();
		}

		void LoadDatabaseList()
		{
			try
			{
				if (replica.Role == ReplicaRole.Primary)
				{
					var mainDb = GetGroupMainDbName();

					if (primaryServer == null || string.IsNullOrEmpty(mainDb))
					{
						throw new InvalidOperationException("primaryServer and mainDB should not be null");
					}

					dbAlwaysOnStatuses = primaryServer.GetAlwaysOnDatabaseSet(mainDb, alterDbSettingToMeetRequiremtns: true);

					if (dbAlwaysOnStatuses == null)
					{
						uiControlForm.ShowError(Invariant($"Unable to load databases from primary server:\r\n\r\n{primaryServer.LastErrorMessage}"));// no translation needed
					}
				}
				else
				{
					uiControlForm.ShowError(Invariant($"Replica role [{replica.Role}] is not valid for this operation. The primary replica is expected."));// no translation needed
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				uiControlForm.ShowError(ex.Message);
			}
		}

		string GetGroupMainDbName()
		{
			var alphaNumericRegex = new Regex(@"^[A-Z][A-Z0-9]*$", RegexOptions.IgnoreCase);

			var mainDb = (
				from db in replica.ParentGroup.Databases
				where alphaNumericRegex.IsMatch(db)
				orderby db
				select db).FirstOrDefault();

			return mainDb;
		}

		void DisplayDatabaseStatus()
		{
			groupDatabaseListView.Items.Clear();

			if (dbAlwaysOnStatuses != null)
			{
				foreach (var database in dbAlwaysOnStatuses)
				{
					var item = new ListViewItem(database.Name);
					item.Tag = database;
					var groupStatus = GetGroupStatus(database);
					item.ImageIndex = (int)groupStatus;
					item.ToolTipText = groupStatus.ToString().Replace('_', ' ');

					item.ForeColor = dbListColour[item.ImageIndex];

					item.SubItems.Add((database.GroupId == Guid.Empty) ? "" : database.GroupId == replica.ParentGroup.GroupId ? "This Group" : "Another Group");// no translation needed
					item.SubItems.Add(database.HasFullBackup ? "Yes" : "");
					item.SubItems.Add(database.IsDataMovementSuspended ? "Yes" : "");

					groupDatabaseListView.Items.Add(item);
				}
			}
		}

		GroupStatus GetGroupStatus(DatabaseAlwaysOnStatus database)
		{
			var result = GroupStatus.Cannot_be_added;

			if (database.HasFullBackup)
			{
				if (database.GroupId == Guid.Empty)
				{
					result = GroupStatus.Ready_to_be_added;
				}
				else if (database.GroupId == replica.ParentGroup.GroupId)
				{
					result = GroupStatus.Already_part_of_this_group;
				}
			}

			return result;
		}

		void SetConfirmButtonEnableState()
		{
			addToGroupButton.Enabled = (
				dbAlwaysOnStatuses != null
				&& dbAlwaysOnStatuses.Any()
				&& dbAlwaysOnStatuses.Any(db => db.HasFullBackup && db.GroupId == Guid.Empty)
			);
		}

		void AddMissingDatabasesToGroup()
		{
			if (dbAlwaysOnStatuses != null)
			{
				var eligibleDatabases = dbAlwaysOnStatuses.Where(db => db.HasFullBackup && db.GroupId == Guid.Empty).Select(db => db.Name);

				if (eligibleDatabases.Any())
				{
					uiControlForm.ShowMessage(Invariant($"Adding the following databases to availability group [{replica.ParentGroup.GroupName}]\r\n\t{string.Join("\r\n\t", eligibleDatabases)}"));

					replica.ParentGroup.AddDatabases(eligibleDatabases);

					if (replica.ParentGroup.HasErrors)
					{
						uiControlForm.ShowError(Invariant($"Failed to add databases to availability group:\r\n\r\n{replica.ParentGroup.LastErrorMessage}"));
					}
					else
					{
						uiControlForm.AppendMessage("\r\n\r\nSuccessfully added databases to availability group. Please join the added databases to each of the secondary replicas.");
					}
				}
				else
				{
					uiControlForm.ShowError("No eligible databases to add.");
				}
			}
		}

		IPrimaryServerInstance primaryServer;
		IAlwaysOnReplica replica;
		IEnumerable<DatabaseAlwaysOnStatus> dbAlwaysOnStatuses;

		static readonly Color[] dbListColour =
		{
			Color.Red,
			Color.Black,
			Color.Green,
		};

		enum GroupStatus
		{
			Cannot_be_added = 0,
			Already_part_of_this_group = 1,
			Ready_to_be_added = 2,
		}

		#region IParentFormHook members

		public void HookUiControlForm(IUiControlForm uiControlForm)
		{
			this.uiControlForm = uiControlForm;
		}
		IUiControlForm uiControlForm;

		#endregion // IParentFormHook members

		#region Event Handlers

		void cancelButton_Click(object sender, EventArgs e)
		{
			uiControlForm.OnActionCancelled();
		}

		void addToGroupButton_Click(object sender, EventArgs e)
		{
			AddMissingDatabasesToGroup();
			uiControlForm.OnActionConfirmed(ContextEnum.CheckPrimaryDatabases);
		}

		#endregion // Event Handlers

		void suspendResumeButton_Click(object sender, EventArgs e)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();

			foreach (ListViewItem item in groupDatabaseListView.SelectedItems)
			{
				var dbName = item.SubItems[0].Text;
				if (suspendResumeButton.Text.Equals("Resume Replication"))
				{
					primaryServer.ResumeReplication(dbName);
					uiControlForm.AppendMessage(Invariant($"\r\nDatabase [{dbName}] replication has resumed."));
				}
				else
				{
					primaryServer.SuspendReplication(dbName);
					uiControlForm.AppendMessage(Invariant($"\r\nDatabase [{dbName}] replication is suspended."));
				}
			}
			ReloadControls();
		}

		void groupDatabaseListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			foreach (ListViewItem item in ((ListView)sender).SelectedItems)
			{
				UpdateSuspendResumeButtonText(item);
			}
		}

		void UpdateSuspendResumeButtonText(ListViewItem selectedItem)
		{
			suspendResumeButton.Enabled = selectedItem.SubItems[1].Text.Equals("This Group", StringComparison.OrdinalIgnoreCase);
			suspendResumeButton.Text = (selectedItem.SubItems[3].Text.Equals("Yes", StringComparison.OrdinalIgnoreCase)) ? "Resume Replication" : "Suspend Replication";
		}
	}
}
#endregion
