using System.Collections.Generic;
using System.Text;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts
{
	public class DeleteStatementBuilder : IStatementBuilder
	{
		internal string tableName;
		readonly List<string> ignoredConstraints = new List<string>();
		readonly List<string> cascadeConstraints = new List<string>();
		string condition;
		public DeleteStatementBuilder From(string tableName)
		{
			this.tableName = tableName;
			return this;
		}

		public DeleteStatementBuilder IgnoreConstraint(string constraintName)
		{
			ignoredConstraints.Add(constraintName);
			return this;
		}
		public DeleteStatementBuilder CascadeDeleteOn(string constraintName)
		{
			cascadeConstraints.Add(constraintName);
			return this;
		}
		public DeleteStatementBuilder Where(string condition)
		{
			this.condition = condition;
			return this;
		}

		void IStatementBuilder.Build(StringBuilder stringBuilder, string databaseName)
		{
			var template = @"
IF EXISTS(SELECT null FROM [{0}].sys.tables WHERE name = '{1}')
BEGIN
	DELETE FROM [{0}]..{1}";

			stringBuilder.AppendFormat(template, databaseName, tableName);
			if (!string.IsNullOrWhiteSpace(condition))
			{
				stringBuilder.AppendFormat(" WHERE {0}", condition);
			}
			stringBuilder.AppendLine("\nEND");
		}
	}
}
