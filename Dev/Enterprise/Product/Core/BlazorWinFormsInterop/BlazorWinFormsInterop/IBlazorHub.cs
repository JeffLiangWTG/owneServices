namespace Enterprise.BlazorWinFormsInterop
{
	public interface IBlazorHub
	{
		void BackchannelConnected();
		void Echo(string value);
		void RunEnterpriseUrl(string url);
		void DisableHybridMode();
	}
}
