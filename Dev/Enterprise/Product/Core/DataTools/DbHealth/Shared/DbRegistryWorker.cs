using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Enterprise.DbHealth.Shared
{
	public interface IDbRegistryWorker
	{
		void ResetStartValues();
		void ResetCurrentValues();
		void LoadStartPoint();
		void SaveCurrentRunningStep(TimeSpan elapsedTime);

		string StartDatabase { get; }
		int StartStepForDb(string dbName);
		string StartTableForDb(string dbName);
		string StartViewForDb(string dbName);

		string CurrentDatabase { get; set; }
		int CurrentStep { get; set; }
		string CurrentTableOrView { get; set; }

		IEnumerable<string> GetDbListFromStartDb(IEnumerable<string> allDbNames);
		bool IsStuckAtInitialPoint { get; }
		string IsStuckMessage { get; }
		bool SteppingApplied { get; }
	}

	public abstract class DbRegistryWorker : IDbRegistryWorker
	{
		protected string startDb;
		protected int startStep;
		protected string startTableOrView;
		protected string currentDb;
		protected int currentStep;
		protected string currentTableOrView;

		public void LoadStartPoint()
		{
			startDb = LastCheckStepDatabase;
			startStep = LastCheckStep;
			startTableOrView = LastCheckStepTableOrView;
		}

		public static readonly string ElapsedTimeFormat = @"hh\:mm\:ss\.fff";

		public virtual void SaveCurrentRunningStep(TimeSpan elapsedTime)
		{
			LastCheckStep = currentStep;
			LastCheckStepTableOrView = currentTableOrView;
			LastCheckStepDatabase = currentDb;
			LastCheckStepElapsedTime = elapsedTime.ToString(DbRegistryWorker.ElapsedTimeFormat, CultureInfo.InvariantCulture);
		}

		public void ResetStartValues()
		{
			startDb = "";
			startStep = 0;
			startTableOrView = "";
		}

		public void ResetCurrentValues()
		{
			currentDb = "";
			currentStep = 0;
			currentTableOrView = "";
		}

		public int StartStepForDb(string dbName)
		{
			return (dbName == startDb) ? startStep : 0;
		}

		public string StartTableForDb(string dbName)
		{
			return (!SteppingApplied || StartStepForDb(dbName) == (int)StepsOfDbCheck.Tables) ? startTableOrView : "";
		}

		public string StartViewForDb(string dbName)
		{
			return (!SteppingApplied || StartStepForDb(dbName) == (int)StepsOfDbCheck.Views) ? startTableOrView : "";
		}

		public IEnumerable<string> GetDbListFromStartDb(IEnumerable<string> allDbNames)
		{
			if (allDbNames.Contains(startDb))
			{
				return allDbNames.SkipWhile(db => db != startDb);
			}
			else
			{
				ResetStartValues();
				return allDbNames;
			}
		}

		#region Registry items

		protected abstract int LastCheckStep { get; set; }
		protected abstract string LastCheckStepTableOrView { get; set; }
		protected abstract string LastCheckStepDatabase { get; set; }
		protected abstract string LastCheckStepElapsedTime { get; set; }

		#endregion

		#region Properties

		public string StartDatabase
		{
			get { return startDb; }
		}

		public string CurrentDatabase
		{
			get { return currentDb; }
			set { currentDb = value; }
		}

		public string CurrentTableOrView
		{
			get { return currentTableOrView; }
			set { currentTableOrView = value; }
		}

		public int CurrentStep
		{
			get { return currentStep; }
			set { currentStep = value; }
		}

		public bool IsStuckAtInitialPoint
		{
			get
			{
				if (string.IsNullOrWhiteSpace(startDb) && startStep == 0 && string.IsNullOrWhiteSpace(startTableOrView))
				{
					return false;
				}

				return startDb == currentDb && startStep == currentStep && startTableOrView == currentTableOrView;
			}
		}

		public abstract string IsStuckMessage { get; }

		public virtual bool SteppingApplied
		{
			get
			{
				return true;
			}
		}

		#endregion

	}

	#region Enums

	public enum StepsOfDbCheck
	{
		None = 0,
		Allocation = 1,
		Tables = 2,
		Catalog = 3,
		Views = 4,
	}
	#endregion
}
