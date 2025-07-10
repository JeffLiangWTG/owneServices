using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	class PrimaryKeyProvider
	{
		public PrimaryKeyProvider(ZQuery initialQuery)
		{
			var compositeParts = initialQuery.FilterParts.GetCompositeParts();
			if (compositeParts.Length > 0)
			{
				var nonPrimaryKeyCompositeParts = new List<ZQuery>(compositeParts.Length);

				foreach (var query in compositeParts)
				{
					var primaryKeyParameter = GetPrimaryKeyParameter(query);
					if (primaryKeyParameter != null)
					{
						var zSqlParameter = primaryKeyParameter as ZSqlParameter;
						if (zSqlParameter != null)
						{
							Add(zSqlParameter);
						}
						else
						{
							var inFilter = primaryKeyParameter as ZSQLInFilter;
							if (inFilter != null)
							{
								Add(inFilter);
							}
						}
					}
					else
					{
						nonPrimaryKeyCompositeParts.Add(query);
					}
				}

				if (ContainsPrimaryKeys || nonPrimaryKeyCompositeParts.Count < compositeParts.Length)
				{
					FilterWithoutPrimaryKeys = new ZQuery();
					foreach (var query in nonPrimaryKeyCompositeParts)
					{
						FilterWithoutPrimaryKeys.AddToFilter(query, JoinCondition.And);
					}
				}
				else
				{
					FilterWithoutPrimaryKeys = initialQuery;
				}
			}
			else
			{
				FilterWithoutPrimaryKeys = initialQuery;
			}
		}

		public bool ContainsPrimaryKeys
		{
			get { return primaryKeys != null; }
		}

		void Add(ZSqlParameter zSqlParameter)
		{
			var list = new List<object>(1);
			list.Add(zSqlParameter.Value);
			AddList(list);
		}

		void Add(ZSQLInFilter inFilter)
		{
			AddList(inFilter.GetValues());
		}

		void AddList(IEnumerable<object> list)
		{
			if (list != null)
			{
				var oldPrimaryKeys = primaryKeys;
				primaryKeys = new Dictionary<ZGuid, bool>();
				foreach (object item in list)
				{
					var guid = new ZGuid(item);
					if (oldPrimaryKeys == null || oldPrimaryKeys.ContainsKey(guid))
					{
						primaryKeys[guid] = true;
					}
				}
			}
		}

		Dictionary<ZGuid, bool> primaryKeys;

		public bool ContainsKey(ZGuid primaryKey)
		{
#if DEBUG
			if (!ContainsPrimaryKeys)
			{
				throw new InvalidOperationException("No primary keys found - Use ContainsPrimaryKeys prior to calling this method");
			}
#endif
			return primaryKeys.ContainsKey(primaryKey);
		}

		public ZQuery FilterWithoutPrimaryKeys { get; private set; }

		IFilterPart GetPrimaryKeyParameter(ZQuery query)
		{
			if (query.FilterParts.Count == 1)
			{
				IFilterPart filterPart = query.FilterParts.GetFilterPart(0);
				if (filterPart is ZSqlParameter)
				{
					ZSqlParameter p = filterPart as ZSqlParameter;
					if (p.ComparisonOperator == SQLComparisonOperator.Equal && p.SchemaColumn.IsPKColumn)
					{
						return p;
					}
				}
				if (filterPart is ZSQLInFilter)
				{
					ZSQLInFilter f = filterPart as ZSQLInFilter;
					if (f.Column.IsPKColumn && f.ComparisonOperator == SQLComparisonOperator.Equal)
					{
						return f;
					}
				}
			}
			return null;
		}
	}
}
