using System;
using System.IO;
using CargoWise.Data;
using CargoWise.Schema;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public static class DataHelpers
	{
		public static void ClearTable(string tableName) => ClearTable(tableName, truncate: false);

		public static void ClearTable(string tableName, bool truncate)
		{
			var command = truncate ? "TRUNCATE TABLE" : "DELETE";
			var query = FormattableString.Invariant($"{command} [{tableName}]");
			Db.Connection.ExecuteNonQuery(query);
		}

		public static Guid GetFirstKey(ITableSchema table)
		{
			if (table is null)
			{
				throw new ArgumentNullException(nameof(table));
			}

			var command = FormattableString.Invariant($"SELECT TOP(1) [{table.PK.Name}] FROM [{table.TableName}]");
			var result = Db.Connection.ExecuteScalar(command)
				?? throw new InvalidDataException(FormattableString.Invariant($"There are no records in table '{table.TableName}'."));

			return (Guid)result;
		}

		public static (Guid companyPk, Guid branchPk) GetCompanyAndBranch()
		{
			var companyPk = Guid.Empty;
			var branchPk = Guid.Empty;
			Db.Connection.ExecuteReader(
				"SELECT TOP 1 GC_PK,GB_PK FROM dbo.GLBCOMPANY INNER JOIN dbo.GLBBRANCH ON GB_GC=GC_PK",
				(reader) =>
				{
					companyPk = reader.GetGuid(0);
					branchPk = reader.GetGuid(1);
				});

			return (companyPk, branchPk);
		}
	}
}
