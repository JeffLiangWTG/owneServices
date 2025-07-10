using System.Collections.Generic;
using System.Text;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts
{
	public class UpdateStatementBuilder : IStatementBuilder
	{
		public struct SetStatement
		{
			public string Field { get; set; }
			public string Value { get; set; }
		}

		internal string tableName;
		readonly List<string> ignoredConstraints = new List<string>();
		internal List<SetStatement> setStatements = new List<SetStatement>();
		string condition;
		public UpdateStatementBuilder From(string tableName)
		{
			this.tableName = tableName;
			return this;
		}
		public UpdateStatementBuilder IgnoreConstraint(string constraintName)
		{
			ignoredConstraints.Add(constraintName);
			return this;
		}
		public UpdateStatementBuilder Set(string field, string value)
		{
			this.setStatements.Add(new SetStatement() { Field = field, Value = value });
			return this;
		}
		public UpdateStatementBuilder Where(string condition)
		{
			this.condition = condition;
			return this;
		}

		void IStatementBuilder.Build(StringBuilder stringBuilder, string databaseName)
		{
			var template = @"
IF EXISTS(SELECT null FROM [{0}].sys.tables WHERE name = '{1}')
BEGIN
	UPDATE [{0}]..{1} SET ";
			stringBuilder.AppendFormat(template, databaseName, tableName);
			int setStatementCounter = 0;
			foreach (var setStatement in setStatements)
			{
				stringBuilder.AppendFormat("{0} = {1}", setStatement.Field, setStatement.Value);
				setStatementCounter++;
				if (setStatementCounter < setStatements.Count)
				{
					stringBuilder.Append(" , ");
				}
			}
			if (!string.IsNullOrWhiteSpace(condition))
			{
				stringBuilder.AppendFormat(" WHERE {0}", condition);
			}
			stringBuilder.AppendLine("\nEND");
		}
	}
}
