using CargoWise.Common;
using CargoWise.Database.Shared;
using static CargoWise.Data.MetaData;

namespace CargoWise.Data
{
	public static class BaseIndexInfoExtensions
	{
		public static void Drop(this BaseIndexInfo info, DbConnection connection)
		{
			Argument.NotNull(info, nameof(info));
			Argument.NotNull(connection, nameof(connection));

			if (IndexLoader.Exists(connection, info.SchemaName, info.TableName, info.IndexName))
			{
				connection.ExecuteNonQuery(info.SQL_Drop);
			}
		}

		public static IndexInfo Create(this IndexInfo info, DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			if (DataUtils.ObjectExistsNolock(connection, info.SchemaName, info.TableName))
			{
				if (!IndexLoader.Exists(connection, info.SchemaName, info.TableName, info.IndexName))
				{
					connection.ExecuteNonQuery(DelayForTests + info.SQL_Create, 0);
				}
				else
				{
					CreateDropExisting(info, connection);
				}
			}

			return info;
		}

		public static SpatialIndexInfo Create(this SpatialIndexInfo info, DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			if (DataUtils.ObjectExistsNolock(connection, info.SchemaName, info.TableName))
			{
				if (!IndexLoader.Exists(connection, info.SchemaName, info.TableName, info.IndexName))
				{
					connection.ExecuteNonQuery(DelayForTests + info.SQL_Create, 0);
				}
				else
				{
					CreateDropExisting(info, connection);
				}
			}

			return info;
		}

		static void CreateDropExisting(SpatialIndexInfo info, DbConnection connection)
		{
			var oldIndex = SpatialIndexLoader.LoadTop1(connection, info.SchemaName, info.TableName, info.IndexName);
			if (oldIndex != null && !info.Equals(oldIndex))
			{
				var newIndex = SpatialIndexInfo.Builder.Copy(info)
					.Option(IndexOptions.DROP_EXISTING, true)
					.GetInfo();
				connection.ExecuteNonQuery(newIndex.SQL_Create);
			}
		}

		static void CreateDropExisting(IndexInfo info, DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			var oldIndex = IndexLoader.LoadTop1(connection, info.SchemaName, info.TableName, info.IndexName);
			if (oldIndex != null && !info.Equals(oldIndex))
			{
				var newIndex = IndexInfo.Builder.Copy(info)
					.Option(IndexOptions.DROP_EXISTING, true)
					.GetInfo();
				connection.ExecuteNonQuery(newIndex.SQL_Create);
			}
		}
	}
}
