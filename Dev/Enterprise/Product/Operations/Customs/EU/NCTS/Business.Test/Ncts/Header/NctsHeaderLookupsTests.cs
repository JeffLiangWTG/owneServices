using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsHeaderLookupsTests : BusinessObjectLookupsTestCase
	{
		public void TestDeclarationLanguagesList()
		{
			AssertEquals(string.Empty, lookups.CommunicationLanguageList.CodesAsString);
		}

		public void TestEventFlagList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.EventFlagList;
				AssertEquals("CodeAsString", "Y, N, C", list.CodesAsString);
				AssertSame("Cached", list, lookups.EventFlagList);
			});
		}

		public void TestEventFlagList_Phase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals("Y, N", lookups.EventFlagList.CodesAsString);
		}

		public void TestCarriers()
		{
			AssertNotNull(lookups.Carriers);
		}

		public void TestConsignees()
		{
			AssertNotNull(lookups.Consignees);
		}

		public void TestConsignors()
		{
			AssertNotNull(lookups.Consignors);
		}

		public void TestOrganisations()
		{
			AssertNotNull(lookups.Organisations);
		}

		public void TestCountryOfDispatchList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			var countryOfDispatchList = lookups.CountryOfDispatchList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes From List", "AU, DE, FR", countryOfDispatchList.CodesAsString);
				AssertSame("Cached", countryOfDispatchList, lookups.CountryOfDispatchList);
			});
		}

		public void TestCountryOfDestinationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			var countryOfDestinationList = lookups.CountryOfDestinationList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes From List", "AU, DE, FR", countryOfDestinationList.CodesAsString);
				AssertSame("Cached", countryOfDestinationList, lookups.CountryOfDestinationList);
			});
		}

		public void TestUnloadedMeansOfTransportAtDepartureNationalityList()
		{
			var inActiveCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Germany);
			inActiveCountry.RN_IsActive = false;
			Factory.Save();
			var unloadedMeansOfTransportAtDepartureNationalityList = lookups.UnloadedMeansOfTransportAtDepartureNationalityList;
			CombineAssertions(() =>
			{
				AssertEquals("No DE Code", false, unloadedMeansOfTransportAtDepartureNationalityList.ContainsCode(Core.Constants.CountryCodes.Germany));
				AssertSame("Cached", unloadedMeansOfTransportAtDepartureNationalityList, lookups.UnloadedMeansOfTransportAtDepartureNationalityList);
			});
		}

		public void TestDestinationCustomsOfficeCodeList_Phase4()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = new CustomsOfficeCodeTestHelper(helper);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB003478", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR001234", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				lookups = new NctsHeaderLookups(nctsHeader);
				var collection = lookups.DestinationCustomsOfficeCodeList;
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new ZString[] { "FR001234" }, collection.Select(x => x.ZZD_Code));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				lookups = new NctsHeaderLookups(nctsHeader);
				var collection = lookups.DestinationCustomsOfficeCodeList;
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new ZString[] { "GB003478", "XI005342" }, collection.Select(x => x.ZZD_Code));
			}
		}

		public void TestDestinationCustomsOfficeCodeList_Phase5()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = new CustomsOfficeCodeTestHelper(helper);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB003478", "GB003478", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB009999", "GB009999", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XI004567", "XI004567", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR001234", "FR001234", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				lookups = new NctsHeaderLookups(nctsHeader);
				var collection = lookups.DestinationCustomsOfficeCodeList;
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new ZString[] { "FR001234" }, collection.Select(x => x.ZZD_Code));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				lookups = new NctsHeaderLookups(nctsHeader);
				var collection = lookups.DestinationCustomsOfficeCodeList;
				collection.Load();
				AssertContainsExactElementsInAnyOrder(new ZString[] { "GB003478", "XI004567" }, collection.Select(x => x.ZZD_Code));
			}
		}

		public void TestPortOfDispatchList()
		{
			AssertType<RefUNLOCOCollection>(lookups.PortOfDispatchList);
		}

		public void TestNctsHeaderToAttach()
		{
			var header = Factory.New<NctsHeader>();
			AssertType<CusEntryHeadersToAttachCollection>(header.Lookups.CusEntryHeadersToAttach);
		}

		public void TestNctsMessageStatusList()
		{
			AssertType<LogicalStatusList>(lookups.NctsMessageStatusList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			lookups = new NctsHeaderLookups(nctsHeader);
		}
		NctsHeader nctsHeader;
		NctsHeaderLookups lookups;
	}
}
