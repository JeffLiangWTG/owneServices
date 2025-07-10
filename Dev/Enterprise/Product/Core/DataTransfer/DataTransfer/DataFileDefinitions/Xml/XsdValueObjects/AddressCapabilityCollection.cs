using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoAddressCapabilityCollection")]
	public class AddressCapabilityCollection : Xsd.AutoAddressCapabilityCollection, ISequencedValueObjectCollection
	{
		public bool HasCapabilityOfType(AddressCapabilityAddressType type)
		{
			foreach (AddressCapability capability in this)
			{
				if ((capability.AddressType == type) && (capability.AddressTypeSpecified))
				{
					return true;
				}
			}
			return false;
		}
	}
}
