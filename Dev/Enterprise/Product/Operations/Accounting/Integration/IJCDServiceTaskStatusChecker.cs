namespace Enterprise.Accounting.Integration
{
	public interface IJCDServiceTaskStatusChecker
	{
		bool IsJCDServiceTaskComplete { get; }
	}
}
