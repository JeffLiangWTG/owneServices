using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public delegate ZQuery GetEntryStatusQueryDelegate(SQLComparisonOperator comparisonOperator, ZString type, ZString value);
	public class EntryStatusFilter : ModuleTextBaseFilter
	{
		public EntryStatusFilter(ZString description,
			GetEntryStatusQueryDelegate queryDelegate,
			Func<CodeDescriptionPairList> getEntryStatusList, bool useFilterType = true)
			: base(description, queryDelegate)
		{
			Argument.NotNull(getEntryStatusList, nameof(getEntryStatusList));
			this.getEntryStatusList = getEntryStatusList;

			useComparisonOperator = true;
			this.useFilterType = useFilterType;

			ShowFilterType = useFilterType;
		}

		public EntryStatusFilter(ZString description,
			Delegate queryDelegate,
			Func<CodeDescriptionPairList> getEntryStatusList,
			bool useComparisonOperator,
			bool useFilterType)
			: base(description, queryDelegate)
		{
			Argument.NotNull(getEntryStatusList, nameof(getEntryStatusList));
			this.getEntryStatusList = getEntryStatusList;

			this.useComparisonOperator = useComparisonOperator;
			this.useFilterType = useFilterType;

			ShowFilterType = useFilterType;
		}

		readonly bool useComparisonOperator;
		readonly bool useFilterType;

		public bool ShowFilterType;
		public bool ShowComparisonOperator => useComparisonOperator;

		public override bool HasComparisonOperator => useComparisonOperator;

		protected override ModuleFilterValidation GetNewValidation() => new EntryStatusFilterValidation(this);

		protected EntryStatusFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			FilterType = EntryStatusFilterTypeList.Codes.Any;
		}

		public new EntryStatusFilterValidation Validation => (EntryStatusFilterValidation)base.Validation;

		#region Properties

		[List(nameof(TypeList))]
		[MaxLength(3)]
		public ZString FilterType
		{
			get => filterType;
			set
			{
				var oldValue = filterType;
				SetNonPersistentPropertyValue(FilterTypeInfo, ref filterType, value);

				if (oldValue != value)
				{
					UpdateAvailableComparisonOperators(value);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateFilterType();
				}
			}
		}

		ZString filterType;

		public ZPropertyInfo FilterTypeInfo => GetZPropertyInfo(nameof(FilterType));

		public CodeDescriptionPairList TypeList => typeList ??= GetTypeListCore();
		CodeDescriptionPairList typeList;

		protected virtual CodeDescriptionPairList GetTypeListCore() => new EntryStatusFilterTypeList();

		[List(nameof(EntryStatusList))]
		public override ZString Property { get => base.Property; set => base.Property = value; }

		public CodeDescriptionPairList EntryStatusList => entryStatusList ??= getEntryStatusList.Invoke();
		CodeDescriptionPairList entryStatusList;

		readonly Func<CodeDescriptionPairList> getEntryStatusList;

		#endregion

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.StatusAndFlags; }
		}

		protected override object[] QueryDelegateParameters
		{
			get
			{
				var propertyList = new List<object>();

				if (useComparisonOperator)
				{
					propertyList.Add(SqlComparisonOperator);
				}

				if (useFilterType)
				{
					propertyList.Add(filterType);
				}

				propertyList.Add(Property);

				return propertyList.ToArray();
			}
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category,
			ModuleFilterCollection parentCollection)
		{
			return new EntryStatusFilter(category, parentCollection);
		}

		protected override void ClearCore()
		{
			base.ClearCore();
			FilterType = EntryStatusFilterTypeList.Codes.Any;
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);

			var source = (EntryStatusFilter)filterToCopyFrom;
			filterType = source.filterType;
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("FilterType", FilterType);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			FilterType = reader.ReadElementString("FilterType");
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && FilterType.ToString() switch
		{
			EntryStatusFilterTypeList.Codes.Any => !ComparisonOperatorsWherePropertyCanBeEmptyAny.Contains(ComparisonOperator),
			EntryStatusFilterTypeList.Codes.All => false,
			_ => true,
		};

		static readonly ImmutableHashSet<ZString> ComparisonOperatorsWherePropertyCanBeEmptyAny =
			ImmutableHashSet.Create<ZString>(ComparisonConstants.IsNotBlank, ComparisonConstants.IsBlank);

		void UpdateAvailableComparisonOperators(ZString newFilterType)
		{
			var oldComparisonOperator = ComparisonOperator;
			RemoveComparisonOperatorsLeavingOne(ComparisonConstants.Exact);

			if (newFilterType == EntryStatusFilterTypeList.Codes.Any)
			{
				ComparisonOperator_List.Add(ComparisonConstants.GetComparisonOperatorPair(ComparisonConstants.NotEqual));
				ComparisonOperator_List.Add(ComparisonConstants.GetComparisonOperatorPair(ComparisonConstants.IsBlank));
				ComparisonOperator_List.Add(ComparisonConstants.GetComparisonOperatorPair(ComparisonConstants.IsNotBlank));
			}

			if (!oldComparisonOperator.IsEmpty && ComparisonOperator_List.ContainsCode(oldComparisonOperator))
			{
				ComparisonOperator = oldComparisonOperator;
			}
		}
	}
}
