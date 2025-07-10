using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	#region class ModuleNumberRangeFilter

	public delegate ZQuery GetNumberRangeQuery(INumericZType value1, INumericZType value2);

	public class ModuleNumberRangeFilter : ModuleFilter
	{
		#region Constructors

		protected ModuleNumberRangeFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{ }

		public ModuleNumberRangeFilter(ZString description, SchemaNumericColumn filterColumn)
			: base(description, filterColumn)
		{
			if (filterColumn is SchemaDecimalColumn schemaDecimalColumn)
			{
				decimals = schemaDecimalColumn.Scale;
			}
		}

		public ModuleNumberRangeFilter(ZString description, GetNumberRangeQuery queryDelegate, Func<ZDecimal> getMinValue = null, Func<ZDecimal> getMaxValue = null)
			: base(description, queryDelegate)
		{
			this.getMinValue = getMinValue;
			this.getMaxValue = getMaxValue;
		}

		public ModuleNumberRangeFilter(ZString description, GetNumberRangeQuery queryDelegate, byte precision, byte scale)
			: base(description, queryDelegate)
		{
			maxValue = Math.Pow(10, precision - scale) - Math.Pow(10, -scale);
			minValue = -maxValue;
			decimals = scale;
		}

		#endregion

		#region GetNewCommonModuleFilter, CopyPropertiesToFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleNumberRangeFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleNumberRangeFilter)filterToCopyFrom;
			propertyType = filter.propertyType;
			decimals = filter.decimals;
			maxValue = filter.maxValue;
			minValue = filter.minValue;
			propertySearch = filter.propertySearch;
			property1 = filter.property1;
			property2 = filter.property2;
			defaultPropertySearch = filter.defaultPropertySearch;
			greaterThanOrEqualToDefaultProperty = filter.greaterThanOrEqualToDefaultProperty;
			lessThanOrEqualToDefaultProperty = filter.lessThanOrEqualToDefaultProperty;
			equalToDefaultProperty = filter.equalToDefaultProperty;
			betweenDefaultProperty1 = filter.betweenDefaultProperty1;
			betweenDefaultProperty2 = filter.betweenDefaultProperty2;
			getMinValue = filter.getMinValue;
			getMaxValue = filter.getMaxValue;
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.NumbersAndReferences; }
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			if (!DefaultPropertySearch.IsEmpty)
			{
				PropertySearch = DefaultPropertySearch;
			}

			ResetPropertyValue();
		}

		protected override bool IsEmptyCore => false;

		public ZDecimal GreaterThanOrEqualToDefaultProperty
		{
			get
			{
				if (greaterThanOrEqualToDefaultProperty < MinValue || greaterThanOrEqualToDefaultProperty > MaxValue)
				{
					greaterThanOrEqualToDefaultProperty = MinValue;
				}

				return greaterThanOrEqualToDefaultProperty;
			}
			set
			{
				if (value < MinValue || value > MaxValue)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Set GreaterThanOrEqualToDefaultProperty with an invalid value: MinValue = {0}, MaxValue = {1}, value = {2}.", MinValue, MaxValue, value));
				}

				if (greaterThanOrEqualToDefaultProperty != value)
				{
					greaterThanOrEqualToDefaultProperty = value;
				}

				if (Property1 != value)
				{
					Property1 = value;
				}
			}
		}

		public ZDecimal LessThanOrEqualToDefaultProperty
		{
			get
			{
				if (lessThanOrEqualToDefaultProperty < MinValue || lessThanOrEqualToDefaultProperty > MaxValue)
				{
					lessThanOrEqualToDefaultProperty = MinValue;
				}

				return lessThanOrEqualToDefaultProperty;
			}
			set
			{
				if (value < MinValue || value > MaxValue)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Set LessThanOrEqualToDefaultProperty with an invalid value: MinValue = {0}, MaxValue = {1}, value = {2}.", MinValue, MaxValue, value));
				}

				if (lessThanOrEqualToDefaultProperty != value)
				{
					lessThanOrEqualToDefaultProperty = value;
				}

				if (Property2 != value)
				{
					Property2 = value;
				}
			}
		}

		public ZDecimal EqualToDefaultProperty
		{
			get
			{
				if (equalToDefaultProperty < MinValue || equalToDefaultProperty > MaxValue)
				{
					equalToDefaultProperty = MinValue;
				}

				return equalToDefaultProperty;
			}
			set
			{
				if (value < MinValue || value > MaxValue)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Set EqualToDefaultProperty with an invalid value: MinValue = {0}, MaxValue = {1}, value = {2}.", MinValue, MaxValue, value));
				}

				if (equalToDefaultProperty != value)
				{
					equalToDefaultProperty = value;
				}

				if (Property1 != value)
				{
					Property1 = value;
				}
			}
		}

		public ZDecimal BetweenDefaultProperty1
		{
			get
			{
				if (betweenDefaultProperty1 < MinValue || betweenDefaultProperty1 > MaxValue)
				{
					betweenDefaultProperty1 = MinValue;
				}

				return betweenDefaultProperty1;
			}
			set
			{
				if (value < MinValue || value > MaxValue)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Set BetweenDefaultProperty1 with an invalid value: MinValue = {0}, MaxValue = {1}, value = {2}.", MinValue, MaxValue, value));
				}

				if (betweenDefaultProperty1 != value)
				{
					betweenDefaultProperty1 = value;
				}

				if (Property1 != value)
				{
					Property1 = value;
				}
			}
		}

		public ZDecimal BetweenDefaultProperty2
		{
			get
			{
				if (betweenDefaultProperty2 < MinValue || betweenDefaultProperty2 > MaxValue)
				{
					betweenDefaultProperty2 = MinValue;
				}

				return betweenDefaultProperty2;
			}
			set
			{
				if (value < MinValue || value > MaxValue)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Set BetweenDefaultProperty2 with an invalid value: MinValue = {0}, MaxValue = {1}, value = {2}.", MinValue, MaxValue, value));
				}

				if (betweenDefaultProperty2 != value)
				{
					betweenDefaultProperty2 = value;
				}

				if (Property2 != value)
				{
					Property2 = value;
				}
			}
		}

		ZDecimal greaterThanOrEqualToDefaultProperty;
		ZDecimal equalToDefaultProperty;
		ZDecimal lessThanOrEqualToDefaultProperty;
		ZDecimal betweenDefaultProperty1;
		ZDecimal betweenDefaultProperty2;

		#endregion

		#region Decimals

		public byte Decimals
		{
			get
			{
				if (decimals == null)
				{
					decimals = (PropertyType == ZCalcEditPropertyType.Decimal) ? (byte)2 : (byte)0;
				}
				return decimals.Value;
			}
			set
			{
				if (decimals != value)
				{
					if ((PropertyType != ZCalcEditPropertyType.Decimal) && (value != 0))
					{
						throw new InvalidOperationException("Decimals cannot be set if the PropertyType is not Decimal.");
					}

					OnModuleFilterChanged();

					decimals = value;
				}
			}
		}

		byte? decimals;

		#endregion

		#region PropertyType

		public ZCalcEditPropertyType PropertyType
		{
			get { return propertyType ?? (propertyType = GetDefaultPropertyType()).Value; }
			set
			{
				if (propertyType != value)
				{
					if (!HasQueryDelegate)
					{
						throw new InvalidOperationException("PropertyType cannot be set if a schema column is used.");
					}

					OnModuleFilterChanged();

					propertyType = value;
					decimals = null;
					minValue = null;
					maxValue = null;
				}
			}
		}

		ZCalcEditPropertyType GetDefaultPropertyType()
		{
			if (HasQueryDelegate)
			{
				return ZCalcEditPropertyType.Decimal;
			}
			else
			{
				switch (FilterColumn.ColumnType)
				{
					case SchemaColumnType.Byte:
						return ZCalcEditPropertyType.Byte;
					case SchemaColumnType.Int:
						return ZCalcEditPropertyType.Int;
					case SchemaColumnType.Short:
						return ZCalcEditPropertyType.Short;
					case SchemaColumnType.Long:
						return ZCalcEditPropertyType.Long;
					default:
						return ZCalcEditPropertyType.Decimal;
				}
			}
		}

		ZCalcEditPropertyType? propertyType;

		#endregion

		#region Property1

		public ZDecimal Property1
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property1; }

			set
			{
				if (property1 != value)
				{
					if (IsLessThanOrEqualToSearch)
					{
						throw new InvalidOperationException("Property1 should not be set when PropertySearch is LessThanOrEqualTo.");
					}

					SetNonPersistentPropertyValue(Property1Info, ref property1, value);

					if (IsEqualToSearch)
					{
						property2 = value;
					}
					else if (property2 == 0 && property2 < value)
					{
						property2 = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty1();
					}

					Property1Info.RefreshBinding();

					Property1Changed?.Invoke(property1, EventArgs.Empty);
					InvalidateCachedQuery();
				}
			}
		}

		public event EventHandler<EventArgs> Property1Changed;

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		ZDecimal property1;

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

		public ZDecimal Property2
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property2; }

			set
			{
				if (property2 != value)
				{
					if (IsGreaterThanOrEqualToSearch || IsEqualToSearch || IsGreaterThanSearch)
					{
						throw new InvalidOperationException("Property2 should not be set when PropertySearch is GreaterThanOrEqualTo or EuqalTo or GreateThan.");
					}

					SetNonPersistentPropertyValue(Property2Info, ref property2, value);

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

		ZDecimal property2;

		protected void UpdateProperty2ToMax()
		{
			property2 = MaxValue;
		}

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

#if DEBUG

		protected ZDecimal Property1_Exposed
		{
			get { return property1; }
			set { property1 = value; }
		}

		protected ZDecimal Property2_Exposed
		{
			get { return property2; }
			set { property2 = value; }
		}

#endif

		#region Validation

		public new ModuleNumberRangeFilterValidation Validation
		{
			get { return (ModuleNumberRangeFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleNumberRangeFilterValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Property1, Property2 }; }
		}

		protected override ZQuery GetQuery()
		{
			if (IsLessThanOrEqualToSearch || IsGreaterThanOrEqualToSearch || IsBetweenSearch || IsEqualToSearch || IsGreaterThanSearch)
			{
				return base.GetQuery();
			}
			else
			{
				return new ZQuery();
			}
		}

		static ZDecimal DecimalMinValue { get { return -99999999999999.99m; } }
		static ZDecimal DecimalMaxValue { get { return 99999999999999.99m; } }

		public static ZQuery AddToFilters(ZQuery query, SchemaColumn filterColumn, INumericZType number1, INumericZType number2, ZCalcEditPropertyType? propertyType = null)
		{
			ZCalcEditPropertyType editPropertyType;

			if (propertyType != null)
			{
				if (new[] { ZCalcEditPropertyType.Byte, ZCalcEditPropertyType.Decimal, ZCalcEditPropertyType.Int, ZCalcEditPropertyType.Short, ZCalcEditPropertyType.Long }.Contains(propertyType.Value))
				{
					editPropertyType = propertyType.Value;
				}
				else
				{
					throw new NotImplementedException("Not supported property type: " + propertyType.ToString());
				}
			}
			else
			{
				var columnSchemeType = filterColumn.GetType();

				if (columnSchemeType == typeof(SchemaByteColumn))
				{
					editPropertyType = ZCalcEditPropertyType.Byte;
				}
				else if (columnSchemeType == typeof(SchemaDecimalColumn))
				{
					editPropertyType = ZCalcEditPropertyType.Decimal;
				}
				else if (columnSchemeType == typeof(SchemaIntColumn))
				{
					editPropertyType = ZCalcEditPropertyType.Int;
				}
				else if (columnSchemeType == typeof(SchemaShortColumn))
				{
					editPropertyType = ZCalcEditPropertyType.Short;
				}
				else if (columnSchemeType == typeof(SchemaLongColumn))
				{
					editPropertyType = ZCalcEditPropertyType.Long;
				}
				else
				{
					throw new NotImplementedException("Not supported column scheme type: " + columnSchemeType.ToString());
				}
			}

			if (editPropertyType == ZCalcEditPropertyType.Decimal)
			{
				AddToFilters(query, filterColumn, (ZDecimal)number1, (ZDecimal)number2, (ZDecimal)number1 == (ZDecimal)number2, (ZDecimal)number1 > DecimalMinValue, (ZDecimal)number2 < DecimalMaxValue);
			}
			else if (editPropertyType == ZCalcEditPropertyType.Long)
			{
				var longValue1 = ((ZDecimal)number1).ToZLong();
				var longValue2 = ((ZDecimal)number2).ToZLong();
				AddToFilters(query, filterColumn, longValue1, longValue2, longValue1 == longValue2, longValue1 > long.MinValue, longValue2 < long.MaxValue);
			}
			else
			{
				var intValue1 = number1.ToZInt();
				var intValue2 = number2.ToZInt();

				if (editPropertyType == ZCalcEditPropertyType.Byte)
				{
					AddToFilters(query, filterColumn, (byte)intValue1, (byte)intValue2, (byte)intValue1 == (byte)intValue2, (byte)intValue1 > byte.MinValue, (byte)intValue2 < byte.MaxValue);
				}
				else if (editPropertyType == ZCalcEditPropertyType.Short)
				{
					AddToFilters(query, filterColumn, (short)intValue1, (short)intValue2, (short)intValue1 == (short)intValue2, (short)intValue1 > short.MinValue, (short)intValue2 < short.MaxValue);
				}
				else
				{
					AddToFilters(query, filterColumn, (int)intValue1, (int)intValue2, (int)intValue1 == (int)intValue2, (int)intValue1 > int.MinValue, (int)intValue2 < int.MaxValue);
				}
			}

			return query;
		}

		static ZQuery AddToFilters<NumberType>(ZQuery query, SchemaColumn filterColumn, NumberType value1, NumberType value2, bool equal, bool useValue1, bool useValue2)
		{
			if (equal)
			{
				query.AddToFilter(filterColumn, SQLComparisonOperator.Equal, value1);
			}
			else
			{
				if (useValue1)
				{
					query.AddToFilter(filterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
				}

				if (useValue2)
				{
					query.AddToFilter(filterColumn, SQLComparisonOperator.LessThanOrEqualTo, value2);
				}
			}

			return query;
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			return AddToFilters(new ZQuery(), FilterColumn, Property1, Property2);
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("PropertySearch", PropertySearch);
			writer.WriteElementString("Property1", Property1.ToString());
			writer.WriteElementString("Property2", Property2.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "PropertySearch")
			{
				PropertySearch = reader.ReadElementString("PropertySearch");
			}

			if (reader.Name == "Property1")
			{
				var property1AsString = reader.ReadElementString("Property1");
				if (!IsLessThanOrEqualToSearch)
				{
					Property1 = ZDecimal.ParseSafe(property1AsString, 0);
				}
			}

			if (reader.Name == "Property2")
			{
				var property2AsString = reader.ReadElementString("Property2");
				if (!IsGreaterThanOrEqualToSearch && !IsEqualToSearch && !IsGreaterThanSearch)
				{
					Property2 = ZDecimal.ParseSafe(property2AsString, 0);
				}
			}
		}

		#endregion

		public CodeDescriptionPairList PropertySearch_List
		{
			get
			{
				if (propertySearch_List == null)
				{
					propertySearch_List = CreatePropertySearch_ListCore();
				}

				return propertySearch_List;
			}
		}

		CodeDescriptionPairList propertySearch_List;

		protected virtual CodeDescriptionPairList CreatePropertySearch_ListCore()
		{
			var searchList = new CodeDescriptionPairList();
			searchList.AddPair(SearchTexts.GreaterThanOrEqualTo, Res.GetString("Filter|NumberRangeSearchList|GreaterThanFilterDescription", "Search for fields that are greater than or equal to the supplied number"));
			searchList.AddPair(SearchTexts.LessThanOrEqualTo, Res.GetString("Filter|NumberRangeSearchList|LessThanFilterDescription", "Search for fields that are less than or equal to the supplied number"));
			searchList.AddPair(SearchTexts.EqualTo, Res.GetString("Filter|NumberRangeSearchList|EqualToFilterDescription", "Search for fields that are equal to the supplied number"));
			searchList.AddPair(SearchTexts.Between, Res.GetString("Filter|NumberRangeSearchList|BetweenFilterDescription", "Search for fields that are between the two supplied numbers"));

			return searchList;
		}

		#region Up/Down Arrow Buttons

		public bool ShowUpAndDownArrows { get; set; }

		public void ClickUpArrow()
		{
			Property1++;
			UpArrowClicked?.Invoke(this, EventArgs.Empty);
		}

		public void ClickDownArrow()
		{
			Property1--;
			DownArrowClicked?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler<EventArgs> UpArrowClicked;
		public event EventHandler<EventArgs> DownArrowClicked;

		#endregion

		#region SearchTexts

		public static class SearchTexts
		{
			public static MultilingualString GreaterThanOrEqualTo { get { return ResString.GetMultilingualString("Filter|NumberRangeSearchList|GreaterThanFilter", "Greater than or equal to"); } }
			public static MultilingualString LessThanOrEqualTo { get { return ResString.GetMultilingualString("Filter|NumberRangeSearchList|LessThanFilter", "Less than or equal to"); } }
			public static MultilingualString Between { get { return ResString.GetMultilingualString("Filter|NumberRangeSearchList|BetweenFilter", "Between"); } }
			public static MultilingualString EqualTo { get { return ResString.GetMultilingualString("Filter|NumberRangeSearchList|EqualToFilter", "Equal to"); } }
		}

		Func<ZDecimal> getMinValue;
		public event EventHandler<EventArgs> MinValueChanged;
		public ZDecimal MinValue
		{
			get
			{
				if (getMinValue != null)
				{
					var newMinValue = getMinValue.Invoke();
					if (minValue == null || minValue != newMinValue)
					{
						ChangeMinValue(newMinValue);
					}
				}
				if (minValue == null)
				{
					minValue = MinValueOfType;
				}

				return minValue.Value;
			}

			set
			{
				if (value >= MinValueOfType && value <= MaxValue && minValue != value)
				{
					ChangeMinValue(value);
					ClearCore();
				}
				else
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, @"MinValue setting error: MaxValue = {0}, MinValueOfType = {1}, value = {2}", MaxValue, MinValueOfType, value));
				}
			}
		}

		ZDecimal? minValue;

		void ChangeMinValue(ZDecimal newMinValue)
		{
			minValue = newMinValue;
			MinValueChanged?.Invoke(minValue, EventArgs.Empty);
			Validation.ValidateAll();
		}

		ZDecimal MaxValueForPrecisionAndScale()
		{
			if (this.FilterColumn is SchemaDecimalColumn decimalColumn)
			{
				var digits = decimalColumn.Precision - decimalColumn.Scale;
				return Math.Min(DecimalMaxValue, new ZDecimal((decimal)Math.Pow(10, digits) - (decimal)Math.Pow(10, -decimalColumn.Scale)));
			}
			return DecimalMaxValue;
		}

		ZDecimal MinValueOfType
		{
			get
			{
				switch (PropertyType)
				{
					case ZCalcEditPropertyType.Byte:
						return Byte.MinValue;

					case ZCalcEditPropertyType.Short:
						return short.MinValue;

					case ZCalcEditPropertyType.Int:
						return int.MinValue;

					case ZCalcEditPropertyType.Decimal:
					default:
						return -MaxValueForPrecisionAndScale();
				}
			}
		}

		Func<ZDecimal> getMaxValue;
		public event EventHandler<EventArgs> MaxValueChanged;
		public ZDecimal MaxValue
		{
			get
			{
				if (getMaxValue != null)
				{
					var newMaxValue = getMaxValue.Invoke();
					if (maxValue == null || maxValue != newMaxValue)
					{
						ChangeMaxValue(newMaxValue);
					}
				}
				if (maxValue == null)
				{
					maxValue = MaxValueOfType;
				}

				return maxValue.Value;
			}

			set
			{
				if (value >= MinValue && value <= MaxValueOfType && maxValue != value)
				{
					ChangeMaxValue(value);
					ClearCore();
				}
				else
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, @"MaxValue setting error: MinValue = {0}, MaxValueOfType = {1}, value = {2}", MinValue, MaxValueOfType, value));
				}
			}
		}

		ZDecimal? maxValue;

		void ChangeMaxValue(ZDecimal newMaxValue)
		{
			maxValue = newMaxValue;
			MaxValueChanged?.Invoke(maxValue, EventArgs.Empty);
			Validation.ValidateAll();
		}

		ZDecimal MaxValueOfType
		{
			get
			{
				switch (PropertyType)
				{
					case ZCalcEditPropertyType.Byte:
						return Byte.MaxValue;

					case ZCalcEditPropertyType.Short:
						return short.MaxValue;

					case ZCalcEditPropertyType.Int:
						return int.MaxValue;

					case ZCalcEditPropertyType.Decimal:
					default:
						return MaxValueForPrecisionAndScale();
				}
			}
		}

		#endregion

		#region PropertySearch

		ZString defaultPropertySearch;

		public ZString DefaultPropertySearch
		{
			get
			{
				return defaultPropertySearch;
			}

			set
			{
				if (defaultPropertySearch != value)
				{
					defaultPropertySearch = value;
				}

				if (!PropertySearch_List.GetAllCodesZString().Contains(value))
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Set DefaultPropertySearch with an invalid value: value = {0}. Valid values include: {1}.", value, PropertySearch_List.CodesAsString));
				}
			}
		}

		public ZString PropertySearch
		{
			get
			{
				return propertySearch;
			}

			set
			{
				if (propertySearch != value)
				{
					propertySearch = value;

					ResetPropertyValue();

					if (!IsValidationSuspended)
					{
						Validation.ValidatePropertySearch();
					}

					PropertySearchInfo.RefreshBinding();

					PropertySearchChanged?.Invoke(propertySearch, EventArgs.Empty);
					InvalidateCachedQuery();
				}
			}
		}

		protected virtual void ResetPropertyValue()
		{
			if (IsGreaterThanOrEqualToSearch)
			{
				Property1 = GreaterThanOrEqualToDefaultProperty;
				property2 = MaxValue;
			}
			else if (IsLessThanOrEqualToSearch)
			{
				property1 = MinValue;
				Property2 = LessThanOrEqualToDefaultProperty;
			}
			else if (IsEqualToSearch)
			{
				Property1 = MinValue <= 0 && 0 <= MaxValue ? 0 : EqualToDefaultProperty;
				property2 = property1;
			}
			else if (IsBetweenSearch)
			{
				Property1 = BetweenDefaultProperty1;
				Property2 = BetweenDefaultProperty2;
			}
		}

		public void SetDefaultValue(ZDecimal value)
		{
			GreaterThanOrEqualToDefaultProperty = value;
			EqualToDefaultProperty = value;
			LessThanOrEqualToDefaultProperty = value;
			BetweenDefaultProperty1 = value;
			BetweenDefaultProperty2 = value;
		}

		public event EventHandler<EventArgs> PropertySearchChanged;

		ZString propertySearch = new ZString(SearchTexts.Between.GetUnresolvedString());

		public bool IsGreaterThanSearch => IsGreaterThanSearchCore;

		protected virtual bool IsGreaterThanSearchCore => false;

		public bool IsGreaterThanOrEqualToSearch
		{
			get { return propertySearch == new ZString(SearchTexts.GreaterThanOrEqualTo.GetUnresolvedString()); }
		}

		public bool IsLessThanOrEqualToSearch
		{
			get { return propertySearch == new ZString(SearchTexts.LessThanOrEqualTo.GetUnresolvedString()); }
		}

		public bool IsBetweenSearch
		{
			get { return propertySearch == new ZString(SearchTexts.Between.GetUnresolvedString()); }
		}

		public bool IsEqualToSearch
		{
			get { return propertySearch == new ZString(SearchTexts.EqualTo.GetUnresolvedString()); }
		}

		public ZPropertyInfo PropertySearchInfo
		{
			get { return GetZPropertyInfo(nameof(PropertySearch)); }
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property1 = ZDecimal.Zero;
		}

