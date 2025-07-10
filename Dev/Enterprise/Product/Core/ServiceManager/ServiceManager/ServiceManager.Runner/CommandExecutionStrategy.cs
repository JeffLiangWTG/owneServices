using System;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	class CommandExecutionStrategy : ICommandExecutionStrategy
	{
		public CommandExecutionStrategy(IServiceTaskRunnerStrategy serviceTaskRunnerStrategy)
		{
			this.serviceTaskRunnerStrategy = serviceTaskRunnerStrategy ?? throw new ArgumentNullException(nameof(serviceTaskRunnerStrategy));
		}

		public ServiceTaskRunResult Execute(ICommandInfo commandInfo)
		{
			switch (commandInfo ?? throw new ArgumentNullException(nameof(commandInfo)))
			{
				case IStopCommandInfo _:
					return ServiceTaskRunResult.Cancelled;
				case IRunCommandInfo runCommandInfo:
					return serviceTaskRunnerStrategy.Run(runCommandInfo);
				default:
					throw new NotImplementedException();
			}
		}

		readonly IServiceTaskRunnerStrategy serviceTaskRunnerStrategy;
	}
}
