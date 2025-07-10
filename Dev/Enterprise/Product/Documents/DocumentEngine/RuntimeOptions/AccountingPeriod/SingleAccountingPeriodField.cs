using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
#if DEBUG
	public interface ISingleAccountingPeriodFieldUnitTestHelper
	{
		ZInt SinglePeriod { get; set; }
	}
#endif

	public class SingleAccountingPeriodField : FilterFieldWithUTSupport, ISchedulableFilterField, IJsonSerializable
#if DEBUG
, ISingleAccountingPeriodFieldUnitTestHelper
#endif
	{
		public SingleAccountingPeriodField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		internal SingleAccountingPeriodField(SingleAccountingPeriodFieldJsonData data)
			: base(data)
		{
			CreateParameters();
			SetSinglePeriodValue(data.SinglePeriod);
		}

		protected override void SetNecessaryPropertiesForDeserializingCore(FilterField origin, CollectionOfIFilter filterCollection)
		{
			base.SetNecessaryPropertiesForDeserializingCore(origin, filterCollection);

			if (origin is SingleAccountingPeriodField originalSingleAccountingPeriodField)
			{
				PeriodCalculator.Company = originalSingleAccountingPeriodField.PeriodCalculator.Company;
			}
		}

		#endregion

		void CreateParameters()
		{
			fSinglePeriod = new SqlParameter(SqlParameterNameGenerator.Next(), 0);
		}

		[ReadOnlyMember(nameof(Scheduled))]
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

		protected void ValidateSinglePeriod(ZInt period)
		{
			SinglePeriodInfo.ClearAllNotifications();

			if (!IsValid)
			{
				SinglePeriodInfo.AddError(Res.GetString("E0B89E2C-D0BD-4548-8DA7-CF3597CF8FE9", "{0}\r\nPlease check if the relevant periods have been set up.", ValidationError));
			}
			else if (!period.IsEmpty && !PeriodCalculator.IsPeriodValid(period))
			{
				SinglePeriodInfo.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(period, PeriodCalculator.Company));
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

		#region Schedule

		public AccPeriodSchedule Schedule
		{
			get
			{
				if (Scheduled)
				{
					if (schedule == null)
					{
						schedule = new AccPeriodSchedule();
						schedule.ScheduleTask = scheduleTask;
						schedule.ValueChanged += delegate { UpdateSinglePeriodFromSchedule(); };
					}
					return schedule;
				}
				return null;
			}
		}

		bool Scheduled
		{
			get { return (scheduleTask != null); }
		}

		void UpdateSinglePeriodFromSchedule()
		{
			ScheduleSinglePeriod = schedule.GetSchedulePeriod();
		}

		AccPeriodSchedule schedule;
		ReportScheduleTask scheduleTask;

		#endregion

		#region Overrides

		public override bool IsEmpty
		{
			get
			{
				return SinglePeriod.IsEmpty;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateSinglePeriod(SinglePeriod);
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.SingleAccountingPeriodUserControl; }
		}

		public override object ValueAsObject
		{
			get { return SinglePeriod; }
		}

		protected override void AddSpecialisedValueProviders()
		{
			base.AddSpecialisedValueProviders();
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".FromDate", new ValueReplacers.ReplacementProviderMethod(GetFromDateReplacement)));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ToDate", new ValueReplacers.ReplacementProviderMethod(GetToDateReplacement)));

			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ToNextDate", new ValueReplacers.ReplacementProviderMethod(GetToNextDateReplacement)));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ToNextDateWithMinuteSubtracted", new ValueReplacers.ReplacementProviderMethod(GetToNextDateWithMinuteSubtractedReplacement)));

			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ThisPeriodLastYearFromDate", new ValueReplacers.ReplacementProviderMethod(GetThisPeriodLastYearFromDateReplacement)));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ThisPeriodLastYearToDate", new ValueReplacers.ReplacementProviderMethod(GetThisPeriodLastYearToDateReplacement)));

			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ThisPeriodLastYearToNextDate", new ValueReplacers.ReplacementProviderMethod(GetThisPeriodLastYearToNextDateReplacement)));

			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".LastYearFromDate", new ValueReplacers.ReplacementProviderMethod(GetLastYearFromDateReplacement)));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".LastYearToDate", new ValueReplacers.ReplacementProviderMethod(GetLastYearToDateReplacement)));

			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".LastYearToNextDate", new ValueReplacers.ReplacementProviderMethod(GetLastYearToNextDateReplacement)));

			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ThisYearFromDate", new ValueReplacers.ReplacementProviderMethod(GetThisYearFromDateReplacement)));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ThisYearToDate", new ValueReplacers.ReplacementProviderMethod(GetThisYearToDateReplacement)));

			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ThisYearToNextDate", new ValueReplacers.ReplacementProviderMethod(GetThisYearToNextDateReplacement)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.FromDate>", ResString.GetMultilingualString("4c2bb8b5-313b-4508-9cd7-45e7a98ec8e3", "Returns the first day of this period.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToDate>", ResString.GetMultilingualString("ce66430c-90bf-47d3-881b-ab72064393e3", "Returns the last day of this period.")));

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToNextDate>", ResString.GetMultilingualString("1b613d2a-993a-431e-9e7f-b0c3de063de0", "Returns the day following the last day of this period.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToNextDateWithMinuteSubtracted>", ResString.GetMultilingualString("17f09aac-8c10-4c3e-b472-f9f264e0e0bd", "Returns the day following the last day of this period minus 1 minute.")));

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ThisPeriodLastYearFromDate>", ResString.GetMultilingualString("f31176f3-cd44-417d-9aea-1d1e98c1d65d", "Returns the first day of this period last year.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ThisPeriodLastYearToDate>", ResString.GetMultilingualString("aabc138e-0b70-4f18-b453-f8bd7b538d41", "Returns the last day of this period last year.")));

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ThisPeriodLastYearToNextDate>", ResString.GetMultilingualString("b53be5bb-a41e-40e9-9a0b-290ccb8e35f4", "Returns the day following the last day of this period, one year ago.")));

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.LastYearFromDate>", ResString.GetMultilingualString("0a87879b-1d09-4bdb-9caf-62c1c333fe0a", "Returns the first day of the first period for last year.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.LastYearToDate>", ResString.GetMultilingualString("0c4c1f4b-0621-4a62-8d2b-03399d870db5", "Returns the last day of the last period for last year.")));

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.LastYearToNextDate>", ResString.GetMultilingualString("1e4b7ac9-29bf-44d4-8f04-0d7b094af549", "Returns the day following the last day of the last period for last year.")));

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ThisYearFromDate>", ResString.GetMultilingualString("f8e308ac-99e4-4fe2-af0e-3db4da8ffdeb", "Returns the first day of the first period of this year.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ThisYearToDate>", ResString.GetMultilingualString("bc4e4859-9c35-46da-a4da-d65b4d87ad06", "Returns the last day of the last period of this year.")));

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ThisYearToNextDate>", ResString.GetMultilingualString("a14abfe5-d0a9-4229-b16a-bae983521460", "Returns the day following last day of the last period of this year.")));
		}

		protected object GetFromDateReplacement(string macro, Report report)
		{
			return PeriodCalculator.GetFirstDayForPeriod(SinglePeriod).Date;
		}

		protected object GetToDateReplacement(string macro, Report report)
		{
			return PeriodCalculator.GetLastDayForPeriod(SinglePeriod).Date;
		}

		protected object GetToNextDateReplacement(string macro, Report report)
		{
			ZDate date = PeriodCalculator.GetLastDayForPeriod(SinglePeriod).Date;
			return date.IsValid ? date.AddDays(1) : date;
		}

		protected object GetToNextDateWithMinuteSubtractedReplacement(string macro, Report report)
		{
			ZDate date = PeriodCalculator.GetLastDayForPeriod(SinglePeriod).Date;
			return date.IsValid ? date.AddDays(1).AddMinutes(-1) : date;
		}

		protected object GetThisPeriodLastYearFromDateReplacement(string macro, Report report)
		{
			return PeriodCalculator.GetFirstDayForPeriod(SinglePeriod - 100).Date;
		}

		protected object GetThisPeriodLastYearToDateReplacement(string macro, Report report)
		{
			return PeriodCalculator.GetLastDayForPeriod(SinglePeriod - 100).Date;
		}

		protected object GetThisPeriodLastYearToNextDateReplacement(string macro, Report report)
		{
			ZDate date = PeriodCalculator.GetLastDayForPeriod(SinglePeriod - 100).Date;
			return date.IsValid ? date.AddDays(1) : date;
		}

		protected object GetLastYearFromDateReplacement(string macro, Report report)
		{
			return PeriodCalculator.GetFirstDayForPeriod(PeriodCalculator.GetFirstPeriodForYear(SinglePeriod / 100 - 1)).Date;
		}

		protected object GetLastYearToDateReplacement(string macro, Report report)
		{
			return PeriodCalculator.GetLastDayForPeriod(PeriodCalculator.GetLastPeriodForYear(SinglePeriod / 100 - 1)).Date;
		}

		protected object GetLastYearToNextDateReplacement(string macro, Report report)
		{
			ZDate date = PeriodCalculator.GetLastDayForPeriod(PeriodCalculator.GetLastPeriodForYear(SinglePeriod / 100 - 1)).Date;
			return date.IsValid ? date.AddDays(1) : date;
		}

		protected object GetThisYearFromDateReplacement(string macro, Report report)
		{
			return PeriodCalculator.GetFirstDayForPeriod(PeriodCalculator.GetFirstPeriodForYear(SinglePeriod / 100)).Date;
		}

		protected object GetThisYearToDateReplacement(string macro, Report report)
		{
			return PeriodCalculator.GetLastDayForPeriod(PeriodCalculator.GetLastPeriodForYear(SinglePeriod / 100)).Date;
		}

		protected object GetThisYearToNextDateReplacement(string macro, Report report)
		{
			ZDate date = PeriodCalculator.GetLastDayForPeriod(PeriodCalculator.GetLastPeriodForYear(SinglePeriod / 100)).Date;
			return date.IsValid ? date.AddDays(1) : date;
		}

		#region DEBUG
