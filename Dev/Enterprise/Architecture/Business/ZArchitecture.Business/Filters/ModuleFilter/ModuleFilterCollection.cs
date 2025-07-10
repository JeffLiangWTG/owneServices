using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public class ModuleFilterCollection : IEnumerable<ModuleFilter>, IModuleFilterCollection
	{
		#region Indexer

		public ModuleFilter this[ZString description]
		{
			get { return GetModuleFilter(description, true); }
		}

		ModuleFilter GetModuleFilter(ZString description, bool includeDuplicates)
		{
			ModuleFilter result;

			foreach (var collection in AllFilters.Values)
			{
				if (collection.TryGetValue(description, out result))
				{
					return result;
				}
			}

			if (includeDuplicates && DuplicateFilters.TryGetValue(description, out result))
			{
				return result;
			}

			if (fAliasFilters != null && fAliasFilters.TryGetValue(description, out result))
			{
				return result;
			}

			return null;
		}

		#endregion

		#region Visible

		public ModuleFilter GetVisibleModuleFilter(ZString description, bool includeDuplicates)
		{
			var filter = GetModuleFilter(description, includeDuplicates);
			return (filter != null && filter.Visibility != FilterVisibility.AlwaysAppliedAndHidden) ? filter : null;
		}

		public ModuleFilter GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive(ZString description, bool includeDuplicates = false)
		{
			var result = GetVisibleModuleFilter(description, includeDuplicates);

			if (result != null && result.IsActive) // filter is already in use, so clone it..
			{
				var originalCode = result.Code;

				var newDescription = GetNextDuplicatedModuleFilterDescription(description);
				result = this[description].ShallowCloneAndClearValues(newDescription);
				result.OriginalCode = originalCode;
				result.IsActive = false;

				DuplicateFilters[newDescription] = result;
			}

			return result;
		}

		ZString GetNextDuplicatedModuleFilterDescription(string description)
		{
			var i = 1;
			ZString result = description + " (" + i + ")";

			while (DuplicateFilters.ContainsKey(result))
			{
				i++;
				result = description + " (" + i + ")";
			}

			return result;
		}

		#endregion

		#region Disable

		internal void DisableModuleFilter(string description)
		{
			ModuleFilter filter;

			if (DuplicateFilters.TryGetValue(description, out filter))
			{
				filter.IsActive = false;
				filter.Delete();
				DuplicateFilters.Remove(description);
			}
			else
			{
				filter = GetVisibleModuleFilter(description, false);
				if (filter != null)
				{
					filter.IsActive = false;
				}
			}
		}

		internal void DisableAndRemoveFilterFromCollection(ModuleFilter filter)
		{
			DisableModuleFilter(filter.Description);
			AllFilters[filter.Category].Remove(filter.Description);
		}

		public bool DisableFilterValidation { get; set; }

		#endregion

		#region Filter List

		public CodeDescriptionPairList Filter_List
		{
			get
			{
				if (fFilter_List == null)
				{
					fFilter_List = new CodeDescriptionPairList();

					foreach (var category in AllFilterCategories)
					{
						var filters = new List<ModuleFilter>();

						foreach (var filter in AllFilters[category].Values)
						{
							if (filter.Visibility != FilterVisibility.AlwaysAppliedAndHidden && IsFilterAvailable(filter) && filter.Visible)
							{
								filters.Add(filter);
							}
						}

						if (filters.Count > 0)
						{
							filters.Sort(ModuleFilterComparison);

							fFilter_List.AddPair("");
							fFilter_List.Add(new CategoryCodeDescriptionPair(category.Description, Res.GetString("8c2fed29-2900-4bde-88df-08e8adf0c875", "Filter by {0}", category.Description)));
							fFilter_List.AddRange(filters);
						}
					}
				}

				return fFilter_List;
			}
		}

		protected virtual bool IsFilterAvailable(ModuleFilter filter)
		{
			return !Globals.IsWeb || filter.IsPublishedOnWeb;
		}

		/// <summary>
		/// Compares module filter descriptions and ensures the "Common x" items are first in sort order.
		/// </summary>
		int ModuleFilterComparison(ModuleFilter filter1, ModuleFilter filter2)
		{
			int result;

			if (filter1.Description == filter2.Description) // need in case the common filter description matches
			{
				result = 0;
			}
			else if (ModuleFilter.CommonFilterDescription(filter1.Category) == filter1.Description)
			{
				result = -1;
			}
			else if (ModuleFilter.CommonFilterDescription(filter2.Category) == filter2.Description)
			{
				result = 1;
			}
			else
			{
				result = filter1.Description.CompareTo(filter2.Description);
			}

			return result;
		}

		List<FilterCategory> AllFilterCategories
		{
			get
			{
				var result = new List<FilterCategory>(AllFilters.Keys);
				result.Sort(FilterCategoryComparison);
				return result;
			}
		}

		int FilterCategoryComparison(FilterCategory category1, FilterCategory category2)
		{
			var result = 0;

			if (CategorySortOrder != null && category1 != category2)
			{
				for (var i = 0; i < CategorySortOrder.Length; i++)
				{
					if (CategorySortOrder[i] == category1)
					{
						result = -1;
						break;
					}
					else if (CategorySortOrder[i] == category2)
					{
						result = 1;
						break;
					}
				}
			}

			if (result == 0)
			{
				result = category1.Description.ToString().CompareTo(category2.Description.ToString());
			}

			return result;
		}

		CodeDescriptionPairList fFilter_List;

		public void ResetFilterList()
		{
			fFilter_List = null;
		}

		public FilterCategory[] CategorySortOrder;

		#endregion

		#region Filters + Duplicates

		Dictionary<FilterCategory, Dictionary<ZString, ModuleFilter>> AllFilters
		{
			get { return fAllFilters ?? (fAllFilters = new Dictionary<FilterCategory, Dictionary<ZString, ModuleFilter>>()); }
		}

		Dictionary<string, ModuleFilter> DuplicateFilters
		{
			get { return fDuplicateFilters ?? (fDuplicateFilters = new Dictionary<string, ModuleFilter>()); }
		}

		public ModuleFilter GetDuplicateFilter(string key)
		{
			DuplicateFilters.TryGetValue(key, out var filter);
			return filter;
		}

		public bool TryGetDuplicateFilter(string key, out ModuleFilter filter)
		{
			filter = GetDuplicateFilter(key);
			return filter != null;
		}

		public ModuleFilter AddNewDuplicateFilter(string newDescription, ModuleFilter filter)
		{
			Argument.NotNull(filter, nameof(filter));

			if (this[filter.Description] == null)
			{
				throw new ArgumentException("Duplicate Filter could not be added as original filter does not exist");
			}

			if (DuplicateFilters.ContainsKey(newDescription))
			{
				throw new ArgumentException("Invalid Key, already a Filter with this key in the DuplicateFilters Collection");
			}
			else
			{
				var newFilter = filter.ShallowCloneAndClearValues(newDescription);
				newFilter.IsDuplicateDefault = filter.IsDuplicateDefault;
				DuplicateFilters.Add(newDescription, newFilter);
				return newFilter;
			}
		}

		public ModuleFilter AddNewDuplicateFilter(ModuleFilter filter)
		{
			var newDescription = GetNextDuplicatedModuleFilterDescription(filter.Description);
			return AddNewDuplicateFilter(newDescription, filter);
		}

		Dictionary<FilterCategory, Dictionary<ZString, ModuleFilter>> fAllFilters;
		Dictionary<string, ModuleFilter> fDuplicateFilters;

		/// <summary>
		/// Add aliases for identifying filters here.
		/// </summary>
		public Dictionary<string, ModuleFilter> AliasFilters
		{
			get => fAliasFilters ?? (fAliasFilters = new Dictionary<string, ModuleFilter>());
		}
		Dictionary<string, ModuleFilter> fAliasFilters;

		#endregion

		#region Adding Filters

		#region AddFilter

		public void AddFilter(ModuleFilter filter)
		{
			var collection = GetOrCreateFilterCollectionByCategory(filter.Category);
			AddFilter(filter, collection);

			if (filter.IsCommon && !collection.ContainsKey(ModuleFilter.CommonFilterDescription(filter.Category)))
			{
				AddFilter(filter.GetNewCommonModuleFilter(filter.Category, this), collection);
			}
		}

		public void AddFilter(ModuleFilter filter, Dictionary<ZString, ModuleFilter> collection)
		{
			var existingFilter = this[filter.Description];
			if (existingFilter != null)
			{
				if (existingFilter.Category != null && existingFilter.Category.Description.Equals(FilterStripBusinessObject.CustomFieldCategoryDescription))
				{
					var newDescription = this.GetNextDuplicatedModuleFilterDescription(existingFilter.Description);
					var newFilter = existingFilter.ShallowCloneAndClearValues(newDescription, existingFilter.Visibility);
					RemoveFilter(existingFilter);
					AddFilter(newFilter);
				}
				else
				{
					throw new ArgumentException($"{AFilterAlreadyExistsWithDescriptionMessage} '{filter.Description}'; filter descriptions must be unique.");
				}
			}

			if (DisableFilterValidation)
			{
				filter.SuspendValidation();
			}
			collection.Add(filter.Description, filter);

			filter.IsActiveChanged -= new EventHandler<ModuleFilter.IsActiveChangedEventArgs>(filter_IsActiveChanged);
			filter.CategoryChanging -= new EventHandler<ModuleFilter.CategoryChangeEventArgs>(filter_CategoryChanging);
			filter.CategoryChanged -= new EventHandler<ModuleFilter.CategoryChangeEventArgs>(filter_CategoryChanged);
			filter.IsActiveChanged += new EventHandler<ModuleFilter.IsActiveChangedEventArgs>(filter_IsActiveChanged);
			filter.CategoryChanging += new EventHandler<ModuleFilter.CategoryChangeEventArgs>(filter_CategoryChanging);
			filter.CategoryChanged += new EventHandler<ModuleFilter.CategoryChangeEventArgs>(filter_CategoryChanged);

			FilterAdded?.Invoke(this, new FilterAddedEventArgs(filter));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Used for building exception message")]
		public const string AFilterAlreadyExistsWithDescriptionMessage = "A filter already exists with the description";

		public event EventHandler<FilterAddedEventArgs> FilterAdded;

		public class FilterAddedEventArgs : EventArgs
		{
			public ModuleFilter AddedFilter { get; }

			public FilterAddedEventArgs(ModuleFilter addedFilter)
			{
				AddedFilter = addedFilter;
			}
		}

		public void RemoveFilter(ModuleFilter filter)
		{
			AllFilters[filter.Category].Remove(filter.Description);
		}

		public Dictionary<ZString, ModuleFilter> GetOrCreateFilterCollectionByCategory(FilterCategory category)
		{
			Dictionary<ZString, ModuleFilter> result;

			if (AllFilters.ContainsKey(category))
			{
				result = AllFilters[category];
			}
			else
			{
				result = new Dictionary<ZString, ModuleFilter>();
				AllFilters.Add(category, result);
			}

			return result;
		}

		public event EventHandler<ModuleFilter.IsActiveChangedEventArgs> ElementIsActiveChanged;

		void filter_IsActiveChanged(object sender, ModuleFilter.IsActiveChangedEventArgs e)
		{
			OnElementIsActiveChanged(e);
		}

		void OnElementIsActiveChanged(ModuleFilter.IsActiveChangedEventArgs e)
		{
			if (ElementIsActiveChanged != null)
			{
				ElementIsActiveChanged(this, e);
			}

			InvalidateCachedQuery();
		}

		void filter_CategoryChanging(object sender, ModuleFilter.CategoryChangeEventArgs e)
		{
			RemoveFilter(e.Filter);
		}

		void filter_CategoryChanged(object sender, ModuleFilter.CategoryChangeEventArgs e)
		{
			AddFilter(e.Filter);
		}

		#endregion

		#region AddCustomFilter

		public void AddCustomFilter(ModuleFilter moduleFilter)
		{
			AddFilter(moduleFilter);
		}

		#endregion

		#region AddSqlFilter

		public ModuleSQLFilter AddSqlFilter(ZString description, Type type)
		{
			var result = new ModuleSQLFilter(description, type);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AddTextFilter

		public ModuleTextFilter AddTextFilter(ZString description, SchemaStringColumn filterColumn)
		{
			var result = new ModuleTextFilter(description, filterColumn);
			AddFilter(result);
			return result;
		}

		public ModuleTextFilter AddTextFilter(ZString description, SchemaStringColumn filterColumn, ComparisonOptions options)
		{
			var result = new ModuleTextFilter(description, filterColumn, options);
			AddFilter(result);
			return result;
		}

		public ModuleTextFilter AddTextFilter(ZString description, SchemaStringColumn filterColumn, IList list)
		{
			var result = new ModuleTextFilter(description, filterColumn, list);
			AddFilter(result);
			return result;
		}

		public ModuleTextFilter AddTextFilter(ZString description, SchemaStringColumn filterColumn, GetList listDelegate)
		{
			var result = new ModuleTextFilter(description, filterColumn, listDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleTextFilter AddTextFilter(ZString description, GetTextQueryWithOperator queryDelegate)
		{
			var result = new ModuleTextFilter(description, queryDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleTextFilter AddTextFilter(ZString description, GetTextQuery queryDelegate, IList list)
		{
			var result = new ModuleTextFilter(description, queryDelegate, list);
			AddFilter(result);
			return result;
		}

		public ModuleTextFilter AddTextFilter(ZString description, GetTextQueryWithOperator queryDelegate, IList list)
		{
			var result = new ModuleTextFilter(description, queryDelegate, list);
			AddFilter(result);
			return result;
		}

		public ModuleTextFilter AddTextFilter(ZString description, GetTextQuery queryDelegate, GetList listDelegate)
		{
			var result = new ModuleTextFilter(description, queryDelegate, listDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleTextFilter AddTextFilter(ZString description, GetTextQueryWithOperator queryDelegate, GetList listDelegate)
		{
			var result = new ModuleTextFilter(description, queryDelegate, listDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleTextFilter AddTranslatableTextFilter(ZString description, GetTextQueryWithOperator queryDelegate, MultilingualString multilingualDescription)
		{
			var result = AddTextFilter(description, queryDelegate);
			result.MultilingualDescription = multilingualDescription;
			return result;
		}

		public ModuleTextFilterForMultipleColumns AddTextFilterForMultipleColumns(ZString description, SchemaStringColumn filterColumn, params SchemaStringColumn[] additionalColumns)
		{
			var result = new ModuleTextFilterForMultipleColumns(description, filterColumn, additionalColumns);
			AddFilter(result);
			return result;
		}

		public ModuleTextFilter AddTextFilterForExactComparison(ZString description, GetTextQueryWithOperator queryDelegate)
		{
			var result = new ModuleTextFilter(description, queryDelegate);
			AddFilter(result);
			result.ComparisonOperator_List.Clear();
			result.ComparisonOperator_List.AddPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);

			return result;
		}

		public void AddFiltersForTranslatableText(ZString description, SchemaStringColumn filterColumn, Type businessObjectType, MultilingualString multilingualDescription)
		{
			AddFiltersForTranslatableText(description, filterColumn, (TranslatableDataFieldAttribute)Attribute.GetCustomAttribute(businessObjectType.GetProperty(filterColumn.Name), typeof(TranslatableDataFieldAttribute), true), multilingualDescription);
		}

		public void AddFiltersForTranslatableText(ZString description, SchemaStringColumn filterColumn, ICustomizableDataCaptionSource source, MultilingualString multilingualDescription)
		{
			if (Res.CurrentLanguage == Res.DefaultLanguage)
			{
				AddTextFilter(description, filterColumn).MultilingualDescription = multilingualDescription;
			}
			else
			{
				var customizable = new CustomizableDataResourceStrings(source);
				var languages = new CodeDescriptionPairList(OLookUpEditType.Language);
				AddTextFilter(description, filterColumn).MultilingualDescription = ResString.GetMultilingualString("FE684518-E4C2-4779-A518-B0E6893BEE1D", "{0} ({1})", multilingualDescription, languages.GetMultilingualDescriptionFromCode(Res.DefaultLanguage));
				var localLanguageFilter = new ModuleTranslatableTextFilter(description + "_Local", filterColumn, customizable);
				localLanguageFilter.MultilingualDescription = ResString.GetMultilingualString("FE684518-E4C2-4779-A518-B0E6893BEE1D", "{0} ({1})", multilingualDescription, languages.GetMultilingualDescriptionFromCode(Res.CurrentLanguage));
				AddFilter(localLanguageFilter);
			}
		}

		#endregion

		#region AddNkFilter

		public ModuleNkFilter AddNkFilter(ZString description, SchemaStringColumn nkFilterColumn, ModuleIdentifier iD, IBusinessObjectCollection list)
		{
			var result = new ModuleNkFilter(description, nkFilterColumn, iD, list);
			AddFilter(result);
			return result;
		}

		public ModuleNkFilter AddNkFilter(ZString description, GetNkQuery queryDelegate, ModuleIdentifier iD, IBusinessObjectCollection list)
		{
			var result = new ModuleNkFilter(description, queryDelegate, iD, list);
			AddFilter(result);
			return result;
		}

		public ModuleNkFilter AddNkFilter(ZString description, GetNkQueryWithOperator queryDelegate, ModuleIdentifier iD, IBusinessObjectCollection list)
		{
			var result = new ModuleNkFilter(description, queryDelegate, iD, list);
			AddFilter(result);
			return result;
		}

		public ModuleNkFilter AddNkFilter(ZString description, SchemaStringColumn nkFilterColumn, ModuleIdentifier iD, GetList listDelegate)
		{
			var result = new ModuleNkFilter(description, nkFilterColumn, iD, listDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleNkFilter AddNkFilter(ZString description, GetNkQueryWithOperator queryDelegate, ModuleIdentifier iD, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
		{
			var result = new ModuleNkFilter(description, queryDelegate, iD, listUsingCurrentModuleFilterDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleNkFilter AddNkFilter(ZString description, GetNkQueryWithOperatorSupportsFiltersMatch queryDelegate, ModuleIdentifier iD, IBusinessObjectCollection list)
		{
			var result = new ModuleNkFilter(description, queryDelegate, iD, list);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AddTextAndNkFilter

		public ModuleTextAndNkFilter AddTextAndNkFilter(ZString description, SchemaStringColumn textFilterColumn, ModuleIdentifier iD, SchemaStringColumn nkFilterColumn, IBusinessObjectCollection list)
		{
			var result = new ModuleTextAndNkFilter(description, textFilterColumn, nkFilterColumn, iD, list);
			AddFilter(result);
			return result;
		}

		public ModuleTextAndNkFilter AddTextAndNkFilter(ZString description, GetTextAndNkQuery queryDelegate, ModuleIdentifier iD, IBusinessObjectCollection list)
		{
			var result = new ModuleTextAndNkFilter(description, queryDelegate, iD, list);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AddNumberFilter

		public ModuleNumberFilter AddNumberFilter(ZString description, SchemaStringColumn filterColumn)
		{
			var result = new ModuleNumberFilter(description, filterColumn);
			AddFilter(result);
			return result;
		}

		public ModuleNumberFilter AddNumberFilterWithoutIsBlankAndIsNotBlank(ZString description, SchemaStringColumn filterColumn)
		{
			var result = AddNumberFilter(description, filterColumn);
			RemoveIsBlankAndIsNotBlank(result);
			return result;
		}

		public ModuleNumberFilter AddNumberFilter(ZString description, GetTextQueryWithOperator queryDelegate)
		{
			var result = new ModuleNumberFilter(description, queryDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleNumberFilter AddNumberFilterWithoutIsBlankAndIsNotBlank(ZString description, GetTextQueryWithOperator queryDelegate)
		{
			var result = AddNumberFilter(description, queryDelegate);
			RemoveIsBlankAndIsNotBlank(result);
			return result;
		}

		void RemoveIsBlankAndIsNotBlank(ModuleNumberFilter result)
		{
			result.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			result.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
		}

		#endregion

		#region AddPeriodFilter

		public ModulePeriodFilter AddPeriodFilter(ZString description, SchemaStringColumn filterColumn)
		{
			var result = new ModulePeriodFilter(description, filterColumn);
			AddFilter(result);
			return result;
		}

		public ModulePeriodFilter AddPeriodFilter(ZString description, GetTextQueryWithOperator queryDelegate)
		{
			var result = new ModulePeriodFilter(description, queryDelegate);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AddFountainFilter

		public ModuleFountainFilter AddFountainFilter(ZString description, SchemaStringColumn filterColumn, ZString fountainPrefix)
		{
			var result = new ModuleFountainFilter(description, filterColumn, fountainPrefix);
			AddFilter(result);
			return result;
		}

		public ModuleFountainFilter AddFountainFilter(ZString description, SchemaStringColumn filterColumn, ZString fountainPrefix, int fountainPaddingLength)
		{
			var result = new ModuleFountainFilter(description, filterColumn, fountainPrefix, fountainPaddingLength);
			AddFilter(result);
			return result;
		}

		public ModuleFountainFilter AddFountainFilter(ZString description, GetTextQueryWithOperator queryDelegate, ZString fountainPrefix)
		{
			var result = new ModuleFountainFilter(description, queryDelegate, fountainPrefix);
			AddFilter(result);
			return result;
		}

		public ModuleFountainFilter AddFountainFilter(ZString description, GetTextQueryWithOperator queryDelegate, ZString fountainPrefix, int fountainPaddingLength)
		{
			var result = new ModuleFountainFilter(description, queryDelegate, fountainPrefix, fountainPaddingLength);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AddSingleDateFilter

		public ModuleSingleDateFilter AddSingleDateFilter(ZString description, SchemaDateTimeColumn dateFilterColumn)
		{
			var result = new ModuleSingleDateFilter(description, dateFilterColumn);
			AddFilter(result);
			return result;
		}

		public ModuleSingleDateFilter AddSingleDateFilter(ZString description, GetSingleDateQuery queryDelegate)
		{
			var result = new ModuleSingleDateFilter(description, queryDelegate);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AddDateOffsetFilter

		public ModuleDateTimeOffsetFilter AddDateFilter(ZString description, SchemaDateTimeOffsetColumn dateFilterColumn)
		{
			return AddDateFilter(description, dateFilterColumn, false);
		}

		public ModuleDateTimeOffsetFilter AddDateFilter(ZString description, GetDateTimeOffsetQuery queryDelegate, bool isNullable = true)
		{
			return AddDateFilter(description, queryDelegate, false, isNullable);
		}

		public ModuleDateTimeOffsetFilter AddDateFilter(ZString description, SchemaDateTimeOffsetColumn dateFilterColumn, bool convertFromLocalToUTC)
		{
			var result = new ModuleDateTimeOffsetFilter(description, dateFilterColumn, convertFromLocalToUTC);
			AddFilter(result);
			return result;
		}

		public ModuleDateTimeOffsetFilter AddDateFilter(ZString description, GetDateTimeOffsetQuery queryDelegate, bool convertFromLocalToUTC, bool isNullable = true)
		{
			var result = new ModuleDateTimeOffsetFilter(description, queryDelegate, convertFromLocalToUTC, isNullable);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AddDateFilter

		public ModuleDateFilter AddDateFilter(ZString description, SchemaDateTimeColumn dateFilterColumn)
		{
			return AddDateFilter(description, dateFilterColumn, false);
		}

		public ModuleDateFilter AddDateFilter(ZString description, GetDateQuery queryDelegate, bool isNullable = true)
		{
			return AddDateFilter(description, queryDelegate, false, isNullable);
		}

		public ModuleDateFilter AddDateFilter(ZString description, SchemaDateTimeColumn dateFilterColumn, bool convertFromLocalToUTC)
		{
			var result = new ModuleDateFilter(description, dateFilterColumn, convertFromLocalToUTC);
			AddFilter(result);
			return result;
		}

		public ModuleDateFilter AddDateFilter(ZString description, GetDateQuery queryDelegate, bool convertFromLocalToUTC, bool isNullable = true)
		{
			var result = new ModuleDateFilter(description, queryDelegate, convertFromLocalToUTC, isNullable);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AddTimeFilter

		public ModuleTimeFilter AddTimeFilter(ZString description, SchemaDateTimeColumn dateFilterColumn)
		{
			var result = new ModuleTimeFilter(description, dateFilterColumn);
			AddFilter(result);
			return result;
		}

		public ModuleTimeFilter AddTimeFilter(ZString description, GetTimeQuery queryDelegate, bool isNullable = true)
		{
			var result = new ModuleTimeFilter(description, queryDelegate, isNullable);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AddGuidFilter

		public ModuleGuidFilter AddGuidFilter(ZString description, ModuleIdentifier iD, SchemaGuidColumn filterColumn, IBusinessObjectCollection list)
		{
			var result = new ModuleGuidFilter(description, iD, filterColumn, list);
			AddFilter(result);
			return result;
		}

		public ModuleGuidFilter AddGuidFilter(ZString description, ModuleIdentifier iD, GetGuidQuery queryDelegate, IBusinessObjectCollection list)
		{
			var result = new ModuleGuidFilter(description, iD, queryDelegate, list);
			AddFilter(result);
			return result;
		}

		public ModuleGuidFilter AddGuidFilter(ZString description, ModuleIdentifier iD, GetGuidQueryWithOperator queryDelegate, IBusinessObjectCollection list, SchemaColumn filterColumnForDelegate = null)
		{
			var result = new ModuleGuidFilter(description, iD, queryDelegate, list, filterColumnForDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleGuidFilter AddGuidFilter(ZString description, ModuleIdentifier iD, GetGuidQueryWithOperatorSupportsFiltersMatch queryDelegate, IBusinessObjectCollection list, SchemaColumn filterColumnForDelegate = null)
		{
			var result = new ModuleGuidFilter(description, iD, queryDelegate, list, filterColumnForDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleGuidFilter AddGuidFilter(ZString description, ModuleIdentifier iD, SchemaGuidColumn filterColumn, GetList listDelegate)
		{
			var result = new ModuleGuidFilter(description, iD, filterColumn, listDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleGuidFilter AddGuidFilter(ZString description, ModuleIdentifier iD, GetGuidQuery queryDelegate, GetList listDelegate)
		{
			var result = new ModuleGuidFilter(description, iD, queryDelegate, listDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleGuidFilter AddGuidFilter(ZString description, ModuleIdentifier iD, GetGuidQueryWithOperator queryDelegate, GetList listDelegate, SchemaColumn filterColumnForDelegate = null)
		{
			var result = new ModuleGuidFilter(description, iD, queryDelegate, listDelegate, filterColumnForDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleGuidFilter AddGuidFilter(ZString description, ModuleIdentifier iD, GetGuidQueryWithOperatorSupportsFiltersMatch queryDelegate, GetList listDelegate, SchemaColumn filterColumnForDelegate = null)
		{
			var result = new ModuleGuidFilter(description, iD, queryDelegate, listDelegate, filterColumnForDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleGuidsFilter AddGuidFilter(ZString description, ModuleIdentifier iD, SchemaGuidColumn filterColumn1, IBusinessObjectCollection list1, SchemaGuidColumn filterColumn2, IBusinessObjectCollection list2)
		{
			var result = new ModuleGuidsFilter(description, iD, filterColumn1, list1, filterColumn2, list2);
			AddFilter(result);
			return result;
		}

		public ModuleGuidsFilter AddGuidFilter(ZString description, ModuleIdentifier iD, GetGuidsQuery queryDelegate, IBusinessObjectCollection list1, IBusinessObjectCollection list2)
		{
			var result = new ModuleGuidsFilter(description, iD, queryDelegate, list1, list2);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AddGuidInSubCollectionFilter

		public ModuleGuidInSubCollectionFilter AddGuidInSubCollectionFilter(ZString description, FilterCategory category, ModuleIdentifier moduleID, GetGuidQueryWithNotIn queryDelegate, IBusinessObjectCollection collection)
		{
			var result = new ModuleGuidInSubCollectionFilter(description, category, moduleID, queryDelegate, collection);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AddLocationFilter

		public ModuleLocationFilter AddLocationFilter(ZString description, SchemaStringColumn location1FilterColumn, IBusinessObjectCollection location1List, SchemaStringColumn location2FilterColumn, IBusinessObjectCollection location2List, bool allowInternationalZones = false)
		{
			var result = new ModuleLocationFilter(description, location1FilterColumn, location1List, location2FilterColumn, location2List, allowInternationalZones);
			AddFilter(result);

			return result;
		}

		public ModuleLocationFilter AddLocationFilter(ZString description, GetCodeQuery queryDelegate, IBusinessObjectCollection location1List, IBusinessObjectCollection location2List)
		{
			var result = new ModuleLocationFilter(description, queryDelegate, location1List, location2List);
			AddFilter(result);

			return result;
		}

		#endregion

		#region AddFlagsFilter

		public ModuleFlagsFilter AddUnionOrOrFilter(ZString description, string flagName, SchemaBoolColumn flagFilterColumn, ModuleFilterSubGroup subGroup)
		{
			var result = new ModuleUnionOrOrFilter(description, flagName, flagFilterColumn, subGroup);
			AddFilter(result);
			return result;
		}

		public ModuleFlagsFilter AddRecompileFilter(ZString description, string flagName, SchemaBoolColumn flagFilterColumn, ModuleFilterSubGroup subGroup)
		{
			var result = new ModuleRecompileFilter(description, flagName, flagFilterColumn, subGroup);
			AddFilter(result);
			return result;
		}

		public ModuleFlagsFilter AddCardinalityFilter(ZString description, string flagName, SchemaBoolColumn flagFilterColumn, ModuleFilterSubGroup subGroup)
		{
			var result = new ModuleCardinalityFilter(description, flagName, flagFilterColumn, subGroup);
			AddFilter(result);
			return result;
		}

		public ModuleFlagsFilter AddFlagFilter(ZString description, string flagName, SchemaBoolColumn flagFilterColumn, ModuleFilterSubGroup subGroup)
		{
			var result = new ModuleFlagsFilter(description, flagName, flagFilterColumn, subGroup);
			AddFilter(result);
			return result;
		}

		public ModuleFlagsFilter AddFlagsFilter(ZString description, string[] flagNames, SchemaBoolColumn[] flagFilterColumns)
		{
			var result = new ModuleFlagsFilter(description, flagNames, flagFilterColumns);
			AddFilter(result);
			return result;
		}

		public ModuleFlagsFilter AddFlagsFilter(ZString description, string[] flagNames, GetFlagsQuery[] queryDelegates)
		{
			var result = new ModuleFlagsFilter(description, flagNames, queryDelegates);
			AddFilter(result);
			return result;
		}

		public ModuleFlagsFilter AddFlagsFilter(ZString description, string[] flagNames, GetFlagsQuery[] queryDelegates, JoinCondition joinConditionForQueryDelegates)
		{
			var result = new ModuleFlagsFilter(description, flagNames, queryDelegates, joinConditionForQueryDelegates);
			AddFilter(result);
			return result;
		}

		public void AddFlagsFilterWithFixedDescription(List<string> flagNames, List<SchemaBoolColumn> flagColumns)
		{
			var dic = new Dictionary<string, SchemaBoolColumn>();

			if (flagNames.Count == flagColumns.Count)
			{
#if NETFRAMEWORK
				dic = flagNames.Select((name, i) => (name, flagColumns[i])).DistinctBy(t => t.name).OrderBy(t => t.name).ToDictionary(t => t.name, t => t.Item2);
#else
				dic = Enumerable.DistinctBy(flagNames.Select((name, i) => (name, flagColumns[i])), t => t.name).OrderBy(t => t.name).ToDictionary(t => t.name, t => t.Item2);
#endif
			}

			if (dic.Count > ModuleFlagsFilter.MaximumFlagsCount)
			{
				for (int i = 0, groupIndex = 1; i < dic.Count; i += ModuleFlagsFilter.MaximumFlagsCount, groupIndex++)
				{
					AddFlagsFilter("Flags " + groupIndex,
						dic.Skip(i).Take(ModuleFlagsFilter.MaximumFlagsCount).Select(kv => kv.Key).ToArray(),
						dic.Skip(i).Take(ModuleFlagsFilter.MaximumFlagsCount).Select(kv => kv.Value).ToArray()) // No need to translate.
						.MultilingualDescription = ResString.GetMultilingualString("ee634b49-56e5-4eeb-bcef-977ba4e4637f", "Flags {0}", groupIndex);
				}
			}
			else
			{
				AddFlagsFilter("Flags", dic.Keys.ToArray(), dic.Values.ToArray()).MultilingualDescription = ResString.GetMultilingualString("f36ccfff-26bb-477b-bcd5-7885a0179eed", "Flags"); // No need to translate.
			}
		}

		#endregion

		#region AddNumberRangeFilter

		public ModuleNumberRangeSubFilter AddNumberRangeSubFilter(ZString description, SchemaNumericColumn filterColumn)
		{
			var result = new ModuleNumberRangeSubFilter(description, filterColumn);
			AddFilter(result);
			return result;
		}

		public ModuleNumberRangeFilter AddNumberRangeFilter(ZString description, SchemaNumericColumn filterColumn)
		{
			var result = new ModuleNumberRangeFilter(description, filterColumn);
			AddFilter(result);
			return result;
		}

		public ModuleNumberRangeFilter AddNumberRangeFilter(ZString description, GetNumberRangeQuery queryDelegate)
		{
			var result = new ModuleNumberRangeFilter(description, queryDelegate);
			AddFilter(result);
			return result;
		}

		public ModuleNumberRangeFilter AddNumberRangeFilter(ZString description, GetNumberRangeQuery queryDelegate, byte precision, byte scale)
		{
			var result = new ModuleNumberRangeFilter(description, queryDelegate, precision, scale);
			AddFilter(result);
			return result;
		}

		public ModuleNumberRangeFilter AddTranslatableNumberRangeFilter(ZString description, GetNumberRangeQuery queryDelegate, MultilingualString multilingualDescription)
		{
			var result = AddNumberRangeFilter(description, queryDelegate);
			result.MultilingualDescription = multilingualDescription;
			return result;
		}

		#endregion

		#region AddTextRangeFilter

		public ModuleTextRangeFilter AddTextRangeFilter(ZString description, SchemaStringColumn filterColumn)
		{
			var result = new ModuleTextRangeFilter(description, filterColumn);
			AddFilter(result);
			return result;
		}

		public ModuleTextRangeFilter AddTextRangeFilter(ZString description, GetTextRangeQuery queryDelegate)
		{
			var result = new ModuleTextRangeFilter(description, queryDelegate);
			AddFilter(result);
			return result;
		}

		#endregion

		#region AttributeFilters

		public delegate ZQuery AttributeFilterQuery(ZQuery queryToAppropriateTable, SchemaColumn columns);

		public void AddAttributeFilters(IEnumerable<CustomAttribute> attributes, AttributeFilterQuery getFilter)
		{
			AddAttributeFilters(attributes, getFilter, FilterCategories.AttributeSearch);
		}

		public void AddAttributeFilters(IEnumerable<CustomAttribute> attributes, AttributeFilterQuery getFilter, FilterCategory filterCategory)
		{
			var textMembers = new List<SchemaStringColumn>();

			foreach (var attribute in attributes)
			{
				if (!attribute.Key.IsEmpty && this[attribute.Key] == null)
				{
					ModuleFilter filter = null;

					if (attribute.Column is SchemaStringColumn)
					{
						filter = AddTextFilter(attribute.Key, (comparisonOperator, value) => getFilter(new ZQuery(attribute.Column, comparisonOperator, value), attribute.Column));
						if (attribute.Column.HasMaxLength)
						{
							filter.MaxLength = attribute.Column.MaxLength;
						}
						textMembers.Add((SchemaStringColumn)attribute.Column);
					}
					else if (attribute.Column is SchemaDateTimeColumn)
					{
						filter = AddDateFilter(attribute.Key, (comparisonOperator, date1, date2) => getFilter(DateFilter(comparisonOperator, date1, date2, attribute.Column), attribute.Column));
					}
					else if (attribute.Column is SchemaBoolColumn)
					{
						filter = AddFlagsFilter(attribute.Key, new[] { Res.GetString("e1777070-b09f-4336-8e25-b5f324321b49", "Show {0}", attribute.Caption) },
							new GetFlagsQuery[] { zBool => getFilter(new ZQuery(attribute.Column, SQLComparisonOperator.Equal, zBool), attribute.Column) });
					}
					else if (attribute.Column is SchemaDecimalColumn)
					{
						filter = AddNumberRangeFilter(attribute.Key, (numValue1, numValue2) => getFilter(NumberRangeFilter(numValue1, numValue2, attribute.Column), attribute.Column), ((SchemaDecimalColumn)attribute.Column).Precision, ((SchemaDecimalColumn)attribute.Column).Scale);
					}

					if (filter != null)
					{
						filter.Category = filterCategory;
						filter.IsAttributeFilter = true;
						filter.MultilingualDescription = attribute.Caption;
					}
				}
			}

			if (textMembers.Count > 1)
			{
				ModuleFilter filter = AddTextFilter(string.Format(AnyTextAttr, filterCategory.Description.GetUnresolvedString().Replace(FilterCategories.AttributeSearch.Description.GetUnresolvedString(), String.Empty)),
					(comparisonOperator, value) => GetMultiColumnTextFilter(comparisonOperator, value, getFilter, textMembers.ToArray()));
				filter.Category = filterCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("ZArchitecture|ModuleFilterCollection|AnyTextAttribute", "Any Text Attribute");
				filter.IsAttributeFilter = true;
				filter.MaxLength = textMembers.Max(x => x.MaxLength);
			}
		}

		public void AddAttributeFilters(IEnumerable<CustomAttribute> attributes, FilterCategory filterCategory, ModuleFilterSubGroup subGroup)
		{
			Argument.NotNull(subGroup, nameof(subGroup));

			var textMembers = new List<SchemaStringColumn>();

			foreach (var attribute in attributes)
			{
				if (!attribute.Key.IsEmpty && this[attribute.Key] == null)
				{
					ModuleFilter filter = null;

					if (attribute.Column is SchemaStringColumn)
					{
						filter = AddTextFilter(attribute.Key, (SchemaStringColumn)attribute.Column);
						if (attribute.Column.HasMaxLength)
						{
							filter.MaxLength = attribute.Column.MaxLength;
						}
						textMembers.Add((SchemaStringColumn)attribute.Column);
					}
					else if (attribute.Column is SchemaDateTimeColumn)
					{
						filter = AddDateFilter(attribute.Key, (SchemaDateTimeColumn)attribute.Column);
					}
					else if (attribute.Column is SchemaBoolColumn)
					{
						filter = AddFlagFilter(attribute.Key, Res.GetString("e1777070-b09f-4336-8e25-b5f324321b49", "Show {0}", attribute.Caption), (SchemaBoolColumn)attribute.Column, subGroup);
					}
					else if (attribute.Column is SchemaDecimalColumn)
					{
						filter = AddNumberRangeFilter(attribute.Key, (SchemaDecimalColumn)attribute.Column);
					}

					if (filter != null)
					{
						filter.Category = filterCategory;
						filter.IsAttributeFilter = true;
						filter.MultilingualDescription = attribute.Caption;
						if (subGroup != null)
						{
							filter.SubGroup = subGroup;
						}
					}
				}
			}

			if (textMembers.Count > 1)
			{
				ModuleFilter filter = AddTextFilter(string.Format(CultureInfo.InvariantCulture, AnyTextAttr, filterCategory.Description.GetUnresolvedString().Replace(FilterCategories.AttributeSearch.Description.GetUnresolvedString(), String.Empty)),
					(comparisonOperator, value) => GetMultiColumnTextFilter(comparisonOperator, value, textMembers.ToArray()));
				filter.Category = filterCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("ZArchitecture|ModuleFilterCollection|AnyTextAttribute", "Any Text Attribute");
				filter.IsAttributeFilter = true;
				filter.SubGroup = subGroup;
				filter.MaxLength = textMembers.Max(x => x.MaxLength);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant used in numerous places")]
		public const string AnyTextAttr = "Any {0}Text Attribute";

		#region Queries

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected ZQuery GetMultiColumnTextFilter(SQLComparisonOperator comparisonOperator, ZString value, params SchemaStringColumn[] columns)
		{
			var validQueryColumns = FilterInvalidQueryColumns(value, columns);
			var result = new ZQuery(validQueryColumns[0], comparisonOperator, value);

			for (var i = 1; i < validQueryColumns.Length; i++)
			{
				result.AddToFilter(new ZQuery(validQueryColumns[i], comparisonOperator, value), JoinCondition.Or);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected ZQuery GetMultiColumnTextFilter(SQLComparisonOperator comparisonOperator, ZString value, AttributeFilterQuery getAttributeFilter, params SchemaStringColumn[] columns)
		{
			var validQueryColumns = FilterInvalidQueryColumns(value, columns);
			var result = getAttributeFilter(new ZQuery(validQueryColumns[0], comparisonOperator, value), validQueryColumns[0]);

			for (var i = 1; i < validQueryColumns.Length; i++)
			{
				result.AddToFilter(getAttributeFilter(new ZQuery(validQueryColumns[i], comparisonOperator, value), validQueryColumns[i]), JoinCondition.Or);
			}

			return result;
		}

		SchemaStringColumn[] FilterInvalidQueryColumns(ZString value, params SchemaStringColumn[] columns)
		{
			return columns.Where(x => value.Length <= x.MaxLength).ToArray();
		}

		ZQuery DateFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2, SchemaColumn column)
		{
			var result = new ZQuery();

			if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				if (column.IsNullable)
				{
					result.AddToFilter(column, SQLComparisonOperator.NotEqual, null);
				}
			}
			else if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				if (column.IsNullable)
				{
					result.AddToFilter(column, SQLComparisonOperator.Equal, null);
				}
				else
				{
					result.IsNoResultQuery = true;
				}
			}
			else
			{
				if (date1.IsValid)
				{
					result.AddToFilter(column, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date1);
				}

				if (date2.IsValid)
				{
					result.AddToFilter(column, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date2);
				}
			}

			return result;
		}

		ZQuery NumberRangeFilter(INumericZType number1, INumericZType number2, SchemaColumn column)
		{
			var result = new ZQuery();

			if (number1.IsValid)
			{
				result.AddToFilter(column, SQLComparisonOperator.GreaterThanOrEqualTo, number1);
			}

			if (number2.IsValid)
			{
				result.AddToFilter(column, SQLComparisonOperator.LessThanOrEqualTo, number2);
			}

			return result;
		}

		#endregion

		#endregion

		#endregion

		#region Filter Query

		public ZQuery GetFilterQuery(IEnumerable<ModuleFilter> activeModuleFiltersForQuery, FilterGroupQueryMapAction forGroups = null)
		{
			var filters = activeModuleFiltersForQuery.ToArray();

			if (IsQueryStale(filters))
			{
				CachedQuery = GetFilterQueryCore(filters, forGroups, null);
				cachedQueryCreationTimeUtc = ZDateTime.UtcNow;
			}

			return CachedQuery.DeepClone();
		}

		public ZQuery GetFilterQuery(IEnumerable<ModuleFilter> activeModuleFiltersForQuery, FilterGroupQueryMapAction forGroups, IEnumerable<BusinessObject> bizosToApplyFiltersTo)
		{
			var filters = activeModuleFiltersForQuery.ToArray();
			return GetFilterQueryCore(filters, forGroups, bizosToApplyFiltersTo).DeepClone();
		}

		public bool IsQueryStale(IEnumerable<ModuleFilter> activeModuleFiltersForQuery)
		{
			return CachedQuery == null || !SupportsQueryCaching || activeModuleFiltersForQuery.Any(x => x.IsQueryStale || x.CachedQueryCreationTimeUtc > cachedQueryCreationTimeUtc);
		}

		protected ZQuery CachedQuery { get; set; }

		ZDateTime cachedQueryCreationTimeUtc;

		public void InvalidateCachedQuery()
		{
			CachedQuery = null;
		}

		public bool SupportsQueryCaching { get; set; } = true;

		protected virtual ZQuery GetFilterQueryCore(IEnumerable<ModuleFilter> activeModuleFiltersForQuery, FilterGroupQueryMapAction forGroups, IEnumerable<BusinessObject> bizosToApplyFiltersTo)
		{
			var filtersRequiringSequenceHelper = activeModuleFiltersForQuery.OfType<IParameterSequenceHelperRequired>().ToArray();
			var parameterSequenceHelper = filtersRequiringSequenceHelper.FirstOrDefault()?.GetParameterSequenceHelper();

			if (filtersRequiringSequenceHelper.Any())
			{
				parameterSequenceHelper = parameterSequenceHelper ?? new ParameterSequenceHelper();
				parameterSequenceHelper.BeginSequence();

				foreach (var filter in filtersRequiringSequenceHelper)
				{
					filter.SetParameterSequenceHelper(parameterSequenceHelper);
				}
			}

			var result = ModuleFilterCombiner.GetCombinedFilter(activeModuleFiltersForQuery, forGroups, bizosToApplyFiltersTo);

			parameterSequenceHelper?.EndSequence();

			return result;
		}

		ModuleFilterCombiner ModuleFilterCombiner
		{
			get { return moduleFilterCombiner ?? (moduleFilterCombiner = new ModuleFilterCombiner()); }
		}

		ModuleFilterCombiner moduleFilterCombiner;

		#endregion

		#region IEnumerable<ModuleFilter> Members

		IEnumerator<ModuleFilter> IEnumerable<ModuleFilter>.GetEnumerator()
		{
			foreach (var collection in AllFilters.Values)
			{
				foreach (var filter in collection.Values)
				{
					yield return filter;
				}
			}

			foreach (var filter in DuplicateFilters.Values)
			{
				yield return filter;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ModuleFilter>)this).GetEnumerator();
		}

		#endregion

		#region ShallowClone

		public ModuleFilterCollection ShallowClone()
		{
			var result = new ModuleFilterCollection();
			foreach (var filter in this)
			{
				if (!filter.IsCommonModuleFilter)
				{
					var clonedFilter = filter.ShallowClone();
					clonedFilter.IsActive = false;
					result.AddFilter(clonedFilter, result.GetOrCreateFilterCollectionByCategory(filter.Category));
				}
			}
			return result;
		}

		#endregion

		#region ToSortedArrayWithIsExclusiveLast

		public ModuleFilter[] ToSortedArrayWithIsExclusiveLast()
		{
			var moduleFilters = new List<ModuleFilter>();
			var moduleFiltersExclusive = new List<ModuleFilter>();
			foreach (var d in AllFilters.Values)
			{
				foreach (var moduleFilter in d.Values)
				{
					if (moduleFilter.IsExclusive)
					{
						moduleFiltersExclusive.Add(moduleFilter);
					}
					else
					{
						moduleFilters.Add(moduleFilter);
					}
				}
			}
			foreach (var filter in moduleFiltersExclusive)
			{
				moduleFilters.Add(filter);
			}
			return moduleFilters.ToArray();
		}

		#endregion
	}
}
