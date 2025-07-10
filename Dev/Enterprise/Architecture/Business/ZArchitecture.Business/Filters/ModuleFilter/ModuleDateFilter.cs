using System;
using System.Xml;
using CargoWise.Application;
using CargoWise.CalendarArithmetic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public delegate ZQuery GetDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2);

	public class ModuleDateFilter : ModuleFilterWithSubDescriptions
	{
		#region Constants

		public static class DateRangeSearchTexts
		{
			#region SuppressResourceStringsCheckRegion

			public static ZString Today => TodayMultilingual.GetUnresolvedString();
			internal static MultilingualString TodayMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Today", "Today");

			public static ZString ThisWeek => ThisWeekMultilingual.GetUnresolvedString();
			internal static MultilingualString ThisWeekMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|ThisWeek", "This Week");

			public static ZString Yesterday => YesterdayMultilingual.GetUnresolvedString();
			internal static MultilingualString YesterdayMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Yesterday", "Yesterday");

			public static ZString LastWeek => LastWeekMultilingual.GetUnresolvedString();
			internal static MultilingualString LastWeekMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|LastWeek", "Last Week");

			public static ZString Last7Days => Last7DaysMultilingual.GetUnresolvedString();
			internal static MultilingualString Last7DaysMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Last7Days", "Last 7 Days");

			public static ZString Last14Days => Last14DaysMultilingual.GetUnresolvedString();
			internal static MultilingualString Last14DaysMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Last14Days", "Last 14 Days");

			public static ZString LastMonth => LastMonthMultilingual.GetUnresolvedString();
			internal static MultilingualString LastMonthMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|LastMonth", "Last Month");

			public static ZString LastCalendarMonth => LastCalendarMonthMultilingual.GetUnresolvedString();
			internal static MultilingualString LastCalendarMonthMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|LastCalendarMonth", "Last Calendar Month");

			public static ZString Last2Mths => Last2MthsMultilingual.GetUnresolvedString();
			internal static MultilingualString Last2MthsMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Last2Mths", "Last 2 Mths.");

			public static ZString Last3Mths => Last3MthsMultilingual.GetUnresolvedString();
			internal static MultilingualString Last3MthsMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Last3Mths", "Last 3 Mths.");

			public static ZString Last6Mths => Last6MthsMultilingual.GetUnresolvedString();
			internal static MultilingualString Last6MthsMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Last6Mths", "Last 6 Mths.");

			public static ZString Last12Mths => Last12MthsMultilingual.GetUnresolvedString();
			internal static MultilingualString Last12MthsMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Last12Mths", "Last 12 Mths.");

			public static ZString Tomorrow => TomorrowMultilingual.GetUnresolvedString();
			internal static MultilingualString TomorrowMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Tomorrow", "Tomorrow");

			public static ZString NextWeek => NextWeekMultilingual.GetUnresolvedString();
			internal static MultilingualString NextWeekMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|NextWeek", "Next Week");

			public static ZString Next7Days => Next7DaysMultilingual.GetUnresolvedString();
			internal static MultilingualString Next7DaysMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Next7Days", "Next 7 Days");

			public static ZString Next14Days => Next14DaysMultilingual.GetUnresolvedString();
			internal static MultilingualString Next14DaysMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Next14Days", "Next 14 Days");

			public static ZString NextMonth => NextMonthMultilingual.GetUnresolvedString();
			internal static MultilingualString NextMonthMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|NextMonth", "Next Month");

			public static ZString NextCalendarMonth => NextCalendarMonthMultilingual.GetUnresolvedString();
			internal static MultilingualString NextCalendarMonthMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|NextCalendarMonth", "Next Calendar Month");

			public static ZString Next2Mths => Next2MthsMultilingual.GetUnresolvedString();
			internal static MultilingualString Next2MthsMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Next2Mths", "Next 2 Mths.");

			public static ZString Next3Mths => Next3MthsMultilingual.GetUnresolvedString();
			internal static MultilingualString Next3MthsMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Next3Mths", "Next 3 Mths.");

			public static ZString Next6Mths => Next6MthsMultilingual.GetUnresolvedString();
			internal static MultilingualString Next6MthsMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Next6Mths", "Next 6 Mths.");

			public static ZString Next12Mths => Next12MthsMultilingual.GetUnresolvedString();
			internal static MultilingualString Next12MthsMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Next12Mths", "Next 12 Mths.");

			#endregion
		}

		#endregion

		#region Construction

		protected bool isNullable = true;

		protected ModuleDateFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: this(category, parentCollection, false)
		{
		}

		public ModuleDateFilter(ZString description, SchemaDateTimeColumn filterColumn)
			: this(description, filterColumn, false)
		{
		}

		public ModuleDateFilter(ZString description, GetDateQuery queryDelegate, bool isNullable = true)
			: this(description, queryDelegate, false, isNullable)
		{
		}

		protected ModuleDateFilter(FilterCategory category, ModuleFilterCollection parentCollection, bool convertFromLocalToUTC)
			: base(category, parentCollection)
		{
			ConvertFromLocalToUTC = convertFromLocalToUTC;
		}

		protected ModuleDateFilter(ZString description, bool convertFromLocalToUTC = false, bool isNullable = true)
			: base(description)
		{
			ConvertFromLocalToUTC = convertFromLocalToUTC;
			this.isNullable = isNullable;
		}

		public ModuleDateFilter(ZString description, SchemaDateTimeColumn filterColumn, bool convertFromLocalToUTC)
			: base(description, filterColumn)
		{
			ConvertFromLocalToUTC = convertFromLocalToUTC;
			isNullable = filterColumn.IsNullable;
		}

		public ModuleDateFilter(ZString description, GetDateQuery queryDelegate, bool convertFromLocalToUTC, bool isNullable = true)
			: base(description, queryDelegate)
		{
			ConvertFromLocalToUTC = convertFromLocalToUTC;
			this.isNullable = isNullable;
		}

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			FilterOption = DateOffsetRangeFilterOptions.Codes.Past;
		}

		#endregion

		#region GetNewCommonModuleFilter, CopyPropertiesToFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleDateFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleDateFilter)filterToCopyFrom;

			PropertySearch = filter.PropertySearch;
			Property1 = filter.Property1;
			Property2 = filter.Property2;
			PropertyDecimal1 = filter.PropertyDecimal1;
			PropertyDecimal2 = filter.PropertyDecimal2;
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory => FilterCategories.Dates;

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			PropertySearch = "";
			Property1 = ZDateTime.Empty;
			Property2 = ZDateTime.Empty;
			PropertyDecimal1 = 0;
			PropertyDecimal2 = 0;
		}

		protected override bool IsEmptyCore => !IsPropertySearchValid ||
					(Property1.IsEmpty && Property2.IsEmpty && (IsPropertySearchUsingSpecifiedDateRange || IsPropertySearchUsingSpecifiedDateTimeRange));

		#endregion

		#region PropertySearch

		[BusinessObjectTestExclude] // don't want the max length
		public ZString PropertySearch
		{
			get { return fPropertySearch; }
			set
			{
				var oldValue = PropertySearch;

				fPropertySearch = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePropertySearch();
				}
				PropertySearchInfo.RefreshBinding();

				if (value != oldValue)
				{
					if (oldValue != ZString.Empty)
					{
						if (IsSearchByOffsetInHours(value) && !IsSearchByOffsetInHours(oldValue) ||
							IsSearchByOffsetInHours(oldValue) && !IsSearchByOffsetInHours(value) ||
							IsSearchByOffsetInDays(value))
						{
							Property1 = ZDateTime.Empty;
							Property2 = ZDateTime.Empty;
						}
					}

					InvalidateCachedQuery();
				}
			}
		}

		bool IsSearchByOffsetInHours(string value) => value == SpecifiedHourOffsetRange || value == SpecifiedWorkHourOffsetRange;

		bool IsSearchByOffsetInDays(string value) => value == SpecifiedDayOffsetRange;

		public ZPropertyInfo PropertySearchInfo => GetZPropertyInfo(nameof(PropertySearch));

		public bool IsPropertySearchUsingSpecifiedDateRange => PropertySearch.EqualsIgnoringCase(SpecifiedDateRange);

		public bool IsPropertySearchUsingSpecifiedDateTimeRange => PropertySearch.EqualsIgnoringCase(SpecifiedDateTimeRange);

		public bool IsPropertySearchUsingSpecifiedHourOffsetRange => PropertySearch.EqualsIgnoringCase(SpecifiedHourOffsetRange);

		public bool IsPropertySearchUsingSpecifiedWorkHourOffsetRange => PropertySearch.EqualsIgnoringCase(SpecifiedWorkHourOffsetRange);

		public bool IsPropertySearchUsingSpecifiedDayOffsetRange => PropertySearch.EqualsIgnoringCase(SpecifiedDayOffsetRange);

		public bool IsPropertySearchUsingHasNoDateEntered => PropertySearch.EqualsIgnoringCase(HasNoDateEntered);

		public bool IsPropertySearchUsingHasDateEntered => PropertySearch.EqualsIgnoringCase(HasDateEntered);

		public bool IsPropertySearchUsingPastDate => PropertySearch.EqualsIgnoringCase(Past);

		public bool IsPropertySearchUsingFutureDate => PropertySearch.EqualsIgnoringCase(Future);

		public bool IsPropertySearchValid => IsPropertySearchUsingSpecifiedDateRange
					|| IsPropertySearchUsingSpecifiedDateTimeRange
					|| IsPropertySearchUsingSpecifiedHourOffsetRange
					|| IsPropertySearchUsingSpecifiedWorkHourOffsetRange
					|| IsPropertySearchUsingSpecifiedDayOffsetRange
					|| IsPropertySearchUsingHasDateEntered
					|| IsPropertySearchUsingHasNoDateEntered
					|| IsPropertySearchUsingPastDate
					|| IsPropertySearchUsingFutureDate
					|| (PropertySearch_List.ContainsCode(PropertySearch) && PropertySearch_List[PropertySearch] is DateRangePair);

		public CodeDescriptionPairList PropertySearch_List => PropertySearch_ListCore;

		DateRangePairList PropertySearch_ListCore
		{
			get
			{
				if (fPropertySearch_ListCore == null)
				{
					fPropertySearch_ListCore = CreatePropertySearch_ListCore();
				}

				return fPropertySearch_ListCore;
			}
		}

		protected virtual DateRangePairList CreatePropertySearch_ListCore()
		{
			var propertySearch_ListCore = new DateRangePairList();

			propertySearch_ListCore.AddPair("");
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.TodayMultilingual, "", 0);
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.ThisWeekMultilingual, DateRangeSpan.CurrentWeek);

			propertySearch_ListCore.AddPair("");
			propertySearch_ListCore.Add(new CategoryCodeDescriptionPair(ResString.GetMultilingualString("6d5d6484-2bf7-4448-913f-8e2ed374a151", "Past Dates Category"), ""));
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.YesterdayMultilingual, "", -1);
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.LastWeekMultilingual, DateRangeSpan.LastWeek);
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.Last7DaysMultilingual, "", 0, -1);
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.Last14DaysMultilingual, "", 0, -2);
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.LastMonthMultilingual, "", 0, 0, -1);
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.LastCalendarMonthMultilingual, DateRangeSpan.LastMonth);
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.Last2MthsMultilingual, "", 0, 0, -2);
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.Last3MthsMultilingual, "", 0, 0, -3);
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.Last6MthsMultilingual, "", 0, 0, -6);
			propertySearch_ListCore.AddPair(DateRangeSearchTexts.Last12MthsMultilingual, "", 0, 0, 0, -1);
			propertySearch_ListCore.AddPair(PastMultilingual, Res.GetString("40be4b69-995e-4535-8b84-1d40d5c78849", "Any date in the past"));

			if (!HideFutureDates)
			{
				propertySearch_ListCore.AddPair("");
				propertySearch_ListCore.Add(new CategoryCodeDescriptionPair(ResString.GetMultilingualString("6392bfb2-8258-4a16-8fad-944ffe1f5d5b", "Future Dates Category"), ""));
				propertySearch_ListCore.AddPair(DateRangeSearchTexts.TomorrowMultilingual, "", 1);
				propertySearch_ListCore.AddPair(DateRangeSearchTexts.NextWeekMultilingual, DateRangeSpan.NextWeek);
				propertySearch_ListCore.AddPair(DateRangeSearchTexts.Next7DaysMultilingual, "", 0, 1);
				propertySearch_ListCore.AddPair(DateRangeSearchTexts.Next14DaysMultilingual, "", 0, 2);
				propertySearch_ListCore.AddPair(DateRangeSearchTexts.NextMonthMultilingual, "", 0, 0, 1);
				propertySearch_ListCore.AddPair(DateRangeSearchTexts.NextCalendarMonthMultilingual, DateRangeSpan.NextMonth);
				propertySearch_ListCore.AddPair(DateRangeSearchTexts.Next2MthsMultilingual, "", 0, 0, 2);
				propertySearch_ListCore.AddPair(DateRangeSearchTexts.Next3MthsMultilingual, "", 0, 0, 3);
				propertySearch_ListCore.AddPair(DateRangeSearchTexts.Next6MthsMultilingual, "", 0, 0, 6);
				propertySearch_ListCore.AddPair(DateRangeSearchTexts.Next12MthsMultilingual, "", 0, 0, 0, 1);
				propertySearch_ListCore.AddPair(FutureMultilingual, Res.GetString("2650b7e9-c623-4f4f-867a-436c50dd1f04", "Any date in the future"));
			}

			#region SuppressResourceStringsCheckRegion

			propertySearch_ListCore.AddPair("");
			propertySearch_ListCore.Add(new CategoryCodeDescriptionPair(ResString.GetMultilingualString("a423caf8-4275-4b29-82af-0d328ee7776b", "Ranges Category"), ""));
			propertySearch_ListCore.AddPair(SpecifiedDateRangeMultilingual, Res.GetString("Filter|DateRangeSearchList|SpecifiedDateRangeFilterDescription", "Specify my own Date Range"));
			propertySearch_ListCore.AddPair(SpecifiedDateTimeRangeMultilingual, Res.GetString("Filter|DateRangeSearchList|SpecifiedTimeRangeFilterDescription", "Specify my own Date-Time Range"));

			if (!HideOffsetFilters)
			{
				propertySearch_ListCore.AddPair(SpecifiedHourOffsetRangeMultilingual, Res.GetString("Filter|DateRangeSearchList|SpecifiedHourRangeFilterDescription", "Specify my own Hour Offset Range"));
				propertySearch_ListCore.AddPair(SpecifiedWorkHourOffsetRangeMultilingual, Res.GetString("Filter|DateRangeSearchList|SpecifiedWorkHourRangeFilterDescription", "Specify my own Working Hour Offset Range"));
				propertySearch_ListCore.AddPair(SpecifiedDayOffsetRangeMultilingual, Res.GetString("Filter|DateRangeSearchList|SpecifiedDayOffsetRangeFilterDescription", "Specify my own Day Offset Range"));
			}

			if (isNullable)
			{
				propertySearch_ListCore.AddPair("");
				propertySearch_ListCore.Add(new CategoryCodeDescriptionPair(ResString.GetMultilingualString("a375efc7-8567-44b1-a15b-29e275bfe3b8", "Date Entered Category"), ""));
				propertySearch_ListCore.AddPair(HasDateEnteredMultilingual, Res.GetString("Filter|DateRangeSearchList|HasDateEnteredFilterDescription", "Date has been entered"));
				propertySearch_ListCore.AddPair(HasNoDateEnteredMultilingual, Res.GetString("Filter|DateRangeSearchList|HasNoDateEnteredFilterDescription", "Date has not been entered"));
			}

			#endregion

			return propertySearch_ListCore;
		}

		public bool HideFutureDates
		{
			get { return fHideFutureDates; }
			set
			{
				fHideFutureDates = value;
				fPropertySearch_ListCore = null;
			}
		}

		public bool HideOffsetFilters
		{
			get { return fHideOffsetFilters; }
			set
			{
				fHideOffsetFilters = value;
				fPropertySearch_ListCore = null;
			}
		}

		public static ZString SpecifiedDateRange => SpecifiedDateRangeMultilingual.GetUnresolvedString();
		internal static MultilingualString SpecifiedDateRangeMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|SpecifiedDateRangeFilter", "Date range");

		public static ZString SpecifiedDateTimeRange => SpecifiedDateTimeRangeMultilingual.GetUnresolvedString();
		internal static MultilingualString SpecifiedDateTimeRangeMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|SpecifiedTimeRangeFilter", "Time range");

		public static ZString SpecifiedHourOffsetRange => SpecifiedHourOffsetRangeMultilingual.GetUnresolvedString();
		internal static MultilingualString SpecifiedHourOffsetRangeMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|SpecifiedHourRangeFilter", "Offset range");

		public static ZString SpecifiedWorkHourOffsetRange => SpecifiedWorkHourOffsetRangeMultilingual.GetUnresolvedString();
		internal static MultilingualString SpecifiedWorkHourOffsetRangeMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|SpecifiedWorkHourRangeFilter", "Offset range (working hours)");

		public static ZString SpecifiedDayOffsetRange => SpecifiedDayOffsetRangeMultilingual.GetUnresolvedString();
		internal static MultilingualString SpecifiedDayOffsetRangeMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|SpecifiedDayRangeFilter", "Offset range (days)");

		public static ZString HasDateEntered => HasDateEnteredMultilingual.GetUnresolvedString();
		internal static MultilingualString HasDateEnteredMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|HasDateEnteredFilter", "Has Date");

		public static ZString HasNoDateEntered => HasNoDateEnteredMultilingual.GetUnresolvedString();
		internal static MultilingualString HasNoDateEnteredMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|HasNoDateEnteredFilter", "Has No Date");

		public static ZString Past => PastMultilingual.GetUnresolvedString();
		internal static MultilingualString PastMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Past", "In the Past");

		public static ZString Future => FutureMultilingual.GetUnresolvedString();
		internal static MultilingualString FutureMultilingual => ResString.GetMultilingualString("Filter|DateRangeSearchList|Future", "In the Future");

		ZString fPropertySearch;
		DateRangePairList fPropertySearch_ListCore;
		bool fHideFutureDates;
		bool fHideOffsetFilters;

		#endregion

		#region Property1

		public ZDateTime Property1
		{
			get { return fProperty1; }
			set
			{
				if (fProperty1 != value)
				{
					fProperty1 = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty1();
					}
					Property1Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		ZDateTime fProperty1;

		#endregion

		#region Property1Validation

		public Validation Property1Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property1Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { property1Validation = value; }
		}
		Validation property1Validation;

		#endregion

		#region Property2

		public ZDateTime Property2
		{
			get { return fProperty2; }
			set
			{
				if (fProperty2 != value)
				{
					fProperty2 = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty2();
					}
					Property2Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo Property2Info
		{
			get { return GetZPropertyInfo(nameof(Property2)); }
		}

		ZDateTime fProperty2;

		#endregion

		#region Property2Validation

		public Validation Property2Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property2Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { property2Validation = value; }
		}
		Validation property2Validation;

		#endregion

		#region PropertyDecimal1

		const int DecimalPlacesForDecimalProperties = 2;
		public const long MaxValueForDecimalProperties = 9999;

		[DecimalPlaces(DecimalPlacesForDecimalProperties)]
		[ResourceStringData("a8b9b67b-28a6-48c6-8aef-25e4166b71f3", Caption = "At least value")]
		public ZDecimal PropertyDecimal1
		{
			get { return propertyDecimal1; }
			set
			{
				if (propertyDecimal1 != value)
				{
					propertyDecimal1 = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidatePropertyDecimal1();
						Validation.ValidatePropertyDecimal2();
					}

					PropertyDecimal1Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo PropertyDecimal1Info => GetZPropertyInfo(nameof(PropertyDecimal1));

		ZDecimal propertyDecimal1;

		#endregion

		#region PropertyDecimal1Validation

		public Validation PropertyDecimal1Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return propertyDecimal1Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { propertyDecimal1Validation = value; }
		}
		Validation propertyDecimal1Validation;

		#endregion

		#region PropertyDecimal2

		[DecimalPlaces(DecimalPlacesForDecimalProperties)]
		[ResourceStringData("2596412d-bc6d-4561-961d-350b61b511b0", Caption = "At most value")]
		public ZDecimal PropertyDecimal2
		{
			get { return propertyDecimal2; }
			set
			{
				if (propertyDecimal2 != value)
				{
					propertyDecimal2 = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidatePropertyDecimal1();
						Validation.ValidatePropertyDecimal2();
					}

					PropertyDecimal2Info.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo PropertyDecimal2Info => GetZPropertyInfo(nameof(PropertyDecimal2));

		ZDecimal propertyDecimal2;

		#endregion

		#region PropertyDecimal1Validation

		public Validation PropertyDecimal2Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return propertyDecimal2Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { propertyDecimal2Validation = value; }
		}
		Validation propertyDecimal2Validation;

		#endregion

		#region FilterOption

		[List("FilterOptions")]
		public ZString FilterOption
		{
			get { return filterOption; }
			set
			{
				if (filterOption != value)
				{
					filterOption = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateFilterOption();
					}

					FilterOptionInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo FilterOptionInfo
		{
			get { return GetZPropertyInfo(nameof(FilterOption)); }
		}

		ZString filterOption;

		public ICodeDescriptionPairList FilterOptions
		{
			get
			{
				if (IsPropertySearchUsingSpecifiedHourOffsetRange || IsPropertySearchUsingSpecifiedWorkHourOffsetRange || IsPropertySearchUsingSpecifiedDayOffsetRange)
				{
					return new DateOffsetRangeFilterOptions();
				}
				else
				{
					return new CodeDescriptionPairList();
				}
			}
		}

		#endregion

		#region Validation

		public new ModuleFilterDateValidation Validation
		{
			get { return (ModuleFilterDateValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleFilterDateValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { GetComparisonOperator(), FromDate, ToDate }; }
		}

		protected DateComparisonOperator GetComparisonOperator()
		{
			var result = DateComparisonOperator.HasDateInRange;
			if (IsPropertySearchUsingHasDateEntered)
			{
				result = DateComparisonOperator.HasDateEntered;
			}
			else if (IsPropertySearchUsingHasNoDateEntered)
			{
				result = DateComparisonOperator.HasNoDateEntered;
			}
			return result;
		}

		protected virtual IZType GetTypeForQuery(ZDateTime value)
		{
			return value;
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = new ZQuery();
			AddFilterParameters(query);

			return query;
		}

		protected virtual void AddFilterParameters(ZQuery query)
		{
			if (IsPropertySearchUsingPastDate)
			{
				query.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualTo, GetTypeForQuery(GetCurrentTime()));
			}
			else if (IsPropertySearchUsingFutureDate)
			{
				query.AddToFilter(FilterColumn, SQLComparisonOperator.GreaterThan, GetTypeForQuery(GetCurrentTime()));
			}
			else if (IsPropertySearchUsingSpecifiedDateTimeRange)
			{
				if (FromDate.IsValid)
				{
					query.AddToFilter(FilterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, GetTypeForQuery(FromDate));
				}

				if (ToDate.IsValid)
				{
					query.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualTo, GetTypeForQuery(ToDate));
				}
			}
			else if (IsPropertySearchUsingHasDateEntered)
			{
				if (FilterColumn.IsNullable)
				{
					query.AddToFilter(JoinCondition.And, FilterColumn, SQLComparisonOperator.NotEqual, null);
				}
			}
			else if (IsPropertySearchUsingSpecifiedHourOffsetRange || IsPropertySearchUsingSpecifiedWorkHourOffsetRange)
			{
				AddHoursOffsetFilter(query);
			}
			else if (IsPropertySearchUsingSpecifiedDayOffsetRange)
			{
				AddDayOffsetFilter(query);
			}
			else if (IsPropertySearchUsingHasNoDateEntered)
			{
				if (FilterColumn.IsNullable)
				{
					query.AddToFilter(JoinCondition.Or, FilterColumn, SQLComparisonOperator.Equal, null);
				}
			}
			else if (IsPropertySearchUsingSpecifiedDateRange)
			{
				if (ConvertFromLocalToUTC)
				{
					if (FromDate.IsValid)
					{
						query.AddToFilter(FilterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, GetTypeForQuery(FromDate));
					}

					if (ToDate.IsValid)
					{
						query.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualTo, GetTypeForQuery(ToDate));
					}
				}
				else
				{
					if (FromDate.IsValid)
					{
						query.AddToFilter(FilterColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, GetTypeForQuery(FromDate));
					}

					if (ToDate.IsValid)
					{
						query.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, GetTypeForQuery(ToDate));
					}
				}
			}
			else
			{
				if (ConvertFromLocalToUTC)
				{
					if (FromDate.IsValid)
					{
						query.AddToFilter(FilterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, GetTypeForQuery(FromDate));
					}

					if (ToDate.IsValid)
					{
						query.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualTo, GetTypeForQuery(ToDate));
					}
				}
				else
				{
					if (FromDate.IsValid)
					{
						query.AddToFilter(FilterColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, GetTypeForQuery(FromDate));
					}

					if (ToDate.IsValid)
					{
						query.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, GetTypeForQuery(ToDate));
					}
				}
			}
		}

		protected virtual void AddHoursOffsetFilter(ZQuery query)
		{
			AddOffsetFilter(query, shouldAddOuterRangeFilter: Property2.IsValid);
		}

		void AddDayOffsetFilter(ZQuery query)
		{
			AddOffsetFilter(query, shouldAddOuterRangeFilter: PropertyDecimal2 > 0);
		}

		void AddOffsetFilter(ZQuery query, bool shouldAddOuterRangeFilter)
		{
			var outerRangeComparisonOperator = FilterOption == DateOffsetRangeFilterOptions.Codes.Past ? SQLComparisonOperator.GreaterThanOrEqualTo : SQLComparisonOperator.LessThanOrEqualTo;
			var innerRangeComparisonOperator = FilterOption == DateOffsetRangeFilterOptions.Codes.Past ? SQLComparisonOperator.LessThanOrEqualTo : SQLComparisonOperator.GreaterThanOrEqualTo;

			query.AddToFilter(FilterColumn, innerRangeComparisonOperator, GetTypeForQuery(InnerOffsetTime));

			if (shouldAddOuterRangeFilter)
			{
				query.AddToFilter(FilterColumn, outerRangeComparisonOperator, GetTypeForQuery(OuterOffsetTime));
			}
		}

		ZDateTime InnerOffsetTime
		{
			get
			{
				var currentTime = GetCurrentTime();
				var minutesMultiplier = FilterOption == DateOffsetRangeFilterOptions.Codes.Past ? -1 : 1;
				var innerOffsetMinutes = 0;
				var result = currentTime;

				if (IsPropertySearchUsingSpecifiedHourOffsetRange)
				{
					innerOffsetMinutes = Property1.IsValid ? (int)Property1.GetMinutesFromDateTimeSpan() * minutesMultiplier : 0;
					result = currentTime.AddMinutes(innerOffsetMinutes);
				}
				else if (IsPropertySearchUsingSpecifiedDayOffsetRange)
				{
					innerOffsetMinutes = GetMinutesFromDays(PropertyDecimal1) * minutesMultiplier;
					result = currentTime.AddMinutes(innerOffsetMinutes);
				}
				else if (IsPropertySearchUsingSpecifiedWorkHourOffsetRange)
				{
					if (Property1.IsValid)
					{
						result = GetDateTimeInWorkingHoursFutureOrPastFromNow(Property1, minutesMultiplier);
					}
				}

				return result;
			}
		}

		ZDateTime OuterOffsetTime
		{
			get
			{
				var currentTime = GetCurrentTime();
				var minutesMultiplier = FilterOption == DateOffsetRangeFilterOptions.Codes.Past ? -1 : 1;
				var result = ZDateTime.Empty;

				if (IsPropertySearchUsingSpecifiedHourOffsetRange)
				{
					if (Property2.IsValid)
					{
						var outerOffsetMinutes = (int)Property2.GetMinutesFromDateTimeSpan() * minutesMultiplier;
						result = currentTime.AddMinutes(outerOffsetMinutes);
					}
				}
				else if (IsPropertySearchUsingSpecifiedDayOffsetRange)
				{
					var outerOffsetMinutes = GetMinutesFromDays(PropertyDecimal2) * minutesMultiplier;
					result = currentTime.AddMinutes(outerOffsetMinutes);
				}
				else if (IsPropertySearchUsingSpecifiedWorkHourOffsetRange)
				{
					if (Property2.IsValid)
					{
						result = GetDateTimeInWorkingHoursFutureOrPastFromNow(Property2, minutesMultiplier);
					}
				}

				return result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		DateTime GetDateTimeInWorkingHoursFutureOrPastFromNow(ZDateTime offset, int minutesMultiplier)
		{
			var workDays = ObjectFactory.Get<IWorkingDaysProvider>().GetWorkingDays(new BusinessObjectFactory(), EnvProxy.Instance.CurrentDepartmentPK, EnvProxy.Instance.CurrentBranchPK);
			var zeroCalculationMode = minutesMultiplier == -1 ? ZeroWorkingHoursCalculationMode.GoIntoThePastIfZero : ZeroWorkingHoursCalculationMode.GoIntoTheFutureIfZero;
			var localResult = workDays.GetDateTimeInWorkingHoursFutureOrPast(ZDateTime.Now.ToDateTime(), offset.GetMinutesFromDateTimeSpan() * minutesMultiplier / 60, zeroCalculationMode: zeroCalculationMode, precisionMinutes: 1);
			return ConvertFromLocalToUTC ? Environment.TimeFactory.Instance.GetUtcFromLocalTime(localResult) : localResult;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		static int GetMinutesFromDays(ZDecimal days)
		{
			return (int)(days * 24 * 60);
		}

		bool IsQueryingUTC()
		{
			return UserEntersUtcValue || ConvertFromLocalToUTC;
		}

		protected ZDateTime GetCurrentTime()
		{
			return IsQueryingUTC() ? ZDateTime.UtcNow : ZDateTime.Now;
		}

		public ZDateTime FromDate
		{
			get
			{
				ZDateTime result;
				if (IsPropertySearchUsingSpecifiedDateRange)
				{
					result = Property1.IsValid ? Property1.Date : Property1;
				}
				else if (IsPropertySearchUsingSpecifiedDateTimeRange)
				{
					result = Property1;
				}
				else if (IsPropertySearchUsingHasNoDateEntered || IsPropertySearchUsingHasDateEntered)
				{
					result = ZDateTime.Empty;
				}
				else if (IsPropertySearchUsingPastDate)
				{
					result = ZDateTime.Empty;
				}
				else if (IsPropertySearchUsingFutureDate)
				{
					result = GetCurrentTime().AddMilliseconds(1);
				}
				else if (IsPropertySearchUsingSpecifiedHourOffsetRange || IsPropertySearchUsingSpecifiedWorkHourOffsetRange || IsPropertySearchUsingSpecifiedDayOffsetRange)
				{
					result = FilterOption == DateOffsetRangeFilterOptions.Codes.Past ? OuterOffsetTime : InnerOffsetTime;
				}
				else if (IsPropertySearchValid)
				{
					result = ((DateRangePair)PropertySearch_List[PropertySearch]).FromDate;
				}
				else
				{
					result = ZDateTime.Empty;
				}

				if (ConvertFromLocalToUTC && result.IsValid && !result.IsEmpty && !IsPropertySearchUsingSpecifiedHourOffsetRange && !IsPropertySearchUsingSpecifiedWorkHourOffsetRange && !IsPropertySearchUsingSpecifiedDayOffsetRange && !IsPropertySearchUsingFutureDate)
				{
					result = Environment.TimeFactory.Instance.GetUtcFromLocalTime(result.ToDateTime());
				}

				return result;
			}
		}

		public ZDateTime ToDate
		{
			get
			{
				ZDateTime result;

				if (IsPropertySearchUsingSpecifiedDateRange)
				{
					result = Property2.IsValid ? Property2.Date.AddHours(24).AddMilliseconds(-3) : Property2;
				}
				else if (IsPropertySearchUsingSpecifiedDateTimeRange)
				{
					result = Property2;
				}
				else if (IsPropertySearchUsingHasNoDateEntered || IsPropertySearchUsingHasDateEntered)
				{
					result = ZDateTime.Empty;
				}
				else if (IsPropertySearchUsingPastDate)
				{
					result = GetCurrentTime();
				}
				else if (IsPropertySearchUsingFutureDate)
				{
					result = ZDateTime.Empty;
				}
				else if (IsPropertySearchUsingSpecifiedHourOffsetRange || IsPropertySearchUsingSpecifiedWorkHourOffsetRange || IsPropertySearchUsingSpecifiedDayOffsetRange)
				{
					result = FilterOption == DateOffsetRangeFilterOptions.Codes.Past ? InnerOffsetTime : OuterOffsetTime;
				}
				else if (IsPropertySearchValid)
				{
					result = ((DateRangePair)PropertySearch_List[PropertySearch]).ToDate;
				}
				else
				{
					result = ZDateTime.Empty;
				}

				if (ConvertFromLocalToUTC && result.IsValid && !result.IsEmpty && !IsPropertySearchUsingSpecifiedHourOffsetRange && !IsPropertySearchUsingSpecifiedWorkHourOffsetRange && !IsPropertySearchUsingSpecifiedDayOffsetRange && !IsPropertySearchUsingPastDate)
				{
					result = Environment.TimeFactory.Instance.GetUtcFromLocalTime(result.ToDateTime());
				}

				return result;
			}
		}

		public int GetDaysDifferenceBetweenToAndFromDates
		{
			get
			{
				if (!ToDate.IsValid || !FromDate.IsValid)
				{
					return int.MaxValue;
				}
				else
				{
					return (ToDate - FromDate).Days;
				}
			}
		}

		public ZBool IsDateRangeWithin3Months
		{
			get
			{
				if (!ToDate.IsValid || !FromDate.IsValid)
				{
					return false;
				}
				else
				{
					return FromDate.Date.AddMonths(3) >= ToDate.Date;
				}
			}
		}

		protected override bool ShouldReevaluateQuery()
		{
			return base.ShouldReevaluateQuery()
				|| IsPropertySearchUsingSpecifiedHourOffsetRange
				|| IsPropertySearchUsingSpecifiedWorkHourOffsetRange
				|| IsPropertySearchUsingSpecifiedDayOffsetRange
				|| IsPropertySearchUsingPastDate
				|| IsPropertySearchUsingFutureDate
				|| (PropertySearch_List.ContainsCode(PropertySearch) && PropertySearch_List[PropertySearch, StringComparison.OrdinalIgnoreCase] is DateRangePair);
		}

		public bool ConvertFromLocalToUTC { get; set; }
		public bool UserEntersUtcValue { get; set; }

		public void SetPropertyValues()
		{
			this.Property1 = this.FromDate;
			this.Property2 = this.ToDate;
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property1 = ZDateTime.BrettsBirthday.AddDays(ModuleFilter.RandomInt(3650));
		}

#endif
		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("SearchProperty", PropertySearch);

			var sqlDate1 = Property1.IsValid ? Property1.SqlFormat : ZString.Empty;
			writer.WriteElementString("Property1", sqlDate1);

			var sqlDate2 = Property2.IsValid ? Property2.SqlFormat : ZString.Empty;
			writer.WriteElementString("Property2", sqlDate2);

			writer.WriteElementString("FilterOption", FilterOption);
			writer.WriteElementString("PropertyDecimal1", PropertyDecimal1.ToString(DecimalPlacesForDecimalProperties));
			writer.WriteElementString("PropertyDecimal2", PropertyDecimal2.ToString(DecimalPlacesForDecimalProperties));
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "SearchProperty")
			{
				PropertySearch = reader.ReadElementString("SearchProperty");
			}
			else // property did not exist in prev version, set it so that the user can see their stored dates
			{
				PropertySearch = SpecifiedDateRange;
			}

			ZDateTime dateValue;

			if (TryDeserialiseDateProperty(reader, nameof(Property1), out dateValue))
			{
				Property1 = dateValue;
			}

			if (TryDeserialiseDateProperty(reader, nameof(Property2), out dateValue))
			{
				Property2 = dateValue;
			}

			if (reader.Name == "FilterOption")
			{
				FilterOption = reader.ReadElementString("FilterOption");
			}

			if (reader.Name == "PropertyDecimal1")
			{
				PropertyDecimal1 = reader.ReadElementContentAsDecimal();
			}

			if (reader.Name == "PropertyDecimal2")
			{
				PropertyDecimal2 = reader.ReadElementContentAsDecimal();
			}
		}

		public new void DeserializeProperties(XmlReader reader) => DeserializePropertiesFromXml(reader);

		static bool TryDeserialiseDateProperty(XmlReader reader, string propertyName, out ZDateTime dateValue)
		{
			var dateString = (reader.Name == propertyName) ? reader.ReadElementString(propertyName) : string.Empty;

			if (!string.IsNullOrEmpty(dateString))
			{
				try
				{
					dateValue = ZDateTime.FromSqlFormat(dateString);
					return true;
				}
				catch (FormatException) { }
			}

			dateValue = ZDateTime.Empty;
			return false;
		}

		#endregion

		#region class DateRangePairList

		protected class DateRangePairList : CodeDescriptionPairList
		{
			public void AddPair(MultilingualString code, string description, int daysFromToday)
			{
				AddPair(code, description, daysFromToday, 0);
			}

			public void AddPair(MultilingualString code, string description, int daysFromToday, int weeksFromToday)
			{
				AddPair(code, description, daysFromToday, weeksFromToday, 0);
			}

			public void AddPair(MultilingualString code, string description, int daysFromToday, int weeksFromToday, int monthsFromToday)
			{
				AddPair(code, description, daysFromToday, weeksFromToday, monthsFromToday, 0);
			}

			public void AddPair(MultilingualString code, string description, int daysFromToday, int weeksFromToday, int monthsFromToday, int yearsFromToday)
			{
				Add(new DateRangePair(code, description, daysFromToday, weeksFromToday, monthsFromToday, yearsFromToday));
			}

			public void AddPair(MultilingualString code, DateRangeSpan rangeSpan)
			{
				Add(new DateRangeSpanPair(code, rangeSpan));
			}
		}

		#endregion

		#region class DateRangePair

		#region class DateRangePair

		public class DateRangePair : CodeDescriptionPair
		{
			#region Construction

			public DateRangePair(MultilingualString code, string description, int daysFromToday)
				: this(code, description, daysFromToday, 0)
			{
			}

			public DateRangePair(MultilingualString code, string description, int daysFromToday, int weeksFromToday)
				: this(code, description, daysFromToday, weeksFromToday, 0)
			{
			}

			public DateRangePair(MultilingualString code, string description, int daysFromToday, int weeksFromToday, int monthsFromToday)
				: this(code, description, daysFromToday, weeksFromToday, monthsFromToday, 0)
			{
			}

			public DateRangePair(MultilingualString code, string description, int daysFromToday, int weeksFromToday, int monthsFromToday, int yearsFromToday)
				: base(code, description)
			{
				DaysFromToday = daysFromToday;
				WeeksFromToday = weeksFromToday;
				MonthsFromToday = monthsFromToday;
				YearsFromToday = yearsFromToday;
			}

			protected DateRangePair(MultilingualString code, string description)
				: base(code, description)
			{
			}

			#endregion

			#region Description

			protected override MultilingualString MultilingualDescriptionCore
			{
				get
				{
					MultilingualString result;

					if (!base.MultilingualDescriptionCore.IsEmpty)
					{
						result = base.MultilingualDescriptionCore;
					}
					else if (IsToday || IsTomorrowOrYesterday)
					{
						result = (NoResString)FromDate.ToShortDateString();
					}
					else
					{
						result = DateRangeDescription;
					}

					return result;
				}
			}

			protected MultilingualString DateRangeDescription
			{
				get { return ResString.GetMultilingualString("ddaa66a4-af9b-4bdf-bcf6-7aeadd5175c5", "{0} to {1}", FromDate.ToShortDateString(), ToDate.ToShortDateString()); }
			}

			#endregion

			#region FromDate

			public ZDateTime FromDate
			{
				get { return FromDateCore; }
			}

			protected virtual ZDateTime FromDateCore
			{
				get
				{
					var result = ZDateTime.Today;
					var daysFromToday = DaysFromToday;

					if (YearsFromToday < 0)
					{
						result = ZDateTime.Today.AddYears(YearsFromToday);
					}
					if (MonthsFromToday < 0)
					{
						result = ZDateTime.Today.AddMonths(MonthsFromToday);
					}
					if (WeeksFromToday < 0)
					{
						daysFromToday = WeeksFromToday * 7;
					}

					if (IsTomorrowOrYesterday)
					{
						result = ZDateTime.Today.AddDays(daysFromToday);
					}
					else if (daysFromToday < 0)
					{
						result = ZDateTime.Today.AddDays(daysFromToday + 1);
					}

					return result;
				}
			}

			#endregion

			#region ToDate

			public ZDateTime ToDate
			{
				get { return ToDateCore; }
			}

			protected virtual ZDateTime ToDateCore
			{
				get
				{
					var result = ZDateTime.Today;
					var daysFromToday = DaysFromToday;

					if (YearsFromToday > 0)
					{
						result = ZDateTime.Today.AddYears(YearsFromToday);
					}
					if (MonthsFromToday > 0)
					{
						result = ZDateTime.Today.AddMonths(MonthsFromToday);
					}
					if (WeeksFromToday > 0)
					{
						daysFromToday = WeeksFromToday * 7;
					}

					if (IsTomorrowOrYesterday || IsToday)
					{
						result = ZDateTime.Today.AddDays(daysFromToday);
					}
					else if (daysFromToday > 0)
					{
						result = ZDateTime.Today.AddDays(daysFromToday - 1);
					}

					result = result.AddDays(1).AddMilliseconds(-3);

					return result;
				}
			}

			#endregion

			bool IsToday
			{
				get { return (DaysFromToday == 0 && WeeksFromToday == 0 && MonthsFromToday == 0 && YearsFromToday == 0); }
			}

			bool IsTomorrowOrYesterday
			{
				get { return (DaysFromToday == 1 || DaysFromToday == -1) && WeeksFromToday == 0 && MonthsFromToday == 0 && YearsFromToday == 0; }
			}

			readonly int DaysFromToday;
			readonly int WeeksFromToday;
			readonly int MonthsFromToday;
			readonly int YearsFromToday;
		}

		#endregion

		#region class DateRangeSpanPair

		public class DateRangeSpanPair : DateRangePair
		{
			public DateRangeSpanPair(MultilingualString code, DateRangeSpan rangeSpan)
				: base(code, code)
			{
				fRangeSpan = rangeSpan;
			}

			#region Description

			protected override MultilingualString MultilingualDescriptionCore
			{
				get { return DateRangeDescription; }
			}

			#endregion

			#region FromDate

			protected override ZDateTime FromDateCore
			{
				get
				{
					ZDateTime result;

					switch (RangeSpan)
					{
						case DateRangeSpan.CurrentWeek:
							result = FirstDayOfThisWeek;
							break;

						case DateRangeSpan.CurrentMonth:
							result = FirstDayOfThisMonth;
							break;

						case DateRangeSpan.LastWeek:
							result = FirstDayOfThisWeek.AddDays(-7);
							break;

						case DateRangeSpan.LastMonth:
							result = FirstDayOfThisMonth.AddMonths(-1);
							break;

						case DateRangeSpan.NextWeek:
							result = FirstDayOfThisWeek.AddDays(7);
							break;

						case DateRangeSpan.NextMonth:
							result = FirstDayOfThisMonth.AddMonths(1);
							break;

						default:
							throw new NotSupportedException(RangeSpan + " is not supported.");
					}

					return result;
				}
			}

			#endregion

			#region ToDate

			protected override ZDateTime ToDateCore
			{
				get
				{
					ZDateTime result;

					switch (RangeSpan)
					{
						case DateRangeSpan.CurrentWeek:
							result = LastDayOfThisWeek;
							break;

						case DateRangeSpan.CurrentMonth:
							result = LastDayOfThisMonth;
							break;

						case DateRangeSpan.LastWeek:
							result = FirstDayOfThisWeek.AddDays(-1);
							break;

						case DateRangeSpan.LastMonth:
							result = FirstDayOfThisMonth.AddDays(-1);
							break;

						case DateRangeSpan.NextWeek:
							result = LastDayOfThisWeek.AddDays(7);
							break;

						case DateRangeSpan.NextMonth:
							result = FirstDayOfThisMonth.AddMonths(2).AddDays(-1);
							break;

						default:
							throw new NotSupportedException(RangeSpan + " is not supported.");
					}

					result = result.AddDays(1).AddMilliseconds(-3);

					return result;
				}
			}

			#endregion

			#region First / Last day of this Week / Month

			ZDateTime FirstDayOfThisWeek
			{
				get
				{
					var today = ZDateTime.Today;
					return today.AddDays(-(int)today.DayOfWeek);
				}
			}

			ZDateTime LastDayOfThisWeek
			{
				get { return FirstDayOfThisWeek.AddDays(6); }
			}

			ZDateTime FirstDayOfThisMonth
			{
				get
				{
					var today = ZDateTime.Today;
					return today.AddDays(-today.Day + 1);
				}
			}

			ZDateTime LastDayOfThisMonth
			{
				get { return FirstDayOfThisMonth.AddMonths(1).AddDays(-1); }
			}

			#endregion

			#region DateRangeSpan

			public DateRangeSpan RangeSpan
			{
				get { return fRangeSpan; }
			}

			readonly DateRangeSpan fRangeSpan;

			#endregion

		}

		#endregion

		public enum DateRangeSpan
		{
			CurrentWeek,
			CurrentMonth,
			LastWeek,
			LastMonth,
			NextWeek,
			NextMonth
		}

		#endregion
	}

	#region class Validation

	public class ModuleFilterDateValidation : ModuleFilterValidation
	{
		public ModuleFilterDateValidation(ModuleDateFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		#region ValidatePropertySearch

		public void ValidatePropertySearch()
		{
			ValidateCalculatedProperty(Parent.PropertySearchInfo);
		}

		protected virtual void CheckPropertySearch()
		{
			ListValidation.ErrorIfInvalidCode(Parent.PropertySearchInfo, Parent.PropertySearch_List);
		}

		#endregion

		#region ValidateProperty1

		public void ValidateProperty1()
		{
			ValidateCalculatedProperty(Parent.Property1Info);
		}

		protected virtual void CheckProperty1()
		{
			if (Parent.IsPropertySearchUsingSpecifiedDateRange || Parent.IsPropertySearchUsingSpecifiedDateTimeRange)
			{
				TypeValidation.CheckValidZDateTimeWithoutRange(Parent.Property1Info);
				TypeValidation.CheckValidZDateTimeRange(Parent.Property1Info, new TypeValidationLimits { PastYearsBeforeError = DateRangeValidation.MaximumPastYears });
			}
			else if (Parent.IsPropertySearchUsingSpecifiedHourOffsetRange || Parent.IsPropertySearchUsingSpecifiedWorkHourOffsetRange)
			{
				if (!Parent.Property1.IsEmpty && !Parent.Property1.IsValid)
				{
					Parent.Property1Info.AddError(Res.GetString("D9CA175E-43F4-4BA5-998E-09F3AD7652EC", "Enter a valid inner offset."));
				}
			}

			Parent.Property1Validation?.Invoke(Parent.Property1Info);
		}

		#endregion

		#region ValidateProperty2

		public void ValidateProperty2()
		{
			ValidateCalculatedProperty(Parent.Property2Info);
		}

		protected virtual void CheckProperty2()
		{
			if (Parent.IsPropertySearchUsingSpecifiedDateRange || Parent.IsPropertySearchUsingSpecifiedDateTimeRange)
			{
				TypeValidation.CheckValidZDateTimeWithoutRange(Parent.Property2Info);
				TypeValidation.CheckValidZDateTimeRange(Parent.Property2Info, new TypeValidationLimits { PastYearsBeforeError = DateRangeValidation.MaximumPastYears });

				if (!Parent.Property1.IsEmpty && !Parent.Property2.IsEmpty)
				{
					if (Parent.Property1 > Parent.Property2)
					{
						Parent.Property2Info.AddWarning(Res.GetString("6ADCE9AB-07F5-4319-BE0F-6C654B453C9A", "To Date and Time should be greater than the From date and Time."));
					}
				}
			}
			else if (Parent.IsPropertySearchUsingSpecifiedHourOffsetRange || Parent.IsPropertySearchUsingSpecifiedWorkHourOffsetRange)
			{
				if (!Parent.Property2.IsEmpty && !Parent.Property2.IsValid)
				{
					Parent.Property2Info.AddError(Res.GetString("4D2C8375-89FA-413D-9522-4BF52FE6F143", "Enter a valid outer offset."));
				}

				if (!Parent.Property1.IsEmpty && Parent.Property1.IsValid
					&& !Parent.Property2.IsEmpty && Parent.Property2.IsValid
					&& Parent.Property1.GetMinutesFromDateTimeSpan() > Parent.Property2.GetMinutesFromDateTimeSpan())
				{
					Parent.Property2Info.AddError(Res.GetString("C2E7AA4B-47F2-4ADE-B303-69F366D4DD1A", "The outer offset should be greater than the inner offset."));
				}
			}

			Parent.Property2Validation?.Invoke(Parent.Property2Info);
		}

		#endregion

		#region ValidatePropertyDecimal1

		public void ValidatePropertyDecimal1()
		{
			ValidateCalculatedProperty(Parent.PropertyDecimal1Info);
		}

		protected void CheckPropertyDecimal1()
		{
			if (Parent.IsPropertySearchUsingSpecifiedDayOffsetRange)
			{
				var decimal2 = Parent.PropertyDecimal2;
				var maxValue = decimal2 > 0 && decimal2 <= ModuleDateFilter.MaxValueForDecimalProperties ? decimal2 : ModuleDateFilter.MaxValueForDecimalProperties;
				CompareValidation.CheckWithinRange(Parent.PropertyDecimal1Info, 0, maxValue);
			}

			Parent.PropertyDecimal1Validation?.Invoke(Parent.PropertyDecimal1Info);
		}

		#endregion

		#region ValidatePropertyDecimal2

		public void ValidatePropertyDecimal2()
		{
			ValidateCalculatedProperty(Parent.PropertyDecimal2Info);
		}

		protected void CheckPropertyDecimal2()
		{
			if (Parent.IsPropertySearchUsingSpecifiedDayOffsetRange)
			{
				CompareValidation.CheckWithinRange(Parent.PropertyDecimal2Info, 0, ModuleDateFilter.MaxValueForDecimalProperties);
			}

			Parent.PropertyDecimal2Validation?.Invoke(Parent.PropertyDecimal2Info);
		}

		#endregion

		#region ValidateFilterOption

		public void ValidateFilterOption()
		{
			ValidateCalculatedProperty(Parent.FilterOptionInfo);
		}

		protected void CheckFilterOption()
		{
			if (Parent.IsPropertySearchUsingSpecifiedHourOffsetRange || Parent.IsPropertySearchUsingSpecifiedWorkHourOffsetRange || Parent.IsPropertySearchUsingSpecifiedDayOffsetRange)
			{
				MandatoryValidation.CheckEntered(Parent.FilterOptionInfo);
				ListValidation.ErrorIfInvalidCode(Parent.FilterOptionInfo);
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidatePropertySearch();
			ValidateProperty1();
			ValidateProperty2();
			ValidateFilterOption();
			ValidatePropertyDecimal1();
			ValidatePropertyDecimal2();
		}

		public override Type AutoValidationType => GetType();

		protected readonly ModuleDateFilter Parent;
	}

	#endregion
}
