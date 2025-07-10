using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.NIP.Business.ConsolAndShipmentImport.Testing
{
	sealed class NIPConsolAndShipmentDataConverterTest : TestCaseWithFactory
	{
		public void TestMapping()
		{
			var consolValue = new Xsd.Consol();

			using (var reader = new StreamReader(PathToFile))
			{
				Converter.ImportFlatFile(consolValue, new CsvFlatFileFormat(), reader);
			}

			AssertEquals(Xsd.ConsolTransportMode.SEA, consolValue.ConsolDetail.TransportMode);
			AssertEquals("SYDYOK000055", consolValue.Masterbill);
			AssertEquals(Xsd.ContainerMode.FCL, consolValue.ConsolDetail.ContainerMode);
			AssertEquals("CSHK", consolValue.ConsolDetail.Carrier.OwnerCode);
			AssertEquals("KAGA", ((Xsd.SailingWithVesselVoyage)consolValue.ConsolDetail.Item).VesselName);
			AssertEquals("8616506", ((Xsd.SailingWithVesselVoyage)consolValue.ConsolDetail.Item).LloydsNo);
			AssertEquals("46N", ((Xsd.SailingWithVesselVoyage)consolValue.ConsolDetail.Item).VoyageNo);

			AssertEquals("JPYOK", consolValue.ConsolDetail.PortOfLoading.Port.Value);
			AssertEquals("AUSYD", consolValue.ConsolDetail.PortOfDischarge.Port.Value);

			AssertEquals(3, consolValue.Shipments.Count);

			var shipmentValue = consolValue.Shipments[0];
			AssertEquals(Xsd.TransportMode.SEA, shipmentValue.ShipmentDetails.TransportMode);
			AssertEquals(Xsd.ContainerMode.LCL, shipmentValue.ShipmentDetails.PackingMode);
			AssertEquals("ShipRef", shipmentValue.ShipmentDetails.BookingReference);
			AssertEquals(1, shipmentValue.ShipmentDetails.CustomsEntryNumbers.Count);
			AssertEquals(Core.Constants.CountryCodes.Australia, shipmentValue.ShipmentDetails.CustomsEntryNumbers[0].Country);
			AssertEquals("XLV", shipmentValue.ShipmentDetails.CustomsEntryNumbers[0].Type);
			AssertEquals(Core.Constants.PkgUnit.Package, shipmentValue.ShipmentDetails.TotalOuterPacksQty.DimensionType);
			AssertEquals("SYYOSRE07580", shipmentValue.Housebill);
			AssertEquals("MS. AKIKO IWAI_JP", shipmentValue.ShipmentDetails.Consignee.OwnerCode);
			AssertEquals("MS. AKIKO IWAI_JP", shipmentValue.ShipmentDetails.Consignor.OwnerCode);
			AssertEquals(1, shipmentValue.CustomValues.Count);
			AssertEquals("IsCustomEntryOnly", shipmentValue.CustomValues[0].Type);
			AssertEquals("Y", shipmentValue.CustomValues[0].Value);

			shipmentValue = consolValue.Shipments[1];
			AssertEquals(Xsd.TransportMode.SEA, shipmentValue.ShipmentDetails.TransportMode);
			AssertEquals(Xsd.ContainerMode.LCL, shipmentValue.ShipmentDetails.PackingMode);
			AssertEquals("BLAH", shipmentValue.ShipmentDetails.BookingReference);
			AssertEquals(1, shipmentValue.ShipmentDetails.CustomsEntryNumbers.Count);
			AssertEquals(Core.Constants.CountryCodes.Australia, shipmentValue.ShipmentDetails.CustomsEntryNumbers[0].Country);
			AssertEquals("XLV", shipmentValue.ShipmentDetails.CustomsEntryNumbers[0].Type);
			AssertEquals(Core.Constants.PkgUnit.Package, shipmentValue.ShipmentDetails.TotalOuterPacksQty.DimensionType);
			AssertEquals("MR. AKIRA SAKAI_JP", shipmentValue.ShipmentDetails.Consignee.OwnerCode);
			AssertEquals("MR. AKIRA SAKAI_JP", shipmentValue.ShipmentDetails.Consignor.OwnerCode);
			AssertEquals(1, shipmentValue.CustomValues.Count);
			AssertEquals("IsCustomEntryOnly", shipmentValue.CustomValues[0].Type);
			AssertEquals("Y", shipmentValue.CustomValues[0].Value);

			shipmentValue = consolValue.Shipments[2];
			AssertEquals(Xsd.TransportMode.SEA, shipmentValue.ShipmentDetails.TransportMode);
			AssertEquals(Xsd.ContainerMode.LCL, shipmentValue.ShipmentDetails.PackingMode);
			AssertEquals("Ship  Ref", shipmentValue.ShipmentDetails.BookingReference);
			AssertEquals(0, shipmentValue.ShipmentDetails.CustomsEntryNumbers.Count);
			AssertEquals(Core.Constants.PkgUnit.Package, shipmentValue.ShipmentDetails.TotalOuterPacksQty.DimensionType);
			AssertEquals("MR. SHIGERU SUMIYA_JP", shipmentValue.ShipmentDetails.Consignee.OwnerCode);
			AssertEquals("MR. SHIGERU SUMIYA_JP", shipmentValue.ShipmentDetails.Consignor.OwnerCode);
			AssertEquals(1, shipmentValue.CustomValues.Count);
			AssertEquals("IsCustomEntryOnly", shipmentValue.CustomValues[0].Type);
			AssertEquals("N", shipmentValue.CustomValues[0].Value);

			AssertEquals(4, consolValue.ConsolDetail.Containers.Count);
			AssertEquals(Xsd.ContainerMode.FCL, consolValue.ConsolDetail.Containers[0].PackingMode);
			AssertEquals("4020FR", consolValue.ConsolDetail.Containers[0].ContainerType.ContainerCode);
			AssertEquals(Xsd.ContainerMode.FCL, consolValue.ConsolDetail.Containers[1].PackingMode);
			AssertEquals("4030GP", consolValue.ConsolDetail.Containers[1].ContainerType.ContainerCode);
			AssertEquals(Xsd.ContainerMode.FCL, consolValue.ConsolDetail.Containers[2].PackingMode);
			AssertEquals("4040FR", consolValue.ConsolDetail.Containers[2].ContainerType.ContainerCode);
			AssertEquals(Xsd.ContainerMode.FCL, consolValue.ConsolDetail.Containers[3].PackingMode);
			AssertEquals("4050GP", consolValue.ConsolDetail.Containers[3].ContainerType.ContainerCode);
		}

		EmbeddedResourceRetriever resourceRetriever;

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}

		NIPConsolAndShipmentDataConverter converter;
		NIPConsolAndShipmentDataConverter Converter => converter ?? (converter = new NIPConsolAndShipmentDataConverter(new NotificationBuffer(), Factory));

		string PathToFile => resourceRetriever.SaveResourceToFile("ImportConsolAndShipmentData.TestFiles.TestFile.csv");
	}
}
