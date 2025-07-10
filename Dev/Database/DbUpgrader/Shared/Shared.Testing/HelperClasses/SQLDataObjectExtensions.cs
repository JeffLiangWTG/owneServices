using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public static class SQLDataObjectExtensions
	{
		public static T AppendInsertAndReturnObject<T>(this T obj, SqlQueryBuilder sql)
			where T : SQLDataObject<T>
		{
			var builder = new StringBuilder();
			obj.AppendInsertAndReturnObject(builder);
			sql.Append(builder.ToString());
			return obj;
		}

		public static T WithDockDoor<T>(this T warehouse, SqlQueryBuilder sql)
			where T : SQLDataObject<T>, IWhsWarehouseSQL
		{
			var builder = new StringBuilder();
			warehouse.WithDockDoor(builder);
			sql.Append(builder.ToString());
			return warehouse;
		}

		public static T AppendToInsert<T>(this IInsertModifier<T> insertModifier, SqlQueryBuilder sql)
			where T : SQLDataObject<T>
		{
			var builder = new StringBuilder();
			var dataObject = insertModifier.AppendToInsert(builder);
			sql.Append(builder.ToString());
			return dataObject;
		}
	}
}
