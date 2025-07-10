using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoShipmentIdentifier")]
	public class ShipmentIdentifier : Xsd.AutoShipmentIdentifier
	{
		public string GetMasterBill(Xsd.Consol consol)
		{
			return (consol != null && Masterbill.IsEmpty) ? consol.Masterbill : Masterbill;
		}

		public MasterAndHouseBill GetMasterAndHouseBill(Xsd.Consol consol)
		{
			ZString masterBill = GetMasterBill(consol);
			ZString houseBill = Value;

			MasterAndHouseBill result = null;
			if (!masterBill.IsEmpty || !houseBill.IsEmpty)
			{
				result = new MasterAndHouseBill(masterBill, houseBill);
			}
			return result;
		}
	}
}
