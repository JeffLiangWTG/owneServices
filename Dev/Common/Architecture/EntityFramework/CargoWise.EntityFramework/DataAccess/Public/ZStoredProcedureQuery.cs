using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class ZStoredProcedureQuery : ZQuery
	{
		public ZStoredProcedureQuery(ZString storedProcedureName, ZSqlParameter[] parameters)
		{
			IsDBOnlyQuery = true;
			this.storedProcedureName = storedProcedureName;
			this.parameters = parameters;
		}

		public override ZDataQuery GetDataQuery(ZSqlConnectionInfo connectionInfo, string tableName) => new ZDataQuery(storedProcedureName, parameters, tableName);

		protected internal override void AddLiteralTextADO(SqlBuilder sqlBuilder)
		{
			sqlBuilder.Append(storedProcedureName);
			if (parameters != null)
			{
				sqlBuilder.Append(" ");
				parameters
					.OrderBy(p => p.ParameterName)
					.ForEachWithBetween(p =>
					{
						sqlBuilder.Append(p.ParameterName).Append(" = ").Append(p.ParameterValueTextADO);
					}, () => sqlBuilder.Append(", "));
			}
		}

		protected override bool CanAddFilter => throw new NotSupportedException("Cannot use a Stored Procedure for filtering or add filtering to a ZStoredProcedure");
		public override bool SupportsFetchHints => false;

		readonly string storedProcedureName;
		readonly ZSqlParameter[] parameters;
	}
}
