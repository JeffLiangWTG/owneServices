using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	#region ModuleNkBaseFilter

	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public abstract class ModuleNkBaseFilter : ModuleTextBaseFilter
	{
		#region Constructors

		protected ModuleNkBaseFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected ModuleNkBaseFilter(ZString description, ModuleIdentifier id, SchemaStringColumn nkFilterColumn, IBusinessObjectCollection list)
			: base(description, id, nkFilterColumn, list)
		{
		}

		protected ModuleNkBaseFilter(ZString description, ModuleIdentifier id, SchemaStringColumn nkFilterColumn, GetList listDelegate)
			: base(description, id, nkFilterColumn, listDelegate)
		{
		}

		protected ModuleNkBaseFilter(ZString description, ModuleIdentifier id, GetNkQuery queryDelegate, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		protected ModuleNkBaseFilter(ZString description, ModuleIdentifier id, GetNkQueryWithOperator queryDelegate, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		protected ModuleNkBaseFilter(ZString description, ModuleIdentifier id, GetNkQueryWithOperatorSupportsFiltersMatch queryDelegate, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
			SupportsFiltersMatchComparisonOperator = true;
		}

		protected ModuleNkBaseFilter(ZString description, ModuleIdentifier id, Delegate queryDelegate, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, id, queryDelegate, listUsingCurrentModuleFilterDelegate)
		{
		}

		protected ModuleNkBaseFilter(ZString description, ModuleIdentifier id, GetTextAndNkQuery queryDelegate, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		#endregion

		protected override bool UsesStandardComparisonOperators => false;

		protected override void AddSelectedFiltersSubquery(FilterStripBusinessObject filterBusinessObject, ZDBOnlyQuery query,
			ZDBOnlySubQuery subQuery)
		{
			SchemaColumn foreignColumn = ForeignCodeColumnOverride;

			if (ForeignCodeColumnOverride == null)
			{
				var foreignColumnName = CodePropertyAttribute.CodePropertyNameFromType(filterBusinessObject.QueryObjectType);
				var foreignTablePrefix = CargoWise.Schema.Schema.GetPrefixFromColumnName(foreignColumnName);
				var foreignTableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(foreignTablePrefix);
				foreignColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(foreignColumnName, foreignTableSchema.TableName);

				if (!(foreignColumn is SchemaStringColumn))
				{
					var ex = new InvalidOperationException("Only string columns may be used for natural key subqueries.");
					ex.Data.Add("ColumnName", foreignColumn.Name);
					ex.Data.Add("ColumnType", foreignColumn.GetType().Name);
					throw ex;
				}
			}

			query.AddSubQuery(FilterColumn, foreignColumn, subQuery, JoinCondition.And);
		}

		public SchemaStringColumn ForeignCodeColumnOverride { get; set; }
	}

	#endregion

	#region ModuleNkFilter

	public delegate ZQuery GetNkQuery(ZString nK);
	public delegate ZQuery GetNkQueryWithOperator(SQLComparisonOperator comparisonOperator, ZString nK);
	public delegate ZQuery GetNkQueryWithOperatorSupportsFiltersMatch(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, ZString nK);

	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleNkFilter : ModuleNkBaseFilter
	{
		#region Construction

		public ModuleNkFilter(ZString description, SchemaStringColumn nkFilterColumn, ModuleIdentifier id, IBusinessObjectCollection list)
			: base(description, id, nkFilterColumn, list)
		{
		}

		public ModuleNkFilter(ZString description, SchemaStringColumn nkFilterColumn, ModuleIdentifier id, GetList listDelegate)
			: base(description, id, nkFilterColumn, listDelegate)
		{
		}

		public ModuleNkFilter(ZString description, GetNkQuery queryDelegate, ModuleIdentifier id, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		public ModuleNkFilter(ZString description, GetNkQueryWithOperator queryDelegate, ModuleIdentifier id, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		public ModuleNkFilter(ZString description, GetNkQueryWithOperatorSupportsFiltersMatch queryDelegate, ModuleIdentifier id, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		public ModuleNkFilter(ZString description, Delegate queryDelegate, ModuleIdentifier id, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, id, queryDelegate, listUsingCurrentModuleFilterDelegate)
		{
		}

		protected ModuleNkFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		#endregion

		#region ComparisonOperator

		protected override string[] GetAdditionalAllowedComparisonOperators()
		{
			var result = base.GetAdditionalAllowedComparisonOperators();

			if (Equals(ModuleId, ModuleIDs.GlbStaff))
			{
				result = result.Concat(new[] { ComparisonConstants.CurrentUser }).ToArray();
			}

			return result;
		}

		protected override bool SupportsComparisonOperatorSet => true;

		#endregion

		#region GetNewCommonModuleFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleNkFilter(category, parentCollection);
		}

		#endregion

		#region DefaultCategory

		protected override FilterCategory DefaultCategory => FilterCategories.Other;

		#endregion

		#region List

		public new IBusinessObjectCollection List => (IBusinessObjectCollection)base.List;

		#endregion

		#region Validation

		public new ModuleNkFilterValidation Validation
		{
			get { return (ModuleNkFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleNkFilterValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get
			{
				if (QueryDelegateType == typeof(GetNkQueryWithOperatorSupportsFiltersMatch))
				{
					var filtersQuery = IsFilterCollectionComparisonOperatorSelected()
						? GetSubModuleSubQuery(SelectedFilters, ((IModuleFilterWithSelectedFilters)this).GetSubFilterQueryIncludingCollectionFilters(), UsesNotInQuery)
						: null;

					return new object[] { filtersQuery, SqlComparisonOperator, Property };
				}
				else if (QueryDelegateType == typeof(GetNkQueryWithOperator))
				{
					return new object[] { SqlComparisonOperator, Property };
				}
				else
				{
					return new object[] { Property };
				}
			}
		}

		public override bool HasComparisonOperator
		{
			get { return base.HasComparisonOperator || QueryDelegateType == typeof(GetNkQueryWithOperator) || QueryDelegateType == typeof(GetNkQueryWithOperatorSupportsFiltersMatch); }
		}

		#endregion

		public override ZString Property
		{
			get
			{
				if (ComparisonOperator == ComparisonConstants.CurrentUser)
				{
					Property = ZString.Empty;
					return new ZString(EnvProxy.Instance.CurrentUser.Initials);
				}
				return base.Property;
			}
			set { base.Property = value; }
		}
	}

	#region class ModuleNkFilterValidation

	public class ModuleNkFilterValidation : ModuleTextFilterValidation
	{
		public ModuleNkFilterValidation(ModuleNkFilter parent)
			: base(parent)
		{
		}

		#region ValidateProperty

		protected override void CheckProperty()
		{
			base.CheckProperty();
			if (!Parent.PropertyInfo.HasErrors())
			{
				if (Parent.ErrorOnCodeNotPresent)
				{
					ListValidation.ErrorIfInvalidCode(Parent.PropertyInfo, Parent.List);
				}
			}
		}

		#endregion

		protected new ModuleNkFilter Parent
		{
			get { return (ModuleNkFilter)base.Parent; }
		}
	}

	#endregion

	#endregion

	#region ModuleTextAndNkFilter

	public delegate ZQuery GetTextAndNkQuery(SQLComparisonOperator textValueComparisonOperator, ZString textValue, ZString nK);

	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleTextAndNkFilter : ModuleNkBaseFilter
	{
		#region Construction

		protected ModuleTextAndNkFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleTextAndNkFilter(ZString description, SchemaStringColumn textFilterColumn, SchemaStringColumn nkFilterColumn, ModuleIdentifier id, IBusinessObjectCollection list)
			: base(description, id, textFilterColumn, list)
		{
			NkFilterColumn = nkFilterColumn;
		}

		public ModuleTextAndNkFilter(ZString description, GetTextAndNkQuery queryDelegate, ModuleIdentifier id, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		readonly SchemaStringColumn NkFilterColumn;

		#endregion

		#region GetNewCommonModuleFilter, CopyPropertiesToFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleTextAndNkFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleTextAndNkFilter)filterToCopyFrom;
			Property = filter.Property;
			NkProperty = filter.NkProperty;
		}

		#endregion

		#region MaxLength

		public ModuleTextAndNkFilter WithMaxLengthOf(SchemaColumn textColumn, SchemaColumn nkColumn)
		{
			MaxLength = textColumn.MaxLength;
			NkMaxLength = nkColumn.MaxLength;
			return this;
		}

		public int NkMaxLength
		{
			get => nkMaxLength > 0 ? nkMaxLength : MaxLength;
			set => nkMaxLength = value;
		}

		int nkMaxLength;

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		#endregion

		#region List

		public new IBusinessObjectCollection List
		{
			get { return (IBusinessObjectCollection)base.List; }
		}

		#endregion

		#region NkProperty

		[BusinessObjectTestExclude] // don't need a maxlength
		public ZString NkProperty
		{
			get
			{
				if (ComparisonOperator == ComparisonConstants.IsBlank || ComparisonOperator == ComparisonConstants.IsNotBlank)
				{
					return GetEmptyPropertyValue();
				}
				return fNkProperty;
			}
			set
			{
				if (fNkProperty != value)
				{
					fNkProperty = value.TrimEnd(' ');
					if (!IsValidationSuspended)
					{
						Validation.ValidateNkProperty();
					}
					NkPropertyInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo NkPropertyInfo
		{
			get { return GetZPropertyInfo(nameof(NkProperty)); }
		}

		public bool NkProperty_ReadOnly
		{
			get
			{
				return PropertyInfo.ReadOnly;
			}
		}

		ZString fNkProperty;

		#endregion

		#region NkPropertyValidation

		public Validation NkPropertyValidation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return nkPropertyValidation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { nkPropertyValidation = value; }
		}
		Validation nkPropertyValidation;

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			base.ClearCore();
			NkProperty = DefaultNkProperty;
		}

		protected override bool IsEmptyCore => Property.IsEmpty && NkProperty.IsEmpty;

		public ZString DefaultNkProperty
		{
			get { return fDefaultNkProperty; }
			set
			{
				fDefaultNkProperty = value;
				NkProperty = value;
			}
		}

		ZString fDefaultNkProperty;

		#endregion

		#region Comparison Operator

		protected override bool UsesStandardComparisonOperators => true;

		#endregion

		#region Validation

		public new ModuleTextAndNkFilterValidation Validation
		{
			get { return (ModuleTextAndNkFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleTextAndNkFilterValidation(this);
		}

		#endregion

		#region Query

		#region HasComparisonOperator

		public override bool HasComparisonOperator
		{
			get { return base.HasComparisonOperator || QueryDelegateType == typeof(GetTextAndNkQuery); }
		}

		#endregion

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { SqlComparisonOperator, Property, NkProperty }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var result = base.GetQueryUsingFilterColumns();

			if (!NkProperty.IsEmpty)
			{
				result.AddToFilter(NkFilterColumn, NkProperty);
			}

			return result;
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("NkProperty", NkProperty);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == "NkProperty")
			{
				NkProperty = reader.ReadElementString("NkProperty");
			}
		}

		#endregion

		#region SetValueFromInitialCode
		public override bool SetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			if (ShouldSetValueFromInitialCode_Prefix(initialCode))
			{
				var values = initialCode.Split(':');
				var splitCode = values[1].Split('/');
				Property = splitCode[0];
				if (splitCode.Length > 1)
				{
					NkProperty = splitCode[1];
				}
				Visibility = FilterVisibility.AlwaysVisible;
				return true;
			}

			return false;
		}

		public override bool ShouldSetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			return ShouldSetValueFromInitialCode_Prefix(initialCode);
		}

		public override ZString GetFormattedInitialCode_Prefix()
		{
			if (NkProperty.IsEmpty)
			{
				return Prefix + ":" + Property;
			}
			else
			{
				return Prefix + ":" + Property + "/" + NkProperty;
			}
		}

		#endregion
	}

	#region class ModuleTextAndNkFilterValidation

	public class ModuleTextAndNkFilterValidation : ModuleTextFilterValidation
	{
		public ModuleTextAndNkFilterValidation(ModuleTextAndNkFilter parent)
			: base(parent)
		{
		}

		#region ValidateProperty

		protected override void CheckProperty()
		{
			Parent.PropertyValidation?.Invoke(Parent.PropertyInfo);
		}

		#endregion

		#region ValidateNkProperty

		public void ValidateNkProperty()
		{
			ValidateCalculatedProperty(Parent.NkPropertyInfo);
		}

		protected void CheckNkProperty()
		{
			Parent.NkPropertyValidation?.Invoke(Parent.NkPropertyInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNkProperty();
		}

		protected new ModuleTextAndNkFilter Parent
		{
			get { return (ModuleTextAndNkFilter)base.Parent; }
		}
	}

	#endregion

	#endregion
}
