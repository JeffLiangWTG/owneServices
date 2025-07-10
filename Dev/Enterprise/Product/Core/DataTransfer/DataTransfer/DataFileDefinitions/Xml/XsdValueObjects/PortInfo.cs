using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoPortInfo")]
	public class PortInfo : Xsd.AutoPortInfo
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return base.IsSpecified && (ContainerYard.IsSpecified || CTO.IsSpecified || Depot.IsSpecified); }
		}
	}
}
