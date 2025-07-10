#define SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using static System.FormattableString;

#region SuppressResourceStringsCheckRegion

namespace Enterprise.AlwaysOn.Setup.GUI
{
	public class SecondaryReplicaActionMediator
	{
		public SecondaryReplicaActionMediator(Action<string> showMessage, Action<string> showError, Action<Action> runDatabaseProcess, string validationErrorPrefix, bool secondaryReplicaExists)
		{
			this.showMessage = showMessage;
			this.showError = showError;
			this.runDatabaseProcess = runDatabaseProcess;
			this.validationErrorPrefix = validationErrorPrefix;
			this.secondaryReplicaExists = secondaryReplicaExists;
		}

		readonly Action<string> showMessage;
		readonly Action<string> showError;
		readonly Action<Action> runDatabaseProcess;
		readonly string validationErrorPrefix;
		readonly bool secondaryReplicaExists;

		IDictionary<string, DbFileAndTransactionLogInfo> primaryReplicaDbs;

		public ISecondaryServerInstance SecondaryServer { get; private set; }

		public IEnumerable<IDatabasePreJoinStatus> DbPreJoinStatuses { get; private set; }

		public IEnumerable<DbLoginInfo> PrimaryReplicaDbLogins { get; private set; }

		public bool CheckAndDisplaySecondaryReplicaActionProgress(IValidationStatus objectToValidate)
		{
			if (objectToValidate.HasErrors)
			{
				showError(Invariant($"{validationErrorPrefix}:\r\n\r\n{objectToValidate.LastErrorMessage}"));
				return false;
			}

			return true;
		}

		public static readonly ReadOnlyCollection<Color> DbListColour = new ReadOnlyCollection<Color>(new Color[]
		{
			Color.Red,
			Color.Purple,
			Color.Blue,
			Color.Green,
			Color.Black,
		});

		#region Load Data

		public Task ConnectLoadAndDisplay(string secServerName, IPrimaryServerInstance primaryServer, IAlwaysOnReplica primaryReplica, ListView groupDatabaseListView, Button confirmButton)
		{
			var task = ConnectToSecondaryServerAsync(secServerName, primaryServer);
			LoadPrimaryAndSecondaryServerData(primaryServer, primaryReplica);
			DisplayDatabaseStatus(groupDatabaseListView);
			SetConfirmButtonEnableState(confirmButton);
			return task;
		}

		ISecondaryServerInstance ConnectToSecondaryServer(string secServerFullInstanceName, IPrimaryServerInstance primaryServer)
		{
			var nameAndInstance = secServerFullInstanceName.Trim().Split('\\');
			var serverInfo = new SqlServerInfo(secServerFullInstanceName, primaryServer.ServerInfo.PortNumber);
			var auxDbServer = DbServerInstanceFactory.ConnectAndValidateSecondaryServer(serverInfo, primaryServer);
			return auxDbServer;
		}

		void LoadPrimaryAndSecondaryServerData(IPrimaryServerInstance primaryServer, IAlwaysOnReplica primaryReplica)
		{
			showMessage("Validating SQL Server instance for AlwaysOn...");// no translation needed

			string errorMessage;
			IEnumerable<IDatabasePreJoinStatus> auxDbPreJoinStatuses;

			var auxSecDbServer = HaverstPrimaryAndSecondaryServerAsyncInfo(out errorMessage);

			if (auxSecDbServer == null)
			{
				showError(Invariant($"Unable to load server information asynchronously:\r\n\r\n{errorMessage}"));// no translation needed
			}
			else if (primaryReplica.HasErrors)
			{
				showError(Invariant($"Unable to load primary replica database information:\r\n\r\n{primaryReplica.LastErrorMessage}"));// no translation needed
			}
			else if (ValidateSecondaryServer(primaryServer, primaryReplica, auxSecDbServer, out errorMessage))
			{
				auxDbPreJoinStatuses = auxSecDbServer.GetPreJoinDatabaseStatuses(primaryReplicaDbs, primaryReplica.ParentGroup.GroupId);

				if (auxSecDbServer.HasErrors)
				{
					showError(Invariant($"Unable to load secondary replica database information:\r\n\r\n{auxSecDbServer.LastErrorMessage}"));// no translation needed
				}
				else
				{
					SecondaryServer = auxSecDbServer;
					DbPreJoinStatuses = auxDbPreJoinStatuses;
					showMessage("Successfully connected to database server.");// no translation needed
				}
			}
			else
			{
				showError(errorMessage);
			}
		}

		bool ValidateSecondaryServer(IPrimaryServerInstance primaryServer, IAlwaysOnReplica primaryReplica, ISecondaryServerInstance secondaryServer, out string validationErrorMsg)
		{
			validationErrorMsg = null;

			if (!secondaryServer.IsLoaded)
			{
				validationErrorMsg = Invariant($"Unable to connect to AlwaysOn server:\r\n\r\n{secondaryServer.LastErrorMessage}");
				return false;
			}

			if (primaryServer.FailoverCluster != null && secondaryServer.FailoverCluster != null && secondaryServer.FailoverCluster.Name != primaryServer.FailoverCluster.Name)
			{
				validationErrorMsg = Invariant($"This server failover cluster [{secondaryServer.FailoverCluster.Name}] is not the same as the one of the primary server [{primaryServer.FailoverCluster.Name}].");
				return false;
			}

			if (!secondaryReplicaExists)
			{
				var existingNodeReplica = primaryReplica.ParentGroup.Replicas.SingleOrDefault(r => r.ServerInfo.ServerMachineName == secondaryServer.ServerInfo.ServerMachineName);

				if (existingNodeReplica != null)
				{
					validationErrorMsg = Invariant($"This server node [{secondaryServer.ServerInfo.ServerMachineName}] already hosts a replica [{existingNodeReplica.ServerInfo.ServerAlias}] in this availability group.");
					return false;
				}
			}

			return true;
		}

