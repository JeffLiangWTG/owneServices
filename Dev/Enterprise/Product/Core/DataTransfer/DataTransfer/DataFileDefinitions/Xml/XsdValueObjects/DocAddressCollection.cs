using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class DocAddressCollection : Xsd.AutoDocAddressCollection
	{
		public Xsd.DocAddress AddNew(Xsd.DocAddressAddressType addressType)
		{
			Xsd.DocAddress result = AddNew();
			result.AddressType = addressType;
			result.AddressTypeSpecified = true;
			return result;
		}

		public Xsd.DocAddress GetOrCreateAddressByType(Xsd.DocAddressAddressType addressType)
		{
			Xsd.DocAddress result = GetAddressByType(addressType) ?? AddNew(addressType);
			return result;
		}

		public Xsd.DocAddress GetAddressByType(Xsd.DocAddressAddressType addressType)
		{
			foreach (Xsd.DocAddress docAddress in this)
			{
				if (docAddress.AddressTypeSpecified && docAddress.AddressType == addressType)
				{
					return docAddress;
				}
			}

			return null;
		}
	}
}
