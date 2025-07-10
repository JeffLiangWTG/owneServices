using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoOrgAddress")]
	public class OrgAddress : Xsd.AutoOrgAddress, ISequencedValueObject
	{
		[XmlIgnore]
		public ZBool AddressCapabilityTypeSpecified
		{
			get
			{
				foreach (AddressCapability capability in AddressCapabilities)
				{
					if (capability.AddressTypeSpecified)
					{
						return ZBool.True;
					}
				}
				return ZBool.False;
			}
		}
	}
}
