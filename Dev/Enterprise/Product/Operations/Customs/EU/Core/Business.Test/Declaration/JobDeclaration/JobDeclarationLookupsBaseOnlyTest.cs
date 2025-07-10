using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using TransportTypeListCodes = Enterprise.Customs.Business.TransportTypeList.Codes;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobDeclarationLookupsBaseOnlyTest : JobDeclarationLookupsTest<JobDeclarationLookups, JobDeclaration>
	{
		public void TestProfileList()
		{
			AssertEquals(ZString.Empty, lookups.ProfileList.CodesAsString);
		}

		public void TestInlandVesselNamesOrLloyds()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
			declaration.JE_TransportMeans = TransportMeansList.Codes.ImoShipIdentificationNumber;
			var inlandVesselNamesOrLloyds = declaration.Lookups.InlandVesselNamesOrLloyds;
			AssertType<RefVesselCollection>("InlandVesselNamesOrLloyds", inlandVesselNamesOrLloyds);
			AssertEquals("Sea - 10 - UseLloyds", true, inlandVesselNamesOrLloyds.UseLloyds);

			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Air;
			AssertEquals("Air - 10 - UseLloyds", false, declaration.Lookups.InlandVesselNamesOrLloyds.UseLloyds);

			declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
			declaration.JE_TransportMeans = TransportMeansList.Codes.NameOfTheSeaGoingVessel;
			AssertEquals("Sea - 11 - UseLloyds", false, declaration.Lookups.InlandVesselNamesOrLloyds.UseLloyds);
		}

		public void TestDefermentPartyCollection()
		{
			AssertType<OrganisationsFindBoxCollection>(lookups.DefermentPartyCollection);
		}

		public void TestBuyers()
		{
			AssertType<ConsigneeCollection>(lookups.Buyers);
		}

		public void TestBox18TransportCountryList()
		{
			AssertType<RefCountryCollection>(lookups.Box18TransportCountryList);
		}

		public void TestCargoIdTypeList()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertContains(Constants.ContainerModes.RollOnRollOff, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertContains(Constants.ContainerModes.ULD, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertContains(Constants.ContainerModes.Loose, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertContains(Constants.ContainerModes.NonContainerised, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertNotContains(Constants.ContainerModes.AgentConsol, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);

			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertContains(Constants.ContainerModes.ULD, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertContains(Constants.ContainerModes.Loose, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertNotContains(Constants.ContainerModes.Liquid, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertNotContains(Constants.ContainerModes.AgentConsol, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertNotContains(Constants.ContainerModes.BuyersConsol, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);

			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertNotContains(Constants.ContainerModes.ULD, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertNotContains(Constants.ContainerModes.Loose, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertContains(Constants.ContainerModes.Liquid, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertContains(Constants.ContainerModes.FCL, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertNotContains(Constants.ContainerModes.AgentConsol, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);

			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertNotContains(Constants.ContainerModes.ULD, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertNotContains(Constants.ContainerModes.Loose, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertContains(Constants.ContainerModes.Liquid, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertContains(Constants.ContainerModes.FCL, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertNotContains(Constants.ContainerModes.AgentConsol, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);

			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertNotContains(Constants.ContainerModes.ULD, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertNotContains(Constants.ContainerModes.Loose, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertNotContains(Constants.ContainerModes.Liquid, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertContains(Constants.ContainerModes.FCL, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertContains(Constants.ContainerModes.FTL, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
			AssertNotContains(Constants.ContainerModes.AgentConsol, jobDeclaration.Lookups.CargoIdTypeList.CodesAsString);
		}

		[TestDate(2019, 3, 17)]
		public void TestIATALoadPorts()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.France, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			var airportVN = Factory.New<RefUNLOCO>();
			airportVN.RL_RN_NKCountryCode = "VN";
			airportVN.RL_Code = "VNDQ1";
			airportVN.RL_HasAirport = true;
			airportVN.RL_IATA = "DQ1";

			var airportFR = Factory.New<RefUNLOCO>();
			airportFR.RL_RN_NKCountryCode = "FR";
			airportFR.RL_Code = "FRCD1";
			airportFR.RL_HasAirport = true;
			airportFR.RL_IATA = "CD1";
			Factory.Save();

			CombineAssertions(() =>
			{
				var q = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.VietNam);
				q.AddToFilter(RefUNLOCOSchema.RL_IATA, SQLComparisonOperator.NotEqual, "");
				q.AddToFilter(RefUNLOCOSchema.RL_HasAirport, true);
				var vietnameseLocos = Factory.Load<RefUNLOCO>(q);
				AssertEquals("Airports from Vietnam should be present", true, vietnameseLocos.Any());

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_RL_NKPortOfLoading = "VNDQT";  // VietNam - few ports, one with mismatch (VNDQT/VCL)
				var list = declaration.Lookups.IATALoadPorts;
				list.Load();
				AssertEquals("For import, all airports from Vietnam should be present", vietnameseLocos.Length, list.Count);
				AssertEquals("For import , all VN airports should be present", true, list.Cast<Airport>().Any(c => c.RL_IATA == "DQ1"));

				declaration.JE_RL_NKPortOfLoading = "FRPAR";
				list = declaration.Lookups.IATALoadPorts;
				list.Load();
				AssertEquals("For import not CO, EU airports should never be present", false, list.Cast<Airport>().Any(c => c.RL_IATA == "CD1"));
			});
		}

		public void TestIATALoadPorts_ImportAirTerritoryCO()
		{
			SetUpTradeGroupEUCAndCUAM2();
			SetUpAirPorts();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;

			declaration.JE_RL_NKPortOfLoading = "FRCD1";
			var list = declaration.Lookups.IATALoadPorts;
			list.Load();
			AssertEquals("EU airports must be present when EntryStyle=CO", true, list.Cast<Airport>().Any(c => c.RL_IATA == "CD1"));

			declaration.JE_RL_NKPortOfLoading = "MQFDF";
			list = declaration.Lookups.IATALoadPorts;
			list.Load();
			AssertEquals("CUAM airports must be present when EntryStyle=CO", true, list.Cast<Airport>().Any(c => c.RL_IATA == "KKK"));

			void SetUpAirPorts()
			{
				var airport = Factory.New<RefUNLOCO>();
				airport.RL_RN_NKCountryCode = "FR";
				airport.RL_Code = "FRCD1";
				airport.RL_HasAirport = true;
				airport.RL_IATA = "CD1";
				Factory.Save();

				var airportMQ = Factory.New<RefUNLOCO>();
				airportMQ.RL_RN_NKCountryCode = "MQ";
				airportMQ.RL_Code = "MQKKK";
				airportMQ.RL_HasAirport = true;
				airportMQ.RL_IATA = "KKK";
				Factory.Save();
			}
		}

		[TestDate(2019, 3, 17)]
		public void TestPortOfFirstArrival()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var tradeGroup = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup, "CQ", new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));

			var country = Factory.New<RefCountry>();
			country.RN_Code = "CQ";
			country.RN_EconomicGrouping = Enterprise.MasterFiles.Business.EconomicGroupList.Codes.EuropeanUnion;

			Factory.Save();

			var port = Factory.New<RefUNLOCO>();
			port.RL_RN_NKCountryCode = "CQ";
			port.RL_Code = "CQCKK";
			port.RL_HasSeaport = true;
			Factory.Save();

			SetupAndRunPortOfFirstArrivalTest(Core.Constants.TransportModes.Sea);

			ResetPortTypes(port);
			port.RL_HasAirport = true;
			Factory.Save();
			SetupAndRunPortOfFirstArrivalTest(Core.Constants.TransportModes.Air);

			ResetPortTypes(port);
			port.RL_HasRail = true;
			Factory.Save();
			SetupAndRunPortOfFirstArrivalTest(Core.Constants.TransportModes.Rail);

			ResetPortTypes(port);
			port.RL_HasPost = true;
			Factory.Save();
			SetupAndRunPortOfFirstArrivalTest(Core.Constants.TransportModes.Mail);
		}

		void ResetPortTypes(RefUNLOCO port)
		{
			port.RL_HasSeaport = false;
			port.RL_HasAirport = false;
			port.RL_HasRail = false;
			port.RL_HasPost = false;
		}

		void SetupAndRunPortOfFirstArrivalTest(ZString transportMode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CQ"))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_TransportMode = transportMode;
				declaration.JE_RL_NKPortOfFirstArrival = "CQCKK";
				AssertNoMessageErrors(declaration.JE_RL_NKPortOfFirstArrivalInfo);

				declaration.JE_RL_NKPortOfFirstArrival = "AUBNE";
				AssertHasMessageErrors(declaration.JE_RL_NKPortOfFirstArrivalInfo);
			}
		}

		public void TestModeOfTransportList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobDeclarationLookups lookups = new JobDeclarationLookups(declaration);
			AssertEquals(typeof(ModeOfTransportList), lookups.ModeOfTransportList.GetType());
		}

		public void TestDeclarantTypeList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobDeclarationLookups lookups = new JobDeclarationLookups(declaration);
			AssertEquals(typeof(RepresentationTypeList), lookups.DeclarantTypeList.GetType());
		}

		public void TestRepresentativeList()
		{
			var lookups = new JobDeclarationLookups(Factory.New<JobDeclaration>());
			var list = lookups.RepresentativeList;
			AssertType<BrokerCollection>(list);
			AssertSame(list, lookups.RepresentativeList);
		}

		public void TestSellerList()
		{
			var lookups = new JobDeclarationLookups(Factory.New<JobDeclaration>());
			var list = lookups.SellerList;
			AssertType<ConsignorCollection>(list);
			AssertSame(list, lookups.SellerList);
		}

		public void TestEntryStyleList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobDeclarationLookups lookups = new JobDeclarationLookups(declaration);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("lookups.EntryStyleList.GetType()", typeof(CodeDescriptionPairList), lookups.EntryStyleList.GetType());
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("lookups.EntryStyleList.GetType()", typeof(CodeDescriptionPairList), lookups.EntryStyleList.GetType());
			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("lookups.MessageTypeList.GetHumanReadableListOfElements(\"|\")", "", lookups.EntryStyleList.GetHumanReadableListOfElements("|"));
		}

		public void TestImportEntryStyleList_NotUCC()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = new JobDeclarationLookups(declaration);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				var list = declaration.Lookups.EntryStyleList;
				AssertEquals(false, declaration.IsUCCCompliant);
				AssertContainsExactElementsInAnyOrder(
					new[] { EntryStyleListImport.Codes.ImportFromEFTAMember, EntryStyleListImport.Codes.ImportFromSpecialTerritory, EntryStyleListImport.Codes.ImportNormal },
					declaration.Lookups.EntryStyleList.GetAllCodes());
				AssertEquals("Import of Goods from an EFTA Member State", list.GetDescriptionFromCode(EntryStyleListImport.Codes.ImportFromEFTAMember));
				AssertEquals("Import of Goods from a Special Territory of the Community", list.GetDescriptionFromCode(EntryStyleListImport.Codes.ImportFromSpecialTerritory));
				AssertEquals("Import of Goods (All Other not covered by CO or EU)", list.GetDescriptionFromCode(EntryStyleListImport.Codes.ImportNormal));
			});
		}

		public void TestImportEntryStyleList_IsUCC()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = new JobDeclarationLookups(declaration);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var list = declaration.Lookups.EntryStyleList;
					AssertEquals(true, declaration.IsUCCCompliant);
					AssertContainsExactElementsInAnyOrder(
						new[] { EntryStyleListImport.Codes.ImportFromSpecialTerritory, EntryStyleListImport.Codes.ImportNormal },
						declaration.Lookups.EntryStyleList.GetAllCodes());
					AssertEquals("Import of Goods from a Special Territory of the Community", list.GetDescriptionFromCode(EntryStyleListImport.Codes.ImportFromSpecialTerritory));
					AssertEquals("Import of Goods (All Other not covered by CO)", list.GetDescriptionFromCode(EntryStyleListImport.Codes.ImportNormal));
				}
			});
		}

		public void TestExportEntryStyleList_NotUCC()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = new JobDeclarationLookups(declaration);
			AssertEquals(typeof(CodeDescriptionPairList), lookups.EntryStyleList.GetType());
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				var list = declaration.Lookups.EntryStyleList;
				AssertEquals(false, declaration.IsUCCCompliant);
				AssertContainsExactElementsInAnyOrder(
					new[] { EntryStyleListExport.Codes.ExportToEFTAMember, EntryStyleListExport.Codes.ExportToSpecialTerritory, EntryStyleListExport.Codes.ExportNormal },
					declaration.Lookups.EntryStyleList.GetAllCodes());
				AssertEquals("Export of Goods (All Other not covered by CO or EU)", list.GetDescriptionFromCode(EntryStyleListExport.Codes.ExportNormal));
				AssertEquals("Export of Goods to an EFTA Member State", list.GetDescriptionFromCode(EntryStyleListExport.Codes.ExportToEFTAMember));
				AssertEquals("Export of Goods in Free Circulation to a Special Territory of the Community", list.GetDescriptionFromCode(EntryStyleListExport.Codes.ExportToSpecialTerritory));
			});
		}

		public void TestExportEntryStyleList_IsUCC()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = new JobDeclarationLookups(declaration);
			AssertEquals(typeof(CodeDescriptionPairList), lookups.EntryStyleList.GetType());
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var list = declaration.Lookups.EntryStyleList;
					AssertEquals(true, declaration.IsUCCCompliant);
					AssertContainsExactElementsInAnyOrder(
						new[] { EntryStyleListExport.Codes.ExportToSpecialTerritory, EntryStyleListExport.Codes.ExportNormal },
						declaration.Lookups.EntryStyleList.GetAllCodes());
					AssertEquals("Export or re-export of goods outside of the customs territory of the Union", list.GetDescriptionFromCode(EntryStyleListExport.Codes.ExportNormal));
					AssertEquals("Trade of Union goods between EU customs territory not covered by the Council Directives 2006/112/EC or 2008/118/EC", list.GetDescriptionFromCode(EntryStyleListExport.Codes.ExportToSpecialTerritory));
				}
			});
		}

		public void TestEntryStyleList_FR()
		{
			var localAdminForStaff = Factory.NewWithValidTestData<GlbStaff>();
			localAdminForStaff.GS_WorkingLanguage = Constants.CountryCodes.France;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);

				var europeanUnionDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.France, parent: europeanUnionDataGrouping);

				helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "Entry style EU customs");
				var fr = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.France, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "FR", "Movement of goods to/from an EU member state", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var im = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.France, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "IM", "Import of goods (All other not covered by FR, CO or EU)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var ex = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.France, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "EX", "Export of goods (All other not covered by FR, CO or EU)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var eu = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.France, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "EU", "Movement of goods to/from a third party country eligible to a common transit procedure", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var co = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.France, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "CO", "Movement of goods to/from a french oversea department or a EU special territory", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				Factory.Save();

				var entryType = Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType;

				helper.CreateNewOrGetExistingRefCusCodeListAttributeName(entryType, "Desc.", Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, Core.Constants.CountryCodes.France);

				helper.CreateCusCodeListAttribute(fr.PK, entryType, MessageTypeList.Codes.Import);
				helper.CreateCusCodeListAttribute(fr.PK, entryType, MessageTypeList.Codes.Export);

				helper.CreateCusCodeListAttribute(im.PK, entryType, MessageTypeList.Codes.Import);

				helper.CreateCusCodeListAttribute(ex.PK, entryType, MessageTypeList.Codes.Export);

				helper.CreateCusCodeListAttribute(eu.PK, entryType, MessageTypeList.Codes.Import);
				helper.CreateCusCodeListAttribute(eu.PK, entryType, MessageTypeList.Codes.Export);

				helper.CreateCusCodeListAttribute(co.PK, entryType, MessageTypeList.Codes.Import);
				helper.CreateCusCodeListAttribute(co.PK, entryType, MessageTypeList.Codes.Export);

				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				var lookups = new JobDeclarationLookups(declaration);
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				CombineAssertions(() =>
				{
					AssertEquals("Assertion for Import - ", @"CO - Movement of goods to/from a french oversea department or a EU special territory
EU - Movement of goods to/from a third party country eligible to a common transit procedure
FR - Movement of goods to/from an EU member state
IM - Import of goods (All other not covered by FR, CO or EU)", lookups.EntryStyleList.ElementsAsString);

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					AssertEquals("Assertion for Export - ", @"CO - Movement of goods to/from a french oversea department or a EU special territory
EU - Movement of goods to/from a third party country eligible to a common transit procedure
EX - Export of goods (All other not covered by FR, CO or EU)
FR - Movement of goods to/from an EU member state", lookups.EntryStyleList.ElementsAsString);

					declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
					AssertEquals(ZString.Empty, lookups.EntryStyleList.ElementsAsString);
				});
			}
		}

		public void TestEntryStyleList_FR_DescriptionInLocalLanguage()
		{
			var localAdminForStaff = Factory.NewWithValidTestData<GlbStaff>();
			localAdminForStaff.GS_WorkingLanguage = Constants.CountryCodes.France;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);

				var europeanUnionDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: europeanUnionDataGrouping);

				helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "Entry style EU customs");
				var fr = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.France, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "FR", "Movement of goods to/from an EU member state", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var im = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.France, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "IM", "Import of goods (All other not covered by FR, CO or EU)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var ex = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.France, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "EX", "Export of goods (All other not covered by FR, CO or EU)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var eu = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.France, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "EU", "Movement of goods to/from a third party country eligible to a common transit procedure", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var co = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.France, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "CO", "Movement of goods to/from a french oversea department or a EU special territory", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				Factory.Save();

				var entryType = Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType;

				helper.CreateNewOrGetExistingRefCusCodeListAttributeName(entryType, "Desc.", Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, Core.Constants.CountryCodes.France);

				helper.CreateCusCodeListAttribute(fr.PK, entryType, MessageTypeList.Codes.Import);
				helper.CreateCusCodeListAttribute(fr.PK, entryType, MessageTypeList.Codes.Export);

				helper.CreateCusCodeListAttribute(im.PK, entryType, MessageTypeList.Codes.Import);

				helper.CreateCusCodeListAttribute(ex.PK, entryType, MessageTypeList.Codes.Export);

				helper.CreateCusCodeListAttribute(eu.PK, entryType, MessageTypeList.Codes.Import);
				helper.CreateCusCodeListAttribute(eu.PK, entryType, MessageTypeList.Codes.Export);

				helper.CreateCusCodeListAttribute(co.PK, entryType, MessageTypeList.Codes.Import);
				helper.CreateCusCodeListAttribute(co.PK, entryType, MessageTypeList.Codes.Export);

				Factory.Save();

				helper.CreateOrGetLanguage(Constants.CountryCodes.France, "French");

				helper.CreateCusCodeListLanguage(fr, Constants.CountryCodes.France, "Mouvement de marchandises au sein de l'UE");
				helper.CreateCusCodeListLanguage(im, Constants.CountryCodes.France, "Import de marchandises en provenance d'un pays tiers (non couvert par FR, CO ou EU)");
				helper.CreateCusCodeListLanguage(ex, Constants.CountryCodes.France, "Export de marchandises vers un pays tiers (non couvert par FR, CO ou EU)");
				helper.CreateCusCodeListLanguage(eu, Constants.CountryCodes.France, "Mouvement de marchandises vers ou en provenance d'un pays tiers éligible aux procédures de transit communautaire");
				helper.CreateCusCodeListLanguage(co, Constants.CountryCodes.France, "Mouvement de marchandises vers ou en provenance d'un département d'outre-mer ou d'un territoire spécial de l'UE");

				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				var lookups = new JobDeclarationLookups(declaration);
				declaration.JE_MessageType = MessageTypeList.Codes.Import;

				using (CurrentUserChanger.SwitchToNewUserTemporarily(localAdminForStaff.GS_LoginName))
				{
					CombineAssertions(() =>
					{
						AssertEquals("Assertion for Import - ", @"CO - Mouvement de marchandises vers ou en provenance d'un département d'outre-mer ou d'un territoire spécial de l'UE
EU - Mouvement de marchandises vers ou en provenance d'un pays tiers éligible aux procédures de transit communautaire
FR - Mouvement de marchandises au sein de l'UE
IM - Import de marchandises en provenance d'un pays tiers (non couvert par FR, CO ou EU)", lookups.EntryStyleList.ElementsAsString);

						declaration.JE_MessageType = MessageTypeList.Codes.Export;
						AssertEquals("Assertion for Export - ", @"CO - Mouvement de marchandises vers ou en provenance d'un département d'outre-mer ou d'un territoire spécial de l'UE
EU - Mouvement de marchandises vers ou en provenance d'un pays tiers éligible aux procédures de transit communautaire
EX - Export de marchandises vers un pays tiers (non couvert par FR, CO ou EU)
FR - Mouvement de marchandises au sein de l'UE", lookups.EntryStyleList.ElementsAsString);
						declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
						AssertEquals(ZString.Empty, lookups.EntryStyleList.ElementsAsString);
					});
				}
			}
		}

		public void TestMessageTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = new JobDeclarationLookups(declaration);
			var list1 = lookups.MessageTypeList;
			var list2 = lookups.MessageTypeList;
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			AssertEquals("EXP - Export|IMP - Import|MSC - Miscellaneous Customs", list1.GetHumanReadableListOfElements("|"));
		}

		public void TestTransportTypeList()
		{
			CombineAssertions(() =>
			{
				AssertEquals(
					"Correct Codes and Description",
					"AIR - Air Freight|FIX - Fixed Transport Installations|IWT - Inland Waterways|OWN - Own Propulsion|MAI - Post/Mail|RAI - Rail Freight|ROA - Road Freight|SEA - Sea Freight",
					lookups.TransportTypeList.GetHumanReadableListOfElements("|"));
				AssertSame("Cached in Factory", lookups.TransportTypeList, Factory.GetCachedValue<TransportTypeList>());
			});
		}

		public void TestTransportModeCodesWorkWithDeclarationIsFlags()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("declaration.IsAir", true, declaration.IsAir);
			AssertEquals("declaration.IsRoad", false, declaration.IsRoad);
			AssertEquals("declaration.IsRail", false, declaration.IsRail);
			AssertEquals("declaration.IsSea", false, declaration.IsSea);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("declaration.IsAir", false, declaration.IsAir);
			AssertEquals("declaration.IsRoad", true, declaration.IsRoad);
			AssertEquals("declaration.IsRail", false, declaration.IsRail);
			AssertEquals("declaration.IsSea", false, declaration.IsSea);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("declaration.IsAir", false, declaration.IsAir);
			AssertEquals("declaration.IsRoad", false, declaration.IsRoad);
			AssertEquals("declaration.IsRail", true, declaration.IsRail);
			AssertEquals("declaration.IsSea", false, declaration.IsSea);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("declaration.IsAir", false, declaration.IsAir);
			AssertEquals("declaration.IsRoad", false, declaration.IsRoad);
			AssertEquals("declaration.IsRail", false, declaration.IsRail);
			AssertEquals("declaration.IsSea", true, declaration.IsSea);
		}

		public void TestImportEntryStyleListCombinations()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType);
			helper.CreateCusCodeListWithAttribute(currentCountry, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListImport.Codes.ImportFromEFTAMember, "Import of Goods from an EFTA Member State", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType, MessageTypeList.Codes.Import);
			helper.CreateCusCodeListWithAttribute(currentCountry, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListImport.Codes.ImportFromSpecialTerritory, "Import of Goods from a Special Territory of the Community", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType, MessageTypeList.Codes.Import);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.SouthAfrica, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListExport.Codes.ExportToEFTAMember, "Import of Goods (All Other not covered by CO or EU)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType, MessageTypeList.Codes.Import);
			helper.CreateCusCodeListWithAttribute(currentCountry, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "AA", "AA for test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType, MessageTypeList.Codes.Import);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var list = declaration.Lookups.EntryStyleList;
			var codesAsString = list.CodesAsString;

			CombineAssertions(() =>
			{
				AssertContains(EntryStyleListImport.Codes.ImportFromEFTAMember, codesAsString);
				AssertContains(EntryStyleListImport.Codes.ImportFromSpecialTerritory, codesAsString);
				AssertContains(EntryStyleListImport.Codes.ImportNormal, codesAsString);
				AssertContains("AA", codesAsString);

				AssertEquals(4, declaration.Lookups.EntryStyleList.Count);

				AssertEquals("Import of Goods from an EFTA Member State", list.GetDescriptionFromCode(EntryStyleListImport.Codes.ImportFromEFTAMember));
				AssertEquals("Import of Goods from a Special Territory of the Community", list.GetDescriptionFromCode(EntryStyleListImport.Codes.ImportFromSpecialTerritory));
				AssertEquals("Import of Goods (All Other not covered by CO or EU)", list.GetDescriptionFromCode(EntryStyleListImport.Codes.ImportNormal));
				AssertEquals("AA for test", list.GetDescriptionFromCode("AA"));
			});
		}

		public void TestExportEntryStyleListCombinations()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType);
			helper.CreateCusCodeListWithAttribute(currentCountry, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListExport.Codes.ExportNormal, "Export of Goods (All Other not covered by CO or EU)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType, MessageTypeList.Codes.Export);
			helper.CreateCusCodeListWithAttribute(currentCountry, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListExport.Codes.ExportToEFTAMember, "Export of Goods to an EFTA Member State", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType, MessageTypeList.Codes.Export);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.SouthAfrica, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, EntryStyleListExport.Codes.ExportToSpecialTerritory, "Export of Goods in Free Circulation to a Special Territory of the Community", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType, MessageTypeList.Codes.Export);
			helper.CreateCusCodeListWithAttribute(currentCountry, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty, "AA", "AA for test", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Constants.Customs.Universal.RefCusCodeList.Attributes.EntryType, MessageTypeList.Codes.Export);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			var list = declaration.Lookups.EntryStyleList;
			var codesAsString = list.CodesAsString;

			CombineAssertions(() =>
			{
				AssertContains(EntryStyleListExport.Codes.ExportNormal, codesAsString);
				AssertContains(EntryStyleListExport.Codes.ExportToEFTAMember, codesAsString);
				AssertContains(EntryStyleListExport.Codes.ExportToSpecialTerritory, codesAsString);
				AssertContains("AA", codesAsString);

				AssertEquals(4, declaration.Lookups.EntryStyleList.Count);

				AssertEquals("Export of Goods (All Other not covered by CO or EU)", list.GetDescriptionFromCode(EntryStyleListExport.Codes.ExportNormal));
				AssertEquals("Export of Goods to an EFTA Member State", list.GetDescriptionFromCode(EntryStyleListExport.Codes.ExportToEFTAMember));
				AssertEquals("Export of Goods in Free Circulation to a Special Territory of the Community", list.GetDescriptionFromCode(EntryStyleListExport.Codes.ExportToSpecialTerritory));
				AssertEquals("AA for test", list.GetDescriptionFromCode("AA"));
			});
		}

		public void TestPackageTypeLookups()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package List");
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					"AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					"BG", "BG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				 "BBB", "BBB DESC", new ZDateTime(2012, 3, 3), ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

				Factory.Save();

				var dec = Factory.New<JobDeclaration>();
				dec.JE_MasterBill = "123";
				var pack = Factory.New<PackageForTest>();
				dec.Packages.Add(pack);
				Assert("BG", pack.PackTypeListExposed.ContainsCode("BG"));
				Assert("BAG should not appear as it is not in the list", !pack.PackTypeListExposed.ContainsCode("BAG"));
				Assert("BBB should not appear as it is too new", !pack.PackTypeListExposed.ContainsCode("BBB"));
				Assert("AAA", pack.PackTypeListExposed.ContainsCode("AAA"));
			}
		}

		public void TestCustomsOffices_IsRelevantToIsLocalCountryOnlyProperty()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			helper.CreateCusCodeType(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE EXT", "DE EXT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BOX", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT", yesterday, tomorrow);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT EXT", "IT EXT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT ENT", "IT ENT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "ENT");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var mainOfficeRequirement = declaration.CustomsOfficeRequirementHelper.MainOffice;
				mainOfficeRequirement.OfficeRole = "EXT";

				mainOfficeRequirement.IsLocalCountryOnly = true;
				var customsOffice = declaration.Lookups.CustomsOffices;
				customsOffice.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "IT EXT" }, customsOffice.Select(x => x.ZZD_Code));

				mainOfficeRequirement.IsLocalCountryOnly = false;
				customsOffice = declaration.Lookups.CustomsOffices;
				customsOffice.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "IT EXT", "DE EXT" }, customsOffice.Select(x => x.ZZD_Code));
			}
		}

		public void TestCustomsOffices_IsForeignOnly()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "Package Types");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE EXT", "DE EXT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BOX", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT", yesterday, tomorrow);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT EXT", "IT EXT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT ENT", "IT ENT", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "ENT");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var mainOfficeRequirement = declaration.CustomsOfficeRequirementHelper.MainOffice;
				mainOfficeRequirement.OfficeRole = "EXT";
				mainOfficeRequirement.IsForeignCountryOnly = true;
				mainOfficeRequirement.IsLocalCountryOnly = false;
				var customsOffice = declaration.Lookups.CustomsOffices;
				customsOffice.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "DE EXT" }, customsOffice.Select(x => x.ZZD_Code));
			}
		}

		public void TestGoodsOriginList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.Latvia, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "Origin country/territory for entry style IM");
			helper.CreateCusCodeList(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "AA", "Test AA", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX15, "Origin country/territory for entry style EX");
			helper.CreateCusCodeList(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX15, "ZZ", "Test ZZ", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "Origin country/territory for entry style CO");
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "BB", "Test BB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "CC", "Test CC", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Export);

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "Origin country/territory for entry style EU");
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "DD", "Test DD", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "EE", "Test EE", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertGoodsOriginListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Import, EntryStyleListImport.Codes.ImportNormal, expectedCodesAsString: "AA");
				AssertGoodsOriginListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Import, EntryStyleListImport.Codes.ImportFromSpecialTerritory, expectedCodesAsString: "BB");
				AssertGoodsOriginListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Import, EntryStyleListImport.Codes.ImportFromEFTAMember, expectedCodesAsString: "DD");
				AssertGoodsOriginListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Export, EntryStyleListExport.Codes.ExportNormal, expectedCodesAsString: "ZZ");
				AssertGoodsOriginListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Export, EntryStyleListExport.Codes.ExportToSpecialTerritory, expectedCodesAsString: "CC");
				AssertGoodsOriginListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Export, EntryStyleListExport.Codes.ExportToEFTAMember, expectedCodesAsString: "EE");
			});

			void AssertGoodsOriginListBasedOnDeclarationTypeAndEntryStyle(ZString messageType, ZString entryStyle, ZString expectedCodesAsString)
			{
				jobDeclaration.JE_MessageType = messageType;
				jobDeclaration.JE_EntryStyle = entryStyle;
				AssertEquals($"When JE_MessageType = {messageType} and JE_EntryStyle = {entryStyle}, GoodsOrigin CodesAsString", expectedCodesAsString, ((CodeDescriptionPairList)lookups.GoodsOrigin).CodesAsString);
			}
		}

		public void TestGoodsDestinationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.Latvia, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "Origin country/territory for entry style IM");
			helper.CreateCusCodeList(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM17, "AA", "Test AA", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "Origin country/territory for entry style EX");
			helper.CreateCusCodeList(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "ZZ", "Test ZZ", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "Origin country/territory for entry style CO");
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "BB", "Test BB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "CC", "Test CC", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Export);

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "Origin country/territory for entry style EU");
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "DD", "Test DD", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "EE", "Test EE", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertGoodsDestinationListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Import, EntryStyleListImport.Codes.ImportNormal, expectedCodesAsString: "AA");
				AssertGoodsDestinationListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Import, EntryStyleListImport.Codes.ImportFromSpecialTerritory, expectedCodesAsString: "BB");
				AssertGoodsDestinationListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Import, EntryStyleListImport.Codes.ImportFromEFTAMember, expectedCodesAsString: "DD");
				AssertGoodsDestinationListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Export, EntryStyleListExport.Codes.ExportNormal, expectedCodesAsString: "ZZ");
				AssertGoodsDestinationListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Export, EntryStyleListExport.Codes.ExportToSpecialTerritory, expectedCodesAsString: "CC");
				AssertGoodsDestinationListBasedOnDeclarationTypeAndEntryStyle(EUJobMessageTypeList.Codes.Export, EntryStyleListExport.Codes.ExportToEFTAMember, expectedCodesAsString: "EE");
			});

			void AssertGoodsDestinationListBasedOnDeclarationTypeAndEntryStyle(ZString messageType, ZString entryStyle, ZString expectedCodesAsString)
			{
				jobDeclaration.JE_MessageType = messageType;
				jobDeclaration.JE_EntryStyle = entryStyle;
				AssertEquals($"When JE_MessageType = {messageType} and JE_EntryStyle = {entryStyle}, GoodsDestination CodesAsString", expectedCodesAsString, ((CodeDescriptionPairList)lookups.GoodsDestination).CodesAsString);
			}
		}

		public void TestOrigins_Import()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			jobDeclaration.JE_EntryStyle = ZString.Empty;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.Origins;
				AssertEquals("Contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("Contains eu ports", false, filter.Contains(rotterdam));
				AssertEquals("Does not contain local ports", false, filter.Contains(aglona));
			});
		}

		public void TestOrigins_Import_EntryStyle_CO()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.Origins;
				AssertEquals("Contains eu ports", true, filter.Contains(rotterdam));
				AssertEquals("Does not contain foreign ports", false, filter.Contains(sydney));
			});
		}

		public void TestOrigins_Import_EntryStyle_EU()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.Latvia, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "Origin country/territory for entry style EU");
			helper.CreateCusCodeListWithAttribute(Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "GB", "Test GB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			Factory.Save();

			var loader = new RefUNLOCO.Loader(Factory);
			var london = loader.Load("GBLON");
			var berlin = loader.Load("DEBER");
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromEFTAMember;
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.Origins;
				AssertEquals("GoodsOrigin codes", "GB", ((CodeDescriptionPairList)jobDeclaration.Lookups.GoodsOrigin).CodesAsString);
				AssertEquals("Contains EU15 code", true, filter.Contains(london));
				AssertEquals("Does not contain Non-EU15 code", false, filter.Contains(berlin));
			});
		}

		public void TestOrigins_Import_EntryStyle_IM()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.Origins;
				AssertEquals("Contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("Contains eu ports", true, filter.Contains(rotterdam));
				AssertEquals("Contains local ports", true, filter.Contains(aglona));
			});
		}

		public void TestOrigins_Export()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var (sydney, _, rotterdam) = SetupUnlocosForPorts();
				var loader = new RefUNLOCO.Loader(Factory);
				var france = loader.Load("FR222");
				var martinique = loader.Load("MQBFT");
				CombineAssertions(() =>
				{
					var filter = jobDeclaration.Lookups.Origins;
					AssertEquals("Does not contain foreign ports", false, filter.Contains(sydney));
					AssertEquals("Does not contain eu ports", false, filter.Contains(rotterdam));
					AssertEquals("Contains local ports", true, filter.Contains(france));
					AssertEquals("Contains jurisdiction ports", true, filter.Contains(martinique));
				});
			}
		}

		public void TestPortOfLoadings_Import()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			jobDeclaration.JE_EntryStyle = ZString.Empty;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.PortOfLoadings;
				AssertEquals("Contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("Does not contain eu ports", false, filter.Contains(rotterdam));
				AssertEquals("Does not contain local ports", false, filter.Contains(aglona));
			});
		}

		public void TestPortOfLoadings_Import_EntryStyle_CO()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
			var (sydney, _, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var portOfLoadings = jobDeclaration.Lookups.PortOfLoadings;
				AssertEquals("Contains eu ports", true, portOfLoadings.Contains(rotterdam));
				AssertEquals("Does not contain foreign ports", false, portOfLoadings.Contains(sydney));
			});
		}

		public void TestPortOfLoadings_Import_EntryStyle_EU()
		{
			var (sydney, aglona, _) = SetupUnlocosForPorts();
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromEFTAMember;
			CombineAssertions(() =>
			{
				var portOfLoadings = jobDeclaration.Lookups.PortOfLoadings;
				AssertEquals("Contains foreign ports", true, portOfLoadings.Contains(sydney));
				AssertEquals("Does not contain eu ports", false, portOfLoadings.Contains(aglona));
			});
		}

		public void TestPortOfLoadings_Export()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var (sydney, _, rotterdam) = SetupUnlocosForPorts();
				var loader = new RefUNLOCO.Loader(Factory);
				var france = loader.Load("FR222");
				var martinique = loader.Load("MQBFT");
				CombineAssertions(() =>
				{
					var filter = jobDeclaration.Lookups.Origins;
					AssertEquals("Does not contain foreign ports", false, filter.Contains(sydney));
					AssertEquals("Does not contain eu ports", false, filter.Contains(rotterdam));
					AssertEquals("Contains local ports", true, filter.Contains(france));
					AssertEquals("Contains jurisdiction ports", true, filter.Contains(martinique));
				});
			}
		}

		public void TestPortOfArrivals_Import()
		{
			SetUpTradeGroupEUCAndCUAM2();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				var (sydney, _, rotterdam) = SetupUnlocosForPorts();
				var loader = new RefUNLOCO.Loader(Factory);
				var france = loader.Load("FR222");
				var martinique = loader.Load("MQBFT");
				CombineAssertions(() =>
				{
					var filter = jobDeclaration.Lookups.PortOfArrivals;
					AssertEquals("Does not contain foreign ports", false, filter.Contains(sydney));
					AssertEquals("Contains eu ports", true, filter.Contains(rotterdam));
					AssertEquals("Contains local ports", true, filter.Contains(france));
					AssertEquals("Contains jurisdiction ports", true, filter.Contains(martinique));
				});
			}
		}

		public void TestPortOfArrivals_Export()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			jobDeclaration.JE_EntryStyle = ZString.Empty;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.PortOfArrivals;
				AssertEquals("Contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("Does not contain eu ports", false, filter.Contains(rotterdam));
				AssertEquals("Does not contain local ports", false, filter.Contains(aglona));
			});
		}

		public void TestPortOfArrivals_Export_EntryStyle_CO()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.PortOfArrivals;
				AssertEquals("Does not contain foreign ports", false, filter.Contains(sydney));
				AssertEquals("Contains eu ports", true, filter.Contains(rotterdam));
			});
		}

		public void TestPortOfArrivals_Export_EntryStyle_EU()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.PortOfArrivals;
				AssertEquals("Contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("Does not contain eu ports", false, filter.Contains(rotterdam));
			});
		}

		public void TestPortOfArrivals_Export_EntryStyle_EX()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.PortOfArrivals;
				AssertEquals("Contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("Contains eu ports", true, filter.Contains(rotterdam));
				AssertEquals("Contains local ports", true, filter.Contains(aglona));
			});
		}

		public void TestFinalDestinations_Import()
		{
			SetUpTradeGroupEUCAndCUAM2();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				var (sydney, _, rotterdam) = SetupUnlocosForPorts();
				var loader = new RefUNLOCO.Loader(Factory);
				var france = loader.Load("FR222");
				var martinique = loader.Load("MQBFT");
				CombineAssertions(() =>
				{
					var finalDestinations = jobDeclaration.Lookups.FinalDestinations;
					AssertEquals("Contains foreign ports", false, finalDestinations.Contains(sydney));
					AssertEquals("Contains eu ports", true, finalDestinations.Contains(rotterdam));
					AssertEquals("Contains local ports", true, finalDestinations.Contains(france));
					AssertEquals("Contains jurisdiction ports", true, finalDestinations.Contains(martinique));
				});
			}
		}

		public void TestFinalDestinations_Export()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			jobDeclaration.JE_EntryStyle = ZString.Empty;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.FinalDestinations;
				AssertEquals("Contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("Does not contain eu ports", false, filter.Contains(rotterdam));
				AssertEquals("Does not contain local ports", false, filter.Contains(aglona));
			});
		}

		public void TestFinalDestinations_Export_EntryStyle_CO()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.FinalDestinations;
				AssertEquals("Does not contain foreign ports", false, filter.Contains(sydney));
				AssertEquals("Contains eu ports", true, filter.Contains(rotterdam));
				AssertEquals("Contains local ports", true, filter.Contains(aglona));
			});
		}

		public void TestFinalDestinations_Export_EntryStyle_EU()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToEFTAMember;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.FinalDestinations;
				AssertEquals("Contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("Does not contain eu ports", false, filter.Contains(rotterdam));
				AssertEquals("Does not contain local ports", false, filter.Contains(aglona));
			});
		}

		public void TestFinalDestinations_Export_EntryStyle_EX()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			jobDeclaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			var (sydney, aglona, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.FinalDestinations;
				AssertEquals("Contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("Contains eu ports", true, filter.Contains(rotterdam));
				AssertEquals("Contains local ports", true, filter.Contains(aglona));
			});
		}

		public void TestPortOfFirstArrivals_Export()
		{
			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var (sydney, _, rotterdam) = SetupUnlocosForPorts();
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.PortOfFirstArrivals;
				AssertEquals("Contains eu ports", true, filter.Contains(rotterdam));
				AssertEquals("Does not contain foreign ports", false, filter.Contains(sydney));
			});
		}

		public void TestIncoTermList()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, false))
			{
				AssertNotContains(Constants.IncoTerms.Other, jobDeclaration.Lookups.IncoTermList.CodesAsString);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, true))
			{
				AssertContains(Constants.IncoTerms.Other, jobDeclaration.Lookups.IncoTermList.CodesAsString);
			}
		}

		public void TestTransportMeansList()
		{
			var declaration = Factory.New<JobDeclarationForTest>();

			AssertTransportMeansList(EUCommonConstants.TransportModeSource.InlandTransportMode, declaration, declaration.JE_TransportModeInlandInfo);
			AssertTransportMeansList(EUCommonConstants.TransportModeSource.TransportModeAtBorder, declaration, declaration.JE_TransportModeInfo);
		}

		void AssertTransportMeansList(EUCommonConstants.TransportModeSource transportMeansDependency, JobDeclarationForTest declaration, ZPropertyInfo transportModePropertyInfo)
		{
			declaration.TransportMeansDependencyForTest = transportMeansDependency;
			var importDefaultTransportMeansList = new MeansOfTransportList().CodesAsString;
			var exportDefaultTransportMeansList = new InlandMeansOfTransportList();
			exportDefaultTransportMeansList.Sort();
			var exportDefaultTransportMeansCodes = exportDefaultTransportMeansList.CodesAsString;
			var defaultTransportMeansList = new MeansOfTransportList().CodesAsString;

			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.Air;
					AssertEquals("Ucc6 import/export disabled", defaultTransportMeansList, declaration.AddInfoLookups.InlandTransportCodeList.CodesAsString);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals("Ucc6 import/export disabled", defaultTransportMeansList, declaration.AddInfoLookups.InlandTransportCodeList.CodesAsString);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					transportModePropertyInfo.Value = ZString.Empty;
					AssertEquals("default - Ucc6 export enabed", exportDefaultTransportMeansCodes, declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.Air;
					AssertEquals("air - Ucc6 export enabed", "40, 41", declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.FixedTransportInstallations;
					AssertEquals("fix - Ucc6 export enabed", exportDefaultTransportMeansCodes, declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.InlandWaterwayTransport;
					AssertEquals("iwt - Ucc6 export enabed", "80, 81", declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.OwnPropulsion;
					AssertEquals("own - Ucc6 export enabed", exportDefaultTransportMeansCodes, declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.Mail;
					AssertEquals("mai - Ucc6 export enabed", exportDefaultTransportMeansCodes, declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.Rail;
					AssertEquals("rai - Ucc6 export enabed", "20, 21", declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.Road;
					AssertEquals("roa - Ucc6 export enabed", "30, 31", declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.Sea;
					AssertEquals("sea - Ucc6 export enabed", "10, 11", declaration.Lookups.TransportMeansList.CodesAsString);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					transportModePropertyInfo.Value = ZString.Empty;
					AssertEquals("default - Ucc6 import enabed", importDefaultTransportMeansList, declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.Air;
					AssertEquals("air - Ucc6 import enabed", "40, 41", declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.FixedTransportInstallations;
					AssertEquals("fix - Ucc6 import enabed", importDefaultTransportMeansList, declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.InlandWaterwayTransport;
					AssertEquals("iwt - Ucc6 import enabed", "80, 81", declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.OwnPropulsion;
					AssertEquals("own - Ucc6 import enabed", importDefaultTransportMeansList, declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.Mail;
					AssertEquals("mai - Ucc6 import enabed", importDefaultTransportMeansList, declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.Rail;
					AssertEquals("rai - Ucc6 import enabed", "20, 21", declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.Road;
					AssertEquals("roa - Ucc6 import enabed", "30, 31", declaration.Lookups.TransportMeansList.CodesAsString);
					transportModePropertyInfo.Value = (ZString)TransportTypeListCodes.Sea;
					AssertEquals("sea - Ucc6 import enabed", "10, 11", declaration.Lookups.TransportMeansList.CodesAsString);
				}
			});
		}

		public void TestTransportCountryList_Export()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "Export Nationality");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "DE", "DEGUO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "ES", "XIBANYA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT, "FR", "FAGUO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var lookups = declaration.Lookups;
			var list = lookups.TransportCountryList as ZZRefCusCodeListCombinedCollection;
			list.Load();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Elements", new[] { "ES", "DE" }, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
				AssertSame("Cached", list, lookups.TransportCountryList);
			});
		}

		public void TestTransportCountryList_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var lookups = declaration.Lookups;
			var list = lookups.TransportCountryList;
			AssertType<RefCountryCollection>(list);
		}

		public void TestFinalDestinations_IMP_NoFilters()
		{
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;

			var result = lookups.PortOfArrivals;
			Assert(!result.Relationship.RelationshipFilter.FilterPartsHashKey.Contains("RL_HasAirport"));
		}

		void SetUpTradeGroupEUCAndCUAM2()
		{
			var (startDate, endDate) = (new ZDateTime(2019, 1, 1), new ZDateTime(2079, 1, 1));
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");

			var tradeGroupEUC = helper.CreateTradeGroup("EUN", "EUC", startDate, endDate);
			helper.AddCountry(tradeGroupEUC, Core.Constants.CountryCodes.France, startDate.Date, endDate.Date);
			helper.AddCountry(tradeGroupEUC, Core.Constants.CountryCodes.Netherlands, startDate.Date, endDate.Date);

			var tradeGroupCUAM = helper.CreateTradeGroup("EUN", Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.CustomsUnionAdditionalMembers, startDate, endDate);
			helper.AddCountry(tradeGroupCUAM, Core.Constants.CountryCodes.Martinique, startDate.Date, endDate.Date);
			Factory.Save();
		}

		(RefUNLOCO Sydney, RefUNLOCO Aglona, RefUNLOCO Rotterdam) SetupUnlocosForPorts()
		{
			var loader = new RefUNLOCO.Loader(Factory);
			var sydney = loader.Load("AUSYD");
			var aglona = loader.Load("LVAGL");
			var rotterdam = loader.Load("NLRTM");
			return (sydney, aglona, rotterdam);
		}

		sealed class PackageForTest : Package
		{
			public PackageForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public CodeDescriptionPairList PackTypeListExposed => PackTypeList;
		}

		sealed class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public EUCommonConstants.TransportModeSource TransportMeansDependencyForTest;

			protected override EUCommonConstants.TransportModeSource TransportMeansDependencyCore => TransportMeansDependencyForTest;
		}
	}
}
