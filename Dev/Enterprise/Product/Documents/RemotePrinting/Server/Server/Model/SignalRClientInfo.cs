using System.Collections.Generic;
using System.Xml.Serialization;

namespace Enterprise.RemotePrinting.Server.Model
{
	[XmlRoot(ElementName = "SignalRClientInfo", Namespace = "http://www.cargowise.com/")]
	public class SignalRClientInfo
	{
		[XmlElement(ElementName = "ServerName", Namespace = "http://www.cargowise.com/")]
		public string ServerName { get; set; }

		[XmlElement(ElementName = "PrintersList", Namespace = "http://www.cargowise.com/")]
		public List<string> PrintersList { get; set; }

		[XmlIgnore]
		public string WebServerAddress { get; set; }

		[XmlIgnore]
		public string WebServerHostName { get; set; }

		[XmlElement(ElementName = "WebPrintClientVersionNumber", Namespace = "http://www.cargowise.com/")]
		public string WebPrintClientVersionNumber { get; set; }

		[XmlElement(ElementName = "ClientId", Namespace = "http://www.cargowise.com/")]
		public string ClientId { get; set; }
	}

	[XmlSerializerAssembly("RemotePrinting.Server.XmlSerializers")]
	[XmlRoot(ElementName = "ArrayOfSignalRClientInfo", Namespace = "http://www.cargowise.com/")]
	public class ArrayOfSignalRClientInfo
	{
		[XmlElement(ElementName = "SignalRClientInfo", Namespace = "http://www.cargowise.com/")]
		public List<SignalRClientInfo> SignalRClientInfo { get; set; }
	}
}
