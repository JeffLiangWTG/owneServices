using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class AddInfoJobDeclarationLookupsTest : EU.Business.Declaration.Testing.AddInfoJobDeclarationLookupsTest
	{
		public override void TestCommunityTransitStatusListExport()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("lookups.CommunityTransitStatusIDList.CodesAsString", "T2L, T2LF, T2LSM", declaration.AddInfoLookups.CommunityTransitStatusIDList.CodesAsString);
		}

		public override void TestSpecificCircumstanceIndicatorList()
		{
			CombineAssertions(() =>
			{
				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					var listImport = declaration.AddInfoLookups.SpecificCircumstanceIndicatorList;
					AssertEquals("lookups.SpecificCircumstanceIndicatorList.CodesAsString for import when UCC6", "A, B, E", listImport.CodesAsString);
					AssertSame("Testing cache for import UCC6", declaration.AddInfoLookups.SpecificCircumstanceIndicatorList, listImport);

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					var listExport = declaration.AddInfoLookups.SpecificCircumstanceIndicatorList;
					AssertEquals("lookups.SpecificCircumstanceIndicatorList.CodesAsString for export when UCC6", "A20, A, B, E", listExport.CodesAsString);
					AssertSame("Testing cache for export UCC6", declaration.AddInfoLookups.SpecificCircumstanceIndicatorList, listExport);
				}
			});
		}

		public void TestDestinationStateCodeList()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			helper.CreateCusCodeListTerritory(countryCode, "01", "Test 1");
			helper.CreateCusCodeListTerritory(countryCode, "02", "Test 2");
			helper.CreateCusCodeListTerritory(countryCode, "03", "Test 3");
			helper.CreateCusCodeListTerritory(countryCode, "04", "Test 4");
			helper.CreateCusCodeListNorthAfricanTerritory(countryCode, "05", "Test 5");
			helper.CreateCusCodeListNorthAfricanTerritory(countryCode, "06", "Test 6");
			helper.CreateCusCodeListCanaryIsland(countryCode, "07", "Test 7");
			helper.CreateCusCodeListCanaryIsland(countryCode, "08", "Test 8");
			helper.CreateCusCodeList("EUN", RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "09", "Test 9", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var lookups = declaration.Lookups.DestinationStateIslandCodeList;
				CombineAssertions(() =>
				{
					AssertEquals("Number of codes in list", 8, lookups.Count);
					Assert(lookups.ContainsCode("01"));
					Assert(lookups.ContainsCode("02"));
					Assert(lookups.ContainsCode("03"));
					Assert(lookups.ContainsCode("04"));
					Assert(lookups.ContainsCode("05"));
					Assert(lookups.ContainsCode("06"));
					Assert(lookups.ContainsCode("07"));
					Assert(lookups.ContainsCode("08"));
					AssertEquals("Test 1", lookups.GetDescriptionFromCode("01"));
				});
			}
		}

		public void TestBorderTransportMeansListImport()
		{
			var expectedAllList = "10, 11, 21, 30, 40, 41, 80, 81";
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var lookups = declaration.AddInfoLookups;

			CombineAssertions(() =>
			{
				var list = lookups.BorderTransportMeansList;
				AssertEquals("List is correct", expectedAllList, list.CodesAsString);
				AssertSame("Testing cache", lookups.BorderTransportMeansList, list);

				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
				{
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Sea, expectedAllList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Rail, expectedAllList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Road, expectedAllList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Air, expectedAllList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.InlandWaterwayTransport, expectedAllList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Other, expectedAllList, EXPORTVersionNumberList.Codes.Aes);
				}

				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
				{
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Sea, expectedAllList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Rail, expectedAllList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Road, expectedAllList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Air, expectedAllList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.InlandWaterwayTransport, expectedAllList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Other, expectedAllList, EXPORTVersionNumberList.Codes.Aes11);
				}
			});
		}

		public void TestBorderTransportMeansListExport()
		{
			var expectedAllList = "10, 11, 21, 30, 40, 41, 80, 81";
			var expectedSeaList = "10, 11";
			var expectedRailList = "21";
			var expectedRoadList = "30";
			var expectedAirList = "40, 41";
			var expectedIWTList = "80, 81";
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var lookupsAll = declaration.AddInfoLookups;

			CombineAssertions(() =>
			{
				var list = lookupsAll.BorderTransportMeansList;
				AssertEquals("List is correct", expectedAllList, list.CodesAsString);
				AssertSame("Testing cache", lookupsAll.BorderTransportMeansList, list);

				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
				{
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Sea, expectedSeaList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.AirSea, expectedAllList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Rail, expectedRailList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.SeaAir, expectedAllList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Road, expectedRoadList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.All, expectedAllList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Air, expectedAirList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Other, expectedAllList, EXPORTVersionNumberList.Codes.Aes);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.InlandWaterwayTransport, expectedIWTList, EXPORTVersionNumberList.Codes.Aes);
				}

				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
				{
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Sea, expectedSeaList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.AirSea, expectedAllList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Rail, expectedRailList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.SeaAir, expectedAllList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Road, expectedRoadList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.All, expectedAllList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Air, expectedAirList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.Other, expectedAllList, EXPORTVersionNumberList.Codes.Aes11);
					CompareCodesBorderTransportMeansList(Core.Constants.TransportModes.InlandWaterwayTransport, expectedIWTList, EXPORTVersionNumberList.Codes.Aes11);
				}
			});
		}

		public void CompareCodesBorderTransportMeansList(string transport, string result, string version)
		{
			declaration.JE_TransportMode = transport;
			var lookups = declaration.AddInfoLookups;
			var list = lookups.BorderTransportMeansList;
			AssertEquals("List for " + version + " with transport mode " + transport + " is correct", result, list.CodesAsString);
			AssertSame("Testing cache", lookups.BorderTransportMeansList, list);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
