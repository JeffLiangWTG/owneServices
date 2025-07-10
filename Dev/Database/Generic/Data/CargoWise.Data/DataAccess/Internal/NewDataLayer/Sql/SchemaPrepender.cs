using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.EntityFramework
{
	public static class SchemaPrepender
	{
		public static string AddSchemaName(string tableName)
		{
			if (tableName.IndexOf('.') == -1)
			{
				var tableSchema = GlobalServiceProvider.Instance.GetRequiredService<IApplicationSchemaResolver>().GetTableSchema(tableName);
				var schemaName = tableSchema?.SqlSchemaName ?? "dbo";
				tableName = schemaName + "." + tableName;
			}
			return tableName;
		}
	}
}
