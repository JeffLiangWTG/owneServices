using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using JC = CargoWise.EntityFramework.JoinCondition;

namespace CargoWise.EntityFramework
{
	#region Number Publisher
#if DEBUG

	public class NumberPublisher
	{
		public int GetNextVariableNumberSuffix()
		{
			int result = ++VariableNumberSuffix;
			return result;
		}
		int VariableNumberSuffix;
	}

#endif
	#endregion

	/// <summary>
	/// A combination of a filter string, and a ZSQLParameterCollection.
	/// </summary>
	[DebuggerDisplay("SQL Query: { LiteralTextSql } [IsNoResultQuery={IsNoResultQuery}]")]
	public class ZQuery : AbstractFilterPart, IFilterPart, IFilterPartsProvider, ISeparateFetchQuery, ISupportMainElement
	{
		static ZQuery CreateEmptyQuery()
		{
			var result = new ZQuery();
			result.modificationsEnabled = false;
			return result;
		}

		internal static ZQuery EmptyQuery
		{
			get { return emptyQuery ?? (emptyQuery = CreateEmptyQuery()); }
		}
		static ZQuery emptyQuery;

		public bool AllowTableValuedParameters { get; set; }
		public int Timeout { get; set; }

		public static ZQuery NoResultQuery
		{
			get
			{
				ZQuery query = new ZQuery();
				query.IsNoResultQuery = true;
				query.ModificationsEnabled = false;
				return query;
			}
		}

		void IFilterPart.DisableModifications()
		{
			//ModificationsEnabled = false;
		}

		public override bool HasParameters
		{
			get { return FilterParts.HasParameters; }
		}

		public override bool HasComparisonOperatorLike
		{
			get { return FilterParts.HasComparisonOperatorLike; }
		}

		public bool ModificationsEnabled
		{
			get { return modificationsEnabled; }
			set
			{
				if (value != ModificationsEnabled)
				{
					CheckModificationStatus();
					modificationsEnabled = value;
				}
			}
		}
		bool modificationsEnabled;

#if DEBUG
		// This will only be used in the unit test
		[ThreadSafe]
		public static ConcurrentBag<ZQuery> InstancesCacheForUnitTest = new();
		[ThreadSafe]
		public static volatile bool EnableRecordAllInstances = false;
#endif

		#region Constructors

