using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public interface AccountingPeriodRangeSerialiser
	{
		ZInt PeriodFrom { get; set; }
		ZInt PeriodTo { get; set; }
	}

	public class AccountingPeriodsRangeField : FilterFieldWithUTSupport, AccountingPeriodRangeSerialiser, ISchedulableFilterField, IJsonSerializable
	{
		public static string PeriodRangeDifferentYearErrorMessage
		{
			get { return Res.GetString("471844b2-054c-417f-8270-a8183de94b52", "'Period From' and 'Period To' must be within a same financial year"); }
		}

		public static string PeriodFromLessToErrorMessage
		{
			get { return Res.GetString("2665765D-6576-4D79-8F41-50804677A98F", "The 'Period From' must be less than 'Period To'."); }
		}

		public static string PeriodToGreaterFromErrorMessage
		{
			get { return Res.GetString("26BB33C4-28DA-466B-92BF-3FF39F66F2F9", "The 'Period To' must be greater than 'Period From'."); }
		}

		public AccountingPeriodsRangeField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		internal AccountingPeriodsRangeField(AccountingPeriodsRangeFieldJsonData data)
			: base(data)
		{
			CreateParameters();

			RequireBothFromAndToPeriods = data.RequireBothFromAndToPeriods;

			SetPeriodFromValue(data.PeriodFrom);
			SetPeriodToValue(data.PeriodTo);
		}

		protected override void SetNecessaryPropertiesForDeserializingCore(FilterField origin, CollectionOfIFilter filterCollection)
		{
			base.SetNecessaryPropertiesForDeserializingCore(origin, filterCollection);

			if (origin is AccountingPeriodsRangeField originalAccountingPeriodsRangeField)
			{
				PeriodCalculator.Company = originalAccountingPeriodsRangeField.PeriodCalculator.Company;
			}
		}

		#endregion

		void CreateParameters()
		{
			fPeriodFrom = new SqlParameter(SqlParameterNameGenerator.Next(), 0);
			fPeriodTo = new SqlParameter(SqlParameterNameGenerator.Next(), 0);
		}

		#region PeriodFrom

		[ReadOnlyMember(nameof(Scheduled))]
		public ZInt PeriodFrom
		{
			get { return Scheduled ? SchedulePeriodFrom : RunPeriodFrom; }
			set
			{
				if (Scheduled)
				{
					SchedulePeriodFrom = value;
				}
				else
				{
					RunPeriodFrom = value;
				}
			}
		}

		ZInt runPeriodFrom;

		protected ZInt RunPeriodFrom
		{
			get { return runPeriodFrom; }
			set
			{
				runPeriodFrom = value;
				if (!IsValidationSuspended)
				{
					ValidatePeriodFrom(runPeriodFrom, runPeriodTo);
					ValidatePeriodTo(runPeriodFrom, runPeriodTo);
					ValidateRequireBothFromAndToPeriods(RunPeriodFrom, RunPeriodTo);
				}
				PeriodFromInfo.RefreshBinding();
			}
		}

		ZInt schedulePeriodFrom;

		protected ZInt SchedulePeriodFrom
		{
			get { return schedulePeriodFrom; }
			set
			{
				schedulePeriodFrom = value;
				if (!IsValidationSuspended)
				{
					ValidatePeriodFrom(schedulePeriodFrom, schedulePeriodTo);
					ValidatePeriodTo(schedulePeriodFrom, schedulePeriodTo);
					ValidateRequireBothFromAndToPeriods(SchedulePeriodFrom, SchedulePeriodTo);
					WarnNoSetupPeriodsInAccounts();
				}
				PeriodFromInfo.RefreshBinding();
			}
		}

		protected void ValidatePeriodFrom(ZInt periodFrom, ZInt periodTo)
		{
			PeriodFromInfo.ClearAllNotifications();

			if (!IsValid)
			{
				PeriodFromInfo.AddError(ValidationError);
			}
			else if (!periodFrom.IsEmpty && !periodTo.IsEmpty)
			{
				if (!PeriodCalculator.IsPeriodValid(periodFrom))
				{
					PeriodFromInfo.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(periodFrom, PeriodCalculator.Company));
				}
				else if (periodTo.ToString().Length >= 6 && periodFrom.ToString().Substring(0, 4) != periodTo.ToString().Substring(0, 4))
				{
					PeriodFromInfo.AddError(PeriodRangeDifferentYearErrorMessage);
				}
				else if (periodFrom > periodTo)
				{
					PeriodFromInfo.AddError(PeriodFromLessToErrorMessage);
				}
			}
		}

		protected void WarnNoSetupPeriodsInAccounts()
		{
			if (Scheduled && !LowSchedule.PeriodScope.IsEmpty && SchedulePeriodFrom.IsEmpty)
			{
				PeriodFromInfo.AddWarning(Res.GetString("a4f2676e-01df-4359-b97b-9a93d4d09289", "Setup periods in Accounts -> Period Management."));
			}
			if (Scheduled && !HighSchedule.PeriodScope.IsEmpty && SchedulePeriodTo.IsEmpty)
			{
				PeriodToInfo.AddWarning(Res.GetString("a4f2676e-01df-4359-b97b-9a93d4d09289", "Setup periods in Accounts -> Period Management."));
			}
		}

		public ZPropertyInfo PeriodFromInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(PeriodFrom));
				result.HumanReadableName = DisplayName;
				return result;
			}
		}

		#endregion

		#region PeriodTo

		[ReadOnlyMember(nameof(Scheduled))]
		public ZInt PeriodTo
		{
			get { return Scheduled ? SchedulePeriodTo : RunPeriodTo; }
			set
			{
				if (Scheduled)
				{
					SchedulePeriodTo = value;
				}
				else
				{
					RunPeriodTo = value;
				}
			}
		}

		ZInt runPeriodTo;

		protected ZInt RunPeriodTo
		{
			get { return runPeriodTo; }
			set
			{
				runPeriodTo = value;
				if (!IsValidationSuspended)
				{
					ValidatePeriodTo(runPeriodFrom, runPeriodTo);
					ValidatePeriodFrom(runPeriodFrom, runPeriodTo);
					ValidateRequireBothFromAndToPeriods(RunPeriodFrom, RunPeriodTo);
				}
				PeriodToInfo.RefreshBinding();
			}
		}

		ZInt schedulePeriodTo;

		protected ZInt SchedulePeriodTo
		{
			get { return schedulePeriodTo; }
			set
			{
				schedulePeriodTo = value;
				if (!IsValidationSuspended)
				{
					ValidatePeriodTo(schedulePeriodFrom, schedulePeriodTo);
					ValidatePeriodFrom(schedulePeriodFrom, schedulePeriodTo);
					ValidateRequireBothFromAndToPeriods(SchedulePeriodFrom, SchedulePeriodTo);
					WarnNoSetupPeriodsInAccounts();
				}
				PeriodToInfo.RefreshBinding();
			}
		}

		protected void ValidatePeriodTo(ZInt periodFrom, ZInt periodTo)
		{
			PeriodToInfo.ClearAllNotifications();

			if (!IsValid)
			{
				PeriodToInfo.AddError(ValidationError);
			}
			else if (!periodFrom.IsEmpty && !periodTo.IsEmpty)
			{
				if (!PeriodCalculator.IsPeriodValid(periodTo))
				{
					PeriodToInfo.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(periodTo, PeriodCalculator.Company));
				}
				else if (periodFrom.ToString().Length >= 6 && periodFrom.ToString().Substring(0, 4) != periodTo.ToString().Substring(0, 4))
				{
					PeriodToInfo.AddError(PeriodRangeDifferentYearErrorMessage);
				}
				else if (periodFrom > periodTo)
				{
					PeriodToInfo.AddError(PeriodToGreaterFromErrorMessage);
				}
			}
		}

		public ZPropertyInfo PeriodToInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(PeriodTo));
				result.HumanReadableName = DisplayName;
				return result;
			}
		}

		#endregion

		#region Schedule

		public AccPeriodSchedule LowSchedule
		{
			get
			{
				if (Scheduled)
				{
					if (lowSchedule == null)
					{
						lowSchedule = new AccPeriodSchedule();
						lowSchedule.ScheduleTask = scheduleTask;
						if (scheduleTask.IsInDatabase)
						{
							lowSchedule.PeriodScope = ZString.Empty;
						}
						lowSchedule.ValueChanged += delegate { UpdatePeriodFromFromSchedule(); };
						UpdatePeriodFromFromSchedule();
					}

					return lowSchedule;
				}

				return null;
			}
		}

		public AccPeriodSchedule HighSchedule
		{
			get
			{
				if (Scheduled)
				{
					if (highSchedule == null)
					{
						highSchedule = new AccPeriodSchedule();
						highSchedule.ScheduleTask = scheduleTask;
						if (scheduleTask.IsInDatabase)
						{
							highSchedule.PeriodScope = ZString.Empty;
						}
						highSchedule.ValueChanged += delegate { UpdatePeriodToFromSchedule(); };
						UpdatePeriodToFromSchedule();
					}

					return highSchedule;
				}

				return null;
			}
		}

		bool Scheduled
		{
			get { return (scheduleTask != null); }
		}

		void UpdatePeriodFromFromSchedule()
		{
			SchedulePeriodFrom = lowSchedule.GetSchedulePeriod();
		}

		void UpdatePeriodToFromSchedule()
		{
			SchedulePeriodTo = highSchedule.GetSchedulePeriod();
		}

		AccPeriodSchedule lowSchedule;
		AccPeriodSchedule highSchedule;
		ReportScheduleTask scheduleTask;

		public bool RequireBothFromAndToPeriods { get; set; }

		#endregion

		#region Overrides

		protected override void AddSpecialisedValueProviders()
		{
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".PeriodFrom", new ValueReplacers.ReplacementProviderMethod(GetFromPeriodReplacement)));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".PeriodTo", new ValueReplacers.ReplacementProviderMethod(GetToPeriodReplacement)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.PeriodFrom>", ResString.GetMultilingualString("782390c6-7907-4f15-b217-d9115a811ea2", "Returns the value of the From period.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.PeriodTo>", ResString.GetMultilingualString("5275451e-e411-4971-845b-34d7cdd13ae1", "Returns the value of the To period.")));
		}

		protected object GetFromPeriodReplacement(string macro, Report report)
		{
			return PeriodFrom;
		}

		protected object GetToPeriodReplacement(string macro, Report report)
		{
			return PeriodTo;
		}

		public override bool IsEmpty
		{
			get
			{
				bool result;
				if (Scheduled)
				{
					result = !LowSchedule.IsValid || !HighSchedule.IsValid;
				}
				else
				{
					result = runPeriodFrom.IsEmpty || runPeriodTo.IsEmpty;
				}
				return result;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			if (Scheduled)
			{
				ValidatePeriodFrom(schedulePeriodFrom, schedulePeriodTo);
				ValidatePeriodTo(schedulePeriodFrom, schedulePeriodTo);
				ValidateRequireBothFromAndToPeriods(schedulePeriodFrom, schedulePeriodTo);
			}
			else
			{
				ValidatePeriodFrom(runPeriodFrom, runPeriodTo);
				ValidatePeriodTo(runPeriodFrom, runPeriodTo);
				ValidateRequireBothFromAndToPeriods(runPeriodFrom, runPeriodTo);
			}
		}

		void ValidateRequireBothFromAndToPeriods(ZInt periodFrom, ZInt periodTo)
		{
			if (RequireBothFromAndToPeriods)
			{
				if (periodFrom.IsEmpty)
				{
					PeriodFromInfo.AddError(Res.GetString("bf210ea0-03e6-414c-9141-9fd4ab47bc32", "period From must be entered."));
				}
				if (periodTo.IsEmpty)
				{
					PeriodToInfo.AddError(Res.GetString("58e69062-2bf1-47ff-adae-c24110fd1e95", "period To must be entered."));
				}
			}
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.AccountingPeriodsRangeUserControl; }
		}

		public override object ValueAsObject
		{
			get
			{
				return Res.GetString("ca44afdb-2576-4ef4-8905-6d46d0574ea0", "From: {0} To: {1}", PeriodFrom.ToString(), PeriodTo.ToString());
			}
		}

		protected override void SetDependencyValue(string value)
		{
			base.SetDependencyValue(value);
			if (value != null)
			{
				PeriodCalculator.Company = GlbCompany.CurrentCompany;
				var reportingBook = Factory.Load<AccReportingBook>(ZGuid.ParseSafe(value));
				if (reportingBook != null && reportingBook.ARB_GC_CompanyOfPeriod != ZGuid.Empty)
				{
					PeriodCalculator.Company = Factory.Load<GlbCompany>(reportingBook.ARB_GC_CompanyOfPeriod);
				}
			}
		}

		#region Debug Only
