using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public delegate ZQuery GetGuidQueryWithNotInAndInherited(ZGuid value, bool notIn, bool includeInherited, ZSqlParameterCollection parameters, BusinessObjectFactory factory);

	public class ModuleGuidAppliedToSubCollectionFilter : ModuleGuidInSubCollectionFilter
	{
		public ModuleGuidAppliedToSubCollectionFilter(ZString description, FilterCategory category, ModuleIdentifier moduleID, GetGuidQueryWithNotIn queryDelegate, IBusinessObjectCollection collection)
			: base(description, category, moduleID, queryDelegate, collection)
		{
			EditComparisonOperatorList();
		}

		public ModuleGuidAppliedToSubCollectionFilter(ZString description, FilterCategory category, ModuleIdentifier moduleID, GetGuidQueryWithNotInandDropdownOption queryDelegate, IBusinessObjectCollection collection)
			: base(description, category, moduleID, queryDelegate, collection)
		{
			EditComparisonOperatorList();
		}

		public ModuleGuidAppliedToSubCollectionFilter(ZString description, FilterCategory category, ModuleIdentifier moduleID, GetGuidQueryWithNotInAndInherited queryDelegate, bool shouldReevaluateQuery, IBusinessObjectCollection collection, ZSqlParameterCollection parameters, BusinessObjectFactory factory)
			: base(description, category, moduleID, WrapDelegateQuery(queryDelegate, parameters, factory), collection)
		{
			EditComparisonOperatorListWithInherited();
			this.shouldReevaluateQuery = shouldReevaluateQuery;
		}

		readonly bool shouldReevaluateQuery;

		protected override bool ShouldReevaluateQuery()
		{
			return shouldReevaluateQuery;
		}

		static GetGuidQueryWithOperator WrapDelegateQuery(GetGuidQueryWithNotInAndInherited queryDelegate, ZSqlParameterCollection parameters, BusinessObjectFactory factory)
		{
			return (comparison, value) => queryDelegate((ZGuid)value, ComparisonOperatorToNotIn(comparison), ComparisonOperatorToInherited(comparison), parameters, factory);
		}

		static bool ComparisonOperatorToNotIn(SQLComparisonOperator comparison)
		{
			if (comparison == SQLComparisonOperator.Contains || comparison == SQLComparisonOperator.Equal)
			{
				return false;
			}
			else if (comparison == SQLComparisonOperator.NotContains || comparison == SQLComparisonOperator.NotEqual)
			{
				return true;
			}
			else
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, comparisonOperatorError, comparison));
			}
		}

		static bool ComparisonOperatorToInherited(SQLComparisonOperator comparison)
		{
			if (comparison == SQLComparisonOperator.Equal || comparison == SQLComparisonOperator.NotEqual)
			{
				return true;
			}
			else if (comparison == SQLComparisonOperator.Contains || comparison == SQLComparisonOperator.NotContains)
			{
				return false;
			}
			else
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, comparisonOperatorError, comparison));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		const string comparisonOperatorError = "Comparison Operator '{0}' is invalid for this conversion. Accept 'Contains' and 'Not Contains' operators only.";

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				return new string[]
				{
					string.Empty,
					IsAppliedComparisonOperator,
					NotAppliedComparisonOperator,
					IsAppliedOrInheritedComparisonOperator,
					NotAppliedNorInheritedComparisonOperator
				};
			}
		}

		public override ZString ComparisonOperator
		{
			get { return base.ComparisonOperator; }
			set { base.ComparisonOperator = DoComparisonOperatorConversion(value); }
		}

		ZString DoComparisonOperatorConversion(ZString value)
		{
			switch (value)
			{
				case ComparisonConstants.Contains:
					return IsAppliedComparisonOperator;
				case ComparisonConstants.NotContain:
					return NotAppliedComparisonOperator;
				case ComparisonConstants.AnyMatch:
					return IsAppliedOrInheritedComparisonOperator;
				case ComparisonConstants.NoneMatch:
					return NotAppliedNorInheritedComparisonOperator;
				default:
					return value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter operator")]
		public const string IsAppliedComparisonOperator = "is applied";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter operator")]
		public const string IsAppliedOrInheritedComparisonOperator = "is applied or inherited";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter operator")]
		public const string NotAppliedComparisonOperator = "not applied";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter operator")]
		public const string NotAppliedNorInheritedComparisonOperator = "not applied nor inherited";

		void EditComparisonOperatorList()
		{
			ComparisonOperator_List.RemoveCode(ComparisonConstants.Contains);
			ComparisonOperator_List.RemoveCode(ComparisonConstants.NotContain);

			ComparisonOperator_List.AddPair(IsAppliedComparisonOperator, ResString.GetMultilingualString("5b3a4541-e941-49a2-b0c4-69f7214b51e6", "is applied"));
			ComparisonOperator_List.AddPair(NotAppliedComparisonOperator, ResString.GetMultilingualString("9df32680-6af9-4ae2-b81f-31c8116d8508", "not applied"));
		}

		void EditComparisonOperatorListWithInherited()
		{
			EditComparisonOperatorList();
			ComparisonOperator_List.RemoveCode(ComparisonConstants.AnyMatch);
			ComparisonOperator_List.RemoveCode(ComparisonConstants.NoneMatch);

			ComparisonOperator_List.AddPair(IsAppliedOrInheritedComparisonOperator, ResString.GetMultilingualString("d15bba09-579c-4ce3-a1ea-d5c84d47e66e", "is applied or inherited"));
			ComparisonOperator_List.AddPair(NotAppliedNorInheritedComparisonOperator, ResString.GetMultilingualString("c9ea9126-e556-4e70-b0c5-c71b197a397c", "not applied nor inherited"));
		}

		public override SQLComparisonOperator SqlComparisonOperator
		{
			get { return GetComparisonOperator_WithIsApplied(ComparisonOperator, DefaultSqlComparisonOperator); }
			set { base.SqlComparisonOperator = value; }
		}

		static SQLComparisonOperator GetComparisonOperator_WithIsApplied(string comparisonOperatorCode, SQLComparisonOperator defaultComparisonOperator)
		{
			if (comparisonOperatorCode == IsAppliedComparisonOperator)
			{
				return SQLComparisonOperator.Contains;
			}
			else if (comparisonOperatorCode == NotAppliedComparisonOperator)
			{
				return SQLComparisonOperator.NotContains;
			}
			else if (comparisonOperatorCode == IsAppliedOrInheritedComparisonOperator)
			{
				return SQLComparisonOperator.Equal;
			}
			else if (comparisonOperatorCode == NotAppliedNorInheritedComparisonOperator)
			{
				return SQLComparisonOperator.NotEqual;
			}
			else
			{
				return GetSqlComparisonOperator(comparisonOperatorCode, defaultComparisonOperator);
			}
		}
	}
}
