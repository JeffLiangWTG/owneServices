using System.Collections.Generic;
using System.IO;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	class ShipmentXQueryPathsTest : TestCaseWithFactory
	{
		public void TestGetValueByXQueryPath_Version_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				TestExtractOutValueByXQueryPath();
			}
		}

		public void TestExtractOutValueByXQueryPath()
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.TransportMode = new CodeDescriptionPair() { Code = TransportModes.SeaAir };
			universalShipment.ShipmentType = new CodeDescriptionPair() { Code = ShipmentTypes.AssemblyMaster };
			universalShipment.PortOfOrigin = new UNLOCO { Code = "USLAX" };
			universalShipment.PortOfDestination = new UNLOCO { Code = "SWEND" };
			universalShipment.PortOfDischarge = new UNLOCO { Code = "USORD" };
			universalShipment.PortOfLoading = new UNLOCO { Code = "AUSYD" };
			universalShipment.AWBServiceLevel = new CodeDescriptionPair() { Code = "AWS" };
			universalShipment.ContainerMode = new ContainerMode() { Code = "LSE" };
			universalShipment.OuterPacksPackageType = new PackageType() { Code = "PLT" };
			universalShipment.ServiceLevel = new ServiceLevel() { Code = "STD" };
			universalShipment.AdditionalTerms = "dfgfdf";
			universalShipment.AgentsReference = "SDFDSFSD";
			universalShipment.BookingConfirmationReference = "asdf";
			universalShipment.CoLoadBookingConfirmationReference = "TR00001029";
			universalShipment.GoodsDescription = "aaaaa";
			universalShipment.IsCancelled = true;
			universalShipment.IsDirectBooking = true;
			universalShipment.IsForwardRegistered = true;
			universalShipment.ShipmentIncoTerm = new IncoTerm() { Code = "PPD" };
			universalShipment.ShippedOnBoard = new CodeDescriptionPair() { Code = "SHP" };
			universalShipment.WayBillNumber = "DFHJGNDFJKGNFD";
			universalShipment.WayBillType = new WayBillType() { Code = "HWB" };

			universalShipment.LocalProcessing = new LocalProcessing()
			{
				DeliveryRequiredBy = new CargoWise.Types.ZDateTime(1900, 1, 1),
				EstimatedDelivery = new CargoWise.Types.ZDateTime(1900, 1, 2),
				EstimatedPickup = new CargoWise.Types.ZDateTime(1900, 1, 3),
				FCLDeliveryEquipmentNeeded = new CodeDescriptionPair() { Code = "PSL" },
				FCLPickupEquipmentNeeded = new CodeDescriptionPair() { Code = "PSX" },
				InsuranceRequired = true,
				PickupRequiredBy = new CargoWise.Types.ZDateTime(1900, 1, 4),
			};

			universalShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() {
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "MockCR", AddressType = "ConsignorDocumentaryAddress" },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "MockCE", AddressType = "ConsigneeDocumentaryAddress" },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "MockPA", AddressType = "PickupAgent" },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "MockDA", AddressType = "DeliveryAgent" },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "MockCA", AddressType = "ControllingAgent" },

				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "MocCRP", AddressType = "ConsignorPickupDeliveryAddress" },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "MocCEP", AddressType = "ConsigneePickupDeliveryAddress" },

				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "ConCus", AddressType = "ControllingCustomer" },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "ExpBro", AddressType = "ExportBroker" },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "ImpBro", AddressType = "ImportBroker" },

				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "DCFSAd", AddressType = "DepartureCFSAddress" },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "PiLoCa", AddressType = "PickupLocalCartage" },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "ShCoPa", AddressType = "ShipmentControllingParty" },
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { OrganizationCode = "ShLiAd", AddressType = "ShippingLineAddress" },
			});

			using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				ObjectFactory.Get<IXmlWriter>().WriteXML(universalShipment, stream, SchemaVersionManager.Current.Namespace);

				using (var reader = new StreamReader(stream))
				{
					var result = reader.ReadToEnd().Replace($"xmlns=\"{SchemaVersionManager.Current.Namespace}\" ", string.Empty);
					var xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(result);

					AssertEquals(TransportModes.SeaAir, xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.TransportMode)).InnerText);
					AssertEquals(ShipmentTypes.AssemblyMaster, xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ShipmentType)).InnerText);
					AssertEquals("USLAX", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.PortOfOrigin)).InnerText);
					AssertEquals("SWEND", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.PortOfDestination)).InnerText);
					AssertEquals("USORD", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.PortOfDischarge)).InnerText);
					AssertEquals("AUSYD", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.PortOfLoading)).InnerText);
					AssertEquals("AWS", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.AWBServiceLevel)).InnerText);
					AssertEquals("LSE", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ContainerMode)).InnerText);
					AssertEquals("PLT", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.OuterPacksPackageType)).InnerText);
					AssertEquals("STD", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ServiceLevel)).InnerText);

					AssertEquals("dfgfdf", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.AdditionalTerms)).InnerText);
					AssertEquals("SDFDSFSD", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.AgentsReference)).InnerText);
					AssertEquals("asdf", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.BookingConfirmationReference)).InnerText);
					AssertEquals("TR00001029", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.CoLoadBookingConfirmationReference)).InnerText);
					AssertEquals("aaaaa", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.GoodsDescription)).InnerText);
					AssertEquals("true", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.IsCancelled)).InnerText);
					AssertEquals("true", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.IsDirectBooking)).InnerText);
					AssertEquals("true", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.IsForwardRegistered)).InnerText);
					AssertEquals("PPD", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ShipmentIncoTerm)).InnerText);
					AssertEquals("SHP", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ShippedOnBoard)).InnerText);
					AssertEquals("DFHJGNDFJKGNFD", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.WayBillNumber)).InnerText);
					AssertEquals("HWB", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.WayBillType)).InnerText);

					AssertEquals("MockCR", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ConsignorDocumentaryAddress)).InnerText);
					AssertEquals("MockCE", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ConsigneeDocumentaryAddress)).InnerText);
					AssertEquals("MockPA", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.PickupAgent)).InnerText);
					AssertEquals("MockDA", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.DeliveryAgent)).InnerText);
					AssertEquals("MockCA", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ControllingAgent)).InnerText);

					AssertEquals("MocCRP", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ConsignorPickupDeliveryAddress)).InnerText);
					AssertEquals("MocCEP", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ConsigneePickupDeliveryAddress)).InnerText);

					AssertEquals("ConCus", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ControllingCustomer)).InnerText);
					AssertEquals("ExpBro", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ExportBroker)).InnerText);
					AssertEquals("ImpBro", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ImportBroker)).InnerText);

					AssertEquals("DCFSAd", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.DepartureCFSAddress)).InnerText);
					AssertEquals("PiLoCa", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.PickupLocalCartage)).InnerText);
					AssertEquals("ShCoPa", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ShipmentControllingParty)).InnerText);
					AssertEquals("ShLiAd", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.ShippingLineAddress)).InnerText);

					AssertEquals("1900-01-01T00:00:00", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.DeliveryRequiredBy)).InnerText);
					AssertEquals("1900-01-02T00:00:00", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.EstimatedDelivery)).InnerText);
					AssertEquals("1900-01-03T00:00:00", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.EstimatedPickup)).InnerText);
					AssertEquals("PSL", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.FCLDeliveryEquipmentNeeded)).InnerText);
					AssertEquals("PSX", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.FCLPickupEquipmentNeeded)).InnerText);
					AssertEquals("true", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.InsuranceRequired)).InnerText);
					AssertEquals("1900-01-04T00:00:00", xmlDocument.SelectSingleNode(GetXQueryPath(ShipmentXQueryPaths.PickupRequiredBy)).InnerText);
				}
			}
		}

		string GetXQueryPath(string path)
		{
			return ShipmentXQueryPaths.GetNamespace(SchemaVersionManager.Current.Namespace, path);
		}
	}
}
