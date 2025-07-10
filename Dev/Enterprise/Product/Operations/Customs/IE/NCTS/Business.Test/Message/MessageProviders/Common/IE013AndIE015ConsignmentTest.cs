using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE013AndIE015ConsignmentTest : Customs.Business.Testing.DataProviderTestCase<IE013AndIE015Consignment>
	{
		protected override IE013AndIE015Consignment GetProvider() => new IE013AndIE015Consignment(nctsHeader);

		public void TestTransportEquipments()
		{
			SetupTransportEquipments();
			AssertType<ITransportEquipmentWithSeals[]>("Transport Equipments", Provider.TransportEquipments);
			AssertEquals(2, Provider.TransportEquipments.Count);
		}

		void SetupTransportEquipments()
		{
			TransportEquipmentWithSealsProviderTest.AddContainer(nctsHeader, "EIRU1909897");
			TransportEquipmentWithSealsProviderTest.AddContainer(nctsHeader, "EIRU1909886");
		}

		public void TestLocationOfGoods()
		{
			AssertType<LocationOfGoodsProvider>("Location of Goods", Provider.LocationOfGoods);
		}

		public void TestDepartureTransportMeans()
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			movementHeader.BM_TransportAtDeparture = "777";
			movementHeader.BM_RN_NKTransportAtDepartureCountry = "IE";
			movementHeader.BM_TransportAtDepartureTrailer1RegNo = "AS1111";
			movementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = "XI";
			AssertType<ITransportMeans[]>("Departure Transport Means", Provider.DepartureTransportMeans);
			AssertEquals(2, Provider.DepartureTransportMeans.Count);
		}

		public void TestCountriesOfRouting()
		{
			var country1 = nctsHeader.CountriesOfRouting.AddNew();
			country1.CY_Data = "IE";
			var country2 = nctsHeader.CountriesOfRouting.AddNew();
			country2.CY_Data = "FR";

			AssertEquals("Countries of Routing", 2, Provider.CountriesOfRouting.Count);
			AssertEquals("IE", Provider.CountriesOfRouting.First());
			AssertEquals("FR", Provider.CountriesOfRouting.Last());
		}

		public void TestActiveBorderTransportMeans()
		{
			AssertType<ActiveTransportMeansProvider>("Active Border Transport Means", Provider.ActiveBorderTransportMeans.FirstOrDefault());
		}

		public void TestPlaceOfLoading()
		{
			CombineAssertions("null or empty values", () =>
			{
				AssertNullOrEmpty("Place of Loading - UNLocode", Provider.PlaceOfLoading.UNLocode);
				AssertNullOrEmpty("Plece of Loading - Country", Provider.PlaceOfLoading.Country);
				AssertNullOrEmpty("Plece of Loading - Location", Provider.PlaceOfLoading.Location);
			});

			CombineAssertions("Two chars long BM_PortOfPresentationCode", () =>
			{
				movementHeader.BM_PortOfPresentationCode = "IE";
				movementHeader.BM_PlaceOfLoading = "Galway";
				AssertNullOrEmpty("Place of Loading - UNLocode", GetProvider().PlaceOfLoading.UNLocode);
				AssertEquals("Plece of Loading - Country", "IE", GetProvider().PlaceOfLoading.Country);
				AssertEquals("Plece of Loading - Location", "Galway", GetProvider().PlaceOfLoading.Location);
			});

			CombineAssertions("More than two chars long BM_PortOfPresentationCode", () =>
			{
				movementHeader.BM_PortOfPresentationCode = "IEGWY";
				movementHeader.BM_PlaceOfLoading = "Galway";
				AssertEquals("Place of Loading - UNLocode", "IEGWY", GetProvider().PlaceOfLoading.UNLocode);
				AssertNullOrEmpty("Plece of Loading - Country", GetProvider().PlaceOfLoading.Country);
				AssertNullOrEmpty("Plece of Loading - Location", GetProvider().PlaceOfLoading.Location);
			});
		}

		public void TestPlaceOfUnloading()
		{
			CombineAssertions("null or empty values", () =>
			{
				AssertNullOrEmpty("Place of Unloading - UNLocode", Provider.PlaceOfUnloading.UNLocode);
				AssertNullOrEmpty("Plece of Unloading - Country", Provider.PlaceOfUnloading.Country);
				AssertNullOrEmpty("Plece of Unloading - Location", Provider.PlaceOfUnloading.Location);
			});

			CombineAssertions("Two chars long BM_ForeignDestPortKCode", () =>
			{
				movementHeader.BM_ForeignDestPortKCode = "ES";
				movementHeader.BM_PlaceOfUnloading = "Malaga";
				AssertNullOrEmpty("Place of Unloading - UNLocode", GetProvider().PlaceOfUnloading.UNLocode);
				AssertEquals("Plece of Unloading - Country", "ES", GetProvider().PlaceOfUnloading.Country);
				AssertEquals("Plece of Unloading - Location", "Malaga", GetProvider().PlaceOfUnloading.Location);
			});

			CombineAssertions("More than two chars long BM_ForeignDestPortKCode", () =>
			{
				movementHeader.BM_ForeignDestPortKCode = "ESAGP";
				movementHeader.BM_PlaceOfUnloading = "Malaga";
				AssertEquals("Place of Unloading - UNLocode", "ESAGP", GetProvider().PlaceOfUnloading.UNLocode);
				AssertNullOrEmpty("Plece of Unloading - Country", GetProvider().PlaceOfUnloading.Country);
				AssertNullOrEmpty("Plece of Unloading - Location", GetProvider().PlaceOfUnloading.Location);
			});
		}

		public void TestPreviousDocuments()
		{
			AssertNull("Previous Documents", Provider.PreviousDocuments.FirstOrDefault());

			nctsHeader.PreviousDocuments.AddNew();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertType<PreviousDocumentProvider>("Previous Documents", GetProvider().PreviousDocuments.FirstOrDefault());
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals("Empty when Transition Period", 0, GetProvider().PreviousDocuments.Count);
			}
		}

		public void TestSupportingDocuments()
		{
			AssertNull("Supporting Documents", Provider.SupportingDocuments.FirstOrDefault());

			nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertType<SupportingDocumentProvider>("Supporting Documents", GetProvider().SupportingDocuments.FirstOrDefault());
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals("Empty when Transition Period", 0, GetProvider().SupportingDocuments.Count);
			}
		}

		public void TestAdditionalReferences()
		{
			SetupAdditionalReferences();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var provider = GetProvider();
				AssertEquals("Additional reference count", 2, provider.AdditionalReferences.Count);
				var docs = provider.AdditionalReferences.ToArray();

				AssertEquals("Doc 1 Type", "Y023", docs[0].Type);
				AssertEquals("Doc 1 Reference", "TEST123", docs[0].Reference);

				AssertEquals("Doc 2 Type", "Y929", docs[1].Type);
				AssertEquals("Doc 2 Reference", "TEST456", docs[1].Reference);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals("Empty when Transition Period", 0, GetProvider().AdditionalReferences.Count);
			}
		}

		void SetupAdditionalReferences()
		{
			var doc1 = nctsHeader.AdditionalDocuments.AddNew();
			doc1.CSI_SubType = "REF";
			doc1.CSI_Code = "Y023";
			doc1.CSI_ReferenceNumber = "TEST123";

			var doc2 = nctsHeader.AdditionalDocuments.AddNew();
			doc2.CSI_SubType = "REF";
			doc2.CSI_Code = "Y929";
			doc2.CSI_ReferenceNumber = "TEST456";
		}

		public void TestTransportDocuments()
		{
			AssertNull("Transport Documents", Provider.TransportDocuments.FirstOrDefault());

			SetupTransportDocument(nctsHeader.AdditionalDocuments.AddNew(), "TRA1", "TRA_REF");
			SetupTransportDocument(nctsHeader.AdditionalDocuments.AddNew(), "TRA2", "TRA_REF");
			SetupTransportDocument(nctsHeader.AdditionalDocuments.AddNew(), "TRA2", "TRA_REF");
			SetupTransportDocument(nctsHeader.AdditionalDocuments.AddNew(), "TRA3", "TRA_REF");
			SetupTransportDocument(nctsHeader.AdditionalDocuments.AddNew(), "TRA4", "TRA_REF1");

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var transportDocuments = GetProvider().TransportDocuments.ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("count", 4, transportDocuments.Length);
					AssertDocument(transportDocuments[0], "TRA1", "TRA_REF");
					AssertDocument(transportDocuments[1], "TRA2", "TRA_REF");
					AssertDocument(transportDocuments[2], "TRA3", "TRA_REF");
					AssertDocument(transportDocuments[3], "TRA4", "TRA_REF1");
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals("Empty when Transition Period", 0, GetProvider().TransportDocuments.Count);
			}
		}

		void AssertDocument(IDocument document, string expectedType, string expectedReference)
		{
			AssertEquals("Type", expectedType, document.Type);
			AssertEquals("Reference", expectedReference, document.Reference);
		}

		void SetupTransportDocument(AdditionalInfo info, ZString code, ZString referenceNumber)
		{
			info.CSI_SubType = "TRA";
			info.CSI_Code = code;
			info.CSI_ReferenceNumber = referenceNumber;
		}

		public void TestAdditionalInformations()
		{
			SetupAdditionalInformations();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var provider = GetProvider();
				AssertEquals("Additional Information count", 2, provider.AdditionalInformations.Count);
				var docs = provider.AdditionalInformations.ToArray();

				AssertEquals("Doc 1 Code", "20100", docs[0].Code);
				AssertEquals("Doc 1 Reference", "TEST123", docs[0].Text);

				AssertEquals("Doc 2 Type", "20300", docs[1].Code);
				AssertEquals("Doc 2 Reference", "TEST456", docs[1].Text);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var additionalInformations = GetProvider().AdditionalInformations;
				AssertEquals("Empty when Transition Period", false, additionalInformations.Any());
			}
		}

		void SetupAdditionalInformations()
		{
			var doc1 = nctsHeader.AdditionalDocuments.AddNew();
			doc1.CSI_SubType = "INF";
			doc1.CSI_Code = "20100";
			doc1.CSI_Description = "TEST123";

			var doc2 = nctsHeader.AdditionalDocuments.AddNew();
			doc2.CSI_SubType = "INF";
			doc2.CSI_Code = "20300";
			doc2.CSI_Description = "TEST456";
		}

		public void TestCountryOfDispatch()
		{
			movementHeader.BM_RN_NKCountryOfDispatch = "IE";
			AssertEquals("CountryOfDispatch", "IE", Provider.CountryOfDispatch);
		}

		public void TestCountryOfDestination()
		{
			movementHeader.BM_RL_NKDestinationPort = "FR";
			AssertEquals("CountryOfDestination", "FR", Provider.CountryOfDestination);
		}

		public void TestAdditionalSupplyChainActors()
		{
			movementHeader.CusSupplyChainActors.AddNew().CFR_Code = "C1";
			movementHeader.CusSupplyChainActors.AddNew().CFR_Code = "C2";

			AssertEquals("AdditionalSupplyChainActors", 2, Provider.AdditionalSupplyChainActors.Count);
			AssertEquals("CFR_Code C1", true, Provider.AdditionalSupplyChainActors.Any(actor => actor.Role == "C1"));
			AssertEquals("CFR_Code C2", true, Provider.AdditionalSupplyChainActors.Any(actor => actor.Role == "C2"));
		}

		public void TestMethodOfPayment()
		{
			movementHeader.BM_MethodOfPayment = "1";
			AssertEquals("MethodOfPayment", "1", Provider.MethodOfPayment);
		}

		public void TestHouseConsignments()
		{
			var bill1 = nctsHeader.Bills.AddNew();
			bill1.SequenceNumber = 2;
			bill1.B0_Weight = 1;
			bill1.B0_ReferenceID = "REF123";
			var bill2 = nctsHeader.Bills.AddNew();
			bill2.B0_Weight = 2;
			bill2.SequenceNumber = 1;
			bill2.B0_ReferenceID = "RE123333";
			var houseConsignments = Provider.HouseConsignments.ToArray();
			AssertEquals("Count", 2, houseConsignments.Length);
			AssertEquals("First Item ReferenceNumber", "RE123333", houseConsignments[0].ReferenceNumberUCR);
			AssertEquals("Second Item ReferenceNumber", "REF123", houseConsignments[1].ReferenceNumberUCR);
			AssertEquals("Consignment 1", new decimal(2), houseConsignments[0].GrossMass);
			AssertEquals("Consignment 2", new decimal(1), houseConsignments[1].GrossMass);
		}

		public void TestInlandTransportMode()
		{
			AssertEquals(string.Empty, Provider.InlandTransportMode);
			nctsHeader.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
			AssertEquals("1", Provider.InlandTransportMode);
		}

		public void TestBorderTransportMode()
		{
			AssertEquals(string.Empty, Provider.BorderTransportMode);
			nctsHeader.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._4_AirTransport;
			AssertEquals("4", Provider.BorderTransportMode);
		}

		public void TestReferenceNumberUCR()
		{
			AssertEquals(string.Empty, Provider.ReferenceNumberUCR);
			nctsHeader.MovementHeader.BM_UniqueConsignmentReference = "REF123";
			AssertEquals("REF123", Provider.ReferenceNumberUCR);
		}

		public void TestGrossMass()
		{
			AssertEquals(0m, Provider.GrossMass);
			nctsHeader.MovementHeader.BM_GrossWeight = 14.123;
			AssertEquals(14.123m, Provider.GrossMass);
		}

		public void TestHasContainer()
		{
			CombineAssertions(() =>
			{
				AssertEquals(false, Provider.HasContainer);
				var container = nctsHeader.DepartureHeaderContainers.AddNew();
				container.BC_ContainerNum = "CONT001";
				AssertEquals(true, Provider.HasContainer);
			});
		}

		public void TestCarrier()
		{
			CombineAssertions(() =>
			{
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CAR", nctsHeader.MovementHeader.Carrier, string.Empty, "Test Carrier Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "12345678", "TIR123", contactName: "Carrier Contact Name", contactPhone: "0858585858", contactEmail: "contact@carrier.com", contactAllocation: OrgConstants.ContactAllocationType.CUS);
				AssertEquals("IE12345678", Provider.Carrier.CarrierId);
				AssertEquals("Carrier Contact Name", Provider.Carrier.CarrierContact.Name);
				AssertEquals("0858585858", Provider.Carrier.CarrierContact.PhoneNumber);
				AssertEquals("contact@carrier.com", Provider.Carrier.CarrierContact.EmailAddress);
			});
		}

		public void TestCarrier_MatchesPrincipal()
		{
			CombineAssertions(() =>
			{
				var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CAR", nctsHeader.MovementHeader.Carrier, string.Empty, "Test Carrier Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "12345678", "TIR123", contactName: "Carrier Contact Name", contactPhone: "0858585858", contactEmail: "contact@carrier.com", contactAllocation: OrgConstants.ContactAllocationType.CUS);
				nctsHeader.Principal.OrganisationPK = orgHeader.PK;
				AssertNull("Carrier matches Principal", Provider.Carrier);
			});
		}

		public void TestConsignor()
		{
			CombineAssertions(() =>
			{
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CNR", nctsHeader.Consignor, string.Empty, "Test Consignor Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "12345678", "TIR123", contactName: "Joe Bloggs", contactPhone: "5551234", contactEmail: "test@example.com", contactAllocation: OrgConstants.ContactAllocationType.CUS);
				AssertEquals("IE12345678", Provider.Consignor.Id);
				AssertEquals("Test Consignor Limited", Provider.Consignor.Name);
				AssertEquals("123 Test Street", Provider.Consignor.Address.StreetAndNumber);
				AssertEquals("A12B3C4", Provider.Consignor.Address.Postcode);
				AssertEquals("City", Provider.Consignor.Address.City);
				AssertEquals("IE", Provider.Consignor.Address.Country);
				AssertEquals("Joe Bloggs", Provider.Consignor.Contact.Name);
				AssertEquals("5551234", Provider.Consignor.Contact.PhoneNumber);
				AssertEquals("test@example.com", Provider.Consignor.Contact.EmailAddress);
			});
		}

		public void TestConsignor_EmptyEORI()
		{
			NCTSTestHelper.CreateJobDocAddressForTest(
				Factory,
				traderId: "CNE",
				jda: nctsHeader.Consignor,
				suffix: string.Empty,
				traderName: "Test Consignor Limited",
				address1: "123 Test Street",
				postCode: "A12B3C4",
				city: "City",
				relatedPortCode: "IEXX",
				countryCode: "GB",
				traderTin: string.Empty,
				traderTir: "TIR001",
				contactName: "Consignor Contact",
				contactEmail: "contact.email@Consignor.company",
				contactPhone: "5551234"
			);
			nctsHeader.Consignor.Organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(TaxTypeGenericList.Codes.Vat, "001", Core.Constants.CountryCodes.Ireland);

			CombineAssertions("Only provide EORI when present for foreign Consignor. Not provide any other.", () =>
			{
				var outputConsignor = Provider.Consignor;
				var contact = outputConsignor.Contact;
				AssertEquals("Consignor.Id should be empty when EORI not present.", string.Empty, outputConsignor.Id);
				AssertEquals("Contact Name should be provided.", "Consignor Contact", contact.Name);
				AssertEquals("Contact phone should be provided.", "5551234", contact.PhoneNumber);
				AssertEquals("Contact email should be provided.", "contact.email@Consignor.company", contact.EmailAddress);
			});
		}

		public void TestConsignor_MatchesPrincipal()
		{
			CombineAssertions(() =>
			{
				var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CNR", nctsHeader.Consignor, string.Empty, "Test Consignor Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "12345678", "TIR123", contactName: "Joe Bloggs", contactPhone: "5551234", contactEmail: "test@example.com", contactAllocation: OrgConstants.ContactAllocationType.CUS);
				nctsHeader.Principal.OrganisationPK = orgHeader.PK;
				AssertNull("Consignor matches Principal", Provider.Consignor);
			});
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CNE", nctsHeader.Consignee, string.Empty, "Test Consignee Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "12345678", string.Empty);
				AssertEquals("IE12345678", Provider.Consignee.Id);
				AssertEquals("Test Consignee Limited", Provider.Consignee.Name);
				AssertEquals("123 Test Street", Provider.Consignee.Address.StreetAndNumber);
				AssertEquals("A12B3C4", Provider.Consignee.Address.Postcode);
				AssertEquals("City", Provider.Consignee.Address.City);
				AssertEquals("IE", Provider.Consignee.Address.Country);
				AssertNull(Provider.Consignee.Contact);
			});
		}

		public void TestConsignee_EmptyEORI()
		{
			NCTSTestHelper.CreateJobDocAddressForTest(
				Factory,
				traderId: "CNE",
				jda: nctsHeader.Consignee,
				suffix: string.Empty,
				traderName: "Test Consignee Limited",
				address1: "123 Test Street",
				postCode: "A12B3C4",
				city: "City",
				relatedPortCode: "IEXX",
				countryCode: "GB",
				traderTin: string.Empty,
				traderTir: "TIR001",
				contactName: "Consignee Contact",
				contactEmail: "contact.email@consignee.company",
				contactPhone: "5551234"
			);
			nctsHeader.Consignee.Organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(TaxTypeGenericList.Codes.Vat, "001", Core.Constants.CountryCodes.Ireland);

			CombineAssertions("Only provide EORI when present for foreign consignee. Not provide any other.", () =>
			{
				var outputConsignee = Provider.Consignee;
				var contact = outputConsignee.Contact;
				AssertEquals("Consignee.Id should be empty when EORI not present.", string.Empty, outputConsignee.Id);
				AssertEquals("Contact Name should be provided.", "Consignee Contact", contact.Name);
				AssertEquals("Contact phone should be provided.", "5551234", contact.PhoneNumber);
				AssertEquals("Contact email should be provided.", "contact.email@consignee.company", contact.EmailAddress);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_RL_NKDestinationPort = "GB";
		}
		NctsDepartureMovementHeader movementHeader;
		NctsHeader nctsHeader;
	}
}
