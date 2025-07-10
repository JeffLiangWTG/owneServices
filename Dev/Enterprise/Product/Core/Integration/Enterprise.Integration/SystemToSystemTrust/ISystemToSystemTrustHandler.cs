namespace Enterprise.Integration.SystemToSystemTrust
{
	public interface ISystemToSystemTrustHandler
	{
		void SendMessage(string accessToken, string postUrl);
	}
}
