using System;
using Newtonsoft.Json;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	public class BrowserMessageEventArgs<DataType> : EventArgs
	{
		[JsonProperty("kind")]
		public string Kind { get; set; }
		[JsonProperty("payload")]
		public DataType Payload { get; set; }
	}
}
