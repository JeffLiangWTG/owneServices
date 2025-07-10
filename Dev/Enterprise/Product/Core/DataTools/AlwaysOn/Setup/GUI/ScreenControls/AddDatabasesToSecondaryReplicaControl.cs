#define SuppressResourceStringsCheckRegion

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public partial class AddDatabasesToSecondaryReplicaControl : UserControl, IParentFormHook // Cannot use ZUserControl because external tool
	{
		public AddDatabasesToSecondaryReplicaControl()
		{
			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string ValidationErrorPrefix = "Failed to add databases to secondary replica";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string ConfirmButtonText = "Add Databases";

		IPrimaryServerInstance primaryServer;
		IAlwaysOnReplica primaryReplica;
		IAlwaysOnReplica secondaryReplica;

		public Task Reset(IPrimaryServerInstance primaryServer, IAlwaysOnReplica primaryReplica,
			IAlwaysOnReplica secondaryReplica)
		{
			InitialiseInternalAttributes(primaryServer, primaryReplica, secondaryReplica);

			InitialiseUserInterface();

			if (uiControlForm == null)
			{
				throw new InvalidOperationException("uiControlForm cannot be null");
			}

			var task = Mediator.LoadPrimaryReplicaDatabaseInfoAsync(primaryReplica);
			var task2 = Mediator.ConnectLoadAndDisplay(secondaryReplica.ServerInfo.ServerAlias, primaryServer, primaryReplica, groupDatabaseListView, confirmButton);
			return Task.WhenAll(task, task2);
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
						secondaryReplicaExists: true);
				}

				return mediator;
			}
		}
		SecondaryReplicaActionMediator mediator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void AddDatabasesToSecondaryReplica()
		{
			var prejoinStatuses = Mediator.DbPreJoinStatuses;
			var secondaryServer = Mediator.SecondaryServer;
			if (prejoinStatuses != null)
			{
				uiControlForm.ShowMessage($"Adding databases to secondary replica [{secondaryServer.ServerInfo.ServerAlias}]...");
				uiControlForm.AppendMessage("\r\nRestore and join secondary databases.");
				secondaryServer.JoinDatabases(secondaryReplica.ParentGroup, prejoinStatuses);

				if (!Mediator.CheckAndDisplaySecondaryReplicaActionProgress(secondaryServer))
				{
					return;
				}

				uiControlForm.AppendMessage("\r\n\r\nSuccessfully added secondary replica databases.");
			}
		}

		#region Initialisation

		void InitialiseInternalAttributes(IPrimaryServerInstance primaryServer, IAlwaysOnReplica primaryReplica, IAlwaysOnReplica secondaryReplica)
		{
			this.primaryServer = primaryServer;
			this.primaryReplica = primaryReplica;
			this.secondaryReplica = secondaryReplica;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void InitialiseUserInterface()
		{
			confirmButton.Enabled = false;
			groupDatabaseListView.Items.Clear();
			dbServerLabel.Text = "Secondary Database Server: " + secondaryReplica.ServerInfo.ServerAlias;
		}

		#endregion // Initialisation

		#region IParentFormHook members

		public void HookUiControlForm(IUiControlForm uiControlForm)
		{
			this.uiControlForm = uiControlForm;
		}
		IUiControlForm uiControlForm;

		#endregion // IParentFormHook members

		#region Events

		void cancelButton_Click(object sender, EventArgs e)
		{
			if (uiControlForm == null)
			{
				throw new InvalidOperationException("uiControlForm cannot be null");
			}

			Mediator.DisposeAsyncTasks();
			uiControlForm.OnActionCancelled();
		}

		void confirmButton_Click(object sender, EventArgs e)
		{
			Mediator.DisposeAsyncTasks();

			if (Mediator.SecondaryServer == null)
			{
				throw new InvalidCastException("Mediator.SecondaryServer cannot be null");
			}

			AddDatabasesToSecondaryReplica();

			uiControlForm.OnActionConfirmed(ContextEnum.CheckSecondaryDatabases);
		}

		void AddDatabasesToSecondaryReplicaControl_Load(object sender, EventArgs e)
		{
			confirmButton.Text = ConfirmButtonText;
		}

		#endregion // Events

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void suspendResumeButton_Click(object sender, EventArgs e)
		{
			var executionScope = Program.SqlContextManager.NewExecutionScope();
			Task loadTask = null;
			Task loadTask2 = null;
			try
			{
				foreach (ListViewItem item in groupDatabaseListView.SelectedItems)
				{
					if (suspendResumeButton.Text.Equals("Resume Replication"))
					{
						secondaryReplica.ResumeReplication(item.SubItems[0].Text);
					}
					else
					{
						secondaryReplica.SuspendReplication(item.SubItems[0].Text);
					}
				}
				loadTask = Mediator.LoadPrimaryReplicaDatabaseInfoAsync(primaryReplica);
				loadTask2 = Mediator.ConnectLoadAndDisplay(secondaryReplica.ServerInfo.ServerAlias, primaryServer, primaryReplica, groupDatabaseListView, confirmButton);
			}
			finally
			{
				var tasks = (new Task[] { loadTask, loadTask2 }).Where(t => t is not null);
				Task.WhenAll(tasks).ContinueWith((t) => executionScope.Dispose());
			}
		}

		void groupDatabaseListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			foreach (ListViewItem item in ((ListView)sender).SelectedItems)
			{
				UpdateSuspendResumeButtonText(item);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		void UpdateSuspendResumeButtonText(ListViewItem selectedItem)
		{
			if (selectedItem.SubItems.Count > 4)
			{
				suspendResumeButton.Text = (selectedItem.SubItems[5].Text.Equals("Yes")) ? "Resume Replication" : "Suspend Replication";
			}
		}
	}
}
