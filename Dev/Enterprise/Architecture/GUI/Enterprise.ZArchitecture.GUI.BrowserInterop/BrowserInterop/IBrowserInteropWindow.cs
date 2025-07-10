namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	public interface IBrowserInteropWindow
	{
		IMessageTransportLayer MessageTransportLayer { get; }

		bool? ShowDialog();

		void Close();
	}
}
