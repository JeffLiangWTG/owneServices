using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	public class ShipmentCollection : Xsd.AutoShipmentCollection
	{
		public string GetTotalShipmentWeight()
		{
			decimal totalShipmentWeight = 0;

			foreach (Xsd.Shipment ship in this)
			{
				totalShipmentWeight += ship.ShipmentDetails.Weight.Value;
			}

			return totalShipmentWeight.ToString();
		}

		public string GetTotalShipmentVolume()
		{
			decimal totalShipmentVolume = 0;

			foreach (Xsd.Shipment ship in this)
			{
				totalShipmentVolume += ship.ShipmentDetails.Volume.Value;
			}

			return totalShipmentVolume.ToString();
		}
	}
}
