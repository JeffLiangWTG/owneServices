#if DEBUG

namespace Enterprise.Builder.DataUpgradeSetup
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using CargoWise.BuildTools;
	using CargoWise.Common;
	using CargoWise.Data;
	using Enterprise.DbUpgrader.Data;
	using Enterprise.ZArchitecture.Core;
	using WTG.DevTools.Definitions;
	public interface ISetupController
	{
		bool IsCheckedOutByMe { get; }
		void FullCheckOut();
		void FullUndoCheckOut();
		void FullSave();
	}

	/// <summary>
	/// Class to manage the setup process of the xml files, enabling developers to change the data
	/// used for data upgrade. Available only in DEBUG mode.
	/// </summary>
	public class DataUpgradeSetupController : ISetupController
	{
		public DataUpgradeSetupController(UpgradeTask[] tasks)
		{
			foreach (UpgradeTask task in tasks)
			{
				if (task.GetType() == typeof(DocumentsUpgradeTask))
				{
					TaskSetupList.Add(new DataTaskSetup(new DocumentsUpgradeTask(new DocumentsDataFile()), this, task));
				}
				else
				{
					TaskSetupList.Add(new DataTaskSetup(task, this));
				}
			}
		}

#region Full Operation Methods

		/// <summary>
		/// 1 - Checks out this controller
		/// 2 - Checks out each of its DataTaskSetup items
		/// </summary>
		public virtual void FullCheckOut()
		{
			CheckOut();
			foreach (DataTaskSetup taskSetup in TaskSetupList)
			{
				taskSetup.CheckOut();
			}
		}

		/// <summary>
		/// 1 - Undoes check out of each of its DataTaskSetup items
		/// 2 - Undoes check out of this controller
		/// </summary>
		public void FullUndoCheckOut()
		{
			foreach (DataTaskSetup taskSetup in TaskSetupList)
			{
				taskSetup.UndoCheckOut();
			}
			UndoCheckOut();
		}

		/// <summary>
		/// Update all XML files from database content
		/// </summary>
		public void FullSave()
		{
			foreach (DataTaskSetup taskSetup in TaskSetupList)
			{
				taskSetup.SaveDataToSourceFile();
			}
		}

		/// <summary>
		/// Apply data on XML to the database
		/// </summary>
		public void FullApplyToDatabase()
		{
			using (Db.DisposableActionForDbConnection())
			{
				foreach (DataTaskSetup taskSetup in TaskSetupList)
				{
					taskSetup.ApplyXmlFileDataToDatabase();
				}
			}
		}

#endregion

#region DataVersion File Source Safe Control

		/// <summary>
		/// 1 - Checks out / Verifies if BuildLock is checked-out by the current user
		/// 2 - Checks out the DataVersion file
		/// 3 - Always gets latest, replacing local with SourceControl version
		/// 4 - Increments the version number
		/// </summary>
		public void CheckOut()
		{
			VerifyDbSchemaVersionIsLatest();

			SourceControl.EnterpriseDatabase.CheckOut(DataVersionFile, false);

			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				CheckDataVersionFile("1");
			}

			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				CheckDataVersionFile("2");
			}

			IncrementVersion();
		}

		void CheckDataVersionFile(string suffix)
		{
			bool localFileExists = File.Exists(DataVersionFile);
			bool localFileWritable = (File.GetAttributes(DataVersionFile) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly;
			string serverFilePath = DataVersionFile.Replace("MockSourceControlFiles", "SourceControl");
			bool serverFileExists = File.Exists(serverFilePath);
			bool serverFileWritable = (File.GetAttributes(serverFilePath) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly;
			bool localFileCheckedOutByMe = SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(DataVersionFile);

			bool correctStatus = true;

			if (serverFilePath != DataVersionFile)
			{
				correctStatus = localFileExists && localFileWritable && serverFileExists && !serverFileWritable && localFileCheckedOutByMe;
			}

			if (!correctStatus)
			{
				StringBuilder errorTextBuilder = new StringBuilder();
				errorTextBuilder.Append("Error encountered at: " + suffix + System.Environment.NewLine);
				errorTextBuilder.Append("LocalFileExists: " + localFileExists.ToString() + System.Environment.NewLine);
				errorTextBuilder.Append("LocalFileWritable: " + localFileWritable.ToString() + System.Environment.NewLine);
				errorTextBuilder.Append("ServerFilePath: " + serverFilePath + System.Environment.NewLine);
				errorTextBuilder.Append("ServerFileWritable: " + serverFileWritable.ToString() + System.Environment.NewLine);
				errorTextBuilder.Append("LocalFileCheckedOutByMe: " + localFileCheckedOutByMe.ToString() + System.Environment.NewLine);
				throw new Exception(errorTextBuilder.ToString());
			}
		}

		public void UndoCheckOut()
		{
			VerifyNoTaskIsCheckedOut();
			SourceControl.EnterpriseDatabase.UndoCheckOut(DataVersionFile, false);
		}

		public bool IsCheckedOutByMe
		{
			get
			{
				try
				{
					return SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(DataVersionFile);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					return false;
				}
			}
		}

#endregion

#region VerifyDbSchemaVersionIsLatest

		protected virtual void VerifyDbSchemaVersionIsLatest()
		{
			if (!NUnit.Framework.TestingState.IsRunningTests)
			{
				DbVersion.EnsureLatest();
			}
		}

#endregion

		/// <summary>
		/// Virtual for testing purposes only
		/// </summary>
		public virtual string DataVersionFile
		{
			get { return BuildConstants.GetLocalPath(dataVersionFileRelativePath); }
		}

		public List<string> CheckedOutTaskDataFilePathList
		{
			get
			{
				List<string> result = new List<string>();

				foreach (DataTaskSetup taskSetup in TaskSetupList)
				{
					if (taskSetup.State == TaskStateEnum.OutUnchanged || taskSetup.State == TaskStateEnum.OutModified)
					{
						result.Add(taskSetup.Task.ResourceFile.FileFullPath);
					}
				}

				return result;
			}
		}

#region Implementation

		protected readonly List<DataTaskSetup> TaskSetupList = new List<DataTaskSetup>();
		protected const string dataVersionFileRelativePath = @"Database\Odyssey\Resource\Version\AutoGenerated\DataVersion.cs";
		protected const string AppMajorVersionPattern = @"(?<=\bApplication\b\s*=\s*new\s*VersionLabel\s*\(\s*)([0-9]+)(?=\s*,\s*[0-9]+\s*\)\s*;)";
		protected const string AppMinorVersionPattern = @"(?<=\bApplication\b\s*=\s*new\s*VersionLabel\s*\(\s*[0-9]+\s*,\s*)([0-9]+)(?=\s*\)\s*;)";

		/// <summary>
		/// MINOR VERSION MUST ALWAYS BE 0 (ZERO) IN ALPHA RELEASE (RING 0)
		/// MAJOR VERSION MUST NOT BE CHANGED IN RELEASES OTHER THAN ALPHA
		/// </summary>
		void IncrementVersion()
		{
			var versionFileContents = File.ReadAllText(DataVersionFile);

			if (ReleaseInfo.Instance.ReleaseRing == ReleaseRings.Codes.ALP)
			{
				var currentMajorVersion = Convert.ToInt32(Regex.Match(versionFileContents, AppMajorVersionPattern).Value);
				var newMajor = currentMajorVersion + 1;
				versionFileContents = Regex.Replace(versionFileContents, AppMajorVersionPattern, newMajor.ToString());
			}
			else
			{
				var currentMinorVersion = Convert.ToInt32(Regex.Match(versionFileContents, AppMinorVersionPattern).Value);
				var newMinor = currentMinorVersion + 1;
				versionFileContents = Regex.Replace(versionFileContents, AppMinorVersionPattern, newMinor.ToString());
			}

			if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
			{
				CheckDataVersionFile("3");
			}

			File.WriteAllText(DataVersionFile, versionFileContents);
		}

		void VerifyNoTaskIsCheckedOut()
		{
			foreach (DataTaskSetup taskSetup in TaskSetupList)
			{
				if (taskSetup.State == TaskStateEnum.OutUnchanged || taskSetup.State == TaskStateEnum.OutModified)
				{
					throw new ControllerHasUncheckedSetupTasksException();
				}
			}
		}

#endregion
	}
}
#endif
