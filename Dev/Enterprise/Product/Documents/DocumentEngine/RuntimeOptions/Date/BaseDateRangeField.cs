using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class BaseDateRangeField<TBaseType, TZType> : FilterFieldWithUTSupport, IDateFormatSupport, System.Runtime.Serialization.ISerializable, ISchedulableFilterField, IJsonSerializable where TBaseType : struct where TZType : IZDate, new()
	{
		protected BaseDateRangeField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Constructor For IJsonSerializable

		internal BaseDateRangeField(BaseDateRangeFieldJsonData<TBaseType> data)
			: base(data)
		{
			Enum.TryParse(data.DateFormat, out pickerFormat);

			ConvertToUtc = data.ConvertToUtc;
			RequireBothFromAndToDates = data.RequireBothFromAndToDates;
			SubstituteMinDateForNullFrom = data.SubstituteMinDateForNullFrom;
			SubstituteMaxDateForNullTo = data.SubstituteMaxDateForNullTo;

			SetDateRangeValue(data);
		}

		#endregion

		readonly string fromDateParameterName = SqlParameterNameGenerator.Next();
		readonly string toDateParameterName = SqlParameterNameGenerator.Next();

		public bool SubstituteMinDateForNullFrom { get; set; }
		public bool SubstituteMaxDateForNullTo { get; set; }
		public bool ConvertToUtc { get; set; }
		public bool RequireBothFromAndToDates { get; set; }
		public int? DateRangeMaxYears { get; set; }
		public int? DateRangeMaxMonths { get; set; }

		public ZDateTimePickerFormat ZPickerFormat
		{
			get { return pickerFormat == DocEngineDatePickerFormats.Long ? ZDateTimePickerFormat.Long : ZDateTimePickerFormat.Short; }
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			var constraints = new List<string>();

			if (!ValueLow.IsEmpty)
			{
				constraints.Add(string.Format("{0} >= {1}", FieldName, fromDateParameterName));
			}

			if (!ValueHigh.IsEmpty)
			{
				constraints.Add(string.Format("{0} < {1}", FieldName, toDateParameterName));
			}

			return string.Join(" AND ", constraints.ToArray());
		}

		protected override SqlParameterList GetSqlParametersCore()
		{
			var fromDateParameter = new SqlParameter(fromDateParameterName, DBNull.Value);
			if (ValueLow.IsValid)
			{
				fromDateParameter.Value = GetFromParamCore().ToBaseDate();
			}

			var toDateParameter = new SqlParameter(toDateParameterName, DBNull.Value);
			if (ValueHigh.IsValid)
			{
				toDateParameter.Value = GetToDateParamCore().ToBaseDate();
			}

			var result = new SqlParameterList();
			result.Add(fromDateParameter);
			result.Add(toDateParameter);

			return result;
		}

		protected TBaseType CalculateToDateAccordingDateFormat()
		{
			if (PickerFormat == DocEngineDatePickerFormats.Long && !Scheduled)
			{
				return GenericDateTypeHelper.AddMinutesForDate((TBaseType)ValueHigh.ToBaseDate(), 1);
			}
			else
			{
				var daysToAdd = (bool)(HighSchedule?.ByHourAndMinute ?? false) ? 0 : 1;
				return GenericDateTypeHelper.AddDaysForDate((TBaseType)ValueHigh.ToBaseDate(), daysToAdd);
			}
		}

		protected abstract string DateFormat { get; }

		#region Value Low

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(Scheduled))]
		public TZType ValueLow
		{
			get { return Scheduled ? ScheduleValueLow : RunValueLow; }
			set
			{
				if (Scheduled)
				{
					ScheduleValueLow = value;
				}
				else
				{
					RunValueLow = value;
				}
			}
		}

		TZType runValueLow;

		protected TZType RunValueLow
		{
			get { return runValueLow; }
			set
			{
				if (!value.IsValid)
				{
					runValueLow = default;
				}
				else
				{
					runValueLow = (TZType)GenericDateTypeHelper.ToZDateTimeOrZDateTimeOffset(GetLowerDate(value));
				}

				if (!IsValidationSuspended)
				{
					ValidateRunValueLow();
				}

				ValueLowInfo.RefreshBinding();
			}
		}

		TZType scheduleValueLow;

		protected TZType ScheduleValueLow
		{
			get { return scheduleValueLow; }
			set
			{
				if (!value.IsValid)
				{
					scheduleValueLow = default;
				}
				else
				{
					scheduleValueLow = (TZType)GenericDateTypeHelper.ToZDateTimeOrZDateTimeOffset(GetLowerDate(value));
				}

				if (!IsValidationSuspended)
				{
					ValidateScheduleValueLow();
				}

				ValueLowInfo.RefreshBinding();
			}
		}

		protected abstract TBaseType GetLowerDate(TZType value);

		public ZPropertyInfo ValueLowInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(ValueLow));
				result.HumanReadableName = Res.GetString("90187ced-2704-4ed9-be90-7aade783973e", "{0} - From", DisplayName);
				return result;
			}
		}

		public void ValidateValueLow()
		{
			if (Scheduled)
			{
				ValidateScheduleValueLow();
			}
			else
			{
				ValidateRunValueLow();
			}
		}

		protected void ValidateRunValueLow()
		{
			ValidateValueLowCore(RunValueLow, RunValueHigh);
			ValidateValueHighCore(RunValueLow, RunValueHigh);
		}

		protected void ValidateScheduleValueLow()
		{
			ValidateValueLowCore(ScheduleValueLow, ScheduleValueHigh);
			ValidateValueHighCore(ScheduleValueLow, ScheduleValueHigh);
		}

		protected virtual void ValidateValueLowCore(TZType valueLow, TZType valueHigh)
		{
			ValueLowInfo.ClearAllNotifications();
			if (!IsValid)
			{
				ValueLowInfo.AddError(ValidationError);
			}
			else if (!valueHigh.IsEmpty && valueHigh.CompareTo(valueLow) < 0)
			{
				ValueLowInfo.AddNotification(CargoWise.EntityFramework.NotificationType.Error, Res.GetString("19fa46c1-794f-4de8-916e-18f499450f20", "The 'From date' must be before 'To date'"));
			}
		}

		#endregion

		#region Value High

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(Scheduled))]
		public TZType ValueHigh
		{
			get { return Scheduled ? ScheduleValueHigh : RunValueHigh; }
			set
			{
				if (Scheduled)
				{
					ScheduleValueHigh = value;
				}
				else
				{
					RunValueHigh = value;
				}
			}
		}

		TZType runValueHigh;
		protected TZType RunValueHigh
		{
			get
			{
				if (runValueHigh.IsValid)
				{
					return (TZType)runValueHigh.AddDays(-1);
				}
				else
				{
					return default;
				}
			}
			set
			{
				if (!value.IsValid)
				{
					runValueHigh = default;
				}
				else
				{
					runValueHigh = (TZType)GenericDateTypeHelper.ToZDateTimeOrZDateTimeOffset(GetUpperDate(value));
				}

				if (!IsValidationSuspended)
				{
					ValidateRunValueHigh();
				}

				ValueHighInfo.RefreshBinding();
			}
		}

		TZType scheduleValueHigh;

		protected TZType ScheduleValueHigh
		{
			get
			{
				if (scheduleValueHigh.IsValid)
				{
					return (TZType)scheduleValueHigh.AddDays(-1);
				}
				else
				{
					return default;
				}
			}
			set
			{
				if (!value.IsValid)
				{
					scheduleValueHigh = default;
				}
				else
				{
					scheduleValueHigh = (TZType)GenericDateTypeHelper.ToZDateTimeOrZDateTimeOffset(GetUpperDate(value));
				}

				if (!IsValidationSuspended)
				{
					ValidateScheduleValueHigh();
				}

				ValueHighInfo.RefreshBinding();
			}
		}

		protected abstract TBaseType GetUpperDate(TZType value);

		public ZPropertyInfo ValueHighInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(ValueHigh));
				result.HumanReadableName = Res.GetString("b28d3b24-70ff-4412-b84a-71dca55862aa", "{0} - To", DisplayName);
				return result;
			}
		}

		public void ValidateValueHigh()
		{
			if (Scheduled)
			{
				ValidateScheduleValueHigh();
			}
			else
			{
				ValidateRunValueHigh();
			}
		}

		protected void ValidateRunValueHigh()
		{
			ValidateValueHighCore(RunValueLow, RunValueHigh);
			ValidateValueLowCore(RunValueLow, RunValueHigh);
		}

		protected void ValidateScheduleValueHigh()
		{
			ValidateValueHighCore(ScheduleValueLow, ScheduleValueHigh);
			ValidateValueLowCore(ScheduleValueLow, ScheduleValueHigh);
		}

		protected virtual void ValidateValueHighCore(TZType valueLow, TZType valueHigh)
		{
			ValueHighInfo.ClearAllNotifications();
			if (!IsValid)
			{
				ValueHighInfo.AddError(ValidationError);
			}
			else if (!valueHigh.IsEmpty && valueHigh.CompareTo(valueLow) < 0)
			{
				ValueHighInfo.AddNotification(CargoWise.EntityFramework.NotificationType.Error, Res.GetString("f24dd59b-6b04-473a-86db-dee902b231d3", "The 'To date' must be after 'From date'"));
			}
		}

		#endregion

		#region Overrides

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ValueLow = (TZType)new TZType().Today;
			ValueHigh = (TZType)new TZType().Today;
		}

		public override void ClearValueForUnitTest()
		{
			ValueLow = (TZType)new TZType().Invalid;
			ValueHigh = (TZType)new TZType().Invalid;
		}
