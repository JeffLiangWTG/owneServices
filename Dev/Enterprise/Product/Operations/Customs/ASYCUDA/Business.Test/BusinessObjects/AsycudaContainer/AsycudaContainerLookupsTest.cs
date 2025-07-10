using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGetSealTypeListForCountry_CountrySpecificMapping()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			var container = header.Containers.AddNew();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.MSELT, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.MSELT, SealTypeList.Codes.MechanicalSeal, "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var list = container.Lookups.SealTypeList;
			AssertContainsExactElementsInAnyOrder(new[] { "M" }, list.GetAllCodes());
		}

		public void TestGetSealTypeListForCountry_GenericMapping()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			var container = header.Containers.AddNew();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.MSELT, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.MSELT, SealTypeList.Codes.ElectronicSeal, "2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			Factory.Save();

			var list = container.Lookups.SealTypeList;
			AssertContainsExactElementsInAnyOrder(new[] { "E" }, list.GetAllCodes());
		}

		public void TestGetSealTypeListForCountry_CountrySpecificAndGenericMapping()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			var container = header.Containers.AddNew();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.MSELT, MapDirectionList.Codes.BTH, "Description", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.MSELT, SealTypeList.Codes.MechanicalSeal, "0", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateCusMap(RefCusMapTypeList.Codes.MSELT, SealTypeList.Codes.MechanicalSeal, "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.MSELT, SealTypeList.Codes.ElectronicSeal, "2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			Factory.Save();

			var list = container.Lookups.SealTypeList;
			AssertContainsExactElementsInAnyOrder(new[] { "M" }, list.GetAllCodes());
		}

		public void TestEmptyFullList()
		{
			AssertSame(Factory.GetCachedValue<EmptyFullIndicatorList>(), Factory.New<AsycudaManifestHeader>().Containers.AddNew().Lookups.EmptyFullList);
		}

		public void TestCommodityCodesForBangladesh()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityCode, "CommodityCode");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "BD", "Bangladesh", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("BD", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "226", "Rangpur", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("BD", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityCode, "46", "Color", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "BD";

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);
			header.AMA_RL_NKPortOfDischarge = "BDXXX";

			var container = header.Containers.AddNew();
			AssertContains("Bangladesh commodity codes contains '46' for 'color' (WTF?)", "46", container.Lookups.CommodityCodes.CodesAsString);
		}

		public void TestWeightCodes()
		{
			AssertSame(Factory.GetWeightUQList(), Factory.New<AsycudaContainer>().Lookups.WeightCodes);
		}

		public void TestSealingPartyListForEritrea()
		{
			AssertSealingPartyTypeMappingFetchedCorrectly(CountryCodes.Eritrea, "ASY", new[]
			{
				ContainerSealParties.Codes.Terminal,
				ContainerSealParties.Codes.ConsignorShipper,
				ContainerSealParties.Codes.Customs,
				ContainerSealParties.Codes.Quarantine,
				ContainerSealParties.Codes.CarrierShippingLine,
			});
		}

		public void TestSealingPartyListForSouthAfrica()
		{
			AssertSealingPartyTypeMappingFetchedCorrectly(CountryCodes.SouthAfrica, "HAB", new[]
			{
				ContainerSealParties.Codes.Terminal,
				ContainerSealParties.Codes.ConsignorShipper,
				ContainerSealParties.Codes.Customs,
			});
		}

		void AssertSealingPartyTypeMappingFetchedCorrectly(string countryCode, string manifestType, string[] expectedCodes)
		{
			const string sealingParty_Unknown = "AB";
			const string sealingParty_Carrier = "CA";
			const string sealingParty_Customs = "CU";
			const string sealingParty_Shipper = "SH";
			const string sealingParty_TerminalOperator = "TO";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.STYPE, MapDirectionList.Codes.BTH, "Sealing Party Type Mapping", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, ContainerSealParties.Codes.Terminal, sealingParty_TerminalOperator, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, ContainerSealParties.Codes.ConsignorShipper, sealingParty_Shipper, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, ContainerSealParties.Codes.Customs, sealingParty_Customs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, ContainerSealParties.Codes.Terminal, sealingParty_TerminalOperator, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, ContainerSealParties.Codes.ConsignorShipper, sealingParty_Shipper, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, ContainerSealParties.Codes.Customs, sealingParty_Customs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, ContainerSealParties.Codes.Quarantine, sealingParty_Unknown, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, ContainerSealParties.Codes.CarrierShippingLine, sealingParty_Carrier, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			Factory.Save();

			CombineAssertions(() =>
			{
				var actualCodes = AsycudaManifestHeaderHelper.CreateNew(Factory, countryCode, manifestType).Containers.AddNew().Lookups.SealingPartyList.OfType<CodeDescriptionPair>().Select(x => x.Code).ToArray();
				AssertEquals($"Count of available SealingParties must be exactly {expectedCodes.Length}", expectedCodes.Length, actualCodes.Length);
				AssertContainsExactElementsInAnyOrder("Incorrect items in available SealingParties", expectedCodes, actualCodes);
			});
		}

		public void TestUnloadingState()
		{
			var lookup = Factory.GetCachedValue<UnloadingStates>();

			AssertEquals("Should contain 5 items", 5, lookup.Count);
			AssertCodeDescriptionPairList(lookup, ("DAM", "Damaged"), ("DEC", "As Declared"), ("DIF", "Differences to Declared"), ("MIS", "Missing"), ("NEW", "New"));

			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();

			AssertSame("UnloadingStatesList is cached", lookup, container.Lookups.UnloadingStatesList);
		}
	}
}
