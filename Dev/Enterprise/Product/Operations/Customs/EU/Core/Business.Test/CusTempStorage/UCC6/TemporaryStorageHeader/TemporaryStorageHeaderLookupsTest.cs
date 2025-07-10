using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsStatusList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.TemporaryStorageCustomsStatus, UniversalReferenceConstants.PNTS.CustomsStatus.IntendedControl, "Intended Control", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList("EUN", UniversalReferenceConstants.RefCusCodeListType.Code.TemporaryStorageCustomsStatus, UniversalReferenceConstants.PNTS.CustomsStatus.IrregularityUnderInvestigation, "Irregularity Under Investigation", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList("EUN", UniversalReferenceConstants.RefCusCodeListType.Code.TemporaryStorageCustomsStatus, UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageEnded, "Ended", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, UniversalReferenceConstants.RefCusCodeListType.Code.TemporaryStorageCustomsStatus, "BLA", "TSTA code outside EU", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals("CustomsStatusList should be made of of EU + LV RefCusCodesof type TSTA", 3, header.Lookups.CustomsStatusList.Count);
			AssertContainsExactElementsInAnyOrder("CustomsStatusList should be made of EU + LV codes of type TSTA, sorted.", new string[] { UniversalReferenceConstants.PNTS.CustomsStatus.IntendedControl, UniversalReferenceConstants.PNTS.CustomsStatus.IrregularityUnderInvestigation, UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageEnded }, header.Lookups.CustomsStatusList.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code).ToArray());
		}

		public void TestPNTSMessageStatusList()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertContainsExactElementsInAnyOrder("PNTSMessageStatusList should be made of codes from PNTSMessageStatusList.", new string[] { PNTSMessageStatusList.Codes.Acknowledged, PNTSMessageStatusList.Codes.TechnicalFailure, PNTSMessageStatusList.Codes.FunctionalRejection, PNTSMessageStatusList.Codes.Sent }, header.Lookups.PNTSMessageStatusList.GetAllCodes());
		}

		public void TestMessageTypeLookUpList()
		{
			var messageTypeList = lookups.MessageTypeList;
			CombineAssertions(() =>
			{
				AssertType<PNTSMessageTypeList>(messageTypeList);
				AssertSequencesEqual(new[] { "TC", "DC", "TS", "PN", "TF" }, messageTypeList.GetAllCodes());
				AssertEquals("Description from TC", "Combined Temporary Storage", messageTypeList.GetDescriptionFromCode("TC"));
				AssertEquals("Description from DC", "Deconsolidation", messageTypeList.GetDescriptionFromCode("DC"));
				AssertEquals("Description from TS", "Pre-Lodged Temporary Storage", messageTypeList.GetDescriptionFromCode("TS"));
				AssertEquals("Description from PN", "Presentation Notification", messageTypeList.GetDescriptionFromCode("PN"));
				AssertEquals("Description from TF", "Transfer", messageTypeList.GetDescriptionFromCode("TF"));
			});
		}

		public void TestMessageTypeList()
		{
			var pntsMessageTypeList = new PNTSMessageTypeList();

			AssertContainsExactElementsInAnyOrder("Temporary Storage header is Null", pntsMessageTypeList.GetAllCodes(), Factory.GetNull<TemporaryStorageHeader>().Lookups.MessageTypeList.GetAllCodes());

			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			var lookups = new TemporaryStorageHeaderLookups(temporaryStorageHeader);
			temporaryStorageHeader.CustomsStatus = ZString.Empty;

			var filteredList = new CodeDescriptionPairList();
			filteredList.AddPair(PNTSMessageTypeList.Codes.PresentationNotification, PNTSMessageTypeList.Descriptions.PresentationNotification);
			filteredList.AddPair(PNTSMessageTypeList.Codes.PreLodgedTempStorage, PNTSMessageTypeList.Descriptions.PreLodgedTempStorage);

			CombineAssertions("MessageTypeList depend on temporaryStorageHeader CustomsStatus, CustomsStatus is empty", () =>
			{
				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
				AssertContainsExactElementsInAnyOrder("CombinedTemporaryStorage", pntsMessageTypeList.GetAllCodes(), lookups.MessageTypeList.GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
				AssertContainsExactElementsInAnyOrder("PreLodgedTempStorage", pntsMessageTypeList.GetAllCodes(), lookups.MessageTypeList.GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
				AssertContainsExactElementsInAnyOrder("PresentationNotification", pntsMessageTypeList.GetAllCodes(), lookups.MessageTypeList.GetAllCodes());
			});

			CombineAssertions("MessageTypeList depend on temporaryStorageHeader CustomsStatus, CustomsStatus is TemporaryStoragePreLodged", () =>
			{
				temporaryStorageHeader.CustomsStatus = PNTS.CustomsStatus.TemporaryStoragePreLodged;
				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
				AssertContainsExactElementsInAnyOrder("CombinedTemporaryStorage", pntsMessageTypeList.GetAllCodes(), lookups.MessageTypeList.GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
				var lookupsList = lookups.MessageTypeList;
				AssertContainsExactElementsInAnyOrder("PreLodgedTempStorage", filteredList.GetAllCodes(), lookupsList.GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
				AssertContainsExactElementsInAnyOrder("PresentationNotification", filteredList.GetAllCodes(), lookups.MessageTypeList.GetAllCodes());
			});

			CombineAssertions("MessageTypeList depend on temporaryStorageHeader CustomsStatus, CustomsStatus is TemporaryStorageActivated", () =>
			{
				temporaryStorageHeader.CustomsStatus = PNTS.CustomsStatus.TemporaryStorageActivated;
				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
				var lookupsList = lookups.MessageTypeList;
				AssertContainsExactElementsInAnyOrder("CombinedTemporaryStorage", pntsMessageTypeList.GetAllCodes(), lookupsList.GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
				AssertContainsExactElementsInAnyOrder("PreLodgedTempStorage", pntsMessageTypeList.GetAllCodes(), lookups.MessageTypeList.GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
				AssertContainsExactElementsInAnyOrder("PresentationNotification", pntsMessageTypeList.GetAllCodes(), lookups.MessageTypeList.GetAllCodes());
			});
		}

		public void TestMessageTypeList_WhenParentIsNull()
		{
			var header = Factory.GetNull<TemporaryStorageHeader>();
			AssertSequencesEqual(new[] { "TC", "DC", "TS", "PN", "TF" }, header.Lookups.MessageTypeList.GetAllCodes());
			header.CustomsStatus = PNTS.CustomsStatus.TemporaryStoragePreLodged;
			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			AssertSequencesEqual(new[] { "TC", "DC", "TS", "PN", "TF" }, header.Lookups.MessageTypeList.GetAllCodes());
		}

		public void TestCarrierList()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			Factory.Save();
			var list = header.Lookups.CarrierList;
			AssertType<ShippingProviderCollection>(list);
			header.AMA_TransportMode = "AIR";
			var airList = header.Lookups.CarrierList;
			AssertType<AirShippingProviderCollection>(airList);
			header.AMA_TransportMode = "SEA";
			var seaList = header.Lookups.CarrierList;
			AssertType<SeaShippingProviderCollection>(seaList);
		}

		public void TestDeclarantList()
		{
			var list = lookups.DeclarantList;
			AssertType<OrganisationsFindBoxCollection>(list);
			var listCached = lookups.DeclarantList;
			Assert("Declarant List is cached.", ReferenceEquals(list, listCached));
		}

		public void TestPresenterList()
		{
			var list = lookups.PresenterList;
			AssertType<OrganisationsFindBoxCollection>(list);
			var listCached = lookups.PresenterList;
			Assert("Presenter List is cached.", ReferenceEquals(list, listCached));
		}

		public void TestRepresentativeList()
		{
			var list = lookups.RepresentativeList;
			AssertType<OrganisationsFindBoxCollection>(list);
			var listCached = lookups.RepresentativeList;
			Assert("Representative List is cached.", ReferenceEquals(list, listCached));
		}

		public void TestDeclarantAndPresenterAndRepresentativeUsingTheSameCachedList()
		{
			var declarantList = lookups.DeclarantList;
			var presenterList = lookups.PresenterList;
			var representativeList = lookups.RepresentativeList;
			Assert("Declarant and Presenter are using the same cached list", ReferenceEquals(declarantList, presenterList));
			Assert("Presenter and Representative are using the same cached list", ReferenceEquals(presenterList, representativeList));
		}

		public void TestCustomsOfficeList()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = "FR";
			var lookups = new TemporaryStorageHeaderLookups(header);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var enuZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: enuZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROFF001", "PARIS PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode1.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent);
			var cusCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROFF002", "PARIS PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode2.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfLodgementEntry);
			var cusCode3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROFF003", "PARIS PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode3.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.CustomsOfficeForTemporaryStorage);
			var cusCode4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROFF004", "PARIS PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode4.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.AuthorityControlCode);
			Factory.Save();

			var list = lookups.CustomsOfficeCodeList;
			list.Load();
			AssertType<CustomsOfficeCodeCollection>(list);
			AssertContainsExactElementsInAnyOrder("CustomsOfficeList should only return offices of role ENT, ENL and TST", new[] { "FROFF001", "FROFF002", "FROFF003", }, list.Select(x => x.ZZD_Code));
		}

		public void TestTransportTypeList()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var lookups = header.Lookups;

			header.AMA_TransportMode = ZString.Empty;
			var listOfApplicableCodes = lookups.TransportTypeList.GetAllCodes();
			AssertEquals("10, 20, 21, 30, 31, 41, 80", string.Join(", ", listOfApplicableCodes));

			header.AMA_TransportMode = "AIR";
			listOfApplicableCodes = lookups.TransportTypeList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder("When Transport Mode is AIR, the list for Transport Type shoud be starte with 41", new string[] { "41" }, listOfApplicableCodes);

			header.AMA_TransportMode = "SEA";
			listOfApplicableCodes = lookups.TransportTypeList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder("When Transport Mode is SEA, the list for Transport Type shoud be starte with 10", new string[] { "10" }, listOfApplicableCodes);

			header.AMA_TransportMode = "RAI";
			listOfApplicableCodes = lookups.TransportTypeList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder("When Transport Mode is RAI, the list for Transport Type shoud be starte with 2", new string[] { "20", "21" }, listOfApplicableCodes);

			header.AMA_TransportMode = "ROA";
			listOfApplicableCodes = lookups.TransportTypeList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder("When Transport Mode is ROA, the list for Transport Type shoud be starte with 3", new string[] { "30", "31" }, listOfApplicableCodes);

			header.AMA_TransportMode = "IWT";
			listOfApplicableCodes = lookups.TransportTypeList.GetAllCodes();
			AssertContainsExactElementsInAnyOrder("When Transport Mode is IWT, the list for Transport Type shoud be starte with 80", new string[] { "80" }, listOfApplicableCodes);
		}

		public void TestTransportModeList()
		{
			var list = lookups.TransportModeList;
			AssertEquals("AIR, SEA, RAI, ROA, IWT", list.CodesAsString);
		}

		public void TestRefUNLOCOCollection()
		{
			var list = lookups.RefUNLOCOCollection;
			AssertType<RefUNLOCOCollection>(list);
		}

		public void TestAuthorizationTypeList()
		{
			var list = lookups.AuthorizationTypeList;
			AssertType<AuthorizationTypeList>(list);
		}

		public void TestAuthorizationOwnerList()
		{
			var list = lookups.OwnerList;
			AssertType<OrganisationsFindBoxCollection>(list);
		}

		public void TestAuthorizationNumberList()
		{
			var orgHeaderPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var cusAuthorizationUsage = header.AuthorizationUsageOrNew;
			cusAuthorizationUsage.FillWithValidTestData();

			cusAuthorizationUsage.AGC_OH_Owner = orgHeaderPK;
			cusAuthorizationUsage.AGC_Code = AuthorizationTypeList.Codes.TST;
			cusAuthorizationUsage.AGC_Number = "OH";
			header.AMA_RN_NKCountry = "FR";

			header.Factory.Save();

			CombineAssertions("Filtered authorisation number list", () =>
			{
				var numberList = header.Lookups.AuthorizationNumberList;
				var authorisationTypeFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType + ":Property"];
				AssertEquals("Value for AuthorisationType", AuthorizationTypeList.Codes.TST, authorisationTypeFilterBO.Value);
				AssertEquals("IsRemovable for AuthorisationType", false, authorisationTypeFilterBO.IsRemovable);

				var authorisationNumberFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.Country + ":Property"];
				AssertEquals("Value for AuthorisationNumber", "FR", authorisationNumberFilterBO.Value);
				AssertEquals("IsRemovable for AuthorisationNumber", false, authorisationNumberFilterBO.IsRemovable);

				var authorisationHolderFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder + ":Property"];
				AssertEquals("Value for AuthorisationHolder", orgHeaderPK, authorisationHolderFilterBO.Value);
				AssertEquals("IsRemovable for AuthorisationHolder", false, authorisationHolderFilterBO.IsRemovable);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new TemporaryStorageHeaderLookups(Factory.NewWithValidTestData<TemporaryStorageHeader>());
		}

		TemporaryStorageHeaderLookups lookups;
	}
}
