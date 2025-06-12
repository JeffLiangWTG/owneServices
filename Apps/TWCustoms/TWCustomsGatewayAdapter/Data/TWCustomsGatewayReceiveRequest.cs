using System;
using System.Xml.Serialization;

namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter.Data
{
	[Serializable]
	[XmlType(AnonymousType = true, Namespace = "http://cargowise.com/ehub/products/TWCPluginRequest")]
	[XmlRoot(Namespace = "http://cargowise.com/ehub/products/TWCPluginRequest", IsNullable = false, ElementName = "TWCPluginServiceReceiveRequest")]
	public class TWCustomsGatewayReceiveRequest : ITWCustomsRequest
	{
		[XmlElement(ElementName = "clientRegistrationId")]
		public string ClientRegistrationId { get; set; }
		[XmlElement(ElementName = "registrationConfiguration")]
		public string RegistrationConfiguration { get; set; }
	}
}