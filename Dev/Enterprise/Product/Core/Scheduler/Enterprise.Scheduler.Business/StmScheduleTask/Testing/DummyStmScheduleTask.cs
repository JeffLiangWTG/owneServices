#if DEBUG

using System;
using System.ComponentModel;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Scheduler.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	internal class DummyReloadedStmScheduleTask : DummyStmScheduleTask
	{
		public DummyReloadedStmScheduleTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}

	[TestExcludeBusinessObjectsAllHaveTestCases]
	public class DummyStmScheduleTask : StmScheduleTask
	{
		public DummyStmScheduleTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetupKeys();
		}

		protected override bool IsErrorThatShouldRetry(Exception ex)
		{
			return ex is NotImplementedException;
		}

		protected override TimeSpan RetryWaitPeriod
		{
			get { return TimeSpan.FromMilliseconds(100); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S5_ParentTableCode = DummyBizoSchema.Constants.Prefix;
		}

		public new bool IsAutoLogged
		{
			get { return base.IsAutoLogged; }
		}

		public new bool IsAutoLogOnlyEnabledForACT
		{
			get { return base.IsAutoLogOnlyEnabledForACT; }
		}

		public bool RunCalled
		{
			get { return BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.RunCalled]; }
			set
			{
				BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.RunCalled] = value;
				UpdateS5_ScheduleDescription();
			}
		}

		public bool ThrowExceptionInvalidOp
		{
			get { return BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.ThrowExceptionInvalidOp]; }
			set
			{
				BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.ThrowExceptionInvalidOp] = value;
				UpdateS5_ScheduleDescription();
			}
		}

		public bool ThrowExceptionNotImplemented
		{
			get { return BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.ThrowExceptionNotImplemented]; }
			set
			{
				BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.ThrowExceptionNotImplemented] = value;
				UpdateS5_ScheduleDescription();
			}
		}

		public bool ThrowExceptionSql
		{
			get { return BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.ThrowExceptionSql]; }
			set
			{
				BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.ThrowExceptionSql] = value;
				UpdateS5_ScheduleDescription();
			}
		}

		public bool SimulateTimeout
		{
			get { return BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.SimulateTimeout]; }
			set
			{
				BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.SimulateTimeout] = value;
				UpdateS5_ScheduleDescription();
			}
		}

		public bool NotifyScheduleRunError
		{
			get { return BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.NotifyScheduleRunError]; }
			set
			{
				BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.NotifyScheduleRunError] = value;
				UpdateS5_ScheduleDescription();
			}
		}

		public bool NotifyScheduleRunWarning
		{
			get { return BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.NotifyScheduleRunWarning]; }
			set
			{
				BooleanKeyValuePairs[S5_ScheduleDescriptionKeys.NotifyScheduleRunWarning] = value;
				UpdateS5_ScheduleDescription();
			}
		}

		public int RunCount
		{
			get { return IntKeyValuePairs[S5_ScheduleDescriptionKeys.RunCount]; }
			set
			{
				IntKeyValuePairs[S5_ScheduleDescriptionKeys.RunCount] = value;
				UpdateS5_ScheduleDescription();
			}
		}

		void UpdateS5_ScheduleDescription()
		{
			S5_ScheduleDescription = BooleanKeyValuePairs.ToString() + IntKeyValuePairs.ToString();
		}

		protected override int PriorityCore => IntKeyValuePairs[S5_ScheduleDescriptionKeys.Priority];

		public void SetPriority(int priority)
		{
			IntKeyValuePairs[S5_ScheduleDescriptionKeys.Priority] = priority;
			UpdateS5_ScheduleDescription();
		}

		public override void OnLoaded()
		{
			SetupKeys();
			string valuesToUse = S5_ScheduleDescription.IndexOf("=") == -1 ? BooleanKeyValuePairs.ToString() + IntKeyValuePairs.ToString() : S5_ScheduleDescription.ToString();
			BooleanKeyValuePairs.LoadFromString(valuesToUse);
			IntKeyValuePairs.LoadFromString(valuesToUse);
		}

		void SetupKeys()
		{
			if (BooleanKeyValuePairs.Count == 0)
			{
				foreach (S5_ScheduleDescriptionKeys key in SupportedBooleanKeyPairs)
				{
					BooleanKeyValuePairs.Add(key, false);
				}
			}
			if (IntKeyValuePairs.Count == 0)
			{
				foreach (S5_ScheduleDescriptionKeys key in SupportedIntKeyPairs)
				{
					IntKeyValuePairs.Add(key, 0);
				}
			}
		}

		readonly BooleanKeyValuePairs BooleanKeyValuePairs = new BooleanKeyValuePairs();
		readonly IntKeyValuePairs IntKeyValuePairs = new IntKeyValuePairs();

		readonly S5_ScheduleDescriptionKeys[] SupportedBooleanKeyPairs = new[]
		{
			S5_ScheduleDescriptionKeys.RunCalled, S5_ScheduleDescriptionKeys.NotifyScheduleRunError, S5_ScheduleDescriptionKeys.NotifyScheduleRunWarning,
			S5_ScheduleDescriptionKeys.ThrowExceptionInvalidOp, S5_ScheduleDescriptionKeys.ThrowExceptionNotImplemented, S5_ScheduleDescriptionKeys.ThrowExceptionSql,
			S5_ScheduleDescriptionKeys.SimulateTimeout
		};

		readonly S5_ScheduleDescriptionKeys[] SupportedIntKeyPairs = new[] { S5_ScheduleDescriptionKeys.RunCount, S5_ScheduleDescriptionKeys.Priority };

		public static Guid LastBranch
		{
			get { return overridableLastBranch.Value; }
			set { overridableLastBranch.Value = value; }
		}

		public static readonly Overridable<Guid> overridableLastBranch = new Overridable<Guid>(Guid.Empty);

		public static string LastUserLoginName
		{
			get { return overridableLastUserLoginName.Value; }
			set { overridableLastUserLoginName.Value = value; }
		}

		public static readonly Overridable<string> overridableLastUserLoginName = new Overridable<string>(string.Empty);

		protected override void RunCore(INotifications notifications, CancellationToken token)
		{
			LastBranch = Environment.Env.CurrentBranch.PK;
			LastUserLoginName = Environment.Env.CurrentUser.LoginName;

			base.RunCore(notifications, token);
			this.RunCalled = true;
			this.RunCount++;
			Factory.Save();

			if (ThrowExceptionInvalidOp)
			{
				throw new InvalidOperationException();
			}
			if (ThrowExceptionNotImplemented)
			{
				throw new NotImplementedException();
			}
			if (ThrowExceptionSql)
			{
				var error = SqlExceptionBuilder.CreateSqlError(1000, 1, 1, "", "", "", 1);
				var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);
				throw exception;
			}
			if (SimulateTimeout)
			{
				Thread.Sleep(10000);
			}
			if (NotifyScheduleRunError)
			{
				notifications.AddError("An error occurred while processing the task");
			}
			if (NotifyScheduleRunWarning)
			{
				notifications.AddWarning("Here is a warning");
			}
			if (StmReportRun != null && !NotifyScheduleRunError)
			{
				StmReportRun.RRI_Status = StmReportRunState.Finished;
				//StmReportRun.Factory isn't owned by us, so we can't use it (add new bizos in it) without causing CrossThreadAccessException
				var stmReportRunInLocalFactory = Factory.Load<StmReportRun>(StmReportRun.PK);
				var note = stmReportRunInLocalFactory.Notes.AddNew();
				note.ST_Description = "Dummy Note";
				note.ST_NoteDataAsText = "hi";
				note.ST_NoteType = nameof(StmNoteVisibility.PUB);
			}

			Factory.Save();
		}

		public override ZString DescriptionForLog
		{
			get
			{
				ZString description = S5_ScheduleDescription.IsEmpty ? "" : string.Format("Description: {0}", S5_ScheduleDescription);
				var parentMenu = Factory.Load<StmMenuItem>(S5_ParentID);
				if (parentMenu != null)
				{
					string nameOfReport = string.Format("Report name: {0}", parentMenu.SU_MenuName);
					description = description.IsEmpty ? nameOfReport : nameOfReport + ", " + description;
				}
				return description;
			}
		}

		public bool DontRun { get; set; }
		public bool MarkAsRunning { get; set; }

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			SetupKeys();
			S5_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			S5_ParentID = Guid.NewGuid();

			HasChanges = false;
		}
