using System;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.DbUpgrader.Schema.Synchronisers;
using Enterprise.DbUpgrader.Assemblies;
using Enterprise.DbUpgrader.ReferenceDatabases;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Script;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Builder.Generator
{
	class DbPreparation
	{
		public void UpgradeDbForSetupNewSchema()
		{
			var logger = new Logger(this);

			AddHeaderLine("Creating template database");
			((IAuxiliaryDbCreator)new AutoRegenTemplate(logger, Db.DatabaseName)).CreateDropExisting();

			using (var connection = Db.NewAdminConnection())
			{
				new MainDbAssembliesUpgrader(logger, connection, new VersionLabel(0, 0)).UpgradeAction.Run();
				var director = new ReferenceDbUpgradeDirector(logger, connection, logger);
				director.UpgradeReferenceDbs();
				director.UpgradeSingleRefDatabase();

				AddHeaderLine("Synchronising table valued types");
				new TableValuedTypesSynchroniser(
					connection,
					logger)
					.SynchroniseTableValuedTypes();

				new ScriptUpgrader(logger, connection, null, null, new VersionLabel(0, 0)).RecreateMainDbViewsAndRoutines();
			}
		}

		#region Custom Events

		public GeneratorEvent OnAddHeaderLine;
		public GeneratorEvent OnAddDetailLine;
		public GeneratorEvent OnAddErrorLine;

		void AddHeaderLine(string message)
		{
			OnAddHeaderLine?.Invoke(message);
		}

		void AddDetailLine(string message)
		{
			OnAddDetailLine?.Invoke("    " + message);
		}

		void AddErrorLine(string message)
		{
			OnAddErrorLine?.Invoke("ERROR: " + message);
		}

		#endregion // Custom Events

		#region Logger

		class Logger : IUpgradeManager
		{
			public Logger(DbPreparation parent)
			{
				this.parent = parent;
			}

			readonly DbPreparation parent;

			bool IUpgradeContext.IsHosted => EnvProxy.IsHostedWithCargowise;

			public bool? IsInternalSystem => throw new NotImplementedException();

			public bool? IsUATSystem => throw new NotImplementedException();

			public VersionLabel SchemaVersionBeforeUpgrade => new VersionLabel(0, 0);

			public VersionLabel TransformationVersionBeforeUpgrade => new VersionLabel(0, 0);

			public void ActivateSubtaskProgress(int numOfSubtasks)
			{
			}

			public void ActivateTaskProgress(int numOfTasks)
			{
			}

			public bool GetUserConfirmation(string title, string message, string[] detailLines)
			{
				return false;
			}

			public void IncrementNumberOfTasks(int numOfTasksToAdd)
			{
			}

			public bool ManagerKeyExists(DbConnection connection, string dbName)
			{
				return false;
			}

			public void SetManagerKey(DbConnection connection, string dbName)
			{
			}

			public void ShowDialog(Form form)
			{
			}

			public bool ShowErrorWithRetry(string errorMessage)
			{
				return false;
			}

			public void ShowInfoMessage(string infoMessage)
			{
				parent.AddDetailLine(infoMessage);
			}

			public void ShowTaskError(string errorMessage)
			{
				parent.AddErrorLine(errorMessage);
			}

			public void StartNonEstimatedTask(string task)
			{
				parent.AddHeaderLine(task);
			}

			public void StartSubtask(string subtask)
			{
				parent.AddDetailLine(subtask);
			}

			public void StartTask(string task)
			{
				parent.AddHeaderLine(task);
			}

			public void UpdateCurrentProgress(int currentProgress)
			{
			}
		}

		#endregion // Logger
	}
}