#if DEBUG
		public override void ClearValueForUnitTest()
		{
			SinglePeriod = 0;
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year + 1);

			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(Factory);
			ZInt currentPeriod = periodCalc.GetPeriodFromDate(ZDateTime.Today);
			SinglePeriod = currentPeriod;
		}
#endif
		#endregion

		#endregion

		#region Implementation

		SqlParameter fSinglePeriod;

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			return FieldName + " = " + fSinglePeriod;
		}

		protected override SqlParameterList GetSqlParametersCore()
		{
			var result = new SqlParameterList();
			if (IsEmpty)
			{
				return result;
			}

			SetSQLParameterValue();

			result.Add(fSinglePeriod);
			return result;
		}

		protected void SetSQLParameterValue()
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
		AccountingPeriodCalculator fPeriodCalculator;

		#endregion

		#region ISchedulableFilterField Members

		public void SetScheduleTask(ReportScheduleTask value)
		{
			scheduleTask = value;
			if (schedule != null)
			{
				schedule.ScheduleTask = scheduleTask;
				UpdateSinglePeriodFromSchedule();
			}
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is SingleAccountingPeriodField singleAccountingPeriodField)
			{
				if (singleAccountingPeriodField.schedule != null)
				{
					if (schedule == null)
					{
						schedule = new AccPeriodSchedule();
						schedule.ValueChanged += delegate { UpdateSinglePeriodFromSchedule(); };
					}
					schedule.CopyChangesFrom(singleAccountingPeriodField.schedule);
					UpdateSinglePeriodFromSchedule();
				}
				else
				{
					schedule?.Clear();
				}

				RunSinglePeriod = singleAccountingPeriodField.RunSinglePeriod;
			}
		}

		public override void ClearValues()
		{
			SinglePeriod = ZInt.Zero;
			schedule?.Clear();
		}

		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new SingleAccountingPeriodFilter();
			SetBaseFilterData(filterData);

			filterData.SinglePeriod = SinglePeriod;
			if (Scheduled)
			{
				if (Schedule.IsValid)
				{
					filterData.ScheduleStorageValue = Schedule.ToStorageValue();
				}
			}

			reportFilterData.SingleAccountingPeriodFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.SingleAccountingPeriodFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				if (Scheduled)
				{
					if (selectedValue.ScheduleStorageValue.HasValue)
					{
						if (AccPeriodSchedule.TryParse(selectedValue.ScheduleStorageValue.Value, out schedule))
						{
							schedule.ValueChanged += delegate { UpdateSinglePeriodFromSchedule(); };
							schedule.ScheduleTask = scheduleTask;
							UpdateSinglePeriodFromSchedule();
						}
					}
				}
				else
				{
					SinglePeriod = selectedValue.SinglePeriod;
				}
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var jsonData = CreateJsonDataCore();
			SetJsonData(jsonData);
			jsonData.SinglePeriod = GetSchedulableStore(SinglePeriod, Schedule);
			return jsonData;
		}

		protected virtual SingleAccountingPeriodFieldJsonData CreateJsonDataCore()
		{
			return new SingleAccountingPeriodFieldJsonData();
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

		void SetSinglePeriodValue(SchedulableStore<int> store)
		{
			if (store != null)
			{
				RunSinglePeriod = store.RunValue ?? 0;
				if (store.Schedule != null)
				{
					if (AccPeriodSchedule.TryParse(store.Schedule.Value, out schedule))
					{
						schedule.ValueChanged += delegate
						{ UpdateSinglePeriodFromSchedule(); };
					}
				}
			}
		}

		#endregion
	}
}
