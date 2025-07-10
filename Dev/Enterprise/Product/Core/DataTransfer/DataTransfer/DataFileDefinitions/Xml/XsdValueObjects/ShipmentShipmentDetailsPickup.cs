using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoShipmentShipmentDetailsPickup")]
	public class ShipmentShipmentDetailsPickup : Xsd.AutoShipmentShipmentDetailsPickup
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get
			{
				return base.IsSpecified
					&&
					(PickupFrom.IsValid
					|| CartageAdvised.IsValid
					|| PickupRequiredBy.IsValid
					|| GoodsPickup.IsValid
					|| DateOfReceipt.IsValid
					|| Address.IsSpecified
					|| CartageCompany.IsSpecified
					|| CFS.AddressSpecified);
			}
		}
	}
}
