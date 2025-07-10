using System;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Shared.Abstractions
{
	public interface ITransactionAdapter : IDisposable
	{
		IServiceTaskGovernor? GetServiceTaskGovernor(Guid pk);
		IServiceTaskGovernor? GetServiceTaskGovernor(string code);
		IServiceTaskGovernor GetNewServiceTaskGovernor(IHostedServiceAttribute newTaskAttribute);
		IServiceTaskCollectionGovernor GetCollectionGovernorForAllTasks();
		void Commit();
		event EventHandler? Committing;
	}
}