#endif
		#endregion

		#region class ModuleNumberRangeFilterValidation

		public class ModuleNumberRangeFilterValidation : ModuleFilterValidation
		{
			public ModuleNumberRangeFilterValidation(ModuleNumberRangeFilter parent)
				: base(parent)
			{
				Parent = parent;
			}

			#region ValidatePropertySearch

			public void ValidatePropertySearch()
			{
				ValidateCalculatedProperty(Parent.PropertySearchInfo);
				ValidateProperty1();
				ValidateProperty2();
			}

			protected void CheckPropertySearch()
			{
				ListValidation.ErrorIfInvalidCode(Parent.PropertySearchInfo, Parent.PropertySearch_List);
			}

			#endregion

			#region ValidateProperty1

			public void ValidateProperty1()
			{
				ValidateCalculatedProperty(Parent.Property1Info);
				ValidateCalculatedProperty(Parent.Property2Info);
			}

			protected virtual void CheckProperty1()
			{
				if (Parent.Property1 > Parent.Property2)
				{
					Parent.Property1Info.AddError(Res.GetString("7a6b8941-c860-44ff-925c-c8997cf23071", "The 'From' value is greater than the 'To' value."));
				}

				if (Parent.Property1Validation != null)
				{
					Parent.Property1Validation(Parent.Property1Info);
				}

				if (Parent.Property1 < Parent.MinValue)
				{
					Parent.Property1Info.AddError(Res.GetString("7fde5439-ec62-4e0c-8b98-cd5282ff7512", "Please enter a value greater than or equal to {0}.", Parent.MinValue.ToString(GetFormatString(), CultureInfo.InvariantCulture)));
				}
				else if (Parent.Property1 > Parent.MaxValue)
				{
					Parent.Property1Info.AddError(Res.GetString("217E7A9B-30E3-45D3-B4C1-B148EED042A5", "Please enter a value less than or equal to {0}.", Parent.MaxValue.ToString(GetFormatString(), CultureInfo.InvariantCulture)));
				}
			}

			string GetFormatString() => string.Format(CultureInfo.InvariantCulture, "N{0}", Parent.Decimals); // String formatting code
			#endregion

			#region ValidateProperty2

			public void ValidateProperty2()
			{
				ValidateCalculatedProperty(Parent.Property1Info);
				ValidateCalculatedProperty(Parent.Property2Info);
			}

			protected virtual void CheckProperty2()
			{
				if (Parent.Property2 < Parent.Property1)
				{
					Parent.Property2Info.AddError(Res.GetString("193a7169-6abd-402b-9dbe-e47ce4cddd76", "The 'To' value is less than the 'From' value."));
				}

				if (Parent.Property2Validation != null)
				{
					Parent.Property2Validation(Parent.Property2Info);
				}

				if (Parent.Property2 < Parent.MinValue)
				{
					Parent.Property2Info.AddError(Res.GetString("7fde5439-ec62-4e0c-8b98-cd5282ff7512", "Please enter a value greater than or equal to {0}.", Parent.MinValue.ToString(GetFormatString(), CultureInfo.InvariantCulture)));
				}
				else if (Parent.Property2 > Parent.MaxValue)
				{
					Parent.Property2Info.AddError(Res.GetString("217E7A9B-30E3-45D3-B4C1-B148EED042A5", "Please enter a value less than or equal to {0}.", Parent.MaxValue.ToString(GetFormatString(), CultureInfo.InvariantCulture)));
				}
			}

			#endregion

			#region ZValidation Overrides

			public override void ValidateAll()
			{
				ValidatePropertySearch();
				ValidateProperty1();
				ValidateProperty2();
			}

			public override Type AutoValidationType
			{
				get { return GetType(); }
			}

			#endregion

			protected readonly ModuleNumberRangeFilter Parent;
		}

		#endregion
	}

	#endregion
}
