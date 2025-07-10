using System;
using CargoWise.Schema;

namespace CargoWise.Data
{
	public interface IZSqlParameter
	{
		bool IsLiteralOnly { get; }
		bool IsTableValued { get; }
		string ParameterName { get; }
		SchemaColumn SchemaColumn { get; }
		object ValueForSql { get; }
		object Value { get; }
		string GetSqlParameterSuffix(DateTime factoryCreateTime);
	}
}
