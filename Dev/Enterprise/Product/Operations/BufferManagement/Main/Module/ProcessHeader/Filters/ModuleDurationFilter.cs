using System;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Module
{
	public class ModuleDurationFilter : ModuleFilter
	{
		ZString scope;
		ZDecimal minDurationMinutes;
		ZDecimal maxDurationMinutes;
		const double minuteValueLimit = (999 + (1 / 60d * 59)) * 60;

		public ModuleDurationFilter(string filterCode, BusinessObjectFactory factory)
			: base(filterCode, factory)
		{
			scope = SearchTexts.Between;
		}

		public ModuleDurationFilter(string filterCode, BusinessObjectFactory factory, SchemaColumn filterColumn)
			: base(filterCode, filterColumn)
		{
			if (FilterColumn != null)
			{
				QueryDelegate = CreateColumnBoundQuery;
			}

			scope = SearchTexts.Between;
		}

		protected ModuleDurationFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		[List("ScopeList")]
		public ZString Scope
		{
			get { return scope; }
			set
			{
				if (Scope != value)
				{
					InvalidateCachedQuery();
				}
				SetNonPersistentPropertyValue(ScopeInfo, ref scope, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateScope();
				}
			}
		}

		public CodeDescriptionPairList ScopeList
		{
			get
			{
				if (scopeList == null)
				{
					scopeList = CreateScopeListCore();
				}
				return scopeList;
			}
		}

		CodeDescriptionPairList scopeList;

		protected virtual CodeDescriptionPairList CreateScopeListCore()
		{
			var searchList = new CodeDescriptionPairList();
			searchList.AddPair(SearchTexts.GreaterThanOrEqualTo, Res.GetString("Filter|DurationSearchList|GreaterThanFilterDescription", "Search for fields that have a duration greater than or equal to the supplied duration"));
			searchList.AddPair(SearchTexts.LessThanOrEqualTo, Res.GetString("Filter|DurationSearchList|LessThanFilterDescription", "Search for fields that have a duration less than or equal to the supplied duration"));
			searchList.AddPair(SearchTexts.Between, Res.GetString("Filter|DurationSearchList|BetweenFilterDescription", "Search for fields that have a duration between the two supplied durations"));
			searchList.AddPair(SearchTexts.EqualTo, Res.GetString("Filter|DurationSearchList|EqualToFilterDescription", "Search for fields that have a duration equal to the supplied duration"));

			return searchList;
		}

		public ZPropertyInfo ScopeInfo => GetZPropertyInfo(nameof(Scope));

		[ResourceStringData("ModuleDurationFilter.MinDuration", Caption = "Min Duration")]
		public ZDateTime MinDuration
		{
			get => TimeSpan.FromMinutes((double)MinDurationMinutes);
			set => MinDurationMinutes = value.GetMinutesFromDateTimeSpan();
		}

		public void SetQueryDelegate(Func<ZDecimal, ZDecimal, ZString, ZQuery> queryDelegate)
		{
			QueryDelegate = queryDelegate;
			InvalidateCachedQuery();
		}

		public ZDecimal MinDurationMinutes
		{
			get { return minDurationMinutes; }
			set
			{
				if (MinDurationMinutes != value)
				{
					InvalidateCachedQuery();
				}
				SetNonPersistentPropertyValue(MinDurationMinutesInfo, ref minDurationMinutes, value > minuteValueLimit ? minuteValueLimit : value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMinDurationMinutes();
				}
			}
		}

		public ZPropertyInfo MinDurationMinutesInfo => GetZPropertyInfo(nameof(MinDurationMinutes));

		[ResourceStringData("ModuleDurationFilter.MaxDuration", Caption = "Max Duration")]
		public ZDateTime MaxDuration
		{
			get => TimeSpan.FromMinutes((double)MaxDurationMinutes);
			set => MaxDurationMinutes = value.GetMinutesFromDateTimeSpan();
		}

		public ZDecimal MaxDurationMinutes
		{
			get { return maxDurationMinutes; }
			set
			{
				if (MaxDurationMinutes != value)
				{
					InvalidateCachedQuery();
				}
				SetNonPersistentPropertyValue(MaxDurationMinutesInfo, ref maxDurationMinutes, value > minuteValueLimit ? minuteValueLimit : value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMaxDurationMinutes();
				}
			}
		}

		public ZPropertyInfo MaxDurationMinutesInfo => GetZPropertyInfo(nameof(MaxDurationMinutes));

		ZQuery CreateColumnBoundQuery(ZDecimal minMinutes, ZDecimal maxMinutes, ZString scope)
		{
			if (FilterColumn == null)
			{
				return new ZDBOnlyQuery(typeof(ProcessHeader));
			}

			var minValue = (ZInt)minMinutes;
			var maxValue = (ZInt)maxMinutes;

			var query = new ZQuery();

			switch (scope.ToString())
			{
				case var s when s == SearchTexts.Between.ToString():
					query.AddToFilter(FilterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, minValue);
					query.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualTo, maxValue);
					break;
				case var s when s == SearchTexts.GreaterThanOrEqualTo.ToString():
					query.AddToFilter(FilterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, minValue);
					break;
				case var s when s == SearchTexts.LessThanOrEqualTo.ToString():
					query.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualTo, minValue);
					break;
				case var s when s == SearchTexts.EqualTo.ToString():
					query.AddToFilter(FilterColumn, SQLComparisonOperator.Equal, minValue);
					break;
				default:
					query.AddToFilter(FilterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, minValue);
					query.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualTo, maxValue);
					break;
			}

			return query;
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			if (QueryDelegate != null)
			{
				return (ZQuery)QueryDelegate.DynamicInvoke(MinDurationMinutes, MaxDurationMinutes, Scope);
			}

			if (FilterColumn != null)
			{
				return CreateColumnBoundQuery(MinDurationMinutes, MaxDurationMinutes, Scope);
			}

			return new ZQuery();
		}

		protected override FilterCategory DefaultCategory => ProcessHeaderFilterBusinessObject.TasksFilterCategory;

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			var newFilter = new ModuleDurationFilter(category, parentCollection);
			return newFilter;
		}

		protected override object[] QueryDelegateParameters => new object[] { MinDurationMinutes, MaxDurationMinutes, Scope };

		protected override bool IsEmptyCore => MinDurationMinutes.IsEmpty && MaxDurationMinutes.IsEmpty && Scope.IsEmpty;

		public override bool IsExpensiveQuery => false;

		protected override void ClearCore()
		{
			MinDurationMinutes = 0;
			MaxDurationMinutes = 0;
			Scope = ZString.Empty;
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			if (filterToCopyFrom is ModuleDurationFilter filter)
			{
				MinDurationMinutes = filter.MinDurationMinutes;
				MaxDurationMinutes = filter.MaxDurationMinutes;
				Scope = filter.Scope;

				if (filter.QueryDelegate != null)
				{
					this.QueryDelegate = filter.QueryDelegate;
				}
			}
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("Scope", Scope);
			writer.WriteElementString("MinDurationMinutes", MinDurationMinutes.ToString());
			writer.WriteElementString("MaxDurationMinutes", MaxDurationMinutes.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			Scope = reader.ReadElementString("Scope");
			MinDurationMinutes = ZDecimal.ParseSafe(reader.ReadElementString("MinDurationMinutes"), 0.0);
			MaxDurationMinutes = ZDecimal.ParseSafe(reader.ReadElementString("MaxDurationMinutes"), 0.0);
		}

		public new ModuleDurationFilterValidation Validation => (ModuleDurationFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation() => new ModuleDurationFilterValidation(this);

		public class ModuleDurationFilterValidation : ModuleFilterValidation
		{
			readonly ModuleDurationFilter parent;

			public ModuleDurationFilterValidation(ModuleDurationFilter parent) : base(parent)
			{
				this.parent = parent;
			}

			public void ValidateScope()
			{
				ValidateCalculatedProperty(parent.ScopeInfo);
			}

			protected virtual void CheckScope()
			{
				MandatoryValidation.CheckEntered(parent.ScopeInfo);
				ListValidation.ErrorIfInvalidCode(parent.ScopeInfo);
			}

			public void ValidateMinDurationMinutes()
			{
				ValidateCalculatedProperty(parent.MinDurationMinutesInfo);
				ValidateCalculatedProperty(parent.MaxDurationMinutesInfo);
			}

			protected virtual void CheckMinDurationMinutes()
			{
				if (parent.MinDurationMinutes < 0)
				{
					parent.MinDurationMinutesInfo.AddError(Res.GetString("48b5becd-f98c-4897-98aa-be37eecb6a68", "Please enter a value greater than or equal to 0."));
				}
				CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(parent.MinDurationMinutesInfo, parent.MaxDurationMinutesInfo);
			}

			public void ValidateMaxDurationMinutes()
			{
				ValidateCalculatedProperty(parent.MaxDurationMinutesInfo);
			}

			protected virtual void CheckMaxDurationMinutes()
			{
				CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(parent.MaxDurationMinutesInfo, parent.MinDurationMinutesInfo);
			}

			public override void ValidateAll()
			{
				ValidateScope();
				ValidateMinDurationMinutes();
				ValidateMaxDurationMinutes();
			}

			public override Type AutoValidationType => GetType();
		}

		public static class SearchTexts
		{
			public static MultilingualString GreaterThanOrEqualTo => ResString.GetMultilingualString("Filter|ModuleDurationSearchList|GreaterThanFilter", "Greater than or equal to");
			public static MultilingualString LessThanOrEqualTo => ResString.GetMultilingualString("Filter|ModuleDurationSearchList|LessThanFilter", "Less than or equal to");
			public static MultilingualString Between => ResString.GetMultilingualString("Filter|ModuleDurationSearchList|BetweenFilter", "Between");
			public static MultilingualString EqualTo => ResString.GetMultilingualString("Filter|ModuleDurationSearchList|EqualToFilter", "Equal to");
		}

#if DEBUG
		protected override void FillWithValidTestFilterValueCore()
		{
		}
#endif
	}
}
