using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Bi.Deployment.AnalysisServices
{
	public class PartitionCollection
	{
		public PartitionCollection()
		{
		}

		public PartitionCollection(IEnumerable<Partition> partitionList)
		{
			list = partitionList.ToList();
		}

		public List<Partition> List
		{
			get
			{
				if (list == null)
				{
					list = new List<Partition>();
				}
				return list;
			}
		}
		List<Partition> list;

		public void Add(Partition partition)
		{
			List.Add(partition);
		}

		public void Add(string databaseName, string tableName)
		{
			List.Add(new Partition(databaseName, tableName));
		}

		public void Add(string databaseName, string tableName, string partitionName)
		{
			List.Add(new Partition(databaseName, tableName, partitionName));
		}

		public void Add(string databaseName, string tableName, string partitionName, DateTime? fromValue, DateTime? toValue)
		{
			List.Add(new Partition(databaseName, tableName, partitionName, fromValue, toValue));
		}

		public void Add(string databaseName, string tableName, string partitionName, DateTime? fromValue, DateTime? toValue, bool needsProcessing)
		{
			List.Add(new Partition(databaseName, tableName, partitionName, fromValue, toValue, needsProcessing));
		}

		public int Count
		{
			get
			{
				return List.Count;
			}
		}

		public IEnumerator<Partition> GetEnumerator()
		{
			return List.GetEnumerator();
		}
	}

	public class Partition
	{
		public Partition(string databaseName, string tableName)
			: this(databaseName, tableName, partitionName: null)
		{ }

		public Partition(string databaseName, string tableName, string partitionName)
			: this(databaseName, tableName, partitionName, fromValue: null, toValue: null)
		{ }

		public Partition(string databaseName, string tableName, string partitionName, DateTime? fromValue, DateTime? toValue)
			: this(databaseName, tableName, partitionName, fromValue, toValue, needsProcessing: false)
		{ }

		public Partition(string databaseName, string tableName, string partitionName, DateTime? fromValue, DateTime? toValue, bool needsProcessing)
		{
			DatabaseName = databaseName;
			TableName = tableName;
			PartitionName = partitionName;
			FromValue = fromValue;
			ToValue = toValue;
			NeedsProcessing = needsProcessing;
		}

		public string DatabaseName { get; private set; }
		public string TableName { get; private set; }
		public string PartitionName { get; private set; }
		public DateTime? FromValue { get; private set; }
		public DateTime? ToValue { get; private set; }
		public bool NeedsProcessing { get; set; }
	}
}
