using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	public interface IMessageTransportLayer
	{
		Action<BrowserMessageEventArgs<JToken>> DefaultHandler { get; set; }

		void AddCommandHandler<DataType>(string command, Action<BrowserMessageEventArgs<DataType>> handler) where DataType : class;

		Task SendToBrowserAsync<DataType>(string command, DataType data = null) where DataType : class;

		void HandleFromBrowser(string message);
	}
}
