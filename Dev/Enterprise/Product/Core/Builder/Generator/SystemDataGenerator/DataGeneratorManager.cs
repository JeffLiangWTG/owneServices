using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.DbUpgrader.Data;
using Enterprise.DbUpgrader.Data.Test;

namespace Enterprise.Builder.Generator
{
	public class DataGeneratorManager
	{
		public DataGeneratorManager()
		{
		}

		public GeneratorEvent OnTaskStarted;
		public GeneratorEvent OnSubtaskStarted;
		public GeneratorEvent OnTaskFailed;

		public bool IsDataVersionFileCheckedOutByMe
		{
			get
			{
				DataUpgradeSetupController dummyController = new DataUpgradeSetupController(Array.Empty<UpgradeTask>());
				return dummyController.IsCheckedOutByMe;
			}
		}

		public UpgradeTask[] GetUpgradeTasks()
		{
			UpgradeTask[] result;

			try
			{
				DataUpgrader dummyUpgrader = new DataUpgraderTest.DataUpgraderForAllTasks();
				result = dummyUpgrader.UpgradeTaskList.OfType<UpgradeTask>().ToArray();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				RaiseTaskError(e.Message);
				result = Array.Empty<UpgradeTask>();
			}

			return result;
		}

		public bool CopyRequiredFilesToLocationAndApplyDataToDb(UpgradeTask[] tasks)
		{
			StartTask("Checking-out UpgradeTasks for changes on DB");

			try
			{
				DataUpgradeSetupController controller = new DataUpgradeSetupController(tasks);
				controller.FullCheckOut();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				RaiseTaskError(e.Message);
				return false;
			}

			StartTask("System data is ready to be modified on: Server [" + Db.ServerName + "] / Database [" + Db.DatabaseName + "]");
			StartTask("");

			return true;
		}

		public bool SaveDbDataToXmlFiles(UpgradeTask[] tasks)
		{
			StartTask("Saving DB data to UpgradeTask XML files");

			try
			{
				DataUpgradeSetupController controller = new DataUpgradeSetupController(tasks);
				controller.FullSave();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				RaiseTaskError(e.Message);
				return false;
			}

			StartTask("System data has been succesfully saved");
			StartTask("");

			return true;
		}

		#region Implementation

		protected void StartTask(string text)
		{
			if (OnTaskStarted != null)
			{
				OnTaskStarted(text);
			}
		}

		protected void StartSubtask(string text)
		{
			if (OnSubtaskStarted != null)
			{
				OnSubtaskStarted(text);
			}
		}

		protected void RaiseTaskError(string errorMessage)
		{
			if (OnTaskFailed != null)
			{
				OnTaskFailed(errorMessage);
			}
		}

		#endregion

	}
}
