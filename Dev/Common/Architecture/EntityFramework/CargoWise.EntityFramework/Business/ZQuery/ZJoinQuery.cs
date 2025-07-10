using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace CargoWise.EntityFramework;

public sealed class ZJoinQuery : ZQuery
{
	public ZJoinQuery(string tableName, string[] selectList)
	{
		this.tableName = Argument.NotNullOrEmpty(tableName, nameof(tableName));
		this.selectList = Argument.NotNull(selectList, nameof(selectList));

		if (selectList.Length == 0)
		{
			throw new ArgumentException("selectList cannot be empty", nameof(selectList));
		}
	}

	readonly string tableName;
	readonly string[] selectList;

	public void AddJoinPart(string join, ZSqlParameter[] parameters = null)
	{
		if (parameters != null)
		{
			otherParameters.AddRange(parameters);
		}

		joinPartBuilder
			.Append("\r\n")
			.Append(join);
	}

	public (string, IEnumerable<ZSqlParameter>) GetSql()
	{
		var sqlBuilder = new SqlBuilder();
		sqlBuilder
			.Append("SELECT ")
			.Append(GetSelectList())
			.Append("\r\nFROM ")
			.Append(tableName);

		foreach (var otherParameter in otherParameters.Union(Params))
		{
			sqlBuilder.ReserveParameter(otherParameter);
		}

		sqlBuilder.Append(joinPartBuilder.ToString());
		var whereAndOrderByClauses = GetAsWhereAndOrderByClause(false);
		sqlBuilder.Append(whereAndOrderByClauses);
		return (sqlBuilder.ToString(), sqlBuilder.Parameters);
	}

	string GetSelectList() => string.Join(", ", selectList);

	readonly SqlBuilder joinPartBuilder = new SqlBuilder();
	readonly List<ZSqlParameter> otherParameters = new List<ZSqlParameter>();

#if DEBUG
	public string JoinPartsSqlText => joinPartBuilder.ToString();
#endif
}
