using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTask : AutoStmScheduleTask, ITemplateCopyable, IStmScheduleTask, IForceLogsOnParentForNewWithNoHasChanges
	{
		public StmScheduleTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
		}

		#region BusinessObject Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return DescriptionForLog;
			}
		}

		#region Type Decider

		public static readonly StmScheduleTaskTypeDecider TypeDecider = new StmScheduleTaskTypeDecider();

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (GlbBranch.CurrentBranch != null)
			{
				S5_GB = GlbBranch.CurrentBranch.PK;
				S5_StartDate = Env.Time.GetUtcFromUnlocoTime(GlbBranch.CurrentBranch.GB_RL_NKHomePort, ZDateTime.Today.ToDateTime());
				S5_IsActive = true;
				Recurrence.DailyRange = true;
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			Recipients.RemoveAndDeleteAll();
			StmReportRuns.RemoveAllFromRelationship();
			base.Delete();
		}

		#endregion

		#region Logging

		protected override bool IsAutoLogOnlyEnabledForACT => true;

		#endregion

		#endregion

		#region Related Business Objects

		#region Recipients

		[ChildEditable(true)]
		public StmScheduleTaskRecipientDependentCollection Recipients
		{
			get
			{
				if (recipients == null)
				{
					recipients = NewRecipients();
					recipients.Load();
					RegisterEditableChildObject(recipients);
				}
				return recipients;
			}
		}
		StmScheduleTaskRecipientDependentCollection recipients;

		protected virtual StmScheduleTaskRecipientDependentCollection NewRecipients()
		{
			return new StmScheduleTaskRecipientDependentCollection(this);
		}

		#endregion

		#region Recurrence

		public StmScheduleTaskRecurrence Recurrence
		{
			get
			{
				if (recurrence == null)
				{
					recurrence = NewStmScheduleTaskRecurrence();
					RegisterEditableChildObject(recurrence);
				}
				return recurrence;
			}
		}
		StmScheduleTaskRecurrence recurrence;

		protected virtual StmScheduleTaskRecurrence NewStmScheduleTaskRecurrence()
		{
			return new StmScheduleTaskRecurrence(this);
		}

		#endregion

		#region StmReportRun

		public StmReportRun StmReportRun { get; set; }

		public StmReportRunCollection StmReportRuns
		{
			get
			{
				if (stmReportRuns == null)
				{
					stmReportRuns = new StmReportRunCollection(this);
				}
				return stmReportRuns;
			}
		}
		StmReportRunCollection stmReportRuns;

		public StmReportRun RunningReport
		{
			get { return StmReportRuns.OrderByDescending(x => x.RRI_StartTimeUtc).FirstOrDefault(x => x.RRI_Status == Constants.StmReportRunState.Running); }
		}

		public StmReportRun LastProcessedReport
		{
			get { return StmReportRuns.OrderByDescending(x => x.RRI_StartTimeUtc).FirstOrDefault(x => x.RRI_Status == Constants.StmReportRunState.Finished); }
		}

		#endregion

		#endregion

		#region Properties

		#region S5_ScheduleDescriptionMultilingual

		public MultilingualString S5_ScheduleDescriptionMultilingual
		{
			get { return CustomizableDataResourceStrings.GetMultilingualString(null, S5_ScheduleDescription); }
		}

#if DEBUG
		internal
