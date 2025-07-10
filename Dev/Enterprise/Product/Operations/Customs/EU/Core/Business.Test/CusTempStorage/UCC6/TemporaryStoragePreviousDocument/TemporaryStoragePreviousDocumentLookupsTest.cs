using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	class TemporaryStoragePreviousDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList_ParentIsTemporaryStorageHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				RefCusCodeList codeLists1, codeLists2, codeLists3, codeLists4;
				GenerateCodeList(out codeLists1, out codeLists2, out codeLists3, out codeLists4);

				var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
				storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
				var previousDocument = storageHeader.PreviousDocuments.AddNew();
				var codeList1 = previousDocument.Lookups.CodeList as ZZRefCusCodeListCombinedCollection;
				var completeFilter1 = codeList1.CompleteFilter;
				AssertCodeListForPresentationNotification(codeLists1, codeLists2, codeLists3, codeLists4, previousDocument, codeList1, completeFilter1);

				storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
				var codeList2 = previousDocument.Lookups.CodeList as ZZRefCusCodeListCombinedCollection;
				var completeFilter2 = codeList2.CompleteFilter;
				AssertCodeListForPreLodgedTempStorage(codeLists1, codeLists2, codeLists3, codeLists4, previousDocument, codeList2, completeFilter2);
			}
		}

		public void TestCodeList_ParentIsTemporaryStorageBill()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				RefCusCodeList codeLists1, codeLists2, codeLists3, codeLists4;
				GenerateCodeList(out codeLists1, out codeLists2, out codeLists3, out codeLists4);

				var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
				storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
				var bill = storageHeader.Bills.AddNew();
				var previousDocument = bill.PreviousDocuments.AddNew();
				var codeList1 = previousDocument.Lookups.CodeList as ZZRefCusCodeListCombinedCollection;
				var completeFilter1 = codeList1.CompleteFilter;
				AssertCodeListForPresentationNotification(codeLists1, codeLists2, codeLists3, codeLists4, previousDocument, codeList1, completeFilter1);

				storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
				var codeList2 = previousDocument.Lookups.CodeList as ZZRefCusCodeListCombinedCollection;
				var completeFilter2 = codeList2.CompleteFilter;
				AssertCodeListForPreLodgedTempStorage(codeLists1, codeLists2, codeLists3, codeLists4, previousDocument, codeList2, completeFilter2);
			}
		}

		public void TestPackTypeList()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = storageHeader.Bills.AddNew();
			var previousDocument = bill.PreviousDocuments.AddNew();
			var lookups = previousDocument.Lookups;
			var packTypeList = lookups.PackTypeList;
			AssertContainsExactElementsInAnyOrder(RefCusCodeListTypes.GetCachedList(Factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				ZDateTime.Today), packTypeList);
		}

		public void TestUnitOfQuantityList()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = storageHeader.Bills.AddNew();
			var previousDocument = bill.PreviousDocuments.AddNew();
			var lookups = previousDocument.Lookups;
			var unitOfQuantityList = lookups.UnitOfQuantityList;
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), unitOfQuantityList);
			Assert("Weight Units", unitOfQuantityList.ContainsCode(Core.Constants.Weight.Kilograms));
		}

		void GenerateCodeList(out RefCusCodeList codeLists1, out RefCusCodeList codeLists2, out RefCusCodeList codeLists3, out RefCusCodeList codeLists4)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS,
				"Previous Documents Of PNTS");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment,
				"Transport Charges Method Of Payment");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Purpose", "Purpose", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, Core.Constants.CountryCodes.France);

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			codeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "A", "Test 1",
				yesterday, tomorrow);
			codeLists1.Attributes.AddNew("Purpose", "Declaration");
			codeLists1.Attributes.AddNew("Purpose", "Presentation");

			codeLists2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "B", "Test 1",
				yesterday, tomorrow);
			codeLists2.Attributes.AddNew("Purpose", "Declaration");

			codeLists3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "C", "Test 1",
				yesterday, tomorrow);
			codeLists3.Attributes.AddNew("Purpose", "Presentation");

			codeLists4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "D", "Test 1",
				yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "B",
				"Test 2", yesterday, tomorrow);
			Factory.Save();
		}

		void AssertCodeListForPresentationNotification(RefCusCodeList codeLists1, RefCusCodeList codeLists2, RefCusCodeList codeLists3, RefCusCodeList codeLists4, TemporaryStoragePreviousDocument previousDocument, ZZRefCusCodeListCombinedCollection codeList1, ZQuery completeFilter1)
		{
			AssertEquals("Matched Purpose: Presentation", true,
				Factory.Load<ZZRefCusCodeListCombined>(codeLists1.PK).MatchesFilter(completeFilter1));
			AssertEquals("Unmatched Purpose: Presentation", false,
				Factory.Load<ZZRefCusCodeListCombined>(codeLists2.PK).MatchesFilter(completeFilter1));
			AssertEquals("Matched Purpose: Presentation", true,
				Factory.Load<ZZRefCusCodeListCombined>(codeLists3.PK).MatchesFilter(completeFilter1));
			AssertEquals("Unmatched Purpose: Presentation", false,
				Factory.Load<ZZRefCusCodeListCombined>(codeLists4.PK).MatchesFilter(completeFilter1));
			AssertSame("Cached", codeList1, (ZZRefCusCodeListCombinedCollection)previousDocument.Lookups.CodeList);
		}

		void AssertCodeListForPreLodgedTempStorage(RefCusCodeList codeLists1, RefCusCodeList codeLists2, RefCusCodeList codeLists3, RefCusCodeList codeLists4, TemporaryStoragePreviousDocument previousDocument, ZZRefCusCodeListCombinedCollection codeList2, ZQuery completeFilter2)
		{
			AssertEquals("Matched Purpose: Declaration", true,
				Factory.Load<ZZRefCusCodeListCombined>(codeLists1.PK).MatchesFilter(completeFilter2));
			AssertEquals("Matched Purpose: Declaration", true,
				Factory.Load<ZZRefCusCodeListCombined>(codeLists2.PK).MatchesFilter(completeFilter2));
			AssertEquals("Unmatched Purpose: Declaration", false,
				Factory.Load<ZZRefCusCodeListCombined>(codeLists3.PK).MatchesFilter(completeFilter2));
			AssertEquals("Unmatched Purpose: Declaration", false,
				Factory.Load<ZZRefCusCodeListCombined>(codeLists4.PK).MatchesFilter(completeFilter2));
			AssertSame("Cached", codeList2, (ZZRefCusCodeListCombinedCollection)previousDocument.Lookups.CodeList);
		}
	}
}
