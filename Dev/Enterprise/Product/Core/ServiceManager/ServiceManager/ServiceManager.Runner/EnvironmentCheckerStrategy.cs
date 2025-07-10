using System;
using System.Collections.Generic;
using CargoWise.Common;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner;

public class EnvironmentCheckerStrategy : IEnvironmentCheckerStrategy
{
	public EnvironmentCheckerStrategy(IEnumerable<IEnvironmentChecker> environmentCheckers)
	{
		this.environmentCheckers = environmentCheckers ?? throw new ArgumentNullException(nameof(environmentCheckers));
	}

	public void Initialize(IRunCommandInfo runCommandInfo)
	{
		foreach (var checker in environmentCheckers)
		{
			checker.Initialize(runCommandInfo);
		}
	}

	public void ExecuteOnServiceTaskCompletion(IServiceTaskHandler serviceTaskHandler)
	{
		try
		{
			foreach (var checker in environmentCheckers)
			{
				checker.CheckOnServiceTaskCompletion(serviceTaskHandler);
			}
		}
		catch (EnvironmentCorruptedException)
		{
			throw;
		}
		catch (Exception exception) when (!exception.IsCriticalException())
		{
			throw new EnvironmentCheckerException(exception);
		}
	}

	public void ExecuteOnServiceTaskException(IServiceTaskHandler serviceTaskHandler)
	{
		try
		{
			foreach (var checker in environmentCheckers)
			{
				checker.CheckOnServiceTaskException(serviceTaskHandler);
			}
		}
		catch (EnvironmentCorruptedException)
		{
			throw;
		}
		catch (Exception exception) when (!exception.IsCriticalException())
		{
			throw new EnvironmentCheckerException(exception);
		}
	}

	readonly IEnumerable<IEnvironmentChecker> environmentCheckers;
}
