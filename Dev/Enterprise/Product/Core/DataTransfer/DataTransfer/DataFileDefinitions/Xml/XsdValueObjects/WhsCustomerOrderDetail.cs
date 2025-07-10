using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoWhsCustomerOrderDetail")]
	public class WhsCustomerOrderDetail : Xsd.AutoWhsCustomerOrderDetail
	{
		public WhsCustomerOrderDetail()
		{
			GoodsBilledTo.AddressType = Xsd.DocAddressAddressType.GBA;
			GoodsBilledTo.AddressTypeSpecified = true;

			Consignee.AddressType = Xsd.DocAddressAddressType.CEA;
			Consignee.AddressTypeSpecified = true;
		}
	}
}