#endif
		#endregion
	}

	enum S5_ScheduleDescriptionKeys
	{
		RunCalled,
		ThrowExceptionInvalidOp,
		ThrowExceptionNotImplemented,
		ThrowExceptionSql,
		NotifyScheduleRunError,
		NotifyScheduleRunWarning,
		RunCount,
		SimulateTimeout,
		Priority,
		None = 10
	}

	class BooleanKeyValuePairs : KeyValuePairs<bool>
	{
		protected override bool TryParseValue(string value, out bool result)
		{
			return bool.TryParse(value, out result);
		}
	}

	class IntKeyValuePairs : KeyValuePairs<int>
	{
		protected override bool TryParseValue(string value, out int result)
		{
			return int.TryParse(value, out result);
		}
	}

	abstract class KeyValuePairs<T> : System.Collections.Generic.Dictionary<S5_ScheduleDescriptionKeys, T>
	{
		public override string ToString()
		{
			string result = "";
			foreach (S5_ScheduleDescriptionKeys key in this.Keys)
			{
				result += ((int)key).ToString() + "=" + this[key].ToString() + Separator;
			}
			return result;
		}

		public void LoadFromString(string value)
		{
			this.Clear();
			string[] keyPairs = value.Split(new string[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string keyPair in keyPairs)
			{
				string[] keyAndPair = keyPair.Split(new char[] { '=' });
				T result;
				if (TryParseValue(keyAndPair[1], out result))
				{
					this.Add(GetKeyFromint(int.Parse(keyAndPair[0])), result);
				}
			}
		}

		const string Separator = " $";

		protected abstract bool TryParseValue(string value, out T result);

		S5_ScheduleDescriptionKeys GetKeyFromint(int key)
		{
			S5_ScheduleDescriptionKeys result = (S5_ScheduleDescriptionKeys)key;
			return result;
		}
	}
}

#endif
