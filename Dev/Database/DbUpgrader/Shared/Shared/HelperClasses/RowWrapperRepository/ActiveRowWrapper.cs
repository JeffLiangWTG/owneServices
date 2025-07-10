using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Schema;

namespace Enterprise.DbUpgrader.Shared
{
	public class ActiveRowWrapper
	{
		readonly RowWrapperRepository repository;
		readonly RowWrapper wrapper;

		public ActiveRowWrapper(ITableSchema schema)
			: this(schema, new RowWrapperRepository()) { }

		ActiveRowWrapper(ITableSchema schema, RowWrapperRepository repository)
			: this(repository.New(schema))
		{
			this.repository = repository;
		}

		ActiveRowWrapper(RowWrapper wrapper)
		{
			this.wrapper = wrapper;
		}

		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers", Justification = "I want indexer by SchemaColumn")]
		public object this[SchemaColumn column]
		{
			get { return wrapper[column]; }
			set { wrapper[column] = value; }
		}

		public Guid PK => wrapper.PK;
		public int Save() => repository.Save();
	}
}
