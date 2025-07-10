namespace ServiceManager.Runner.Abstractions
{
	public interface IRunCommandInfo : ICommandInfo
	{
		string AssemblyName { get; }
		string Code { get; }
		string ConfigString { get; }
	}
}
