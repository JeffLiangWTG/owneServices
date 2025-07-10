namespace Enterprise.BlazorWinFormsInterop
{
	public interface IBlazorClient
	{
		void OpenUrlInWinzorMode(string queryString);
		void EchoReply(string value);
		void ExitWinzorMode();
	}
}