		public ZQuery()
		{
			modificationsEnabled = true;
			DefaultJoinCondition = JC.And;
			IgnoreBlobFieldsCheck = false;
#if DEBUG
			if (EnableRecordAllInstances)
			{
				InstancesCacheForUnitTest.Add(this);
			}
#endif
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ZQuery(ZQuery filter)
			: this()
		{
			if (filter != null)
			{
				this.IgnoreBlobFieldsCheck = filter.IgnoreBlobFieldsCheck;
			}
			AddToFilter(filter);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ZQuery(ZQuery filter1, ZQuery filter2)
			: this()
		{
			AddToFilter(filter1);
			AddToFilter(filter2);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ZQuery(ZQuery filter1, JC joinCondition, ZQuery filter2)
			: this()
		{
			AddToFilter(filter1);
			AddToFilter(filter2, joinCondition);
		}

		internal ZQuery(JC joinCondition, IFilterPart filterPart)
			: this()
		{
			FilterParts.Append(joinCondition);
			FilterParts.Append(filterPart);
		}

		internal ZQuery(IFilterPart filterPart)
			: this()
		{
			FilterParts.Append(filterPart);

			IDbOnlyFilter dbOnlyFilter = filterPart as IDbOnlyFilter;
			if (dbOnlyFilter != null)
			{
				IsDBOnlyQuery |= dbOnlyFilter.IsDbOnlyFilter;
			}
		}

		#region Preferred Constructors

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ZQuery(SchemaColumn schemaColumn, object value)
			: this()
		{
			AddToFilter(schemaColumn, value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		internal ZQuery(string parameterName, SchemaColumn schemaColumn, object value) : this()
		{
			AddParameter(parameterName, value, schemaColumn, SQLComparisonOperator.Equal, JC.And, ComparisonOptions.None);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ZQuery(SchemaColumn schemaColumn, SQLComparisonOperator @operator, object value)
			: this()
		{
			AddToFilter(schemaColumn, @operator, value);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ZQuery(SchemaColumn schemaColumn, SQLComparisonOperator @operator, object value, ComparisonOptions options)
			: this()
		{
			AddToFilter(schemaColumn, @operator, value, options);
		}

		#endregion

		#endregion

		#region AddToFilter

		#region Merging two ZQuery objects together

		internal void AddToFilter(ZSQLInFilter inFilter, JC condition)
		{
			BracketIfRequired(condition);
			FilterParts.Append(condition, inFilter);
			IsDBOnlyQuery |= ((IDbOnlyFilter)inFilter).IsDbOnlyFilter;
		}

		public ZQuery AddToFilter(ZQuery sQLFilter)
		{
			AddToFilter(sQLFilter, DefaultJoinCondition);

			return this;
		}

		internal bool IsDBOnlyQuery; // temp till readonly factory
		public bool IsNoResultQuery;
		public bool IsUnionQuery;
		public bool ContainsZDBOnlyUnionQuery; //slightly different semantics from IsUnionQuery for backwards compatibility
		protected virtual bool CanAddFilter => true;
		public virtual bool SupportsFetchHints => true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ZQuery AddToFilter(ZQuery sQLFilter, JC condition, bool isForceToSetNoResultQuery = false)
		{
			if (sQLFilter != null && sQLFilter.CanAddFilter && this.CanAddFilter) //Cannot Add Filter to ZStoredProcedure and ZStoredProcedure to ZQuery
			{
				if (sQLFilter.IsNoResultQuery)
				{
					if (condition == JC.And || (isForceToSetNoResultQuery && condition == JC.Or && this.FilterParts.Count == 0))
					{
						IsNoResultQuery = true;
						FilterParts.Reset();
					}
					else if (condition != JC.Or)
					{
						throw new NotSupportedException("Unknown join condition detected - this condition must be coded for");
					}
				}
				else
				{
					if (condition == JC.Union || condition == JC.UnionAll || sQLFilter.IsUnionQuery)
					{
						IsUnionQuery = true;
						IsDBOnlyQuery = true;
					}
					else if (IsUnionQuery)
					{
						//actually add the non-union part to every individual unioned section
						foreach (var child in FilterParts.GetOutermostQueries())
						{
							child.AddToFilter(sQLFilter, condition);
						}
						return this;
					}
					else if (ContainsZDBOnlyUnionQuery)
					{
						//actually add the non-union part to the ZDBOnlyUnionQuery sections
						foreach (var child in FilterParts.GetOutermostQueries().OfType<ZDBOnlyUnionQuery>())
						{
							child.AddToFilter(sQLFilter, condition);
						}
						return this;
					}

					if (IsNoResultQuery && condition == JC.Or)
					{
						IsNoResultQuery = false;
					}

					AddUsedTables(sQLFilter);
					BracketIfRequired(condition);
					FilterParts.Append(condition, sQLFilter);
				}
				IncludeBlob(sQLFilter.LoadWithBlobs);

				if (sQLFilter.AddOptionRecompileConditionally)
				{
					AddOptionRecompileConditionally = true;
				}

				if (sQLFilter.ReLoadExistingRows)
				{
					ReLoadExistingRows = true;
				}

				if (sQLFilter.IgnoreDbQueryCache)
				{
					IgnoreDbQueryCache = true;
				}

				if (sQLFilter.FetchOnlyFromLocalCache)
				{
					FetchOnlyFromLocalCache = true;
				}

				if (sQLFilter.IsNoLock)
				{
					IsNoLock = true;
				}

				if (sQLFilter.isForceSeek)
				{
					IsForceSeek = true;
				}

				if (sQLFilter.QueryHints != QueryHints.None)
				{
					QueryHints = sQLFilter.QueryHints;
				}

				if (!(sQLFilter is ZDBOnlySubQuery))
				{
					if (sQLFilter.TableHints != TableHints.None)
					{
						TableHints = sQLFilter.TableHints;
					}

					if (sQLFilter.tableIndexHints != null)
					{
						foreach (var tableIndexHint in sQLFilter.tableIndexHints)
						{
							TableIndexHints.Add(tableIndexHint);
						}
					}
				}

				if (sQLFilter.IsDBOnlyQuery)
				{
					IsDBOnlyQuery = true;
				}

				if (sQLFilter.IgnoreActiveFilter)
				{
					IgnoreActiveFilter = true;
				}

				if (sQLFilter.IgnoreBlobFieldsCheck)
				{
					IgnoreBlobFieldsCheck = true;
				}

				if (!sQLFilter.OrderBy.IsEmpty)
				{
					if (OrderBy.IsEmpty)
					{
						OrderBy = sQLFilter.OrderBy;
					}
					else
					{
						OrderBy += ", " + sQLFilter.OrderBy;
					}
				}
			}

			return this;
		}

		void BracketIfRequired(JC joinOperator)
		{
			if (!FilterParts.FilterIsEmpty)
			{
				if (joinOperator == JC.And && FilterParts.ContainsNonBracketedOr)
				{
					FilterParts.Bracket();
				}
			}
		}

		#endregion

#if DEBUG
		#region Support for ToCSharpCode
		internal void AddToFilterForTesting(JC joinCondition, IFilterPart filterPart)
		{
			FilterParts.Append(joinCondition);
			FilterParts.Append(filterPart);
		}
		#endregion
#endif

		public ZQuery AddFilterAndZSQLParameterCollection(ZString filter, ZSqlParameterCollection parameterCollection, bool ignoreParameterSuffix = false)
		{
			return AddFilterAndZSQLParameterCollection(filter, parameterCollection, JC.And, ignoreParameterSuffix);
		}

		public ZQuery AddFilterAndZSQLParameterCollection(ZString filter, ZSqlParameterCollection parameterCollection, JC joinCondition, bool ignoreParameterSuffix = false)
		{
			if (parameterCollection == null)
			{
				parameterCollection = new ZSqlParameterCollection();
			}
			if (!ignoreParameterSuffix && filter.Contains(ParameterNameFactory.ParameterPrefix, StringComparison.OrdinalIgnoreCase))
			{
				ErrorReporter.ReportOnce(filter, "The filter contains an autogenerated parameter - this is incorrect and will make the filter fail when applied.\r\n\r\n" + filter + "\r\n\r\n");
			}
			BracketIfRequired(JC.And);
			FilterParts.Append(joinCondition, new ZNonPersistentDataQuery(filter, parameterCollection));
			return this;
		}

		#region Column Name / Value pair

		public ZQuery AddToFilter(SchemaColumn schemaColumn, object value)
		{
			return AddToFilter(DefaultJoinCondition, schemaColumn, SQLComparisonOperator.Equal, value);
		}

		public ZQuery AddToFilter(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value)
		{
			return AddToFilter(DefaultJoinCondition, schemaColumn, comparisonOperator, value);
		}

		public ZQuery AddToFilter(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value, ComparisonOptions options)
		{
			return AddToFilter(DefaultJoinCondition, schemaColumn, comparisonOperator, value, options);
		}

		public ZQuery AddToFilter_PossiblyCommaSeparated(SchemaColumn schemaColumn, object value, ComparisonOptions options = ComparisonOptions.Default, string delimiter = null)
		{
			return AddToFilter_PossiblyCommaSeparated(DefaultJoinCondition, schemaColumn, SQLComparisonOperator.Equal, value, options, delimiter);
		}

		public ZQuery AddToFilter_PossiblyCommaSeparated(JC joinOperator, SchemaColumn schemaColumn, object value, ComparisonOptions options = ComparisonOptions.Default, string delimiter = null)
		{
			return AddToFilter_PossiblyCommaSeparated(joinOperator, schemaColumn, SQLComparisonOperator.Equal, value, options, delimiter);
		}

		public ZQuery AddToFilter_PossiblyCommaSeparated(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value, ComparisonOptions options = ComparisonOptions.Default, string delimiter = null)
		{
			return AddToFilter_PossiblyCommaSeparated(DefaultJoinCondition, schemaColumn, comparisonOperator, value, options, delimiter);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI003", Justification = "Don't have access to the alternatives it wants")]
		public ZQuery AddToFilter_PossiblyCommaSeparated(JC joinOperator, SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value, ComparisonOptions options = ComparisonOptions.Default, string delimiter = null)
		{
			if (string.IsNullOrEmpty(delimiter))
			{
				delimiter = ObjectFactory.Get<IEntityFrameworkSettings>().MultiSearchSeparator;
			}

			if (value is ZString stringValue)
			{
				if (string.IsNullOrEmpty(delimiter)) //registry setting has been disabled - Left and do normal case
				{
					value = stringValue.Left(schemaColumn.MaxLength);
					return AddToFilter(joinOperator, schemaColumn, comparisonOperator, value, options);
				}

				if (stringValue.Contains(delimiter, StringComparison.Ordinal) &&
				(
				comparisonOperator == SQLComparisonOperator.Equal
				|| comparisonOperator == SQLComparisonOperator.NotEqual
				|| comparisonOperator == SQLComparisonOperator.StartsWith
				|| comparisonOperator == SQLComparisonOperator.EndsWith
				|| comparisonOperator == SQLComparisonOperator.Contains
				)
				)
				{
					IEnumerable<ZString> valueList = stringValue.Split(delimiter);
					if (schemaColumn.MaxLength > 0)
					{
						valueList = valueList.Select(x => x.Trim().Left(schemaColumn.MaxLength)).Where(x => !string.IsNullOrEmpty(x));
					}
					return AddToFilter(joinOperator, schemaColumn, comparisonOperator, valueList, options);
				}

				//have to also crop in the case where it's not comma separated
				if (schemaColumn.MaxLength > 0)
				{
					value = stringValue.Left(schemaColumn.MaxLength);
				}
			}

			return AddToFilter(joinOperator, schemaColumn, comparisonOperator, value, options);
		}

		public ZQuery AddToFilter(JC joinOperator, SchemaColumn schemaColumn, object value)
		{
			return AddToFilter(joinOperator, schemaColumn, SQLComparisonOperator.Equal, value);
		}

		public ZQuery AddToFilter(JC joinOperator, SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value)
		{
			return AddToFilter(joinOperator, schemaColumn, comparisonOperator, value, ComparisonOptions.Default);
		}

		/// <summary>
		/// Ultimate calling path for ALL AddToFilter methods.
		/// </summary>
		public ZQuery AddToFilter(JC joinOperator, SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, object value, ComparisonOptions options)
		{
			BracketIfRequired(joinOperator);
			AddParameter(value, schemaColumn, comparisonOperator, joinOperator, options);

			return this;
		}

		#endregion

		#region Column Name / Column Name pair

		public ZQuery AddToFilter(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, SchemaColumn withSchemaColumn)
		{
			if (withSchemaColumn == null)
			{
				AddToFilter(DefaultJoinCondition, schemaColumn, comparisonOperator, null);
			}
			else
			{
				ZSQLColumnComparer filter = new ZSQLColumnComparer(schemaColumn, comparisonOperator, withSchemaColumn);
				BracketIfRequired(DefaultJoinCondition);
				FilterParts.Append(DefaultJoinCondition, filter);
			}

			return this;
		}

		#endregion

		#endregion

		#region FilterByForeignKey

		public void FilterByForeignKey(SchemaGuidColumn schemaColumn, BusinessObject[] businessObjects)
		{
			if (businessObjects != null && businessObjects.Length > 0)
			{
				ZGuid[] guids = new ZGuid[businessObjects.Length];
				for (int i = 0; i < businessObjects.Length; i++)
				{
					guids[i] = businessObjects[i].PK;
				}
				AddToFilter(schemaColumn, guids);
			}
			else
			{   // Invalidates query, you're filtering by nothing
				IsNoResultQuery = true;
			}
		}

		#endregion

		#region BlobFilters

		public IEnumerable<SchemaColumn> BlobFilters
		{
			get { return FilterParts.BlobFilters; }
		}

		#endregion

		/// <summary>
		/// The default join condition; used when adding to the filter without stating a join condition.
		/// The default value is AND
		/// </summary>
		public JC DefaultJoinCondition
		{
			get { return defaultJoinCondition; }
			set
			{
				CheckModificationStatus();
				defaultJoinCondition = value;
			}
		}
		JC defaultJoinCondition;

		/// <summary>
		/// Indicates to refresh existing data from permanent storage where applicable
		/// </summary>
		public bool ReLoadExistingRows
		{
			get { return reLoadExistingRows; }
			set
			{
				CheckModificationStatus();
				reLoadExistingRows = value;
			}
		}
		bool reLoadExistingRows;

		public bool IgnoreDbQueryCache
		{
			get => ignoreDbQueryCache;
			set
			{
				CheckModificationStatus();
				ignoreDbQueryCache = value;
			}
		}
		bool ignoreDbQueryCache;

		public int GetMaxLengthOfUsedColumns()
		{
			return FindMaxLengthOfUsedColumns(this);
		}

		int FindMaxLengthOfUsedColumns(IFilterPart filterPart)
		{
			int maxLength = 0;

			var sqlParameterfilterpart = filterPart as ZSqlParameter;
			if (sqlParameterfilterpart != null && sqlParameterfilterpart.SchemaColumn != null)
			{
				maxLength = sqlParameterfilterpart.SchemaColumn.MaxLength;
			}

			var filterPartsProvider = filterPart as IFilterPartsProvider;
			if (filterPartsProvider != null)
			{
				for (int i = 0; i < filterPartsProvider.FilterParts.Length; i++)
				{
					int currentMaxLength = FindMaxLengthOfUsedColumns(filterPartsProvider.FilterParts[i]);
					if (currentMaxLength > maxLength)
					{
						maxLength = currentMaxLength;
					}
				}
			}

			return maxLength;
		}

		/// <summary>
		/// Indicates to never hit the DB to get results - only fetch from local cache
		/// </summary>
		public bool FetchOnlyFromLocalCache
		{
			get { return fetchOnlyFromLocalCache; }
			set
			{
				CheckModificationStatus();
				fetchOnlyFromLocalCache = value;
			}
		}
		bool fetchOnlyFromLocalCache;

		/// <summary>
		/// Set the maximum number of rows to return - Leave as default value for all available rows
		/// </summary>
		public int? MaximumRows
		{
			get { return maximumRows; }
			set
			{
				CheckModificationStatus();
				maximumRows = value;
			}
		}
		int? maximumRows;

		/// <summary>
		/// Indicates if NOLOCK hints (Isolation Level Read Uncommitted) will be used for the query - Default is set in Registry
		/// </summary>
		public bool IsNoLock
		{
			get
			{
				return isNoLock;
			}
			set
			{
				CheckModificationStatus();
				isNoLock = value;
			}
		}
		bool isNoLock;

		public bool IsForceSeek
		{
			get
			{
				if (isForceSeek)
				{
					return true;
				}
				return ShouldDefaultToForceSeek();
			}
			set
			{
				CheckModificationStatus();
				isForceSeek = value;
			}
		}
		internal bool isForceSeek;

		protected internal bool ShouldDefaultToForceSeek()
		{
			if (!ObjectFactory.Get<IEntityFrameworkSettings>().DefaultToForceSeek)
			{
				return false;
			}
			//DB specific subqueries can have anything in them, so can't really be introspected
			if (IsDBOnlyQuery)
			{
				return false;
			}
			//Algorithm: Return true if the query is entirely equality checks against one or more foreign key columns.
			//(Incomplete) information on what a FORCESEEK using query is allowed to do: https://docs.microsoft.com/en-us/sql/t-sql/queries/hints-transact-sql-table?view=sql-server-ver15
			QueryState queryState = new QueryState();
			var result = QueriesOneOrMoreIndexedColumns(ref queryState, this.FilterParts) && queryState.ColumnName != null;
			return result;
		}

		const int MAX_INFILTER_ALLOWED_FOR_FORCESEEK = 64;

		protected struct QueryState
		{
			public string ColumnName;
			public bool HasHugeInFilter;
			public bool? HasOr;

			public override bool Equals(object obj)
			{
				return obj is QueryState qs2 && qs2.ColumnName == ColumnName && qs2.HasHugeInFilter == HasHugeInFilter && qs2.HasOr == HasOr;
			}

			public override int GetHashCode()
			{
				return ColumnName.GetHashCode() + 7 * HasHugeInFilter.GetHashCode() + 13 * HasOr.GetHashCode();
			}

			public static bool operator ==(QueryState qs1, QueryState qs2)
			{
				return qs1.Equals(qs2);
			}

			public static bool operator !=(QueryState qs1, QueryState qs2)
			{
				return !(qs1 == qs2);
			}
		}

		bool QueriesOneOrMoreIndexedColumns(ref QueryState queryState, IEnumerable<IFilterPart> curFilterParts)
		{
			foreach (var filterPart in curFilterParts)
			{
				//these queries/filters can contain arbitrary SQL, so can't really be introspected
				if ((filterPart is IDbOnlyFilter dbOnly && dbOnly.IsDbOnlyFilter) || filterPart is ZNonPersistentDataQuery)
				{
					return false;
				}

				//recursively explore queries and filter brackets
				if (filterPart is ZQuery partialQuery && !QueriesOneOrMoreIndexedColumns(ref queryState, partialQuery.FilterParts))
				{
					return false;
				}

				if (filterPart is FilterBracket filterBracket && !QueriesOneOrMoreIndexedColumns(ref queryState, filterBracket.FilterParts))
				{
					return false;
				}

				SchemaColumn column = null;

				if (filterPart is ZSqlParameter parameter)
				{
					if (parameter.Value == DBNull.Value)
					{
						return false;
					}
					column = parameter.SchemaColumn;
				}

				if (filterPart is ZSQLInFilter inFilter)
				{
					//FORCESEEK is not allowed for IN (65 or more values) if there's also an OR in the query. (NOT IN 65+ seems to work, but I'm not going to special case that to work since it's safer to not try to FORCESEEK if we're not sure.)
					var count = inFilter.GetValues().Take(MAX_INFILTER_ALLOWED_FOR_FORCESEEK + 1).Count();
					if (count > MAX_INFILTER_ALLOWED_FOR_FORCESEEK)
					{
						queryState.HasHugeInFilter = true;
						if (queryState.HasOr == null)
						{
							queryState.HasOr = this.ContainsOrOperator;
						}
						if (queryState.HasOr.Value)
						{
							return false;
						}
					}
					if (count > 0)
					{
						column = inFilter.Column;
					}
				}

				if (column != null)
				{
					queryState.ColumnName = column.Name;
					if (!IsIndexed(column))
					{
						return false;
					}
				}
			}

			return true;
		}

		protected internal static bool IsIndexed(SchemaColumn column)
		{
			if (column is SchemaGuidColumn guidColumn)
			{
				return guidColumn.Indexed;
			}
			return false;
		}

		/// <summary>
		/// Ignores the ActiveFilter for the business object when retrieving.
		/// </summary>
		public bool IgnoreActiveFilter;

		ZString orderBy;
		/// <summary>
		/// Indicates the sort order used to retrieve rows.  May be suffixed with ascending/descending as per SQL syntax
		/// </summary>
		public ZString OrderBy
		{
			get
			{
				return orderBy;
			}
			set
			{
				orderBy = value;
				if (!orderBy.IsEmpty)
				{
					List<ZString> columns = new List<ZString>(orderBy.Split(','));
					Dictionary<ZString, ZString> resultColumns = new Dictionary<ZString, ZString>();
					foreach (ZString column in columns)
					{
						ZString columnName = column.ToLower().Replace(" asc", "").Replace(" desc", "").Trim();
						if (!resultColumns.ContainsKey(columnName))
						{
							resultColumns.Add(columnName, column);
						}
					}
					orderBy = ZString.Join(",", resultColumns.Values.ToArray());
				}
			}
		}

		public bool IsPrimaryKeyQuery
		{
			get
			{
				ZSqlParameter sqlParam = GetMostUniqueSingleEqualParameter();
				return sqlParam != null
						&& sqlParam.SchemaColumn.IsPKColumn
						&& sqlParam.ComparisonOperator == SQLComparisonOperator.Equal;
			}
		}

		/// <summary>
		/// Returns the most efficient 'single' 'Equals' parameter from a filter when it is the only parameter in a bracket
		/// Returns null in all other cases
		/// </summary>
		/// <returns></returns>
		public ZSqlParameter GetMostUniqueSingleEqualParameter()
		{
			return FilterParts.GetMostUniqueSingleEqualParameter();
		}

		public void CheckModificationStatus()
		{
			if (!ModificationsEnabled)
			{
				ErrorReporter.ReportOnce("ModificationsEnabled", "Modifications have been disabled on this filter. No further changes should be made. Check the origin of this object (AdditionalFilter / RelationshipFilter etc)");
			}
		}

		public bool IsTableUsedInQuery(string tableName)
		{
			return TableList.Contains(tableName);
		}

		protected void AddUsedTables(ZQuery query)
		{
			ZDBOnlyQuery dBQuery = query as ZDBOnlyQuery;
			if (dBQuery != null)
			{
				AddUsedTable(dBQuery.TableName);
			}

			foreach (string tableName in query.TableList)
			{
				AddUsedTable(tableName);
			}
		}

		protected void AddUsedTable(string tableName)
		{
			if (!TableList.Contains(tableName))
			{
				TableList.Add(tableName);
			}
		}

		public (string sql, ZSqlParameter[] parameters) GetAsCompleteSQLStatementWithParameters(string tableName)
		{
			var builder = new SqlBuilder();
			AddAsCompleteSQLStatement(builder, tableName, combineFilterAndParams: false, selectList: null);
			return (builder.ToString(), builder.Parameters.ToArray());
		}

		public ZString GetAsCompleteSQLStatement(string tableName, bool combineFilterAndParams, SchemaColumn[] selectList = null)
		{
			var builder = new SqlBuilder();
			AddAsCompleteSQLStatement(builder, tableName, combineFilterAndParams, selectList);
			return builder.ToString();
		}

		public virtual void AddAsCompleteSQLStatement(SqlBuilder sqlBuilder, string tableName, bool combineFilterAndParams, SchemaColumn[] selectList = null)
		{
			sqlBuilder.Append("SELECT ");

			bool isTopNQuery = MaximumRows >= 0;

			bool useOverBy = false;
			if (isTopNQuery)
			{
				if (MaximumRows > 1 && ObjectFactory.Get<IEntityFrameworkSettings>().RunSelectTopNAsRowNumberQuery)
				{
					SchemaColumn pkColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumnSafe(tableName);
					if (pkColumn != null)
					{
						string orderByIncludingPrimaryKey = OrderBy;
						bool orderByAlreadyContainsPrimaryKey = OrderBy.ToString().IndexOf(pkColumn.Name, StringComparison.OrdinalIgnoreCase) > -1;
						if (!OrderBy.IsEmpty)
						{
							if (!orderByAlreadyContainsPrimaryKey)
							{
								orderByIncludingPrimaryKey = orderByIncludingPrimaryKey + ", " + pkColumn.Name;
							}
						}
						else
						{
							orderByIncludingPrimaryKey = pkColumn.Name;
						}

						AddColumnListToSelect(sqlBuilder, tableName, selectList);
						sqlBuilder.Append((NoResString)" FROM (SELECT ROW_NUMBER() OVER (ORDER BY ").Append(orderByIncludingPrimaryKey).Append((NoResString)") RowN, *");
						useOverBy = true;
					}
				}
				if (!useOverBy)
				{
					sqlBuilder.Append(" TOP ").Append(MaximumRows);
				}
			}

			if (!useOverBy)
			{
				sqlBuilder.Append("\r\n");
				AddColumnListToSelect(sqlBuilder, tableName, selectList);
			}

			sqlBuilder.Append("\r\n\tFROM ")
			.Append(AddSchemaName(tableName));

			if (this.TableHints != TableHints.None ||
				this.tableIndexHints != null ||
				this.IsForceSeek ||
				this.IsNoLock)
			{
				AddTableHints(sqlBuilder);
			}
			sqlBuilder.FinaliseSelectStatement();
			AddAsWhereAndOrderByClause(sqlBuilder, combineFilterAndParams, useOverBy);
		}

		/// <summary>
		/// Only use as a last resort. Table hints are for special cases only.
		/// </summary>
		public TableHints TableHints { get; set; } = TableHints.None;

		protected void AddTableHints(SqlBuilder sqlBuilder)
		{
			// Temporarily add IsNoLock and IsForceSeek as these flags propogate to all tables in the query, while
			// table hints is for each table.
			var allTableHints = this.TableHints;

			if (this.IsNoLock)
			{
				allTableHints |= TableHints.NOLOCK;
			}

			if (this.IsForceSeek)
			{
				allTableHints |= TableHints.FORCESEEK;
			}

			if (allTableHints != TableHints.None || (tableIndexHints != null && tableIndexHints.Count > 0))
			{
				sqlBuilder.Append((NoResString)" WITH (");
				bool indexHintNeedsComma = false;
				Enum.GetValues(typeof(TableHints))
					.Cast<TableHints>()
					.Where(tableHint => allTableHints.HasFlag(tableHint) && tableHint != TableHints.None)
					.ForEachWithBetween(tableHint =>
					{
						indexHintNeedsComma = true;
						sqlBuilder.Append(TableHintNames.Name(tableHint));
					}, () => sqlBuilder.Append(", "));

				if (tableIndexHints != null && TableIndexHints.Count != 0)
				{
					if (indexHintNeedsComma)
					{
						sqlBuilder.Append(", ");
					}

					sqlBuilder.Append("INDEX(");
					TableIndexHints.Select(tableIndexHint => tableIndexHint.IndexName).ForEachWithBetween(indexName => sqlBuilder.Append(indexName), () => sqlBuilder.Append(", "));
					sqlBuilder.Append(")");
				}
				sqlBuilder.Append(")");
			}
		}

		/// <summary>
		/// Forces SQL server to use a specific index.
		/// </summary>
		/// <remarks>
		/// Only use as a last resort, indicates the need for a re-design.
		/// </remarks>
		public ICollection<TableIndexHint> TableIndexHints
		{
			get
			{
				return this.tableIndexHints ?? (this.tableIndexHints = new List<TableIndexHint>());
			}
		}

		List<TableIndexHint> tableIndexHints;

		public void ClearTableIndexHintsIncludingSubQueries()
		{
			tableIndexHints?.Clear();
			foreach (var query in FilterParts.GetOutermostQueries())
			{
				query.ClearTableIndexHintsIncludingSubQueries();
			}
		}

		public void DisableForceSeekIncludingSubQueries()
		{
			IsForceSeek = false;
			foreach (var query in FilterParts.GetOutermostQueries())
			{
				query.DisableForceSeekIncludingSubQueries();
			}
		}

		/// <summary>
		/// Only use as a last resort. Table hints are for special cases only.
		/// </summary>
		public QueryHints QueryHints { get; set; } = QueryHints.None;

		protected void AddQueryHints(SqlBuilder sqlBuilder)
		{
			var queryHints = this.QueryHints;
			if (queryHints != QueryHints.None)
			{
				IEnumerable<string> GetQueryHintNames()
				{
					foreach (QueryHints queryHint in Enum.GetValues(typeof(QueryHints)))
					{
						if (queryHints.HasFlag(queryHint) && queryHint != QueryHints.None)
						{
							yield return QueryHintNames.Name(queryHint);
						}
					}
				}

				sqlBuilder.Append((NoResString)" OPTION (");
				GetQueryHintNames().ForEachWithBetween(a => sqlBuilder.Append(a), () => sqlBuilder.Append(", "));
				sqlBuilder.Append(")");
			}
		}

		public void ClearBlobs()
		{
			loadWithBlobs = null;
		}

		public void IncludeBlob(IEnumerable<SchemaColumn> columns)
		{
			if (columns != null && columns.Any())
			{
				loadWithBlobs = loadWithBlobs == null ? columns.ToArray() : loadWithBlobs.Union(columns).ToArray();
			}
		}

		public void IncludeBlob(SchemaColumn column)
		{
			var set = new[] { column };
			loadWithBlobs = loadWithBlobs == null ? set : loadWithBlobs.Union(set).ToList();
		}

		public ICollection<SchemaColumn> LoadWithBlobs
		{
			get { return loadWithBlobs ?? Array.Empty<SchemaColumn>(); }
		}
		ICollection<SchemaColumn> loadWithBlobs;

		public ushort LoadSmallBlobs
		{
			get { return loadSmallBlobs; }
			set { loadSmallBlobs = value; }
		}
		ushort loadSmallBlobs = 1024;

		public bool IgnoreBlobFieldsCheck { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Sql statement - do not localize")]
		void AddColumnListToSelect(SqlBuilder result, string tableName, SchemaColumn[] selectList = null)
		{
			IList<SchemaColumn> schemaColumns;
			try
			{
				schemaColumns = GetSelectList(tableName, selectList);
			}
			catch (InvalidTableNameException)
			{
				result.Append(" * ");
				return; // Trying to query a table that doesn't exist, so we've decided to select *.
			}

			for (int index = 0; index < schemaColumns.Count; index++)
			{
				var schemaColumn = schemaColumns[index];
				if (index != 0)
				{
					result.Append(index % 5 == 0 ? ",\r\n" : ", ");
				}
				if (schemaColumn.IsLargeBinaryOrText && !IgnoreBlobFieldsCheck && !LoadWithBlobs.Contains(schemaColumn) && !BlobFilters.Contains(schemaColumn))
				{
					if (LoadSmallBlobs > 0)
					{
						//TODO: have to do this because xml columns can't be UNIONed together otherwise. Verify that this is a valid thing to do globally
						result.Append((NoResString)"case when ")
							.Append(schemaColumn.Name)
							.Append((NoResString)" is null then null when datalength(")
							.Append(schemaColumn.Name)
							.Append(") < ")
							.Append(LoadSmallBlobs)
							.Append((NoResString)" then ")
							.Append(schemaColumn.SqlDbType == System.Data.SqlDbType.Xml ? string.Format(CultureInfo.InvariantCulture, "cast({0} as nvarchar(max))", schemaColumn.Name) : schemaColumn.Name)
							.Append((NoResString)" else ")
							.Append(LazyLoading.SqlPlaceholder(schemaColumn))
							.Append((NoResString)" end as ")
							.Append(schemaColumn.Name);
					}
					else
					{
						result
							.Append(LazyLoading.SqlPlaceholder(schemaColumn))
							.Append(" as ")
							.Append(schemaColumn.Name);
					}
				}
				else if (schemaColumn is SchemaGeographyColumn)
				{
					result.Append("CAST(").Append(schemaColumn.Name).Append(" AS varbinary(max)) AS ").Append(schemaColumn.Name);
				}
				else
				{
					result.Append(schemaColumn.Name);
				}
			}
		}

		protected static IList<SchemaColumn> GetSelectList(string tableName, SchemaColumn[] selectList = null)
		{
			if ((selectList?.Length ?? 0) > 0)
			{
				if (selectList.All(col => col.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))) // Why is this check here?
				{
					return selectList.Where(col => !col.IsComputed).ToList();
				}
			}

			return ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumns(tableName);
		}

		public ZString GetAsWhereClause(bool combineFilterAndParams, string append = "")
		{
			var builder = new SqlBuilder(combineFilterAndParams ? SqlBuilder.QueryType.Literal : SqlBuilder.QueryType.Parameterised);
			AddAsWhereClause(builder, combineFilterAndParams, append);
			return builder.ToString();
		}

		/// <summary>
		/// Add the where clause to given builder.
		/// </summary>
		/// <param name="sqlBuilder"></param>
		/// <param name="combineFilterAndParams">Set to true to have parameters occur as literals in the SQL.
		/// Set false to have parameterized SQL. The params can be found in the return value.</param>
		/// <param name="append"></param>
		/// <returns>parameters when combineFilterAndParams is false, null otherwise</returns>
		void AddAsWhereClause(SqlBuilder sqlBuilder, bool combineFilterAndParams, string append)
		{
			using (sqlBuilder.WithPrefix("\r\n\tWHERE "))
			{
				if (combineFilterAndParams)
				{
					AddLiteralTextADO(sqlBuilder);
				}
				else
				{
					AddFilterString(sqlBuilder);
				}
			}

			sqlBuilder.Append(append);

			if (QueryHints == QueryHints.None)
			{
				if (AddOptionRecompileConditionally && ObjectFactory.Get<IEntityFrameworkSettings>().IsWeb) //don't add IsWebService without testing that it improves performance, please
				{
					sqlBuilder.Append((NoResString)" OPTION (RECOMPILE)"); // code for developers only
				}
				else if (AddOptionRecompileConditionally && HasComparisonOperatorLike && ObjectFactory.Get<IEntityFrameworkSettings>().ApplyOptionRecompile)
				{
					sqlBuilder.Append((NoResString)" OPTION (RECOMPILE)"); // code for developers only
				}
			}
			else
			{
				AddQueryHints(sqlBuilder);
			}
		}

		public ZString GetAsWhereAndOrderByClause(bool combineFilterAndParams, bool useOverBy = false)
		{
			var sqlBuilder = new SqlBuilder(combineFilterAndParams ? SqlBuilder.QueryType.Literal : SqlBuilder.QueryType.Parameterised);
			AddAsWhereAndOrderByClause(sqlBuilder, combineFilterAndParams, useOverBy);
			return sqlBuilder.ToString();
		}

		void AddAsWhereAndOrderByClause(SqlBuilder sqlBuilder, bool combineFilterAndParams, bool useOverBy)
		{
			var append = "";

			if (useOverBy)
			{
				append += (NoResString)") InnerQuery where RowN <= " + MaximumRows.ToString();
			}
			else if (!OrderBy.IsEmpty)
			{
				if (IsDBOnlyQuery || IsTopNQuery)
				{
					append += (NoResString)"\r\n\tORDER BY " + OrderBy;
				}
			}

			AddAsWhereClause(sqlBuilder, combineFilterAndParams, append);
		}

		public bool IsEmpty
		{
			get { return FilterParts.FilterIsEmpty && OrderBy == "" && !IsTopNQuery && !IsNoResultQuery; }
		}

#if DEBUG
		#region ToCSharp

		public string ToCSharpCode()
		{
			string innerVariable = "";
			return ToCSharpCode("", new NumberPublisher(), out innerVariable);
		}
		protected virtual string GetCSharpConstructor(string variableName)
		{
			return "ZQuery " + variableName + " = new ZQuery();";
		}

		internal string ToCSharpCode(string initialJoinCondition, NumberPublisher publisher, out string queryVariable)
		{
			StringBuilder stringBuilder = new StringBuilder();
			queryVariable = "query" + publisher.GetNextVariableNumberSuffix().ToString();
			stringBuilder.AppendLine(GetCSharpConstructor(queryVariable));
			if (IsDBOnlyQuery && this.GetType() == typeof(ZQuery))  // DBOnlyQuery will set this automatically
			{
				stringBuilder.AppendLine(queryVariable + ".IsDBOnlyQuery = true;");
			}
			if (MaximumRows != null)
			{
				stringBuilder.AppendLine(queryVariable + ".MaximumRows = " + MaximumRows.ToString() + ";");
			}
			if (!OrderBy.IsEmpty)
			{
				stringBuilder.AppendLine(queryVariable + ".OrderBy = \"" + OrderBy + "\";");
			}

			stringBuilder.Append(FilterParts.ToCSharpCode(initialJoinCondition, publisher, queryVariable));
			return stringBuilder.ToString();
		}
		#endregion
#endif

		/// <summary>
		/// FilterString is the filter with containing parameters as '@blah'
		/// </summary>
		public string FilterString
		{
			get { return ParameterisedText.ParameterisedQueryText; }
		}

		public void AddFilterString(SqlBuilder sqlBuilder)
		{
			ParameterisedSql(sqlBuilder);
		}

		/// <summary>
		/// Contains the parameter collection for passing to the datarow generator
		/// </summary>
		public ZSqlParameter[] Params
		{
			get { return FilterParts.Params(); }
		}

		public ZQuery[] GetCompositeParts()
		{
			return FilterParts.GetCompositeParts();
		}

		public ZQuery[] GetOrParts()
		{
			return FilterParts.GetOrParts();
		}

		public ZQuery[] GetAndParts()
		{
			return FilterParts.GetAndParts();
		}

		public bool IsTopNQuery
		{
			get { return MaximumRows != null; }
		}

		#region Implementation

		const int MinimumCountInValuesListToGenerateZSQLInFilter = 10;

		internal StringCollectionX TableList
		{
			get { return tableList ?? (tableList = new StringCollectionX()); }
		}
		StringCollectionX tableList;

		internal FilterStringBuilder FilterParts
		{
			get { return filterParts ?? (filterParts = new FilterStringBuilder(1)); }
			private set { filterParts = value; }
		}
		FilterStringBuilder filterParts;

		protected void AddPKParameter(ZGuid pK)
		{
			BracketIfRequired(JC.And);
			FilterParts.Append(JC.And, ZSqlParameter.New("@PK", pK, CargoWise.Schema.Schema.GenericGuidSchemaColumn));
		}

		protected void AddCodeParameter(string value)
		{
			BracketIfRequired(JC.And);
			FilterParts.Append(JC.And, ZSqlParameter.New("@Code", value, CargoWise.Schema.Schema.GenericGuidSchemaColumn, SQLComparisonOperator.Equal));
		}

		protected internal void AddParameter(object value, SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, JC condition, ComparisonOptions options)
		{
			AddParameter("", value, schemaColumn, comparisonOperator, condition, options);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected internal void AddParameter(string parameterName, object value1, SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, JC joinOperator, ComparisonOptions options)
		{
			var savedIsNoResultQuery = IsNoResultQuery;

			if (joinOperator == JC.Union || joinOperator == JC.UnionAll)
			{
				IsUnionQuery = true;
				IsDBOnlyQuery = true;
			}
			else if (IsUnionQuery)
			{
				//actually add the non-union part to every individual unioned section
				foreach (var child in FilterParts.GetOutermostQueries())
				{
					child.AddParameter(parameterName, value1, schemaColumn, comparisonOperator, joinOperator, options);
				}
				return;
			}
			else if (ContainsZDBOnlyUnionQuery)
			{
				//actually add the non-union part to the ZDBOnlyUnionQuery sections
				foreach (var child in FilterParts.GetOutermostQueries().OfType<ZDBOnlyUnionQuery>())
				{
					child.AddParameter(parameterName, value1, schemaColumn, comparisonOperator, joinOperator, options);
				}
				return;
			}

			if (IsNoResultQuery && joinOperator == JC.Or)
			{
				IsNoResultQuery = false;
			}

			ICollection values = value1 as ICollection;
			if (values == null && !(value1 is string))
			{
				IEnumerable enumerable = value1 as IEnumerable;
				if (enumerable != null)
				{
					values = enumerable.Cast<object>().ToList();
				}
			}

			if (values != null && !(values is byte[] && schemaColumn is SchemaBinaryColumn))
			{
				if (values.Count == 0 && comparisonOperator == SQLComparisonOperator.Equal)
				{
					if (savedIsNoResultQuery || joinOperator != JC.Or)
					{
						IsNoResultQuery = true;
					}
				}
				else if (
					(
						values.Count < MinimumCountInValuesListToGenerateZSQLInFilter
						&& comparisonOperator.In(SQLComparisonOperator.StartsWith, SQLComparisonOperator.EndsWith)
					)
					|| comparisonOperator == SQLComparisonOperator.Contains
				)
				{
					ZQuery joinQuery = new ZQuery { DefaultJoinCondition = JC.Or };
					foreach (object value in values)
					{
						joinQuery.AddToFilter(schemaColumn, comparisonOperator, value);
					}

					AddToFilter(joinQuery, joinOperator);
				}
				else
				{
					ZSQLInFilter inFilter = new ZSQLInFilter(schemaColumn, comparisonOperator, values, options, this.AllowTableValuedParameters);
					inFilter.ConstantsTempTableID = constantsTempTableID;
					AddToFilter(inFilter, joinOperator);
					constantsTempTableID++;
				}
			}
			else if (comparisonOperator == SQLComparisonOperator.EqualToDatePartOnly)
			{
				if (schemaColumn.ColumnType == SchemaColumnType.DateTimeOffset)
				{
					ZDateTimeOffset valueAsDateTimeOffset = ZDateTimeOffset.Empty;
					if (value1 != null)
					{
						ZDateTimeOffsetTypeConverter converter = ZDateTimeOffsetTypeConverter.Instance;
						if (converter.CanConvertFrom(value1.GetType()))
						{
							valueAsDateTimeOffset = (ZDateTimeOffset)converter.ConvertFrom(value1);
						}
						else
						{
							throw new ArgumentException("Cannot use DatePart comparison for value that cannot be converted to a DateTimeOffset");
						}
					}

					ZQuery filter = new ZQuery();
					if (valueAsDateTimeOffset.IsEmpty)
					{
						filter.AddToFilter(schemaColumn, SQLComparisonOperator.Equal, value1);
					}
					else
					{
						filter.AddToFilter(schemaColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, valueAsDateTimeOffset);
						filter.AddToFilter(schemaColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, valueAsDateTimeOffset);
					}

					AddToFilter(filter, joinOperator);
				}
				else //SchemaColumnType.DateTime
				{
					ZDateTime valueAsDateTime = ZDateTime.Empty;
					if (value1 != null)
					{
						ZDateTimeTypeConverter converter = ZDateTimeTypeConverter.Instance;
						if (converter.CanConvertFrom(value1.GetType()))
						{
							valueAsDateTime = (ZDateTime)converter.ConvertFrom(value1);
						}
						else
						{
							throw new ArgumentException("Cannot use DatePart comparison for value that cannot be converted to a DateTime");
						}
					}

					ZQuery filter = new ZQuery();
					if (valueAsDateTime.IsEmpty)
					{
						filter.AddToFilter(schemaColumn, SQLComparisonOperator.Equal, value1);
					}
					else
					{
						filter.AddToFilter(schemaColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, valueAsDateTime);
						filter.AddToFilter(schemaColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, valueAsDateTime);
					}

					AddToFilter(filter, joinOperator);
				}
			}
			else
			{
				ZSqlParameter param = ZSqlParameter.New(parameterName, value1, schemaColumn, comparisonOperator, options);
				FilterParts.Append(joinOperator, param);
			}
		}

		int constantsTempTableID;

		#endregion

		#region IFilterPart Members

		public bool ContainsOrOperator
		{
			get { return FilterParts.ContainsOrOperator; }
		}

		bool IFilterPart.ContainsOrOperator
		{
			get { return FilterParts.ContainsOrOperator; }
		}

		ZNonPersistentDataQuery IFilterPart.ParameterisedSql(ParameterNameFactory factory)
		{
			return ParameterisedSql(factory);
		}

		void IFilterPart.ParameterisedSql(SqlBuilder sqlBuilder)
		{
			ParameterisedSql(sqlBuilder);
		}

		protected ZNonPersistentDataQuery ParameterisedSql(ParameterNameFactory factory)
		{
			return GetDataQuery(new SqlBuilder(factory));
		}

		public ZNonPersistentDataQuery GetDataQuery(SqlBuilder sqlBuilder)
		{
			ParameterisedSql(sqlBuilder);
			return new ZNonPersistentDataQuery(sqlBuilder.ToString(), sqlBuilder.GetParameters());
		}

		protected virtual void ParameterisedSql(SqlBuilder sqlBuilder)
		{
			FilterParts.ParameterisedSql(sqlBuilder);
		}

		public ZNonPersistentDataQuery ParameterisedText
		{
			get { return ParameterisedSql(new ParameterNameFactory()); }
		}

		internal string LiteralTextSql
		{
			get { return new ZNonPersistentDataQuery(FilterString, Params).LiteralTextSql; }
		}

		/// <summary>
		/// LiteralTextADO is the filter with all parameters '@foo' changed to standard easy reading SQL.
		/// </summary>
		public string LiteralTextADO
		{
			get
			{
				var sqlBuilder = new SqlBuilder(SqlBuilder.QueryType.LiteralADO);
				AddLiteralTextADO(sqlBuilder);
				return sqlBuilder.ToString();
			}
		}

		protected internal virtual void AddLiteralTextADO(SqlBuilder sqlBuilder)
		{
			FilterParts.AddLiteralTextADO(sqlBuilder);
		}

		void IFilterPart.AddLiteralTextADO(SqlBuilder sqlBuilder)
		{
			AddLiteralTextADO(sqlBuilder);
		}

		public string FilterPartsHashKey => LiteralTextADO;

		public IEnumerable<string> BlobPartsHashKey => LoadWithBlobs.Count > 1 ? LoadWithBlobs.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).Select(x => x.Name) : LoadWithBlobs.Select(s => s.Name);

		public virtual IQueryHashKey GetHashKey()
		{
			var list = new FetchHint.EnumerableHashObject(3);
			list.Add(LiteralTextADO);

			foreach (var blob in BlobPartsHashKey)
			{
				list.Add(blob);
			}

			if (MaximumRows.HasValue)
			{
				list.Add(MaximumRows.Value);
			}

			if (!OrderBy.IsEmpty)
			{
				list.Add(OrderBy);
			}
			return list;
		}

		/// <summary>
		/// Returns the LiteralTextADO of the query as formatted text.
		/// Inner queries are indented, line breaks added after ands/ors etc.
		/// </summary>
		public string LiteralTextADOFormatted
		{
			get { return ZQueryFormatter.GetFormattedText(LiteralTextADO); }
		}

		/// <summary>
		/// Returns the LiteralTextSql of the query as formatted text.
		/// Inner queries are indented, line breaks added after ands/ors etc.
		/// </summary>
		public string LiteralTextSqlFormatted
		{
			get { return ZQueryFormatter.GetFormattedText(LiteralTextSql); }
		}

		bool IFilterPart.NeedsBrackets
		{
			get { return NeedsBrackets; }
		}

		protected virtual bool NeedsBrackets
		{
			get
			{
				for (int i = 0; i < FilterParts.Count; i++)
				{
					IFilterPart filterPart = FilterParts.filterParts[i];
					if (!(filterPart is JC) || i > 0)
					{
						if (filterPart.NeedsBrackets)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		bool IFilterPart.FilterIsEmpty
		{
			get { return FilterIsEmpty; }
		}

		public virtual bool IsDataViewOptimisable
		{
			get { return !IsDBOnlyQuery; }
		}

		protected virtual internal bool FilterIsEmpty
		{
			get { return FilterParts.FilterIsEmpty && !IsNoResultQuery; }
		}

		#endregion

		/// <summary>
		/// Clear the filter entirely
		/// </summary>
		public void Clear()
		{
			FilterParts.Reset();
			TableList.Clear();
			OrderBy = "";
			IsNoResultQuery = false;
			ClearCore();
		}

		public void Simplify()
		{
			FilterParts.Simplify();
		}

		IFilterPart[] IFilterPart.GetSimplifiedVersion(JC lastJoinCondition)
		{
			if (IsEmpty && FilterIsEmpty)
			{
				return Array.Empty<IFilterPart>();
			}
			else
			{
				Simplify();
				if (lastJoinCondition == null || FilterParts.ContainsOnly(lastJoinCondition))
				{
					return FilterParts.FilterParts;
				}
				else
				{
					return new IFilterPart[] { this };
				}
			}
		}

		IEnumerable<IFilterPart> IFilterPart.FilterParts => FilterParts;

		protected virtual void ClearCore()
		{
		}

		public ZQuery ShallowClone()
		{
			return CloneCore(false);
		}

		public ZQuery DeepClone()
		{
			return CloneCore(true);
		}

		IFilterPart IFilterPart.DeepClone()
		{
			return DeepClone();
		}

		protected virtual ZQuery CloneCore(bool isDeepClone)
		{
			var result = (ZQuery)MemberwiseClone();

			if (isDeepClone)
			{
				result.FilterParts = FilterParts.DeepClone();
			}
			else
			{
				result.FilterParts = FilterParts.ShallowClone();
			}
			if (result.tableList != null)
			{
				result.tableList = null;
			}

			result.modificationsEnabled = true;
			result.DefaultJoinCondition = DefaultJoinCondition;

			if (tableList != null && tableList.Count > 0)
			{
				foreach (var table in tableList)
				{
					result.TableList.Add(table);
				}
			}

			if (tableIndexHints != null && tableIndexHints.Count > 0)
			{
				result.tableIndexHints = new List<TableIndexHint>();

				foreach (var tableIndexHint in tableIndexHints)
				{
					result.TableIndexHints.Add(tableIndexHint);
				}
			}

			result.LoadSmallBlobs = LoadSmallBlobs;
			result.IncludeBlob(LoadWithBlobs);
			result.AllowTableValuedParameters = AllowTableValuedParameters;
			result.IsUnionQuery = IsUnionQuery;

			return result;
		}

		protected string AddSchemaName(string tableName)
		{
			return SchemaPrepender.AddSchemaName(tableName);
		}

		#region AddOptionRecompile

		public bool AddOptionRecompileConditionally { get; set; }

		#endregion

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			ZQuery rhs = obj as ZQuery;
			bool result = rhs != null;

			result = result && GetType() == rhs.GetType();
			result = result && IsNoResultQuery == rhs.IsNoResultQuery;
			result = result && FetchOnlyFromLocalCache == rhs.FetchOnlyFromLocalCache;
			result = result && IsNoLock == rhs.IsNoLock;
			result = result && TableHints == rhs.TableHints;
			result = result && (tableIndexHints?.Count ?? 0) == (rhs.tableIndexHints?.Count ?? 0);
			if (result && tableIndexHints != null && rhs.tableIndexHints != null)
			{
				result = result && tableIndexHints.SequenceEqual(rhs.tableIndexHints);
			}

			result = result && QueryHints == rhs.QueryHints;
			result = result && ReLoadExistingRows == rhs.ReLoadExistingRows;
			result = result && IgnoreDbQueryCache == rhs.IgnoreDbQueryCache;
			result = result && MaximumRows == rhs.MaximumRows;
			result = result && IgnoreActiveFilter == rhs.IgnoreActiveFilter;
			result = result && OrderBy == rhs.OrderBy;
			result = result && LoadSmallBlobs == rhs.LoadSmallBlobs;
			result = result && new HashSet<SchemaColumn>(LoadWithBlobs).SetEquals(rhs.LoadWithBlobs);
			result = result && object.Equals(FilterParts, rhs.FilterParts);
			return result;
		}

		public override int GetHashCode()
		{
			int result = hashCode;
			if (result == 0)
			{
				result = LiteralTextADO.GetHashCode();
				if (!ModificationsEnabled)
				{
					hashCode = result;
				}
			}
			return result;
		}
		int hashCode;

		#endregion

		#region IFilterPartsProvider Members

		IFilterPart[] IFilterPartsProvider.FilterParts
		{
			get { return FilterParts.FilterParts; }
		}

		#endregion

		#region ISeparateFetchQuery Members

		bool ISeparateFetchQuery.CannotBeJoinedInFetchHint { get; set; }

		#endregion

		#region ISupportMainElement Members

		void ISupportMainElement.SetMainElement(BusinessObject mainElement)
		{
			SetMainElement(this, mainElement);
		}

		void SetMainElement(IFilterPartsProvider filterPartsProvider, BusinessObject mainElement)
		{
			foreach (var filterPart in filterPartsProvider.FilterParts)
			{
				var partSupportingMainElement = filterPart as ISupportMainElement;
				if (partSupportingMainElement != null)
				{
					partSupportingMainElement.SetMainElement(mainElement);
				}

				var partWithFilterParts = filterPart as IFilterPartsProvider;
				if (partWithFilterParts != null)
				{
					SetMainElement(partWithFilterParts, mainElement);
				}
			}
		}

		#endregion

		#region Stored Procedure

		public virtual ZDataQuery GetDataQuery(ZSqlConnectionInfo connectionInfo, string tableName) => new ZDataQuery(connectionInfo, tableName, this);

		#endregion

		public static List<(string, IFilterPart)> RecursiveGetFilterParts(ZQuery query, int index = 0, string perfix = "0")
		{
			return recursiveGetFilterParts(query.FilterParts, index, perfix);
		}

		static List<(string, IFilterPart)> recursiveGetFilterParts(IEnumerable<IFilterPart> parts, int index, string perfix)
		{
			var result = new List<(string, IFilterPart)>();
			foreach (var part in parts)
			{
				index++;
				result.Add(($"{perfix}.{index}", part));
				if (part.FilterParts != null)
				{
					if (part.FilterParts.Any())
					{
						result.AddRange(ZQuery.recursiveGetFilterParts(part.FilterParts, 0, $"{perfix}.{index}"));
					}
				}
			}
			return result;
		}
	}
}
