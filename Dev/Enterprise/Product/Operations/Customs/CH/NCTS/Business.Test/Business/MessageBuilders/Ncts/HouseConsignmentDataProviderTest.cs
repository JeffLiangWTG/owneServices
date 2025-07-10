using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.NCTS.Business.UniversalReferenceConstants;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(HouseConsignmentDataProvider))]
sealed class HouseConsignmentDataProviderTest : BaseDepartureDataProviderTest<HouseConsignmentDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNewCollection()
	{
		AssertNull("null", HouseConsignmentDataProvider.NewCollection(null));
	}

	public void TestProvider()
	{
		DepartureMovementHeader.BM_UniqueConsignmentReference = string.Empty;
		NctsBill.B0_ReferenceID = "UCR-REF";
		AddAddress("CRD", "-consignor-");
		AddAddress("CEA", "-consignee-");
		CombineAssertions(() =>
		{
			AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);
			AssertEquals("ReferenceNumberUCR", "UCR-REF", DataProvider.ReferenceNumberUCR);
			AssertEquals("Consignor", "-consignor-", DataProvider.Consignor.Name);
			AssertEquals("Consignee", "-consignee-", DataProvider.Consignee.Name);
		});
	}

	public void TestSequenceNumberAndOrder() => CombineAssertions(() =>
	{
		AddNewBill(3);
		AddNewBill(1);
		AddNewBill(2);

		var dataProviders = HouseConsignmentDataProvider.NewCollection(NctsHeader.Bills);

		var expectedSequenceNumber = 1;
		foreach (var houseConsignment in dataProviders)
		{
			AssertEquals($"Bill {expectedSequenceNumber} SequenceNumber", expectedSequenceNumber, houseConsignment.SequenceNumber);
			AssertEquals($"Bill {expectedSequenceNumber} ReferenceNumberUCR", expectedSequenceNumber.ToString(), houseConsignment.ReferenceNumberUCR);
			expectedSequenceNumber++;
		}

		void AddNewBill(ZShort sequenceNumber)
		{
			var bill = NctsHeader.Bills.AddNew();
			bill.SequenceNumber = sequenceNumber;
			bill.B0_ReferenceID = sequenceNumber.ToString();
		}
	});

	public void TestConsignor()
	{
		var consignor = OrgHeader.New(Factory);
		consignor.OH_FullName = "Consignor";
		NctsBill.Consignor.OrganisationPK = consignor.PK;
		var contact = consignor.Contacts.AddNew();
		contact.OC_Email = "cus@example.org";
		contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.Consignor);
			AssertEquals("Consignor", "Consignor", DataProvider.Consignor.Name);
			AssertEquals("Allocated contact of type CUS expected", "cus@example.org", DataProvider.Consignor.ContactPerson.EmailAddress);
			AssertSame("cached", DataProvider.Consignor, DataProvider.Consignor);

			AssertParticipentIdentificationNumberOrNameAddress("Consignor", NctsBill.Consignor, () => DataProvider.Consignor, OrgCusCode.SwissCodeTypes.BID);
		});
	}

	public void TestConsignee()
	{
		var consignee = OrgHeader.New(Factory);
		consignee.OH_FullName = "Consignee";
		NctsBill.Consignee.OrganisationPK = consignee.PK;

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.Consignee);
			AssertSame("cached", DataProvider.Consignee, DataProvider.Consignee);

			AssertParticipentIdentificationNumberOrNameAddress("Consignee", NctsBill.Consignee, () => DataProvider.Consignee, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
		});
	}

	public void TestReferenceUCR()
	{
		NctsBill.B0_ReferenceID = "UCR-REF";
		DepartureMovementHeader.BM_UniqueConsignmentReference = "UCR-REF";
		AssertNull("null when BM_UniqueConsignmentReference is not emtpy", DataProvider.ReferenceNumberUCR);

		DepartureMovementHeader.BM_UniqueConsignmentReference = string.Empty;
		AssertEquals("has value when BM_UniqueConsignmentReference is emtpy", "UCR-REF", DataProvider.ReferenceNumberUCR);

		NctsBill.B0_ReferenceID = ZString.Empty;
		AssertNull("null when empty", DataProvider.ReferenceNumberUCR);
	}

	public void TestCountryOfDispatch_V4()
	{
		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, true))
		{
			NctsBill.B0_RN_NKCountryOfExport = ZString.Empty;
			AssertNull("null when empty", DataProvider.CountryOfDispatch);
			ResetDataProvider();
			NctsBill.B0_RN_NKCountryOfExport = "AL";
			AssertEquals("mapped to NctsBill.B0_RN_NKCountryOfExport ", "AL", DataProvider.CountryOfDispatch);
			AssertSame("cached", DataProvider.CountryOfDispatch, DataProvider.CountryOfDispatch);
		}
	}

	public void TestCountryOfDispatch_V5() => CombineAssertions(() =>
	{
		var noCountry = ZString.Empty;
		var country1 = new ZString("C1");
		var country2 = new ZString("C2");

		var goodsItem1 = DepartureGoodsItem;
		var goodsItem2 = NctsBill.GoodsItems.AddNew();
		var otherBill = NctsHeader.Bills.AddNew();
		var otherBillItem = otherBill.GoodsItems.AddNew();
		var previousDocument = NctsHeader.PreviousDocuments.AddNew();

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, false))
		{
			AssertCountryOfDispatch(null, country1, noCountry, noCountry, noCountry, "All countries same - provided at Consignment level");
			AssertCountryOfDispatch(null, noCountry, noCountry, country1, country2, "Different GoodsItems countries provided at ConsignmentItem level");
			AssertCountryOfDispatch(country1, noCountry, noCountry, country1, country1, "All GoodsItems same - provided at HouseConsignment level", otherBillCountry: country2);
			AssertCountryOfDispatch(country1, noCountry, country1, country1, noCountry, "All GoodsItems same as Bill or empty - provided at HouseConsignment level");
			AssertCountryOfDispatch(null, noCountry, country1, noCountry, country2, "All GoodsItems not same - provided at ConsignmentItem level");
			AssertCountryOfDispatch(null, noCountry, country1, noCountry, noCountry, "All Bill same - provided at Consignment level", otherBillCountry: country1);
			AssertCountryOfDispatch(country1, country1, noCountry, noCountry, noCountry, "All Bill not same - provided at HouseConsignment level", otherBillCountry: country2);

			AssertCountryOfDispatch(country1, noCountry, country1, noCountry, country1, "All GoodsItems and one foreign Bill same - provided at HouseConsignment level with export document", otherBillCountry: country1, hasLinkedExportDocument: true);
			AssertCountryOfDispatch(country1, noCountry, country1, noCountry, country1, "All GoodsItems and one foreign Bill same - provided at HouseConsignment level with export header", otherBillCountry: country1, hasLinkedExportEntryHeader: true);
		}

		AssertSame("cached", DataProvider.CountryOfDispatch, DataProvider.CountryOfDispatch);

		void AssertCountryOfDispatch(string expectedCountry, string headerCountry, string billCountry, string item1Country, string item2Country, string info, string otherBillCountry = null, bool hasLinkedExportDocument = false, bool hasLinkedExportEntryHeader = false, [CallerLineNumber] int line = 0)
		{
			ResetDataProvider();
			DepartureMovementHeader.BM_RN_NKCountryOfDispatch = headerCountry;
			NctsBill.B0_RN_NKCountryOfExport = billCountry;
			goodsItem1.BY_RN_NKCountryOfDispatch = item1Country;
			goodsItem2.BY_RN_NKCountryOfDispatch = item2Country;
			otherBill.B0_RN_NKCountryOfExport = otherBillCountry;
			previousDocument.CSI_Code = hasLinkedExportDocument ? PreviousDocumentCodes.Export : string.Empty;
			if (hasLinkedExportEntryHeader)
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				jobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
				var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
				NctsHeader.MovementHeader.RelatedExportEntryHeaders.AddPivotFor(entryHeader);
			}
			AssertEquals($"[{line}] MovementCountry={headerCountry} BillCountry={billCountry} GoodsItemCountries={item1Country},{item2Country} OtherBillCountry={otherBillCountry} hasPreviousDocumentEXPO={hasLinkedExportDocument} hasLinkesExportEntryHeader={hasLinkedExportEntryHeader} - {info}", expectedCountry, DataProvider.CountryOfDispatch);
		}
	});

	public void TestCountryOfDestination() => CombineAssertions(() =>
	{
		var noCountry = ZString.Empty;
		var country1 = new ZString("C1");
		var country2 = new ZString("C2");

		var goodsItem1 = DepartureGoodsItem;
		var goodsItem2 = NctsBill.GoodsItems.AddNew();
		var otherBill = NctsHeader.Bills.AddNew();
		var otherBillItem = otherBill.GoodsItems.AddNew();
		var previousDocument = NctsHeader.PreviousDocuments.AddNew();

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.CHNT015V4, Core.Constants.CountryCodes.Switzerland, ZDate.Today, false))
		{
			AssertCountryOfDestination(null, country1, noCountry, noCountry, noCountry, "All countries same - provided at Consignment level");
			AssertCountryOfDestination(null, noCountry, noCountry, country1, country2, "Different GoodsItems countries provided at ConsignmentItem level");
			AssertCountryOfDestination(country1, noCountry, noCountry, country1, country1, "All GoodsItems same - provided at HouseConsignment level", otherBillCountry: country2);
			AssertCountryOfDestination(country1, noCountry, country1, country1, noCountry, "All GoodsItems same as Bill or empty - provided at HouseConsignment level");
			AssertCountryOfDestination(null, noCountry, country1, noCountry, country2, "All GoodsItems not same - provided at ConsignmentItem level");
			AssertCountryOfDestination(null, noCountry, country1, noCountry, noCountry, "All Bill same - provided at Consignment level", otherBillCountry: country1);
			AssertCountryOfDestination(country1, country1, noCountry, noCountry, noCountry, "All Bill not same - provided at HouseConsignment level", otherBillCountry: country2);
			AssertCountryOfDestination(country1, noCountry, country1, noCountry, noCountry, "All Bill same, but linked export document - provided at HouseConsignment level", otherBillCountry: country1, hasLinkedExportDocument: true);
			AssertCountryOfDestination(country1, noCountry, country1, noCountry, noCountry, "All Bill same, linked export header - provided at HouseConsignment level", otherBillCountry: country1, hasLinkedExportEntryHeader: true);
			AssertCountryOfDestination(country1, country1, noCountry, noCountry, noCountry, "Country only on Movement - provided at Consignment level");
			AssertCountryOfDestination(country1, country1, noCountry, noCountry, noCountry, "Country only on Movement, but linked export document - provided at HouseConsignment level", hasLinkedExportDocument: true);
			AssertCountryOfDestination(country1, country1, noCountry, noCountry, noCountry, "Country only on Movement, but linked export header - provided at HouseConsignment level", hasLinkedExportEntryHeader: true);
		}

		AssertSame("cached", DataProvider.CountryOfDestination, DataProvider.CountryOfDestination);

		void AssertCountryOfDestination(string expectedCountry, string headerCountry, string billCountry, string item1Country, string item2Country, string info, string otherBillCountry = null, bool hasLinkedExportDocument = false, bool hasLinkedExportEntryHeader = false, [CallerLineNumber] int line = 0)
		{
			ResetDataProvider();
			DepartureMovementHeader.BM_RL_NKDestinationPort = headerCountry;
			NctsBill.B0_RN_NKCountryOfDestination = billCountry;
			goodsItem1.BY_RN_NKCountryOfDestination = item1Country;
			goodsItem2.BY_RN_NKCountryOfDestination = item2Country;
			otherBill.B0_RN_NKCountryOfDestination = otherBillCountry;
			previousDocument.CSI_Code = hasLinkedExportDocument ? PreviousDocumentCodes.Export : string.Empty;
			if (hasLinkedExportEntryHeader)
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				jobDeclaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
				var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
				NctsHeader.MovementHeader.RelatedExportEntryHeaders.AddPivotFor(entryHeader);
			}
			AssertEquals($"[{line}] MovementCountry={headerCountry} BillCountry={billCountry} GoodsItemCountries={item1Country},{item2Country} OtherBillCountry={otherBillCountry} hasPreviousDocumentEXPO={hasLinkedExportDocument} hasLinkesExportEntryHeader={hasLinkedExportEntryHeader} - {info}", expectedCountry, DataProvider.CountryOfDestination);
		}
	});

	public void TestConsignmentItem()
	{
		NctsBill.GoodsItems.AddNew();
		NctsBill.GoodsItems.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("count", 3, DataProvider.ConsignmentItems.Count);
			AssertSame("cached", DataProvider.ConsignmentItems, DataProvider.ConsignmentItems);
		});
	}

	public void TestPreviousDocument()
	{
		NctsBill.PreviousDocuments.AddNew();
		NctsBill.PreviousDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.PreviousDocuments);
			AssertEquals("count", 2, DataProvider.PreviousDocuments.Count);
			Assert("Type", DataProvider.PreviousDocuments.All(p => p is DocumentDataProvider));
			AssertSame("cached", DataProvider.PreviousDocuments, DataProvider.PreviousDocuments);
		});
	}

	public void TestSupportingDocument()
	{
		NctsBill.SupportingDocuments.AddNew();
		NctsBill.SupportingDocuments.AddNew();

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.SupportingDocuments);
			AssertEquals("count", 2, DataProvider.SupportingDocuments.Count);
			Assert("Type", DataProvider.SupportingDocuments.All(p => p is DocumentDataProvider));
			AssertSame("cached", DataProvider.SupportingDocuments, DataProvider.SupportingDocuments);
		});
	}

	public void TestTransportDocument()
	{
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.TransportDocuments);
			AssertEquals("count", 2, DataProvider.TransportDocuments.Count);
			Assert("Type", DataProvider.TransportDocuments.All(p => p is TransportDocumentDataProvider));
			AssertSame("cached", DataProvider.TransportDocuments, DataProvider.TransportDocuments);
		});
	}

	public void TestAdditionalInformation()
	{
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.AdditionalInformations);
			AssertEquals("count", 2, DataProvider.AdditionalInformations.Count);
			Assert("Type", DataProvider.AdditionalInformations.All(p => p is AdditionalInformationDataProvider));
			AssertSame("cached", DataProvider.AdditionalInformations, DataProvider.AdditionalInformations);
		});
	}

	public void TestAdditionalReference()
	{
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		NctsBill.AdditionalDocuments.AddNew().CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.AdditionalReferences);
			AssertEquals("count", 2, DataProvider.AdditionalReferences.Count);
			Assert("Type", DataProvider.AdditionalReferences.All(p => p is AdditionalReferenceDataProvider));
			AssertSame("cached", DataProvider.AdditionalReferences, DataProvider.AdditionalReferences);
		});
	}

	void AddAddress(string addressType, string fullname)
	{
		var address = NctsBill.DocAddresses.AddNew(DocAddressTypes.GetDocAddressTypeFromCode(Factory, addressType));
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = fullname;
		address.OrganisationPK = orgHeader.PK;
	}

	protected override HouseConsignmentDataProvider CreateDataProvider() => HouseConsignmentDataProvider.NewCollection(NctsHeader.Bills).First();
}
