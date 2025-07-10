using System;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public delegate ZQuery GetTimeQuery(TimeComparisonOperator comparisonOperator, ZTime value1, ZTime value2);

	public class ModuleTimeFilter : ModuleFilterWithSubDescriptions
	{
		#region Constants

		#endregion

		#region Construction

		protected bool isNullable = true;

		public ModuleTimeFilter(ZString description, GetTimeQuery queryDelegate, bool isNullable = true)
			: base(description, queryDelegate)
		{
			this.isNullable = isNullable;
		}

		protected ModuleTimeFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected ModuleTimeFilter(ZString description, bool isNullable = true)
			: base(description)
		{
			this.isNullable = isNullable;
		}

		public ModuleTimeFilter(ZString description, SchemaDateTimeColumn filterColumn)
			: base(description, filterColumn)
		{
			isNullable = filterColumn.IsNullable;
		}

		#endregion

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
		}

		#endregion

		#region GetNewCommonModuleFilter, CopyPropertiesToFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleTimeFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleTimeFilter)filterToCopyFrom;

			PropertySearch = filter.PropertySearch;
			Property1 = filter.Property1;
			Property2 = filter.Property2;
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory => FilterCategories.Times;

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			PropertySearch = "";
			Property1 = ZTime.Empty;
			Property2 = ZTime.Empty;
		}

		protected override bool IsEmptyCore => !IsPropertySearchValid ||
					(Property1.IsEmpty && Property2.IsEmpty && (IsPropertySearchUsingSpecifiedTimeRange || IsPropertySearchUsingSpecifiedTimeRange));

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
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo PropertySearchInfo => GetZPropertyInfo(nameof(PropertySearch));

		public bool IsPropertySearchUsingSpecifiedTimeRange => PropertySearch.EqualsIgnoringCase(SpecifiedTimeRange);

		public bool IsPropertySearchUsingHasNoTimeEntered => PropertySearch.EqualsIgnoringCase(HasNoTimeEntered);

		public bool IsPropertySearchUsingHasTimeEntered => PropertySearch.EqualsIgnoringCase(HasTimeEntered);

		public bool IsPropertySearchValid => IsPropertySearchUsingSpecifiedTimeRange
					|| IsPropertySearchUsingSpecifiedTimeRange
					|| IsPropertySearchUsingHasTimeEntered
					|| IsPropertySearchUsingHasNoTimeEntered;

		public CodeDescriptionPairList PropertySearch_List => PropertySearch_ListCore;

		CodeDescriptionPairList PropertySearch_ListCore
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

		protected virtual CodeDescriptionPairList CreatePropertySearch_ListCore()
		{
			var propertySearch_ListCore = new CodeDescriptionPairList();

			propertySearch_ListCore.AddPair("");
			propertySearch_ListCore.Add(new CategoryCodeDescriptionPair(ResString.GetMultilingualString("Filter|TimeRangeSearchList|TimeRangesCategory", "Time Ranges Category"), ""));
			propertySearch_ListCore.AddPair(SpecifiedTimeRangeMultilingual, Res.GetString("Filter|TimeRangeSearchList|SpecifiedTimeRangeFilterDescription", "Specify my own Time Range"));
			if (isNullable)
			{
				propertySearch_ListCore.AddPair("");
				propertySearch_ListCore.Add(new CategoryCodeDescriptionPair(ResString.GetMultilingualString("Filter|TimeRangeSearchList|TimeEnteredCategory", "Time Entered Category"), ""));
				propertySearch_ListCore.AddPair(HasTimeEnteredMultilingual, Res.GetString("Filter|TimeRangeSearchList|HasTimeEnteredFilterDescription", "Time has been entered"));
				propertySearch_ListCore.AddPair(HasNoTimeEnteredMultilingual, Res.GetString("Filter|TimeRangeSearchList|HasNoTimeEnteredFilterDescription", "Time has not been entered"));
			}

			return propertySearch_ListCore;
		}

		public static ZString SpecifiedTimeRange => SpecifiedTimeRangeMultilingual.GetUnresolvedString();
		internal static MultilingualString SpecifiedTimeRangeMultilingual => ResString.GetMultilingualString("Filter|TimeRangeSearchList|SpecifiedTimeRangeFilter", "Time range");
		public static ZString HasTimeEntered => HasTimeEnteredMultilingual.GetUnresolvedString();
		internal static MultilingualString HasTimeEnteredMultilingual => ResString.GetMultilingualString("Filter|TimeRangeSearchList|HasTimeEnteredFilter", "Has Time");
		public static ZString HasNoTimeEntered => HasNoTimeEnteredMultilingual.GetUnresolvedString();
		internal static MultilingualString HasNoTimeEnteredMultilingual => ResString.GetMultilingualString("Filter|TimeRangeSearchList|HasNoTimeEnteredFilter", "Has No Time");

		ZString fPropertySearch;
		CodeDescriptionPairList fPropertySearch_ListCore;

		#endregion

		#region Property1

		public ZTime Property1
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

		ZTime fProperty1;

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

		public ZTime Property2
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

		ZTime fProperty2;

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

		#region Validation

		public new ModuleFilterTimeValidation Validation
		{
			get { return (ModuleFilterTimeValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleFilterTimeValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { GetComparisonOperator(), FromTime, ToTime }; }
		}

		protected TimeComparisonOperator GetComparisonOperator()
		{
			var result = TimeComparisonOperator.HasTimeInRange;
			if (IsPropertySearchUsingHasTimeEntered)
			{
				result = TimeComparisonOperator.HasTimeEntered;
			}
			else if (IsPropertySearchUsingHasNoTimeEntered)
			{
				result = TimeComparisonOperator.HasNoTimeEntered;
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
			if (IsPropertySearchUsingHasTimeEntered)
			{
				if (FilterColumn.IsNullable)
				{
					query.AddToFilter(JoinCondition.And, FilterColumn, SQLComparisonOperator.NotEqual, null);
				}
			}
			else if (IsPropertySearchUsingHasNoTimeEntered)
			{
				if (FilterColumn.IsNullable)
				{
					query.AddToFilter(JoinCondition.Or, FilterColumn, SQLComparisonOperator.Equal, null);
				}
			}
			else if (FromTime.IsValid && !FromTime.IsEmpty && ToTime.IsValid && !ToTime.IsEmpty && FromTime > ToTime)
			{
				var subQuery = new ZQuery();
				subQuery.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualTo, GetTypeForQuery(ToTime));
				subQuery.AddToFilter(JoinCondition.Or, FilterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, GetTypeForQuery(FromTime));
				query.AddToFilter(subQuery);
			}
			else
			{
				if (FromTime.IsValid && !FromTime.IsEmpty)
				{
					query.AddToFilter(FilterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, GetTypeForQuery(FromTime));
				}

				if (ToTime.IsValid && !ToTime.IsEmpty)
				{
					query.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualTo, GetTypeForQuery(ToTime));
				}
			}
		}

		protected ZTime GetCurrentTime()
		{
			return ZDateTime.UtcNow.ToTimeSpan();
		}

		public ZDateTime FromTime
		{
			get
			{
				var baseDate = new ZDate(1900, 1, 1);
				return baseDate.Add(Property1.ToTimeSpan());
			}
		}

		public ZDateTime ToTime
		{
			get
			{
				var baseDate = new ZDate(1900, 1, 1);
				return baseDate.Add(Property2.ToTimeSpan());
			}
		}

		protected override bool ShouldReevaluateQuery()
		{
			return base.ShouldReevaluateQuery()
				|| (PropertySearch_List.ContainsCode(PropertySearch) && PropertySearch_List[PropertySearch, StringComparison.OrdinalIgnoreCase] is CodeDescriptionPair);
		}

		public bool ConvertFromLocalToUTC { get; set; }
		public bool UserEntersUtcValue { get; set; }

		public void SetPropertyValues()
		{
			this.Property1 = this.FromTime.TimeOfDay;
			this.Property2 = this.ToTime.TimeOfDay;
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property1 = new ZTime(1, 2);
		}

