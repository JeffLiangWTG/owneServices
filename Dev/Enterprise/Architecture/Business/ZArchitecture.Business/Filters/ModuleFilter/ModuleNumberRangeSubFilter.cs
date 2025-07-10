using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class ModuleNumberRangeSubFilter : ModuleNumberRangeFilter
	{
		public ModuleNumberRangeSubFilter(ZString description, SchemaNumericColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);

			var filter = (ModuleNumberRangeSubFilter)filterToCopyFrom;
			greaterThanDefaultProperty = filter.greaterThanDefaultProperty;
		}

		public ZDecimal GreaterThanDefaultProperty
		{
			get
			{
				if (greaterThanDefaultProperty < MinValue)
				{
					greaterThanDefaultProperty = MinValue;
				}
				else if (greaterThanDefaultProperty > MaxValue)
				{
					greaterThanDefaultProperty = MaxValue;
				}

				return greaterThanDefaultProperty;
			}
			set
			{
				if (value < MinValue || value > MaxValue)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Set GreaterThanDefaultProperty with an invalid value: MinValue = {0}, MaxValue = {1}, value = {2}.", MinValue, MaxValue, value));
				}

				if (greaterThanDefaultProperty != value)
				{
					greaterThanDefaultProperty = value;
				}

				if (Property1 != value)
				{
					Property1 = value;
				}
			}
		}
		ZDecimal greaterThanDefaultProperty;

		protected override CodeDescriptionPairList CreatePropertySearch_ListCore()
		{
			var searchList = base.CreatePropertySearch_ListCore();
			searchList.AddPair(SearchTexts.GreaterThan, Res.GetString("Filter|NumberRangeSubSearchList|GreaterThanFilterDescription", "Search for fields that are greater than the supplied number"));
			return searchList;
		}

		public new static class SearchTexts
		{
			public static MultilingualString GreaterThan { get { return ResString.GetMultilingualString("Filter|NumberRangeSubSearchList|GreaterThanFilter", "Greater than"); } }
		}

		protected override void ResetPropertyValue()
		{
			if (IsGreaterThanSearch)
			{
				Property1 = GreaterThanDefaultProperty;
				UpdateProperty2ToMax();
			}
			else
			{
				base.ResetPropertyValue();
			}
		}

		public new void SetDefaultValue(ZDecimal value)
		{
			base.SetDefaultValue(value);
			GreaterThanDefaultProperty = value;
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			if (IsGreaterThanSearch)
			{
				return new ZQuery(FilterColumn, SQLComparisonOperator.GreaterThan, Property1);
			}
			else
			{
				return base.GetQueryUsingFilterColumns();
			}
		}

		protected override bool IsGreaterThanSearchCore
		{
			get { return PropertySearch == new ZString(SearchTexts.GreaterThan.GetUnresolvedString()); }
		}
	}
}
