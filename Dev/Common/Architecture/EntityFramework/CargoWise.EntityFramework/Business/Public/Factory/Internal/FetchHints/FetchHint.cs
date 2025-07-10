using System.Collections.Generic;
using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class FetchHint : IFetchHint
	{
		public FetchHint(SchemaColumn column, IZType value, params SchemaColumn[] loadWithBlobs)
			: this(column, value)
		{
			this.loadWithBlobs = loadWithBlobs;
		}

		public FetchHint(SchemaColumn column, IZType value)
		{
			this.Value = value;
			this.Column = column;
		}

		public IEnumerable<SchemaColumn> LoadWithBlobs
		{
			get { return loadWithBlobs ?? System.Array.Empty<SchemaColumn>(); }
		}
		readonly SchemaColumn[] loadWithBlobs;

		public ZQuery GetQuery()
		{
			return new ZQuery(Column, Value);
		}

		public virtual IQueryHashKey GetHashKeyObject()
		{
			return new EnumerableHashObject { TableName, Column.Name, Value };
		}

		public string TableName => Column.TableName;

		public bool IsDataHintLoaded
		{
			get { return isDataHintLoaded; }
			set { isDataHintLoaded = value; }
		}

		bool isDataHintLoaded;

		public readonly SchemaColumn Column;
		public readonly IZType Value;

		string IFetchHint.BuilderKey
		{
			get { return Column.Name + LoadWithBlobsKey; }
		}

		string LoadWithBlobsKey
		{
			get
			{
				var loadWithBlobNames = LoadWithBlobs.Select(c => c.Name).OrderBy(n => n);
				return string.Join(",", loadWithBlobNames);
			}
		}

		bool IFetchHint.IsNeeded(QueryHistoryProvider provider)
		{
			if (Column.IsPKColumn)
			{
				return !provider.HasLoadedRow(TableName, (ZGuid)Value);
			}
			else
			{
				ZQuery filter = new ZQuery(Column, Value);
				return !provider.IsQueryCached(TableName, filter);
			}
		}

		void IFetchHint.GenerateQuery(QueryBuilder builder)
		{
			if (builder.IsEmpty)
			{
				builder.Init(new ZQuery(), Column);
			}

			builder.AddValue(Value);
		}

		#region EnumerableHashObject

		public class EnumerableHashObject : List<object>, IQueryHashKey
		{
			public EnumerableHashObject()
			{
			}

			public EnumerableHashObject(int capacity)
			: base(capacity)
			{
			}

			public IEnumerable<object> KeyParts => this;

			public void AddExpandingEnumerable(object item)
			{
				EnumerableHashObject enumerable = item as EnumerableHashObject;
				if (enumerable != null)
				{
					AddRange(enumerable);
				}
				else
				{
					Add(item);
				}
			}

			public override string ToString()
			{
				return string.Concat(ToArray());
			}
		}

		#endregion
	}
}
