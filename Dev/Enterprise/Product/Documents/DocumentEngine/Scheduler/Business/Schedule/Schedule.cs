using System;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public abstract partial class Schedule : NonPersistentBusinessObject
	{
		protected Schedule()
		{
		}

		public ReportScheduleTask ScheduleTask
		{
			get { return scheduleTask; }
			set { scheduleTask = value; }
		}

		public bool IsThisPeriodScope
		{
			get { return (PeriodScope == PeriodScopeList.Codes.This); }
		}

		protected virtual ZDateTime BaseDateTime
		{
			get
			{
				ZDateTime result;
				if (shouldClearDate)
				{
					result = ZDateTime.Empty;
					shouldClearDate = false;
				}
				else if (ScheduleTask != null)
				{
					result = ScheduleTask.S5_NextScheduledPrintRunTimeUtc.IsValid ? ScheduleTask.CalcNextRunTimeLocal : ScheduleTask.CalcStartDateLocal;
				}
				else
				{
					result = ZDateTime.Empty;
				}
				return result;
			}
		}

		protected int PeriodsToAdd
		{
			get
			{
				var result = 0;

				if (!IsThisPeriodScope)
				{
					result = (PeriodScope == PeriodScopeList.Codes.Previous) ? (PeriodCount * -1) : (int)PeriodCount;
				}

				return result;
			}
		}

		protected void OnValueChanged()
		{
			if (!IsSettingHasChangesSuspended && !IsCopying)
			{
				DescriptionInfo.RefreshBinding();
				if (ValueChanged != null)
				{
					ValueChanged(this, EventArgs.Empty);
				}
			}
		}

		protected ScheduleCalculator Calculator
		{
			get
			{
				if (calculator == null)
				{
					BusinessObjectFactory scheduleFactory = Factory;
					GlbCompany scheduleCompany = null;
					if (ScheduleTask != null)
					{
						scheduleFactory = scheduleFactory ?? new BusinessObjectFactory();
						GlbBranch scheduleBranch = scheduleFactory.Load<GlbBranch>(ScheduleTask.S5_GB);
						if (scheduleBranch != null)
						{
							scheduleCompany = scheduleBranch.Company;
						}
					}
					calculator = new ScheduleCalculator(scheduleFactory, scheduleCompany);
				}
				return calculator;
			}
		}

		public const int MaxPeriodCount = 9999;
		public event EventHandler ValueChanged;
		ReportScheduleTask scheduleTask;
		ScheduleCalculator calculator;

		#region Clear

		public void Clear()
		{
			ClearCore();
		}

		protected virtual void ClearCore()
		{
			PeriodScope = "";
			PeriodCount = 0;
		}

		protected bool shouldClearDate;

		public virtual void ClearDate()
		{
			shouldClearDate = true;
			OnValueChanged();
		}

		#endregion

		#region Is Valid

		public bool IsValid
		{
			get
			{
				ValidationClone.CopyValuesFrom(this);
				ValidationClone.Validation.ValidateAll();
				return !ValidationClone.HasErrors;
			}
		}

		Schedule ValidationClone
		{
			get
			{
				if (validationClone == null)
				{
					validationClone = (Schedule)Clone();
				}
				return validationClone;
			}
		}

		Schedule validationClone;

		#endregion

		#region Storage Value

		public DateTime ToStorageValue()
		{
			if (!IsValid)
			{
				StmMenuItem menuItem = null;
				if (ScheduleTask != null)
				{
					menuItem = new BusinessObjectFactory { NameForDebugging = "MenuItemFactory" }.Load<StmMenuItem>(ScheduleTask.S5_ParentID);
				}
				ErrorReporter.ReportOnce("[WI00040360] Schedule Report ? Date Range Validation Issue",
					string.Format("ToStorageValue for report details [{0}] cannot be called when IsValid is false.The PeriodScope is:{1} , and The PeriodCount is:{2}",
					(menuItem != null) ? menuItem.SU_BusinessContext + ":" + menuItem.SU_MenuName + ":" + menuItem.SU_MenuPath + ":" + menuItem.SU_ContactType : (NoResString)"The menuItem is null",
					ValidationClone.PeriodScope, validationClone.PeriodCount));
				throw new InvalidOperationException("Some date filters are invalid. Please check their value!");
			}
			return ToStorageValueCore(BaseStorageDate.AddMonths(PeriodsToAdd));
		}

		protected static bool ValidRange(DateTime storageValue)
		{
			DateTime dateMask = GetDateMask(storageValue);
			return (storageValue.Date <= dateMask.AddMonths(MaxPeriodCount)) && (storageValue.Date >= dateMask.AddMonths(-1 * MaxPeriodCount));
		}

		protected void PopulatePeriodScopeAndNumber(DateTime storageValue)
		{
			DateTime dateMask = GetDateMask(storageValue);

			if (dateMask.Date == storageValue.Date)
			{
				PeriodScope = PeriodScopeList.Codes.This;
			}
			else
			{
				DateTime lowerDate;
				DateTime upperDate;
				bool subtractPeriods;

				if (storageValue < dateMask)
				{
					lowerDate = storageValue;
					upperDate = dateMask;
					subtractPeriods = true;
				}
				else
				{
					lowerDate = dateMask;
					upperDate = storageValue;
					subtractPeriods = false;
				}

				ZInt periodsToAdd = ZInt.Zero;
				while (lowerDate.Date < upperDate.Date)
				{
					++periodsToAdd;
					lowerDate = lowerDate.AddMonths(1);
				}

				PeriodScope = subtractPeriods ? PeriodScopeList.Codes.Previous : PeriodScopeList.Codes.Next;
				PeriodCount = periodsToAdd;
			}
		}

		static DateTime GetDateMask(DateTime storageValue)
		{
			return new DateTime(BaseStorageDate.Year, BaseStorageDate.Month, storageValue.Day);
		}

		protected static readonly DateTime BaseStorageDate = new DateTime(1910, 1, 1);
		protected abstract DateTime ToStorageValueCore(DateTime periodAdjustedValue);

		#endregion

		#region Cloning

		public void CopyChangesFrom(Schedule clone)
		{
			CopyValuesFrom(clone);
			OnValueChanged();
		}

		protected sealed override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			Schedule result = (Schedule)Activator.CreateInstance(GetType());
			using (result.SuspendSettingHasChanges())
			using (result.GetValidationSuspender())
			{
				result.CopyValuesFrom(this);
				result.ScheduleTask = ScheduleTask;
			}

			return result;
		}

		protected sealed override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Bound Properties

		#region Description

		public ZString Description
		{
			get { return GetDescriptionCore(); }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		protected virtual ZString GetDescriptionCore()
		{
			return IsValid ? GenerateDescription() : "";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		string GenerateDescription()
		{
			StringBuilder builder = new StringBuilder();
			AppendDescriptionStart(builder);
			if (PeriodDescription != "today")
			{
				if (PeriodDescription != "hour and minute")
				{
					if (builder.Length > 0)
					{
						builder.Append(" ");
					}
					if (IsThisPeriodScope || PeriodCount == 1)
					{
						builder.Append(builder.Length > 0 ? Res.GetString("fd40f7e9-39e2-456e-a386-947b38f50af1", "the") + " " : Res.GetString("e9cf98ff-018c-476d-82e3-d0f8d0e80228", "The") + " ");
						builder.Append(PeriodDescription);
					}
					else
					{
						builder.Append(PeriodCount.ToString());
						builder.Append(" ");
						builder.Append(PeriodDescription);
						builder.Append('s');
					}
				}
				if (IsThisPeriodScope)
				{
					builder.Append(" " + Res.GetString("7520d497-4d34-491b-a34d-b2c70bebf3b6", "of"));
				}
				else if (PeriodScope == PeriodScopeList.Codes.Previous)
				{
					builder.Append(" " + Res.GetString("20ebc7bf-f53b-490d-ba03-9ef1f1bb2c74", "prior to"));
				}
				else if (PeriodScope == PeriodScopeList.Codes.Next)
				{
					builder.Append(" " + Res.GetString("73e27007-a99e-47af-9d88-b846db03217c", "after"));
				}
			}
			AppendDescriptionMiddle(builder);
			AppendDescriptionEnd(builder);

			return builder.ToString();
		}

		protected virtual void AppendDescriptionStart(StringBuilder builder)
		{
		}

		protected virtual void AppendDescriptionEnd(StringBuilder builder)
		{
		}

		protected virtual void AppendDescriptionMiddle(StringBuilder builder)
		{
			builder.Append(" " + Res.GetString("354714be-55b1-45a0-ae7b-0590cd2ea748", "when the report is run."));
		}

		internal abstract string PeriodDescription { get; }

		#endregion

		#region Period Count

		[ReadOnlyMember(nameof(IsThisPeriodScope))]
		public ZInt PeriodCount
		{
			get { return periodCount; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(PeriodCountInfo, ref periodCount, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePeriodCount();
				}

				PeriodCountInfo.RefreshBinding();
				OnValueChanged();
			}
		}

		public ZPropertyInfo PeriodCountInfo
		{
			get { return GetZPropertyInfo(nameof(PeriodCount)); }
		}

		ZInt periodCount;

		#endregion

		#region Period Scope

		[MaxLength(8)]
		public ZString PeriodScope
		{
			get { return periodScope; }
			set
			{
				CheckMaximumLength(PeriodScopeInfo, value);
				SetNonPersistentPropertyValue<ZString>(PeriodScopeInfo, ref periodScope, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidatePeriodScope();
					Validation.ValidatePeriodCount();
				}

				if (IsThisPeriodScope)
				{
					PeriodCount = ZInt.Zero;
				}
				else
				{
					if (PeriodCount == ZInt.Zero)
					{
						PeriodCount = 1;
					}
					else
					{
						OnValueChanged();
					}
				}
			}
		}

		public ZPropertyInfo PeriodScopeInfo
		{
			get { return GetZPropertyInfo(nameof(PeriodScope)); }
		}

		ZString periodScope;

		#endregion

		#endregion

		#region Lookups

		public ScheduleLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}

		protected virtual ScheduleLookups GetNewLookups()
		{
			return new ScheduleLookups(this);
		}

		ScheduleLookups lookups;

		#endregion

		#region Validation

		public ScheduleValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual ScheduleValidation GetNewValidation()
		{
			return new ScheduleValidation(this);
		}

		protected sealed override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#endregion
	}

	#region Test Hooks
#if DEBUG
	public abstract partial class Schedule
	{
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			PeriodScope = PeriodScopeList.Codes.This;
		}

		internal ZDateTime BaseDateTimeForTesting
		{
			get { return this.BaseDateTime; }
		}

		internal ScheduleCalculator CalculatorForTesting
		{
			get { return this.Calculator; }
		}

		internal int PeriodsToAddForTesting
		{
			get { return PeriodsToAdd; }
		}

		internal static bool ValidRangeForTesting(DateTime storageValue)
		{
			return ValidRange(storageValue);
		}

		internal void PopulatePeriodScopeAndNumberForTesting(DateTime storageValue)
		{
			PopulatePeriodScopeAndNumber(storageValue);
		}
	}

#endif
	#endregion
}
