using System;
using System.Xml;
using System.Xml.Serialization;

namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter.Data
{
	[Serializable]
	[XmlType(AnonymousType = true, Namespace = "http://cargowise.com/ehub/products/TWCPluginResponse")]
	[XmlRoot(Namespace = "http://cargowise.com/ehub/products/TWCPluginResponse", IsNullable = false, ElementName = "TWCPluginServiceResponse")]
	public class TWCustomsGatewayResponse
	{
		public static TWCustomsGatewayResponse OK => new TWCustomsGatewayResponse()
		{
			ResponseCode = "0",
			ResponseMessage = "OK"
		};

		[XmlElement(ElementName = "responseCode")]
		public string ResponseCode { get; set; }

		[XmlElement(ElementName = "responseMessage")]
		public string ResponseMessage { get; set; }

		public TWCustomsGatewayResponse(AdapterResult result)
		{
			ResponseCode = result.ErrorCode;
			ResponseMessage = result.ErrorMessge;
		}

		public TWCustomsGatewayResponse(Exception e)
		{
			ResponseCode = "-1";
			ResponseMessage = e.Message;
		}

		private TWCustomsGatewayResponse()
		{
		}
	}
}