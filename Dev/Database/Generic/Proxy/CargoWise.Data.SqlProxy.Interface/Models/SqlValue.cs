using System.Data;
using System.Data.SqlTypes;
using CargoWise.Data.SqlProxy.Interface.Converters;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models
{
	public class SqlValue
	{
		public SqlValue()
		{
		}

		public SqlValue(object? value, string columnSqlDataTypeName)
		{
			var valueIsNull = value is null or DBNull or INullable { IsNull: true };
			var columnSqlDbType = TypeMappingHelper.StringToSqlDbType(columnSqlDataTypeName);
			if (!valueIsNull && columnSqlDbType is SqlDbType.Variant or SqlDbType.Structured or SqlDbType.Udt)
			{
				var valueType = value?.GetType();
				var sqlDbType = TypeMappingHelper.GetSqlDbType(valueType);
				SqlVariantValueType = sqlDbType != null
					? sqlDbType.ToString()
					: valueType?.AssemblyQualifiedName;
			}

			JsonStringValue = SqlValueConverter.ToJson(valueIsNull ? null : value, SqlVariantValueType ?? columnSqlDataTypeName);
		}

		[JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string? SqlVariantValueType { get; set; }

		[JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string? JsonStringValue { get; set; }

		public object? ToSqlValue(string columnSqlDataTypeName)
		{
			return SqlValueConverter.FromJson(
				JsonStringValue,
				string.IsNullOrEmpty(SqlVariantValueType) ? columnSqlDataTypeName : SqlVariantValueType!);
		}
	}
}
