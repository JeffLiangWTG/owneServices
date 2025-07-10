using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Enterprise.DataTransfer.Native.Common.Stat
{
	public class StatisticsImpl : IStatistics
	{
		readonly List<DBEntity> entityAffected;

		public StatisticsImpl()
		{
			entityAffected = new List<DBEntity>();
		}

		#region SyncRoot

		object syncRoot;
		object SyncRoot
		{
			get
			{
				if (syncRoot == null)
				{
					Interlocked.CompareExchange(ref syncRoot, new object(), null);
				}

				return syncRoot;
			}
		}

		#endregion

		public long GetCount(DBEntity.DbAction action)
		{
			return entityAffected.Count(e => e.Action == action);
		}

		public void Clear()
		{
			lock (SyncRoot)
			{
				entityAffected.Clear();
			}
		}

		public void Add(DBEntity entity)
		{
			lock (SyncRoot)
			{
				entityAffected.Add(entity);
			}
		}

		public string[] EntityNames
		{
			get
			{
				lock (SyncRoot)
				{
					return entityAffected.Select(e => e.Name).Distinct().ToArray();
				}
			}
		}

		public DBEntityStatistics GetEntityStatistics(string name)
		{
			lock (SyncRoot)
			{
				return new DBEntityStatistics(entityAffected.Where(e => e.Name == name).ToList());
			}
		}

		public IEnumerable<DBEntity> EntityAffected
		{
			get { return entityAffected; }
		}

#if DEBUG
		public readonly ConcurrentDictionary<string, int> BatchMergeEntityCountForTest = new ConcurrentDictionary<string, int>();

		public void UpdateBatchMergeCountForTest(string entityName)
		{
			BatchMergeEntityCountForTest.AddOrUpdate(entityName, 1, (key, oldValue) => oldValue + 1);
		}
#endif
	}
}