#endif
		CustomizableDataResourceStrings CustomizableDataResourceStrings
		{
			get
			{
				if (customizableDataResourceStrings == null)
				{
					customizableDataResourceStrings = new CustomizableDataResourceStrings(ObjectFactory.Get<ICustomizableDataCaptionSource>("StmScheduleTaskDescriptionCaptionSource"));
				}
				return customizableDataResourceStrings;
			}
		}
		CustomizableDataResourceStrings customizableDataResourceStrings;

		#endregion

		#region S5_ParentID

		public override ZGuid S5_ParentID
		{
			get { return base.S5_ParentID; }
			set
			{
				if (base.S5_ParentID != value)
				{
					base.S5_ParentID = value;
					S5_ScheduleState = ZBlob.Empty;
				}
			}
		}

		#endregion

		#region S5_NextScheduledPrintRunTimeUtc

		ZDateTime CalculateNextRunTime(ZDateTime utcDate)
		{
			var dailyStartTimeLocal = S5_DailyStartTime;

			var originalUtcDate = utcDate;
			var utcToday = GetUtcDate(dailyStartTimeLocal.IsValid ? ZDateTime.Now : ZDateTime.Today);

			if (!utcDate.IsValid || utcDate < utcToday)
			{
				utcDate = utcToday;
			}

			ZDateTime result = utcDate;

			var taskBranch = Branch;
			var currentBranch = Env.CurrentBranch;

			if (taskBranch != null && currentBranch != null && taskBranch.GB_Code != currentBranch.Code && dailyStartTimeLocal.IsValid)
			{
				dailyStartTimeLocal = Env.Time.GetTimeInOneZoneFromTimeInAnotherZone(taskBranch.GB_RL_NKHomePort, dailyStartTimeLocal.ToDateTime(), currentBranch.NKUNLOCO);
			}

			if (dailyStartTimeLocal.IsValid) //valid: specific local time of day
			{
				//Convert to local time, apply local time of day. By definition this can't move us back or forward a local day.

				DateTime resultLocal = GetLocalDate(result).ToDateTime().Date.Add(dailyStartTimeLocal.TimeOfDay);
				result = GetUtcDate(resultLocal);
			}

			if (result < UtcNow && S5_NextScheduledPrintRunTimeUtc.IsValid && originalUtcDate != S5_StartDate) //attempting to run the report in the past
			{
				//Previously this was reported as an issue. But, it also occurs if the SRR service task has been offline for a while.
				//So we should just fix the situation and walk away.

				switch (recurrence.TaskPeriod)
				{
					case ScheduleRecurrenceType.Hourly:
						result = UtcNow + new TimeSpan(0, recurrence.TaskPeriodCount, 0, 0);
						break;

					case ScheduleRecurrenceType.Minute:
						result = UtcNow + new TimeSpan(0, 0, recurrence.TaskPeriodCount, 0);
						break;

					case ScheduleRecurrenceType.Second:
						result = UtcNow + new TimeSpan(0, 0, 0, recurrence.TaskPeriodCount);
						break;

					default:
						result = result.AddDays(1);
						break;
				}
			}

			return result;
		}

		protected ZDateTime UtcNow
		{
			get
			{
				var result = ZDateTime.UtcNow;
				if (UtcOffsetOverride.HasValue)
				{
					result = ZDateTime.Now.Add(-UtcOffsetOverride.Value);
				}
				return result;
			}
		}

		protected ZDateTime GetLocalDate(ZDateTime utcDate)
		{
			if (utcDate.IsEmpty || !utcDate.IsValid)
			{
				return utcDate;
			}

			if (UtcOffsetOverride.HasValue)
			{
				return utcDate.Add(UtcOffsetOverride.Value);
			}

			return Env.Time.GetLocalTimeFromUtc(utcDate.ToDateTime());
		}

		ZDateTime GetUtcDate(ZDateTime localDate)
		{
			if (localDate.IsEmpty || !localDate.IsValid)
			{
				return localDate;
			}

			if (UtcOffsetOverride.HasValue)
			{
				return localDate.Add(-UtcOffsetOverride.Value);
			}

			return Env.Time.GetUtcFromLocalTime(localDate.ToDateTime());
		}

		#region CalcNextRunTimeLocal

		[BusinessObjectTestExclude]
		[ResourceStringData("StmScheduleTask.CalcNextRunTimeLocal", Caption = "Next Run Time (local)")]
		public ZDateTime CalcNextRunTimeLocal
		{
			get
			{
				return GetLocalDate(NextRunTime);
			}
			set
			{
				NextRunTime = GetUtcDate(value);
			}
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("StmScheduleTask.CalcNextRunTimeLocalText")]
		public ZString CalcNextRunTimeLocalText
		{
			get
			{
				return $"{CalcNextRunTimeLocal.ToLongTimeString()}";
			}
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("StmScheduleTask.NextRunTime", Caption = "Next Run Time")]
		public ZDateTime NextRunTime
		{
			get
			{
				var result = S5_NextScheduledPrintRunTimeUtc.IsValid ?
					S5_NextScheduledPrintRunTimeUtc :
					ZDateTime.Empty;

				return result;
			}
			set
			{
				if (value.IsValid && S5_NextScheduledPrintRunTimeUtc != value)
				{
					S5_NextScheduledPrintRunTimeUtc = value;
					CalcNextRunTimeLocalInfo.RefreshBinding();
				}
			}
		}

		public ZWrappedPropertyInfo CalcNextRunTimeLocalInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetWrappedZPropertyInfo(nameof(CalcNextRunTimeLocal), x => S5_NextScheduledPrintRunTimeUtcInfo); }
		}

		#endregion

		#endregion

		#region S5_DateScheduleFirstRun

		[ReadOnly(true)]
		public override ZDateTime S5_DateScheduleFirstRun
		{
			get { return base.S5_DateScheduleFirstRun; }
			set { base.S5_DateScheduleFirstRun = value; }
		}

		#endregion

		#region S5_ScheduleActualRunCount

		[ReadOnly(true)]
		public override ZInt S5_ScheduleActualRunCount
		{
			get { return base.S5_ScheduleActualRunCount; }
			set { base.S5_ScheduleActualRunCount = value; }
		}

		#endregion

		#region S5_IsPrivate

		[ReadOnly(true)]
		public override ZBool S5_IsPrivate
		{
			get { return base.S5_IsPrivate; }
			set { base.S5_IsPrivate = value; }
		}

		#endregion

		#region S5_StartDate

		public override ZDateTime S5_StartDate
		{
			get { return base.S5_StartDate; }
			set
			{
				if (value != base.S5_StartDate)
				{
					base.S5_StartDate = value;

					if (S5_IsPrivate)
					{
						S5_NextScheduledPrintRunTimeUtc = CalculateNextRunTime(S5_StartDate);
					}
				}
			}
		}

		public ZDateTime CalcStartDateLocal
		{
			get
			{
				var result = S5_StartDate.IsValid ?
					GetLocalDate(S5_StartDate) :
					ZDateTime.Empty;

				return result;
			}
			set
			{
				if (value.IsValid)
				{
					ZDateTime utcValue = GetUtcDate(value.ToDateTime());

					if (utcValue.IsValid && S5_StartDate != utcValue)
					{
						S5_StartDate = utcValue;
						CalcNextRunTimeLocalInfo.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region CalcEndDateLocal

		public ZDateTime CalcEndDateLocal => S5_EndDate.IsValid ? GetLocalDate(S5_EndDate) : ZDateTime.Empty;

		#endregion

		#region S5_DailyStartTime

		[BusinessObjectTestExclude]
		public override ZDateTime S5_DailyStartTime
		{
			get
			{
				return base.S5_DailyStartTime;
			}
			set
			{
				if (value != base.S5_DailyStartTime)
				{
					base.S5_DailyStartTime = (value.IsValid) ? ZDateTime.MinSmallDateTimeValue.Date.Add(value.TimeOfDay) : value;
				}
			}
		}

		#region CalcDailyStartTimeUtc

		[BusinessObjectTestExclude]
		public ZDateTime CalcDailyStartTimeUtc
		{
			get
			{
				return GetTimeOnlyUtcFromTaskLocal(S5_DailyStartTime);
			}
			set
			{
				SetTimeOnlyTaskLocalFromUtcIfValid(S5_DailyStartTimeInfo, value);
			}
		}

		#endregion

		#region CalcDailyStartTimeLocal

		[BusinessObjectTestExclude]
		public ZDateTime CalcDailyStartTimeLocal
		{
			get
			{
				return GetTimeOnlyUserLocalFromTaskLocal(S5_DailyStartTime);
			}
			set
			{
				SetTimeOnlyTaskLocalFromUserLocalValueIfValid(S5_DailyStartTimeInfo, value);
				CalcDailyStartTimeLocalInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CalcDailyStartTimeLocalInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(CalcDailyStartTimeLocal)); }
		}

		#endregion

		TimeSpan GetUtcOffsetForTaskUNLOCOOrFallback(DateTime utcNow)
		{
			if (UtcOffsetOverride.HasValue)
			{
				return UtcOffsetOverride.Value;
			}

			if (Branch != null)
			{
				return Env.Time.GetUtcOffsetBasedOnUtc(((IBranch)Branch).NKUNLOCO, utcNow);
			}
			return Env.Time.GetUtcOffsetBasedOnUtc(utcNow);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		void SetTimeOnlyTaskLocalFromUtcIfValid(ZPropertyInfo ptyToSet, ZDateTime utcValue)
		{
			if (utcValue.IsValid)
			{
				DateTime utcNow = DateTime.UtcNow;

				TimeSpan localScheduleTaskOffset = GetUtcOffsetForTaskUNLOCOOrFallback(utcNow);

				ZDateTime localScheduleTaskTimeOfDay = utcValue.Add(localScheduleTaskOffset);
				localScheduleTaskTimeOfDay = ZDateTime.MinSmallDateTimeValue.Date.Add(localScheduleTaskTimeOfDay.TimeOfDay);

				if (localScheduleTaskTimeOfDay.IsValid && !ptyToSet.Value.Equals(localScheduleTaskTimeOfDay))
				{
					SetPropertyValue(ptyToSet, localScheduleTaskTimeOfDay);
				}
			}
			else if (utcValue.IsEmpty)
			{
				SetPropertyValue(ptyToSet, utcValue);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		ZDateTime GetTimeOnlyUtcFromTaskLocal(ZDateTime localValue)
		{
			if (localValue.IsValid)
			{
				DateTime utcNow = DateTime.UtcNow;

				TimeSpan localScheduleTaskOffset = GetUtcOffsetForTaskUNLOCOOrFallback(utcNow);

				ZDateTime utcTimeOfDay = localValue.Add(-localScheduleTaskOffset);
				utcTimeOfDay = ZDateTime.MinSmallDateTimeValue.Date.Add(utcTimeOfDay.TimeOfDay);

				return utcTimeOfDay;
			}
			else
			{
				return localValue;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		void SetTimeOnlyTaskLocalFromUserLocalValueIfValid(ZPropertyInfo ptyToSet, ZDateTime localValue)
		{
			if (localValue.IsValid)
			{
				//We have to convert the local time (as seen by the current logged in branch) to the local time (as seen by the schedule task's branch).
				//We do this by subtracting the offset of the current branch and adding the offset of the schedule task's branch.
				//The current DST offsets in both time zones will be used.

				DateTime utcNow = DateTime.UtcNow;

				TimeSpan localUserOffset = Env.Time.GetUtcOffsetBasedOnUtc(utcNow);
				TimeSpan localScheduleTaskOffset = GetUtcOffsetForTaskUNLOCOOrFallback(utcNow);

				ZDateTime localScheduleTaskTimeOfDay = localValue.Add(-localUserOffset).Add(localScheduleTaskOffset);
				localScheduleTaskTimeOfDay = ZDateTime.MinSmallDateTimeValue.Date.Add(localScheduleTaskTimeOfDay.TimeOfDay);

				if (localScheduleTaskTimeOfDay.IsValid && !ptyToSet.Value.Equals(localScheduleTaskTimeOfDay))
				{
					SetPropertyValue(ptyToSet, localScheduleTaskTimeOfDay);
				}
			}
			else if (localValue.IsEmpty)
			{
				SetPropertyValue(ptyToSet, localValue);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		ZDateTime GetTimeOnlyUserLocalFromTaskLocal(ZDateTime localValue)
		{
			if (localValue.IsValid)
			{
				//We have to convert the local time (as seen by the schedule task's branch) to the local time (as seen by the currentl ogged in branch).
				//We do this by subtracting the offset of the schedule task's branch and adding the offset of the current branch.
				//The current DST offsets in both time zones will be used.

				DateTime utcNow = DateTime.UtcNow;

				TimeSpan localUserOffset = Env.Time.GetUtcOffsetBasedOnUtc(utcNow);
				TimeSpan localScheduleTaskOffset = GetUtcOffsetForTaskUNLOCOOrFallback(utcNow);

				ZDateTime localUserTimeOfDay = localValue.Add(-localScheduleTaskOffset).Add(localUserOffset);
				localUserTimeOfDay = ZDateTime.MinSmallDateTimeValue.Date.Add(localUserTimeOfDay.TimeOfDay);

				return localUserTimeOfDay;
			}
			else
			{
				return localValue;
			}
		}

		#endregion

		#region S5_DailyEndTime

		[BusinessObjectTestExclude]
		public override ZDateTime S5_DailyEndTime
		{
			get
			{
				return base.S5_DailyEndTime;
			}
			set
			{
				if (value != base.S5_DailyEndTime)
				{
					base.S5_DailyEndTime = (value.IsValid) ? ZDateTime.MinSmallDateTimeValue.Date.Add(value.TimeOfDay) : value;
				}
			}
		}

		#region CalcDailyEndTimeUtc

		[BusinessObjectTestExclude]
		public ZDateTime CalcDailyEndTimeUtc
		{
			get
			{
				return GetTimeOnlyUtcFromTaskLocal(S5_DailyEndTime);
			}
			set
			{
				SetTimeOnlyTaskLocalFromUtcIfValid(S5_DailyEndTimeInfo, value);
			}
		}

		#endregion

		#region CalcDailyEndTimeLocal

		[BusinessObjectTestExclude]
		public ZDateTime CalcDailyEndTimeLocal
		{
			get
			{
				return GetTimeOnlyUserLocalFromTaskLocal(S5_DailyEndTime);
			}
			set
			{
				SetTimeOnlyTaskLocalFromUserLocalValueIfValid(S5_DailyEndTimeInfo, value);
				CalcDailyEndTimeLocalInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CalcDailyEndTimeLocalInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(CalcDailyEndTimeLocal)); }
		}

		#endregion

		#endregion

		#region S5_GB

		protected virtual bool S5_GB_ReadOnly => true;

		protected GlbBranch GetDefaultBranch()
		{
			var defaultCompanyQuery = new ZQuery(GlbCompanySchema.GC_IsActive, true);
			defaultCompanyQuery.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.DemoCompanyCode);

			var defaultCompany = Factory.LoadTop1<GlbCompany>(defaultCompanyQuery);

			GlbBranch defaultBranch = null;

			if (defaultCompany != null)
			{
				var query = new ZQuery() { OrderBy = GlbBranchSchema.GB_Code.Name };
				query.AddToFilter(GlbBranchSchema.GB_IsActive, ZBool.True);
				query.AddToFilter(GlbBranchSchema.GB_GC, defaultCompany.PK);

				return Factory.LoadTop1<GlbBranch>(query);
			}

			return defaultBranch;
		}

		#endregion

		#region DescriptionForLog

		public virtual ZString DescriptionForLog => S5_ScheduleDescription;

		#endregion

		#region PrintUserName

		[BusinessObjectTestExclude]
		[ResourceStringData("StmScheduleTask.PrintUserName", Caption = "Print User")]
		public ZString PrintUserName
		{
			get
			{
				var staff = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, S5_GS_NKPrintUser)).FirstOrDefault();
				if (staff != null)
				{
					return staff.GS_LoginName;
				}

				return string.Empty;
			}
		}

		public ZPropertyInfo PrintUserNameInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(PrintUserName)); }
		}

		#endregion

		#region Priority

		public int Priority => PriorityCore;

		protected virtual int PriorityCore => 0;

		#endregion

		#region UtcOffsetOverride

		[BusinessObjectTestExclude]
		public virtual TimeSpan? UtcOffsetOverride
		{
			get { return null; }
		}

		#endregion

		#region DbServerName

		public string DbServerName { get; set; } = Db.ServerName;

		#endregion

		#endregion

		#region Start / Next Date Calculation

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!S5_IsPrivate)
			{
				if ((!IsInDatabase && S5_NextScheduledPrintRunTimeUtc.IsEmpty) || RecurrenceRangeHasChanges)
				{
					S5_StartDate = Recurrence.CalculateNewStartDate();

					if (S5_IsActive)
					{
						S5_NextScheduledPrintRunTimeUtc = CalculateNextRunTime(S5_StartDate);
					}
				}
				else if (IsInDatabase)
				{
					if (S5_ScheduleActualRunCountInfo.HasChanges)
					{
						if (S5_ScheduleActualRunCount >= S5_EndAfterCount && S5_EndAfterCount != 0)
						{
							S5_IsActive = false;
						}

						UpdateNextScheduledDate();
					}
					else if (S5_IsActiveInfo.HasChanges && ShouldUpdateNextScheduleDate)
					{
						UpdateNextScheduledDate();
					}
				}

				if (S5_NextScheduledPrintRunTimeUtc > S5_EndDate && !S5_EndDate.IsEmpty)
				{
					S5_IsActive = false;
				}
			}
		}

		protected virtual bool ShouldUpdateNextScheduleDate => true;

		public void UpdateNextScheduledDate()
		{
			UpdateNextScheduledDateCore();
		}

		protected virtual void UpdateNextScheduledDateCore()
		{
			if (S5_IsActive)
			{
				if (S5_NextScheduledPrintRunTimeUtc.IsEmpty)
				{
					S5_NextScheduledPrintRunTimeUtc = CalculateNextRunTime(S5_StartDate);
				}
				else
				{
					ZDateTime updatedDateTime = Recurrence.CalculateNextScheduleDate();
					if (updatedDateTime == S5_NextScheduledPrintRunTimeUtc && S5_TaskPeriod == ScheduleRecurrenceType.AccountingPeriod)
					{
						S5_IsActive = false;
						NotifyPostMastersThatThisTaskIsSetInactive();
					}
					S5_NextScheduledPrintRunTimeUtc = CalculateNextRunTime(updatedDateTime);
				}
			}
		}

		void NotifyPostMastersThatThisTaskIsSetInactive()
		{
			EmailDef eMail = new EmailDef();

			eMail.AddRecipientForUserCommunication(new EmailGroupUtility().GetGroupEmailCollection(EnvProxy.Instance.Registry.PostMasterGroup, false));

			eMail.Subject = Res.GetString("bfbd229c-abd6-4fb1-9f98-e7a48a41db74", "A scheduled job has been disabled by {0}", Core.Constants.ProductName);
			eMail.Body =
				Res.GetString("9404169b-9dc8-4d4e-a7e1-ed10341990d8", @"Scheduled Job '{0}' has been set to Inactive by {1} running on database '{2}'. This job is setup to run per Accounting Period, but the current Accounting Period for your company ('{3}') is the last period in the Accounting Year and the next Accounting Year has not been setup yet.

To re-enable this Scheduled Job:
1)  Setup your Accounting Periods for the next Accounting Year. You can do this by going into Manage > General Ledger > Period Management module and press Setup next accounting year.
2)  You can then re-enable this Scheduled Job by going into Maintain -> System -> Scheduled Reports module, use Description filter with value {4} to find this Scheduled Job and tick its Active flag.", S5_ScheduleDescription, Core.Constants.ProductName, Db.Connection.CurrentDatabase, Env.CurrentCompany.Code, S5_ScheduleDescription);

			EnvProxy.Instance.OutgoingMailManager.CreateAndSave(eMail);
		}

		protected bool RecurrenceRangeHasChanges
		{
			get
			{
				return
					S5_StartDateInfo.HasChanges ||
					S5_TaskPeriodInfo.HasChanges ||
					S5_TaskPeriodCountInfo.HasChanges ||
					S5_WeekDayOccurrenceNumberInfo.HasChanges ||
					S5_WeekDaysOnlyInfo.HasChanges ||
					S5_MonthNumberInfo.HasChanges ||
					S5_DayNumberInfo.HasChanges ||
					S5_DayListInfo.HasChanges ||
					S5_DailyStartTimeInfo.HasChanges ||
					S5_DailyEndTimeInfo.HasChanges;
			}
		}

		#endregion

		#region Run

