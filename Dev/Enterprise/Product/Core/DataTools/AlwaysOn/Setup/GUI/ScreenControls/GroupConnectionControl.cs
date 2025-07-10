using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.FormattableString;

#region SuppressResourceStringsCheckRegion

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public partial class GroupConnectionControl : UserControl, IParentFormHook // Cannot use ZUserControl because external tool
	{
		public GroupConnectionControl()
		{
			InitializeComponent();

			dbServerTextBox.TextChanged -= dbServerTextBox_TextChanged;
			portRegex = new Regex("^\\d{1,5}$");

			if (dbServerTextBox.Tag == null)
			{
				throw new InvalidOperationException("dbServerTextBox.Tag cannot be null");
			}

			dbServerTextBox.Text = dbServerTextBox.Tag.ToString();
			dbServerTextBox.TextChanged += dbServerTextBox_TextChanged;

			dbServerTextBox.BackColor = MainForm.TextBoxErrorColor;
		}

		public void Reset()
		{
			if (dbServer != null)
			{
				dbServer = null;

				if (agTextBox.Tag == null)
				{
					throw new InvalidOperationException("dbServerTextBox.Tag cannot be null");
				}

				RefreshServerDependentControls();
			}
		}

		void ConnectAndValidateServer()
		{
			uiControlForm.ShowMessage("Validating SQL Server instance for AlwaysOn ...");// no translation needed

			var alias = dbServerTextBox.Text.Trim();
			var portNumber = string.IsNullOrWhiteSpace(portTextBox.Text) ? default : Int32.Parse(portTextBox.Text);

			dbServer = null;

			var auxDbServer = GetInDatabaseProcess(() =>
			{
				var serverInfo = new SqlServerInfo(alias, portNumber);
				return DbServerInstanceFactory.ConnectAndValidatePrimaryServer(serverInfo);
			});

			if (auxDbServer.IsLoaded)
			{
				uiControlForm.ShowMessage("Successfully connected to database server.");// no translation needed
				uiControlForm.AppendMessage("\r\nSQL Server service account: " + auxDbServer.SqlServiceAccount);// no translation needed

				if (auxDbServer.EligibleTopLevelDatabases?.Any() ?? false)
				{
					dbServer = auxDbServer;
				}
				else
				{
					uiControlForm.AppendMessage("\r\nNo AlwaysOn eligible databases found.");// no translation needed
				}
			}
			else
			{
				uiControlForm.ShowError(Invariant($"Unable to connect to AlwaysOn server:\r\n\r\n{auxDbServer.LastErrorMessage}"));// no translation needed
			}

			RefreshServerDependentControls();
		}

		void LoadAvailabilityGroupStructure(IAlwaysOnDatabase alwaysOnDb)
		{
			if (dbServer != null && alwaysOnDb != null && alwaysOnDb.GroupId != Guid.Empty)
			{
				uiControlForm.ShowMessage("Loading AlwaysOn Availability Group structure...");
				var availabilityGroup = GetInDatabaseProcess(() => AvailabilityGroupFactory.LoadAvailabilityGroupStructure(dbServer.ServerInfo, alwaysOnDb));

				if (availabilityGroup.IsLoaded)
				{
					uiControlForm.ShowMessage("Query admin logins on replicas...");
					foreach (var replica in availabilityGroup.Replicas)
					{
						replica.RefreshHealthState();
						if (replica.Health != SyncronisationHealth.NOT_HEALTHY)
						{
							_ = replica.GetOdysseyAdminLoginIfExists();
							replica.CheckOdysseyAdminIsDbOwner(availabilityGroup.Databases);
						}
					}

					uiControlForm.ShowMessage("Successfully loaded availability group.");
					uiControlForm.OnActionConfirmed(ContextEnum.ConnectToGroup, availabilityGroup, dbServer);
				}
				else
				{
					uiControlForm.ShowError(Invariant($"Unable to load availability group:\r\n\r\n{availabilityGroup.LastErrorMessage}"));
				}
			}
		}

		T GetInDatabaseProcess<T>(Func<T> getter)
		{
			var val = default(T);
			uiControlForm.RunDatabaseProcess(() => val = getter());
			return val;
		}

		void CreateNewAvailabilityGroup(IAlwaysOnDatabase alwaysOnDb, string newGroupName, int endpointPort)
		{
			uiControlForm.ShowMessage("Creating new AlwaysOn Availability Group...");
			Guid newGroupId = Guid.Empty;

			uiControlForm.RunDatabaseProcess(() =>
			{
				newGroupId = dbServer.CreateAvailabilityGroup(alwaysOnDb.Name, newGroupName, endpointPort);
			});

			if (dbServer.HasErrors)
			{
				uiControlForm.ShowError(Invariant($"Unable to create new availability group:\r\n\r\n{dbServer.LastErrorMessage}"));
			}
			else
			{
				alwaysOnDb.SetGroupInfo(newGroupName, newGroupId);
				uiControlForm.ShowMessage("Availability group created successfully.");
			}
		}

		#region Refresh GUI

		void RefreshServerDependentControls()
		{
			RefreshDatabaseComboBox();
			RefreshEndpointPortTextBox();
		}

		private protected void RefreshDatabaseComboBox()
		{
			databaseComboBox.BeginUpdate();

			var eligibleTopLevelDatabases = dbServer?.EligibleTopLevelDatabases;
			var databaseBindingList = eligibleTopLevelDatabases == null
				? new BindingList<IAlwaysOnDatabase>()
				: new BindingList<IAlwaysOnDatabase>(eligibleTopLevelDatabases.ToArray());
			databaseComboBox.DataSource = databaseBindingList;
			databaseComboBox.Enabled = (databaseBindingList.Count > 0);

			databaseComboBox.EndUpdate();

			if (databaseBindingList.Count > 0)
			{
				databaseComboBox.Focus();
			}
			else
			{
				RefreshGroupTextBox();
			}
		}

		void RefreshEndpointPortTextBox()
		{
			if (dbServer != null && dbServer.AlwaysOnEndpointPort > 0)
			{
				endpointPortTextBox.Enabled = false;
				endpointPortTextBox.Text = dbServer.AlwaysOnEndpointPort.ToString();
			}
			else
			{
				endpointPortTextBox.Enabled = (dbServer != null);
				endpointPortTextBox.Text = endpointPortTextBox.Enabled ? endpointPortTextBox.Tag.ToString() : string.Empty;
			}
		}

		void RefreshGroupTextBox()
		{
			var alwaysOnDb = databaseComboBox.SelectedItem as IAlwaysOnDatabase;

			if (alwaysOnDb == null || string.IsNullOrWhiteSpace(alwaysOnDb.GroupName))
			{
				agTextBox.Text = agTextBox.Tag.ToString();
				ToggleCreateShowGroupAndShowListenersButtons(groupExists: false);

				if (alwaysOnDb == null)
				{
					agTextBox.Enabled = false;
				}
				else
				{
					agTextBox.Enabled = true;
					agTextBox.Focus();
				}
			}
			else
			{
				agTextBox.Enabled = false;
				agTextBox.Text = alwaysOnDb.GroupName;
				ToggleCreateShowGroupAndShowListenersButtons(groupExists: true);
				showReplicasButton.Focus();
			}
		}

		void ToggleCreateShowGroupAndShowListenersButtons(bool groupExists)
		{
			createGroupButton.Enabled = CanCreateGroup;
			showReplicasButton.Enabled = showListenersButton.Enabled = (dbServer != null && groupExists);
		}

		bool CanCreateGroup
		{
			get
			{
				return dbServer != null &&
					(!(databaseComboBox.SelectedItem is IAlwaysOnDatabase alwaysOnDb) || string.IsNullOrWhiteSpace(alwaysOnDb.GroupName))
					&& !string.IsNullOrWhiteSpace(agTextBox.Text)
					&& agTextBox.Text != agTextBox.Tag.ToString()
					&& int.TryParse(endpointPortTextBox.Text.Trim(), out var port) && port > 0;
			}
		}

		#endregion // Refresh GUI

		#region IParentFormHook members

		public void HookUiControlForm(IUiControlForm uiControlForm)
		{
			this.uiControlForm = uiControlForm;
		}
		IUiControlForm uiControlForm;

		#endregion // IParentFormHook members

		#region Events

		private protected void connectButton_Click(object sender, EventArgs e)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();

			if (string.IsNullOrWhiteSpace(dbServerTextBox.Text))
			{
				uiControlForm.ShowError("Please enter a database server including the instance name, if not the default one.");
			}
			else
			{
				ConnectAndValidateServer();
			}
		}

		void databaseComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			RefreshGroupTextBox();
		}

		void createGroupButton_Click(object sender, EventArgs e)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();

			var alwaysOnDb = databaseComboBox.SelectedItem as IAlwaysOnDatabase;
			string groupName = agTextBox.Text.Trim();
			int endpointPort;

			endpointPortTextBox.Text = endpointPortTextBox.Text ?? ""; // for cc purposes
			if (
				int.TryParse(endpointPortTextBox.Text.Trim(), out endpointPort)
				&& alwaysOnDb != null
				&& !string.IsNullOrWhiteSpace(groupName))
			{
				CreateNewAvailabilityGroup(alwaysOnDb, groupName, endpointPort);

				if (!string.IsNullOrWhiteSpace(alwaysOnDb.GroupName))
				{
					if (databaseComboBox.DataSource == null)
					{
						throw new InvalidOperationException("A binding error occured because the DataSource property was null");
					}

					((BindingList<IAlwaysOnDatabase>)databaseComboBox.DataSource).ResetBindings();

					if (agTextBox.Tag == null)
					{
						throw new InvalidOperationException("dbServerTextBox.Tag cannot be null");
					}

					RefreshGroupTextBox();
				}
			}
		}

		private protected void showReplicasButton_Click(object sender, EventArgs e)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();

			var alwaysOnDb = databaseComboBox.SelectedItem as IAlwaysOnDatabase;
			LoadAvailabilityGroupStructure(alwaysOnDb);
		}

		void dbServerTextBox_TextChanged(object sender, EventArgs e)
		{
			ToggleConnectButon();

			uiControlForm.OnTextBoxTextChanged(sender as TextBox);
		}

		void ToggleConnectButon()
		{
			connectButton.Enabled =
				!string.IsNullOrWhiteSpace(dbServerTextBox.Text)
				&& dbServerTextBox.Text != dbServerTextBox.Tag.ToString()
				&& ValidSqlPort(portTextBox.Text);
		}

		void agTextBox_TextChanged(object sender, EventArgs e)
		{
			createGroupButton.Enabled = CanCreateGroup;

			uiControlForm.OnTextBoxTextChanged(sender as TextBox);
		}

		void databaseComboBox_Enter(object sender, EventArgs e)
		{
			databaseComboBox.BackColor = MainForm.FocusedColour;
		}

		void databaseComboBox_Leave(object sender, EventArgs e)
		{
			databaseComboBox.BackColor = databaseComboBox.Parent.BackColor;
		}

		void dbServerTextBox_Enter(object sender, EventArgs e)
		{
			uiControlForm.OnTextBoxEnter(sender as TextBox);
		}

		void dbServerTextBox_Leave(object sender, EventArgs e)
		{
			uiControlForm.OnTextBoxLeave(sender as TextBox);
		}

		void endpointPortTextBox_Enter(object sender, EventArgs e)
		{
			uiControlForm.OnTextBoxEnter(sender as TextBox);
		}

		void endpointPortTextBox_Leave(object sender, EventArgs e)
		{
			uiControlForm.OnTextBoxLeave(sender as TextBox);
		}

		void agTextBox_Enter(object sender, EventArgs e)
		{
			uiControlForm.OnTextBoxEnter(sender as TextBox);
		}

		void agTextBox_Leave(object sender, EventArgs e)
		{
			uiControlForm.OnTextBoxLeave(sender as TextBox);
		}

		#endregion // Events

		private protected IPrimaryServerInstance dbServer;
		readonly Regex portRegex;

		void endpointPortTextBox_TextChanged(object sender, EventArgs e)
		{
			createGroupButton.Enabled = CanCreateGroup;

			uiControlForm.OnTextBoxTextChanged(sender as TextBox);
		}

		void showListenersButton_Click(object sender, EventArgs e)
		{
			using var executionScope = Program.SqlContextManager.NewExecutionScope();

			uiControlForm.ShowMessage("Loading AlwaysOn Availability Group structure...");
			var availabilityGroup = GetInDatabaseProcess(() => AvailabilityGroupFactory.LoadAvailabilityGroupStructure(dbServer.ServerInfo, (IAlwaysOnDatabase)databaseComboBox.SelectedItem));

			if (availabilityGroup.IsLoaded)
			{
				uiControlForm.ShowMessage("Successfully loaded availability group.");
				uiControlForm.OnActionConfirmed(ContextEnum.ListenerSettings, availabilityGroup, this.dbServer);
			}
			else
			{
				uiControlForm.ShowError(Invariant($"Unable to load availability group:\r\n\r\n{availabilityGroup.LastErrorMessage}"));
			}
		}

		void portTextBox_TextChanged(object sender, EventArgs e)
		{
			if (sender is TextBox portBox)
			{
				var text = portBox.Text;
				if (ValidSqlPort(text))
				{
					var parent = portBox.Parent;
					if (parent != null)
					{
						portBox.BackColor = parent.BackColor;
					}
				}
				else
				{
					portBox.BackColor = MainForm.TextBoxErrorColor;
				}
			}

			ToggleConnectButon();
		}

		bool ValidSqlPort(string port)
		{
			return string.IsNullOrEmpty(port) || portRegex.IsMatch(port) && int.Parse(port, CultureInfo.InvariantCulture) <= 65535;
		}
	}
}
#endregion
