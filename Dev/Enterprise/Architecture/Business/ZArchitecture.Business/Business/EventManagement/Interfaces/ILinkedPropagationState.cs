using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Business
{
	public interface ILinkedPropagationState
	{
		IEnumerable<(IStmALogParent propagationTarget, StmALog propagatedLog, StmALog replacedLog)> InMemoryPropagatedLogs { get; }
		void AddPropagatedLog(IStmALogParent logParent, StmALog propagatedLog, StmALog replacedLog);
		void ClearInMemoryPropagatedLogs();
		bool TryRemovePropagatedLog(IStmALogParent logParent, StmALog propagatedLog);
	}
}
