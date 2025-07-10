using System;
using System.Collections.Generic;

namespace ServiceManager.Shared.Abstractions
{
	public interface IServiceTaskCollectionGovernor
	{
		void SetActive(bool value, IEnumerable<Guid> taskPkFilter);
		void SetBranchPk(Guid pk, IEnumerable<Guid> taskPkFilter);
		void Reload();

		IEnumerable<IServiceTask> GovernedTasks { get; }
	}
}
