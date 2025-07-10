using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class BaseDateField<TBaseType, TZType> : FilterFieldValueSerialisable, IDateFormatSupport, ISchedulableFilterField, IJsonSerializable where TBaseType : struct where TZType : IZDate, new()
	{
		protected BaseDateField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		internal BaseDateField(BaseDateFieldJsonData<TBaseType> data)
			: base(data)
		{
			CreateParameters();

			Enum.TryParse(data.DateFormat, out pickerFormat);

			SetDateTimeValue(data.Value);
		}

		#endregion

		public ZDateTimePickerFormat ZPickerFormat
		{
			get { return pickerFormat == DocEngineDatePickerFormats.Long ? ZDateTimePickerFormat.Long : ZDateTimePickerFormat.Short; }
		}

		protected SqlParameter LowerParam
		{
			get { return lowerParam; }
		}

		protected SqlParameter UpperParam
		{
			get { return upperParam; }
		}

		protected void CreateParameters()
		{
			lowerParam = new SqlParameter(SqlParameterNameGenerator.Next(), DBNull.Value);
			upperParam = new SqlParameter(SqlParameterNameGenerator.Next(), DBNull.Value);
		}

		SqlParameter lowerParam;
		SqlParameter upperParam;

		#region Value

		[ReadOnlyMember(nameof(Scheduled))]
		public TZType Value
		{
			get { return Scheduled ? ScheduleValue : RunValue; }
			set
			{
				if (Scheduled)
				{
					ScheduleValue = value;
				}
				else
				{
					RunValue = value;
				}
			}
		}

		TZType runLowerValue;
		TZType runUpperValue;

		protected TZType RunValue
		{
			get { return runLowerValue; }
			set
			{
				if (!value.Equals(RunValue))
				{
					SetLowerAndUpperValues(value, out runLowerValue, out runUpperValue);

					SetHasChangesBecauseTheValueHasBeenSetByTheUser();
					if (!IsValidationSuspended)
					{
						ValidateValue(RunValue);
					}
					ValueInfo.RefreshBinding();
				}
			}
		}

		TZType scheduleLowerValue;
		TZType scheduleUpperValue;

		protected TZType ScheduleValue
		{
			get { return scheduleLowerValue; }
			set
			{
				if (!value.Equals(ScheduleValue))
				{
					SetLowerAndUpperValues(value, out scheduleLowerValue, out scheduleUpperValue);

					SetHasChangesBecauseTheValueHasBeenSetByTheUser();
					if (!IsValidationSuspended)
					{
						ValidateValue(ScheduleValue);
					}
					ValueInfo.RefreshBinding();
				}
			}
		}

		protected abstract void SetLowerAndUpperValues(TZType value, out TZType lower, out TZType upper);

		public void ValidateValue()
		{
			ValidateValue(Value);
		}

		protected void ValidateValue(TZType value)
		{
			ValueInfo.ClearAllNotifications();
			if (!IsValid)
			{
				ValueInfo.AddError(ValidationError);
			}
			else if (!value.IsEmpty && !value.IsValid)
			{
				ValueInfo.AddError(Res.GetString("9887f9c5-2db4-4f1a-94ec-6953184cb89e", "{0} is an invalid date!", DisplayName));
			}
		}

		public ZPropertyInfo ValueInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(Value));
				result.HumanReadableName = DisplayName;
				return result;
			}
		}

		#endregion

		#region Schedule

		public DateSchedule Schedule
		{
			get
			{
				if (Scheduled)
				{
					if (schedule == null)
					{
						schedule = new DateSchedule();
						schedule.ScheduleTask = scheduleTask;
						schedule.ValueChanged += delegate
						{ UpdateValueFromSchedule(); };
					}
					return schedule;
				}
				return null;
			}
		}

		protected bool Scheduled
		{
			get { return (scheduleTask != null); }
		}

		protected void UpdateValueFromSchedule()
		{
			ScheduleValue = GenericDateTypeHelper.GetDateFromSchedule<TZType>(schedule);
		}

		protected DateSchedule schedule;
		ReportScheduleTask scheduleTask;

		#endregion

		#region Overrides

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			Value = (TZType)new TZType().Today;
		}

		public override void ClearValueForUnitTest()
		{
			Value = default;
		}
