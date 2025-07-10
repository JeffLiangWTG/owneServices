using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class AccountingPeriodField : FilterFieldWithUTSupport, ISchedulableFilterField, IJsonSerializable
	{
		public AccountingPeriodField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
			SetDefaults();
		}

		#region Constructor For IJsonSerializable

		internal AccountingPeriodField(AccountingPeriodFieldJsonData data)
			: base(data)
		{
			CreateParameters();

			SetSinglePeriodValue(data.SinglePeriod);
			SetFromPeriodValue(data.FromPeriod);
			SetToPeriodValue(data.ToPeriod);
			SetYearToPeriodValue(data.YearToPeriod);

			UseAllPeriods = data.UseAllPeriods;
			UsePeriodRange = data.UsePeriodRange;
			UseSinglePeriod = data.UseSinglePeriod;
			UseYearToPeriod = data.UseYearToPeriod;
		}

		#endregion

		void CreateParameters()
		{
			fSingleParamList = new SqlParameterList();
			fSinglePeriod = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.Int);
			fSingleParamList.Add(fSinglePeriod);

			fRangeParamList = new SqlParameterList();
			fFromPeriod = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.Int);
			fToPeriod = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.Int);
			fRangeParamList.Add(fFromPeriod);
			fRangeParamList.Add(fToPeriod);

			fYearToPeriodParamList = new SqlParameterList();
			fYearStartPeriod = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.Int);
			fYearToPeriod = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.Int);
			fYearToPeriodParamList.Add(fYearStartPeriod);
			fYearToPeriodParamList.Add(fYearToPeriod);
		}

		protected void SetDefaults()
		{
			using (SuspendSettingHasChanges())
			{
				UseAllPeriods = true;
				UsePeriodRange = false;
				UseSinglePeriod = false;
				UseYearToPeriod = false;
			}
		}

		protected override SqlParameterList GetSqlParametersCore()
		{
			SqlParameterList result;

			if (UseSinglePeriod)
			{
				SetSingleSQLParameterValue();
				result = fSingleParamList;
			}
			else if (UsePeriodRange)
			{
				SetFromSQLParameterValue();
				SetToSQLParameterValue();
				result = fRangeParamList;
			}
			else if (UseYearToPeriod)
			{
				SetYearToSQLParameterValues();
				result = fYearToPeriodParamList;
			}
			else
			{
				result = new SqlParameterList();
			}

			return result;
		}

		protected void SetSingleSQLParameterValue()
		{
			if (Scheduled)
			{
				if (scheduleSinglePeriod.IsEmpty || !scheduleSinglePeriod.IsValid)
				{
					fSinglePeriod.Value = DBNull.Value;
				}
				else
				{
					fSinglePeriod.Value = (int)scheduleSinglePeriod;
				}
			}
			else
			{
				if (runSinglePeriod.IsEmpty || !runSinglePeriod.IsValid)
				{
					fSinglePeriod.Value = DBNull.Value;
				}
				else
				{
					fSinglePeriod.Value = (int)runSinglePeriod;
				}
			}
		}

		protected void SetFromSQLParameterValue()
		{
			if (Scheduled)
			{
				if (scheduleFromPeriod.IsEmpty || !scheduleFromPeriod.IsValid)
				{
					fFromPeriod.Value = DBNull.Value;
				}
				else
				{
					fFromPeriod.Value = (int)scheduleFromPeriod;
				}
			}
			else
			{
				if (runFromPeriod.IsEmpty || !runFromPeriod.IsValid)
				{
					fFromPeriod.Value = DBNull.Value;
				}
				else
				{
					fFromPeriod.Value = (int)runFromPeriod;
				}
			}
		}

		protected void SetToSQLParameterValue()
		{
			if (Scheduled)
			{
				if (scheduleToPeriod.IsEmpty || !scheduleToPeriod.IsValid)
				{
					fToPeriod.Value = DBNull.Value;
				}
				else
				{
					fToPeriod.Value = (int)scheduleToPeriod;
				}
			}
			else
			{
				if (runToPeriod.IsEmpty || !runToPeriod.IsValid)
				{
					fToPeriod.Value = DBNull.Value;
				}
				else
				{
					fToPeriod.Value = (int)runToPeriod;
				}
			}
		}

		protected void SetYearToSQLParameterValues()
		{
			if (Scheduled)
			{
				if (scheduleYearToPeriod.IsEmpty || !scheduleYearToPeriod.IsValid)
				{
					fYearToPeriod.Value = DBNull.Value;
				}
				else
				{
					fYearToPeriod.Value = (int)scheduleYearToPeriod;
				}
			}
			else
			{
				if (runYearToPeriod.IsEmpty || !runYearToPeriod.IsValid)
				{
					fYearToPeriod.Value = DBNull.Value;
				}
				else
				{
					fYearToPeriod.Value = (int)runYearToPeriod;
				}
			}

			if (Scheduled)
			{
				if (scheduleYearStartPeriod.IsEmpty || !scheduleYearStartPeriod.IsValid)
				{
					fYearStartPeriod.Value = DBNull.Value;
				}
				else
				{
					fYearStartPeriod.Value = (int)scheduleYearStartPeriod;
				}
			}
			else
			{
				if (runYearToPeriod.IsEmpty || !runYearToPeriod.IsValid)
				{
					fYearStartPeriod.Value = DBNull.Value;
				}
				else
				{
					fYearStartPeriod.Value = (int)runYearStartPeriod;
				}
			}
		}

		public override bool IsEmpty
		{
			get
			{
				bool result = false;
				if (UseSinglePeriod)
				{
					if (Scheduled)
					{
						result = !SinglePeriodSchedule.IsValid;
					}
					else
					{
						result = runSinglePeriod.IsEmpty;
					}
				}
				else if (UsePeriodRange)
				{
					if (Scheduled)
					{
						result = !FromPeriodSchedule.IsValid && !ToPeriodSchedule.IsValid;
					}
					else
					{
						result = runFromPeriod.IsEmpty && runToPeriod.IsEmpty;
					}
				}
				else if (UseYearToPeriod)
				{
					if (Scheduled)
					{
						result = !YearToPeriodSchedule.IsValid;
					}
					else
					{
						result = runYearToPeriod.IsEmpty;
					}
				}
				return result;
			}
		}

		public override object ValueAsObject
		{
			get
			{
				object result;

				if (UseSinglePeriod)
				{
					result = SinglePeriod.ToString();
				}
				else if (UsePeriodRange)
				{
					result = Res.GetString("53794711-a4c4-4620-b6c4-be4965c7a177", "{0} to {1}", FromPeriod, ToPeriod);
				}
				else if (UseYearToPeriod)
				{
					result = Res.GetString("3b6bb622-4055-4435-b4bf-448bff1d0c90", "Year To {0}", YearToPeriod);
				}
				else
				{
					result = Res.GetString("1d7d3a31-2026-4833-aeb7-8e1febc04413", "All Periods");
				}

				return result;
			}
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			string result = "";

			if (UseSinglePeriod)
			{
				result = FieldName + " = " + fSinglePeriod;
			}
			else if (UsePeriodRange)
			{
				result = String.Format((NoResString)@"{0} >= {1} AND {0} <= {2}", FieldName, fFromPeriod, fToPeriod);
			}
			else if (UseYearToPeriod)
			{
				result = String.Format((NoResString)@"{0} >= {1} AND {0} <= {2}", FieldName, fYearStartPeriod, fYearToPeriod);
			}

			return result;
		}

		public ZBool UseSinglePeriod
		{
			get { return fUseSinglePeriod; }
			set
			{
				fUseSinglePeriod = value;
				UseSinglePeriodInfo.RefreshBinding();
				SinglePeriodInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UseSinglePeriodInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(UseSinglePeriod));
				info.HumanReadableName = DisplayName;
				return info;
			}
		}

		public ZBool UsePeriodRange
		{
			get { return fUsePeriodRange; }
			set
			{
				fUsePeriodRange = value;
				UsePeriodRangeInfo.RefreshBinding();
				FromPeriodInfo.RefreshBinding();
				ToPeriodInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UsePeriodRangeInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(UsePeriodRange));
				info.HumanReadableName = DisplayName;
				return info;
			}
		}

		public ZBool UseYearToPeriod
		{
			get { return fUseYearToPeriod; }
			set
			{
				fUseYearToPeriod = value;
				UseYearToPeriodInfo.RefreshBinding();
				YearToPeriodInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UseYearToPeriodInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(UseYearToPeriod));
				info.HumanReadableName = DisplayName;
				return info;
			}
		}

		public ZBool UseAllPeriods
		{
			get { return useAllPeriods; }
			set
			{
				SetNonPersistentPropertyValue(UseAllPeriodsInfo, ref useAllPeriods, value);
			}
		}

		public ZPropertyInfo UseAllPeriodsInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(UseAllPeriods));
				info.HumanReadableName = DisplayName;
				return info;
			}
		}

		#region Single Period

		public ZInt SinglePeriod
		{
			get { return Scheduled ? ScheduleSinglePeriod : RunSinglePeriod; }
			set
			{
				if (Scheduled)
				{
					ScheduleSinglePeriod = value;
				}
				else
				{
					RunSinglePeriod = value;
				}
			}
		}

		ZInt runSinglePeriod;

		protected ZInt RunSinglePeriod
		{
			get { return runSinglePeriod; }
			set
			{
				runSinglePeriod = value;
				if (!IsValidationSuspended)
				{
					ValidateSinglePeriod(RunSinglePeriod);
				}
				SinglePeriodInfo.RefreshBinding();
			}
		}

		ZInt scheduleSinglePeriod;

		protected ZInt ScheduleSinglePeriod
		{
			get { return scheduleSinglePeriod; }
			set
			{
				scheduleSinglePeriod = value;
				if (!IsValidationSuspended)
				{
					ValidateSinglePeriod(ScheduleSinglePeriod);
				}
				SinglePeriodInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SinglePeriodInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(SinglePeriod));
				result.HumanReadableName = DisplayName;
				return result;
			}
		}

		protected void ValidateSinglePeriod(ZInt period)
		{
			if (UseSinglePeriod)
			{
				SinglePeriodInfo.ClearAllNotifications();
				ValidatePeriod(period, SinglePeriodInfo);
			}
		}

		protected bool SinglePeriod_ReadOnly
		{
			get { return Scheduled || !UseSinglePeriod; }
		}

		#endregion

		#region FromPeriod

		public ZInt FromPeriod
		{
			get { return Scheduled ? ScheduleFromPeriod : RunFromPeriod; }
			set
			{
				if (Scheduled)
				{
					ScheduleFromPeriod = value;
				}
				else
				{
					RunFromPeriod = value;
				}
			}
		}

		ZInt runFromPeriod;

		protected ZInt RunFromPeriod
		{
			get { return runFromPeriod; }
			set
			{
				runFromPeriod = value;
				if (!IsValidationSuspended)
				{
					ValidateFromPeriod(RunFromPeriod);
				}
				FromPeriodInfo.RefreshBinding();
			}
		}

		ZInt scheduleFromPeriod;

		protected ZInt ScheduleFromPeriod
		{
			get { return scheduleFromPeriod; }
			set
			{
				scheduleFromPeriod = value;
				if (!IsValidationSuspended)
				{
					ValidateFromPeriod(ScheduleFromPeriod);
				}
				FromPeriodInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FromPeriodInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(FromPeriod));
				result.HumanReadableName = DisplayName;
				return result;
			}
		}

		protected void ValidateFromPeriod(ZInt fromPeriod)
		{
			if (UsePeriodRange && !IsValidationSuspended)
			{
				FromPeriodInfo.ClearAllNotifications();
				ValidatePeriod(fromPeriod, FromPeriodInfo);
			}
		}

		protected bool FromPeriod_ReadOnly
		{
			get { return Scheduled || !UsePeriodRange; }
		}

		#endregion

		#region ToPeriod

		public ZInt ToPeriod
		{
			get { return Scheduled ? ScheduleToPeriod : RunToPeriod; }
			set
			{
				if (Scheduled)
				{
					ScheduleToPeriod = value;
				}
				else
				{
					RunToPeriod = value;
				}
			}
		}

		ZInt runToPeriod;

		protected ZInt RunToPeriod
		{
			get { return runToPeriod; }
			set
			{
				runToPeriod = value;
				if (!IsValidationSuspended)
				{
					ValidateToPeriod(RunToPeriod);
				}
				ToPeriodInfo.RefreshBinding();
			}
		}

		ZInt scheduleToPeriod;

		protected ZInt ScheduleToPeriod
		{
			get { return scheduleToPeriod; }
			set
			{
				scheduleToPeriod = value;
				if (!IsValidationSuspended)
				{
					ValidateToPeriod(ScheduleToPeriod);
				}
				ToPeriodInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ToPeriodInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(ToPeriod));
				result.HumanReadableName = DisplayName;
				return result;
			}
		}

		protected void ValidateToPeriod(ZInt toPeriod)
		{
			if (UsePeriodRange)
			{
				ToPeriodInfo.ClearAllNotifications();
				ValidatePeriod(toPeriod, ToPeriodInfo);
			}
		}

		protected bool ToPeriod_ReadOnly
		{
			get { return Scheduled || !UsePeriodRange; }
		}

		#endregion

		#region YearToPeriod

		public ZInt YearToPeriod
		{
			get { return Scheduled ? ScheduleYearToPeriod : RunYearToPeriod; }
			set
			{
				if (Scheduled)
				{
					ScheduleYearToPeriod = value;
				}
				else
				{
					RunYearToPeriod = value;
				}
			}
		}

		ZInt runYearToPeriod;
		ZInt runYearStartPeriod;

		protected ZInt RunYearToPeriod
		{
			get { return runYearToPeriod; }
			set
			{
				runYearToPeriod = value;
				runYearStartPeriod = PeriodCalculator.GetFirstPeriodForYear(value / 100);
				if (!IsValidationSuspended)
				{
					ValidateYearToPeriod(RunYearToPeriod);
				}
				YearToPeriodInfo.RefreshBinding();
			}
		}

		ZInt scheduleYearToPeriod;
		ZInt scheduleYearStartPeriod;

		protected ZInt ScheduleYearToPeriod
		{
			get { return scheduleYearToPeriod; }
			set
			{
				scheduleYearToPeriod = value;
				scheduleYearStartPeriod = PeriodCalculator.GetFirstPeriodForYear(value / 100);
				if (!IsValidationSuspended)
				{
					ValidateYearToPeriod(ScheduleYearToPeriod);
				}
				YearToPeriodInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo YearToPeriodInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(YearToPeriod));
				result.HumanReadableName = DisplayName;
				return result;
			}
		}

		public void ValidateYearToPeriod(ZInt yearToPeriod)
		{
			if (UseYearToPeriod)
			{
				YearToPeriodInfo.ClearAllNotifications();
				ValidatePeriod(yearToPeriod, YearToPeriodInfo);
			}
		}
		protected bool YearToPeriod_ReadOnly
		{
			get { return Scheduled || !UseYearToPeriod; }
		}

		#endregion

		public ZInt YearStartPeriod
		{
			get { return Scheduled ? scheduleYearStartPeriod : runYearStartPeriod; }
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.AccountingPeriodFieldUserControl; }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (UseSinglePeriod)
			{
				SinglePeriodInfo.ClearAllNotifications();
				ValidatePeriod(SinglePeriod, SinglePeriodInfo);
			}
			else if (UsePeriodRange)
			{
				ToPeriodInfo.ClearAllNotifications();
				FromPeriodInfo.ClearAllNotifications();
				ValidatePeriod(ToPeriod, ToPeriodInfo);
				ValidatePeriod(FromPeriod, FromPeriodInfo);
			}
			else if (UseYearToPeriod)
			{
				YearToPeriodInfo.ClearAllNotifications();
				ValidatePeriod(YearToPeriod, YearToPeriodInfo);
			}
		}

		#region Debug Only
