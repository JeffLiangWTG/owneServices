using System;
using System.Data;
using CargoWise.Common.Testing;
using DbConnection = CargoWise.Data.DbConnection;

namespace CargoWise.EntityFramework
{
	#region ILargeColumnSaver

	public interface ILargeColumnSaver
	{
		void Save(DbConnection connection);
		Guid RowPk { get; }
	}

	#endregion

	public abstract class ZLargeColumnSaver : ILargeColumnSaver
	{
		public ZLargeColumnSaver(string tableName, string pkColumnName, Guid rowPk, string dataColumnName, SqlDbType dataColumnType)
		{
			this.tableName = tableName;
			this.pkColumnName = pkColumnName;
			this.RowPk = rowPk;
			this.dataColumnName = dataColumnName;
			this.dataColumnType = dataColumnType;
		}

		protected readonly string tableName;
		protected readonly string pkColumnName;
		protected readonly string dataColumnName;
		protected readonly SqlDbType dataColumnType;

		public Guid RowPk
		{
			get;
			private set;
		}

		abstract public void Save(DbConnection connection);

#if DEBUG
		[SuppressThreadStaticFieldMessage]
		public static int MaxChunkSize = 42000;
#else
		internal const int MaxChunkSize = 42000;
#endif
	}
}