#endif

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateValue(Value);
		}

		public override object ValueAsObject
		{
			get { return Value; }
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			return FieldName + " >= " + lowerParam + " AND " + FieldName + " < " + upperParam;
		}

		protected override SqlParameterList GetSqlParametersCore()
		{
			var result = new SqlParameterList();
			if (IsEmpty)
			{
				return result;
			}

			SetLowerSQLParameterValue();
			SetUpperSQLParameterValue();

			result.Add(lowerParam);
			result.Add(upperParam);
			return result;
		}

		protected void SetLowerSQLParameterValue()
		{
			if (Scheduled)
			{
				if (scheduleLowerValue.IsEmpty || !scheduleLowerValue.IsValid)
				{
					lowerParam.Value = DBNull.Value;
				}
				else
				{
					lowerParam.Value = scheduleLowerValue.ToBaseDate();
				}
			}
			else
			{
				if (runLowerValue.IsEmpty || !runLowerValue.IsValid)
				{
					lowerParam.Value = DBNull.Value;
				}
				else
				{
					lowerParam.Value = runLowerValue.ToBaseDate();
				}
			}
		}

		protected void SetUpperSQLParameterValue()
		{
			if (Scheduled)
			{
				if (scheduleUpperValue.IsEmpty || !scheduleUpperValue.IsValid)
				{
					upperParam.Value = DBNull.Value;
				}
				else
				{
					upperParam.Value = scheduleUpperValue.ToBaseDate();
				}
			}
			else
			{
				if (runUpperValue.IsEmpty || !runUpperValue.IsValid)
				{
					upperParam.Value = DBNull.Value;
				}
				else
				{
					upperParam.Value = runUpperValue.ToBaseDate();
				}
			}
		}

		public override bool IsEmpty
		{
			get { return Value.IsEmpty; }
		}

		#endregion

		#region ValueProviders

		protected override void AddSpecialisedValueProviders()
		{
			base.AddSpecialisedValueProviders();
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".SqlFormat", GetSqlFormat));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".EndOfDate", GetEndOfDate));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ToNextDate", GetToNextDate));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ValueForSQLParameter", GetValueForSQLParameter));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ToNextDateForSQLParameter", GetToNextDateForSQLParameter));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.SqlFormat>", ResString.GetMultilingualString("84768b64-8f46-4623-bdcf-374b55440365", "Returns the SQL formatted value of the selected date.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.EndOfDate>", ResString.GetMultilingualString("fd4f94a5-fba0-4287-8b96-9def8f913b32", "Returns the selected date with the time component set to 23:59:59.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToNextDate>", ResString.GetMultilingualString("a51b7e52-3a86-4c0a-8c9e-4ad0ae684d3f", "Returns the selected date plus one day.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ValueForSQLParameter>", ResString.GetMultilingualString("23d4e4de-4a29-42e8-82cc-758047aec09a", "Returns the selected date for use as an SQL parameter.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToNextDateForSQLParameter>", ResString.GetMultilingualString("1F96BD4D-CCF1-4302-8A4E-CBD7AB119176", "Returns the selected date plus one day for use as an SQL parameter.")));
		}

		object GetEndOfDate(string macro, Report report)
		{
			return Value.EndOfDay();
		}

		object GetSqlFormat(string macro, Report report)
		{
			return Value.IsEmpty ? ZString.Empty : Value.SqlFormat;
		}

		object GetToNextDate(string macro, Report report)
		{
			if (Value.IsValid && !Value.IsEmpty)
			{
				return Value.AddDays(1);
			}

			return Value;
		}

		object GetValueForSQLParameter(string macro, Report report)
		{
			return new ReplacementWithSqlDbType(Value.IsEmpty ? DBNull.Value : Value.ToBaseDate(), GenericDateTypeHelper.GetSqlDbType<TBaseType>());
		}

		object GetToNextDateForSQLParameter(string macro, Report report)
		{
			return new ReplacementWithSqlDbType(Value.IsEmpty ? DBNull.Value : Value.AddDays(1).ToBaseDate(), GenericDateTypeHelper.GetSqlDbType<TBaseType>());
		}

		#endregion

		#region IDateFormatSupport Members

		public virtual DocEngineDatePickerFormats PickerFormat
		{
			get { return pickerFormat; }
			set { pickerFormat = value; }
		}

		DocEngineDatePickerFormats pickerFormat = DocEngineDatePickerFormats.Short;

		#endregion

		#region ISchedulableFilterField Members

		public void SetScheduleTask(ReportScheduleTask value)
		{
			scheduleTask = value;
			if (schedule != null)
			{
				Schedule.ScheduleTask = scheduleTask;
				UpdateValueFromSchedule();
			}
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is BaseDateField<TBaseType, TZType> dateField)
			{
				if (dateField.schedule != null)
				{
					if (schedule == null)
					{
						schedule = new DateSchedule();
						schedule.ValueChanged += delegate
						{ UpdateValueFromSchedule(); };
					}

					schedule.CopyChangesFrom(dateField.schedule);
					UpdateValueFromSchedule();
				}
				else
				{
					schedule?.Clear();
				}

				this.RunValue = dateField.RunValue;
			}
		}

		public override void ClearValues()
		{
			RunValue = default;
			schedule?.Clear();
			ScheduleValue = default;
		}

		#endregion

		protected override string ValueAsStringForSerialisationInternal
		{
			get { return Value.ToISO8601String(); }
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					Value = default;
				}
				else
				{
					if (!new TZType().TryParse(value, out var newValue))
					{
						try
						{
							newValue = GenericDateTypeHelper.ToZDateTimeOrZDateTimeOffset(GenericDateTypeHelper.ParseBaseDate<TBaseType>(value, System.Globalization.CultureInfo.CurrentCulture)); // Yes, this is (was) dumb because it wont work in other countries, but is here for backwards compatibility with UDF
						}
						catch (FormatException)
						{
							//if wrong format, do nothing.
						}
					}
					Value = (TZType)newValue;
				}
			}
		}

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = GetNewFilter();
			SetBaseFilterData(filterData);

			if (Value.IsValid && !Value.IsEmpty)
			{
				filterData.Value = (TBaseType)(Scheduled ? Schedule.ToStorageValue() : Value.ToBaseDate());
			}

			filterData.DateFormat = PickerFormat.ToString();
			AddFilterDataToFilterCollection(reportFilterData, filterData);
		}

		protected abstract BaseDateFilter<TBaseType> GetNewFilter();

		protected abstract void AddFilterDataToFilterCollection(ReportFilterData reportFilterData, BaseFilterData filterData);

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = GetFirstFilterDataFromCollection(reportFilterData);
			if (selectedValue != null)
			{
				if (DateTime.MinValue.Equals(selectedValue.Value) || DateTimeOffset.MinValue.Equals(selectedValue.Value))
				{
					Value = default;
				}
				else
				{
					if (Scheduled)
					{
						if (DateSchedule.TryParse(GenericDateTypeHelper.ToDateTime(selectedValue.Value), out schedule))
						{
							schedule.ValueChanged += delegate { UpdateValueFromSchedule(); };
							schedule.ScheduleTask = scheduleTask;
							UpdateValueFromSchedule();
						}
						else
						{
							Schedule.Clear();
						}
					}
					else
					{
						Value = (TZType)GenericDateTypeHelper.ToZDateTimeOrZDateTimeOffset(selectedValue.Value);
					}
				}
			}
		}

		protected abstract BaseDateFilter<TBaseType> GetFirstFilterDataFromCollection(ReportFilterData reportFilterData);

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var filterData = CreateJsonDataCore();
			SetJsonData(filterData);
			filterData.DateFormat = PickerFormat.ToString();

			var store = new SchedulableStore<TBaseType>();
			if (RunValue.IsValid && !RunValue.IsEmpty)
			{
				store.RunValue = (TBaseType)RunValue.ToBaseDate();
			}

			if (schedule != null && schedule.IsValid)
			{
				store.Schedule = schedule.ToStorageValue();
			}

			filterData.Value = store;
			return filterData;
		}

		protected abstract BaseDateFieldJsonData<TBaseType> CreateJsonDataCore();

		void SetDateTimeValue(SchedulableStore<TBaseType> store)
		{
			if (store != null)
			{
				if (store.RunValue != null)
				{
					RunValue = (TZType)GenericDateTypeHelper.ToZDateTimeOrZDateTimeOffset(store.RunValue);
				}
				else
				{
					RunValue = default;
				}
				if (store.Schedule != null)
				{
					if (DateSchedule.TryParse(store.Schedule.Value, out schedule))
					{
						schedule.ValueChanged += delegate
						{ UpdateValueFromSchedule(); };
					}
				}
			}
		}

		#endregion
	}
}
