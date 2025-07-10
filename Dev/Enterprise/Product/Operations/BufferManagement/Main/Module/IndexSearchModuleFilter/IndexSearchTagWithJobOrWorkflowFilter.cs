using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using GlowIndexQueryService.Business;

namespace Enterprise.BufferManagement.Module
{
	public class IndexSearchTagWithJobOrWorkflowFilter : TagWithJobOrWorkflowFilter, IIndexSearchModuleFilter
	{
		public IndexSearchTagWithJobOrWorkflowFilter(SearchField searchField, FilterCategory category, ModuleIdentifier moduleID, Func<string, ZGuid, ZString, string, IGlowQuery> getGlowIndexQueryDelegate, IBusinessObjectCollection collection, bool useJobOrWorkflowFilter = true)
			: base(searchField.FieldName, category, moduleID, new GetGuidQueryWithNotInandDropdownOption((a, b, c) => new ZQuery()), collection)
		{
			((IModuleFilterForStrategyInternal)this).Description = searchField.FieldName;
			MultilingualDescription = (NoResString)searchField.Description;
			SearchField = searchField;
			this.getGlowIndexQueryDelegate = getGlowIndexQueryDelegate;
			this.useJobOrWorkflowFilter = useJobOrWorkflowFilter;
		}

		public SearchField SearchField { get; }

		readonly Func<string, ZGuid, ZString, string, IGlowQuery> getGlowIndexQueryDelegate;
		readonly bool useJobOrWorkflowFilter;

		public IGlowQuery GetGlowIndexQuery()
		{
			if (getGlowIndexQueryDelegate != null)
			{
				return getGlowIndexQueryDelegate(SearchField.FieldName, Property, DropDownTypeName, ComparisonOperator);
			}

			return GetGlowIndexQueryCore(SearchField.FieldName, Property, ComparisonOperator);
		}

		public static IGlowQuery GetGlowIndexQueryCore(string fieldName, ZGuid property, string comparisonOperator)
		{
			if (property.IsEmpty)
			{
				return new EmptyQuery();
			}

			var term = new Term(fieldName, property.ToString());

			return comparisonOperator switch
			{
				IsAppliedComparisonOperator => new EqualQuery(term, false),
				NotAppliedComparisonOperator => new NotEqualQuery(term, false),
				_ => new EmptyQuery(),
			};
		}

		public static IGlowQuery GetGlowIndexQueryForTag(string fieldName, ZGuid property, string comparisonOperator, BusinessObjectFactory factory)
		{
			if (property.IsEmpty)
			{
				return new EmptyQuery();
			}

			var magnitude = factory.Load<TagMagnitude>(property);
			var isExclusive = magnitude?.Definition?.TGD_IsExclusive ?? true;
			var group = magnitude?.TGM_TGD_Tag ?? ZGuid.Empty;

			var term = new Term(fieldName, property.ToString());
			var parentTerm = new Term("JOB" + fieldName, property.ToString());

			IGlowQuery query;
			if (!isExclusive)
			{
				query = new BooleanQuery(BooleanOperator.Or, new[]
				{
					new EqualQuery(term, false),
					new EqualQuery(parentTerm, false),
				});
			}
			else
			{
				var tagGroupTerm = new Term("TAGGROUP", group.ToString());
				query = new BooleanQuery(BooleanOperator.Or, new IGlowQuery[]
				{
					new EqualQuery(term, false),
					new BooleanQuery(BooleanOperator.And, new IGlowQuery[]
					{
						new EqualQuery(parentTerm, false),
						new NotEqualQuery(tagGroupTerm, false),
					}),
				});
			}

			return comparisonOperator switch
			{
				IsAppliedComparisonOperator => query,
				NotAppliedComparisonOperator => new NotQuery(query),
				_ => new EmptyQuery(),
			};
		}

		public static IGlowQuery GetGlowIndexQueryForTagGroup(string fieldName, ZGuid property, string comparisonOperator)
		{
			if (property.IsEmpty)
			{
				return new EmptyQuery();
			}

			var term = new Term(fieldName, property.ToString());
			var parentTerm = new Term("JOB" + fieldName, property.ToString());

			var queries = new BooleanQuery(BooleanOperator.Or, new[]
				{
					new EqualQuery(term, false),
					new EqualQuery(parentTerm, false),
				});

			return comparisonOperator switch
			{
				IsAppliedComparisonOperator => queries,
				NotAppliedComparisonOperator => new NotQuery(queries),
				_ => new EmptyQuery(),
			};
		}

		protected override CodeDescriptionPairList CreateDropDownTypeNameList()
		{
			return useJobOrWorkflowFilter ? base.CreateDropDownTypeNameList() : null;
		}
	}
}
