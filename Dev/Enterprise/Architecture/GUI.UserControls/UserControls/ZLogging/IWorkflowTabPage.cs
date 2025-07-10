namespace Enterprise.ZArchitecture.GUI
{
	public interface IWorkflowTabPage
	{
		bool SupportsEventTracking { get; }

		bool Initialized { get; }
	}
}
