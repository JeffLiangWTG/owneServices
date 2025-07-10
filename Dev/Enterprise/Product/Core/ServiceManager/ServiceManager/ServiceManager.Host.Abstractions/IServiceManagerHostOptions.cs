namespace ServiceManager.Host.Abstractions
{
	public interface IServiceManagerHostOptions
	{
		bool OptionConsole { get; }
		bool OptionInstall { get; }
		bool OptionUninstall { get; }
		bool OptionStop { get; }
		bool OptionStart { get; }
		bool OptionQuiet { get; }
		string ServerName { get; }
		string DatabaseName { get; }
		string Username { get; }
		string Password { get; }
		string Host { get; }
		bool Automatic { get; }
		string Config { get; }
		bool RemoveDbRecord { get; }
	}
}
