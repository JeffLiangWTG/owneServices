using System.Data;
using System.Data.Common;
using CargoWise.Data.SqlProxy.Interface.Converters;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

public class SqlParameterDTO
{
	public string Name { get; set; } = string.Empty;

	[JsonProperty(NullValueHandling = NullValueHandling.Include)]
	public string? Value { get; set; }

	public int Size { get; set; }

	public DbType DbType { get; set; }

	[JsonRequired]
	public SqlDbType SqlDbType { get; set; }

	[JsonRequired]
	public string ValueTypeName { get; set; } = string.Empty;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string? SqlTypeName { get; set; }

	public ParameterDirection Direction { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string? UdtTypeName { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public string? StructuredValue { get; set; }

	public object AsSqlValue()
	{
		if (SqlDbType == SqlDbType.Structured && StructuredValue != null)
		{
			return SqlValueConverter.DeserializeDataTable(StructuredValue);
		}

		return SqlValueConverter.FromJson(Value, SqlDbType);
	}

	public object AsValue()
	{
		if (DbType == DbType.Object && StructuredValue != null)
		{
			return SqlValueConverter.DeserializeDataTable(StructuredValue);
		}

#pragma warning disable CS8603 // Possible null reference return.
		return SqlValueConverter.SqlValueToCsValue(AsSqlValue());
#pragma warning restore CS8603 // Possible null reference return.
	}

	public static SqlParameterDTO FromSqlParameter(SqlParameter param)
	{
		return new SqlParameterDTO
		{
			Name = param.ParameterName,
			Size = param.Size,
			Direction = param.Direction,
			DbType = param.DbType,
			SqlDbType = param.SqlDbType,
			SqlTypeName = param.TypeName,
			UdtTypeName = param.UdtTypeName,
			Value = SqlValueConverter.ToJson(param.Value, param.SqlDbType),
			ValueTypeName = param.Value?.GetType().Name ?? string.Empty,
			StructuredValue = param.SqlDbType == SqlDbType.Structured ? SqlValueConverter.SerializeDataTable(param.Value as DataTable) : null
		};
	}

	public static SqlParameterDTO FromDbParameter(DbParameter param)
	{
		var dto = new SqlParameterDTO
		{
			Name = param.ParameterName,
			Size = param.Size,
			Direction = param.Direction,
			DbType = param.DbType,
			Value = param.DbType == DbType.Object ? null : SqlValueConverter.CsValueToJson(param.Value, param.DbType),
			StructuredValue = param is { DbType: DbType.Object, Value: DataTable dataTable } ? SqlValueConverter.SerializeDataTable(dataTable) : null
		};

		if (param.Value is DataTable)
		{
			dto.SqlDbType = SqlDbType.Structured;
		}

		if (param is SqlParameter sqlParam)
		{
			dto.SqlDbType = sqlParam.SqlDbType;
			dto.SqlTypeName = sqlParam.TypeName;
		}
		else
		{
			dto.SqlDbType = TypeMappingHelper.DbTypeToSqlDbType(param.DbType);
			dto.SqlTypeName = Enum.GetName(typeof(SqlDbType), dto.SqlDbType);
		}

		return dto;
	}

	public SqlParameter ToSqlParameter()
	{
		var result = new SqlParameter
		{
			ParameterName = Name,
			Size = Size,
			Direction = Direction,
			DbType = DbType,
			SqlDbType = SqlDbType,
			UdtTypeName = UdtTypeName ?? string.Empty
		};

		if (SqlDbType == SqlDbType.Structured)
		{
			result.TypeName = SqlTypeName ?? string.Empty;

			if (StructuredValue != null)
			{
				result.Value = SqlValueConverter.DeserializeDataTable(StructuredValue);
			}
		}
		else if (SqlDbType == SqlDbType.Variant)
		{
			result.Value = string.IsNullOrEmpty(ValueTypeName)
				? Value
				: JsonHelper.FromJson(Value, Type.GetType(ValueTypeName) ?? typeof(string));
		}
		else
		{
			result.Value = SqlValueConverter.FromJson(Value, SqlDbType);
		}

		return result;
	}

	public override string ToString()
	{
		return $@"Name: {Name}
SqlDbType: {SqlDbType}
AsValue: {AsValue()}";
	}
}
