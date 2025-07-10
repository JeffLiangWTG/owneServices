using System.Threading.Tasks;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	public interface ISendMessageToBrowser
	{
		Task SendToBrowserAsync(string message);
	}
}
