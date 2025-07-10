
namespace Enterprise.Security
{
	public interface IWiseCloudSecurityClient
	{
		string GetClientIPAddress(string license, string username);
	}
}
