namespace CargoWise.ServiceManager.Next.Shared;

public interface INextRunnerOptions : INextSharedOptions
{
	public const string LauncherHubOption = "-LauncherHub:";
	Uri LauncherHub { get; }
}