#if DEBUG
		public override void ClearValueForUnitTest()
		{
			if (Scheduled)
			{
				scheduleSinglePeriod = ZInt.Zero;
				scheduleFromPeriod = ZInt.Zero;
				scheduleToPeriod = ZInt.Zero;
				scheduleYearStartPeriod = ZInt.Zero;
				scheduleYearToPeriod = ZInt.Zero;
			}
			else
			{
				runSinglePeriod = ZInt.Zero;
				runFromPeriod = ZInt.Zero;
				runToPeriod = ZInt.Zero;
				runYearStartPeriod = ZInt.Zero;
				runYearToPeriod = ZInt.Zero;
			}
		}
#endif
		#endregion

		#region Schedule

		public AccPeriodSchedule SinglePeriodSchedule
		{
			get
			{
				if (Scheduled)
				{
					if (singlePeriodSchedule == null)
					{
						singlePeriodSchedule = new AccPeriodSchedule();
						singlePeriodSchedule.ScheduleTask = scheduleTask;
						singlePeriodSchedule.ValueChanged += delegate { UpdateSinglePeriodFromSchedule(); };
					}
					return singlePeriodSchedule;
				}
				return null;
			}
		}

		public AccPeriodSchedule YearToPeriodSchedule
		{
			get
			{
				if (Scheduled)
				{
					if (yearToPeriodSchedule == null)
					{
						yearToPeriodSchedule = new AccPeriodSchedule();
						yearToPeriodSchedule.ScheduleTask = scheduleTask;
						yearToPeriodSchedule.ValueChanged += delegate { UpdateYearToPeriodFromSchedule(); };
					}
					return yearToPeriodSchedule;
				}
				return null;
			}
		}

		public AccPeriodSchedule FromPeriodSchedule
		{
			get
			{
				if (Scheduled)
				{
					if (fromPeriodSchedule == null)
					{
						fromPeriodSchedule = new AccPeriodSchedule();
						fromPeriodSchedule.ScheduleTask = scheduleTask;
						fromPeriodSchedule.ValueChanged += delegate { UpdateFromPeriodFromSchedule(); };
					}
					return fromPeriodSchedule;
				}
				return null;
			}
		}

		public AccPeriodSchedule ToPeriodSchedule
		{
			get
			{
				if (Scheduled)
				{
					if (toPeriodSchedule == null)
					{
						toPeriodSchedule = new AccPeriodSchedule();
						toPeriodSchedule.ScheduleTask = scheduleTask;
						toPeriodSchedule.ValueChanged += delegate { UpdateToPeriodFromSchedule(); };
					}
					return toPeriodSchedule;
				}
				return null;
			}
		}

		protected bool Scheduled
		{
			get { return (scheduleTask != null); }
		}

		void UpdateSinglePeriodFromSchedule()
		{
			ScheduleSinglePeriod = singlePeriodSchedule.GetSchedulePeriod();
		}

		void UpdateFromPeriodFromSchedule()
		{
			ScheduleFromPeriod = fromPeriodSchedule.GetSchedulePeriod();
		}

		void UpdateToPeriodFromSchedule()
		{
			ScheduleToPeriod = toPeriodSchedule.GetSchedulePeriod();
		}

		void UpdateYearToPeriodFromSchedule()
		{
			ScheduleYearToPeriod = yearToPeriodSchedule.GetSchedulePeriod();
		}

		AccPeriodSchedule singlePeriodSchedule;
		AccPeriodSchedule yearToPeriodSchedule;
		AccPeriodSchedule fromPeriodSchedule;
		AccPeriodSchedule toPeriodSchedule;
		ReportScheduleTask scheduleTask;

		#endregion

		#region Implementation

		ZBool fUseSinglePeriod;
		ZBool fUsePeriodRange;
		ZBool fUseYearToPeriod;
		ZBool useAllPeriods;
		AccountingPeriodCalculator fPeriodCalculator;

		protected SqlParameterList fSingleParamList, fRangeParamList, fYearToPeriodParamList;
		protected SqlParameter fSinglePeriod, fFromPeriod, fToPeriod, fYearToPeriod, fYearStartPeriod;

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

		protected void ValidatePeriod(ZInt period, ZPropertyInfo periodInfo)
		{
			if (!Scheduled && !PeriodCalculator.IsPeriodValid(period))
			{
				periodInfo.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(period));
			}
		}

		#endregion

		#region ISchedulableFilterField Members

		public void SetScheduleTask(ReportScheduleTask value)
		{
			scheduleTask = value;
			if (singlePeriodSchedule != null)
			{
				singlePeriodSchedule.ScheduleTask = scheduleTask;
				UpdateSinglePeriodFromSchedule();
			}
			if (fromPeriodSchedule != null)
			{
				fromPeriodSchedule.ScheduleTask = scheduleTask;
				UpdateFromPeriodFromSchedule();
			}
			if (toPeriodSchedule != null)
			{
				toPeriodSchedule.ScheduleTask = scheduleTask;
				UpdateToPeriodFromSchedule();
			}
			if (yearToPeriodSchedule != null)
			{
				yearToPeriodSchedule.ScheduleTask = scheduleTask;
				UpdateYearToPeriodFromSchedule();
			}
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is AccountingPeriodField accountingPeriodField)
			{
				UseAllPeriods = accountingPeriodField.UseAllPeriods;
				UsePeriodRange = accountingPeriodField.UsePeriodRange;
				UseYearToPeriod = accountingPeriodField.UseYearToPeriod;
				UseSinglePeriod = accountingPeriodField.UseSinglePeriod;

				if (accountingPeriodField.singlePeriodSchedule != null)
				{
					if (singlePeriodSchedule == null)
					{
						singlePeriodSchedule = new AccPeriodSchedule();
						singlePeriodSchedule.ValueChanged += delegate { UpdateSinglePeriodFromSchedule(); };
					}
					singlePeriodSchedule.CopyChangesFrom(accountingPeriodField.singlePeriodSchedule);
					UpdateSinglePeriodFromSchedule();
				}
				else
				{
					singlePeriodSchedule?.Clear();
				}

				if (accountingPeriodField.fromPeriodSchedule != null)
				{
					if (fromPeriodSchedule == null)
					{
						fromPeriodSchedule = new AccPeriodSchedule();
						fromPeriodSchedule.ValueChanged += delegate { UpdateFromPeriodFromSchedule(); };
					}
					fromPeriodSchedule.CopyChangesFrom(accountingPeriodField.fromPeriodSchedule);
					UpdateFromPeriodFromSchedule();
				}
				else
				{
					fromPeriodSchedule?.Clear();
				}

				if (accountingPeriodField.toPeriodSchedule != null)
				{
					if (toPeriodSchedule == null)
					{
						toPeriodSchedule = new AccPeriodSchedule();
						toPeriodSchedule.ValueChanged += delegate { UpdateToPeriodFromSchedule(); };
					}

					toPeriodSchedule.CopyChangesFrom(accountingPeriodField.toPeriodSchedule);
					UpdateToPeriodFromSchedule();
				}
				else
				{
					toPeriodSchedule?.Clear();
				}

				if (accountingPeriodField.yearToPeriodSchedule != null)
				{
					if (yearToPeriodSchedule == null)
					{
						yearToPeriodSchedule = new AccPeriodSchedule();
						yearToPeriodSchedule.ValueChanged += delegate { UpdateYearToPeriodFromSchedule(); };
					}

					yearToPeriodSchedule.CopyChangesFrom(accountingPeriodField.yearToPeriodSchedule);
					UpdateYearToPeriodFromSchedule();
				}
				else
				{
					yearToPeriodSchedule?.Clear();
				}

				RunSinglePeriod = accountingPeriodField.RunSinglePeriod;
				RunFromPeriod = accountingPeriodField.RunFromPeriod;
				RunToPeriod = accountingPeriodField.RunToPeriod;
				RunYearToPeriod = accountingPeriodField.RunYearToPeriod;
			}
		}

		public override void ClearValues()
		{
			this.SinglePeriod = ZInt.Zero;
			this.FromPeriod = ZInt.Zero;
			this.ToPeriod = ZInt.Zero;
			this.YearToPeriod = ZInt.Zero;
			singlePeriodSchedule?.Clear();
			fromPeriodSchedule?.Clear();
			toPeriodSchedule?.Clear();
			yearToPeriodSchedule?.Clear();
			UseAllPeriods = false;
			UsePeriodRange = false;
			UseYearToPeriod = false;
			UseSinglePeriod = false;
		}

		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new AccountingPeriodFilter();
			SetBaseFilterData(filterData);

			filterData.UseSinglePeriod = UseSinglePeriod;
			filterData.UsePeriodRange = UsePeriodRange;
			filterData.UseYearToPeriod = UseYearToPeriod;
			filterData.UseAllPeriods = UseAllPeriods;
			filterData.SinglePeriod = SinglePeriod;
			filterData.YearToPeriod = YearToPeriod;
			filterData.ToPeriod = ToPeriod;
			filterData.FromPeriod = FromPeriod;
			if (Scheduled)
			{
				if (SinglePeriodSchedule.IsValid)
				{
					filterData.ScheduleStorageSinglePeriod = SinglePeriodSchedule.ToStorageValue();
				}

				if (YearToPeriodSchedule.IsValid)
				{
					filterData.ScheduleStorageYearTo = YearToPeriodSchedule.ToStorageValue();
				}

				if (FromPeriodSchedule.IsValid)
				{
					filterData.ScheduleStorageFrom = FromPeriodSchedule.ToStorageValue();
				}

				if (ToPeriodSchedule.IsValid)
				{
					filterData.ScheduleStorageTo = ToPeriodSchedule.ToStorageValue();
				}
			}

			reportFilterData.AccountingPeriodFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.AccountingPeriodFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				UseSinglePeriod = selectedValue.UseSinglePeriod;
				UsePeriodRange = selectedValue.UsePeriodRange;
				UseYearToPeriod = selectedValue.UseYearToPeriod;
				UseAllPeriods = selectedValue.UseAllPeriods;
				if (Scheduled)
				{
					if (!TryUpdateSchedule(selectedValue.ScheduleStorageSinglePeriod, UpdateSinglePeriodFromSchedule, out singlePeriodSchedule))
					{
						SinglePeriodSchedule.Clear();
					}
					if (!TryUpdateSchedule(selectedValue.ScheduleStorageYearTo, UpdateYearToPeriodFromSchedule, out yearToPeriodSchedule))
					{
						YearToPeriodSchedule.Clear();
					}
					if (!TryUpdateSchedule(selectedValue.ScheduleStorageFrom, UpdateFromPeriodFromSchedule, out fromPeriodSchedule))
					{
						FromPeriodSchedule.Clear();
					}
					if (!TryUpdateSchedule(selectedValue.ScheduleStorageTo, UpdateToPeriodFromSchedule, out toPeriodSchedule))
					{
						ToPeriodSchedule.Clear();
					}
				}
				else
				{
					SinglePeriod = selectedValue.SinglePeriod;
					YearToPeriod = selectedValue.YearToPeriod;
					ToPeriod = selectedValue.ToPeriod;
					FromPeriod = selectedValue.FromPeriod;
				}
			}
		}

		bool TryUpdateSchedule(DateTime? scheduleStorage, Action updatePeriodAction, out AccPeriodSchedule scheduleToUpdate)
		{
			scheduleToUpdate = null;
			if (scheduleStorage.HasValue && AccPeriodSchedule.TryParse(scheduleStorage.Value, out scheduleToUpdate))
			{
				scheduleToUpdate.ValueChanged += delegate { updatePeriodAction?.Invoke(); };
				scheduleToUpdate.ScheduleTask = scheduleTask;
				updatePeriodAction?.Invoke();
				return true;
			}
			return false;
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var filterData = CreateJsonDataCore();
			SetJsonData(filterData);

			filterData.UseSinglePeriod = UseSinglePeriod;
			filterData.UsePeriodRange = UsePeriodRange;
			filterData.UseYearToPeriod = UseYearToPeriod;
			filterData.UseAllPeriods = UseAllPeriods;

			filterData.SinglePeriod = GetSchedulableStore(RunSinglePeriod, singlePeriodSchedule);
			filterData.YearToPeriod = GetSchedulableStore(RunYearToPeriod, yearToPeriodSchedule);
			filterData.ToPeriod = GetSchedulableStore(RunToPeriod, toPeriodSchedule);
			filterData.FromPeriod = GetSchedulableStore(RunFromPeriod, fromPeriodSchedule);

			return filterData;
		}
		protected virtual AccountingPeriodFieldJsonData CreateJsonDataCore() => new AccountingPeriodFieldJsonData();

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

		void SetSinglePeriodValue(SchedulableStore<int> store)
		{
			if (store != null)
			{
				RunSinglePeriod = store.RunValue ?? 0;
				if (store.Schedule != null)
				{
					if (AccPeriodSchedule.TryParse(store.Schedule.Value, out singlePeriodSchedule))
					{
						singlePeriodSchedule.ValueChanged += delegate { UpdateSinglePeriodFromSchedule(); };
					}
				}
			}
		}

		void SetFromPeriodValue(SchedulableStore<int> store)
		{
			if (store != null)
			{
				RunFromPeriod = store.RunValue ?? 0;
				if (store.Schedule != null)
				{
					if (AccPeriodSchedule.TryParse(store.Schedule.Value, out fromPeriodSchedule))
					{
						fromPeriodSchedule.ValueChanged += delegate { UpdateFromPeriodFromSchedule(); };
					}
				}
			}
		}

		void SetToPeriodValue(SchedulableStore<int> store)
		{
			if (store != null)
			{
				RunToPeriod = store.RunValue ?? 0;
				if (store.Schedule != null)
				{
					if (AccPeriodSchedule.TryParse(store.Schedule.Value, out toPeriodSchedule))
					{
						toPeriodSchedule.ValueChanged += delegate { UpdateToPeriodFromSchedule(); };
					}
				}
			}
		}

		void SetYearToPeriodValue(SchedulableStore<int> store)
		{
			if (store != null)
			{
				RunYearToPeriod = store.RunValue ?? 0;
				if (store.Schedule != null)
				{
					if (AccPeriodSchedule.TryParse(store.Schedule.Value, out yearToPeriodSchedule))
					{
						yearToPeriodSchedule.ValueChanged += delegate { UpdateYearToPeriodFromSchedule(); };
					}
				}
			}
		}

		#endregion
	}
}