#if DEBUG
		public void Run()
		{
			Run(CancellationToken.None);
		}

		public void Run(INotifications notifications)
		{
			Run(notifications, CancellationToken.None);
		}
#endif

		public void Run(CancellationToken token)
		{
			Run(new NotificationBuffer(), token);
		}

		public void Run(INotifications notifications, CancellationToken token)
		{
			S5_CurrentPrintRunTime = ZDateTime.Now;
			Factory.Save();

			Recipients.Load();

			bool wasActive = S5_IsActive;
			var runNotifications = notifications is NotificationBuffer buffer ? buffer : new NotificationBuffer(notifications);
			ErrorNotificationSender = new ScheduleReportErrorNotificationSender(this, runNotifications);
			RunCore(runNotifications, token);
			if (wasActive && runNotifications.HasErrors)
			{
				SendErrorNotificationEmailToUser(runNotifications);
			}

			OnAfterRun();
		}

		protected void SendErrorNotificationEmailToUser(NotificationBuffer runNotifications, string customSubject = "")
		{
			try
			{
				if (!string.IsNullOrEmpty(businessObjectName) && businessObjectGuid != Guid.Empty && !string.IsNullOrEmpty(controllerID.ToString()))
				{
					runNotifications.Add(CargoWise.ComponentModel.NotificationType.Information, AddHyperLink());
				}
				var subject = string.IsNullOrEmpty(customSubject)
					? Res.GetString("e9cd569f-c322-498c-855e-fb40c6f6505c", "One or more errors were encountered for Scheduled Task - {0} run on {1}", S5_ScheduleDescription, ZDateTime.Now)
					: customSubject;
				var body = MakeBodyOfMail(runNotifications.AsString);

				ErrorNotificationSender ??= new ScheduleReportErrorNotificationSender(this, runNotifications);
				ErrorNotificationSender.SendErrorNotification(subject, body, () =>
				{
					var recipientsWithEmailAddress = GetRecipientsWithEmailAddress();
					Env.OutgoingMailManager.CreateAndSaveWithCopyToCompanyNotificationGroup(subject, body, recipientsWithEmailAddress, CopyToNotificationsGroupIfRecipientsNotEmpty, EmailContentTypes.HTML);
					return recipientsWithEmailAddress.Length > 0 ? recipientsWithEmailAddress : new EmailGroupUtility().GetCompanyNotificationGroupEmails().Cast<string>().ToArray();
				});
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				runNotifications.AddError(Res.GetString("6a5aabee-7d6b-40b1-b683-522221c2205a", "Error notification email could not be sent. Please make sure that the group specified in the Notification > Company Notification Group Registry setting has a member with an email address on their Staff profile, or the last editing user has an email address."));
			}
		}

		public ScheduleReportErrorNotificationSender ErrorNotificationSender { get; private set; }

		string AddHyperLink()
		{
			var hyperLinkText = Res.GetString("4A60D253-EE0A-45C8-AB3F-19BD98DFE274", "Click to open {0}", businessObjectName);
			var hyperLink = string.Format(Culture.Invariant, @"<a href=""{0}"">{1}</a>.<br>",
				ShowEditFormUrlCreator.Create(controllerID, businessObjectGuid),
				hyperLinkText);
#if DEBUG
			BusinessObjectHyperLink_Exposed = hyperLink;
#endif
			return hyperLink;
		}
		protected ControllerID controllerID;
		protected Guid businessObjectGuid;
		protected string businessObjectName;

		IShowEditFormUrlCreator ShowEditFormUrlCreator
		{
			get { return showEditFormUrlCreator ?? (showEditFormUrlCreator = ObjectFactory.Get<IShowEditFormUrlCreator>()); }
		}
		IShowEditFormUrlCreator showEditFormUrlCreator;

		protected virtual string[] GetRecipientsWithEmailAddress()
		{
			var result = GetEmailOfLastEditingUser();
			return string.IsNullOrEmpty(result) ? Array.Empty<string>() : new[] { result };
		}

		protected virtual string GetEmailOfLastEditingUser()
		{
			var emailAddress = string.Empty;
			if (!S5_SystemLastEditUser.IsEmpty)
			{
				emailAddress = GlbStaff.GetEmailAddressFromUserCode(Factory, S5_SystemLastEditUser);
			}

			if (string.IsNullOrEmpty(emailAddress) && !S5_SystemLastEditUser.Equals(S5_SystemCreateUser) && !S5_SystemCreateUser.IsEmpty)
			{
				emailAddress = GlbStaff.GetEmailAddressFromUserCode(Factory, S5_SystemCreateUser);
			}

			return emailAddress;
		}

		string GetEmailOfCreateUser()
		{
			var emailAddress = string.Empty;
			if (!S5_SystemCreateUser.IsEmpty)
			{
				emailAddress = GlbStaff.GetEmailAddressFromUserCode(Factory, S5_SystemCreateUser);
			}

			return emailAddress;
		}

		protected virtual bool CopyToNotificationsGroupIfRecipientsNotEmpty
		{
			get { return true; }
		}

		string MakeBodyOfMail(string runNotifications)
		{
			StringBuilder resultBuilder = new StringBuilder();
			resultBuilder.Append((NoResString)"<pre>" + runNotifications + (NoResString)"</pre>");
			resultBuilder.Append(": <br>");
			resultBuilder.Append(Res.GetString("3B9B9970-EBCF-46A1-A939-9DE0CAD2CAD3", "Machine Name : "));
			resultBuilder.Append(System.Environment.MachineName + "<br>");

			resultBuilder.Append(Res.GetString("C123E9D3-24FA-4816-AA1B-5914AA711DFE", @"Database Server \ Instance : "));
			resultBuilder.Append(Db.Connection.ServerName + "<br>");

			resultBuilder.Append(Res.GetString("B8095257-478E-4426-BB4D-48A9ED2DF943", "Database Name : "));
			resultBuilder.Append(Db.Connection.CurrentDatabase + "<br>");

			resultBuilder.Append(Env.CurrentCompany.GetSummaryAndRegistrationText().Replace(System.Environment.NewLine, "<br>"));
			return resultBuilder.ToString();
		}

		protected virtual void RunCore(INotifications notifications, CancellationToken token)
		{
		}

		internal void OnAfterRun()
		{
			if (S5_IsPrivate)
			{
				Delete();
			}
			else
			{
				Reload();
				S5_CurrentPrintRunTime = ZDateTime.Empty;
				S5_ScheduleActualRunCount = S5_ScheduleActualRunCount + 1;
				if (S5_DateScheduleFirstRun.IsEmpty)
				{
					S5_DateScheduleFirstRun = ZDateTime.Now;
				}
			}

			Factory.Save();
		}