#if DEBUG
		public override void ClearValueForUnitTest()
		{
			PeriodFrom = 0;
			PeriodTo = 0;
		}
#endif
		#endregion

		#endregion

		#region Implementation

		SqlParameter fPeriodFrom;
		SqlParameter fPeriodTo;

		AccountingPeriodCalculator fPeriodCalculator;

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			string lowerConstraint = PeriodFrom.IsEmpty ? "" : FieldName + " >= " + fPeriodFrom;
			string upperConstraint = PeriodTo.IsEmpty ? "" : FieldName + " <= " + fPeriodTo;
			return lowerConstraint + ((!string.IsNullOrEmpty(lowerConstraint) && !string.IsNullOrEmpty(upperConstraint)) ? " AND " : "") + upperConstraint;
		}

		protected override SqlParameterList GetSqlParametersCore()
		{
			var result = new SqlParameterList();
			if (IsEmpty)
			{
				return result;
			}

			SetFromSQLParameterValue();
			SetToSQLParameterValue();

			result.Add(fPeriodFrom);
			result.Add(fPeriodTo);
			return result;
		}

		protected void SetFromSQLParameterValue()
		{
			if (Scheduled)
			{
				if (schedulePeriodFrom.IsEmpty || !schedulePeriodFrom.IsValid)
				{
					fPeriodFrom.Value = DBNull.Value;
				}
				else
				{
					fPeriodFrom.Value = (int)schedulePeriodFrom;
				}
			}
			else
			{
				if (runPeriodFrom.IsEmpty || !runPeriodFrom.IsValid)
				{
					fPeriodFrom.Value = DBNull.Value;
				}
				else
				{
					fPeriodFrom.Value = (int)runPeriodFrom;
				}
			}
		}

		protected void SetToSQLParameterValue()
		{
			if (Scheduled)
			{
				if (schedulePeriodTo.IsEmpty || !schedulePeriodTo.IsValid)
				{
					fPeriodTo.Value = DBNull.Value;
				}
				else
				{
					fPeriodTo.Value = (int)schedulePeriodTo;
				}
			}
			else
			{
				if (runPeriodTo.IsEmpty || !runPeriodTo.IsValid)
				{
					fPeriodTo.Value = DBNull.Value;
				}
				else
				{
					fPeriodTo.Value = (int)runPeriodTo;
				}
			}
		}

		protected AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return fPeriodCalculator;
			}
		}

		#endregion

		#region ISchedulableFilterField Members

		public void SetScheduleTask(ReportScheduleTask value)
		{
			scheduleTask = value;
			if (lowSchedule != null)
			{
				lowSchedule.ScheduleTask = scheduleTask;
				UpdatePeriodFromFromSchedule();
			}
			if (highSchedule != null)
			{
				highSchedule.ScheduleTask = scheduleTask;
				UpdatePeriodToFromSchedule();
			}
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is AccountingPeriodsRangeField accountingPeriodsRangeField)
			{
				if (accountingPeriodsRangeField.lowSchedule != null)
				{
					if (lowSchedule == null)
					{
						lowSchedule = new AccPeriodSchedule();
						lowSchedule.ValueChanged += delegate { UpdatePeriodFromFromSchedule(); };
					}

					lowSchedule.CopyChangesFrom(accountingPeriodsRangeField.lowSchedule);
					UpdatePeriodFromFromSchedule();
				}
				else
				{
					lowSchedule?.Clear();
				}

				if (accountingPeriodsRangeField.highSchedule != null)
				{
					if (highSchedule == null)
					{
						highSchedule = new AccPeriodSchedule();
						highSchedule.ValueChanged += delegate { UpdatePeriodToFromSchedule(); };
					}

					highSchedule.CopyChangesFrom(accountingPeriodsRangeField.highSchedule);
					UpdatePeriodToFromSchedule();
				}
				else
				{
					highSchedule?.Clear();
				}
				this.RunPeriodFrom = accountingPeriodsRangeField.RunPeriodFrom;
				this.RunPeriodTo = accountingPeriodsRangeField.RunPeriodTo;
			}
		}

		public override void ClearValues()
		{
			this.PeriodFrom = ZInt.Zero;
			this.PeriodTo = ZInt.Zero;
			lowSchedule?.Clear();
			highSchedule?.Clear();
		}

		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new AccountingPeriodsRangeFilter();
			SetBaseFilterData(filterData);

			filterData.PeriodFrom = PeriodFrom;
			filterData.PeriodTo = PeriodTo;
			if (Scheduled)
			{
				if (HighSchedule.IsValid)
				{
					filterData.ScheduleStorageTo = HighSchedule.ToStorageValue();
				}
				if (LowSchedule.IsValid)
				{
					filterData.ScheduleStorageFrom = LowSchedule.ToStorageValue();
				}
			}

			reportFilterData.AccountingPeriodsRangeFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.AccountingPeriodsRangeFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				if (Scheduled)
				{
					if (selectedValue.ScheduleStorageTo.HasValue && AccPeriodSchedule.TryParse(selectedValue.ScheduleStorageTo.Value, out highSchedule))
					{
						highSchedule.ValueChanged += delegate { UpdatePeriodToFromSchedule(); };
						highSchedule.ScheduleTask = scheduleTask;
						UpdatePeriodToFromSchedule();
					}
					else
					{
						HighSchedule.Clear();
					}

					if (selectedValue.ScheduleStorageFrom.HasValue && AccPeriodSchedule.TryParse(selectedValue.ScheduleStorageFrom.Value, out lowSchedule))
					{
						lowSchedule.ValueChanged += delegate { UpdatePeriodFromFromSchedule(); };
						lowSchedule.ScheduleTask = scheduleTask;
						UpdatePeriodFromFromSchedule();
					}
					else
					{
						LowSchedule.Clear();
					}
				}
				else
				{
					PeriodFrom = selectedValue.PeriodFrom;
					PeriodTo = selectedValue.PeriodTo;
				}
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new AccountingPeriodsRangeFieldJsonData()
			{
				RequireBothFromAndToPeriods = RequireBothFromAndToPeriods,
				PeriodFrom = GetSchedulableStore(RunPeriodFrom, lowSchedule),
				PeriodTo = GetSchedulableStore(RunPeriodTo, highSchedule)
			};
			SetJsonData(result);
			return result;
		}

		SchedulableStore<int> GetSchedulableStore(ZInt period, AccPeriodSchedule schedule)
		{
			var store = new SchedulableStore<int>();

			if (period.IsValid && !period.IsEmpty)
			{
				store.RunValue = period;
			}

			if (schedule != null && schedule.IsValid)
			{
				store.Schedule = schedule.ToStorageValue();
			}

			return store;
		}

		void SetPeriodFromValue(SchedulableStore<int> store)
		{
			if (store != null)
			{
				RunPeriodFrom = store.RunValue ?? 0;
				if (store.Schedule != null)
				{
					if (AccPeriodSchedule.TryParse(store.Schedule.Value, out lowSchedule))
					{
						lowSchedule.ValueChanged += delegate { UpdatePeriodFromFromSchedule(); };
					}
				}
			}
		}

		void SetPeriodToValue(SchedulableStore<int> store)
		{
			if (store != null)
			{
				RunPeriodTo = store.RunValue ?? 0;
				if (store.Schedule != null)
				{
					if (AccPeriodSchedule.TryParse(store.Schedule.Value, out highSchedule))
					{
						highSchedule.ValueChanged += delegate { UpdatePeriodToFromSchedule(); };
					}
				}
			}
		}

		#endregion
	}
}
