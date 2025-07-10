using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using EURefCusCodeListTypes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class ImportJobDeclarationLookupsTest : EU.Business.Declaration.Testing.JobDeclarationLookupsTest<ImportJobDeclarationLookups, JobDeclaration>
	{
		public void TestDeclarantTypeList()
		{
			CombineAssertions(() =>
			{
				AssertDeclarantTypeList("No Instruction", "SEL, DIR, IND");

				var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
				AssertDeclarantTypeList(ImportDeclarationTypeList.Codes.EAV, "SEL, DIR");

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				AssertDeclarantTypeList(ImportDeclarationTypeList.Codes.AAV, "SEL, DIR, IND");

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
				AssertDeclarantTypeList(ImportDeclarationTypeList.Codes.VAV, "SEL, DIR, IND");

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
				AssertDeclarantTypeList("Others", "SEL, DIR, IND");
			});
		}

		public void TestDeclarantTypeList_CWPAuthorizationType_IntoWarehouseCPC()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Germany, "", "71", "78", "", "Desc", "IMP", intoWarehouse: true, group: "EZL,VZL,AZL");
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = org.Addresses.AddNew();
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			authorizationHeader.CPH_OH_PermitHolder = org.PK;

			CombineAssertions(() =>
			{
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				AssertDeclarantTypeList("No Declarant", "SEL, DIR, IND");

				jobDeclaration.JE_OA_DeclarantAddress = orgAddress.PK;
				AssertDeclarantTypeList("Has Declarant with CWP Type Authorization, no line with IntoWarehouseCPC", "SEL, DIR, IND");

				var line = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
				var instruction = jobDeclaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
				line.JI_Procedure = "7178";
				AssertEquals("Precondition: CPC is into warehouse", true, line.CusProcedure.IsIntoWarehouse());
				AssertDeclarantTypeList("Has Declarant with CWP Type Authorization and has line with IntoWarehouseCPC, line doesn't link to instruction", "SEL, DIR, IND");

				line.JI_CEI = instruction.PK;
				AssertDeclarantTypeList("Has Declarant with CWP Type Authorization and has line with IntoWarehouseCPC and line links to instruction", "SEL, DIR");
			});
		}

		public void TestGoodsOrigin()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_IM15, "Origin country/territory for entry style IM");
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_CO15, "Origin country/territory for entry style CO");
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EU15, "Origin country/territory for entry style EU");

			helper.CreateCusCodeList("DE", EURefCusCodeListTypes.Code_IM15, "AA", "Test AA", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_CO15, "BB", "Test BB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_CO15, "CC", "Test CC", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_EU15, "DD", "Test DD", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_EU15, "EE", "Test EE", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

			Factory.Save();

			CombineAssertions(() =>
			{
				jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
				AssertEquals("IM Entry Style", "AA", ((CodeDescriptionPairList)lookups.GoodsOrigin).CodesAsString);

				jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
				AssertEquals("CO Entry Style", "CC", ((CodeDescriptionPairList)lookups.GoodsOrigin).CodesAsString);

				jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromEFTAMember;
				AssertEquals("EU Entry Style", "EE", ((CodeDescriptionPairList)lookups.GoodsOrigin).CodesAsString);
			});
		}

		public void TestGoodsDestination()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_IM17, "Destination country/territory for entry style IM");
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_CO17, "Destination country/territory for entry style CO");
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EU17, "Destination country/territory for entry style EU");

			helper.CreateCusCodeList("DE", EURefCusCodeListTypes.Code_IM17, "AA", "Test AA", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));

			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_CO17, "BB", "Test BB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_CO17, "CC", "Test CC", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_EU17, "DD", "Test DD", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Export);
			helper.CreateCusCodeListWithAttribute("DE", EURefCusCodeListTypes.Code_EU17, "EE", "Test EE", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);

			Factory.Save();

			CombineAssertions(() =>
			{
				//GoodsDestination list all comes from CO17 import.
				jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
				AssertEquals("IM Entry Style", "CC", ((CodeDescriptionPairList)lookups.GoodsDestination).CodesAsString);

				jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromSpecialTerritory;
				AssertEquals("CO Entry Style", "CC", ((CodeDescriptionPairList)lookups.GoodsDestination).CodesAsString);

				jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromEFTAMember;
				AssertEquals("EU Entry Style", "CC", ((CodeDescriptionPairList)lookups.GoodsDestination).CodesAsString);
			});
		}

		public void TestTransportCountryList()
		{
			var transportCountryList = lookups.TransportCountryList;
			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>(lookups.TransportCountryList);
				AssertSame("Cached", transportCountryList, lookups.TransportCountryList);
			});
		}

		public void TestTransportCountryList_Content()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_IM15, "IM15");
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EU15, "EU15");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, EURefCusCodeListTypes.Code_IM15, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, EURefCusCodeListTypes.Code_EU15, "AE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, EURefCusCodeListTypes.Code_IM15, "AF", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, EURefCusCodeListTypes.Code_EU15, "AG", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EURefCusCodeListTypes.Code_EU15, "AI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				var transportCountryList = lookups.TransportCountryList as ZZRefCusCodeListCombinedCollection;
				transportCountryList.Load();

				var countries = transportCountryList.Select(x => x.ZZD_Code);
				AssertEquals("Existing code, IM15", true, countries.Contains("AD"));
				AssertEquals("Existing code, EU15", true, countries.Contains("AE"));
				AssertEquals("Invalid startDate", false, countries.Contains("AF"));
				AssertEquals("Invalid endDate", false, countries.Contains("AG"));
				AssertEquals("EUN code", false, countries.Contains("AI"));
			});
		}

		public void TestOrigins_EntryStyle_EU()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(EURefCusCodeListTypes.Code_EU15, "Origin country/territory for entry style EU");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, EURefCusCodeListTypes.Code_EU15, "GB", "Test GB", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, EURefCusCodeListTypes.Code_EU15, "XS", "Test Serbia", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), RefCusCodeListAttributeTypes.Codes.Direction, EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Import);
			Factory.Save();

			var loader = new RefUNLOCO.Loader(Factory);
			var london = loader.Load("GBLON");
			var berlin = loader.Load("DEBER");
			var belgrad = loader.Load("RSBEG");
			jobDeclaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportFromEFTAMember;
			CombineAssertions(() =>
			{
				var filter = jobDeclaration.Lookups.Origins;
				AssertEquals("GoodsOrigin codes", "GB, XS", ((CodeDescriptionPairList)jobDeclaration.Lookups.GoodsOrigin).CodesAsString);
				AssertEquals("Contains EU15 code", true, filter.Contains(london));
				AssertEquals("Contains Serbia", true, filter.Contains(belgrad));
				AssertEquals("Contains Non-EU15 code: we don't want to filter by country because it's confusing for users, and there's validation on the country field anyway.", true, filter.Contains(berlin));
			});
		}

		public void TestFinalDestinations_NoFilters()
		{
			jobDeclaration.JE_TransportMode = "AIR";
			var result = lookups.FinalDestinations;
			Assert(result.Relationship.RelationshipFilter.IsEmpty);
		}

		public void TestPortOfArrivals_NoFilters()
		{
			jobDeclaration.JE_TransportMode = "AIR";
			var result = lookups.PortOfArrivals;
			Assert(result.Relationship.RelationshipFilter.IsEmpty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
		}

		void AssertDeclarantTypeList(string testCase, ZString expectedCodesAsString)
		{
			var list = (CodeDescriptionPairList)lookups.DeclarantTypeList;
			AssertEquals(testCase + "->CodesAsString", expectedCodesAsString, list.CodesAsString);
			AssertSame(testCase + "->Cached", list, lookups.DeclarantTypeList);
		}
	}
}
