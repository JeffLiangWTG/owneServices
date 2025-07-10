using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class DateTimeOffsetRangeField : BaseDateRangeField<DateTimeOffset, ZDateTimeOffset>
	{
		public DateTimeOffsetRangeField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Constructor For IJsonSerializable

		internal DateTimeOffsetRangeField(DateTimeOffsetRangeFieldJsonData data)
			: base(data)
		{
		}

		#endregion

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.DateTimeOffsetRangeFieldUserControl; }
		}

		protected override string DateFormat => (NoResString)"dd-MMM-yy HH:mm zzz";

		protected override void AddFilterDataToFilterCollection(ReportFilterData reportFilterData, BaseFilterData filterData)
		{
			if (filterData is DateTimeOffsetRangeFilter valueAsDateRangeFilter)
			{
				reportFilterData.DateTimeOffsetRangeFilterCollection.Add(valueAsDateRangeFilter);
			}
		}

		protected override BaseDateRangeFilter<DateTimeOffset> GetNewFilter()
		{
			return new DateTimeOffsetRangeFilter();
		}

		protected override BaseDateRangeFilter<DateTimeOffset> GetFirstFilterDataFromCollection(ReportFilterData reportFilterData)
		{
			return reportFilterData.DateTimeOffsetRangeFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
		}

		protected override ZDateTimeOffset GetFromParamCore()
		{
			var result = ValueLow;

			if (ValueLow.IsValid && !ValueLow.IsEmpty)
			{
				var dateTime = ValueLow.ToDateTimeOffset();
				result = dateTime;

				if (ConvertToUtc)
				{
					result = result.ToDateTimeOffset().ToUniversalTime();
				}
			}

			return result;
		}

		protected override ZDateTimeOffset GetToDateParamCore()
		{
			var result = ValueHigh;

			if (ValueHigh.IsValid && !ValueHigh.IsEmpty)
			{
				if (ConvertToUtc)
				{
					result = CalculateToDateAccordingDateFormat().ToUniversalTime();
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

			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".FromDateForSQLParameter", GetFromDateForSQLParameter));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ToDateForSQLParameter", GetToDateForSQLParameter));

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

			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.FromDateForSQLParameter>", ResString.GetMultilingualString("5826fa9c-b133-481c-b1a6-15d40785668a", "Returns the selected From Date for use as an SQL parameter.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToDateForSQLParameter>", ResString.GetMultilingualString("db618cb8-bd25-49df-b75a-2f5149fcb86d", "Returns the selected To Date for use as an SQL parameter.")));

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
			return new ReplacementWithSqlDbType(ValueLow.IsEmpty ? DBNull.Value : ValueLow.ToDateTimeOffset(), SqlDbType.DateTimeOffset);
		}

		protected object GetToDateForSQLParameter(string macro, Report report)
		{
			return new ReplacementWithSqlDbType(ValueHigh.IsEmpty ? DBNull.Value : ValueHigh.ToDateTimeOffset(), SqlDbType.DateTimeOffset);
		}

		ZDateTimeOffset SubstituteIfNecessaryMinDateForNullFrom(ZDateTimeOffset from)
		{
			if (SubstituteMinDateForNullFrom && (!from.IsValid || from.IsEmpty))
			{
				return DateTimeOffset.MinValue;
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
				result = new ZDateTimeOffset(ValueLow.ToDateTimeOffsetSafe()?.ToUniversalTime());
			}

			return SubstituteIfNecessaryMinDateForNullFrom(result);
		}

		protected object GetToDateReplacement(string macro, Report report)
		{
			return SubstituteIfNecessaryMaxDateForNullTo(ValueHigh);
		}

		protected object GetToDateSqlParamReplacement(string macro, Report report)
		{
			return SubstituteIfNecessaryMaxDateForNullTo(GetToDateParamCore());
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

		ZDateTimeOffset SubstituteIfNecessaryMaxDateForNullTo(ZDateTimeOffset to)
		{
			if (SubstituteMaxDateForNullTo && (!to.IsValid || to.IsEmpty))
			{
				return DateTimeOffset.MaxValue;
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
				result = new ZDateTimeOffset(ValueHigh.ToDateTimeOffsetSafe()?.ToUniversalTime());
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
				result = new ZDateTimeOffset(ValueHigh.AddDays(1).ToDateTimeOffsetSafe()?.ToUniversalTime());
			}

			return SubstituteIfNecessaryMaxDateForNullTo(result);
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
				result = new ZDateTimeOffset(ValueHigh.AddDays(1).AddSeconds(-1).ToDateTimeOffsetSafe()?.ToUniversalTime());
			}

			return SubstituteIfNecessaryMaxDateForNullTo(result);
		}

		#endregion

		protected override BaseDateRangeFieldJsonData<DateTimeOffset> CreateJsonDataCore() => new DateTimeOffsetRangeFieldJsonData();

		protected override DateTimeOffset GetLowerDate(ZDateTimeOffset value)
		{
			var lowerDate = (DateTimeOffset)((IZTypeInternals)value).GetValueForLogicalDataLayer(true);
			return lowerDate;
		}

		protected override DateTimeOffset GetUpperDate(ZDateTimeOffset value)
		{
			var upperDate = ((DateTimeOffset)((IZTypeInternals)value).GetValueForLogicalDataLayer(true)).AddDays(1);
			return upperDate;
		}
	}
}
