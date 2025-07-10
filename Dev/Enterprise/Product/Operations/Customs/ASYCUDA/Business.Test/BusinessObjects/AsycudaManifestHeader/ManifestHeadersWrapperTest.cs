using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using universalAlias = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(ManifestHeadersWrapper))]
	sealed class ManifestHeadersWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSynchroniseWithConsol()
		{
			SetUpManifestCountries(Factory, Core.Constants.CountryCodes.Eritrea);
			var etd = ZDate.Today;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "VUTAH";
			consol.JK_RL_NKDischargePort = "ERTES";
			consol.MostInterestingTransportForBinding[0].JW_ETDForBinding = etd;
			consol.MostInterestingTransportForBinding[0].JW_ETAForBinding = ZDate.Today.AddDays(5);

			var wrapper = new ManifestHeadersWrapper(consol);
			var header = wrapper.CreateCountry(Core.Constants.CountryCodes.Eritrea, "ASY");
			AssertEquals("ETD has synchronised", etd, header.AMA_E_DEP);

			etd = ZDate.Today.AddDays(1);
			consol.MostInterestingTransportForBinding[0].JW_ETDForBinding = etd;
			AssertEquals("ETD has synchronised after creating", etd, header.AMA_E_DEP);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var consolIn2 = factory2.Load<ForwardingConsol>(consol.PK);
			var wrapperIn2 = new ManifestHeadersWrapper(consolIn2);
			var headerIn2 = wrapperIn2.Headers[0];
			AssertEquals("ETD was persisted", etd, headerIn2.AMA_E_DEP);

			wrapperIn2.SynchroniseHeaders();

			etd = ZDate.Today.AddDays(2);
			consolIn2.MostInterestingTransportForBinding[0].JW_ETDForBinding = etd;
			AssertEquals("ETD has synchronised", etd, headerIn2.AMA_E_DEP);
		}

		public void TestKeywordCombinations()
		{
			SetUpManifestCountries(Factory, "FJ", "ZA", "SG");
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);
			var header1 = wrapper.Headers.AddNew();
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header1.AMA_ManifestType = "ASY";

			var header2 = wrapper.Headers.AddNew();
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header2.AMA_ManifestType = "";
			Factory.Save();

			wrapper.CreateCountry(Core.Constants.CountryCodes.SouthAfrica, "HAB");

			AssertEquals("ER, ER|ASY, ZA|HAB", wrapper.KeywordCombinations.CodesAsString);
		}

		public void TestManifestToDelete()
		{
			SetUpManifestCountries(Factory, "ER", "ZA", "SG");

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);
			var header1 = wrapper.Headers.AddNew();
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header1.AMA_ManifestType = "ASY";

			var header2 = wrapper.Headers.AddNew();
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			header2.AMA_ManifestType = "";
			Factory.Save();

			var manifestZA = wrapper.CreateCountry(Core.Constants.CountryCodes.SouthAfrica, "BBB");
			wrapper.WR_KeywordCombination = "ZA|BBB";
			AssertEquals(manifestZA, wrapper.ManifestToDelete);

			wrapper.WR_KeywordCombination = "";
			AssertNull(wrapper.ManifestToDelete);

			wrapper.WR_KeywordCombination = "ER|ASY";
			AssertEquals(header1, wrapper.ManifestToDelete);

			wrapper.WR_KeywordCombination = "ER";
			AssertEquals(header2, wrapper.ManifestToDelete);
		}

		public void TestDeleteManifest()
		{
			SetUpManifestCountries(Factory, "FJ", "ZA", "SG");
			var wrapper = (ManifestHeadersWrapper)GetNewBusinessObject();
			var manifestZA = wrapper.CreateCountry(Core.Constants.CountryCodes.SouthAfrica, "HAB");
			var manifestFJ = wrapper.CreateCountry(Core.Constants.CountryCodes.Fiji, "ASY");
			Assert(wrapper.Headers.Contains(manifestZA));
			Assert(wrapper.Headers.Contains(manifestFJ));

			wrapper.DeleteManifest(manifestZA);
			Assert(!wrapper.Headers.Contains(manifestZA));
		}

		public void TestGetConsolCountriesAndTransporModesThatMightNeedManifest()
		{
			AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory,
				new ModeAndCountry("AIR", "VU"),
				new ModeAndCountry("ROA", "VU"),
				new ModeAndCountry("", "ER"));

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "VUTAH";
			consol.JK_RL_NKDischargePort = "ERTES";
			AddTransportLeg(Core.Constants.TransportModes.Road, "VUTAH", "VUEPI");
			AddTransportLeg(Core.Constants.TransportModes.Sea, "VUEPI", "VUVLI");
			AddTransportLeg(Core.Constants.TransportModes.Air, "VUEPI", "AUBNE");
			AddTransportLeg(Core.Constants.TransportModes.Rail, "AUBNE", "AUSYD");
			AddTransportLeg(Core.Constants.TransportModes.Air, "AUSYD", "ERTES");

			CombineAssertions(() =>
			{
				var requireManifest = wrapper.GetConsolCountriesAndTransportModesThatMightNeedManifest();
				AssertContainsExactElementsInAnyOrder("Two Countries in Dictionary", new[] { Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Vanuatu }, requireManifest.Keys);
				AssertContainsExactElementsInAnyOrder("Eritrea Transport Modes", new[] { Core.Constants.TransportModes.Air }, requireManifest.GetValueOrDefault(Core.Constants.CountryCodes.Eritrea));
				AssertContainsExactElementsInAnyOrder("Vanuatu Transport Modes", new[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road }, requireManifest.GetValueOrDefault(Core.Constants.CountryCodes.Vanuatu));
			});

			void AddTransportLeg(string transportMode, string loadPort, string destinationPort)
			{
				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = transportMode;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = destinationPort;
			}
		}

		public void TestGetConsolCountriesAndTransporModesThatMightNeedManifest_Empty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "ERASM";
			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("No manfest applicable", false, wrapper.GetConsolCountriesAndTransportModesThatMightNeedManifest().Any());
		}

		public void TestGetConsolCountriesAndTransporModesThatMightNeedManifest_Singapore()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");

			var sgCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, Core.Constants.CountryCodes.Singapore, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(sgCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ManifestCountryAttributeValue);

			var sgACCESSEnable = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable as BooleanRegistryItem;
			using (sgACCESSEnable.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var wrapper = new ManifestHeadersWrapper(consol);
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "SGSIN";
				consol.JK_RL_NKDischargePort = "SGTPG";
				var transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Road;
				transport.JW_RL_NKLoadPort = "SGTPG";
				transport.JW_RL_NKDiscPort = "SGSIN";
				AssertEquals("SG Access does not plugin to a consolidation", false, wrapper.GetConsolCountriesAndTransportModesThatMightNeedManifest().Any());
			}
		}

		public void TestCountryCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");

			var erCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(erCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ManifestCountryAttributeValue);

			var fjCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Fiji, Core.Constants.CountryCodes.Fiji, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(fjCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ManifestCountryAttributeValue);

			var zaCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Latvia, Core.Constants.CountryCodes.Latvia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(zaCountry.PK, RefCusCodeListAttributeTypes.Codes.VOC, ManifestCountryAttributeValue);

			var brCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Brazil, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(brCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ManifestCountryAttributeValue);
			helper.CreateCusCodeListAttribute(brCountry.PK, RefCusCodeListAttributeTypes.Codes.VOC, ManifestCountryAttributeValue);

			helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Australia, Core.Constants.CountryCodes.Australia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();
			var wrapper = (ManifestHeadersWrapper)GetNewBusinessObject();
			Assert("NVC ASYCUDAManifest", wrapper.CountryCodes.ContainsCode(Core.Constants.CountryCodes.Eritrea));
			Assert("NVC ASYCUDAManifest", wrapper.CountryCodes.ContainsCode(Core.Constants.CountryCodes.Fiji));
			Assert("NVC/VOC ASYCUDAManifest", wrapper.CountryCodes.ContainsCode(Core.Constants.CountryCodes.Brazil));
			Assert("VOC ASYCUDAManifest", !wrapper.CountryCodes.ContainsCode(Core.Constants.CountryCodes.Latvia));
			Assert("!NVC/!VOC ASYCUDAManifest", !wrapper.CountryCodes.ContainsCode(Core.Constants.CountryCodes.Australia));
			Assert("NVC with other ApplicationProvider", wrapper.CountryCodes.ContainsCode(Core.Constants.CountryCodes.SouthAfrica));
		}

		public void TestWrapperGroupedByCountryCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var gbCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(gbCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ManifestCountryAttributeValue);
			helper.CreateCusCodeListAttribute(gbCountry.PK, RefCusCodeListAttributeTypes.Codes.VOC, ManifestCountryAttributeValue);
			Factory.Save();
			var wrapper = (ManifestHeadersWrapper)GetNewBusinessObject();
			var duplicateCodes = wrapper.CountryCodes.GetAllCodesZString().GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
			Assert("Duplicates found:", duplicateCodes.Count == 0);
			Assert("NVC ASYCUDAManifest", !wrapper.CountryCodes.GetDescriptionFromCode(Core.Constants.CountryCodes.UnitedKingdom).Contains("Great Britain"));
			Assert("NVC ASYCUDAManifest", !wrapper.CountryCodes.GetDescriptionFromCode(Core.Constants.CountryCodes.UnitedKingdom).Contains("Northern Ireland"));
			Assert("NVC ASYCUDAManifest", !wrapper.CountryCodes.GetDescriptionFromCode(Core.Constants.CountryCodes.UnitedKingdom).Contains("UK GVMS Manifest"));

			Assert("NVC ASYCUDAManifest", wrapper.CountryCodes.GetDescriptionFromCode(Core.Constants.CountryCodes.UnitedKingdom).Contains("United Kingdom"));
		}

		public void TestCreateOrGetCountry_EU_ENS_NoError()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(universalAlias.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.EuropeanUnionEUN, universalAlias.RefCusCodeListTypes.Codes.ICS2EUMemberState, Core.Constants.CountryCodes.Germany, "Germany", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();

			Factory.Save();
			using var countryChange = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany);
			var wrapper = new ManifestHeadersWrapper(consol);

			wrapper.WR_CountryCode = Core.Constants.CountryCodes.EuropeanUnion;
			wrapper.CreateCountry(Core.Constants.CountryCodes.EuropeanUnion, "ENS");

			var createdHeader = wrapper.Headers.Cast<AsycudaManifestHeader>().Single(h => h.AMA_RN_NKCountry == Core.Constants.CountryCodes.Germany);
			AssertEquals("Creates EU AsycudaManifestHeader manifest with country DE",
				"Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader",
				createdHeader.GetType().FullName);

			AssertEquals("Setting AMA_RN_NKCountry for creation doesn't raise error", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestCreateOrGetCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");

			var zaCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(zaCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ManifestCountryAttributeValue);

			var fjCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Fiji, Core.Constants.CountryCodes.Fiji, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(fjCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ManifestCountryAttributeValue);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();

			shipment.JS_OuterPacks = 10;
			shipment.JS_ActualWeight = 100m;
			shipment.JS_ActualVolume = 100m;

			Factory.Save();

			bool saveButtonEnabled = false;
			consol.HasChangesChanged += (object sender, HasChangesChangedEventArgs e) => { saveButtonEnabled = consol.HasChanges; };

			var wrapper = new ManifestHeadersWrapper(consol);
			AssertEquals("Precondition - Consol does not have changes yet", false, consol.HasChanges);
			AssertEquals("Wrapper has no existing headers", 0, wrapper.Headers.Count);

			wrapper.WR_CountryCode = Core.Constants.CountryCodes.SouthAfrica;
			wrapper.CreateCountry(Core.Constants.CountryCodes.SouthAfrica, "HAB");

			var createdHeader = wrapper.Headers.Cast<AsycudaManifestHeader>().FirstOrDefault(h => h.AMA_RN_NKCountry == Core.Constants.CountryCodes.SouthAfrica);
			Assert("Should have created a ZA AsycudaManifestHeader", createdHeader is Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader);
			Assert("Consol Has Changes", consol.HasChanges);
			Assert("HasChangesChanged on Consol was called in a way that will enable the Save button on the Form.", saveButtonEnabled);

			wrapper.WR_CountryCode = Core.Constants.CountryCodes.Fiji;
			wrapper.CreateCountry(Core.Constants.CountryCodes.Fiji, "ASY");

			createdHeader = wrapper.Headers.Cast<AsycudaManifestHeader>().FirstOrDefault(h => h.AMA_RN_NKCountry == Core.Constants.CountryCodes.Fiji);
			Assert("Should have created a Normal AsycudaManifestHeader", createdHeader is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader);
			Assert("Created Header Has Changes", createdHeader.HasChanges);
			AssertEquals("Created Header does not have errors from setting up", false, createdHeader.HasNotifications());
		}

		public void TestManifestTypeIsSetFromCountryCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");

			var sgCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, Core.Constants.CountryCodes.Singapore, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(sgCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ManifestCountryAttributeValue);

			var usCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(usCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ManifestCountryAttributeValue);

			var sgACCESSEnable = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>().ACCESSEnable as BooleanRegistryItem;
			using (sgACCESSEnable.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var wrapper = new ManifestHeadersWrapper(consol);

				AssertEquals("Precondition, not set.", ZString.Empty, wrapper.WR_CountryCode);
				AssertEquals("Precondition, not set.", 0, wrapper.ManifestTypes.Count);
				AssertEquals("Precondition, not set.", ZString.Empty, wrapper.WR_ManifestType);

				wrapper.WR_CountryCode = Core.Constants.CountryCodes.Eritrea;
				AssertEquals("Eritrea has no manifest types", 0, wrapper.ManifestTypes.Count);
				AssertEquals("Eritrea is empty", ZString.Empty, wrapper.WR_ManifestType);

				wrapper.WR_CountryCode = Core.Constants.CountryCodes.Singapore;
				AssertEquals("Singapore has 2 manifest types", 2, wrapper.ManifestTypes.Count);
				AssertEquals("Singapore is not defaulted", ZString.Empty, wrapper.WR_ManifestType);

				wrapper.WR_CountryCode = Core.Constants.CountryCodes.UnitedStates;
				AssertEquals("UnitedStates has 1 manifest type", 1, wrapper.ManifestTypes.Count);
				AssertEquals("UnitedStates is Defaulted", "IAM", wrapper.WR_ManifestType);
			}
		}

		public void TestSettingNewManifestPropertiesDoesNotIndicateAChange()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var wrapper = new ManifestHeadersWrapper(consol);
			AssertEquals(false, wrapper.HasChanges);

			wrapper.WR_CountryCode = "FJ";
			AssertEquals("Changing WR_CountryCode does not set HasChanges on the Wrapper", false, wrapper.HasChanges);

			wrapper.WR_ManifestType = "ASY";
			AssertEquals("Changing WR_ManifestType does not set HasChanges on the Wrapper", false, wrapper.HasChanges);

			wrapper.WR_KeywordCombination = "SG|MGI";
			AssertEquals("Changing WR_KeywordCombination does not set HasChanges on the Wrapper", false, wrapper.HasChanges);
		}

		public static void SetUpManifestCountries(BusinessObjectFactory factory, params string[] countryCodes)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");

			foreach (var country in countryCodes)
			{
				var manifestCountry = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, country, country, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateCusCodeListAttribute(manifestCountry.PK, RefCusCodeListAttributeTypes.Codes.NVC, ManifestCountryAttributeValue);
			}

			factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject() => new ManifestHeadersWrapper(Factory.NewWithValidTestData<ForwardingConsol>());

		const string ManifestCountryAttributeValue = "17.3.29.001";
	}
}
