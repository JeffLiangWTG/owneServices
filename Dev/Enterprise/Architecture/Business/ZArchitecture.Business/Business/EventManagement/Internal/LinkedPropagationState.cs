using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business
{
	sealed class LinkedPropagationState : ILinkedPropagationState, IStmALogInMemoryIdentifier
	{
		readonly LinkedPropagationStateService linkedPropagationStateService;
		readonly IStmALog log;

		public LinkedPropagationState(BusinessObjectFactory factory, IStmALog log)
		{
			var service = factory.ServiceContainer.GetService<LinkedPropagationStateService>()
				?? factory.ServiceContainer.AddService(new LinkedPropagationStateService());

			linkedPropagationStateService = service;
			this.log = log;

			factory.Saved += (_, saveSuccess) =>
			{
				if (saveSuccess)
				{
					ClearInMemoryPropagatedLogs();
				}
			};
		}

		public ZGuid InMemoryIdentifier => linkedPropagationStateService.GetLogInMemoryUniqueIdentifier(log);

		public IEnumerable<(IStmALogParent propagationTarget, StmALog propagatedLog, StmALog replacedLog)> InMemoryPropagatedLogs => linkedPropagationStateService.InMemoryPropagatedLogs;

		public void AddPropagatedLog(IStmALogParent logParent, StmALog propagatedLog, StmALog replacedLog)
		{
			linkedPropagationStateService.AddPropagatedLog(logParent, propagatedLog, replacedLog);
		}

		public bool TryRemovePropagatedLog(IStmALogParent logParent, StmALog propagatedLog)
		{
			return linkedPropagationStateService.TryRemovePropagatedLog(logParent, propagatedLog);
		}

		public void ClearInMemoryPropagatedLogs()
		{
			linkedPropagationStateService.ClearInMemoryPropagatedLogs();
		}

		public void AddLogInMemoryUniqueIdentifier(IStmALog log, ZGuid identifier)
		{
			linkedPropagationStateService.AddLogInMemoryUniqueIdentifier(log, identifier);
		}

		sealed class LinkedPropagationStateService : IService, ILinkedPropagationState
		{
			readonly Dictionary<IStmALogParent, List<(StmALog propagatedLog, StmALog replacedLog)>> propagatedLogs = new Dictionary<IStmALogParent, List<(StmALog propagatedLog, StmALog replacedLog)>>();
			readonly Dictionary<ZGuid, ZGuid> logIdentifiers = new Dictionary<ZGuid, ZGuid>();

			public IEnumerable<(IStmALogParent propagationTarget, StmALog propagatedLog, StmALog replacedLog)> InMemoryPropagatedLogs => propagatedLogs.SelectMany(parentLogs => parentLogs.Value.Select(logPair => (parentLogs.Key, logPair.propagatedLog, logPair.replacedLog)));

			public void AddPropagatedLog(IStmALogParent logParent, StmALog propagatedLog, StmALog replacedLog)
			{
				if (!propagatedLogs.TryGetValue(logParent, out var logs))
				{
					logs = new List<(StmALog propagatedLog, StmALog replacedLog)>();
					propagatedLogs.Add(logParent, logs);
				}

				logs.Add((propagatedLog, replacedLog));
			}

			public bool TryRemovePropagatedLog(IStmALogParent logParent, StmALog propagatedLog)
			{
				if (!propagatedLogs.TryGetValue(logParent, out var logs))
				{
					return false;
				}

				return logs.RemoveAll(logPair => logPair.propagatedLog.PK == propagatedLog.PK) > 0;
			}

			public void ClearInMemoryPropagatedLogs()
			{
				propagatedLogs.Clear();
			}

			public void AddLogInMemoryUniqueIdentifier(IStmALog log, ZGuid identifier)
			{
				logIdentifiers.Add(log.PK, identifier);
			}

			public ZGuid GetLogInMemoryUniqueIdentifier(IStmALog log)
			{
				if (logIdentifiers.TryGetValue(log.PK, out var identifier))
				{
					return identifier;
				}

				return ZGuid.Empty;
			}
		}
	}
}
