using System;
using System.Data;
using System.Linq;
using CargoWise.BuildTools;
using Enterprise.DbUpgrader.Data;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Builder.DataUpgradeSetup
{
	#region TaskState Enumeration

	public enum TaskStateEnum
	{
		Start,
		OutUnchanged,
		OutModified,
		InUnchanged,
		InModified
	}

	#endregion

	/// <summary>
	/// This class controls the check-out / modification / check-in / undo-check-out of
	/// a DataUpgrade task XML file.
	/// </summary>
	public class DataTaskSetup
	{
		public DataTaskSetup(UpgradeTask task, ISetupController controller) : this(task, controller, task)
		{
		}

		public DataTaskSetup(UpgradeTask task, ISetupController controller, UpgradeTask taskForReading)
		{
			fTask = task;
			fController = controller;
			fTaskForReading = taskForReading;
		}

		/// <summary>
		/// 1 - Checks out the UpgradeTask XML DataFile
		/// 2 - Always gets latest, replacing local with SourceControl version
		/// 3 - Apply data on XML to the database
		/// </summary>
		public void CheckOut()
		{
			VerifyIfControllerIsCheckedOut();

			SourceControl.EnterpriseDatabase.CheckOut(fTask.ResourceFile.FileFullPath, false);

			PerformPreApplyTasks();
			ApplyXmlFileDataToDatabase();
			State = TaskStateEnum.OutUnchanged;
		}

		protected virtual void PerformPreApplyTasks()
		{ }

		/// <summary>
		/// 1 - Undoes check-out of the UpgradeTask XML DataFile
		/// 2 - Apply data on XML to the database
		/// </summary>
		public void UndoCheckOut()
		{
			VerifyIfControllerIsCheckedOut();
			SourceControl.EnterpriseDatabase.UndoCheckOut(fTask.ResourceFile.FileFullPath, false);
			ApplyXmlFileDataToDatabase();

			State = TaskStateEnum.InUnchanged;
		}

		public bool IsCheckedOutByMe
		{
			get { return SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(fTask.ResourceFile.FileFullPath); }
		}

		/// <summary>
		/// 1 - Saves data in the DB to the XML file
		/// 2 - Before saves, it gets latest replacing local with SourceControl version
		///     to avoid skipping version numbers
		/// </summary>
		public void SaveDataToSourceFile()
		{
			VerifyIfControllerIsCheckedOut();
			SaveNewDataToFileAndIncrementVersionIfDataHasChanged();
		}

		public TaskStateEnum State
		{
			get
			{
				if (!fStatePopulated)
				{
					SetInitialState();
				}
				return fState;
			}
			private set
			{
				fState = value;
				fStatePopulated = true;
			}
		}

		TaskStateEnum fState;
		bool fStatePopulated;

		public UpgradeTask Task
		{
			get { return fTask; }
		}

		protected readonly UpgradeTask fTask;

		#region Implementation

		protected readonly ISetupController fController;

		protected readonly UpgradeTask fTaskForReading;

		void SetInitialState()
		{
			if (SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(fTask.ResourceFile.FileFullPath))
			{
				if (SourceControl.EnterpriseDatabase.IsDifferent(fTask.ResourceFile.FileFullPath))
				{
					State = TaskStateEnum.OutModified;
				}
				else
				{
					State = TaskStateEnum.OutUnchanged;
				}
			}
			else
			{
				State = TaskStateEnum.Start;
			}
		}

		protected DataSet LoadDataFromSourceFile()
		{
			return fTaskForReading.ResourceFile.LoadDataFromFile();
		}

		internal protected virtual void ApplyXmlFileDataToDatabase()
		{
			fTaskForReading.RunForSetupFromFile();
		}

		/// <summary>
		/// 1 - Overwrites file with version from source safe
		/// 2 - Load new data from DB
		/// 3 - Set version as current and writes to file
		/// 4 - Compares to source safe, increments version if has changed
		/// </summary>
		protected virtual void SaveNewDataToFileAndIncrementVersionIfDataHasChanged()
		{
			// Load New Data from database
			var newData = fTask.ResourceFile.LoadDataFromDatabase();
			// Audit Columns and AutoVersion column are updated by the system, they do not need to form part of the XML file
			RemoveAutoUpdatedColumns(newData);

			// Sets New DataSet version (DataSetName) to the same as the current one
			// and writes it to the source file
			newData.DataSetName = VersionInXmlSourceFile;
			fTask.ResourceFile.WriteXml(newData, fTask.ResourceFile.FileFullPath, XmlWriteMode.WriteSchema);

			// If the source file has changed (based on its source safe contents), increments the version
			if (!fTask.ExternalVersionBump && IsFileDiffenrentInSourceControl(fTask.ResourceFile.FileFullPath))
			{
				IncrementDataSetVersionNumber(newData);
				fTask.ResourceFile.WriteXml(newData, fTask.ResourceFile.FileFullPath, XmlWriteMode.WriteSchema);
				fState = TaskStateEnum.OutModified;
			}
		}

		void RemoveAutoUpdatedColumns(DataSet dataSet)
		{
			var columnsToRemove = new[]
			{
				"_" + AuditDetailsColumns.SystemCreateTimeUtc,
				"_" + AuditDetailsColumns.SystemCreateUser,
				"_" + AuditDetailsColumns.SystemLastEditTimeUtc,
				"_" + AuditDetailsColumns.SystemLastEditUser,
				"_AutoVersion",
			};

			foreach (DataTable table in dataSet.Tables)
			{
				var removeCount = 0;

				for (var index = table.Columns.Count - 1; index >= 0; index--)
				{
					var dataColumn = table.Columns[index];
					if (columnsToRemove.Any(c => dataColumn.ColumnName.EndsWith(c, StringComparison.InvariantCulture)))
					{
						table.Columns.RemoveAt(index);
						if (++removeCount == columnsToRemove.Length)
						{
							break;
						}
					}
				}
			}
		}

		protected string VersionInXmlSourceFile
		{
			get
			{
				try
				{
					DataSet currentSourceFileData = LoadDataFromSourceFile();
					return currentSourceFileData.DataSetName;
				}
				catch (System.Xml.XmlException ex)
				{
					if (ex.Message.StartsWith("root element is missing", StringComparison.OrdinalIgnoreCase))
					{
						return "0";
					}
					else
					{
						throw;
					}
				}
			}
		}

		bool IsFileDiffenrentInSourceControl(string path)
		{
			if (SourceControl.EnterpriseDatabase.IsFileInSourceControl(path))
			{
				return SourceControl.EnterpriseDatabase.IsDifferent(fTask.ResourceFile.FileFullPath);
			}
			else
			{
				return true;
			}
		}

		protected virtual void IncrementDataSetVersionNumber(DataSet data)
		{
			// Glow data can use a CRC instead of a sequential bump, but this function should only be called for Enterprise data.
			// Thus, no need to worry about that here.

			int newVersion = fTask.ResourceFile.GetVersion(data) + 1;
			data.DataSetName = Convert.ToString(newVersion);
		}

		/// <summary>
		/// Verifies if the UpgradeSetupController (i.e. the DataVersionFile) is checked-out by the current user.
		/// Throws a ControllerIsNotCheckedOutException if not.
		/// </summary>
		protected void VerifyIfControllerIsCheckedOut()
		{
			if (!fController.IsCheckedOutByMe)
			{
				if (SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(fTask.ResourceFile.FileFullPath))
				{
					((DataUpgradeSetupController)fController).CheckOut();
				}
				else
				{
					throw new ControllerNotCheckedOutByMeException();
				}
			}
		}

		#endregion
	}
}
