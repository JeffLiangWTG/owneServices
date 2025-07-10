using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.CH.NCTS.Business.UniversalReferenceConstants;
using static Enterprise.Customs.Universal.Constants;
using Constants = Enterprise.Core.Constants;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class ConsignmentDataProviderTest : BaseDepartureDataProviderTest<ConsignmentDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("NctsHeader==null", ConsignmentDataProvider.New(null));

			var nctsHeader = Factory.New<NctsHeader>();
			AssertNull("MovementHeader==null", ConsignmentDataProvider.New(nctsHeader));

			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			AssertNotNull("MovementHeader != null", ConsignmentDataProvider.New(nctsHeader));
		});
	}

	public void TestProvider()
	{
		const string referenceNumberUCR = "12345";

		NctsHeader.MovementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Switzerland;
		NctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Germany;
		NctsHeader.MovementHeader.BM_UniqueConsignmentReference = referenceNumberUCR;
		NctsHeader.Bills.AddNew().GoodsItems.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Country of destination", Core.Constants.CountryCodes.Switzerland, DataProvider.CountryOfDestination);
			AssertEquals("Country of dispatch", Core.Constants.CountryCodes.Germany, DataProvider.CountryOfDispatch);
			AssertEquals("Reference Number UCR", referenceNumberUCR, DataProvider.ReferenceNumberUCR);
		});
	}

	public void TestUnusedProperties() => CombineAssertions(() =>
	{
		AssertNull("PartialDelivery", DataProvider.PartialDelivery);
		AssertEquals("Preference", false, DataProvider.Preference);
		AssertNull("ConsignmentItems not available", DataProvider.ConsignmentItems);
		AssertNull("Importer", DataProvider.Importer);
		AssertNull("PartialShipment", DataProvider.PartialShipment);
	});

	public void TestProviderNullableProperties()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Country of destination", null, DataProvider.CountryOfDestination);
			AssertEquals("Country of dispatch", null, DataProvider.CountryOfDispatch);
			AssertEquals("Reference Number UCR", null, DataProvider.ReferenceNumberUCR);
		});
	}

	public void TestContainerIndicator() => CombineAssertions(() =>
	{
		AssertEquals("No NctsDepartureContainerHeader", false, DataProvider.ContainerIndicator);

		var container1 = NctsHeader.DepartureHeaderContainers.AddNew();
		container1.BC_Mode = ContainerModes.NonContainerised;
		var container2 = NctsHeader.DepartureHeaderContainers.AddNew();
		container2.BC_Mode = ContainerModes.NonContainerised;
		ResetDataProvider();
		AssertEquals("No NctsDepartureContainerHeader with BC_Mode=CNT", false, DataProvider.ContainerIndicator);

		container2.BC_Mode = ContainerModes.Containerised;
		ResetDataProvider();
		AssertEquals("At least one NctsDepartureContainerHeader with BC_Mode=CNT", true, DataProvider.ContainerIndicator);

		container2.BC_Mode = ContainerModes.NonContainerised;

		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		NctsHeader.MovementHeader.RelatedExportEntryHeaders.AddPivotFor(entryHeader);

		jobDeclaration.JE_ContainerMode = ContainerModes.NonContainerised;
		ResetDataProvider();
		AssertEquals($"No containerised linked job", false, DataProvider.ContainerIndicator);

		foreach (var containerMode in new[] { ContainerModes.Containerised, ContainerModes.FCL, ContainerModes.LCL })
		{
			jobDeclaration.JE_ContainerMode = containerMode;
			ResetDataProvider();
			AssertEquals($"Linked EXP declaration with JE_ContainerMode={containerMode}", true, DataProvider.ContainerIndicator);
		}

		jobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		ResetDataProvider();
		AssertEquals($"Linked EDA declaration with JE_ContainerMode={jobDeclaration.JE_ConsolidatedCargoStatus}", true, DataProvider.ContainerIndicator);

		jobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		ResetDataProvider();
		AssertEquals($"Linked declaration neither EXP nor EDA", false, DataProvider.ContainerIndicator);
	});

	public void TestConsignor()
	{
		var consignor = OrgHeader.New(Factory);
		consignor.OH_FullName = "Consignor";
		NctsHeader.Consignor.OrganisationPK = consignor.PK;
		var contact = consignor.Contacts.AddNew();
		contact.OC_Email = "cus@example.org";
		contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.Consignor);
			AssertEquals("Consignor", "Consignor", DataProvider.Consignor.Name);
			AssertEquals("Allocated contact of type CUS expected", "cus@example.org", DataProvider.Consignor.ContactPerson.EmailAddress);
			AssertSame("cached", DataProvider.Consignor, DataProvider.Consignor);

			AssertParticipentIdentificationNumberOrNameAddress("Consignor", NctsHeader.Consignor, () => DataProvider.Consignor, OrgCusCode.SwissCodeTypes.BID);
		});
	}

	public void TestConsignee()
	{
		var consignee = OrgHeader.New(Factory);
		consignee.OH_FullName = "Consignee";
		NctsHeader.Consignee.OrganisationPK = consignee.PK;

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.Consignee);
			AssertEquals("Consignee", DataProvider.Consignee.Name);
			AssertSame("cached", DataProvider.Consignee, DataProvider.Consignee);

			AssertParticipentIdentificationNumberOrNameAddress("Consigee", NctsHeader.Consignee, () => DataProvider.Consignee, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
		});
	}

	public void TestHouseConsignment()
	{
		var bill1 = NctsHeader.Bills.AddNew();
		var bill2 = NctsHeader.Bills.AddNew();
		bill1.B0_ReferenceID = "BILL1";
		bill2.B0_ReferenceID = "BILL2";

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.HouseConsignments);
			AssertEquals("count", 2, DataProvider.HouseConsignments.Count);
			AssertEquals("Bill1", "BILL1", DataProvider.HouseConsignments.ElementAt(0).ReferenceNumberUCR);
			AssertEquals("Bill2", "BILL2", DataProvider.HouseConsignments.ElementAt(1).ReferenceNumberUCR);
			AssertSame("cached", DataProvider.HouseConsignments, DataProvider.HouseConsignments);
		});
	}

	public void TestCountryofRouting()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.CountryOfRoutingOfConsignments);
			AssertSame("cached", DataProvider.CountryOfRoutingOfConsignments, DataProvider.CountryOfRoutingOfConsignments);

			NctsHeader.CountriesOfRouting.AddNew();
			NctsHeader.CountriesOfRouting.AddNew();
			var dataProvider = ConsignmentDataProvider.New(NctsHeader);
			AssertEquals("Country of Routing Count", NctsHeader.CountriesOfRouting.Count, dataProvider.CountryOfRoutingOfConsignments.Count);
		});
	}

	public void TestAdditionalSupplyChainActor() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.AdditionalSupplyChainActors);
		AssertSame("cached", DataProvider.AdditionalSupplyChainActors, DataProvider.AdditionalSupplyChainActors);

		var movementHeader = NctsHeader.MovementHeader;

		movementHeader.CusSupplyChainActors.AddNew();
		movementHeader.CusSupplyChainActors.AddNew();
		ResetDataProvider();
		AssertEquals("No Additional Supply Chain Actors Count", movementHeader.CusSupplyChainActors.Count, DataProvider.AdditionalSupplyChainActors.Count);

		movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		ResetDataProvider();
		AssertNull("No Additional Supply Chain Actors for T-CH", DataProvider.AdditionalSupplyChainActors);
	});

	public void TestPlaceOfLoading()
	{
		CombineAssertions(() =>
		{
			NctsHeader.MovementHeader.BM_PortOfPresentationCode = "CH3DV";
			AssertNotNull(DataProvider.PlaceOfLoading);
			AssertEquals("PlaceOfLoading.UNLocode", "CH3DV", DataProvider.PlaceOfLoading.UNLocode);
			AssertSame("cached", DataProvider.PlaceOfLoading, DataProvider.PlaceOfLoading);
		});
	}

	public void TestPlaceOfUnloading() => CombineAssertions(() =>
	{
		NctsHeader.MovementHeader.BM_ForeignDestPortKCode = "CH3DV";
		NctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
		AssertNotNull("BM_TypeOfSecurity <> NON", DataProvider.PlaceOfUnloading);
		AssertEquals("PlaceOfUnloading.UNLocode", "CH3DV", DataProvider.PlaceOfUnloading.UNLocode);
		AssertSame("cached", DataProvider.PlaceOfUnloading, DataProvider.PlaceOfUnloading);

		ResetDataProvider();
		NctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
		AssertNull("BM_TypeOfSecurity = NON", DataProvider.PlaceOfUnloading);
	});

	public void TestPreviousDocuments() => CombineAssertions(() =>
	{
		AssertEquals("Precondition", 0, DataProvider.PreviousDocuments.Count);

		NctsHeader.PreviousDocuments.AddNew();
		NctsHeader.PreviousDocuments.AddNew();
		NctsHeader.PreviousDocuments.AddNew();

		var consignmentDataProvider = ConsignmentDataProvider.New(NctsHeader);
		AssertEquals("Count (previous documents only)", 3, consignmentDataProvider.PreviousDocuments.Count);
		Assert("PreviousDocuments Type (previous documents only)", consignmentDataProvider.PreviousDocuments.All(p => p is DocumentDataProvider));
		Assert("SequenceNumber (previous documents only)", !consignmentDataProvider.PreviousDocuments.Where((x, i) => x.SequenceNumber != i + 1).Any());

		var jobDeclaration = Factory.New<JobDeclaration>();
		AddEntryHeader("MRN1");
		AddEntryHeader("MRN2");

		consignmentDataProvider = ConsignmentDataProvider.New(NctsHeader);
		AssertEquals("Count (export entries and previous documents)", 5, consignmentDataProvider.PreviousDocuments.Count);
		Assert("ExportEntries Type (export entries and previous documents)", consignmentDataProvider.PreviousDocuments.Take(2).All(p => p is ExportEntryDocumentDataProvider));
		Assert("PreviousDocuments Type (export entries and previous documents)", consignmentDataProvider.PreviousDocuments.Skip(2).All(p => p is DocumentDataProvider));
		Assert("SequenceNumber (export entries and previous documents)", !consignmentDataProvider.PreviousDocuments.Where((x, i) => x.SequenceNumber != i + 1).Any());

		NctsHeader.PreviousDocuments.RemoveAll();
		consignmentDataProvider = ConsignmentDataProvider.New(NctsHeader);
		AssertEquals("Count (export entries only)", 2, consignmentDataProvider.PreviousDocuments.Count);
		Assert("ExportEntries Type (export entries only)", consignmentDataProvider.PreviousDocuments.Take(2).All(p => p is ExportEntryDocumentDataProvider));

		AssertSame("Cached", consignmentDataProvider.PreviousDocuments, consignmentDataProvider.PreviousDocuments);

		void AddEntryHeader(string mrn)
		{
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(mrn);
			NctsHeader.MovementHeader.RelatedExportEntryHeaders.AddPivotFor(entryHeader);
		}
	});

	public void TestSupportingDocuments()
	{
		AssertEquals("Precondition", 0, DataProvider.SupportingDocuments.Count);

			NctsHeader.MovementHeader.SupportingDocuments.AddNew();
			NctsHeader.MovementHeader.SupportingDocuments.AddNew();

		var consignmentDataProvider = ConsignmentDataProvider.New(NctsHeader);

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, consignmentDataProvider.SupportingDocuments.Count);
			Assert("Type", consignmentDataProvider.SupportingDocuments.All(p => p is DocumentDataProvider));
			AssertSame("Cached", consignmentDataProvider.SupportingDocuments, consignmentDataProvider.SupportingDocuments);
		});
	}

	public void TestTransportDocuments()
	{
		AssertEquals("Precondition", 0, DataProvider.TransportDocuments.Count);

		var transportDocument1 = NctsHeader.AdditionalDocuments.AddNew();
		transportDocument1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		var transportDocument2 = NctsHeader.AdditionalDocuments.AddNew();
		transportDocument2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

		var consignmentDataProvider = ConsignmentDataProvider.New(NctsHeader);

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, consignmentDataProvider.TransportDocuments.Count);
			Assert("Type", consignmentDataProvider.TransportDocuments.All(p => p is TransportDocumentDataProvider));
			AssertSame("Cached", consignmentDataProvider.TransportDocuments, consignmentDataProvider.TransportDocuments);
		});
	}

	public void TestAdditionalInformation()
	{
		AssertEquals("Precondition", 0, DataProvider.AdditionalInformations.Count);

		var additionalInformation1 = NctsHeader.AdditionalDocuments.AddNew();
		additionalInformation1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		var additionalInformation2 = NctsHeader.AdditionalDocuments.AddNew();
		additionalInformation2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

		var consignmentDataProvider = ConsignmentDataProvider.New(NctsHeader);

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, consignmentDataProvider.AdditionalInformations.Count);
			Assert("Type", consignmentDataProvider.AdditionalInformations.All(p => p is AdditionalInformationDataProvider));
			AssertSame("Cached", consignmentDataProvider.AdditionalInformations, consignmentDataProvider.AdditionalInformations);
		});
	}

	public void TestAdditionalReferences()
	{
		AssertEquals("Precondition", 0, DataProvider.AdditionalReferences.Count);

		var additionalReference1 = NctsHeader.AdditionalDocuments.AddNew();
		additionalReference1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		var additionalReference2 = NctsHeader.AdditionalDocuments.AddNew();
		additionalReference2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

		var consignmentDataProvider = ConsignmentDataProvider.New(NctsHeader);

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, consignmentDataProvider.AdditionalReferences.Count);
			Assert("Type", consignmentDataProvider.AdditionalReferences.All(p => p is AdditionalReferenceDataProvider));
			AssertSame("Cached", consignmentDataProvider.AdditionalReferences, consignmentDataProvider.AdditionalReferences);
		});
	}

	public void TestTransportEquipment()
	{
		var headerContainer1 = NctsHeader.DepartureHeaderContainers.AddNew();
		var headerContainer2 = NctsHeader.DepartureHeaderContainers.AddNew();
		headerContainer1.BC_ContainerNum = "ABCD1234";
		headerContainer1.BC_Mode = Constants.ContainerModes.Containerised;
		headerContainer2.BC_ContainerNum = "EFGH5678";
		headerContainer2.BC_Mode = Constants.ContainerModes.Containerised;

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.TransportEquipments);
			AssertEquals("Header Container 1", headerContainer1.BC_ContainerNum, DataProvider.TransportEquipments.ElementAt(0).ContainerIdentificationNumber);
			AssertEquals("Header Container 2", headerContainer2.BC_ContainerNum, DataProvider.TransportEquipments.ElementAt(1).ContainerIdentificationNumber);
			AssertSame("cached", DataProvider.TransportEquipments, DataProvider.TransportEquipments);
		});
	}

	public void TestActiveBorderTransportMeans_WithExportTransportMode()
	{
		NctsHeader.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._1_SeaTransport;
		NctsHeader.MovementHeader.BM_ActiveBorderIdentificationType = ZString.Empty;

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.ActiveBorderTransportMeans);
			AssertEquals("Active Border Transport Means", null, DataProvider.ActiveBorderTransportMeans.ElementAt(0).TypeOfIdentification);
			AssertSame("cached", DataProvider.ActiveBorderTransportMeans, DataProvider.ActiveBorderTransportMeans);
		});
	}

	public void TestActiveBorderTransportMeans_WithoutExportTransportMode()
	{
		AssertNull(DataProvider.ActiveBorderTransportMeans);
	}

	public void TestTransportCharges()
	{
		NctsHeader.MovementHeader.BM_MethodOfPayment = "A";

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.TransportCharges);
			AssertEquals("Movement Method Of Payment", "A", DataProvider.TransportCharges.First().MethodOfPayment);
			AssertSame("cached", DataProvider.TransportCharges, DataProvider.TransportCharges);
		});
	}

	public void TestTransportCharges_EmptyMethodOfPayment()
	{
		NctsHeader.MovementHeader.BM_MethodOfPayment = string.Empty;
		AssertEquals(0, DataProvider.TransportCharges.Count);
	}

	public void TestCountryOfDestination_V4() => CombineAssertions(() =>
	{
		var noCountry = ZString.Empty;
		var country1 = new ZString("C1");
		var country2 = new ZString("C2");

		var bill1 = NctsHeader.Bills.AddNew();
		var item11 = bill1.GoodsItems.AddNew();
		var item12 = bill1.GoodsItems.AddNew();
		var bill2 = NctsHeader.Bills.AddNew();
		var item21 = bill2.GoodsItems.AddNew();

		var previousDocument = NctsHeader.PreviousDocuments.AddNew();

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, true))
		{
			AssertCountryOfDestination(country1, country1, country1, noCountry, noCountry, country1);
			AssertCountryOfDestination(country1, country1, noCountry, country1, noCountry, country1);
			AssertCountryOfDestination(country1, country1, noCountry, noCountry, noCountry, country1);
			AssertCountryOfDestination(null, country1, country2, noCountry, noCountry, noCountry);
			AssertCountryOfDestination(null, country1, noCountry, country2, noCountry, noCountry);
			AssertCountryOfDestination(null, country1, noCountry, noCountry, noCountry, country2);
			AssertCountryOfDestination(null, noCountry, country1, noCountry, noCountry, country2);
			AssertCountryOfDestination(null, noCountry, noCountry, country1, country2, noCountry);
			AssertCountryOfDestination(null, country1, country1, country1, country1, country1, true);
			AssertCountryOfDestination(null, noCountry, noCountry, noCountry, noCountry, noCountry);
		}

		AssertSame("cached", DataProvider.CountryOfDestination, DataProvider.CountryOfDestination);

		void AssertCountryOfDestination(string expectedCountry, ZString headerCountry, ZString bill1Country, ZString item11Country, ZString item12Country, ZString bill2Country, bool isLinkedExport = false, [CallerLineNumber] int line = 0)
		{
			ResetDataProvider();
			NctsHeader.MovementHeader.BM_RL_NKDestinationPort = headerCountry;
			bill1.B0_RN_NKCountryOfDestination = bill1Country;
			item11.BY_RN_NKCountryOfDestination = item11Country;
			item12.BY_RN_NKCountryOfDestination = item12Country;
			bill2.B0_RN_NKCountryOfDestination = bill2Country;
			previousDocument.CSI_Code = isLinkedExport ? PreviousDocumentCodes.Export : string.Empty;
			AssertEquals($"[{line}] header({headerCountry}) bill1({bill1Country}) item11({item11Country}) item12({item12Country}) bill2({bill2Country}) isLinkedExport={isLinkedExport}", expectedCountry, DataProvider.CountryOfDestination);
		}
	});

	public void TestCountryOfDestination_V5() => CombineAssertions(() =>
	{
		var noCountry = ZString.Empty;
		var country1 = new ZString("C1");
		var country2 = new ZString("C2");

		var bill1 = NctsHeader.Bills.AddNew();
		var item11 = bill1.GoodsItems.AddNew();
		var item12 = bill1.GoodsItems.AddNew();
		var bill2 = NctsHeader.Bills.AddNew();
		bill2.GoodsItems.AddNew();

		var previousDocument = NctsHeader.PreviousDocuments.AddNew();

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, false))
		{
			AssertCountryOfDestination(country1, country1, country1, noCountry, noCountry, country1, "All Bill countries same - provided at Consignment level");
			AssertCountryOfDestination(country1, country1, noCountry, country1, noCountry, country1, "All Bill and GoodsItem countries same - provided at Consignment level");
			AssertCountryOfDestination(country1, country1, noCountry, noCountry, noCountry, country1, "Default Bill country from Header, all countries same - provided at Consignment level");
			AssertCountryOfDestination(null, country1, country2, noCountry, noCountry, noCountry, "Different Bill country - provided at HouseConsignment level");
			AssertCountryOfDestination(null, country1, noCountry, country2, noCountry, noCountry, "Different GoodsItem country - provided at ConsignmentItem level");
			AssertCountryOfDestination(null, noCountry, country1, noCountry, noCountry, country2, "No country on Header, different Bill countries - provided at HouseConsignment level");
			AssertCountryOfDestination(null, noCountry, noCountry, country1, country2, noCountry, "No country on Header, different GoodsItem countries - provided at ConsignmentItemLevel");
			AssertCountryOfDestination(null, country1, country1, country1, country1, country1, "All countries same, but linked export document", hasLinkedExportDocument: true);
			AssertCountryOfDestination(null, country1, country1, country1, country1, country1, "All countries same, but linked export header", hasLinkedExportEntryHeader: true);
			AssertCountryOfDestination(null, noCountry, noCountry, noCountry, noCountry, noCountry, "No country at all");
		}

		AssertSame("cached", DataProvider.CountryOfDestination, DataProvider.CountryOfDestination);

		void AssertCountryOfDestination(string expectedCountry, ZString headerCountry, ZString bill1Country, ZString item11Country, ZString item12Country, ZString bill2Country, string info, bool hasLinkedExportDocument = false, bool hasLinkedExportEntryHeader = false, [CallerLineNumber] int line = 0)
		{
			ResetDataProvider();
			NctsHeader.MovementHeader.BM_RL_NKDestinationPort = headerCountry;
			bill1.B0_RN_NKCountryOfDestination = bill1Country;
			item11.BY_RN_NKCountryOfDestination = item11Country;
			item12.BY_RN_NKCountryOfDestination = item12Country;
			bill2.B0_RN_NKCountryOfDestination = bill2Country;
			previousDocument.CSI_Code = hasLinkedExportDocument ? PreviousDocumentCodes.Export : string.Empty;
			if (hasLinkedExportEntryHeader)
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				jobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
				var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
				NctsHeader.MovementHeader.RelatedExportEntryHeaders.AddPivotFor(entryHeader);
			}
			AssertEquals($"[{line}] header({headerCountry}) bill1({bill1Country}) item11({item11Country}) item12({item12Country}) bill2({bill2Country}) hasPreviousDocumentEXPO={hasLinkedExportDocument} hasLinkedExportEntryHeader={hasLinkedExportEntryHeader} - {info}", expectedCountry, DataProvider.CountryOfDestination);
		}
	});

	public void TestCountryOfDispatch_V4() => CombineAssertions(() =>
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, true))
		{
			NctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch = ZString.Empty;
			AssertNull("null when empty", DataProvider.CountryOfDispatch);
			ResetDataProvider();
			NctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch = "AL";
			AssertEquals("mapped to NctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch", "AL", DataProvider.CountryOfDispatch);
			AssertSame("cached", DataProvider.CountryOfDispatch, DataProvider.CountryOfDispatch);
		}
	});

	public void TestCountryOfDispatch_V5()
	{
		var noCountry = ZString.Empty;
		var country1 = new ZString("C1");
		var country2 = new ZString("C2");

		var bill1 = NctsHeader.Bills.AddNew();
		var item11 = bill1.GoodsItems.AddNew();
		var item12 = bill1.GoodsItems.AddNew();
		var bill2 = NctsHeader.Bills.AddNew();
		bill2.GoodsItems.AddNew();

		var previousDocument = NctsHeader.PreviousDocuments.AddNew();
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, false))
		{
			AssertCountryOfDispatch(country1, country1, country1, noCountry, noCountry, country1, "All Bill countries same - provided at Consignment level");
			AssertCountryOfDispatch(country1, country1, noCountry, country1, noCountry, country1, "All Bill and GoodsItem countries same - provided at Consignment level");
			AssertCountryOfDispatch(country1, country1, noCountry, noCountry, noCountry, country1, "Default Bill country from Header, all countries same - provided at Consignment level");
			AssertCountryOfDispatch(null, country1, country2, noCountry, noCountry, noCountry, "Different Bill country - provided at HouseConsignment level");
			AssertCountryOfDispatch(null, country1, noCountry, country2, noCountry, noCountry, "Different GoodsItem country - provided at ConsignmentItem level");
			AssertCountryOfDispatch(null, noCountry, country1, noCountry, noCountry, country2, "No country on Header, different Bill countries - provided at HouseConsignment level");
			AssertCountryOfDispatch(null, noCountry, noCountry, country1, country2, noCountry, "No country on Header, different GoodsItem countries - provided at ConsignmentItemLevel");
			AssertCountryOfDispatch(null, noCountry, noCountry, noCountry, noCountry, noCountry, "No country at all");

			AssertCountryOfDispatch(country1, country1, noCountry, country1, noCountry, country1, "[With linked Export Document] All Bill and GoodsItem countries same - provided at Consignment level (optional)", hasLinkedExportDocument: true);
			AssertCountryOfDispatch(null, noCountry, noCountry, country1, noCountry, country1, "[With linked Export Document] All Bill and GoodsItem countries same - empty Consignment level (optional)", hasLinkedExportDocument: true);
			AssertCountryOfDispatch(country1, country1, noCountry, country1, noCountry, country1, "[With linked Export Entry Header] All Bill and GoodsItem countries same - provided at Consignment level (optional)", hasLinkedExportEntryHeader: true);
			AssertCountryOfDispatch(null, noCountry, noCountry, country1, noCountry, country1, "[With linked Export Entry Header] All Bill and GoodsItem countries same - empty Consignment level (optional)", hasLinkedExportEntryHeader: true);

			AssertCountryOfDispatch(country1, country1, noCountry, noCountry, noCountry, noCountry, "[With linked Export Document] optional country on Header - provided at Consignment level", hasLinkedExportDocument: true);
			AssertCountryOfDispatch(null, noCountry, noCountry, noCountry, noCountry, noCountry, "[With linked Export Document] emtpy country on Header - empty Consignment level", hasLinkedExportDocument: true);
			AssertCountryOfDispatch(country1, country1, noCountry, noCountry, noCountry, noCountry, "[With linked Export Entry Header] optional country on Header - provided at Consignment level", hasLinkedExportEntryHeader: true);
			AssertCountryOfDispatch(null, noCountry, noCountry, noCountry, noCountry, noCountry, "[With linked Entry Header] emtpy country on Header - empty Consignment level", hasLinkedExportEntryHeader: true);
		}

		AssertSame("cached", DataProvider.CountryOfDispatch, DataProvider.CountryOfDispatch);

		void AssertCountryOfDispatch(string expectedCountry, ZString headerCountry, ZString bill1Country, ZString item11Country, ZString item12Country, ZString bill2Country, string info, bool hasLinkedExportDocument = false, bool hasLinkedExportEntryHeader = false, [CallerLineNumber] int line = 0)
		{
			ResetDataProvider();
			NctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch = headerCountry;
			bill1.B0_RN_NKCountryOfExport = bill1Country;
			item11.BY_RN_NKCountryOfDispatch = item11Country;
			item12.BY_RN_NKCountryOfDispatch = item12Country;
			bill2.B0_RN_NKCountryOfExport = bill2Country;
			previousDocument.CSI_Code = hasLinkedExportDocument ? PreviousDocumentCodes.Export : string.Empty;
			if (hasLinkedExportEntryHeader)
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				jobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
				var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
				NctsHeader.MovementHeader.RelatedExportEntryHeaders.AddPivotFor(entryHeader);
			}
			AssertEquals($"[{line}] header({headerCountry}) bill1({bill1Country}) item11({item11Country}) item12({item12Country}) bill2({bill2Country}) hasPreviousDocumentEXPO={hasLinkedExportDocument} hasLinkedExportEntryHeader={hasLinkedExportEntryHeader} - {info}", expectedCountry, DataProvider.CountryOfDispatch);
		}
	}

	protected override ConsignmentDataProvider CreateDataProvider() => ConsignmentDataProvider.New(NctsHeader);
}
