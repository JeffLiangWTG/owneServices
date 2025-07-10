using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoISFContainer")]
	public class ISFContainer : Xsd.AutoISFContainer
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return ContainerNumberSpecified || DescriptionCodeSpecified || ISOTypeSpecified; }
		}
	}
}
