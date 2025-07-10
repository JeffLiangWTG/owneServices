using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public delegate ZQuery GetGuidQueryWithNotIn(ZGuid value, bool notIn);
	public delegate ZQuery GetGuidQueryWithNotInandDropdownOption(ZGuid value, bool notIn, ZString option);

	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleGuidInSubCollectionFilter : ModuleGuidFilter
	{
		public ModuleGuidInSubCollectionFilter(ZString description, FilterCategory category, ModuleIdentifier moduleID, GetGuidQueryWithNotIn queryDelegate, IBusinessObjectCollection collection)
			: base(description, moduleID, WrapDelegateQuery(queryDelegate), collection)
		{
			Category = category;
		}

		public ModuleGuidInSubCollectionFilter(ZString description, FilterCategory category, ModuleIdentifier moduleID, GetGuidQueryWithNotInandDropdownOption queryDelegate, IBusinessObjectCollection collection)
			: base(description, moduleID, WrapDelegateQueryWithOption(queryDelegate), collection)
		{
			Category = category;
		}

		public ModuleGuidInSubCollectionFilter(ZString description, FilterCategory category, ModuleIdentifier moduleID, GetGuidQueryWithOperator queryDelegate, IBusinessObjectCollection collection)
			: base(description, moduleID, queryDelegate, collection)
		{
			Category = category;
		}

		static GetGuidQueryWithOperator WrapDelegateQuery(GetGuidQueryWithNotIn queryDelegate)
		{
			return (comparison, value) => queryDelegate((ZGuid)value, ComparisonOperatorToNotIn(comparison));
		}

		static GetGuidQueryWithOperatorAndOption WrapDelegateQueryWithOption(GetGuidQueryWithNotInandDropdownOption queryDelegate)
		{
			return (comparison, value, option) => queryDelegate((ZGuid)value, ComparisonOperatorToNotIn(comparison), option);
		}

		static bool ComparisonOperatorToNotIn(SQLComparisonOperator comparison)
		{
			if (comparison == SQLComparisonOperator.Contains)
			{
				return false;
			}
			else if (comparison == SQLComparisonOperator.NotContains)
			{
				return true;
			}
			else
			{
				throw new InvalidOperationException(string.Format("Comparison Operator '{0}' is invalid for this conversion. Accept 'Contains' and 'Not Contains' operators only.", comparison));
			}
		}

		#region Comparison

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				return new string[]
				{
					string.Empty,
					ComparisonConstants.Contains,
					ComparisonConstants.NotContain
				};
			}
		}

		protected override object[] QueryDelegateParameters
		{
			get
			{
				if (QueryDelegateType == typeof(GetGuidQueryWithOperatorAndOption))
				{
					return new object[] { SqlComparisonOperator, Property, DropDownTypeName };
				}
				else
				{
					return base.QueryDelegateParameters;
				}
			}
		}

		protected override SQLComparisonOperator DefaultSqlComparisonOperator
		{
			get { return SQLComparisonOperator.Contains; }
		}

		#endregion

		#region DropDown

		public static class Schema
		{
			public const string DropDownTypeName = nameof(ModuleGuidInSubCollectionFilter.DropDownTypeName);
			public const string DropDownTypeNameList = nameof(ModuleGuidInSubCollectionFilter.DropDownTypeNameList);
		}

		public ZString DropDownTypeName
		{
			get { return dropDownTypeName; }
			set
			{
				if (DropDownTypeName != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(DropDownTypeNameInfo, ref dropDownTypeName, value);
			}
		}

		ZString dropDownTypeName;

		public ZPropertyInfo DropDownTypeNameInfo { get { return GetZPropertyInfo(Schema.DropDownTypeName); } }

		public CodeDescriptionPairList DropDownTypeNameList => dropDownTypeList ?? (dropDownTypeList = CreateDropDownTypeNameList());
		CodeDescriptionPairList dropDownTypeList;

		protected virtual CodeDescriptionPairList CreateDropDownTypeNameList()
		{
			return null;
		}

		#endregion
	}
}
