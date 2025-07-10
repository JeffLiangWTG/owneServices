using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class DateRangeField : BaseDateRangeField<DateTime, ZDateTime>
	{
		public DateRangeField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		internal DateRangeField(DateRangeFieldJsonData data)
			: base(data)
		{
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.DateRangeFieldUserControl; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "There should be exact date format.")]
		protected override string DateFormat => PickerFormat == DocEngineDatePickerFormats.Long ? (NoResString)"dd-MMM-yy HH:mm" : "dd-MMM-yy";

		protected override void AddFilterDataToFilterCollection(ReportFilterData reportFilterData, BaseFilterData filterData)
		{
			if (filterData is DateRangeFilter valueAsDateRangeFilter)
			{
				reportFilterData.DateRangeFilterCollection.Add(valueAsDateRangeFilter);
			}
		}

		protected override BaseDateRangeFilter<DateTime> GetNewFilter()
		{
			return new DateRangeFilter();
		}

		protected override BaseDateRangeFilter<DateTime> GetFirstFilterDataFromCollection(ReportFilterData reportFilterData)
		{
			return reportFilterData.DateRangeFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
		}

		protected override ZDateTime GetFromParamCore()
		{
			var result = ValueLow;

			if (ValueLow.IsValid && !ValueLow.IsEmpty)
			{
				var dateTime = ValueLow.ToDateTime();
				result = dateTime;

				if (ConvertToUtc)
				{
					result = Env.Time.GetUtcFromLocalTime(dateTime);
				}
			}

			return result;
		}

		protected override ZDateTime GetToDateParamCore()
		{
			var result = ValueHigh;

			if (ValueHigh.IsValid && !ValueHigh.IsEmpty)
			{
				if (ConvertToUtc)
				{
					result = Env.Time.GetUtcFromLocalTime(CalculateToDateAccordingDateFormat());
				}
				else
				{
					result = CalculateToDateAccordingDateFormat();
				}
			}

			return result;
		}

		#region ValueProviders

		protected override void AddSpecialisedValueProviders()
		{
			// Use these macros to show entered dates in document
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".FromDate", new ReplacementProviderMethod(GetFromDateReplacement)));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToDate", new ReplacementProviderMethod(GetToDateReplacement)));

			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".FromDate.ToDateTimeOffset", GetDateTimeOffsetLow));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToDate.ToDateTimeOffset", GetDateTimeOffsetHigh));

			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".FromDateForSQLParameter", GetFromDateForSQLParameter));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToDateForSQLParameter", GetToDateForSQLParameter));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToNextDateForSQLParameter", GetToNextDateForSQLParameter));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".FromDateUtcForSQLParameter", GetFromDateUtcForSQLParameter));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToNextDateUtcForSQLParameter", GetToNextDateUtcForSQLParameter));

			// Use these macros for the stored procedure or function parameters. They will apply conversion to UTC if ConvertToUtc is set
			// Gives From value with conditional conversion to UTC as per above
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".FromDateSqlParam", new ReplacementProviderMethod(GetFromDateParamReplacement)));
			// This parameter is for < condition. It will give you 12AM of next day for date only. For date and time it will give entered value plus a minimum time slice (1 minute).
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToDateSqlParam", new ReplacementProviderMethod(GetToDateSqlParamReplacement)));

			// Obsolete macros
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToNextDate", new ReplacementProviderMethod(GetToNextDateReplacement)));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToEndDate", new ReplacementProviderMethod(GetToEndDateReplacement)));

			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".FromDateUtc", new ReplacementProviderMethod(GetFromDateUtcReplacement)));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToDateUtc", new ReplacementProviderMethod(GetToDateUtcReplacement)));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToNextDateUtc", new ReplacementProviderMethod(GetToNextDateUtcReplacement)));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToEndDateUtc", new ReplacementProviderMethod(GetToEndDateUtcReplacement)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.FromDate>", ResString.GetMultilingualString("fe2ad833-e5ab-49f0-b59f-353c37e894dd", "Returns the From Date value.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToDate>", ResString.GetMultilingualString("cfbd3388-de42-4146-a52d-b0a8c3363baf", "Returns the To Date value.")));

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.FromDate.ToDateTimeOffset>", ResString.GetMultilingualString("16f892f7-58be-40a7-a47c-21a126c3126b", "Returns the From Date value as date and time relative to UTC.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToDate.ToDateTimeOffset>", ResString.GetMultilingualString("d2dbea76-3e72-4acf-93b6-f9395f913fc4}", "Returns the To Date value as date and time relative to UTC.")));

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.FromDateForSQLParameter>", ResString.GetMultilingualString("5826fa9c-b133-481c-b1a6-15d40785668a", "Returns the selected From Date for use as an SQL parameter.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToDateForSQLParameter>", ResString.GetMultilingualString("db618cb8-bd25-49df-b75a-2f5149fcb86d", "Returns the selected To Date for use as an SQL parameter.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToNextDateForSQLParameter>", ResString.GetMultilingualString("9E95BE75-5ABA-4CFD-8574-806B1F1256C0", "Returns the the day following the To Date for use as an SQL parameter.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.FromDateUtcForSQLParameter>", ResString.GetMultilingualString("AC620DAC-6789-44EF-8BA8-61845473BF4C", "Returns the UTC equivalent of the local From Date value for use as an SQL parameter.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToNextDateUtcForSQLParameter>", ResString.GetMultilingualString("C9920E96-FF10-4E53-811D-A50EDD2AA391", "Returns the UTC equivalent of the day following the local To Date value for use as an SQL parameter.")));

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.FromDateSqlParam>", ResString.GetMultilingualString("7110bb7d-62e6-403b-b139-84b66e62309d", "Returns the local date corresponding to the From Date value or its UTC equivalent if the option {0} is set on the filter.", "ConvertToUTC")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToDateSqlParam>", ResString.GetMultilingualString("00d6690d-e04e-46ca-82ad-db865b77f26d", "Returns the local date corresponding to the To Date value or its UTC equivalent if the option {0} is set on the filter.", "ConvertToUTC")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToNextDate>", ResString.GetMultilingualString("e2bcc721-75d4-4196-913e-0cf5955e55c7", "Returns the day following the To Date value.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToEndDate>", ResString.GetMultilingualString("a5f8bb20-46d1-4f60-b837-009a440feb81", "Returns the To Date value of the selected date with the time component set to 23:59:59.")));

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.FromDateUtc>", ResString.GetMultilingualString("5f073c4d-8be7-42af-9a7a-be6d242bdca0", "Returns the UTC equivalent of the local From Date value.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToDateUtc>", ResString.GetMultilingualString("c9cd7bcd-b13d-47fe-b9cd-a3fc4bcbf5a6", "Returns the UTC equivalent of the local To Date value.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToNextDateUtc>", ResString.GetMultilingualString("38eaefff-16e3-4753-9827-8a64a3eda49f", "Returns the UTC equivalent of the day following the local To Date value.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToEndDateUtc>", ResString.GetMultilingualString("2afaebcb-2035-45f5-ac43-09a15930e0b9", "Returns the UTC equivalent of the local To Date value with the local time component set to 23:59:59.")));
		}

		protected object GetFromDateReplacement(string macro, Report report)
		{
			return SubstituteIfNecessaryMinDateForNullFrom(ValueLow);
		}

		protected object GetFromDateParamReplacement(string macro, Report report)
		{
			return SubstituteIfNecessaryMinDateForNullFrom(GetFromParamCore());
		}

		protected object GetFromDateForSQLParameter(string macro, Report report)
		{
			return new ReplacementWithSqlDbType(ValueLow.IsEmpty ? DBNull.Value : ValueLow.ToDateTime(), SqlDbType.DateTime);
		}

		protected object GetToDateForSQLParameter(string macro, Report report)
		{
			return new ReplacementWithSqlDbType(ValueHigh.IsEmpty ? DBNull.Value : ValueHigh.ToDateTime(), SqlDbType.DateTime);
		}

		ZDateTime SubstituteIfNecessaryMinDateForNullFrom(ZDateTime from)
		{
			if (SubstituteMinDateForNullFrom && (!from.IsValid || from.IsEmpty))
			{
				return ZDateTime.MinSmallDateTimeValue;
			}
			else
			{
				return from;
			}
		}

		protected object GetFromDateUtcReplacement(string macro, Report report)
		{
			var result = ValueLow;

			if (ValueLow.IsValid && !ValueLow.IsEmpty)
			{
				result = new ZDateTime(Env.Time.GetUtcFromLocalTime(ValueLow.ToDateTime()));
			}

			return SubstituteIfNecessaryMinDateForNullFrom(result);
		}

		protected object GetFromDateUtcForSQLParameter(string macro, Report report)
		{
			var result = (ZDateTime)GetFromDateUtcReplacement(macro, report);
			return new ReplacementWithSqlDbType(result.IsEmpty ? DBNull.Value : result.ToDateTime(), SqlDbType.DateTime);
		}

		protected object GetToDateReplacement(string macro, Report report)
		{
			return SubstituteIfNecessaryMaxDateForNullTo(ValueHigh);
		}

		protected object GetToDateSqlParamReplacement(string macro, Report report)
		{
			return SubstituteIfNecessaryMaxDateForNullTo(GetToDateParamCore());
		}

		protected object GetToNextDateForSQLParameter(string macro, Report report)
		{
			return new ReplacementWithSqlDbType(ValueHigh.IsEmpty ? DBNull.Value : ValueHigh.AddDays(1).ToDateTime(), SqlDbType.DateTime);
		}

		protected object GetDateTimeOffsetLow(string macro, Report report)
		{
			if (ValueLow.IsValid && !ValueLow.IsEmpty)
			{
				return new ZDateTimeOffset(ValueLow).ToDateTimeOffset();
			}
			else if (SubstituteMinDateForNullFrom)
			{
				return new ZDateTimeOffset(ZDateTime.MinSmallDateTimeValue).ToDateTimeOffset();
			}

			return ValueLow;
		}

		protected object GetDateTimeOffsetHigh(string macro, Report report)
		{
			if (ValueHigh.IsValid && !ValueHigh.IsEmpty)
			{
				return new ZDateTimeOffset(ValueHigh).ToDateTimeOffset();
			}
			else if (SubstituteMaxDateForNullTo)
			{
				return new ZDateTimeOffset(ZDateTime.MaxSmallDateTimeValue).ToDateTimeOffset();
			}

			return ValueHigh;
		}

		ZDateTime SubstituteIfNecessaryMaxDateForNullTo(ZDateTime to)
		{
			if (SubstituteMaxDateForNullTo && (!to.IsValid || to.IsEmpty))
			{
				return ZDateTime.MaxSmallDateTimeValue;
			}
			else
			{
				return to;
			}
		}

		protected object GetToDateUtcReplacement(string macro, Report report)
		{
			var result = ValueHigh;

			if (ValueHigh.IsValid && !ValueHigh.IsEmpty)
			{
				result = new ZDateTime(Env.Time.GetUtcFromLocalTime(ValueHigh.ToDateTime()));
			}

			return SubstituteIfNecessaryMaxDateForNullTo(result);
		}

		protected object GetToNextDateReplacement(string macro, Report report)
		{
			var result = ValueHigh;

			if (ValueHigh.IsValid && !ValueHigh.IsEmpty)
			{
				result = ValueHigh.AddDays(1);
			}

			return SubstituteIfNecessaryMaxDateForNullTo(result);
		}

		protected object GetToNextDateUtcReplacement(string macro, Report report)
		{
			var result = ValueHigh;

			if (ValueHigh.IsValid && !ValueHigh.IsEmpty)
			{
				result = new ZDateTime(Env.Time.GetUtcFromLocalTime(ValueHigh.AddDays(1).ToDateTime()));
			}

			return SubstituteIfNecessaryMaxDateForNullTo(result);
		}

		protected object GetToNextDateUtcForSQLParameter(string macro, Report report)
		{
			var result = (ZDateTime)GetToNextDateUtcReplacement(macro, report);
			return new ReplacementWithSqlDbType(result.IsEmpty ? DBNull.Value : result.ToDateTime(), SqlDbType.DateTime);
		}

		protected object GetToEndDateReplacement(string macro, Report report)
		{
			var result = ValueHigh;

			if (ValueHigh.IsValid && !ValueHigh.IsEmpty)
			{
				result = ValueHigh.AddDays(1).AddSeconds(-1);
			}

			return SubstituteIfNecessaryMaxDateForNullTo(result);
		}

		protected object GetToEndDateUtcReplacement(string macro, Report report)
		{
			var result = ValueHigh;

			if (ValueHigh.IsValid && !ValueHigh.IsEmpty)
			{
				result = new ZDateTime(Env.Time.GetUtcFromLocalTime(ValueHigh.AddDays(1).AddSeconds(-1).ToDateTime()));
			}

			return SubstituteIfNecessaryMaxDateForNullTo(result);
		}

		#endregion

		protected override BaseDateRangeFieldJsonData<DateTime> CreateJsonDataCore() => new DateRangeFieldJsonData();

		protected override DateTime GetLowerDate(ZDateTime value)
		{
			var lowerDate = (DateTime)((IZTypeInternals)value).GetValueForLogicalDataLayer(true);
			if (PickerFormat == DocEngineDatePickerFormats.Short)
			{
				lowerDate = lowerDate.Date;
			}
			else if (PickerFormat == DocEngineDatePickerFormats.YearAndMonth)
			{
				lowerDate = new DateTime(lowerDate.Year, lowerDate.Month, 1);
			}
			return lowerDate;
		}

		protected override DateTime GetUpperDate(ZDateTime value)
		{
			var upperDate = ((DateTime)((IZTypeInternals)value).GetValueForLogicalDataLayer(true)).AddDays(1);
			if (PickerFormat == DocEngineDatePickerFormats.Short)
			{
				upperDate = upperDate.Date;
			}
			else if (PickerFormat == DocEngineDatePickerFormats.YearAndMonth)
			{
				upperDate = new DateTime(upperDate.Year, upperDate.Month, 1).AddMonths(1);
			}
			return upperDate;
		}
	}
}
