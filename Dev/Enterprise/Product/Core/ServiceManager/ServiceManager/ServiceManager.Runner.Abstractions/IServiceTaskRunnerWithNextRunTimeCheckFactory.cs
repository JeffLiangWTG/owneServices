namespace ServiceManager.Runner.Abstractions;

public interface IServiceTaskRunnerWithNextRunTimeCheckFactory
{
	IServiceTaskRunnerWithNextRunTimeCheck CreateRunner();
}

