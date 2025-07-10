using System;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public delegate ZQuery GetGuidQuery(ZGuid value);
	public delegate ZQuery GetGuidQueryWithOperator(SQLComparisonOperator comparisonOperator, object value);
	public delegate ZQuery GetGuidQueryWithOperatorSupportsFiltersMatch(ZDBOnlySubQuery filterQuery, SQLComparisonOperator comparisonOperator, object value);
	public delegate ZQuery GetGuidQueryWithOperatorAndOption(SQLComparisonOperator comparisonOperator, object value, ZString option);
	public delegate ZQuery GetGuidsQuery(ZGuid value1, ZGuid value2);

	#region class ModuleGuidFilter

	public class ModuleGuidFilter : ModuleFilterWithSelectedFilters<ZGuid>
	{
		#region Construction

		protected ModuleGuidFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleGuidFilter(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn, IBusinessObjectCollection list)
			: base(description, id, filterColumn, list)
		{
		}

		public ModuleGuidFilter(ZString description, ModuleIdentifier id, GetGuidQuery queryDelegate, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		public ModuleGuidFilter(ZString description, ModuleIdentifier id, GetGuidQueryWithOperator queryDelegate, IBusinessObjectCollection list, SchemaColumn filterColumnForDelegate = null)
			: base(description, id, queryDelegate, list)
		{
			FilterColumnForDelegate = filterColumnForDelegate;
		}

		public ModuleGuidFilter(ZString description, ModuleIdentifier id, GetGuidQueryWithOperatorSupportsFiltersMatch queryDelegate, IBusinessObjectCollection list, SchemaColumn filterColumnForDelegate)
			: base(description, id, queryDelegate, list)
		{
			Argument.NotNull(filterColumnForDelegate, nameof(filterColumnForDelegate));
			FilterColumnForDelegate = filterColumnForDelegate;
			SupportsFiltersMatchComparisonOperator = true;
		}

		public ModuleGuidFilter(ZString description, ModuleIdentifier id, GetGuidQueryWithOperatorAndOption queryDelegate, IBusinessObjectCollection list, SchemaColumn filterColumnForDelegate = null)
			: base(description, id, queryDelegate, list)
		{
			FilterColumnForDelegate = filterColumnForDelegate;
		}

		public ModuleGuidFilter(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn, GetList listDelegate)
			: base(description, id, filterColumn, listDelegate)
		{
		}

		public ModuleGuidFilter(ZString description, ModuleIdentifier id, GetGuidQuery queryDelegate, GetList listDelegate)
			: base(description, id, queryDelegate, listDelegate)
		{
		}

		public ModuleGuidFilter(ZString description, ModuleIdentifier id, GetGuidQueryWithOperator queryDelegate, GetList listDelegate, SchemaColumn filterColumnForDelegate = null)
			: base(description, id, queryDelegate, listDelegate)
		{
			FilterColumnForDelegate = filterColumnForDelegate;
		}

		public ModuleGuidFilter(ZString description, ModuleIdentifier id, GetGuidQueryWithOperatorSupportsFiltersMatch queryDelegate, GetList listDelegate, SchemaColumn filterColumnForDelegate)
			: base(description, id, queryDelegate, listDelegate)
		{
			Argument.NotNull(filterColumnForDelegate, nameof(filterColumnForDelegate));
			FilterColumnForDelegate = filterColumnForDelegate;
			SupportsFiltersMatchComparisonOperator = true;
		}

		public ModuleGuidFilter(ZString description, ModuleIdentifier id, SchemaColumn filterColumn, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, id, filterColumn, listUsingCurrentModuleFilterDelegate)
		{
		}

		#endregion

		#region GetNewCommonModuleFilter, CopyPropertiesToFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleGuidFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleGuidFilter)filterToCopyFrom;
			Property = filter.Property;
		}

		#endregion

		#region SetValueFromInitialCode

		public override bool SetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			if (ShouldSetValueFromInitialCode_FilterColumnName(initialProperty))
			{
				Visibility = FilterVisibility.AlwaysVisible;
				return true;
			}

			return base.SetValueFromInitialCode(initialProperty, initialCode);
		}

		public override bool ShouldSetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			return ShouldSetValueFromInitialCode_FilterColumnName(initialProperty) || base.ShouldSetValueFromInitialCode(initialProperty, initialCode);
		}

		protected override bool ShouldSetValueFromInitialCode_FilterColumnName(ZString initialProperty)
		{
			return FilterColumn != null && FilterColumn.Name == initialProperty;
		}

		public override ZString GetFormattedInitialCode_FilterColumnName()
		{
			return Property.ToString();
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region DefaultCategory

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			base.ClearCore();

			using (GetValidationSuspender())
			{
				Property = DefaultProperty;
			}
		}

		protected override bool IsEmptyCore => !Property.IsValid;

		public ZGuid DefaultProperty
		{
			get { return fDefaultProperty; }
			set
			{
				fDefaultProperty = value;
				Property = value;
			}
		}

		ZGuid fDefaultProperty;

		#endregion

		#region Property

		public override ZGuid Property
		{
			get
			{
				return base.Property;
			}
			set
			{
				if (base.Property != value)
				{
					base.Property = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty();
					}
					PropertyInfo.RefreshBinding();
				}
			}
		}

		protected override ZGuid GetEmptyPropertyValue()
		{
			return ZGuid.Empty;
		}

		object PropertyForZQuery
		{
			get
			{
				if (Property.IsValid || !HasComparisonOperator)
				{
					return Property;
				}
				return DBNull.Value;
			}
		}

		#endregion

		#region Template Filter Query

		public ZString PropertyCode { get; set; }

		protected override ZQuery XQueryCore => !PropertyCode.IsEmpty && SqlComparisonOperator != SQLComparisonOperator.NotSpecified && XQueryInfo != null
			? XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SqlComparisonOperator, PropertyCode), XQueryInfo) : base.XQueryCore;

		#endregion

		#region PropertyValidation

		public Validation PropertyValidation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return propertyValidation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { propertyValidation = value; }
		}
		Validation propertyValidation;

		#endregion

		#region Validation

		public new ModuleGuidFilterValidation Validation
		{
			get { return (ModuleGuidFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleGuidFilterValidation(this);
		}

		internal bool IsFilterColumnForValidationNullable => FilterColumnForDelegate?.IsNullable ?? true;

		#endregion

		#region Comparison

		protected override SQLComparisonOperator DefaultSqlComparisonOperator
		{
			get { return SQLComparisonOperator.Equal; }
		}

		#endregion

		#region ForceProcessingGroup

		protected internal override bool ForceProcessingGroup
		{
			get
			{
				return SqlComparisonOperator == SpecialComparisonOperator.IsNotBlank && SubGroup != null;
			}
		}

		#endregion

		#region Query

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			if (SqlComparisonOperator == SpecialComparisonOperator.IsBlank || SqlComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				if (SqlComparisonOperator == SpecialComparisonOperator.IsNotBlank && !FilterColumn.IsNullable)
				{
					return new ZQuery();
				}

				return new ZQuery(FilterColumn, SqlComparisonOperator, DBNull.Value, ComparisonOptions);
			}
			var query = base.GetQueryUsingFilterColumns();
			AddIsBlankQueryIfNeed(query);
			return query;
		}

		void AddIsBlankQueryIfNeed(ZQuery query)
		{
			if (query != null && FilterColumn.IsNullable && SqlComparisonOperator == SQLComparisonOperator.NotEqual)
			{
				query.AddToFilter(JoinCondition.Or, FilterColumn, SpecialComparisonOperator.IsBlank, DBNull.Value);
			}
		}

		protected override object[] QueryDelegateParameters
		{
			get
			{
				if (QueryDelegateType == typeof(GetGuidQueryWithOperatorSupportsFiltersMatch))
				{
					var filtersQuery = IsFilterCollectionComparisonOperatorSelected()
						? GetSubModuleSubQuery(SelectedFilters, ((IModuleFilterWithSelectedFilters)this).GetSubFilterQueryIncludingCollectionFilters(), UsesNotInQuery)
						: null;

					return new object[] { filtersQuery, SqlComparisonOperator, PropertyForZQuery };
				}
				else if (QueryDelegateType == typeof(GetGuidQueryWithOperator))
				{
					return new object[] { SqlComparisonOperator, PropertyForZQuery };
				}
				else
				{
					return new object[] { PropertyForZQuery };
				}
			}
		}

		protected override SchemaColumn SubQueryColumn => FilterColumnForDelegate;

		SchemaColumn FilterColumnForDelegate
		{
			get => filterColumnForDelegate ?? FilterColumn;
			set => filterColumnForDelegate = value;
		}
		SchemaColumn filterColumnForDelegate;

		#endregion

		#region HasComparisonOperator

		public override bool HasComparisonOperator
		{
			get { return base.HasComparisonOperator || QueryDelegateType == typeof(GetGuidQueryWithOperator) || QueryDelegateType == typeof(GetGuidQueryWithOperatorSupportsFiltersMatch) || QueryDelegateType == typeof(GetGuidQueryWithOperatorAndOption); }
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property = ZGuid.NewZGuid();
		}

#endif
		#endregion

		#region Serialization

		protected override ZGuid GetPropertyValueFromDeserializedString(string deserializedValue)
		{
			try
			{
				return new ZGuid(deserializedValue);
			}
			catch (FormatException)
			{
			}

			return ZGuid.Empty;
		}

		#endregion
	}

	#region class Validation

	public class ModuleGuidFilterValidation : ModuleFilterWithSelectedFilters<ZGuid>.ModuleFilterWithSelectedFiltersValidation
	{
		public ModuleGuidFilterValidation(ModuleGuidFilter parent)
			: base(parent)
		{
		}

		#region ValidateProperty

		public void ValidateProperty()
		{
			ValidateCalculatedProperty(Parent.PropertyInfo);
		}

		protected virtual void CheckProperty()
		{
			TypeValidation.CheckValidGuid(Parent.PropertyInfo, Parent.HasMultilingualDescription ? Parent.MultilingualDescription : Parent.Description);

			if (!Parent.PropertyInfo.HasErrors())
			{
				ErrorIfInvalidPK(Parent.PropertyInfo, Parent.List);
			}

			GetParent().PropertyValidation?.Invoke(Parent.PropertyInfo);
		}

		#endregion

		#region ValidateComparisonOperator

		protected override void CheckComparisonOperator()
		{
			base.CheckComparisonOperator();
			if (Parent is ModuleGuidFilter guidFilter && !guidFilter.IsFilterColumnForValidationNullable)
			{
				if (guidFilter.IsBlankFilterQueryDelegate == null && Parent.SqlComparisonOperator == SpecialComparisonOperator.IsBlank)
				{
					Parent.ComparisonOperatorInfo.AddError(Res.GetString("9affcb37-ed69-40d3-851a-3086fcee2265", "{0} field cannot be blank. Please choose another filter option.", Parent.HasMultilingualDescription ? Parent.MultilingualDescription : Parent.Description));
				}

				if (!guidFilter.ForceProcessingGroup && guidFilter.QueryDelegate == null && Parent.SqlComparisonOperator == SpecialComparisonOperator.IsNotBlank)
				{
					Parent.ComparisonOperatorInfo.AddWarning(Res.GetString("524a96c9-6efd-4561-b5e5-e2952c8fbde9", "{0} field cannot be blank. This filter will return all records.", Parent.HasMultilingualDescription ? Parent.MultilingualDescription : Parent.Description));
				}
			}
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateProperty();
			ValidateComparisonOperator();
		}

		public override Type AutoValidationType => GetType();

		protected ModuleGuidFilter GetParent() => (ModuleGuidFilter)Parent;

		#endregion
	}

	#endregion

	#endregion

	#region class ModuleGuidsFilter

	public class ModuleGuidsFilter : ModuleFilterWithLists, IModuleFilterWithModuleID
	{
		#region Construction

		protected ModuleGuidsFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleGuidsFilter(ZString description, ModuleIdentifier iD, SchemaGuidColumn filterColumn1, IBusinessObjectCollection list1, SchemaGuidColumn filterColumn2, IBusinessObjectCollection list2)
			: base(description, filterColumn1, list1, filterColumn2, list2)
		{
			moduleId = iD;
			SetCategory();
		}

		public ModuleGuidsFilter(ZString description, ModuleIdentifier iD, GetGuidsQuery queryDelegate, IBusinessObjectCollection list1, IBusinessObjectCollection list2)
			: base(description, queryDelegate, list1, list2)
		{
			moduleId = iD;
			SetCategory();
		}

		void SetCategory()
		{
			if (Equals(ModuleId, ModuleIDs.Organisation))
			{
				Category = FilterCategories.Organisations;
			}
		}

		public ModuleIdentifier ModuleId
		{
			get { return moduleId; }
		}
		readonly ModuleIdentifier moduleId;

		#endregion

		#region GetNewCommonModuleFilter, CopyPropertiesToFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleGuidsFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleGuidsFilter)filterToCopyFrom;
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

		#region DefaultCategory

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			Property1 = DefaultProperty1;
			Property2 = DefaultProperty2;
		}

		protected override bool IsEmptyCore => !Property1.IsValid && !Property2.IsValid;

		public ZGuid DefaultProperty1
		{
			get { return fDefaultProperty1; }
			set
			{
				fDefaultProperty1 = value;
				Property1 = value;
			}
		}

		public ZGuid DefaultProperty2
		{
			get { return fDefaultProperty2; }
			set
			{
				fDefaultProperty2 = value;
				Property2 = value;
			}
		}

		ZGuid fDefaultProperty1;
		ZGuid fDefaultProperty2;

		#endregion

		#region Property1

		[List("List1")]
		public ZGuid Property1
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

		ZGuid fProperty1;

		#endregion

		#region Property2

		[List("List2")]
		public ZGuid Property2
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

		ZGuid fProperty2;

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

		public new ModuleGuidsFilterValidation Validation
		{
			get { return (ModuleGuidsFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleGuidsFilterValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Property1, Property2 }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var result = new ZQuery();
			if (Property1.IsValid)
			{
				result.AddToFilter(FilterColumn1, Property1);
			}

			if (Property2.IsValid)
			{
				result.AddToFilter(FilterColumn2, Property2);
			}

			return result;
		}

		#endregion

		#region Template Filter Query

		public ZString PropertyCode1 { get; set; }

		public ZString PropertyCode2 { get; set; }

		protected override ZQuery XQueryCore => XQueryInfo != null ? XQueryFilterHelper.GenerateXQuery((column1, column2) => GetFilterQuery(column1, column2), XQueryInfo) : base.XQueryCore;

		ZQuery GetFilterQuery(SchemaStringColumn column1, SchemaStringColumn column2)
		{
			ZQuery result = new ZQuery();
			if (!PropertyCode1.IsEmpty)
			{
				result.AddToFilter(column1, SQLComparisonOperator.Equal, PropertyCode1);
			}
			if (!PropertyCode2.IsEmpty)
			{
				result.AddToFilter(column2, SQLComparisonOperator.Equal, PropertyCode2);
			}

			return result;
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property1 = ZGuid.NewZGuid();
		}

#endif
		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("Property1", Property1.ToString());
			writer.WriteElementString("Property2", Property2.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "Property1")
			{
				var guidString1 = reader.ReadElementString("Property1");
				try
				{ Property1 = new ZGuid(guidString1); }
				catch (FormatException) { /* the guid in the xml was bad */ }
			}

			if (reader.Name == "Property2")
			{
				var guidString2 = reader.ReadElementString("Property2");
				try
				{ Property2 = new ZGuid(guidString2); }
				catch (FormatException) { /* the guid in the xml was bad */ }
			}
		}

		public new void DeserializeProperties(XmlReader reader) => DeserializePropertiesFromXml(reader);

		#endregion
	}

	#region class Validation

	public class ModuleGuidsFilterValidation : ModuleFilterValidation
	{
		public ModuleGuidsFilterValidation(ModuleGuidsFilter parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region ValidateProperty1

		public void ValidateProperty1()
		{
			ValidateCalculatedProperty(Parent.Property1Info);
		}

		protected virtual void CheckProperty1()
		{
			TypeValidation.CheckValidGuid(Parent.Property1Info);

			if (!Parent.Property1Info.HasErrors())
			{
				ErrorIfInvalidPK(Parent.Property1Info, Parent.List1);
			}

			if (Parent.Property1Validation != null)
			{
				Parent.Property1Validation(Parent.Property1Info);
			}
		}

		#endregion

		#region ValidateProperty2

		public void ValidateProperty2()
		{
			ValidateCalculatedProperty(Parent.Property2Info);
		}

		protected virtual void CheckProperty2()
		{
			TypeValidation.CheckValidGuid(Parent.Property2Info);

			if (!Parent.Property2Info.HasErrors())
			{
				ErrorIfInvalidPK(Parent.Property2Info, Parent.List2);
			}

			if (Parent.Property2Validation != null)
			{
				Parent.Property2Validation(Parent.Property2Info);
			}
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			ValidateProperty1();
			ValidateProperty2();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}

		protected readonly ModuleGuidsFilter Parent;

		#endregion
	}

	#endregion

	#endregion
}
