using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BE.NCTS.Business.Messaging.MessageProviders.NCTS.Outgoing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(NCTSConsignmentProvider))]
	sealed class NCTSConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSConsignmentProvider>
	{
		public void TestCountryOfDispatch()
		{
			nctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Belgium;
			AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.CountryOfDispatch);
		}

		public void TestCountryOfDestination()
		{
			nctsHeader.MovementHeader.BM_RL_NKDestinationPort = "NL";
			AssertEquals("NL", Provider.CountryOfDestination);
		}

		public void TestContainerIndicator()
		{
			nctsHeader.DepartureHeaderContainers.AddNew();
			AssertEquals(true, Provider.ContainerIndicator);
		}

		public void TestInlandModeOfTransport()
		{
			nctsHeader.MovementHeader.BM_InlandTransportMode = "1";
			AssertEquals(1, Provider.InlandModeOfTransport);
		}

		public void TestModeOfTransportAtTheBorder()
		{
			nctsHeader.MovementHeader.BM_ExportTransportMode = "2";
			AssertEquals(2, Provider.ModeOfTransportAtTheBorder);
		}

		public void TestGrossMass()
		{
			CombineAssertions(() =>
			{
				var movementHeader = nctsHeader.MovementHeader;
				movementHeader.BM_GrossWeight = 10.0000m;
				AssertEquals("BY_GrossWeight = 10.0000", "10", new NCTSConsignmentProvider(nctsHeader).GrossMass.ToString());
				movementHeader.BM_GrossWeight = 10.2000m;
				AssertEquals("BY_GrossWeight = 10.2000", "10.2", new NCTSConsignmentProvider(nctsHeader).GrossMass.ToString());
				movementHeader.BM_GrossWeight = 10.2750m;
				AssertEquals("BY_GrossWeight = 10.2750", "10.275", new NCTSConsignmentProvider(nctsHeader).GrossMass.ToString());
			});
		}

		public void TestReferenceNumberUCR()
		{
			nctsHeader.MovementHeader.BM_UniqueConsignmentReference = "text";
			AssertEquals("text", Provider.ReferenceNumberUCR);
		}

		public void TestTransportEquipments()
		{
			AssertNotNull(Provider.TransportEquipments);
		}

		public void TestLocationOfGoods()
		{
			AssertNotNull(Provider.LocationOfGoods);
		}

		public void TestDepartureTransportMeans()
		{
			AssertNotNull(Provider.DepartureTransportMeans);
		}

		public void TestCountryOfRoutingOfConsignments()
		{
			nctsHeader.CountriesOfRouting.AddNew().CY_Data = Core.Constants.CountryCodes.Germany;
			nctsHeader.CountriesOfRouting.AddNew().CY_Data = Core.Constants.CountryCodes.Australia;
			AssertContainsExactElementsInAnyOrder(new[] { (1, "DE"), (2, "AU") }, Provider.CountryOfRoutingOfConsignments.Select(x => (x.SequenceNumber, x.Country)));
		}

		public void TestActiveBorderTransportMeans()
		{
			nctsHeader.MovementHeader.BM_CustomsOfficeAtBorder = ZString.Empty;
			nctsHeader.MovementHeader.BM_ActiveBorderIdentificationType = ZString.Empty;
			nctsHeader.MovementHeader.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			nctsHeader.MovementHeader.BM_ConveyanceNumber = ZString.Empty;
			var provider1 = new NCTSConsignmentProvider(nctsHeader);
			CombineAssertions("Mandatory fields are empty", () =>
			{
				AssertType<ActiveBorderTransportMeansCollectionProvider>(provider1.ActiveBorderTransportMeans);
				AssertEquals("Count", 0, provider1.ActiveBorderTransportMeans.Count);
			});

			nctsHeader.MovementHeader.BM_ActiveBorderIdentificationType = "A";
			nctsHeader.MovementHeader.BM_TOLCarrierID = "ABC123";
			nctsHeader.MovementHeader.BM_RN_NKTOLCarrierNationality = "ES";
			var provider2 = new NCTSConsignmentProvider(nctsHeader);
			CombineAssertions("Mandatory fields are filled", () =>
			{
				AssertEquals("Count", 1, provider2.ActiveBorderTransportMeans.Count);
				Assert("Type should be ActiveBorderTransportMeansProvider", provider2.ActiveBorderTransportMeans.All(tm => tm is ActiveBorderTransportMeansProvider));
			});
		}

		public void TestPlaceOfLoading()
		{
			AssertNotNull(Provider.PlaceOfLoading);
		}

		public void TestPlaceOfUnloading()
		{
			AssertNotNull(Provider.PlaceOfUnloading);
		}

		public void TestCarrier()
		{
			AssertNull(Provider.Carrier);
			Factory.CreateJobDocAddress("CAR", "CarrierName", parent: nctsHeader.MovementHeader);
			var provider = new NCTSConsignmentProvider(nctsHeader);
			AssertEquals("CarrierName", provider.Carrier.Name);
		}

		public void TestConsignor()
		{
			AssertNull(Provider.Consignor);
			Factory.CreateJobDocAddress("CRD", "ConsignorName", parent: nctsHeader);
			var provider = new NCTSConsignmentProvider(nctsHeader);
			AssertEquals("ConsignorName", provider.Consignor.Name);
		}

		public void TestConsignee()
		{
			CombineAssertions("Consignee", () =>
			{
				var consignee = Provider.Consignee;
				AssertEquals("Name", "", consignee.Name);
				AssertNull("IdentificationNumber", consignee.IdentificationNumber);
				AssertEquals("Address.StreetAndNumber", "", consignee.Address?.StreetAndNumber);
				AssertEquals("Address.Country", "", consignee.Address?.Country);
				AssertEquals("Address.City", "", consignee.Address?.City);
				AssertEquals("Address.Postcode", "", consignee.Address?.Postcode);
				AssertNull("ContactPerson", consignee.ContactPerson);
			});

			var consignee = Factory.CreateJobDocAddress("CEA", "ConsigneeName", "ContactName", "ContactPhone", "ContactEmail", parent: nctsHeader);
			nctsHeader.Consignee.E2_OA_Address = consignee.E2_OA_Address;
			var provider = new NCTSConsignmentProvider(nctsHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Consignee.Name", "ConsigneeName", provider.Consignee.Name);
				AssertNull("ConsigneeName", provider.Consignee.ContactPerson);
			});
		}

		public void TestAdditionalSupplyChainActors()
		{
			Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "SCA", "AddSupID", nctsHeader);
			Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "SCA", "AddSupID", nctsHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, Provider.AdditionalSupplyChainActors.Count);
				var first = Provider.AdditionalSupplyChainActors.First();
				AssertEquals("1st IdentificationNumber", "AddSupID", first.IdentificationNumber);
				AssertEquals("1st Role", "FR1", first.Role);
				AssertEquals("1st SequenceNumber", 1, first.SequenceNumber);
				AssertEquals("2nd SequenceNumber", 2, Provider.AdditionalSupplyChainActors.ElementAt(1).SequenceNumber);
			});
		}

		public void TestPreviousDocuments()
		{
			Factory.CreateCusSupportingInfo("PRE", null, "PrevDocRefNum", "PrevDocCOI", "C15", nctsHeader);
			Factory.CreateCusSupportingInfo("PRE", null, "PrevDocRefNum", "PrevDocCOI", "C15", nctsHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, Provider.PreviousDocuments.Count);
				var first = Provider.PreviousDocuments.First();
				AssertEquals("1st ComplementOfInformation", "PrevDocCOI", first.ComplementOfInformation);
				AssertEquals("1st ReferenceNumber", "PrevDocRefNum", first.ReferenceNumber);
				AssertEquals("1st Type", "C15", first.Type);
			});
		}

		public void TestSupportingDocuments()
		{
			Factory.CreateCusSupportingInfo("SUP", null, "SupDocRefNum", "SupDocCOI", "C15", nctsHeader.MovementHeader);
			Factory.CreateCusSupportingInfo("SUP", null, "SupDocRefNum", "SupDocCOI", "C15", nctsHeader.MovementHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, Provider.SupportingDocuments.Count);
				var first = Provider.SupportingDocuments.First();
				AssertEquals("1st ComplementOfInformation", "SupDocCOI", first.ComplementOfInformation);
				AssertEquals("1st ReferenceNumber", "SupDocRefNum", first.ReferenceNumber);
				AssertEquals("1st Type", "C15", first.Type);
			});
		}

		public void TestTransportDocuments()
		{
			Factory.CreateCusSupportingInfo("OTH", "TRA", "TranDocRefNum", null, "C14", nctsHeader);
			Factory.CreateCusSupportingInfo("OTH", "TRA", "TranDocRefNum", null, "C14", nctsHeader);
			Factory.CreateCusSupportingInfo("OTH", "REF", "AddRefRefNum", null, "C13", nctsHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, Provider.TransportDocuments.Count);
				var first = Provider.TransportDocuments.First();
				AssertEquals("1st ReferenceNumber", "TranDocRefNum", first.ReferenceNumber);
				AssertEquals("1st Type", "C14", first.Type);
			});
		}

		public void TestAdditionalReferences()
		{
			Factory.CreateCusSupportingInfo("OTH", "REF", "AddRefRefNum", null, "C13", nctsHeader);
			Factory.CreateCusSupportingInfo("OTH", "REF", "AddRefRefNum", null, "C13", nctsHeader);
			Factory.CreateCusSupportingInfo("OTH", "TRA", "TranDocRefNum", null, "C14", nctsHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, Provider.AdditionalReferences.Count);
				var first = Provider.AdditionalReferences.First();
				AssertEquals("1st ReferenceNumber", "AddRefRefNum", first.ReferenceNumber);
				AssertEquals("1st Type", "C13", first.Type);
			});
		}

		public void TestAdditionalInformation()
		{
			Factory.CreateCusSupportingInfo("OTH", "INF", "AddInfoRefNum", null, "C13", nctsHeader);
			Factory.CreateCusSupportingInfo("OTH", "INF", "AddInfoRefNum", null, "C13", nctsHeader);

			AssertEquals("Count", 2, Provider.AdditionalInformation.Count);
		}

		public void TestHouseConsignments()
		{
			var bill1 = Factory.New<NctsBill>();
			bill1.B0_BH = nctsHeader.PK;
			var bill2 = Factory.New<NctsBill>();
			bill2.B0_BH = nctsHeader.PK;
			nctsHeader.Bills.AddNew();
			nctsHeader.Bills.AddNew();

			AssertEquals(4, Provider.HouseConsignments.Count);
		}

		public void TestTransportChargesMethodOfPayment()
		{
			AssertNullOrEmpty(Provider.TransportChargesMethodOfPayment);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			provider = new NCTSConsignmentProvider(nctsHeader);
		}

		NctsHeader nctsHeader;
		NCTSConsignmentProvider provider;

		protected override NCTSConsignmentProvider GetProvider() => provider;
	}
}
