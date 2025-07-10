using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class FormFilterBuisnessObject<T> : FilterStripBusinessObject where T : BusinessObject
	{
		protected FormFilterBuisnessObject(ModuleIdentifier moduleIdentifier) : this(moduleIdentifier?.ID.ToString() ?? string.Empty)
		{
		}

		protected FormFilterBuisnessObject(ZString moduleId)
		{
			if (moduleId.IsEmpty)
			{
				throw new ArgumentException("moduleId cannot be empty.", nameof(moduleId));
			}

			FuncDictionary = new Dictionary<ZString, Func<ModuleFilter, T, bool>>();
			((IFilterStripBusinessObjectInternals)this).LayoutContext = moduleId;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected readonly Dictionary<ZString, Func<ModuleFilter, T, bool>> FuncDictionary;

		public Predicate<T> Predicate
		{
			get
			{
				return businessObject =>
				{
					var result = true;
					var filterGroups = ActiveModuleFiltersForQuery.GroupBy(x => x.OrCategory);
					foreach (var filterGroup in filterGroups)
					{
						var matchFilterGroup = MatchFilterGroup(businessObject, filterGroup, filterGroup.Key == FilterOrCategory.None);
						result = result && matchFilterGroup;
					}
					return result;
				};
			}
		}

		bool MatchFilterGroup(T businessObject, IEnumerable<ModuleFilter> filterGroup, bool isOperatorAND)
		{
			var result = isOperatorAND;//If AND, start with true as it will be && as below
			Func<bool, bool, bool> evaluate = (x, y) => isOperatorAND ? x && y : x || y;

			foreach (var moduleFilter in filterGroup)
			{
				Func<ModuleFilter, T, bool> func;
				if (FuncDictionary.TryGetValue(moduleFilter.OriginalCode, out func))
				{
					result = evaluate(result, func.Invoke(moduleFilter, businessObject));
				}
			}
			return result;
		}

		protected override bool ShouldAddCustomSqlFilter { get { return false; } }

		protected sealed override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddOrUpdateFunc(filters);
			return filters;
		}

		protected abstract void AddOrUpdateFunc(ModuleFilterCollection filters);

		protected void UpdateTextFunc(ZString description, Func<T, ZString> textSelector)
		{
			if (FuncDictionary.ContainsKey(description))
			{
				FuncDictionary[description] = (filter, businessObject) => GetTextFunc(filter, businessObject, textSelector);
			}
		}

		protected ModuleTextFilter AddTextFunc(ModuleFilterCollection filters, ZString description, Func<T, ZString> textSelector)
		{
			return AddTextFunc(filters, description, (filter, businessObject) => GetTextFunc(filter, businessObject, textSelector));
		}

		protected ModuleTextFilter AddTextFunc(ModuleFilterCollection filters, ZString description, Func<ModuleFilter, T, bool> func)
		{
			var filter = filters.AddTextFilter(description, (comparisonoperator, value) => new ZQuery());
			AddOrUpdateFuncDict(description, func);
			return filter;
		}

		protected void AddTranslatableTextFunc(ModuleFilterCollection filters, ZString description, Func<T, ZString> textSelector, MultilingualString multilingualDescription)
		{
			AddTranslatableTextFunc(filters, description, (filter, businessObject) => GetTextFunc(filter, businessObject, textSelector), multilingualDescription);
		}

		protected void AddTranslatableTextFunc(ModuleFilterCollection filters, ZString description, Func<ModuleFilter, T, bool> func, MultilingualString multilingualDescription)
		{
			filters.AddTranslatableTextFilter(description, (comparisonoperator, value) => new ZQuery(), multilingualDescription);
			AddOrUpdateFuncDict(description, func);
		}

		protected void AddTranslatableTextFunc(ModuleFilterCollection filters, ZString description, Func<T, IEnumerable<ZString>> textSelector, MultilingualString multilingualDescription)
		{
			AddTranslatableTextFunc(filters, description, (filter, businessObject) => GetTextFunc(filter, businessObject, textSelector), multilingualDescription);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void AddNumberRangeFunc(ModuleFilterCollection filters, ZString description, Func<T, ZDecimal?> numberSelector)
		{
			AddNumberRangeFunc(filters, description, (filter, businessObject) => GetNumberRangeFunc(filter, businessObject, numberSelector));
		}

		protected void AddNumberRangeFunc(ModuleFilterCollection filters, ZString description, Func<ModuleFilter, T, bool> func)
		{
			filters.AddNumberRangeFilter(description, (value1, value2) => new ZQuery());
			AddOrUpdateFuncDict(description, func);
		}

		protected void AddTranslatableNumberRangeFunc(ModuleFilterCollection filters, ZString description, Func<T, ZDecimal?> numberSelector, MultilingualString multilingualDescription)
		{
			AddTranslatableNumberRangeFunc(filters, description, (filter, businessObject) => GetNumberRangeFunc(filter, businessObject, numberSelector), multilingualDescription);
		}

		protected void AddTranslatableNumberRangeFunc(ModuleFilterCollection filters, ZString description, Func<ModuleFilter, T, bool> func, MultilingualString multilingualDescription)
		{
			filters.AddTranslatableNumberRangeFilter(description, (value1, value2) => new ZQuery(), multilingualDescription);
			AddOrUpdateFuncDict(description, func);
		}

		protected void AddCustomFilterFunc(ModuleFilterCollection filters, ModuleFilter filter, Func<ModuleFilter, T, bool> func)
		{
			filters.AddCustomFilter(filter);
			AddOrUpdateFuncDict(filter.Description, func);
		}

		void AddOrUpdateFuncDict(ZString description, Func<ModuleFilter, T, bool> func)
		{
			if (FuncDictionary.ContainsKey(description))
			{
				FuncDictionary[description] = func;
			}
			else
			{
				FuncDictionary.Add(description, func);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected bool GetNumberRangeFunc(ModuleFilter moduleFilter, T businessObject, Func<T, ZDecimal?> numberSelector)
		{
			var result = true;
			var number = numberSelector.Invoke(businessObject);
			var moduleNumberRangeFilter = (ModuleNumberRangeFilter)moduleFilter;
			var filterProperty1 = moduleNumberRangeFilter.Property1;
			var filterProperty2 = moduleNumberRangeFilter.Property2;
			if (number.HasValue)
			{
				if (moduleNumberRangeFilter.IsBetweenSearch)
				{
					result = (filterProperty1.IsEmpty && filterProperty2.IsEmpty) ||
							(number >= filterProperty1 && number <= filterProperty2);
				}
				else if (moduleNumberRangeFilter.IsEqualToSearch)
				{
					result = filterProperty1.IsEmpty || number == filterProperty1;
				}
				else if (moduleNumberRangeFilter.IsGreaterThanOrEqualToSearch)
				{
					result = filterProperty1.IsEmpty || number >= filterProperty1;
				}
				else if (moduleNumberRangeFilter.IsLessThanOrEqualToSearch)
				{
					result = filterProperty2.IsEmpty || number <= filterProperty2;
				}
			}
			else
			{
				result = false;
			}
			return result;
		}

		protected bool GetTextFunc(ModuleFilter moduleFilter, T businessObject, Func<T, ZString> textSelector)
		{
			return GetTextFunc(moduleFilter, businessObject, o => new ZString[] { textSelector.Invoke(o) });
		}

		protected bool GetTextFunc(ModuleFilter moduleFilter, T businessObject, Func<T, IEnumerable<ZString>> textSelector)
		{
			var result = true;
			var currentValues = textSelector.Invoke(businessObject);
			var moduleTextFilter = (ModuleTextFilter)moduleFilter;
			var comparisonoperator = moduleTextFilter.SqlComparisonOperator;
			var filterProperty = moduleTextFilter.Property;

			if (comparisonoperator == SpecialComparisonOperator.IsBlank)
			{
				result = !currentValues.Any() || currentValues.Any(x => x.IsEmpty);
			}
			else if (comparisonoperator == SpecialComparisonOperator.IsNotBlank)
			{
				result = currentValues.Any(x => !x.IsEmpty);
			}
			else
			{
				result = filterProperty.IsEmpty || currentValues.Any(x => MatchFilter(x, comparisonoperator, filterProperty));
			}
			return result;
		}

		bool MatchFilter(ZString currentValue, SQLComparisonOperator comparisonoperator, ZString filterProperty)
		{
			var result = false;
			if (comparisonoperator == SQLComparisonOperator.Equal)
			{
				result = currentValue.EqualsIgnoringCase(filterProperty);
			}
			else if (comparisonoperator == SQLComparisonOperator.StartsWith)
			{
				result = currentValue.StartsWith(filterProperty, StringComparison.CurrentCultureIgnoreCase);
			}
			else if (comparisonoperator == SQLComparisonOperator.Contains)
			{
				result = currentValue.Contains(filterProperty, StringComparison.CurrentCultureIgnoreCase);
			}
			else if (comparisonoperator == SQLComparisonOperator.NotEqual)
			{
				result = !currentValue.EqualsIgnoringCase(filterProperty);
			}
			else if (comparisonoperator == SQLComparisonOperator.DoesNotStartWith)
			{
				result = !currentValue.StartsWith(filterProperty, StringComparison.CurrentCultureIgnoreCase);
			}
			else if (comparisonoperator == SQLComparisonOperator.NotContains)
			{
				result = !currentValue.Contains(filterProperty, StringComparison.CurrentCultureIgnoreCase);
			}
			return result;
		}
	}
}
