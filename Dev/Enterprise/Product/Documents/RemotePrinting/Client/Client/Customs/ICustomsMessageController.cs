namespace Enterprise.RemotePrinting.Client;

public interface ICustomsMessageController
{
	WebClientConfiguration ConfigSetting { get; }
	void ResetConfigSetting();
	void ShowInformation(string message);
}