#if DEBUG
		public void RunSafe(INotifications notifications)
		{
			RunSafe(notifications, CancellationToken.None);
		}

#endif

		protected IDisposable SetEnvironmentForTask(StmScheduleTask scheduleTask)
		{
			var branchPk = scheduleTask.BranchPKForRunSafe;
			return branchPk.IsValid ? new DisposableEnvironment(branchPk.ToGuid(), loginName: Env.CurrentUser.LoginName) : null;
		}

		public virtual void RunSafe(INotifications notifications, CancellationToken token)
		{
			if (HasChanges)
			{
				ErrorReporter.ReportOnce("{0E0B981E-7FDC-4b8d-8646-1621A1659624}", "Scheduled Tasks can only be run if they are saved.");
			}

			var completedSuccessfully = false;
			var numberOfAttempts = 1;

			while (!completedSuccessfully)
			{
				using (Db.DisposableActionForDbConnection())
				{
					var taskFactory = new BusinessObjectFactory();
					var scheduleTask = (StmScheduleTask)taskFactory.Load(this.GetType(), this.PK);
					if (scheduleTask == null)
					{
						notifications.AddError(Res.GetString("85bb3e74-0b19-48b2-b181-0bb4b2804dab", "Could not load scheduled task for {0}, PK = {1}", this.GetType(), this.PK));
						return;
					}

					using (SetEnvironmentForTask(scheduleTask))
					{
						var runNotifications = new NotificationBuffer(notifications);
						try
						{
							scheduleTask.StmReportRun = this.StmReportRun;
							scheduleTask.Run(runNotifications, token);
							if (scheduleTask.IsDeleted)
							{
								Delete();
								Factory.Save();
							}
							completedSuccessfully = true;
						}
						catch (Exception ex)
						{
							if (ex.IsCriticalException())
							{
								throw;
							}
							if (numberOfAttempts < 10 && scheduleTask.IsErrorThatShouldRetry(ex))
							{
								if (scheduleTask.RetryWaitPeriod > TimeSpan.Zero)
								{
									Thread.Sleep(scheduleTask.RetryWaitPeriod);
								}
								numberOfAttempts++;
							}
							else
							{
								try
								{
									if (IsInfrastructureDbError(ex))
									{
										var sqlException = ex as SqlException;
										var dbError = new DbErrorMatch(sqlException);
										var message = Res.GetString("c5eb1531-d2c1-46a7-a513-5d8cf58cd42c", "Scheduled Task '{0}' could not run due to an infrastructure DB error on the DB server '{1}', exception type: {2}, SQL error number: {3} (see next event for details). It will be run again later.", S5_ScheduleDescription, scheduleTask.DbServerName, dbError.ExceptionType, sqlException?.Number);
										notifications.AddWarning(message);
									}
									else
									{
										NotifyStaffThatTaskWasDeactivatedDueToException(ex, notifications);
										scheduleTask.S5_IsActive = false;
									}
									taskFactory.Save();
								}
								catch (Exception ex1) when (!ex1.IsCriticalException())
								{
									// if we can't set the task to inactive, we still want to report the original exception
								}

								throw;
							}
						}
					}
				}
			}
		}

		protected virtual void NotifyStaffThatTaskWasDeactivatedDueToException(Exception e, INotifications notifications)
		{
		}

		ZGuid BranchPKForRunSafe => S5_GB.IsValid ? S5_GB : GetDefaultBranch()?.PK ?? ZGuid.Empty;

		protected virtual bool IsErrorThatShouldRetry(Exception ex)
		{
			return false;
		}

		protected bool IsInfrastructureDbError(Exception ex)
			=> ex is SqlException sqlEx && ZExceptionExtensions.IsInfrastructureDbError(sqlEx);

		protected virtual TimeSpan RetryWaitPeriod
		{
			get { return TimeSpan.Zero; }
		}

		public void NotifyUserOfReportBeingCanceled()
		{
			if (S5_SystemCreateUser == User.WebUserCode)
			{
				return;
			}
			var emailOfCreateUser = GetEmailOfCreateUser();
			var emailOfLastEditUser = GetEmailOfLastEditingUser();
			var defaultRecipients = new List<string> { emailOfCreateUser, emailOfLastEditUser }.Where(r => !string.IsNullOrEmpty(r)).Distinct().ToArray();

			var constantRecipients = !string.IsNullOrEmpty(emailOfCreateUser)
				? new[] { emailOfCreateUser }
				: null;

			var reportName = S5_ScheduleDescription;

			var subject = Res.GetString("2ab5a4de-2aff-4ebd-8772-52f1eb723285",
					"Report has been canceled: {0}", reportName);
			var body = Res.GetString("937af9f7-917d-4f15-a974-f4789c728ce5",
			"Report {0} was canceled by {1} on {2}.",
				reportName, GlbStaff.CurrentUser.GS_Code, ZDateTimeOffset.UtcNow.ToString("dd-MMM-yy HH:mm zzz")) + "\r\n";

			var sender = new ScheduleReportErrorNotificationSender(this, null);
			sender.SendErrorNotification(subject, body, () =>
			{
				if (defaultRecipients.Length > 0)
				{
					var mail = new EmailDef();
					mail.Subject = subject;
					mail.Body = body;
					mail.AddRecipientForUserCommunication(defaultRecipients);
					EnvProxy.Instance.OutgoingMailManager.Create(Factory, mail);
					return defaultRecipients;
				}

				return null;
			}, false, ScheduledReportErrorNotificationType.Canceled, constantRecipients);
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			return Clone();
		}

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(GetNotCloneableFields());
			StmScheduleTask result = (StmScheduleTask)base.CloneInternal(args);

			foreach (StmScheduleTaskRecipient recipient in Recipients)
			{
				result.Recipients.Add(recipient.Clone());
			}

			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region GetNotCloneableFields

		string[] GetNotCloneableFields()
		{
			return new string[1]
				{
					StmScheduleTaskSchema.Constants.S5_ScheduleActualRunCount
				};
		}

		#endregion

		#region Test Data
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			S5_ParentTableCode = StmServiceHostSchema.Constants.Prefix;
			S5_ParentID = new ZGuid();

			HasChanges = false;
		}

		public string BusinessObjectHyperLink_Exposed
		{ get; set; }
#endif
		#endregion
	}
}
