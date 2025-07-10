namespace Enterprise.ServiceManager.Runner
{
	interface ICurrentProcessInfoProvider
	{
		TimeSpan TotalProcessorTime { get; }
	}
}
