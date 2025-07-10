using System.Xml.Serialization;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.PortalAuth
{
	public class AutoLoginTokenScope
	{
		[XmlElement(ElementName = "OrgCode")]
		public string OrgCode { get; set; } = string.Empty;

		[XmlElement(ElementName = "DatabaseNumber")]
		public string DatabaseNumber { get; set; } = string.Empty;

		[XmlElement(ElementName = "ReturnUrl")]
		public string ReturnUrl { get; set; } = string.Empty;
	}
}
