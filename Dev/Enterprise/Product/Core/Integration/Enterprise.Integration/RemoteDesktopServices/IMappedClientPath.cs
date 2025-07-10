
namespace Enterprise.Integration.RemoteDesktopServices
{
	public interface IMappedClientPath
	{
		string GetMappedPath(string unmappedPath);
		string GetUnmappedPath(string mappedPath);
	}
}
