namespace Enterprise.ZArchitecture.Modules
{
	public interface ILogSubscriber
	{
		string Name { get; }
		string[] EventTypes { get; }
		string[] TableNames { get; }
	}
}