#endif

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			if (Scheduled)
			{
				ValidateScheduleValueLow();
				ValidateScheduleValueHigh();
				ValidateRequireBothFromAndToDates(ScheduleValueLow, ScheduleValueHigh);
			}
			else
			{
				ValidateRunValueLow();
				ValidateRunValueHigh();
				ValidateRequireBothFromAndToDates(RunValueLow, RunValueHigh);
			}
		}

		void ValidateRequireBothFromAndToDates(TZType valueLow, TZType valueHigh)
		{
			if (RequireBothFromAndToDates)
			{
				if (valueLow.IsEmpty != valueHigh.IsEmpty)
				{
					var message = Res.GetString("eca2d9ed-0674-45b5-971d-d191aa5e64ed", "Both from and to date must be entered.");

					ValueLowInfo.AddError(message);
					ValueHighInfo.AddError(message);
				}

				if (DateRangeMaxYears.HasValue && !valueLow.IsEmpty && !valueHigh.IsEmpty && GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(valueLow, valueHigh, DateRangeMaxYears.Value, 0))
				{
					var message = Res.GetString("47C9CF2D-3A67-4EAB-A7E8-BD51761D6AC3", "Date Range must be no more than {0} year(s).", DateRangeMaxYears);

					ValueLowInfo.AddError(message);
					ValueHighInfo.AddError(message);
				}

				if (DateRangeMaxMonths.HasValue && !valueLow.IsEmpty && !valueHigh.IsEmpty && GenericDateTypeHelper.IsRangeExceedingMaxTimeSpan(valueLow, valueHigh, 0, DateRangeMaxMonths.Value))
				{
					var message = Res.GetString("F50A4F3A-E5B5-4163-AE41-44F5C2AB8BB4", "Date Range must be no more than {0} month(s).", DateRangeMaxMonths);

					ValueLowInfo.AddError(message);
					ValueHighInfo.AddError(message);
				}
			}
		}

		public override object ValueAsObject
		{
			get
			{
				var minutesToAdd = (bool)(HighSchedule?.ByHourAndMinute ?? false) ? 0 : -1;
				var toDateString = ValueHigh.IsEmpty ? ValueHigh.ToString(DateFormat, null) : GenericDateTypeHelper.ToStringWithDateFormatFromDate(GenericDateTypeHelper.AddMinutesForDate(CalculateToDateAccordingDateFormat(), minutesToAdd), DateFormat, CultureInfo.InvariantCulture);
				return IsEmpty ? "" : Res.GetString("3eb816a9-3c15-4f52-beef-370851cf81d3", "From: {0} To: {1}", ValueLow.ToString(DateFormat, null), toDateString);
			}
		}

		public override bool IsEmpty
		{
			get { return !ValueLow.IsValid && !ValueHigh.IsValid; }
		}

		#endregion

		#region ValueProviders

		protected abstract TZType GetFromParamCore();

		protected abstract TZType GetToDateParamCore();

		#endregion

		#region Schedule

		public DateSchedule LowSchedule
		{
			get
			{
				if (Scheduled)
				{
					if (lowSchedule == null)
					{
						lowSchedule = new DateSchedule(scheduleTask);
						lowSchedule.PeriodScope = ZString.Empty;
						lowSchedule.ValueChanged += delegate { UpdateValueLowFromSchedule(); };
					}
					return lowSchedule;
				}
				return null;
			}
		}

		public DateSchedule HighSchedule
		{
			get
			{
				if (Scheduled)
				{
					if (highSchedule == null)
					{
						highSchedule = new DateSchedule(scheduleTask);
						highSchedule.PeriodScope = ZString.Empty;
						highSchedule.ValueChanged += delegate { UpdateValueHighFromSchedule(); };
					}
					return highSchedule;
				}
				return null;
			}
		}

		protected bool Scheduled
		{
			get { return (scheduleTask != null); }
		}

		protected void UpdateValueLowFromSchedule()
		{
			ScheduleValueLow = GenericDateTypeHelper.GetDateFromSchedule<TZType>(lowSchedule);
		}

		protected void UpdateValueHighFromSchedule()
		{
			ScheduleValueHigh = GenericDateTypeHelper.GetDateFromSchedule<TZType>(highSchedule);
		}

		protected DateSchedule lowSchedule;
		protected DateSchedule highSchedule;
		ReportScheduleTask scheduleTask;

		#endregion

		#region IDateFormatSupport Members

		public virtual DocEngineDatePickerFormats PickerFormat
		{
			get { return pickerFormat; }
			set { pickerFormat = value; }
		}

		DocEngineDatePickerFormats pickerFormat = DocEngineDatePickerFormats.Short;

		#endregion

		#region ISerializable Members

		void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("fName", fName);
			info.AddValue("FieldName", FieldName);
			info.AddValue("PickerFormat", pickerFormat.ToString());

			var store = new SchedulableStore<TBaseType>();
			if (RunValueHigh.IsValid && !RunValueHigh.IsEmpty)
			{
				store.RunValue = (TBaseType)RunValueHigh.ToBaseDate();
			}

			if (highSchedule != null && highSchedule.IsValid)
			{
				store.Schedule = highSchedule.ToStorageValue();
			}

			info.AddValue("ValueHigh", store);

			store = new SchedulableStore<TBaseType>();
			if (RunValueLow.IsValid && !RunValueLow.IsEmpty)
			{
				store.RunValue = (TBaseType)RunValueLow.ToBaseDate();
			}

			if (lowSchedule != null && lowSchedule.IsValid)
			{
				store.Schedule = lowSchedule.ToStorageValue();
			}

			info.AddValue("ValueLow", store);

			info.AddValue("ConvertToUtc", ConvertToUtc);
			info.AddValue("RequireBothFromAndToDates", RequireBothFromAndToDates);
			info.AddValue("SubstituteMinDateForNullFrom", SubstituteMinDateForNullFrom);
			info.AddValue("SubstituteMaxDateForNullTo", SubstituteMaxDateForNullTo);
		}

		#endregion

		#region ISchedulableFilterField Members

		public void SetScheduleTask(ReportScheduleTask value)
		{
			scheduleTask = value;

			if (lowSchedule != null)
			{
				LowSchedule.ScheduleTask = scheduleTask;
				UpdateValueLowFromSchedule();
			}

			if (highSchedule != null)
			{
				HighSchedule.ScheduleTask = scheduleTask;
				UpdateValueHighFromSchedule();
			}
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is BaseDateRangeField<TBaseType, TZType> dateRangeField)
			{
				if (dateRangeField.lowSchedule != null)
				{
					if (lowSchedule == null)
					{
						lowSchedule = new DateSchedule();
						lowSchedule.ValueChanged += delegate { UpdateValueLowFromSchedule(); };
					}

					lowSchedule.CopyChangesFrom(dateRangeField.lowSchedule);
					UpdateValueLowFromSchedule();
				}
				else
				{
					lowSchedule?.Clear();
				}

				RunValueLow = dateRangeField.RunValueLow;

				if (dateRangeField.highSchedule != null)
				{
					if (highSchedule == null)
					{
						highSchedule = new DateSchedule();
						highSchedule.ValueChanged += delegate { UpdateValueHighFromSchedule(); };
					}

					highSchedule.CopyChangesFrom(dateRangeField.highSchedule);
					UpdateValueHighFromSchedule();
				}
				else
				{
					highSchedule?.Clear();
				}

				RunValueHigh = dateRangeField.RunValueHigh;
			}
		}

		public override void ClearValues()
		{
			RunValueLow = default;
			RunValueHigh = default;
			lowSchedule?.Clear();
			highSchedule?.Clear();
			ScheduleValueLow = default;
			ScheduleValueHigh = default;
		}

		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = GetNewFilter();
			SetBaseFilterData(filterData);

			if (ValueHigh.IsValid && !ValueHigh.IsEmpty)
			{
				filterData.ValueHigh = (TBaseType)(Scheduled ? HighSchedule.ToStorageValue() : ValueHigh.ToBaseDate());
			}

			if (ValueLow.IsValid && !ValueLow.IsEmpty)
			{
				filterData.ValueLow = (TBaseType)(Scheduled ? LowSchedule.ToStorageValue() : ValueLow.ToBaseDate());
			}

			filterData.DateFormat = PickerFormat.ToString();
			AddFilterDataToFilterCollection(reportFilterData, filterData);
		}

		protected abstract BaseDateRangeFilter<TBaseType> GetNewFilter();

		protected abstract void AddFilterDataToFilterCollection(ReportFilterData reportFilterData, BaseFilterData filterData);

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = GetFirstFilterDataFromCollection(reportFilterData);
			if (selectedValue != null)
			{
				if (Scheduled)
				{
					if (DateSchedule.TryParse(GenericDateTypeHelper.ToDateTime(selectedValue.ValueHigh), out highSchedule))
					{
						highSchedule.ValueChanged += delegate { UpdateValueHighFromSchedule(); };
						highSchedule.ScheduleTask = scheduleTask;
						UpdateValueHighFromSchedule();
					}
					else
					{
						HighSchedule.Clear();
					}

					if (DateSchedule.TryParse(GenericDateTypeHelper.ToDateTime(selectedValue.ValueLow), out lowSchedule))
					{
						lowSchedule.ValueChanged += delegate { UpdateValueLowFromSchedule(); };
						lowSchedule.ScheduleTask = scheduleTask;
						UpdateValueLowFromSchedule();
					}
					else
					{
						LowSchedule.Clear();
					}
				}
				else
				{
					ValueHigh = (TZType)GenericDateTypeHelper.ToZDateTimeOrZDateTimeOffset(selectedValue.ValueHigh);
					ValueLow = (TZType)GenericDateTypeHelper.ToZDateTimeOrZDateTimeOffset(selectedValue.ValueLow);
				}
			}
		}

		protected abstract BaseDateRangeFilter<TBaseType> GetFirstFilterDataFromCollection(ReportFilterData reportFilterData);

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var filterData = CreateJsonDataCore();
			SetJsonData(filterData);
			filterData.DateFormat = PickerFormat.ToString();

			var store = new SchedulableStore<TBaseType>();
			if (RunValueLow.IsValid && !RunValueLow.IsEmpty)
			{
				store.RunValue = (TBaseType)RunValueLow.ToBaseDate();
			}

			if (lowSchedule != null && lowSchedule.IsValid)
			{
				store.Schedule = lowSchedule.ToStorageValue();
			}
			filterData.ValueLow = store;

			store = new SchedulableStore<TBaseType>();
			if (RunValueHigh.IsValid && !RunValueHigh.IsEmpty)
			{
				store.RunValue = (TBaseType)RunValueHigh.ToBaseDate();
			}

			if (highSchedule != null && highSchedule.IsValid)
			{
				store.Schedule = highSchedule.ToStorageValue();
			}
			filterData.ValueHigh = store;

			filterData.ConvertToUtc = ConvertToUtc;
			filterData.RequireBothFromAndToDates = RequireBothFromAndToDates;
			filterData.SubstituteMinDateForNullFrom = SubstituteMinDateForNullFrom;
			filterData.SubstituteMaxDateForNullTo = SubstituteMaxDateForNullTo;

			return filterData;
		}

		protected abstract BaseDateRangeFieldJsonData<TBaseType> CreateJsonDataCore();

		void SetDateRangeValue(BaseDateRangeFieldJsonData<TBaseType> dateRangeFilter)
		{
			if (dateRangeFilter.ValueHigh != null)
			{
				if (dateRangeFilter.ValueHigh.RunValue != null)
				{
					RunValueHigh = (TZType)ZDataType.ObjectToZType(dateRangeFilter.ValueHigh.RunValue);
				}

				if (dateRangeFilter.ValueHigh.Schedule != null && DateSchedule.TryParse((DateTime)dateRangeFilter.ValueHigh.Schedule, out highSchedule))
				{
					highSchedule.ValueChanged += delegate { UpdateValueHighFromSchedule(); };
				}
			}

			if (dateRangeFilter.ValueLow != null)
			{
				if (dateRangeFilter.ValueLow.RunValue != null)
				{
					RunValueLow = (TZType)ZDataType.ObjectToZType(dateRangeFilter.ValueLow.RunValue);
				}

				if (dateRangeFilter.ValueLow.Schedule != null && DateSchedule.TryParse((DateTime)dateRangeFilter.ValueLow.Schedule, out lowSchedule))
				{
					lowSchedule.ValueChanged += delegate { UpdateValueLowFromSchedule(); };
				}
			}
		}

		#endregion
	}
}
