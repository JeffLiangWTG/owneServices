using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.Builder.GenerateDbUpgraderResources
{
	public delegate void SetupUpgraderEvent(string message);

	/// <summary>
	/// Prepare Database Upgrader resource files with the new schema scripts and version.
	/// </summary>
	class SetupController
	{
		public SetupUpgraderEvent OnTaskStarted;
		public SetupUpgraderEvent OnTaskFailed;

		public SetupController()
		{
		}

		public bool RunSetup(bool isMinorSetup, bool isBumpVersion, bool isMerge)
		{
			RegenActionEnum regenAction = (isBumpVersion) ? RegenActionEnum.BumpOnly : RegenActionEnum.Full;
			if (isMerge)
			{
				regenAction = RegenActionEnum.Merge;
			}
			RegenVersionChangeEnum regenVersionChange = (isMinorSetup) ? RegenVersionChangeEnum.ForceMinor : RegenVersionChangeEnum.Standard;

			return RunSetup(regenAction, regenVersionChange);
		}

		bool RunSetup(RegenActionEnum regenAction, RegenVersionChangeEnum regenVersionChange)
		{
			bool result = false;

			if (CheckOutUpgraderFiles(regenAction))
			{
				try
				{
					RecreateUpgraderFiles(regenAction, regenVersionChange);
					result = true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					RaiseTaskError(ex.Message);
					UndoCheckOutUpgraderFiles();
				}
			}

			return result;
		}

		/// <summary>
		/// Get list of files to add to GeneratorOutputDirectory
		/// (instead of check them in)
		/// </summary>
		public List<string> GetUpgraderFilesToCheckin()
		{
			List<string> files = new List<string>();

			try
			{
				files.AddRange(SchemaSetup.GetFilesToCheckIn());
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				RaiseTaskError(ex.Message);
				return null;
			}

			return files;
		}

		public bool UndoCheckOutUpgraderFiles()
		{
			try
			{
				SchemaSetup.UndoCheckOutFiles();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				RaiseTaskError(ex.Message);
				return false;
			}

			return true;
		}

		#region Implementation

		#region Update DbUpgrader Files

		protected bool CheckOutUpgraderFiles(RegenActionEnum regenAction)
		{
			try
			{
				SchemaSetup.CheckOutFiles(regenAction);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				RaiseTaskError(ex.Message);
				return false;
			}

			return true;
		}

		void RecreateUpgraderFiles(RegenActionEnum regenAction, RegenVersionChangeEnum regenVersionChange)
		{
			if (regenAction != RegenActionEnum.Merge)
			{
				StartTask("Recreating latest schema resource and version files");
				SchemaSetup.RegenarateFiles(regenAction, regenVersionChange);
			}
		}

		SchemaRegenSetup SchemaSetup
		{
			get
			{
				if (schemaSetup == null)
				{
					schemaSetup = new SchemaRegenSetup();
				}

				return schemaSetup;
			}
		}

		SchemaRegenSetup schemaSetup;

		#endregion

		#region Event Firing

		void StartTask(string task)
		{
			if (OnTaskStarted != null)
			{
				OnTaskStarted(task);
			}
		}

		void RaiseTaskError(string errorMessage)
		{
			if (OnTaskFailed != null)
			{
				OnTaskFailed(errorMessage);
			}
		}

		#endregion

		#endregion
	}
}
