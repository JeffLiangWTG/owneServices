using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GB.Business.Testing;
namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class NCTSConsignmentProviderTest : DataProviderTestCase<NCTSConsignmentProvider>
	{
		public void TestCountryOfDispatch()
		{
			Header.MovementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals(Core.Constants.CountryCodes.UnitedKingdom, Provider.CountryOfDispatch);
		}

		public void TestCountryOfDestination()
		{
			Header.MovementHeader.BM_RL_NKDestinationPort = "NL";
			AssertEquals("NL", Provider.CountryOfDestination);
		}

		public void TestContainerIndicator()
		{
			Header.DepartureHeaderContainers.AddNew();
			AssertEquals(true, Provider.ContainerIndicator);
		}

		public void TestInlandModeOfTransport()
		{
			Header.MovementHeader.BM_InlandTransportMode = "1";
			AssertEquals("1", Provider.InlandModeOfTransport);
		}

		public void TestModeOfTransportAtTheBorder()
		{
			Header.MovementHeader.BM_ExportTransportMode = "2";
			AssertEquals("2", Provider.ModeOfTransportAtTheBorder);
		}

		public void TestGrossMass()
		{
			Header.MovementHeader.BM_GrossWeight = 10.0m;
			AssertEquals(10.0m, Provider.GrossMass);
			Header.MovementHeader.BM_GrossWeight = 12.25m;
			AssertEquals(12.25m, Provider.GrossMass);
		}

		public void TestGrossMass_InTransitionPeriod()
		{
			Header.MovementHeader.BM_GrossWeight = 12345678.123456789m;
			Header.MovementHeader.BM_GrossWeightUQ = "KG";

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals("GrossMass is rounded to 6 digits", 12345678.123457m, GetProvider().GrossMass);
			});

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals("GrossMass is rounded to 3 digits", 12345678.123m, GetProvider().GrossMass);
			});
		}

		public void TestReferenceNumberUCR()
		{
			Header.MovementHeader.BM_UniqueConsignmentReference = "text";
			AssertEquals("text", Provider.ReferenceNumberUCR);
		}

		public void TestTransportEquipments()
		{
			AssertNotNull(Provider.TransportEquipments);
		}

		public void TestLocationOfGoods()
		{
			var location = Header.MovementHeader.GoodsLocation;
			var provider = GetProvider();
			location.CGL_Qualifier = ZString.Empty;
			AssertNull(provider.LocationOfGoods);

			provider = GetProvider();
			location.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
			location.CGL_AdditionalIdentifier = "GB0001";
			location.Address.E2_Contact = "Name";
			AssertEquals("GB0001", provider.LocationOfGoods.UNLocode);
			AssertNotNull("Name", provider.LocationOfGoods.ContactPerson.Name);
		}

		public void TestDepartureTransportMeans()
		{
			AssertNotNull(Provider.DepartureTransportMeans);
			for (var test = 0; test < 16; ++test)
			{
				var countTransportAtDeparture = (test & 1) == 1 ? 1 : 0;
				var countTransportAtDepartureTrailer1RegNo = (test & 2) == 2 ? 1 : 0;
				var countTransportAtDepartureTrailer2RegNo = (test & 4) == 4 ? 1 : 0;
				var countAircraftIDAtDeparture = (test & 8) == 8 ? 1 : 0;

				Header.MovementHeader.BM_TransportAtDeparture = countTransportAtDeparture == 1 ? "X" : ZString.Empty;
				Header.MovementHeader.BM_TransportAtDepartureTrailer1RegNo = countTransportAtDepartureTrailer1RegNo == 1 ? "1" : ZString.Empty;
				Header.MovementHeader.BM_TransportAtDepartureTrailer2RegNo = countTransportAtDepartureTrailer2RegNo == 1 ? "2" : ZString.Empty;
				Header.MovementHeader.BM_AircraftIDAtDeparture = countAircraftIDAtDeparture == 1 ? "A1" : ZString.Empty;

				var providers = GetProvider().DepartureTransportMeans;
				var expectedCount = countTransportAtDeparture + countTransportAtDepartureTrailer1RegNo + countTransportAtDepartureTrailer2RegNo + countAircraftIDAtDeparture;
				AssertEquals($"Case {test} Provider.DepartureTransportMeans.Count", expectedCount, providers.Count);
				AssertEquals($"Case {test} Provider.DepartureTransportMeans.OfType<DepartureTransportMeansTransportAtDepartureProvider>().Count()", countTransportAtDeparture, providers.OfType<DepartureTransportMeansTransportAtDepartureProvider>().Count());
				AssertEquals($"Case {test} Provider.DepartureTransportMeans.OfType<DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider>().Count()", countTransportAtDepartureTrailer1RegNo, providers.OfType<DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider>().Count());
				AssertEquals($"Case {test} Provider.DepartureTransportMeans.OfType<DepartureTransportMeansTransportAtDepartureTrailer2RegNoProvider>().Count()", countTransportAtDepartureTrailer2RegNo, providers.OfType<DepartureTransportMeansTransportAtDepartureTrailer2RegNoProvider>().Count());
				AssertEquals($"Case {test} Provider.DepartureTransportMeans.OfType<DepartureTransportMeansAircraftIDAtDepartureProvider>().Count()", countAircraftIDAtDeparture, providers.OfType<DepartureTransportMeansAircraftIDAtDepartureProvider>().Count());

				var expectedSequenceNumber = 0;
				foreach (var provider in providers)
				{
					AssertEquals($"Case {test} Provider.DepartureTransportMeans[{expectedSequenceNumber}].SequenceNumber", ++expectedSequenceNumber, provider.SequenceNumber);
				}
			}
		}

		public void TestCountryOfRoutingOfConsignments()
		{
			Header.CountriesOfRouting.AddNew().CY_Data = Core.Constants.CountryCodes.Finland;
			Header.CountriesOfRouting.AddNew().CY_Data = Core.Constants.CountryCodes.Sweden;
			Header.CountriesOfRouting.AddNew().CY_Data = Core.Constants.CountryCodes.Norway;
			AssertContainsExactElementsInAnyOrder(new[] { (1, "FI"), (2, "SE"), (3, "NO") }, Provider.CountryOfRoutingOfConsignments.Select(x => (x.SequenceNumber, x.Country)));
		}

		public void TestActiveBorderTransportMeans()
		{
			Header.MovementHeader.BM_CustomsOfficeAtBorder = ZString.Empty;
			Header.MovementHeader.BM_ActiveBorderIdentificationType = ZString.Empty;
			Header.MovementHeader.BM_RN_NKTOLCarrierNationality = ZString.Empty;
			Header.MovementHeader.BM_ConveyanceNumber = ZString.Empty;
			AssertEquals(0, Provider.ActiveBorderTransportMeans.Count);
			AssertType<ActiveBorderTransportMeansCollectionProvider>(Provider.ActiveBorderTransportMeans);

			Header.MovementHeader.BM_ActiveBorderIdentificationType = "X";
			Header.MovementHeader.BM_TOLCarrierID = "ABC123";
			Header.MovementHeader.BM_RN_NKTOLCarrierNationality = "SE";
			var providers = GetProvider().ActiveBorderTransportMeans;
			AssertEquals(1, providers.Count);
			AssertType<ActiveBorderTransportMeansProvider>(providers.First());
		}

		public void TestPlaceOfLoading()
		{
			AssertNull(Provider.PlaceOfLoading);
			Header.MovementHeader.BM_PortOfPresentationCode = "";
			AssertNull(Provider.PlaceOfLoading);
			Header.MovementHeader.BM_PortOfPresentationCode = "GBDOV";
			AssertNotNull(Provider.PlaceOfLoading);
			AssertEquals("GBDOV", Provider.PlaceOfLoading.UnLocode);
		}

		public void TestPlaceOfUnloading()
		{
			AssertNull(Provider.PlaceOfUnloading);
			Header.MovementHeader.BM_ForeignDestPortKCode = "";
			AssertNull(Provider.PlaceOfUnloading);
			Header.MovementHeader.BM_ForeignDestPortKCode = "GBNRW";
			AssertNotNull(Provider.PlaceOfUnloading);
			AssertEquals("GBNRW", Provider.PlaceOfUnloading.UnLocode);
		}

		public void TestCarrier()
		{
			AssertNull(Provider.Carrier);
			Factory.CreateJobDocAddress("CAR", "CarrierName", parent: Header.MovementHeader);
			var provider = new NCTSConsignmentProvider(Header);
			AssertEquals("CarrierName", provider.Carrier.Name);
		}

		public void TestConsignor()
		{
			AssertNull(Provider.Consignor);
			var address = Factory.CreateOrgAddress("Address1", "Address2", "Postcode", "City", Core.Constants.CountryCodes.UnitedKingdom);
			Factory.CreateJobDocAddress("CRD", "ConsignorName", orgAddress: address, parent: Header);
			var provider = new NCTSConsignmentProvider(Header);

			CombineAssertions(() =>
			{
				AssertEquals("Address.StreetAndNumber", "Address1 Address2", provider.Consignor.Address.StreetAndNumber);
				AssertEquals("Address.Postcode", "Postcode", provider.Consignor.Address.Postcode);
				AssertEquals("Address.City", "City", provider.Consignor.Address.City);
				AssertEquals("Address.Country", "GB", provider.Consignor.Address.Country);
				AssertEquals("Consignor.Name", "ConsignorName", provider.Consignor.Name);
			});
		}

		public void TestConsignorContactPersonExcluded()
		{
			var address = Factory.CreateOrgAddress("Address1", "Address2", "Postcode", "City", Core.Constants.CountryCodes.UnitedKingdom);
			_ = Factory.CreateJobDocAddress("CRD", "ConsignorName", "ContactName", "ContactPhone", "ContactEmail", orgAddress: address, parent: Header);
			var provider = new NCTSConsignmentProvider(Header);
			AssertNull(provider.Consignor.ContactPerson);
		}

		public void TestConsignorReturnsNullIfEmpty()
		{
			AssertNull(Provider.Consignor);
			var address = Factory.CreateOrgAddress("", "", "", "", "");
			var jda = Factory.CreateJobDocAddress("CRD", "", "", "", "", orgAddress: address, parent: Header);
			var provider = new NCTSConsignmentProvider(Header);
			AssertNull(provider.Consignor);
			address.Address1 = "Street";
			provider = new NCTSConsignmentProvider(Header);
			AssertNotNull(provider.Consignor);
		}

		public void TestConsignee()
		{
			AssertNull(Provider.Consignee);

			var address = Factory.CreateOrgAddress("Address1", "Address2", "Postcode", "City", Core.Constants.CountryCodes.UnitedKingdom);
			var jda = Factory.CreateJobDocAddress("CEA", "ConsigneeName", "ContactName", "ContactPhone", "ContactEmail", orgAddress: address, parent: Header);
			Header.Consignee.E2_OA_Address = address.PK;
			var provider = new NCTSConsignmentProvider(Header);

			CombineAssertions(() =>
			{
				AssertEquals("Address.StreetAndNumber", "Address1 Address2", provider.Consignee.Address.StreetAndNumber);
				AssertEquals("Address.Postcode", "Postcode", provider.Consignee.Address.Postcode);
				AssertEquals("Address.City", "City", provider.Consignee.Address.City);
				AssertEquals("Address.Country", "GB", provider.Consignee.Address.Country);
				AssertEquals("Consignee.Name", "ConsigneeName", provider.Consignee.Name);
				AssertNull("ConsigneeName", provider.Consignee.ContactPerson);
			});
		}

		public void TestConsigneeContactPersonExcluded()
		{
			var address = Factory.CreateOrgAddress("Address1", "Address2", "Postcode", "City", Core.Constants.CountryCodes.UnitedKingdom);
			_ = Factory.CreateJobDocAddress("CEA", "ConsigneeName", "ContactName", "ContactPhone", "ContactEmail", orgAddress: address, parent: Header);
			Header.Consignee.E2_OA_Address = address.PK;
			var provider = new NCTSConsignmentProvider(Header);
			AssertNull(provider.Consignee.ContactPerson);
		}

		public void TestConsigneeReturnsNullIfEmpty()
		{
			AssertNull(Provider.Consignee);
			var address = Factory.CreateOrgAddress("", "", "", "", "");
			var jda = Factory.CreateJobDocAddress("CEA", "", "", "", "", orgAddress: address, parent: Header);
			Header.Consignee.E2_OA_Address = address.PK;
			var provider = new NCTSConsignmentProvider(Header);
			AssertNull(provider.Consignee);
			address.Address1 = "Street";
			provider = new NCTSConsignmentProvider(Header);
			AssertNotNull(provider.Consignee);
		}

		public void TestAdditionalSupplyChainActors()
		{
			Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "SCA", "AddSupID", Header);
			Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "SCA", "AddSupID", Header);

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
			Factory.CreateCusSupportingInfo("PRE", null, "PrevDocRefNum", "PrevDocCOI", "C15", Header);
			Factory.CreateCusSupportingInfo("PRE", null, "PrevDocRefNum", "PrevDocCOI", "C15", Header);

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: Count", 0, provider.PreviousDocuments.Count);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 2, provider.PreviousDocuments.Count);
				var first = provider.PreviousDocuments.First();
				AssertEquals("1st ComplementOfInformation", "PrevDocCOI", first.ComplementOfInformation);
				AssertEquals("1st ReferenceNumber", "PrevDocRefNum", first.ReferenceNumber);
				AssertEquals("1st Type", "C15", first.Type);
			});
		}

		public void TestSupportingDocuments()
		{
			Factory.CreateCusSupportingInfo("SUP", null, "SupDocRefNum", "SupDocCOI", "C15", Header.MovementHeader);
			Factory.CreateCusSupportingInfo("SUP", null, "SupDocRefNum", "SupDocCOI", "C15", Header.MovementHeader);

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: Count", 0, provider.SupportingDocuments.Count);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 2, provider.SupportingDocuments.Count);
				var first = provider.SupportingDocuments.First();
				AssertEquals("1st ComplementOfInformation", "SupDocCOI", first.ComplementOfInformation);
				AssertEquals("1st ReferenceNumber", "SupDocRefNum", first.ReferenceNumber);
				AssertEquals("1st Type", "C15", first.Type);
			});
		}

		public void TestTransportDocuments()
		{
			Factory.CreateCusSupportingInfo("OTH", "TRA", "TranDocRefNum", null, "C14", Header);
			Factory.CreateCusSupportingInfo("OTH", "TRA", "TranDocRefNum", null, "C14", Header);
			Factory.CreateCusSupportingInfo("OTH", "REF", "AddRefRefNum", null, "C13", Header);

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: Count", 0, provider.TransportDocuments.Count);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 2, provider.TransportDocuments.Count);
				var first = provider.TransportDocuments.First();
				AssertEquals("1st ReferenceNumber", "TranDocRefNum", first.ReferenceNumber);
				AssertEquals("1st Type", "C14", first.Type);
			});
		}

		public void TestAdditionalReferences()
		{
			Factory.CreateCusSupportingInfo("OTH", "REF", "AddRefRefNum", null, "C13", Header);
			Factory.CreateCusSupportingInfo("OTH", "REF", "AddRefRefNum", null, "C13", Header);
			Factory.CreateCusSupportingInfo("OTH", "TRA", "TranDocRefNum", null, "C14", Header);

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: Count", 0, provider.AdditionalReferences.Count);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 2, provider.AdditionalReferences.Count);
				var first = provider.AdditionalReferences.First();
				AssertEquals("1st ReferenceNumber", "AddRefRefNum", first.ReferenceNumber);
				AssertEquals("1st Type", "C13", first.Type);
			});
		}

		public void TestAdditionalInformation()
		{
			Factory.CreateCusSupportingInfo("OTH", "INF", "AddInfoRefNum", null, "C13", Header);
			Factory.CreateCusSupportingInfo("OTH", "INF", "AddInfoRefNum", null, "C13", Header);

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: Count", 0, provider.AdditionalInformation.Count);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 2, provider.AdditionalInformation.Count);
			});
		}

		public void TestHouseConsignments()
		{
			var bill1 = Factory.New<NctsBill>();
			bill1.B0_BH = Header.PK;
			var bill2 = Factory.New<NctsBill>();
			bill2.B0_BH = Header.PK;
			Header.Bills.AddNew();
			Header.Bills.AddNew();
			AssertEquals(4, Provider.HouseConsignments.Count);
		}

		public void TestHouseConsignmentsSorted()
		{
			AddBill(1, "1");
			AddBill(10, "10");
			AddBill(11, "11");
			AddBill(12, "12");
			AddBill(2, "2");
			AddBill(3, "3");
			AddBill(4, "4");
			AddBill(5, "5");
			AddBill(6, "6");
			AddBill(7, "7");
			AddBill(8, "8");
			AddBill(9, "9");

			AssertEquals(12, Provider.HouseConsignments.Count);
			var consignmentNo = 1;

			foreach (var consignment in Provider.HouseConsignments)
			{
				AssertEquals("Expect SequenceNumber in order", true, consignment.SequenceNumber == consignmentNo);
				AssertEquals("Expect ReferenceNumberUCR in order", true, consignment.ReferenceNumberUCR == consignmentNo.ToString());
				consignmentNo++;
			}
		}

		void AddBill(ZShort sequenceNo, string reference)
		{
			var bill = Header.Bills.AddNew();
			bill.SequenceNumber = sequenceNo;
			bill.B0_ReferenceID = reference;
		}

		public void TestTransportChargesMethodOfPayment()
		{
			AssertNullOrEmpty(Provider.TransportChargesMethodOfPayment);
			Header.MovementHeader.BM_MethodOfPayment = "A";
			AssertEquals("A", Provider.TransportChargesMethodOfPayment);
		}

		protected override NCTSConsignmentProvider GetProvider() => new(Header);

		NctsHeader Header => _header ??= GetHeader();
		NctsHeader _header;

		NctsHeader GetHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.DepartureHeaderContainers.AddNew();
			return nctsHeader;
		}
	}
}
