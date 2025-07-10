#if DEBUG

using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Shared
{
	public class BatchInsertHelper
	{
		readonly LinkedList<LinkedList<string>> insertStatements = new LinkedList<LinkedList<string>>();

		public IEnumerable<string> GetAllInsertStatements()
		{
			return insertStatements.Select(list => string.Join(System.Environment.NewLine, list));
		}

		public void AddInsertStatement(InsertStatement insertStatement)
		{
			var shouldIncludeInsertHeader = insertStatements.Count == 0 || insertStatements.Last.Value.Count == MaxRowValueExpressionsInInsertStatements;
			var statementSql = shouldIncludeInsertHeader ? insertStatement.FullStatement : "," + insertStatement.InsertValuesBlock;

			if (shouldIncludeInsertHeader)
			{
				insertStatements.AddLast(new LinkedList<string>());
			}

			insertStatements.Last.Value.AddLast(statementSql);
		}

		const int MaxRowValueExpressionsInInsertStatements = 1000;

		public void ExecuteAll(DbConnection connection)
		{
			foreach (var statement in GetAllInsertStatements())
			{
				connection.ExecuteNonQuery(statement);
			}
		}
	}
}

#endif
