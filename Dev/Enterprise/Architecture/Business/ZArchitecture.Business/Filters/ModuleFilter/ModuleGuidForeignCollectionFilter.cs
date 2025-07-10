using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleGuidForeignCollectionFilter : ModuleGuidFilter, IParameterSequenceHelperRequired
	{
		protected ModuleGuidForeignCollectionFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleGuidForeignCollectionFilter(ZString description, ModuleIdentifier moduleId, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, IBusinessObjectCollection list, Type parentBusinessObjectType)
			: base(description, moduleId, primaryKeyColumn, list)
		{
			ForeignKey = foreignKeyColumn;
			this.parentBusinessObjectType = parentBusinessObjectType;
		}

		public ModuleGuidForeignCollectionFilter(ZString description, ModuleIdentifier moduleId, SchemaGuidColumn primaryKeyColumn, SchemaGuidColumn foreignKeyColumn, GetList listDelegate, Type parentBusinessObjectType)
			: base(description, moduleId, primaryKeyColumn, listDelegate)
		{
			ForeignKey = foreignKeyColumn;
			this.parentBusinessObjectType = parentBusinessObjectType;
		}

		protected readonly SchemaGuidColumn ForeignKey;
		protected readonly Type parentBusinessObjectType;
		protected ParameterSequenceHelper SequenceHelper { get; private set; }

		#region ModuleGuidFilter Overrides

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			string.Empty,
			ModuleTextFilter.ComparisonConstants.AllMatch,
			ModuleTextFilter.ComparisonConstants.AnyMatch,
			ModuleTextFilter.ComparisonConstants.NoneMatch
		};

		public override bool HasComparisonOperator => true;

		protected virtual ZQuery GetSubQueryForPrimaryKeyBusinessObjects(ZQuery subQuery)
		{
			return subQuery.DeepClone();
		}

		protected virtual bool TryGetSubQueryForForeignKeyBusinessObjects(ZQuery subQuery, out ZQuery result)
		{
			result = subQuery.DeepClone();
			return true;
		}

		protected override bool IsFilterCollectionComparisonOperatorCore(string comparisonOperator)
		{
			return base.IsFilterCollectionComparisonOperatorCore(comparisonOperator) || string.IsNullOrEmpty(comparisonOperator);
		}

		protected override bool ShouldGetQueryForSelectedFiltersFromLayout => true;

		protected override ZDBOnlyQuery GetNewQueryForSelectedFilters()
		{
			return new ZDBOnlyQuery(parentBusinessObjectType);
		}

		protected override void AddSelectedFiltersSubquery(FilterStripBusinessObject filterBusinessObject, ZDBOnlyQuery query, ZDBOnlySubQuery subQuery)
		{
			if (FilterColumn is not null && FilterColumn != query.PKColumn && FilterColumn.TableName == query.PKColumn.TableName)
			{
				query.AddSubQuery(FilterColumn, subQuery, JoinCondition.And);
			}
			else
			{
				base.AddSelectedFiltersSubquery(filterBusinessObject, query, subQuery);
			}
		}

		protected override ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			if (ComparisonOperator == ModuleTextFilter.ComparisonConstants.AnyMatch || ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch)
			{
				return base.GetQueryForSelectedFiltersCore(filterBusinessObject, subModuleFilter);
			}

			if (filterBusinessObject.ActiveModuleFilters.Any())
			{
				var query = GetNewQueryForSelectedFilters();
				var parametersFromSubQueries = new ZSqlParameterCollection();

				// We use ALL here (as opposed to NOT EXISTS) to eliminate one of two table scans on ProcessTasks
				var sql = string.Format(CultureInfo.InvariantCulture, @"
{0} = ALL
(
	{1}
)
", ParentPkColumnNameForAllMatch, CreateAllMatchSql(query, filterBusinessObject, parametersFromSubQueries)); // part of SQL statement

				query.AddFilterAndZSQLParameterCollection(sql, parametersFromSubQueries);

				foreach (var column in filterBusinessObject.Filter.BlobFilters)
				{
					query.IncludeBlob(column);
				}

				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "part of sql clause, ALL operator should not work.")]
		protected virtual string CreateAllMatchSql(ZDBOnlyQuery query, FilterStripBusinessObject filterBusinessObject, ZSqlParameterCollection collectionToAddRenamedParameters)
		{
			var inStatement = AllMatchSubqueryStatement(query);
			var filterFormat = string.IsNullOrEmpty(inStatement) ? "AND ({0})" : inStatement;
			var parameterNameFactory = GetNewParameterNameFactory(SequenceHelper ?? new ParameterSequenceHelper());
			var savedFilterText = filterBusinessObject.Filter.IsEmpty
				? "1=1"
				: RenameParametersAndReturnParameterizedText(filterBusinessObject.Filter, parameterNameFactory, collectionToAddRenamedParameters);
			var savedFilter = string.Format(CultureInfo.InvariantCulture, filterFormat, savedFilterText);

			string emptyFilter;

			using (var module = (IZFilterModuleHelper)CreateModuleForQueryBuilding())
			{
				var emptyFilterBusinessObject = module.FilterBusinessObject;
				var emptyFilterPredicate = emptyFilterBusinessObject.Filter.IsEmpty
					? "1=1"
					: RenameParametersAndReturnParameterizedText(emptyFilterBusinessObject.Filter, parameterNameFactory, collectionToAddRenamedParameters);

				emptyFilter = string.Format(CultureInfo.InvariantCulture, filterFormat, emptyFilterPredicate);
			}

			if (emptyFilter == savedFilter)
			{
				return "SELECT NULL WHERE (1=0)";
			}

			var foreignKeyName = ForeignKey.Name;
			var parentPk = ParentPkColumnNameForAllMatch;
			var childSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(CargoWise.Schema.Schema.GetPrefixFromColumnName(foreignKeyName));
			var childTable = childSchema.TableName;
			var childPk = childSchema.PK.Name;

			var selectStatement = string.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1} child WHERE child.{2} = {3}", childPk, childTable, foreignKeyName, parentPk);

			return string.Format(CultureInfo.InvariantCulture, @"
	(
		{0}
		{1}
		EXCEPT
		{0}
		{2}
	)",
selectStatement, emptyFilter, savedFilter);
		}

		static ParameterNameFactory GetNewParameterNameFactory(ParameterSequenceHelper sequenceHelper)
		{
			var prefix = FormattableString.Invariant($"@FOREIGN__{sequenceHelper.NextSequence()}__"); // Parameter prefix must not be translated.

			return new ParameterNameFactory(prefix);
		}

		static string RenameParametersAndReturnParameterizedText(ZQuery query, ParameterNameFactory parameterNameFactory, ZSqlParameterCollection collectionToAddRenamedParameters)
		{
			var sqlText = query.ParameterisedText.ParameterisedQueryText;

			var parameters = query.ParameterisedText.Parameters.OrderByDescending(x => x.ParameterName).ToArray();
			foreach (var parameter in parameters)
			{
				var oldName = parameter.ParameterName;
				var newName = oldName.StartsWith("@FOREIGN__") ? oldName : parameterNameFactory.GetParameterName(parameter);
				sqlText = sqlText.Replace(oldName, newName);

				ZSqlParameterHelper.CreateParameterWithNewNameAndAddToCollection(parameter, collectionToAddRenamedParameters, newName);
			}

			return sqlText;
		}

		protected override bool UsesNotInQuery => ComparisonOperator == ModuleTextFilter.ComparisonConstants.NoneMatch;

		protected override SchemaColumn SubQueryColumn => ForeignKey;

		protected virtual string ParentPkColumnNameForAllMatch => FilterColumn.Name;

		protected virtual string AllMatchSubqueryStatement(ZQuery topLevelQuery) => string.Empty;

		#endregion

		#region IParameterSequenceHelperRequired Overrides

		public ParameterSequenceHelper GetParameterSequenceHelper()
		{
			return SequenceHelper;
		}

		public void SetParameterSequenceHelper(ParameterSequenceHelper sequenceHelper)
		{
			this.SequenceHelper = sequenceHelper;
			var subFiltersNeedingSequenceHelper = SelectedFilters.ActiveModuleFiltersForQuery.OfType<IParameterSequenceHelperRequired>();
			foreach (var subFilter in subFiltersNeedingSequenceHelper)
			{
				subFilter.SetParameterSequenceHelper(this.SequenceHelper);
			}
		}

		#endregion

		protected override bool IsEmptyCore => false;
	}
}
