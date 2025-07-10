using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public sealed class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRegistrationLookups()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			// TODO: Once we switched to 'CMAN' statuses from 'CSTA' change to 'Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus'
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			var za8 = helper.CreateNewOrGetExistingCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "8", "Proceed to Border (SACU clearances)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IAllowCancel", "true");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "INotify", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "ISendEntryDocs", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IUpdateCustomsStatus", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IUpdateEntryNumber", "");

			var za9 = helper.CreateNewOrGetExistingCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "9", "Already on Customs system", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(za9.PK, "INotify", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za9.PK, "IUpdateEntryNumber", "");

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "VU";
			var list1 = header.Lookups.RegistrationStatusList;
			var list2 = header.Lookups.RegistrationStatusList;
			Assert(object.ReferenceEquals(list1, list2));
			Assert(list1.ContainsCode("NOT"));
		}

		public void TestMethodOfPaymentList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var zzDataGrouping = helper.CreateNewOrGetExistingDataGrouping("ZZ");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, parent: zzDataGrouping);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ICSMethodOfPayment, "ICS Method of Payment");
			helper.CreateCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ICSMethodOfPayment, "A", "Payment in cash", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ICSMethodOfPayment, "B", "Payment by credit card", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			Assert("Contains code of parent data grouping", header.Lookups.MethodOfPaymentList.ContainsCode("A"));
			Assert("Contains code of current country", header.Lookups.MethodOfPaymentList.ContainsCode("B"));
		}

		public void TestSpecialMentionsList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var zzDataGrouping = helper.CreateNewOrGetExistingDataGrouping("ZZ");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, parent: zzDataGrouping);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ICSSpecialMentions, "ICS Special Mentions");
			helper.CreateCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ICSSpecialMentions, "00000", "Special Mentions 0", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ICSSpecialMentions, "11111", "Special Mentions 1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			Assert("Contains code of parent data grouping", header.Lookups.SpecialMentionsList.ContainsCode("00000"));
			Assert("Contains code of current country", header.Lookups.SpecialMentionsList.ContainsCode("11111"));
		}

		public void TestMessageStatusList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertSame(Factory.GetCachedValue<MessageStatusCodeList>(), header.Lookups.MessageStatusList);
		}

		public void TestOrganisations()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals(typeof(OrgHeaderCollection), header.Lookups.Organisations.GetType());
		}

		public void TestManifestTypes()
		{
			var headerASYCUDA = Factory.New<AsycudaManifestHeader>();
			headerASYCUDA.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;

			AssertManifestTypesForTransportModes(headerASYCUDA, headerASYCUDA.Lookups, "No Manifest Types for ASYCUDAManifest", ("SEA", ""), ("AIR", ""), ("ROA", ""), ("RAI", ""));

			var headerZA = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			headerZA.FillWithValidTestData();
			var consol = Factory.New<ForwardingConsol>();
			headerZA.SetParent(consol);

			AssertManifestTypesForTransportModes(headerZA, headerZA.Lookups, "Manifest Types for NVC ZAManifest", ("SEA", "ALH, COH"), ("AIR", "HAB"), ("ROA", "RFM"), ("RAI", ""));

			var headerZA2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			headerZA2.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			headerZA2.AMA_ApplicationCode = "VOC";
			headerZA2.AMA_ManifestType = "ALM";

			AssertManifestTypesForTransportModes(headerZA2, headerZA2.Lookups, "Manifest Types for VOC ZAManifest", ("SEA", "ALM, AQM, BBB, COM, ECL"), ("AIR", "FFM, FWB"), ("ROA", ""), ("RAI", "RMA"));

			headerZA2.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			headerZA2.AMA_ApplicationCode = ZString.Empty;
			AssertNoExceptionThrown(() => { headerZA2.Lookups.ManifestTypes.GetAllCodes(); });

			var headerTR = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			headerTR.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			headerTR.AMA_ApplicationCode = "VOC";
			headerTR.AMA_ManifestType = "ATAIHR";
			AssertManifestTypesForTransportModes(headerTR, headerTR.Lookups, "Manifest Types for VOC TRManifest"
						   , ("AIR", "ATAIHR, ATAITH, CIKONC, DIGIHR, DIGITH, GRUPAJ, HAVIHR, HAVITH, TESLIM, VARONC")
						   , ("RAI", "ATAIHR, ATAITH, CIKONC, DEMIHR, DEMITH, DIGIHR, DIGITH, GRUPAJ, TESLIM, VARONC")
						   , ("SEA", "ATAIHR, ATAITH, CIKONC, DENIHR, DENITH, DIGIHR, DIGITH, EMANIF, GRUPAJ, TESLIM, VARONC")
						   , ("ROA", "ATAIHR, ATAITH, CIKONC, DIGIHR, DIGITH, GRUPAJ, TESLIM, TIRIHR, TIRITH, VARONC")
						   , ("MAI", "ATAIHR, ATAITH, CIKONC, DIGIHR, DIGITH, GRUPAJ, TESLIM, VARONC")
						   , ("FIX", "ATAIHR, ATAITH, CIKONC, DIGIHR, DIGITH, GRUPAJ, TESLIM, VARONC")
						   , ("IWT", "ATAIHR, ATAITH, CIKONC, DIGIHR, DIGITH, GRUPAJ, TESLIM, VARONC")
						   , ("OWN", "ATAIHR, ATAITH, CIKONC, DIGIHR, DIGITH, GRUPAJ, TESLIM, VARONC"));
		}

		public void TestCustomsOfficeListShowsCodesForRelevantCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "PG", "Papua New Guinea", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var vuSAIR = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "SAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var attpk = helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR.PK, "PORT", "VUSAN");
			helper.CreateTransportModeForCusCodeList(vuSAIR.PK, "AIR");

			var vuSAIR2 = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "VULI", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var attpk2 = helper.CreateNewOrGetExistingCusCodeListAttribute(vuSAIR2.PK, "PORT", "VUVLI");
			helper.CreateTransportModeForCusCodeList(vuSAIR2.PK, "AIR");

			helper.CreateNewOrGetExistingCusCodeList("PG", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "JAS", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("SB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "HIRH", "Honiara Point Cruz", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "VU";
			header.AMA_RL_NKPortOfLoading = "VUSAN";
			var customsOffices = (CodeDescriptionPairList)header.Lookups.CustomsOffices;
			AssertEquals(true, customsOffices.ContainsCode("SAIR"));
			AssertEquals(false, customsOffices.ContainsCode("JAS"));

			header.AMA_RN_NKCountry = "PG";
			customsOffices = (CodeDescriptionPairList)header.Lookups.CustomsOffices;
			AssertEquals(true, customsOffices.ContainsCode("JAS"));
			AssertEquals(false, customsOffices.ContainsCode("SAIR"));
			header.AMA_RN_NKCountry = "SB";
			customsOffices = (CodeDescriptionPairList)header.Lookups.CustomsOffices;
			AssertEquals(true, customsOffices.ContainsCode("HIRH"));
			AssertEquals(false, customsOffices.ContainsCode("SAIR"));

			header.AMA_RN_NKCountry = "VU";
			header.AMA_RL_NKPortOfLoading = "VUTST";
			customsOffices = (CodeDescriptionPairList)header.Lookups.CustomsOffices;
			AssertEquals(false, customsOffices.ContainsCode("SAIR"));
			AssertEquals(false, customsOffices.ContainsCode("JAS"));

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);
			header.AMA_RL_NKPortOfLoading = "TSSAN";

			var routingSupport = consol as Freight.Business.IRoutingSupport;
			var transports = routingSupport.TransportsIncludingRelated.AddNew();
			transports.JW_RL_NKDiscPort = "VUSAN";
			transports.JW_RL_NKLoadPort = "VUVLI";
			customsOffices = (CodeDescriptionPairList)header.Lookups.CustomsOffices;
			AssertEquals(true, customsOffices.ContainsCode("SAIR"));
			AssertEquals(true, customsOffices.ContainsCode("VULI"));
			AssertEquals(false, customsOffices.ContainsCode("JAS"));
		}

		public void TestPortList()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("PORT", "Port");
			helper.CreateCusCodeType("CUTCS", "EXDOCS Skins and Hides Cut Codes");
			helper.CreateCusCodeList("SG", "PORT", "ADZZZ", yesterday, tomorrow);
			helper.CreateCusCodeList("SG", "PORT", "AODGR", yesterday, tomorrow);
			helper.CreateCusCodeList("SG", "PORT", "ARSDE", yesterday, tomorrow);

			helper.CreateCusCodeList("ZA", "PORT", "AUADO", yesterday, tomorrow);
			helper.CreateCusCodeList("SG", "PORT", "YTLON", today.AddDays(-2), yesterday);
			helper.CreateCusCodeList("SG", "CUTCS", "ARSDE", yesterday, tomorrow);
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var list = header.Lookups.CustomsLoadingPortList as BusinessObjectCollection;
				list.Load();
				AssertEquals(3, list.Count);
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "ADZZZ"));
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "AODGR"));
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "ARSDE"));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var list = header.Lookups.CustomsLoadingPortList as BusinessObjectCollection;
				list.Load();
				AssertEquals(1, list.Count);
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "AUADO"));
			}
		}

		public void TestNatures()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var list = header.Lookups.Natures;
				var natureList = new CodeDescriptionPairList();
				natureList.AddPair(ShipmentTypeList.Codes.Export22, ShipmentTypeList.Descriptions.Export22);
				natureList.AddPair(ShipmentTypeList.Codes.Import23, ShipmentTypeList.Descriptions.Import23);
				natureList.AddPair(ShipmentTypeList.Codes.Transhipment28, ShipmentTypeList.Descriptions.Transhipment28);
				natureList.AddPair(ShipmentTypeList.Codes.Transit24, ShipmentTypeList.Descriptions.Transit24);
				AssertEquals(natureList, list);

				header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "TESLIM");
				list = header.Lookups.Natures;
				natureList = new CodeDescriptionPairList();
				natureList.AddPair(ShipmentTypeList.Codes.Export22, ShipmentTypeList.Descriptions.Export22);
				natureList.AddPair(ShipmentTypeList.Codes.Import23, ShipmentTypeList.Descriptions.Import23);
				AssertEquals(natureList, list);
			}
		}

		public void TestAgentTypeList()
		{
			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				AssertEquals("BASE", "DRT, CLD, AGT, CHT, COU, OTH", header.Lookups.AgentTypeList.CodesAsString);
				header.AMA_TransportMode = "AIR";
				AssertEquals("BASE_AIR", "DRT, CLD, AGT, CHT, COU, OTH, CLA, CLM", header.Lookups.AgentTypeList.CodesAsString);
				header.AMA_ApplicationCode = "VOC";
				AssertEquals("BASE_AIR_VOC", "DRT, CLD, AGT, CHT, COU, OTH, CLA, CLM", header.Lookups.AgentTypeList.CodesAsString);
				header.AMA_TransportMode = "";
				AssertEquals("BASE_VOC", "DRT, CLD, AGT, CHT, COU, OTH", header.Lookups.AgentTypeList.CodesAsString);
			});
		}

		public void TestCarrierLookups()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertType(typeof(ShippingProviderCollection), header.Lookups.CarrierList);
			header.AMA_TransportMode = "AIR";
			AssertType(typeof(AirShippingProviderCollection), header.Lookups.CarrierList);
			header.AMA_TransportMode = "SEA";
			AssertType(typeof(SeaShippingProviderCollection), header.Lookups.CarrierList);
		}

		public void TestTransportModes_TransportModeList_ModeAndCountry()
		{
			EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory,
				new ModeAndCountry("AIR", "VU"),
				new ModeAndCountry("SEA", "VU"),
				new ModeAndCountry("AIR", "PG"),
				new ModeAndCountry("ROA", "PG"));
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			CombineAssertions(() =>
			{
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Vanuatu;
				AssertEquals("Country (VU) - all relevant modes", "AIR, SEA", header.Lookups.TransportModeList.CodesAsString);
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.PapuaNewGuinea;
				AssertEquals("Country (PG) - all relevant modes", "AIR, ROA", header.Lookups.TransportModeList.CodesAsString);
			});
		}

		public void TestTransportModes_TransportModeList_EmptyMode()
		{
			EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry("", "ER"));
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			AssertEquals("Country (ER) - all relevant modes", "AIR, MAI, ROA, SEA", header.Lookups.TransportModeList.CodesAsString);
		}

		public void TestGetAcceptableTransportModesFromApplicationBusinessProvider()
		{
			EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ModeAndCountry("", "ER"));
			AssertContainsExactElementsInAnyOrder(
				new[] { "AIR", "MAI", "ROA", "SEA" },
				AsycudaManifestHeaderLookups.GetAcceptableTransportModesFromApplicationBusinessProvider(Factory, Core.Constants.CountryCodes.Eritrea, ApplicationCodeTypeList.Codes.Consolidator, Directions.Unknown));
		}

		public void TestGetAcceptableTransportModesFromApplicationBusinessProvider_NotImplementedCountry()
		{
			AssertContainsExactElementsInAnyOrder(
				System.Array.Empty<string>(),
				AsycudaManifestHeaderLookups.GetAcceptableTransportModesFromApplicationBusinessProvider(Factory, Core.Constants.CountryCodes.FrenchSouthernTerritories, ApplicationCodeTypeList.Codes.Consolidator, Directions.Unknown));
		}

		public void TestContainerModes()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "BBB", ApplicationCodeTypeList.Codes.ShippingLine);
			header.FillWithValidTestData();
			header.AMA_TransportMode = "SEA";
			header.AMA_ManifestType = "BBB";
			var containerModes = header.Lookups.ContainerModes.ToArray();

			AssertEquals("ContainerModes", 4, containerModes.Length);
			Assert("ContainerModes must contain \"BBK\"", containerModes.Any(item => item.Code == "BBK"));
			Assert("ContainerModes must contain \"BLK\"", containerModes.Any(item => item.Code == "BLK"));
			Assert("ContainerModes must contain \"LQD\"", containerModes.Any(item => item.Code == "LQD"));
			Assert("ContainerModes must contain \"OTH\"", containerModes.Any(item => item.Code == "OTH"));

			var header2 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ECL");
			header2.FillWithValidTestData();
			header2.AMA_TransportMode = "SEA";
			header2.AMA_ManifestType = "ECL";
			var containerModes2 = header2.Lookups.ContainerModes.ToArray();
			AssertEquals("ContainerModes", 5, containerModes2.Length);
			Assert("ContainerModes must contain \"CNT\"", containerModes2.Any(item => item.Code == "CNT"));

			var header3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedKingdom, "BBB", ApplicationCodeTypeList.Codes.ShippingLine);
			header3.FillWithValidTestData();
			header3.AMA_TransportMode = "SEA";
			header3.AMA_ManifestType = "BBB";
			var containerModes3 = header3.Lookups.ContainerModes.ToArray();

			AssertEquals("ContainerModes", 5, containerModes3.Length);
			Assert("ContainerModes must contain \"BBK\"", containerModes3.Any(item => item.Code == "BBK" && item.Description == "Break Bulk"));
			Assert("ContainerModes must contain \"BLK\"", containerModes3.Any(item => item.Code == "BLK" && item.Description == "Bulk"));
			Assert("ContainerModes must contain \"CNT\"", containerModes3.Any(item => item.Code == "CNT" && item.Description == "Containerized"));
			Assert("ContainerModes must contain \"LQD\"", containerModes3.Any(item => item.Code == "LQD" && item.Description == "Liquid"));
			Assert("ContainerModes must contain \"OTH\"", containerModes3.Any(item => item.Code == "OTH" && item.Description == "Other"));
		}

		public void TestOriginPortList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var originPortList = header.Lookups.OriginPortList;
			AssertNotNull("OriginPortList", originPortList);
			AssertType<RefUNLOCOCollection>("OriginPortList", originPortList);
		}

		public void TestDestinationPortList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var destinationPortList = header.Lookups.DestinationPortList;
			AssertType<RefUNLOCOCollection>("DestinationPortList", destinationPortList);
		}

		public static void EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(BusinessObjectFactory factory, params ModeAndCountry[] allowedModesAndCountries)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			foreach (var pair in allowedModesAndCountries)
			{
				var queryZzd = new ZQuery(RefCusCodeListSchema.ZZD_ZZK_NKCodeType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry);
				queryZzd.AddToFilter(RefCusCodeListSchema.ZZD_ZZZ_NKDataGrouping, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
				queryZzd.AddToFilter(RefCusCodeListSchema.ZZD_Code, pair.Country);
				var zzd = factory.LoadTop1<RefCusCodeList>(queryZzd);
				if (zzd == null)
				{
					helper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
					zzd = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry,
					pair.Country, "Lame", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
				}

				if (!pair.Mode.IsEmpty)
				{
					if (!RefTransportModesHelper.ExistsSpecificTransportModeForThatCodeTypeAndCountryOrGroupInZZDatabase(factory, zzd.ZZD_ZZK_NKCodeType, zzd.ZZD_ZZZ_NKDataGrouping,
						pair.Mode))
					{
						helper.CreateTransportModeForCusCodeList(zzd.PK, pair.Mode);
					}
				}

				helper.CreateNewOrGetExistingCusCodeListAttribute(zzd.PK, pair.Type, pair.VersionNo);
			}

			factory.Save();
		}

		void AssertManifestTypesForTransportModes(AsycudaManifestHeader header, AsycudaManifestHeaderLookups lookups, string message, params (string transportMode, string manifestTypes)[] testCases)
		{
			CombineAssertions(message, () =>
			{
				foreach (var testCase in testCases)
				{
					header.AMA_TransportMode = testCase.transportMode;
					AssertEquals(testCase.manifestTypes, lookups.ManifestTypes.CodesAsString);
				}
			});
		}
	}
}
