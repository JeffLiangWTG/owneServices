using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class AddInfoJobDeclarationLookupsTest : EU.Business.Declaration.Testing.AddInfoJobDeclarationLookupsTest
	{
		public override void TestSpecificCircumstanceIndicatorList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var lookups = Declaration.AddInfoLookups;
			var specificCircumstanceIndicatorType = Constants.RefCusCodeListTypes.SpecificCircumstanceIndicatorType;
			helper.CreateNewOrGetExistingCusCodeType(specificCircumstanceIndicatorType, "Ireland SpecificeCircumstanceIndicatorType");
			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var grouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
			var irelandCode = Core.Constants.CountryCodes.Ireland;
			var italyCode = Core.Constants.CountryCodes.Italy;
			helper.CreateNewOrGetExistingDataGrouping(irelandCode, parent: grouping);
			helper.CreateNewOrGetExistingDataGrouping(italyCode, parent: grouping);
			helper.CreateCusCodeList(irelandCode, specificCircumstanceIndicatorType, "IEA20", "Ireland SpecificeCircumstanceIndicatorType Type 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(irelandCode, specificCircumstanceIndicatorType, "IEA21", "Ireland SpecificeCircumstanceIndicatorType Type 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(italyCode, specificCircumstanceIndicatorType, "ITA20", "Italy SpecificeCircumstanceIndicatorType Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(eunCode, specificCircumstanceIndicatorType, "ENA20", "EUN SpecificeCircumstanceIndicatorType Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var specificeCircumstanceIndicatorList = lookups.SpecificCircumstanceIndicatorList;

			CombineAssertions(() =>
			{
				AssertEquals("List should have 2 items.", 2, specificeCircumstanceIndicatorList.Count);
				AssertEquals("List should have item IELOC1.", true, specificeCircumstanceIndicatorList.ContainsCode("IEA20"));
				AssertEquals("List should have item IELOC2.", true, specificeCircumstanceIndicatorList.ContainsCode("IEA21"));

				AssertEquals("List should NOT have IT item.", false, specificeCircumstanceIndicatorList.ContainsCode("ITA20"));
				AssertEquals("List should NOT have EU item.", false, specificeCircumstanceIndicatorList.ContainsCode("ENA20"));

				var newDeclaration = Factory.New<JobDeclaration>();
				AssertSame("List should have been cached.", specificeCircumstanceIndicatorList, newDeclaration.AddInfoLookups.SpecificCircumstanceIndicatorList);
			});
		}

		public void TestTransportMeansList_Import()
		{
			CombineAssertions(() =>
			{
				AssertMeansOfTransportListIsCorrect(EUJobMessageTypeList.Codes.Import, ZString.Empty, ZString.Empty, true);
				AssertMeansOfTransportListIsCorrect(EUJobMessageTypeList.Codes.Import, TransportTypeList.Codes.Sea, "10, 11", true);
				AssertMeansOfTransportListIsCorrect(EUJobMessageTypeList.Codes.Import, TransportTypeList.Codes.Rail, "20, 21", true);
				AssertMeansOfTransportListIsCorrect(EUJobMessageTypeList.Codes.Import, TransportTypeList.Codes.Road, "30", true);
				AssertMeansOfTransportListIsCorrect(EUJobMessageTypeList.Codes.Import, TransportTypeList.Codes.Air, "40, 41", true);
				AssertMeansOfTransportListIsCorrect(EUJobMessageTypeList.Codes.Import, TransportTypeList.Codes.Mail, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", true);
				AssertMeansOfTransportListIsCorrect(EUJobMessageTypeList.Codes.Import, TransportTypeList.Codes.FixedTransportInstallations, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", true);
				AssertMeansOfTransportListIsCorrect(EUJobMessageTypeList.Codes.Import, TransportTypeList.Codes.InlandWaterwayTransport, "80, 81", true);
				AssertMeansOfTransportListIsCorrect(EUJobMessageTypeList.Codes.Import, TransportTypeList.Codes.OwnPropulsion, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", true);
			});
		}

		public void TestTransportMeansList_Export()
		{
			CombineAssertions(() =>
			{
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.Export, ZString.Empty, ZString.Empty, true);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.Export, TransportTypeList.Codes.Sea, "10, 11", true);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.Export, TransportTypeList.Codes.Rail, "20, 21", true);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.Export, TransportTypeList.Codes.Road, "30", true);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.Export, TransportTypeList.Codes.Air, "40, 41", true);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.Export, TransportTypeList.Codes.Mail, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", true);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.Export, TransportTypeList.Codes.FixedTransportInstallations, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", true);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.Export, TransportTypeList.Codes.InlandWaterwayTransport, "80, 81", true);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.Export, TransportTypeList.Codes.OwnPropulsion, "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", true);
			});
		}

		public void TestTransportMeansList_ReExport()
		{
			CombineAssertions(() =>
			{
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ReExport, ZString.Empty, ZString.Empty, false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ReExport, TransportTypeList.Codes.Sea, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ReExport, TransportTypeList.Codes.Rail, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ReExport, TransportTypeList.Codes.Road, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ReExport, TransportTypeList.Codes.Air, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ReExport, TransportTypeList.Codes.Mail, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ReExport, TransportTypeList.Codes.FixedTransportInstallations, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ReExport, TransportTypeList.Codes.InlandWaterwayTransport, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ReExport, TransportTypeList.Codes.OwnPropulsion, "", false);
			});
		}

		public void TestTransportMeansList_ExitSummary()
		{
			CombineAssertions(() =>
			{
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ExitSummary, ZString.Empty, ZString.Empty, false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ExitSummary, TransportTypeList.Codes.Sea, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ExitSummary, TransportTypeList.Codes.Rail, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ExitSummary, TransportTypeList.Codes.Road, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ExitSummary, TransportTypeList.Codes.Air, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ExitSummary, TransportTypeList.Codes.Mail, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ExitSummary, TransportTypeList.Codes.FixedTransportInstallations, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ExitSummary, TransportTypeList.Codes.InlandWaterwayTransport, "", false);
				AssertMeansOfTransportListIsCorrect(IEJobMessageTypeList.Codes.ExitSummary, TransportTypeList.Codes.OwnPropulsion, "", false);
			});
		}

		void AssertMeansOfTransportListIsCorrect(ZString mssageType, ZString decTransport, ZString expectedMeansOfTransportCodes, bool testListIsCached)
		{
			Declaration.JE_MessageType = mssageType;
			Declaration.JE_TransportModeInland = decTransport;

			var newDeclaration = Factory.New<JobDeclaration>();
			newDeclaration.JE_MessageType = mssageType;
			newDeclaration.JE_TransportModeInland = decTransport;

			AssertEquals(decTransport, expectedMeansOfTransportCodes, Declaration.AddInfoLookups.TransportMeansList.CodesAsString);
			if (testListIsCached)
			{
				AssertSame($"TransportMeansList for {mssageType} - {decTransport}", Declaration.AddInfoLookups.TransportMeansList, newDeclaration.AddInfoLookups.TransportMeansList);
			}
		}

		public void TestTransportMeansListDefaults()
		{
			CombineAssertions(() =>
			{
				Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				AssertTransportMeansListDefaults();

				Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertTransportMeansListDefaults();
			});
		}

		void AssertTransportMeansListDefaults()
		{
			var lookups = Declaration.AddInfoLookups;
			Declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
			AssertEquals($"JE_MessageType = {Declaration.JE_MessageType}; JE_TransportModeInland = Sea", "10", lookups.TransportMeansList.DefaultCode);
			Declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
			AssertEquals($"JE_MessageType = {Declaration.JE_MessageType}; JE_TransportModeInland = Air", "40", lookups.TransportMeansList.DefaultCode);
			Declaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;
			AssertNull($"JE_MessageType = {Declaration.JE_MessageType}; JE_TransportModeInland = Rail", lookups.TransportMeansList.DefaultCode);
			Declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			AssertEquals($"JE_MessageType = {Declaration.JE_MessageType}; JE_TransportModeInland = Road", "30", lookups.TransportMeansList.DefaultCode);
			Declaration.JE_TransportModeInland = TransportTypeList.Codes.InlandWaterwayTransport;
			AssertEquals($"JE_MessageType = {Declaration.JE_MessageType}; JE_TransportModeInland = IWT", "80", lookups.TransportMeansList.DefaultCode);
			Declaration.JE_TransportModeInland = TransportTypeList.Codes.FixedTransportInstallations;
			AssertNull($"JE_MessageType = {Declaration.JE_MessageType}; JE_TransportModeInland = Fixed", lookups.TransportMeansList.DefaultCode);
			Declaration.JE_TransportModeInland = TransportTypeList.Codes.OwnPropulsion;
			AssertNull($"JE_MessageType = {Declaration.JE_MessageType}; JE_TransportModeInland = OWN", lookups.TransportMeansList.DefaultCode);
			Declaration.JE_TransportModeInland = TransportTypeList.Codes.Mail;
			AssertNull($"JE_MessageType = {Declaration.JE_MessageType}; JE_TransportModeInland = MAI", lookups.TransportMeansList.DefaultCode);
		}

		public void TestAgreedPlaceCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: euDataGrouping);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "IncoTerm Key");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "1", "111", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "2", "222", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var entryInstruction = Factory.New<CusEntryInstruction>();
			declaration.CustomsEntryInstructions.Add(entryInstruction);

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			CombineAssertions("AgreedPlaceCodeList for UCC5", () =>
			{
				string[] hCodes = { "H1", "H3", "H4", "H5" };

				declaration.JE_ShipmentIncoTermPlace = "";
				var list = declaration.AddInfoLookups.AgreedPlaceCodeList;
				foreach (var code in hCodes)
				{
					entryInstruction.CEI_Style = code;	
					AssertType<RefUNLOCOCollection>($"Should return UNLOCO list for {code} (UCC5, Empty IncoTerm)", list);
				}

				declaration.JE_ShipmentIncoTermPlace = "SomePlace";
				var list2 = declaration.AddInfoLookups.AgreedPlaceCodeList;
				foreach (var code in hCodes)
				{
					entryInstruction.CEI_Style = code;
					AssertType<RefCountryCollection>($"Should return Country list for {code} (UCC5, With IncoTerm)", list2);
				}
			});

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;

			CombineAssertions("AgreedPlaceCodeList for UCC6", () =>
			{
				string[] hCodes = { "H1", "H3", "H4", "H5" };

				declaration.JE_ShipmentIncoTermPlace = "";
				var list = declaration.AddInfoLookups.AgreedPlaceCodeList;
				foreach (var code in hCodes)
				{
					entryInstruction.CEI_Style = code;
					AssertType<RefUNLOCOCollection>($"Should return UNLOCO list for {code} (UCC6, Empty IncoTerm)", list);
				}

				declaration.JE_ShipmentIncoTermPlace = "SomePlace";
				var list2 = declaration.AddInfoLookups.AgreedPlaceCodeList;
				foreach (var code in hCodes)
				{
					entryInstruction.CEI_Style = code;
					AssertType<RefCountryCollection>($"Should return Country list for {code} (UCC6, With IncoTerm)", list2);
				}
			});
		}

		public override void TestRegionOfDestinationList()
		{
			var list = Declaration.AddInfoLookups.RegionOfDestinationList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes from list", new RegionOfDestinationList().CodesAsString, list.CodesAsString);
				AssertSame("Cached", list, Declaration.AddInfoLookups.RegionOfDestinationList);
			});
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;
	}
}