#endif
		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("SearchProperty", PropertySearch);

			var sqlTime1 = Property1.IsValid ? Property1.SqlFormat : ZString.Empty;
			writer.WriteElementString("Property1", sqlTime1);

			var sqlTime2 = Property2.IsValid ? Property2.SqlFormat : ZString.Empty;
			writer.WriteElementString("Property2", sqlTime2);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "SearchProperty")
			{
				PropertySearch = reader.ReadElementString("SearchProperty");
			}
			else // property did not exist in prev version, set it so that the user can see their stored dates
			{
				PropertySearch = SpecifiedTimeRange;
			}

			ZTime dateValue;

			if (TryDeserialiseTimeProperty(reader, nameof(Property1), out dateValue))
			{
				Property1 = dateValue;
			}

			if (TryDeserialiseTimeProperty(reader, nameof(Property2), out dateValue))
			{
				Property2 = dateValue;
			}
		}

		public new void DeserializeProperties(XmlReader reader) => DeserializePropertiesFromXml(reader);

		static bool TryDeserialiseTimeProperty(XmlReader reader, string propertyName, out ZTime dateValue)
		{
			var dateString = (reader.Name == propertyName) ? reader.ReadElementString(propertyName) : string.Empty;

			if (!string.IsNullOrEmpty(dateString))
			{
				try
				{
					dateValue = ZTime.FromSqlFormat(dateString);
					return true;
				}
				catch (FormatException) { }
			}

			dateValue = ZTime.Empty;
			return false;
		}

		#endregion
	}

	#region class Validation

	public class ModuleFilterTimeValidation : ModuleFilterValidation
	{
		public ModuleFilterTimeValidation(ModuleTimeFilter parent)
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
			Parent.Property2Validation?.Invoke(Parent.Property2Info);
		}

		#endregion

		public override void ValidateAll()
		{
			ValidatePropertySearch();
			ValidateProperty1();
			ValidateProperty2();
		}

		public override Type AutoValidationType => GetType();

		protected readonly ModuleTimeFilter Parent;
	}

	#endregion
}
