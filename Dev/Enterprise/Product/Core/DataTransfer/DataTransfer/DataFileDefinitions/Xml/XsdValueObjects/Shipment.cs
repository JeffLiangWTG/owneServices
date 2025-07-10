using System.Collections;
using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoShipment")]
	public class Shipment : Xsd.AutoShipment
	{
		public Xsd.PackageCollection GetPackagesForContainer(string containerNumber)
		{
			Xsd.PackageCollection packages = new Xsd.PackageCollection();

			if (this.ShipmentDetails.Packages != null)
			{
				foreach (Xsd.Package package in this.ShipmentDetails.Packages)
				{
					if (package.ContainerNumber == containerNumber)
					{
						packages.Add(package);
					}
				}
			}
			return packages;
		}

		[XmlIgnore]
		public ZString Housebill
		{
			get
			{
				foreach (Xsd.ShipmentIdentifier identifier in this.ShipmentIdentifier)
				{
					if (identifier.ShipmentIdentifierType == Xsd.ShipmentIdentifierType.Housebill)
					{
						return identifier.Value;
					}
				}
				return ZString.Empty;
			}
		}

		public MasterAndHouseBill[] GetMasterAndHouseBillIdentifiers(Xsd.Consol consol)
		{
			ArrayList result = new ArrayList();
			foreach (Xsd.ShipmentIdentifier identifier in ShipmentIdentifier)
			{
				if (identifier.ShipmentIdentifierType == Xsd.ShipmentIdentifierType.Housebill)
				{
					Xsd.MasterAndHouseBill masterAndHouseBill = identifier.GetMasterAndHouseBill(consol);
					if (masterAndHouseBill != null)
					{
						result.Add(masterAndHouseBill);
					}
				}
			}
			return (MasterAndHouseBill[])result.ToArray(typeof(MasterAndHouseBill));
		}

		[XmlIgnore]
		public CustomValueCollection CustomValues
		{
			get
			{
				return ShipmentDetails.CustomValues;
			}
		}

		[XmlIgnore]
		public Xsd.UNLOCO OriginPort
		{
			get
			{
				return (ShipmentDetailsSpecified && ShipmentDetails.PortOfOriginSpecified && ShipmentDetails.PortOfOrigin.PortSpecified) ?
					ShipmentDetails.PortOfOrigin.Port : null;
			}
		}

		[XmlIgnore]
		public Xsd.UNLOCO DestinationPort
		{
			get
			{
				return (ShipmentDetailsSpecified && ShipmentDetails.PortofDestinationSpecified && ShipmentDetails.PortofDestination.PortSpecified) ?
				  ShipmentDetails.PortofDestination.Port : null;
			}
		}
	}
}
