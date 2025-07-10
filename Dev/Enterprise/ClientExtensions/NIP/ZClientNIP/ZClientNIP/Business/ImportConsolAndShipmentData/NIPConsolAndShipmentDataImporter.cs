using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.NIP.Business.ConsolAndShipmentImport
{
	public class NIPConsolAndShipmentDataImporter : FlatFileDataImporter
	{
		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new NIPConsolAndShipmentDataConverter(notifications, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.Consol();
		}

		static Xsd.ContainerCollection Containers;
		protected override bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
		{
			var consolValue = (Xsd.Consol)xsd;
			var interchange = new Xsd.XmlInterchange();
			var buffer = new NotificationBuffer(notifications);
			var importContext = new ValueObjectImportContext(FactoryProvider.Current, interchange, buffer);

			var decCollection = new Xsd.ShipmentCollection();
			var shipmentCollection = new Xsd.ShipmentCollection();
			foreach (Xsd.Shipment xsdShipment in consolValue.Shipments)
			{
				var cusValue = (from value in xsdShipment.CustomValues.Cast<Xsd.CustomValue>()
								where value.Type == "IsCustomEntryOnly"
								select value).First();
				if (cusValue.Value == "Y")
				{
					decCollection.Add(xsdShipment);
				}
				else
				{
					shipmentCollection.Add(xsdShipment);
				}
			}

			Containers = consolValue.ConsolDetail.Containers;
			foreach (Xsd.Shipment entry in decCollection)
			{
				var containers = (from container in Containers.Cast<Xsd.Container>()
								  let packLines = entry.ShipmentDetails.Packages
								  from packLine in packLines.Cast<Xsd.Package>()
								  where packLine.ContainerNumber == container.ContainerNumber
								  select container);
				var xsdConsolAndShipment = new Xsd.ConsolAndShipment();
				xsdConsolAndShipment.Consol = consolValue;
				xsdConsolAndShipment.Consol.ConsolDetail.Containers = new Xsd.ContainerCollection();
				foreach (var container in containers)
				{
					xsdConsolAndShipment.Consol.ConsolDetail.Containers.Add(container);
				}

				xsdConsolAndShipment.Shipment = entry;
				DeclarationAdapter.CreateOrUpdateFromValueObject(xsdConsolAndShipment, importContext);
			}

			consolValue.ConsolDetail.Containers = Containers;
			if (shipmentCollection.Count > 0)
			{
				var containers = (from container in Containers.Cast<Xsd.Container>()
								  let packLinesCollection = (from shipment in shipmentCollection.Cast<Xsd.Shipment>()
												   select shipment.ShipmentDetails.Packages)
								  from packLines in packLinesCollection.Cast<Xsd.PackageCollection>()
								  from packLine in packLines.Cast<Xsd.Package>()
								  where packLine.ContainerNumber == container.ContainerNumber
								  select container);
				consolValue.ConsolDetail.Containers = new Xsd.ContainerCollection();
				foreach (var container in containers)
				{
					consolValue.ConsolDetail.Containers.Add(container);
				}

				consolValue.Shipments = shipmentCollection;
				ConsolAdapter.CreateOrUpdateFromValueObject(consolValue, importContext);
			}

			if (!buffer.HasErrors)
			{
				FactoryProvider.SaveCurrentAndCreateNew();
				return true;
			}

			return false;
		}

		ForwardingConsolValueObjectDataAdapter ConsolAdapter
		{
			get { return consolAdapter ?? (consolAdapter = new ForwardingConsolValueObjectDataAdapter()); }
		}
		ForwardingConsolValueObjectDataAdapter consolAdapter;

		DeclarationValueObjectDataAdapter DeclarationAdapter
		{
			get { return declarationAdapter ?? (declarationAdapter = DeclarationValueObjectDataAdapter.New()); }
		}
		DeclarationValueObjectDataAdapter declarationAdapter;
	}
}
