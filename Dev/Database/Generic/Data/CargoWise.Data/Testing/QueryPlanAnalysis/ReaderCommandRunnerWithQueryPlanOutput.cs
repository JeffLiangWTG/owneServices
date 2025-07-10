#if DEBUG

using System.Data;
using CargoWise.Common;

namespace CargoWise.Data.Testing
{
	class ReaderCommandRunnerWithQueryPlanOutput : ReaderCommandRunner
	{
		public ReaderCommandRunnerWithQueryPlanOutput(CommandBehavior behavior, DbCommand dbCommand)
			: base(behavior)
		{
			Argument.NotNull(dbCommand, nameof(dbCommand));

			this.dbCommand = dbCommand;
		}

		readonly DbCommand dbCommand;

		protected override object ExecuteCore(IDbCommand command)
		{
			return new DataReaderWrapperWithQueryPlanExtraction(command, dbCommand, behaviour);
		}
	}
}

#endif