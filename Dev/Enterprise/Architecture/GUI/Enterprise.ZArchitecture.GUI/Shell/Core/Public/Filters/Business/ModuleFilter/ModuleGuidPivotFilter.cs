using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleGuidPivotFilter : ModuleGuidForeignCollectionFilter
	{
		protected ModuleGuidPivotFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleGuidPivotFilter(ZString description, ModuleIdentifier toModuleId, SchemaGuidColumn pivotToColumn, SchemaGuidColumn pivotFromColumn, IBusinessObjectCollection list, Type parentBusinessObjectType, Type pivotBusinessObjectType, ZQuery pivotTableFilter = null)
			: base(description, toModuleId, pivotToColumn, pivotFromColumn, list, parentBusinessObjectType)
		{
			PivotTableFilter = pivotTableFilter;
			PivotBusinessObjectType = pivotBusinessObjectType;
		}

		public ModuleGuidPivotFilter(ZString description, ModuleIdentifier toModuleId, SchemaGuidColumn pivotToColumn, SchemaGuidColumn pivotFromColumn, GetList listDelegate, Type parentBusinessObjectType, Type pivotBusinessObjectType, ZQuery pivotTableFilter = null)
			: base(description, toModuleId, pivotToColumn, pivotFromColumn, listDelegate, parentBusinessObjectType)
		{
			PivotTableFilter = pivotTableFilter;
			PivotBusinessObjectType = pivotBusinessObjectType;
		}

		ZQuery PivotTableFilter { get; }
		Type PivotBusinessObjectType { get; }

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			if (ComparisonOperator == ComparisonConstants.AllMatch)
			{
				return base.GetQueryForSelectedFiltersCore(filterBusinessObject, subModuleFilter);
			}

			var query = GetNewQueryForSelectedFilters();
			var pivotSubQuery = GetPivotSubQuery(filterBusinessObject, subModuleFilter);

			AddAdditionalFiltersIfRequired(pivotSubQuery, filterBusinessObject, subModuleFilter);

			query.AddSubQuery(pivotSubQuery, JoinCondition.And);

			return query;
		}

		protected virtual void AddAdditionalFiltersIfRequired(ZDBOnlySubQuery pivotSubQuery, FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
		}

		protected ZDBOnlySubQuery GetPivotSubQuery(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			var pivotSubQuery = new ZDBOnlySubQuery(PivotBusinessObjectType, ForeignKey, notIn: ComparisonOperator == ComparisonConstants.NoneMatch);

			if (PivotTableFilter != null)
			{
				pivotSubQuery.AddToFilter(PivotTableFilter);
			}

			var subModuleSubQuery = GetSubModuleSubQuery(filterBusinessObject, subModuleFilter, usesNotInQuery: false);
			pivotSubQuery.AddSubQuery(FilterColumn, subModuleSubQuery, JoinCondition.And);

			return pivotSubQuery;
		}

		protected override SchemaColumn SubQueryColumn => ObjectFactory.Get<IApplicationSchemaResolver>().GetForeignKeyTableSchemaFromColumnName(FilterColumn.Name).PK;

		protected override string ParentPkColumnNameForAllMatch => ObjectFactory.Get<IApplicationSchemaResolver>().GetForeignKeyTableSchemaFromColumnName(ForeignKey.Name).PK.Name;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "part of sql clause")]
		protected override string AllMatchSubqueryStatement(ZQuery topLevelQuery)
		{
			var pivotFilter = string.Empty;

			if (PivotTableFilter != null)
			{
				pivotFilter = string.Format(CultureInfo.InvariantCulture, " AND ({0})", PivotTableFilter.LiteralTextSqlFormatted);

				foreach (var column in PivotTableFilter.BlobFilters)
				{
					topLevelQuery.IncludeBlob(column);
				}
			}

			var inStatement = string.Format(CultureInfo.InvariantCulture, " AND {0} IN (SELECT {1} FROM {2} WHERE ({3}))", FilterColumn.Name, SubQueryColumn.Name, SubQueryColumn.TableName, "{0}");
			return pivotFilter + inStatement;
		}

		protected override ZQuery GetQueryForSelectedFilters()
		{
			var result = GetQueryForSelectedFiltersFromLayout();
			return result ?? new ZQuery();
		}
	}
}