		void DisplayDatabaseStatus(ListView groupDatabaseListView)
		{
			groupDatabaseListView.Items.Clear();

			if (DbPreJoinStatuses != null)
			{
				foreach (var database in DbPreJoinStatuses)
				{
					var item = new ListViewItem(database.Name)
					{
						Tag = database,
						ImageIndex = (int)database.JoinLevel,
						ToolTipText = database.JoinLevel.ToString().Replace('_', ' ')
					};

					item.ForeColor = DbListColour[item.ImageIndex];

					item.SubItems.Add((database.LastPrimaryBackupLsn > 0) ? database.LastPrimaryBackupLsn.ToString(CultureInfo.InvariantCulture) : "");

					if (database.SecondaryExists)
					{
						item.SubItems.Add(database.SecondaryStateDescription);
						item.SubItems.Add((database.SecondaryRedoLsn > 0) ? database.SecondaryRedoLsn.ToString(CultureInfo.InvariantCulture) : "");
						item.SubItems.Add((database.DbFilesMatch) ? "Match" : "Do not match");
						item.SubItems.Add(database.IsDataMovementSuspended ? "Yes" : "");
					}
					else
					{
						item.SubItems.Add("-");
					}

					groupDatabaseListView.Items.Add(item);
				}
			}
		}

		void SetConfirmButtonEnableState(Button confirmButton)
		{
			confirmButton.Enabled = (
				DbPreJoinStatuses != null
				&& DbPreJoinStatuses.Any()
				&& DbPreJoinStatuses.Any(db => db.JoinLevel != PreJoinLevel.Already_joined)
				&& DbPreJoinStatuses.All(db => db.JoinLevel != PreJoinLevel.Cannot_be_joined));
		}

		#region Async Load

		public Task LoadPrimaryReplicaDatabaseInfoAsync(IAlwaysOnReplica primaryReplica)
		{
			ResetAsyncLoadedPrimaryReplicaInfo();
			ResetAsyncLoadedSecondaryServerInfo();
			return StartPrimaryServerAsyncTasks(primaryReplica);
		}

		Task ConnectToSecondaryServerAsync(string secServerFullInstanceName, IPrimaryServerInstance primaryServer)
		{
			ResetAsyncLoadedSecondaryServerInfo();
			DisposeTask(connectToSecondaryTask);
			connectToSecondaryTask = StartAsyncTask(() => ConnectToSecondaryServer(secServerFullInstanceName, primaryServer));
			return connectToSecondaryTask;
		}

		ISecondaryServerInstance HaverstPrimaryAndSecondaryServerAsyncInfo(out string asyncLoadErrorMsg)
		{
			ISecondaryServerInstance auxSecDbServer = null;
			asyncLoadErrorMsg = string.Empty;

			try
			{
				runDatabaseProcess(() =>
				{
					if (primaryReplicaDbs == null)
					{
						WaitAndHaverstPrimaryServerAsyncTaskResults();
					}

					auxSecDbServer = WaitAndHaverstAsyncTaskResults(connectToSecondaryTask);
				});
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				asyncLoadErrorMsg = ex.Message ?? "";
			}

			return auxSecDbServer;
		}

		void ResetAsyncLoadedPrimaryReplicaInfo()
		{
			primaryReplicaDbs = null;
			PrimaryReplicaDbLogins = null;
		}

		void ResetAsyncLoadedSecondaryServerInfo()
		{
			SecondaryServer = null;
			DbPreJoinStatuses = null;
		}

		public void DisposeAsyncTasks()
		{
			DisposeTasks(primaryDbLoadTask, primaryDbLoginLoadTask, connectToSecondaryTask);
		}

		Task StartPrimaryServerAsyncTasks(IAlwaysOnReplica primaryReplica)
		{
			DisposeTasks(primaryDbLoadTask, primaryDbLoginLoadTask);

			primaryDbLoadTask = StartAsyncTask(primaryReplica.GetGroupDatabases);

			if (!secondaryReplicaExists)
			{
				primaryDbLoginLoadTask = primaryDbLoadTask.ContinueWith((t) => primaryReplica.GetGroupDatabaseLogins());
				return primaryDbLoginLoadTask;
			}
			else
			{
				return primaryDbLoadTask;
			}
		}

		Task<T> StartAsyncTask<T>(Func<T> taskDelegate)
		{
			var function = new Func<T>(() => taskDelegate());
			return Task.Factory.StartNew(function);
		}

		void WaitAndHaverstPrimaryServerAsyncTaskResults()
		{
			primaryReplicaDbs = WaitAndHaverstAsyncTaskResults(primaryDbLoadTask);
			PrimaryReplicaDbLogins = WaitAndHaverstAsyncTaskResults(primaryDbLoginLoadTask);
		}

		T WaitAndHaverstAsyncTaskResults<T>(Task<T> task)
		{
			var result = default(T);

			if (task != null)
			{
				task.Wait();
				result = task.Result;
			}

			return result;
		}

		void DisposeTasks(params IDisposable[] tasksToDispose)
		{
			foreach (var task in tasksToDispose)
			{
				DisposeTask(task);
			}
		}

		void DisposeTask(IDisposable taskToDispose)
		{
			if (taskToDispose != null)
			{
				taskToDispose.Dispose();
			}
		}

		Task<IDictionary<string, DbFileAndTransactionLogInfo>> primaryDbLoadTask;
		Task<ISecondaryServerInstance> connectToSecondaryTask;
		Task<IEnumerable<DbLoginInfo>> primaryDbLoginLoadTask;

		#endregion

		#endregion // Load Data
	}
}
#endregion
