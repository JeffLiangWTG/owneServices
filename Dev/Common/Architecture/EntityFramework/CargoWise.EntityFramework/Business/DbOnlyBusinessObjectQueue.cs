using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common.ErrorManagement;
using CargoWise.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public delegate void DbOnlyBusinessObjectQueueAction<T>(T t, CancelEventArgs e);

	public class DbOnlyBusinessObjectQueue<T> where T : BusinessObject
	{
		public DbOnlyBusinessObjectQueue(ZQuery query = null)
		{
			SetNonPersistentDataQuery(query);
		}

		public DbOnlyBusinessObjectQueue(ZNonPersistentDataQuery query)
		{
			if (query != null)
			{
				NonPersistentDataQuery = query;
			}
			else
			{
				SetNonPersistentDataQuery(null);
			}
		}

		public DbOnlyBusinessObjectQueue(ZNonPersistentDataQuery query, bool isProcedure)
			: this(query)
		{
			IsProcedure = isProcedure;
		}

		void SetNonPersistentDataQuery(ZQuery query)
		{
			if (query == null)
			{
				query = new ZQuery();
			}

			var dataQuery = query.ParameterisedText;
			var parameterisedQueryText = dataQuery.ParameterisedQueryText;
			var parameters = dataQuery.Parameters;

			var maximumRows = query.MaximumRows;
			var top = maximumRows.HasValue ? ("TOP " + maximumRows) : "";

			var where = (String.IsNullOrEmpty(parameterisedQueryText) ? "" : "WHERE ") + parameterisedQueryText;

			var orderby = query.OrderBy;
			orderby = String.IsNullOrEmpty(orderby) ? "" : ((NoResString)"ORDER BY " + orderby);

			var sqlText = String.Format(CultureInfo.InvariantCulture, "SELECT {0} {1} FROM {2} WITH (READPAST, READCOMMITTEDLOCK) {3} {4}", top, _tableSchema.PK.Name, _tableSchema.TableName, where, orderby);

			NonPersistentDataQuery = new ZNonPersistentDataQuery(sqlText, parameters);
		}

		ZNonPersistentDataQuery NonPersistentDataQuery { get; set; }
		readonly bool IsProcedure;

#if DEBUG
		public void ProcessBatch(DbOnlyBusinessObjectQueueAction<T[]> action, int maxBatchSize, bool disableDataRefresh = false)
		{
			ProcessBatch(action, maxBatchSize, CancellationToken.None, disableDataRefresh);
		}
#endif

		public void ProcessBatch(DbOnlyBusinessObjectQueueAction<T[]> action, int maxBatchSize, CancellationToken token, bool disableDataRefresh = false)
		{
			ZGuid[] itemPKs = GetUnprocessedItemPKs();
			Dictionary<ZGuid, int> pkIndexes = new Dictionary<ZGuid, int>(itemPKs.Length);
			for (int i = 0; i < itemPKs.Length; i++)
			{
				pkIndexes[itemPKs[i]] = i;
			}

			if (maxBatchSize == -1)
			{
				maxBatchSize = itemPKs.Length;
			}

			for (int i = 0; i < itemPKs.Length; i += maxBatchSize)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				List<ZGuid> batchPKs = new List<ZGuid>(maxBatchSize);

				int maxItem = Math.Min(i + maxBatchSize, itemPKs.Length);
				for (int j = i; j < maxItem; j++)
				{
					batchPKs.Add(itemPKs[j]);
				}

				var factory = new BusinessObjectFactory();

				if (disableDataRefresh)
				{
					factory.RefreshEnabled = false;
				}

				ZQuery query = new ZQuery(BusinessObjectFactory.GetPKColumnFromType(typeof(T)), batchPKs);
				CancelEventArgs e = new CancelEventArgs();

				T[] bizos = factory.Load<T>(query);

				int[] indexes = new int[bizos.Length];
				for (int j = 0; j < bizos.Length; j++)
				{
					int pkIndex;
					pkIndexes.TryGetValue(bizos[j].PK, out pkIndex);
					indexes[j] = pkIndex;
				}
				Array.Sort(indexes, bizos);

				action(bizos, e);

				if (e.Cancel)
				{
					break;
				}
			}
		}

		public void Process(DbOnlyBusinessObjectQueueAction<T> action, int maxItemsPerFactory)
		{
			ZGuid[] itemPKs = GetUnprocessedItemPKs();
			for (int i = 0; i < itemPKs.Length; i += maxItemsPerFactory)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				int maxItem = Math.Min(i + maxItemsPerFactory, itemPKs.Length);
				for (int j = i; j < maxItem; j++)
				{
					factory.AddFetchHint(_tableSchema.TableName, itemPKs[j]);
				}

				CancelEventArgs e = new CancelEventArgs();
				for (int j = i; j < maxItem; j++)
				{
					action(factory.Load<T>(itemPKs[j]), e);
					if (e.Cancel)
					{
						break;
					}
				}
				if (e.Cancel)
				{
					break;
				}
			}
		}

		public void Process(DbOnlyBusinessObjectQueueAction<ZGuid> action)
		{
			CancelEventArgs e = new CancelEventArgs();
			foreach (ZGuid pk in GetUnprocessedItemPKs())
			{
				action(pk, e);
				if (e.Cancel)
				{
					break;
				}
			}
		}

		ZGuid[] GetUnprocessedItemPKs()
		{
			var retryHandler = new RetryHandler(Enumerable.Range(1, 4).Select(i => TimeSpan.FromSeconds(i)));
			return retryHandler.Invoke<ZGuid[], SqlException>(GetUnprocessedItemPKs_Core, shouldCatch: IsSqlDeadlockException);
		}

		static bool IsSqlDeadlockException(SqlException ex)
		{
			return ex != null && new DbErrorMatch(ex).ExceptionType == DbErrorType.DeadlockError;
		}

		protected virtual ZGuid[] GetUnprocessedItemPKs_Core()
		{
			var result = new List<ZGuid>();
			var connectionInfo = new ZSqlConnectionInfo(Db.Connection, Db.DatabaseName);
			DbCommand cmd;
			if (IsProcedure)
			{
				cmd = connectionInfo.GetNewDbCommandForStoredProcedure(NonPersistentDataQuery.ParameterisedQueryText, NonPersistentDataQuery.Parameters);
			}
			else
			{
				cmd = connectionInfo.GetNewDbCommandForSelect(NonPersistentDataQuery.ParameterisedQueryText, NonPersistentDataQuery.Parameters);
			}

			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add((Guid)reader[0]);
				}
			}

			return result.ToArray();
		}

		readonly ITableSchema _tableSchema = BusinessObjectFactory.GetTableSchemaFromType(typeof(T));
	}
}
