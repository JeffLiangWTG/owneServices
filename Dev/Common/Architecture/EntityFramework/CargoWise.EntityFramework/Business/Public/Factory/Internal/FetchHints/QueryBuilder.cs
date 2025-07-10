using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class QueryBuilder
	{
		public QueryBuilder()
		{
			IsEmpty = true;
		}

		public bool IsEmpty { get; private set; }

		public int ParameterCount { get; private set; }

		public void Init(ZQuery mainQuery, ZQuery subQuery)
		{
			Argument.NotNull(mainQuery, nameof(mainQuery));
			Argument.NotNull(subQuery, nameof(subQuery));

			if (IsEmpty)
			{
				this.mainQuery = new ZQuery(mainQuery);
				this.subQuery = new ZQuery(subQuery);
				ParameterCount += mainQuery.Params.Length + subQuery.Params.Length;

				IsEmpty = false;
			}
			else
			{
				throw new InvalidOperationException("The builder is already initialized.");
			}
		}

		public void Init(ZQuery mainQuery, SchemaColumn column)
		{
			Argument.NotNull(mainQuery, nameof(mainQuery));
			Argument.NotNull(column, nameof(column));

			if (IsEmpty)
			{
				this.mainQuery = new ZQuery(mainQuery);
				this.column = column;
				this.columnValues = new Dictionary<IZType, object>();
				ParameterCount += mainQuery.Params.Length;

				IsEmpty = false;
			}
			else
			{
				throw new InvalidOperationException("The builder is already initialized.");
			}
		}

		public void Init(string directSQL, string tvpParameterName, SchemaColumn column)
		{
			Argument.NotNullOrEmpty(directSQL, nameof(directSQL));
			Argument.NotNullOrEmpty(tvpParameterName, nameof(tvpParameterName));
			Argument.NotNull(column, nameof(column));

			if (IsEmpty)
			{
				this.mainQuery = new ZQuery();
				this.directSQL = directSQL;
				this.tvpParameterName = tvpParameterName;
				this.column = column;
				this.columnValues = new Dictionary<IZType, object>();

				IsEmpty = false;
			}
			else
			{
				throw new InvalidOperationException("The builder is already initialized.");
			}
		}

		public void AddValue(ZQuery subQuery)
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException("The builder is not initialized yet. Please use an Init method to initial setup the builder.");
			}
			else if (this.subQuery == null)
			{
				throw new InvalidOperationException("You cannot use SubQuery because the builder was initialized for using in other way. Please use a proper Init method to initial setup the builder.");
			}
			else
			{
				this.subQuery.AddToFilter(subQuery, JoinCondition.Or);
				this.subQuery.MaximumRows = subQuery.MaximumRows;

				ParameterCount += subQuery.Params.Length;
			}
		}

		public void AddValue(IZType value)
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException("The builder is not initialized yet. Please use Init method to initial setup the builder.");
			}
			else if (this.column == null || this.columnValues == null)
			{
				throw new InvalidOperationException("You cannot use SchemaColumn because the builder was initialized for using in other way. Please use a proper Init method to initial setup the builder.");
			}
			else
			{
				if (!this.columnValues.ContainsKey(value))
				{
					this.columnValues.Add(value, null);
					ParameterCount++;
				}
			}
		}

		public ZQuery GetQuery()
		{
			if (!finalized)
			{
				if (IsEmpty)
				{
					throw new InvalidOperationException("The builder is not initialized yet. Please use Init method to initial setup the builder.");
				}
				else if (subQuery != null)
				{
					mainQuery.AddToFilter(subQuery, JoinCondition.And);
					mainQuery.MaximumRows = subQuery.MaximumRows;
				}
				else if (!String.IsNullOrWhiteSpace(directSQL))
				{
					var parameter = ZSqlParameter.New(tvpParameterName, columnValues.Keys, column, true);
					var parameterCollection = new ZSqlParameterCollection(parameter);
					mainQuery.AddFilterAndZSQLParameterCollection(directSQL, parameterCollection, JoinCondition.And);
				}
				else if (column != null)
				{
					mainQuery.AddToFilter(column, columnValues.Keys);
				}

				((IFilterPart)this.mainQuery).DisableModifications();
				finalized = true;
			}

			return mainQuery;
		}

		public override string ToString()
		{
			return GetQuery().LiteralTextSql;
		}

		#region Implementation

		bool finalized;

		ZQuery mainQuery;
		ZQuery subQuery;

		SchemaColumn column;
		Dictionary<IZType, object> columnValues;

		string directSQL;
		string tvpParameterName;

		#endregion // Implementation
	}
}
