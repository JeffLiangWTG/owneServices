using System;
using System.Data;

namespace Enterprise.DbHealth.Shared.Test
{
	public class DbRegistryWorkerForTest : DbRegistryWorker
	{
		DataTable _steps;

		public override void SaveCurrentRunningStep(TimeSpan elapsedTime)
		{
			RegStep = currentStep;
			RegTab = currentTableOrView;
			RegDb = currentDb;
			RegElapsedTime = elapsedTime.ToString(DbRegistryWorker.ElapsedTimeFormat);
		}

		public DataTable StepsTaken
		{
			get
			{
				if (_steps == null)
				{
					_steps = new DataTable();
					_steps.Columns.Add("db");
					_steps.Columns.Add("step", typeof(Int32));
					_steps.Columns.Add("obj");
				}

				return _steps;
			}
		}

		public string RegDb { get; set; }

		public string RegTab { get; set; }

		public int RegStep { get; set; }

		public string RegElapsedTime { get; set; }

		protected override int LastCheckStep
		{
			get { return RegStep; }
			set { RegStep = value; }
		}

		protected override string LastCheckStepTableOrView
		{
			get { return RegTab; }
			set { RegTab = value; }
		}

		protected override string LastCheckStepDatabase
		{
			get { return RegDb; }
			set { RegDb = value; }
		}

		protected override string LastCheckStepElapsedTime
		{
			get { return RegElapsedTime; }
			set { RegElapsedTime = value; }
		}

		public string StartDB
		{
			get { return startDb; }
			set { startDb = value; }
		}

		public string StartTabOrView
		{
			get { return startTableOrView; }
			set { startTableOrView = value; }
		}

		public int StartStep
		{
			get { return startStep; }
			set { startStep = value; }
		}

		public override string IsStuckMessage
		{
			get { return "Test message"; }
		}
	}
}
