using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.CH.NCTS.Business.UniversalReferenceConstants;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(ConsignmentItemDataProvider))]
sealed class ConsignmentItemDataProviderTest : BaseDepartureDataProviderTest<ConsignmentItemDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNewCollection()
	{
		AssertNull(ConsignmentItemDataProvider.NewCollection(null));
	}

	public void TestProvider() => CombineAssertions(() =>
	{
		DepartureMovementHeader.BM_UniqueConsignmentReference = string.Empty;
		NctsBill.B0_ReferenceID = string.Empty;

		DepartureGoodsItem.BY_LineNo = 1;
		DepartureGoodsItem.BY_DeclarationGoodsItemNumber = 456;
		DepartureGoodsItem.BY_RN_NKCountryOfDispatch = "DI";
		DepartureGoodsItem.BY_RN_NKCountryOfDestination = "DE";
		DepartureGoodsItem.BY_CommercialReferenceNumber = "U123";
		DepartureGoodsItem.BY_Type = "T2345";

		var anotherItemWithAnotherDestination = NctsBill.GoodsItems.AddNew();
		anotherItemWithAnotherDestination.BY_RN_NKCountryOfDestination = "FR";

		AssertEquals("GoodsItemNumber", 1, DataProvider.GoodsItemNumber);
		AssertEquals("DeclarationGoodsItemNumber", 456, DataProvider.DeclarationGoodsItemNumber);
		AssertEquals("CountryOfDispatch", "DI", DataProvider.CountryOfDispatch);
		AssertEquals("CountryOfDestination", "DE", DataProvider.CountryOfDestination);
		AssertEquals("ReferenceNumberUCR", "U123", DataProvider.ReferenceNumberUCR);
		AssertEquals("DeclarationType", "T2345", DataProvider.DeclarationType);
	});

	public void TestSequenceNumberAndOrder() => CombineAssertions(() =>
	{
		var nctsBill = Factory.New<NctsBill>();

		AddNewGoodsItem(3);
		AddNewGoodsItem(1);
		AddNewGoodsItem(2);

		var dataProviders = ConsignmentItemDataProvider.NewCollection(nctsBill.GoodsItems);

		var expectedGoodsItemNumber = 1;
		foreach (var consignmentItem in dataProviders)
		{
			AssertEquals($"GoodsItem {expectedGoodsItemNumber} GoodsItemNumber", expectedGoodsItemNumber, consignmentItem.GoodsItemNumber);
			AssertEquals($"GoodsItem {expectedGoodsItemNumber} DescriptionOfGoods", expectedGoodsItemNumber.ToString(), consignmentItem.Commodity.DescriptionOfGoods);
			expectedGoodsItemNumber++;
		}

		void AddNewGoodsItem(ZShort lineNo)
		{
			var goodsItem = nctsBill.GoodsItems.AddNew();
			goodsItem.BY_LineNo = lineNo;
			goodsItem.BY_Description = lineNo.ToString();
		}
	});

	public void TestCountryOfDispatch_V4()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, true))
		{
			DepartureGoodsItem.BY_RN_NKCountryOfDispatch = ZString.Empty;
			AssertEquals("null when empty", null, DataProvider.CountryOfDispatch);
			ResetDataProvider();
			DepartureGoodsItem.BY_RN_NKCountryOfDispatch = "AL";
			AssertEquals("mapped to DepartureGoodsItem.BY_RN_NKCountryOfDispatch", "AL", DataProvider.CountryOfDispatch);
			AssertSame("cached", DataProvider.CountryOfDispatch, DataProvider.CountryOfDispatch);
		}
	}

	public void TestCountryOfDispatch_V5()
	{
		var noCountry = ZString.Empty;
		var country1 = new ZString("C1");
		var country2 = new ZString("C2");

		var bill1 = DepartureGoodsItem.Bill;
		var item11 = DepartureGoodsItem;
		var item12 = bill1.GoodsItems.AddNew();
		var bill2 = NctsHeader.Bills.AddNew();

		NctsHeader.PreviousDocuments.AddNew();

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, false))
		{
			AssertCountryOfDispatch(null, country1, country1, noCountry, noCountry, country1, "Default from Bill, all countries same - provided at Consignment level");
			AssertCountryOfDispatch(null, country1, noCountry, country1, noCountry, country1, "Country on GoodsItem, all countries same - provided at Consignment level");
			AssertCountryOfDispatch(null, country1, noCountry, noCountry, noCountry, country1, "Default from Header, all countries same - provided at Consignment level");
			AssertCountryOfDispatch(null, country1, country2, noCountry, noCountry, country2, "Default from Bill, all GoodsItem countries same - provided at HouseConsignment level");
			AssertCountryOfDispatch(country2, country1, noCountry, country2, noCountry, noCountry, "Other country at GodsItem - provided at ConsignmentItem level");
			AssertCountryOfDispatch(null, country1, noCountry, noCountry, noCountry, country2, "Default from Header, all of countries Bill same - provided at HouseConsignment level");
			AssertCountryOfDispatch(null, noCountry, country1, noCountry, noCountry, country2, "Default from Bill, all countries of Bill same - provided at HouseConsignment level");
			AssertCountryOfDispatch(country1, noCountry, noCountry, country1, country2, noCountry, "Country on GoodsItem, different country on other GoodsItem - provided at ConsignmentItem level");
			AssertCountryOfDispatch(null, noCountry, noCountry, noCountry, noCountry, noCountry, "No country at all");
		}

		AssertSame("cached", DataProvider.CountryOfDestination, DataProvider.CountryOfDestination);

		void AssertCountryOfDispatch(string expectedCountry, ZString headerCountry, ZString bill1Country, ZString item11Country, ZString item12Country, ZString bill2Country, string info, [CallerLineNumber] int line = 0)
		{
			ResetDataProvider();
			NctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch = headerCountry;
			bill1.B0_RN_NKCountryOfExport = bill1Country;
			item11.BY_RN_NKCountryOfDispatch = item11Country;
			item12.BY_RN_NKCountryOfDispatch = item12Country;
			bill2.B0_RN_NKCountryOfExport = bill2Country;
			AssertEquals($"[{line}] header({headerCountry}) bill1({bill1Country}) item11({item11Country}) item12({item12Country}) bill2({bill2Country}) - {info}", expectedCountry, DataProvider.CountryOfDispatch);
		}
	}

	public void TestCountryOfDestination_V4() => CombineAssertions(() =>
	{
		var noCountry = ZString.Empty;
		var country1 = new ZString("C1");
		var country2 = new ZString("C2");

		var bill1 = DepartureGoodsItem.Bill;
		var item11 = DepartureGoodsItem;
		var item12 = bill1.GoodsItems.AddNew();
		var bill2 = NctsHeader.Bills.AddNew();

		var previousDocument = NctsHeader.PreviousDocuments.AddNew();

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, true))
		{
			AssertCountryOfDestination(null, country1, country1, noCountry, noCountry, country1);
			AssertCountryOfDestination(null, country1, noCountry, country1, noCountry, country1);
			AssertCountryOfDestination(null, country1, noCountry, noCountry, noCountry, country1);
			AssertCountryOfDestination(null, country1, country2, noCountry, noCountry, country2);
			AssertCountryOfDestination(country2, country1, noCountry, country2, noCountry, noCountry);
			AssertCountryOfDestination(null, country1, noCountry, noCountry, noCountry, country2);
			AssertCountryOfDestination(null, noCountry, country1, noCountry, noCountry, country2);
			AssertCountryOfDestination(country1, noCountry, noCountry, country1, country2, noCountry);
			AssertCountryOfDestination(country1, country1, country1, country1, country1, country1, true);
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

		var bill1 = DepartureGoodsItem.Bill;
		var item11 = DepartureGoodsItem;
		var item12 = bill1.GoodsItems.AddNew();
		var bill2 = NctsHeader.Bills.AddNew();

		var previousDocument = NctsHeader.PreviousDocuments.AddNew();

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, false))
		{
			AssertCountryOfDestination(null, country1, country1, noCountry, noCountry, country1, "Default from Bill, all countries same - provided at Consignment level");
			AssertCountryOfDestination(null, country1, noCountry, country1, noCountry, country1, "Country on GoodsItem, all countries same - provided at Consignment level");
			AssertCountryOfDestination(null, country1, noCountry, noCountry, noCountry, country1, "Default from Header, all countries same - provided at Consignment level");
			AssertCountryOfDestination(null, country1, country2, noCountry, noCountry, country2, "Default from Bill, all GoodsItem countries same - provided at HouseConsignment level");
			AssertCountryOfDestination(country2, country1, noCountry, country2, noCountry, noCountry, "Other country at GodsItem - provided at ConsignmentItem level");
			AssertCountryOfDestination(null, country1, noCountry, noCountry, noCountry, country2, "Default from Header, all of countries Bill same - provided at HouseConsignment level");
			AssertCountryOfDestination(null, noCountry, country1, noCountry, noCountry, country2, "Default from Bill, all countries of Bill same - provided at HouseConsignment level");
			AssertCountryOfDestination(country1, noCountry, noCountry, country1, country2, noCountry, "Country on GoodsItem, different country on other GoodsItem - provided at ConsignmentItem level");
			AssertCountryOfDestination(null, noCountry, noCountry, noCountry, noCountry, noCountry, "No country at all");
		}

		AssertSame("cached", DataProvider.CountryOfDestination, DataProvider.CountryOfDestination);

		void AssertCountryOfDestination(string expectedCountry, ZString headerCountry, ZString bill1Country, ZString item11Country, ZString item12Country, ZString bill2Country, string info, [CallerLineNumber] int line = 0)
		{
			ResetDataProvider();
			NctsHeader.MovementHeader.BM_RL_NKDestinationPort = headerCountry;
			bill1.B0_RN_NKCountryOfDestination = bill1Country;
			item11.BY_RN_NKCountryOfDestination = item11Country;
			item12.BY_RN_NKCountryOfDestination = item12Country;
			bill2.B0_RN_NKCountryOfDestination = bill2Country;
			AssertEquals($"[{line}] header({headerCountry}) bill1({bill1Country}) item11({item11Country}) item12({item12Country}) bill2({bill2Country}) - {info}", expectedCountry, DataProvider.CountryOfDestination);
		}
	});

	public void TestReferenceNumberUCR()
	{
		DepartureGoodsItem.BY_CommercialReferenceNumber = "U123";

		NctsBill.B0_ReferenceID = "U321";
		DepartureMovementHeader.BM_UniqueConsignmentReference = string.Empty;
		AssertNull("null when B0_ReferenceID is not empty", DataProvider.ReferenceNumberUCR);

		DepartureMovementHeader.BM_UniqueConsignmentReference = "U456";
		AssertNull("null when both BM_UniqueConsignmentReference and B0_ReferenceID are not empty", DataProvider.ReferenceNumberUCR);

		NctsBill.B0_ReferenceID = string.Empty;
		AssertNull("null when BM_UniqueConsignmentReference is not empty", DataProvider.ReferenceNumberUCR);

		DepartureMovementHeader.BM_UniqueConsignmentReference = string.Empty;
		AssertEquals("has a value when both BM_UniqueConsignmentReference and B0_ReferenceID are empty", "U123", DataProvider.ReferenceNumberUCR);

		DepartureGoodsItem.BY_CommercialReferenceNumber = ZString.Empty;
		AssertNull("null when empty", DataProvider.ReferenceNumberUCR);
	}

	public void TestCommodity()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.Commodity);
			AssertSame("cached", DataProvider.Commodity, DataProvider.Commodity);
		});
	}

	public void TestPackages()
	{
		AssertEquals("Precondition", 0, DataProvider.Packagings.Count);

		DepartureGoodsItem.Packages.AddNew();
		DepartureGoodsItem.Packages.AddNew();

		var consignmentDataProvider = CreateDataProvider();

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, consignmentDataProvider.Packagings.Count);
			AssertEquals("Sequence at 1 index", 1, consignmentDataProvider.Packagings.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence at 2 index", 2, consignmentDataProvider.Packagings.ElementAt(1).SequenceNumber);
			Assert("Type", consignmentDataProvider.Packagings.All(p => p is ConsignmentItemPackagingDataProvider));
			AssertSame("Cached", consignmentDataProvider.Packagings, consignmentDataProvider.Packagings);
		});
	}

	public void TestPreviousDocuments()
	{
		AssertEquals("Precondition", 0, DataProvider.PreviousDocuments.Count);

		DepartureGoodsItem.PreviousDocuments.AddNew();
		DepartureGoodsItem.PreviousDocuments.AddNew();

		var consignmentDataProvider = CreateDataProvider();

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, consignmentDataProvider.PreviousDocuments.Count);
			AssertEquals("Sequence at 1 index", 1, consignmentDataProvider.PreviousDocuments.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence at 2 index", 2, consignmentDataProvider.PreviousDocuments.ElementAt(1).SequenceNumber);
			Assert("Type", consignmentDataProvider.PreviousDocuments.All(p => p is DocumentDataProvider));
			AssertSame("Cached", consignmentDataProvider.PreviousDocuments, consignmentDataProvider.PreviousDocuments);
		});
	}

	public void TestSupportingDocuments()
	{
		AssertEquals("Precondition", 0, DataProvider.SupportingDocuments.Count);

		DepartureGoodsItem.SupportingDocuments.AddNew();
		DepartureGoodsItem.SupportingDocuments.AddNew();

		var consignmentDataProvider = CreateDataProvider();

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, consignmentDataProvider.SupportingDocuments.Count);
			AssertEquals("Sequence at 1 index", 1, consignmentDataProvider.SupportingDocuments.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence at 2 index", 2, consignmentDataProvider.SupportingDocuments.ElementAt(1).SequenceNumber);
			Assert("Type", consignmentDataProvider.SupportingDocuments.All(p => p is DocumentDataProvider));
			AssertSame("Cached", consignmentDataProvider.SupportingDocuments, consignmentDataProvider.SupportingDocuments);
		});
	}

	public void TestAdditionalInformation()
	{
		AssertEquals("Precondition", 0, DataProvider.AdditionalInformations.Count);

		DepartureGoodsItem.AdditionalInfos.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		DepartureGoodsItem.AdditionalInfos.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		DepartureGoodsItem.AdditionalInfos.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		DepartureGoodsItem.AdditionalInfos.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

		var consignmentDataProvider = CreateDataProvider();

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, consignmentDataProvider.AdditionalInformations.Count);
			AssertEquals("Sequence at 1 index", 1, consignmentDataProvider.AdditionalInformations.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence at 2 index", 2, consignmentDataProvider.AdditionalInformations.ElementAt(1).SequenceNumber);
			Assert("Type", consignmentDataProvider.AdditionalInformations.All(p => p is AdditionalInformationDataProvider));
			AssertSame("Cached", consignmentDataProvider.AdditionalInformations, consignmentDataProvider.AdditionalInformations);
		});
	}

	public void TestAdditionalReferences()
	{
		AssertEquals("Precondition", 0, DataProvider.AdditionalInformations.Count);

		DepartureGoodsItem.AdditionalInfos.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		DepartureGoodsItem.AdditionalInfos.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		DepartureGoodsItem.AdditionalInfos.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		DepartureGoodsItem.AdditionalInfos.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

		var consignmentDataProvider = CreateDataProvider();

		CombineAssertions(() =>
		{
			AssertEquals("Count", 2, consignmentDataProvider.AdditionalReferences.Count);
			AssertEquals("Sequence at 1 index", 1, consignmentDataProvider.AdditionalReferences.ElementAt(0).SequenceNumber);
			AssertEquals("Sequence at 2 index", 2, consignmentDataProvider.AdditionalReferences.ElementAt(1).SequenceNumber);
			Assert("Type", consignmentDataProvider.AdditionalReferences.All(p => p is AdditionalReferenceDataProvider));
			AssertSame("Cached", consignmentDataProvider.AdditionalReferences, consignmentDataProvider.AdditionalReferences);
		});
	}

	public void TestAdditionalSupplyChainActor() => CombineAssertions(() =>
	{
		AssertNotNull(DataProvider.AdditionalSupplyChainActors);
		AssertSame("cached", DataProvider.AdditionalSupplyChainActors, DataProvider.AdditionalSupplyChainActors);
		ResetDataProvider();

		DepartureGoodsItem.CusSupplyChainActorReferences.AddNew();
		DepartureGoodsItem.CusSupplyChainActorReferences.AddNew();
		ResetDataProvider();
		AssertEquals("count", 2, DataProvider.AdditionalSupplyChainActors.Count);

		NctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		ResetDataProvider();
		AssertNull("No Additional Supply Chain Actors for T-CH", DataProvider.AdditionalSupplyChainActors);
	});

	public void TestUnusedProperties() => CombineAssertions(() =>
	{
		AssertNull("Unused Property Refinement", DataProvider.Refinement);
		AssertNull("Unused Property Refund", DataProvider.Refund);
		AssertNull("Unused Property Consignee", DataProvider.Consignee);
		AssertNull("Unused Property TransportDocuments", DataProvider.TransportDocuments);
		AssertNull("Unused Property TransportCharges", DataProvider.TransportCharges);
		AssertEquals("Unused Property isReleased", false, DataProvider.IsReleased);
		AssertEquals("Unused Property isBlocked", false, DataProvider.IsBlocked);
	});

	protected override ConsignmentItemDataProvider CreateDataProvider() => ConsignmentItemDataProvider.NewCollection(NctsBill.GoodsItems).First();
}
