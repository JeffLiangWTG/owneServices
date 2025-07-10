using System.Xml.Serialization;
using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoConsol")]
	public class Consol : Xsd.AutoConsol
	{
		[XmlIgnore]
		public ZString Masterbill
		{
			get
			{
				ZString result = "";
				bool foundOne = false;
				foreach (Xsd.ConsolIdentifier identifier in ConsolIdentifier)
				{
					if (identifier.ConsolIdentifierType == Xsd.ConsolIdentifierType.MasterWaybill)
					{
						if (foundOne)
						{
							result = ZString.Empty;
							break;
						}
						foundOne = true;
						result = identifier.Value;
					}
				}
				return result;
			}
		}

		public Xsd.ShipmentCollection GetShipmentsForContainer(string containerNumber)
		{
			Xsd.ShipmentCollection shipmentsForContainer = new Xsd.ShipmentCollection();

			if (Shipments != null)
			{
				foreach (Xsd.Shipment shipment in Shipments)
				{
					bool containerFoundInShipment = false;
					if (shipment.ShipmentDetails.Packages != null)
					{
						foreach (Xsd.Package package in shipment.ShipmentDetails.Packages)
						{
							if (package.ContainerNumber == containerNumber)
							{
								containerFoundInShipment = true;
								break;
							}
						}
					}

					if (containerFoundInShipment)
					{
						shipmentsForContainer.Add(shipment);
					}
				}
			}

			return shipmentsForContainer;
		}

		public Xsd.PackageCollection GetPackagesForContainer(string containerNumber)
		{
			Xsd.PackageCollection packagesForContainer = new Xsd.PackageCollection();

			if (Shipments != null)
			{
				foreach (Xsd.Shipment shipment in Shipments)
				{
					Xsd.PackageCollection packages = shipment.GetPackagesForContainer(containerNumber);
					packagesForContainer.Add(packages);
				}
			}

			return packagesForContainer;
		}

		[XmlIgnore]
		public CustomValueCollection CustomValues
		{
			get
			{
				return this.ConsolDetail.CustomValues;
			}
		}

		[XmlIgnore]
		public UNLOCO LoadPort
		{
			get
			{
				return (ConsolDetailSpecified && ConsolDetail.PortOfLoadingSpecified && ConsolDetail.PortOfLoading.PortSpecified) ?
					ConsolDetail.PortOfLoading.Port : null;
			}
		}

		[XmlIgnore]
		public UNLOCO DischargePort
		{
			get
			{
				return (ConsolDetailSpecified && ConsolDetail.PortOfDischargeSpecified && ConsolDetail.PortOfDischarge.PortSpecified) ?
					ConsolDetail.PortOfDischarge.Port : null;
			}
		}
	}
}
