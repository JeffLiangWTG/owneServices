using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	class ProcessHeaderUniqueCompletionStatementGenerator : IProcessHeaderUniqueCompletionStatementGenerator
	{
		public ZString GetUniqueCompletionStatement(IProcessHeaderCollection processHeaders, ZString completionStatement)
			=> ProcessHeaderCompletionStatementSequenceHandler.GetUniqueCompletionStatementForReappliedWorkflowTemplate(processHeaders, completionStatement);
	}
}
