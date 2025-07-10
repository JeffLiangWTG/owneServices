using System.Data;
using CargoWise.Common;
using CargoWise.Data.Diagnostics;

namespace CargoWise.Data
{
	public abstract class CommandRunner
	{
		protected CommandRunner()
		{
		}

		public object Execute(IDbCommand command)
		{
			Argument.NotNull(command, nameof(command));
			Argument.NotNullOrEmpty(command.CommandText, nameof(command.CommandText));

			if (QueryStackTraceRecorderCore.InstanceCore.Enabled && command is SqlCommand sqlCommand)
			{
				command = QueryStackTraceRecorderCore.InstanceCore.GetCommandWithCallStackTraceAddedIfEnabled(sqlCommand);
			}

			return ExecuteCore(command);
		}

		protected abstract object ExecuteCore(IDbCommand command);
	}

	#region CommandRunner classes

	/// <summary>
	/// Runs a command using ExecuteReader()
	/// </summary>
	public class ReaderCommandRunner : CommandRunner
	{
		public ReaderCommandRunner()
			: this(CommandBehavior.Default)
		{
		}

		public ReaderCommandRunner(CommandBehavior behavior)
		{
			behaviour = behavior;
		}

		protected override object ExecuteCore(IDbCommand command)
		{
			return command.ExecuteReader(behaviour);
		}

		protected CommandBehavior behaviour;
	}

	/// <summary>
	/// Runs a command using ExecuteScalar()
	/// </summary>
	public class ScalarCommandRunner : CommandRunner
	{
		protected override object ExecuteCore(IDbCommand command)
		{
			return command.ExecuteScalar();
		}
	}

	/// <summary>
	/// Runs a command using ExecuteNonQuery()
	/// </summary>
	public class NonQueryCommandRunner : CommandRunner
	{
		protected override object ExecuteCore(IDbCommand command)
		{
			return command.ExecuteNonQuery();
		}
	}

	#endregion
}
