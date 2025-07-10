
namespace Enterprise.BlazorWinFormsInterop
{
	public interface IWinFormsListener
	{
		string ListenUrl { get; }
		void Initialise();
		void OpenModule(string id, string queryString, out bool success);
		bool ExitHybridMode();
		void Disable();
#if DEBUG
		void StartListener();
#endif
	}
}

