namespace ServiceManager.Integration.Abstractions
{
	public interface ILogViewerDataProvider
	{
		string[] GetFileNames();
		byte[] GetBytes(string fileName);
		string Hostname { get; }
	}
}
