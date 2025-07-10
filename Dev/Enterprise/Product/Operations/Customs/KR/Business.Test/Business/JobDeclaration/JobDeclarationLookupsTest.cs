using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(declaration.Lookups.Declaration, declaration);
		}

		public void TestCodeDescriptionPairList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "Bonded Area Code");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, "11111111", "경의선철도 입출경검사장(지상)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.IndustrialParkCode, "Industrial Park Code");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.IndustrialParkCode, "101", "한국수출", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.ExpressDeliveryServiceIDs, "Express Delivery Service IDs");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ExpressDeliveryServiceIDs, "AD0001", "유나이티드파슬서비스코리아(주)", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.TaxOffice, "Tax Office");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.TaxOffice, "100", "서울청", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.TaxOffice, "101", "종로", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.TaxOffice, "104", "남대문", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var lookups = new JobDeclarationLookups(declaration);

			AssertEquals(TransactionTypeCodeList.Codes._11, lookups.TransactionTypeCodeList[0].Code);
			AssertEquals(TransactionTypeCodeList.Descriptions._11, lookups.TransactionTypeCodeList[0].Description);

			Assert(lookups.TransactionTypeCodeList.ContainsCode("80"));
			Assert(!lookups.TransactionTypeCodeList.ContainsCode("102"));

			AssertEquals(ExportTypeCodeList.Codes.A, lookups.MessageSubTypeList[0].Code);
			AssertEquals(ExportTypeCodeList.Descriptions.A, lookups.MessageSubTypeList[0].Description);

			Assert(!lookups.DeclarationProcedureTypeList.ContainsCode("I"));
			Assert(!lookups.DeclarationProcedureTypeList.ContainsCode("K"));
			Assert(!lookups.DeclarationProcedureTypeList.ContainsCode("S"));

			AssertEquals(Core.Constants.ContainerModes.Containerised, lookups.CargoIdTypeList[0].Code);
			AssertEquals(Core.Constants.ContainerModeDescriptions.Containerised, lookups.CargoIdTypeList[0].Description);

			var customsOfficelist = lookups.CustomsOfficeList as BusinessObjectCollection;
			customsOfficelist.Load();
			AssertEquals(1, customsOfficelist.Count);
			Assert(customsOfficelist.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "010"));
			Assert(customsOfficelist.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "서울세관"));

			var bondedAreaCodeList = lookups.BondedAreaCodeList as BusinessObjectCollection;
			bondedAreaCodeList.Load();
			AssertEquals(1, bondedAreaCodeList.Count);
			Assert(bondedAreaCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "11111111"));
			Assert(bondedAreaCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "경의선철도 입출경검사장(지상)"));

			var industrialParkCodeList = lookups.IndustrialParkCodeList as BusinessObjectCollection;
			industrialParkCodeList.Load();
			AssertEquals(1, industrialParkCodeList.Count);
			Assert(industrialParkCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "101"));
			Assert(industrialParkCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "한국수출"));

			var expressDeliveryServiceIDsList = lookups.CourierCompanyList as BusinessObjectCollection;
			expressDeliveryServiceIDsList.Load();
			AssertEquals(1, expressDeliveryServiceIDsList.Count);
			Assert(expressDeliveryServiceIDsList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "AD0001"));
			Assert(expressDeliveryServiceIDsList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "유나이티드파슬서비스코리아(주)"));

			AssertEquals("20", lookups.CustomsDivisionList[0].Code);
			AssertEquals("내륙기지통관과", lookups.CustomsDivisionList[0].Description);

			AssertEquals(ExporterTypeCodeList.Codes.A, lookups.ExporterTypeList[0].Code);
			AssertEquals(ExporterTypeCodeList.Descriptions.A, lookups.ExporterTypeList[0].Description);

			AssertEquals(OutOfHoursDeclarationIndicatorCodeList.Codes.N, lookups.OutOfHoursDeclarationIndicatorList[0].Code);
			AssertEquals(OutOfHoursDeclarationIndicatorCodeList.Descriptions.N, lookups.OutOfHoursDeclarationIndicatorList[0].Description);

			AssertEquals(ReturnReasonCodeList.Codes._11, lookups.ReturnReasonList[0].Code);
			AssertEquals(ReturnReasonCodeList.Descriptions._11, lookups.ReturnReasonList[0].Description);

			AssertEquals(ReturnTypeCodeList.Codes.A, lookups.ReturnTypeList[0].Code);
			AssertEquals(ReturnTypeCodeList.Descriptions.A, lookups.ReturnTypeList[0].Description);

			AssertEquals(GoodsStatusCodeList.Codes.Mixed, lookups.GoodsStatusList[0].Code);
			AssertEquals(GoodsStatusCodeList.Descriptions.Mixed, lookups.GoodsStatusList[0].Description);

			AssertEquals(ApplicationForSimpleDrawbackCodeList.Codes.AD, lookups.ApplicationForSimpleDrawbackList[0].Code);
			AssertEquals(ApplicationForSimpleDrawbackCodeList.Descriptions.AD, lookups.ApplicationForSimpleDrawbackList[0].Description);

			AssertEquals(SouthNorthTradeYNCodeList.Codes.A, lookups.SouthNorthTradeYNList[0].Code);
			AssertEquals(SouthNorthTradeYNCodeList.Descriptions.A, lookups.SouthNorthTradeYNList[0].Description);

			var lateDecPenaltyDateCodeList = lookups.LateDecPenaltyDateCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(2, lateDecPenaltyDateCodeList.Count);
				AssertEquals("D, W", lateDecPenaltyDateCodeList.CodesAsString);
			});

			var taxOfficeList = lookups.TaxOfficeList as BusinessObjectCollection;
			taxOfficeList.Load();
			AssertEquals(3, taxOfficeList.Count);
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "100"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "서울청"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "101"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "종로"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "104"));
			Assert(taxOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "남대문"));
		}

		public void TestContainedItemOfTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var transportTypeList = declaration.Lookups.TransportTypeList;
			AssertEquals(3, transportTypeList.Count);
			AssertEquals("AIR, SEA, MAI", transportTypeList.CodesAsString);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			transportTypeList = declaration.Lookups.TransportTypeList;
			AssertEquals(2, transportTypeList.Count);
			AssertEquals("AIR, SEA", transportTypeList.CodesAsString);
		}

		public void TestContainedItemOfContainerPackCodeListInTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			CheckContainedItemOfContainerPackCodeList(declaration, Core.Constants.TransportModes.Air, 6, "BU, ETC, MPA, PA, RO, UL");
			CheckContainedItemOfContainerPackCodeList(declaration, Core.Constants.TransportModes.Sea, 7, "BU, ETC, FC, LC, MPA, PA, RO");
			CheckContainedItemOfContainerPackCodeList(declaration, Core.Constants.TransportModes.Mail, 8, "BU, ETC, FC, LC, MPA, PA, RO, UL");
		}

		void CheckContainedItemOfContainerPackCodeList(JobDeclaration declaration, string transportMode, int expectedCount, string expectedListItems)
		{
			declaration.JE_TransportMode = transportMode;
			var cargoIdTypeList = declaration.Lookups.ContainerPackKRList;
			AssertEquals(expectedCount, cargoIdTypeList.Count);
			AssertEquals(expectedListItems, cargoIdTypeList.CodesAsString);
		}

		public void TestContainedItemOfCarrierCodeCollection()
		{
			var carrier1 = Factory.New<ZZRefCarrierCombined>();
			carrier1.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.KoreaSouth;
			carrier1.ZZ4_Code = "ABC";
			carrier1.ZZ4_IsAir = true;
			var carrier2 = Factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.KoreaSouth;
			carrier2.ZZ4_Code = "DEF";
			carrier2.ZZ4_IsSea = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(1, declaration.Lookups.CarrierCodeCollection.Count);
			AssertEquals("ABC", ((ZZRefCarrierCombined)declaration.Lookups.CarrierCodeCollection[0]).ZZ4_Code);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(1, declaration.Lookups.CarrierCodeCollection.Count);
			AssertEquals("DEF", ((ZZRefCarrierCombined)declaration.Lookups.CarrierCodeCollection[0]).ZZ4_Code);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals(2, declaration.Lookups.CarrierCodeCollection.Count);
			AssertEquals("ABC", ((ZZRefCarrierCombined)declaration.Lookups.CarrierCodeCollection[0]).ZZ4_Code);
			AssertEquals("DEF", ((ZZRefCarrierCombined)declaration.Lookups.CarrierCodeCollection[1]).ZZ4_Code);
		}

		public void TestMessageSubTypeListByMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(declaration.JE_MessageType, "EXP");
			AssertEquals(ExportTypeCodeList.Codes.A, declaration.Lookups.MessageSubTypeList[0].Code);
			AssertEquals(ExportTypeCodeList.Descriptions.A, declaration.Lookups.MessageSubTypeList[0].Description);

			Assert(!declaration.Lookups.MessageSubTypeList.ContainsCode("C"));
			Assert(declaration.Lookups.MessageSubTypeList.ContainsCode("H"));

			declaration.JE_MessageType = "LEX";
			AssertEquals(LocalExportTransactionNatureCodeList.Codes._01, declaration.Lookups.MessageSubTypeList[0].Code);
			AssertEquals(LocalExportTransactionNatureCodeList.Descriptions._01, declaration.Lookups.MessageSubTypeList[0].Description);
		}

		public void TestTransactionTypeCodeListByMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(declaration.JE_MessageType, KRJobMessageTypeList.Codes.Export);
			AssertEquals(TransactionTypeCodeList.Codes._11, declaration.Lookups.TransactionTypeCodeList[0].Code);
			AssertEquals(TransactionTypeCodeList.Descriptions._11, declaration.Lookups.TransactionTypeCodeList[0].Description);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			AssertEquals(LocalExportGoodsTypeList.Codes.OriginalState, declaration.Lookups.TransactionTypeCodeList[0].Code);
			AssertEquals(LocalExportGoodsTypeList.Descriptions.OriginalState, declaration.Lookups.TransactionTypeCodeList[0].Description);

			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
			AssertEquals(CarnetCommodityUsageCodeList.Codes.A, declaration.Lookups.TransactionTypeCodeList[0].Code);
			AssertEquals(CarnetCommodityUsageCodeList.Descriptions.A, declaration.Lookups.TransactionTypeCodeList[0].Description);
		}

		public void TestIncoTermListByMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var incoTermsCodeList = declaration.Lookups.IncoTermList;
			CombineAssertions(() =>
			{
				AssertEquals(17, incoTermsCodeList.Count);
				AssertEquals("CFR, CIF, CIN, CIP, CPT, DAF, DAP, DAT, DDP, DDU, DEQ, DES, DPU, EXW, FAS, FCA, FOB", incoTermsCodeList.CodesAsString);
			});

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			incoTermsCodeList = declaration.Lookups.IncoTermList;
			CombineAssertions(() =>
			{
				AssertEquals(17, incoTermsCodeList.Count);
				AssertEquals("CFR, CIF, CIN, CIP, CPT, DAF, DAP, DAT, DDP, DDU, DEQ, DES, DPU, EXW, FAS, FCA, FOB", incoTermsCodeList.CodesAsString);
			});

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			incoTermsCodeList = declaration.Lookups.IncoTermList;
			CombineAssertions(() =>
			{
				AssertEquals(4, incoTermsCodeList.Count);
				AssertEquals("EXW, FAS, FCA, FOB", incoTermsCodeList.CodesAsString);
			});
		}

		public void TestMessageTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals(2, declaration.Lookups.MessageTypeList.Count);
			AssertEquals(KRJobMessageTypeList.Codes.Export, declaration.Lookups.MessageTypeList[0].Code);
			AssertEquals(KRJobMessageTypeList.Codes.LocalExport, declaration.Lookups.MessageTypeList[1].Code);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals(3, declaration.Lookups.MessageTypeList.Count);
			AssertEquals(KRJobMessageTypeList.Codes.Export, declaration.Lookups.MessageTypeList[0].Code);
			AssertEquals(KRJobMessageTypeList.Codes.Import, declaration.Lookups.MessageTypeList[1].Code);
			AssertEquals(KRJobMessageTypeList.Codes.LocalExport, declaration.Lookups.MessageTypeList[2].Code);
			KRCustomsRegistry.Instance.EnableToSaveImportDeclaration.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals(3, declaration.Lookups.MessageTypeList.Count);
			AssertEquals(KRJobMessageTypeList.Codes.Export, declaration.Lookups.MessageTypeList[0].Code);
			AssertEquals(KRJobMessageTypeList.Codes.Import, declaration.Lookups.MessageTypeList[1].Code);
			AssertEquals(KRJobMessageTypeList.Codes.LocalExport, declaration.Lookups.MessageTypeList[2].Code);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.PersonalItems;
			AssertMessageTypeIsMisc();

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.ValuationDeclaration;
			AssertMessageTypeIsMisc();

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Carnet;
			AssertMessageTypeIsMisc();

			void AssertMessageTypeIsMisc()
			{
				AssertEquals(6, declaration.Lookups.MessageTypeList.Count);
				Assert(declaration.Lookups.MessageTypeList.ContainsCode(KRJobMessageTypeList.Codes.Export));
				Assert(declaration.Lookups.MessageTypeList.ContainsCode(KRJobMessageTypeList.Codes.Import));
				Assert(declaration.Lookups.MessageTypeList.ContainsCode(KRJobMessageTypeList.Codes.LocalExport));
				Assert(declaration.Lookups.MessageTypeList.ContainsCode(KRJobMessageTypeList.Codes.PersonalItems));
				Assert(declaration.Lookups.MessageTypeList.ContainsCode(KRJobMessageTypeList.Codes.ValuationDeclaration));
				Assert(declaration.Lookups.MessageTypeList.ContainsCode(KRJobMessageTypeList.Codes.Carnet));
			}
		}

		public void TestMessageSubTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var messageSubTypeList = declaration.Lookups.MessageSubTypeList;

			AssertEquals(9, messageSubTypeList.Count);
			AssertEquals("A, B, D, E, F, G, H, L, P", messageSubTypeList.CodesAsString);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			messageSubTypeList = declaration.Lookups.MessageSubTypeList;

			AssertEquals(7, messageSubTypeList.Count);
			AssertEquals("A, B, C, D, E, F, G", messageSubTypeList.CodesAsString);

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			messageSubTypeList = declaration.Lookups.MessageSubTypeList;

			AssertEquals(10, messageSubTypeList.Count);
			AssertEquals("01, 02, 03, 04, 06, 07, 08, 09, 17, 18", messageSubTypeList.CodesAsString);
		}

		public void TestImportDeclarationPlanList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = new JobDeclarationLookups(declaration);
			AssertEquals(9, lookups.DeclarationPlanList.Count);
			AssertEquals("A, B, C, D, E, F, G, H, Z", lookups.DeclarationPlanList.CodesAsString);
		}

		public void TestImportTradeTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lookups = declaration.Lookups;
			AssertEquals(45, lookups.ImportDealingTypeCodeList.Count);
			AssertEquals("11, 12, 13, 14, 15, 21, 22, 29, 31, 39, 41, 49, 51, 52, 53, 54, 55, 59, 61, 69, 70, 71, 72, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101", lookups.ImportDealingTypeCodeList.CodesAsString);
		}

		public void TestImporterKRVList()
		{
			var importer1 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK1", "(주)레디코리아");
			var contact = TestOrgDataSetUpHelper.AddOrgContact(importer1, "name", false);
			contact.OC_Phone = "01012345678";
			contact.OC_JobCategory = "123-4567/123";
			contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.ValuationAuthorityForKRCustoms;

			var importer2 = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "RK2", "(주)레디코리아2");
			var contact2 = TestOrgDataSetUpHelper.AddOrgContact(importer2, "name2", false);
			contact2.OC_Phone = "01012345678";
			contact2.OC_JobCategory = "123-4567/123";
			contact2.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.ValuationAuthorityForKRCustoms;

			var contact3 = TestOrgDataSetUpHelper.AddOrgContact(importer2, "name3", false);
			contact3.OC_Phone = "01012345678";
			contact3.OC_JobCategory = "123-4567/123";
			contact3.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.ValuationAuthorityForKRCustoms;

			var contact4 = TestOrgDataSetUpHelper.AddOrgContact(importer2, "name4", false);
			contact4.OC_Phone = "01012345678";
			contact4.OC_JobCategory = "123-4567/123";
			contact4.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.HAZ;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer1.PK;
			AssertEquals(1, declaration.Lookups.OrgContactCollectionList.Count);

			declaration.JE_OH_Importer = importer2.PK;
			AssertEquals(2, declaration.Lookups.OrgContactCollectionList.Count);

			contact3.OC_IsActive = false;
			Factory.Save();
			AssertEquals(1, declaration.Lookups.OrgContactCollectionList.Count);

			var contact5 = TestOrgDataSetUpHelper.AddOrgContact(importer2, "name5", false);
			contact5.OC_Phone = "01012345678";
			contact5.OC_JobCategory = "123-4567/123";
			contact5.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.ValuationAuthorityForKRCustoms;

			contact3.OC_IsActive = true;
			Factory.Save();
			AssertEquals(3, declaration.Lookups.OrgContactCollectionList.Count);
		}
	}
}
