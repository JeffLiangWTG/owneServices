using CargoWise.Data;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner;

class TransactionChecker : IEnvironmentChecker
{
	public void Initialize(IRunCommandInfo runCommandInfo) { }

	public void CheckOnServiceTaskCompletion(IServiceTaskHandler serviceTaskHandler)
	{
		var transactionCount = -1;

		transactionCount = Db.Connection.AppTransactionCount > 0
			? Db.Connection.AppTransactionCount
			: Db.Connection.ExecuteScalar<int>("SELECT @@TRANCOUNT");

		if (transactionCount > 0)
		{
			throw new UncommittedTransactionException(serviceTaskHandler.HostedServiceAttribute, transactionCount);
		}
	}

	public void CheckOnServiceTaskException(IServiceTaskHandler serviceTaskHandler) { }
}
