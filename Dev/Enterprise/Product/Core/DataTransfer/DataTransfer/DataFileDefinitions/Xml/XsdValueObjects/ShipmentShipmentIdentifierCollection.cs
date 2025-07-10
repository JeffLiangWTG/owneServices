using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class ShipmentIdentifierCollection : Xsd.AutoShipmentIdentifierCollection
	{
		public Xsd.ShipmentIdentifier FindFirst(Xsd.ShipmentIdentifierType typeOfIdentifier)
		{
			foreach (Xsd.ShipmentIdentifier identifier in this)
			{
				if (identifier.ShipmentIdentifierType == typeOfIdentifier)
				{
					return identifier;
				}
			}
			return null;
		}

		public Xsd.ShipmentIdentifierCollection Find(Xsd.ShipmentIdentifierType typeOfIdentifier)
		{
			Xsd.ShipmentIdentifierCollection result = new ShipmentIdentifierCollection();
			foreach (Xsd.ShipmentIdentifier identifier in this)
			{
				if (identifier.ShipmentIdentifierType == typeOfIdentifier)
				{
					result.Add(identifier);
				}
			}
			return result;
		}

		public Xsd.ShipmentIdentifier AddNew(Xsd.ShipmentIdentifierType identifierType, ZString value)
		{
			Xsd.ShipmentIdentifier result = this.AddNew();
			result.ShipmentIdentifierType = identifierType;
			result.Value = value;

			return result;
		}
	}
}
