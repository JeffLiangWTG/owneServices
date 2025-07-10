namespace Enterprise.RemotePrinting.Client
{
	public interface ICustomseHubClientSetting
	{
		string MachineName { get; }
		string EHubClientStatus { get; }
		string EHubClientID { get; }
		string EHubClientPassword { get; }
		string EHubGatewayServerAddress { get; }
		int RunningIntervalInSeconds { get; }
		bool IsValid { get; set; }
	}
}
