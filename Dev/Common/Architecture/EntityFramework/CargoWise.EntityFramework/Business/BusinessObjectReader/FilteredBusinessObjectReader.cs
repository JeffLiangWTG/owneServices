using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class FilteredBusinessObjectReader<T> : FilteredBusinessObjectReader
		where T : BusinessObject
	{
		public FilteredBusinessObjectReader(ZQuery objectFilter, BusinessObjectFactory factory = null)
			: base(objectFilter, typeof(T), factory)
		{
		}

		public FilteredBusinessObjectReader(BusinessObjectFactoryProvider factoryProvider, ZQuery objectFilter)
			: base(factoryProvider, objectFilter, typeof(T))
		{
		}
	}

	public partial class FilteredBusinessObjectReader : BusinessObjectReader
	{
		public FilteredBusinessObjectReader(ZQuery objectFilter, Type businessObjectType, BusinessObjectFactory factory = null)
			: this(new BusinessObjectFactoryProvider(factory), objectFilter, businessObjectType)
		{
		}

		public FilteredBusinessObjectReader(BusinessObjectFactoryProvider factoryProvider, ZQuery objectFilter, Type businessObjectType)
			: base(factoryProvider)
		{
			fObjectFilter = objectFilter;
			fBusinessObjectType = businessObjectType;
			batchingStrategy = NeedsStmALogBatchStrategy ? new StmALogBatchingStrategy(this) : new DefaultBatchingStrategy(this);
		}

		protected FilteredBusinessObjectReader(BusinessObjectFactoryProvider factoryProvider, Type businessObjectType)
			: this(factoryProvider, null, businessObjectType)
		{
		}

		protected FilteredBusinessObjectReader(Type businessObjectType, BusinessObjectFactory factory = null)
			: this(new BusinessObjectFactoryProvider(factory), businessObjectType)
		{
		}

		public ZQuery ObjectFilter
		{
			get
			{
				if (fObjectFilter == null)
				{
					fObjectFilter = GetDefaultObjectFilter();
				}

				if (!ObjectFilterChecked)
				{
					CheckObjectFilterOrderBy();
					ObjectFilterChecked = true;
				}

				return fObjectFilter;
			}
		}

		public int BatchSize
		{
			get { return fBatchSize; }
			set
			{
				if (value <= 0)
				{
					throw new ArgumentException("Cannot set the FilteredBusinessObjectReader.BatchSize to a value less than 1.");
				}

				fBatchSize = value;
			}
		}

		int fBatchSize = 100;

		public override bool HasRecords
		{
			get { return Factory.LoadTop1(BusinessObjectType, ObjectFilter) != null; }
		}

		public override int ApproximateCount
		{
			get { return Factory.GetDatabaseCount(BusinessObjectType, ObjectFilter); }
		}

		public override Type BusinessObjectType
		{
			get { return fBusinessObjectType; }
		}

		readonly Type fBusinessObjectType;

		public bool AllowTableValuedParameters { get; set; }

		#region Implementation

		ZQuery fObjectFilter;
		bool ObjectFilterChecked;
		SchemaGuidColumn fPKColumn;
		readonly BatchingStrategy batchingStrategy;

		protected override BusinessObject[] LoadNextBatchCore(ZGuid pk) => batchingStrategy.LoadNextBatch(pk);

		protected override BusinessObject[] LoadNextBatchCore(BusinessObject lastBusinessObjectRead) => batchingStrategy.LoadNextBatch(lastBusinessObjectRead);

		bool NeedsStmALogBatchStrategy => PKColumn.TableName.Equals("StmALog", StringComparison.OrdinalIgnoreCase);

		BusinessObject[] LoadWithCache(ZGuid pk)
		{
			if (ObjectPKs.Count > 0)
			{
				var skipCount = (pk.IsEmpty || !ObjectPKIndexes.TryGetValue(pk, out int idx))
					? 0
					: idx + 1;

				if (skipCount < ObjectPKs.Count)
				{
					return LoadWithCacheCore(skipCount, PKColumn);
				}
			}

			return (BusinessObject[])Array.CreateInstance(BusinessObjectType, 0);
		}

		protected virtual BusinessObject[] LoadWithCacheCore(int skipCount, SchemaGuidColumn pkColumn)
		{
			var filter = new ZDBOnlyQuery(BusinessObjectType)
			{
				AllowTableValuedParameters = AllowTableValuedParameters,
				MaximumRows = BatchSize,
				IgnoreActiveFilter = ObjectFilter.IgnoreActiveFilter
			};

			foreach (var blobColumn in ObjectFilter.LoadWithBlobs)
			{
				filter.IncludeBlob(blobColumn);
			}

			filter.AddToFilter(pkColumn, ObjectPKs.Skip(skipCount).Take(BatchSize).ToArray());

			if (typeof(IHaveNonUniquePrimaryKey).IsAssignableFrom(BusinessObjectType))
			{
				filter.AddToFilter(ObjectFilter);
			}

			var result = Factory.Load(BusinessObjectType, filter);
			Array.Sort(result, (x, y) => ObjectPKIndexes[(ZGuid)x[pkColumn]].CompareTo(ObjectPKIndexes[(ZGuid)y[pkColumn]]));

			return result;
		}

		public string LastStatementExecuted { get; private set; }
		public string QueryName { get; set; }

		List<ZGuid> objectPKs;
		Dictionary<ZGuid, int> objectPKIndexes;

		List<ZGuid> ObjectPKs
		{
			get
			{
				if (objectPKs == null)
				{
					LoadObjectPKs();
				}

				return objectPKs;
			}
		}

		Dictionary<ZGuid, int> ObjectPKIndexes
		{
			get
			{
				if (objectPKs == null)
				{
					LoadObjectPKs();
				}

				return objectPKIndexes;
			}
		}

		void LoadObjectPKs()
		{
			objectPKs = GetObjectPKsCore();
			objectPKIndexes = new Dictionary<ZGuid, int>();
			for (int i = 0; i < objectPKs.Count; i++)
			{
				var pk = objectPKs[i];
				if (!objectPKIndexes.ContainsKey(pk))
				{
					objectPKIndexes.Add(pk, i);
				}
			}
		}

		protected virtual List<ZGuid> GetObjectPKsCore()
		{
			var pks = new List<ZGuid>();

			var filter = new ZDBOnlyQuery(BusinessObjectType);
			filter.AddToFilter(ObjectFilter);

			if (!filter.IsNoResultQuery)
			{
				string sql = filter.GetAsCompleteSQLStatement(PKColumn.TableName, false, new[] { PKColumn });
				if (!QueryName.IsNullOrEmpty())
				{
					sql = string.Format(CultureInfo.InvariantCulture, @"-- {0}
{1}", QueryName, sql);
				}

				var parametersInString = filter.Params.Select(p => string.Format(CultureInfo.InvariantCulture, "{0}={1}", p.ParameterisedSql, p.ParameterValueTextSql));
				LastStatementExecuted = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} -- with parameters: {1}", sql, string.Join(",", parametersInString));

				var collection = new DynamicBusinessObjectCollection(Factory);
				collection.Load(sql, filter.Params);

				foreach (DynamicBusinessObject item in collection)
				{
					pks.Add((ZGuid)item[PKColumn.Name]);
				}
			}

			return pks;
		}

		BusinessObject[] DirectLoad(BusinessObject lastBusinessObjectRead)
		{
			var filter = new ZDBOnlyQuery(BusinessObjectType);
			filter.AddToFilter(ObjectFilter);

			if (lastBusinessObjectRead != null)
			{
				AddOrderByAndPKGreaterThanToFilter(filter, lastBusinessObjectRead);
			}

			filter.OrderBy = ObjectFilterOrderByAlsoOrderedByPK;
			filter.MaximumRows = BatchSize;

			return Factory.Load(BusinessObjectType, filter);
		}

		partial void SetNumberOfRecordsInLastBatch_ForTest(int value);

		protected virtual ZQuery GetDefaultObjectFilter()
		{
			throw new NotSupportedException("Override this in the sub-class");
		}

		string ObjectFilterOrderByAlsoOrderedByPK
		{
			get
			{
				string result;
				if (ObjectFilter.OrderBy.IsEmpty)
				{
					result = PKColumn.Name;
				}
				else
				{
					result = ObjectFilter.OrderBy + "," + PKColumn.Name;
				}
				return result;
			}
		}

		void AddOrderByAndPKGreaterThanToFilter(ZDBOnlyQuery filterToAddTo, BusinessObject bizObj)
		{
			// OrderBy > BizObj[OrderBy] or (OrderBy = BizObj[OrderBy] and PK > BizObj.PK)
			ZQuery filter = new ZQuery();
			AddOrderByColumnCompareToFilter(filter, SQLComparisonOperator.GreaterThan, bizObj);
			filter.AddToFilter(GetEqualsToOrderByColumnAndGreaterThanPKFilter(bizObj), JoinCondition.Or);

			filterToAddTo.AddToFilter(filter, JoinCondition.And);
		}

		ZQuery GetEqualsToOrderByColumnAndGreaterThanPKFilter(BusinessObject bizObj)
		{
			ZQuery result = new ZQuery();

			AddOrderByColumnCompareToFilter(result, SQLComparisonOperator.Equal, bizObj);
			result.AddToFilter(JoinCondition.And, PKColumn, SQLComparisonOperator.GreaterThan, bizObj.PK);
			return result;
		}

		void AddOrderByColumnCompareToFilter(ZQuery filter, SQLComparisonOperator @operator, BusinessObject bizObjWithOrderByColumnValue)
		{
			if (!ObjectFilter.OrderBy.IsEmpty)
			{
				string tableName = BusinessObjectFactory.GetTableNameFromType(BusinessObjectType);
				SchemaColumn orderByColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(ObjectFilter.OrderBy, tableName);

				object orderByColumnValue = bizObjWithOrderByColumnValue[ObjectFilter.OrderBy];
				filter.AddToFilter(orderByColumn, @operator, orderByColumnValue);
			}
		}

		void CheckObjectFilterOrderBy()
		{
			if (fObjectFilter.OrderBy.IndexOf("(") != -1 || fObjectFilter.OrderBy.IndexOf("@") != -1)
			{
				throw new ArgumentException("Functions and parameters are not supported for OrderBy on the filter for a " + GetType().Name);
			}
		}

		SchemaGuidColumn PKColumn
		{
			get
			{
				if (fPKColumn == null)
				{
					string tableName = BusinessObjectFactory.GetTableNameFromType(fBusinessObjectType);
					fPKColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(tableName);
				}

				return fPKColumn;
			}
		}

		#endregion

		#region Batching Strategies

		abstract class BatchingStrategy
		{
			public BatchingStrategy(FilteredBusinessObjectReader reader)
			{
				Reader = reader;
			}

			public FilteredBusinessObjectReader Reader { get; }

			internal virtual BusinessObject[] LoadNextBatch(BusinessObject lastBusinessObject)
			{
				var pk = lastBusinessObject?.PK ?? ZGuid.Empty;
				return LoadNextBatch(pk);
			}

			internal BusinessObject[] LoadNextBatch(ZGuid lastBusinessObjectReadPK)
			{
				var result = LoadNextBatchCore(lastBusinessObjectReadPK);
				Reader.SetNumberOfRecordsInLastBatch_ForTest(result.Length);
				return result;
			}

			internal abstract BusinessObject[] LoadNextBatchCore(ZGuid lastBusinessObjectReadPK);
		}

		class DefaultBatchingStrategy : BatchingStrategy
		{
			internal DefaultBatchingStrategy(FilteredBusinessObjectReader reader)
				: base(reader)
			{
			}

			internal override BusinessObject[] LoadNextBatchCore(ZGuid lastBusinessObjectReadPK) => Reader.LoadWithCache(lastBusinessObjectReadPK);
		}

		class StmALogBatchingStrategy : BatchingStrategy
		{
			internal StmALogBatchingStrategy(FilteredBusinessObjectReader reader)
				: base(reader)
			{
			}

			internal override BusinessObject[] LoadNextBatchCore(ZGuid lastBusinessObjectReadPK) => throw new InvalidOperationException("Cannot load StmALog from PK");

			internal override BusinessObject[] LoadNextBatch(BusinessObject lastBusinessObject) => Reader.DirectLoad(lastBusinessObject);
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace CargoWise.EntityFramework
{
	public partial class FilteredBusinessObjectReader
	{
		internal int NumberOfRecordsInLastBatch_ForTest { get; set; }

		partial void SetNumberOfRecordsInLastBatch_ForTest(int value)
		{
			NumberOfRecordsInLastBatch_ForTest = value;
		}
	}
}

#endif
#endregion
