using System.Collections.Generic;
using System.Text;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts
{
	public interface IClearDataScript
	{
		void BuildScript(ClearDataScriptBuilder builder);
	}

	public interface IStatementBuilder
	{
		void Build(StringBuilder stringBuilder, string databaseName);
	}

	public class ClearDataScriptBuilder
	{
		protected List<IStatementBuilder> statementBuilders = new List<IStatementBuilder>();

		readonly string databaseName;
		public ClearDataScriptBuilder(string databaseName)
		{
			this.databaseName = databaseName.Trim().Trim('[').Trim(']');
		}

		public DeleteStatementBuilder DeleteRecords()
		{
			var deleteBuilder = new DeleteStatementBuilder();
			statementBuilders.Add(deleteBuilder);
			return deleteBuilder;
		}
		public UpdateStatementBuilder UpdateRecords()
		{
			var updateBuilder = new UpdateStatementBuilder();
			statementBuilders.Add(updateBuilder);
			return updateBuilder;
		}
		public CommentStatementBuilder AddComment()
		{
			var commentBuilder = new CommentStatementBuilder();
			statementBuilders.Add(commentBuilder);
			return commentBuilder;
		}

		public string Build()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var statementBuilder in statementBuilders)
			{
				statementBuilder.Build(sb, databaseName);
				sb.AppendLine();
			}
			return sb.ToString();
		}
	}

	public class CommentStatementBuilder : IStatementBuilder
	{
		string comment;

		public void ForStarting(IClearDataScript script)
		{
			comment = $"-- \n-- Script From {script.GetType().Name} \n--";
		}

		void IStatementBuilder.Build(StringBuilder stringBuilder, string databaseName)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(comment);
		}
	}
}
