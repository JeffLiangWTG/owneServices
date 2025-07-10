using Newtonsoft.Json;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop.Tests
{
	public static class IMessageTransportLayerExtensions
	{
		public static void HandleObjectFromBrowser<DataType>(this IMessageTransportLayer messageTransportLayer, string kind, DataType payload = null) where DataType : class
		{
			var message = new BrowserMessageEventArgs<DataType> { Kind = kind, Payload = payload };
			var messageStr = JsonConvert.SerializeObject(message);
			messageTransportLayer.HandleFromBrowser(messageStr);
		}
	}
}
