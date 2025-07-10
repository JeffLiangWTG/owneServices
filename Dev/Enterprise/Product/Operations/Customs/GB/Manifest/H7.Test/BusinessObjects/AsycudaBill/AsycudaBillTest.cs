using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.GB.H7.Business.AsycudaBill;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		[TestDate(2024, 12, 13)]
		public void TestABL_UCRNumber()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			var bill1 = header1.Bills.AddNew();
			Assert(bill1.ABL_UCRNumberInfo.ReadOnly);

			header1.AMA_JobReference = "H7D001";
			header1.AMA_OA_Declarant = ZGuid.Empty;
			var consignee = Factory.NewWithValidTestData<OrgAddress>();
			var countryAU = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			consignee.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789ABC", countryAU);
			bill1.ABL_OA_Consignee = consignee.PK;
			Factory.Save();
			AssertEquals("Use Importer EORI when Declarant is not available", "4AU123456789ABC-H7D001", bill1.ABL_UCRNumber);

			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "H7D002";
			var bill2 = header2.Bills.AddNew();
			var countryGB = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");
			var declarant = Factory.NewWithValidTestData<OrgAddress>();
			declarant.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321ABC", countryGB);
			header2.AMA_OA_Declarant = declarant.PK;
			Factory.Save();
			AssertEquals("Use Declarant EORI", "4GB987654321ABC-H7D002", bill2.ABL_UCRNumber);
			var bill6 = header2.Bills.AddNew();
			Factory.Save();
			AssertEquals("Next UCR Number should be with /1", "4GB987654321ABC-H7D002/1", bill6.ABL_UCRNumber);

			var header3 = Factory.New<AsycudaManifestHeader>();
			header3.AMA_JobReference = "H7D003";
			var bill3 = header3.Bills.AddNew();
			var bill4 = header3.Bills.AddNew();
			var bill5 = header3.Bills.AddNew();
			header3.AMA_OA_Declarant = declarant.PK;
			Factory.Save();
			AssertEquals("4GB987654321ABC-H7D003", bill3.ABL_UCRNumber);
			AssertEquals("4GB987654321ABC-H7D003/1", bill4.ABL_UCRNumber);
			AssertEquals("4GB987654321ABC-H7D003/2", bill5.ABL_UCRNumber);
		}

		public void TestVATNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Assert("VATNumber text box is readonly", bill.VATNumberInfo.ReadOnly);

			var consignee = Factory.NewWithValidTestData<OrgAddress>();
			var countryAU = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var countryGB = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");
			consignee.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "999999", countryGB);
			consignee.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "888888", countryGB);
			consignee.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "777777", countryAU);
			consignee.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "666666", countryAU);
			bill.ABL_OA_Consignee = consignee.PK;
			Factory.Save();
			AssertEquals("VATNumber is obtained from Consignee's config where type is VAT and country is GB", "999999", bill.VATNumber);
		}

		public void TestPostponedVatAccountingCheck()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			Assert("PostponedVatAccounting checkbox is readonly if VATNumber is empty", bill.PostponedVatAccountingCheckInfo.ReadOnly);

			var consignee1 = Factory.NewWithValidTestData<OrgAddress>();
			var countryGB1 = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");
			consignee1.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "999999", countryGB1);
			bill.ABL_OA_Consignee = consignee1.PK;
			Factory.Save();
			CombineAssertions("PostponedVatAccountingCheck behavior after a valid VAT number entered", () =>
			{
				Assert("PostponedVatAccounting checkbox is editable if VATNumber is not empty", !bill.PostponedVatAccountingCheckInfo.ReadOnly);
				Assert("Will be false if PostponedVatAccounting checkbox gets unchecked", !bill.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.PostponedVatAccountingEntryColumnName));
			});

			bill.PostponedVatAccountingCheck = true;
			Assert("Will be true if PostponedVatAccounting checkbox gets checked", bill.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.PostponedVatAccountingEntryColumnName));

			bill.PostponedVatAccountingCheck = false;
			Assert("Will be false if PostponedVatAccounting checkbox gets unchecked", !bill.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.PostponedVatAccountingEntryColumnName));

			bill.PostponedVatAccountingCheck = true;
			var consignee2 = Factory.NewWithValidTestData<OrgAddress>();
			var countryGB2 = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");
			consignee2.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "888888", countryGB2);
			bill.ABL_OA_Consignee = consignee2.PK;
			CombineAssertions("PostponedVatAccountingCheck behavior after a valid VAT number replaced", () =>
			{
				AssertEquals("Will be populated before saving if Consignee gets changed", "888888", bill.VATNumber);
				Assert("Will be false if Consignee gets changed", !bill.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.PostponedVatAccountingEntryColumnName));
				Assert("PostponedVatAccounting checkbox is editable if VATNumber is not empty", !bill.PostponedVatAccountingCheckInfo.ReadOnly);
			});

			var consignee3 = Factory.NewWithValidTestData<OrgAddress>();
			var countryGB3 = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");
			consignee3.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "", countryGB3);
			bill.ABL_OA_Consignee = consignee3.PK;
			CombineAssertions("PostponedVatAccountingCheck behavior after an empty VAT number replaced", () =>
			{
				AssertEquals("Will be populated before saving if Consignee gets changed", ZString.Empty, bill.VATNumber);
				Assert("Will be false if Consignee gets changed", !bill.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.PostponedVatAccountingEntryColumnName));
				Assert("PostponedVatAccounting checkbox is readonly if VATNumber is empty", bill.PostponedVatAccountingCheckInfo.ReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetBillForTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetBillForTest(factory);

		public void TestGetCusSupportingInfoTypes()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			CombineAssertions(() =>
			{
				AssertEquals(typeof(PreviousDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.PreviousDocument]);
				AssertEquals(typeof(AdditionalDocument), ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.AdditionalDocument]);
				AssertEquals(typeof(EU.H7.Business.AdditionalInfo), ((Integration.Customs.ICusSupportingInfoTypeSupporter)bill).GetCusSupportingInfoTypes()[H7CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			});
		}

		public void TestPreviousDocuments()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<PreviousDocumentCollection<PreviousDocument>>(bill.PreviousDocuments);
		}

		AsycudaBill GetBillForTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		public void TestCusGoodsLocationType()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			var cusGoodsLocationProvider = bill as ICusGoodsLocationProvider;

			CombineAssertions("CusGoodsLocation Type", () =>
			{
				AssertType<CusGoodsLocation>("ICusGoodsLocationProvider.GoodsLocation", cusGoodsLocationProvider.GoodsLocation);
				AssertType<CusGoodsLocation>("CusGoodsLocation", bill.CusGoodsLocation);
			});
		}

		public void TestCusGoodsLocationProviderKey()
		{
			var cusGoodsLocationProvider = GetNewBusinessObject() as ICusGoodsLocationProvider;

			AssertEquals("CusGoodsLocationProvider key", "GBH7D", cusGoodsLocationProvider.ProviderKey);
		}

		public void TestAdditionalDocument()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<AdditionalDocumentCollection<AdditionalDocument>>(bill.AdditionalDocuments);
		}

		public void TestAutoGenerateLRN()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.OnSaving();
			AssertNotNull(bill.LocalReferenceNumber);
		}

		public void TestABL_ShipmentType_Caption()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var resourceStringDataAttribute = bill.ABL_ShipmentTypeInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Additional Declaration Type", resourceStringDataAttribute.Caption);
				AssertEquals("Add. Decl. Type", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Add. Decl. Type", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Code identifying both the type of declaration and whether or not the goods have arrived at the goods location.", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestABL_BillNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var resourceStringDataAttribute = bill.ABL_BillNumberInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Bill Number", resourceStringDataAttribute.Caption);
				AssertEquals("Bill No.", resourceStringDataAttribute.ShortCaption);
				AssertEquals("Bill No.", resourceStringDataAttribute.MediumCaption);
				AssertEquals("The Transport Document Number used to identify the relevant Consignment on the declaration.", resourceStringDataAttribute.FullDescription);
			});
		}

		public void TestIAdditionalProcedureParent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			IAdditionalProcedureParent cpcParent = bill;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CDS", "APC", "40", "00", "C07", ZString.Empty, ZString.Empty);
			helper.CreateRefCusProcedure("CDS", "APC", "40", "00", "C08", ZString.Empty, ZString.Empty);
			helper.CreateRefCusProcedure("CDS", "APC", "40", "00", "1RV", ZString.Empty, ZString.Empty);
			helper.CreateRefCusProcedure("CDS", "APC", "40", "00", "F48", ZString.Empty, ZString.Empty);
			helper.CreateRefCusProcedure("CDS", "APC", "40", "00", "F47", ZString.Empty, ZString.Empty);
			Factory.Save();

			var codes = cpcParent.AdditionalProcedureCodeList;

			AssertEquals("40001H7", cpcParent.MainProcedure);
			AssertEquals("4000", cpcParent.MainProcedurePrefix);
			AssertEquals(99, cpcParent.MaxNumberOfAdditionalProcedureCode);
			Assert(codes.ContainsCode("4000C07"));
			Assert(codes.ContainsCode("4000C08"));
			Assert(codes.ContainsCode("40001RV"));
			Assert(codes.ContainsCode("4000F48"));
			Assert(codes.ContainsCode("4000F47"));
			AssertEquals(bill.Lookups.AdditionalProcedureList, cpcParent.AdditionalProcedureCodeList);
		}

		public void TestSetDefaultValue()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("Seller Reg No Type default value", OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration, bill.ABL_SellerRegNoType);
		}

		public void TestSupportingDocuments()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<SupportingDocumentCollection<SupportingDocument>>(bill.SupportingDocuments);
		}

		public void TestImportExport()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = "IMP";

			CombineAssertions("When it is import ", () =>
			{
				Assert("this bill should be import", bill.IsImport);
				Assert("this bill should not be export", !bill.IsExport);
			});

			bill.ABL_ShipmentType = "EXP";

			CombineAssertions("When it is export ", () =>
			{
				Assert("this bill should not be import", !bill.IsImport);
				Assert("this bill should be export", bill.IsExport);
			});
		}

		public void TestABL_ConsigneeRegNoWhenImporterIsOrgAppendsSingleCountryCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			var orgCusCode = orgAddress.CustomsCodes.AddNew();
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			orgHeader.OH_Code = "TestOrg";
			orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			orgCusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedKingdom;
			bill.ABL_OA_Consignee = orgAddress.PK;

			orgCusCode.OK_CustomsRegNo = "123456";
			AssertEquals("Should append GB", "GB123456", bill.ABL_ConsigneeRegNo);

			orgCusCode.OK_CustomsRegNo = orgCusCode.OK_RN_NKCodeCountry + "123456";
			AssertEquals("Should not append if already has country code", "GB123456", bill.ABL_ConsigneeRegNo);

			orgCusCode.OK_CustomsRegNo = "XI123456";
			AssertEquals("Should not append if already has XI", "XI123456", bill.ABL_ConsigneeRegNo);
		}

		public void TestGetReleasedStatus()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals(Common.EU.EntryStatusList.Codes.Clear, bill.GetReleasedStatus());
		}

		public void TestGetReleasedStatusDescription()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals("Cleared", bill.GetReleasedStatusDescription());
		}

		public void TestRequiredCurrencyCode()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertEquals(CurrencyCodes.UnitedKingdom, bill.RequiredCurrencyCode);
		}

		public void TestAdditionalInfoCollection()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			AssertType<AdditionalInfoCollection<AdditionalInfo>>(bill.AdditionalInfos);
		}

		public void TestValidationForMasterChild()
		{
			var bill = GetNewBusinessObject() as AsycudaBill;
			bill.ABL_BolType = "BOL";

			AssertType<AsycudaBillValidationForMasterChild>(bill.Validation);
		}

		public void TestTryConvert()
		{
			var bill = BuildBill();
			var converter = new StandAloneDeclarationConverter();
			converter.TryConvert(new List<AsycudaBill> { bill, BuildBill() });

			var newFactory = new BusinessObjectFactory();
			var reloadedBill = newFactory.Load<AsycudaBill>(bill.PK);
			var declaration = newFactory.LoadTop1<GB.Business.Declaration.JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, reloadedBill.EntrySummaryReferenceNumber));

			foreach (var instruction in declaration.CustomsEntryInstructions)
			{
				AssertEquals("H1", instruction.CEI_Style);
			}
		}

		AsycudaBill BuildBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			var item1 = bill.PackedItems.AddNew();
			var item2 = bill.PackedItems.AddNew();

			bill.ABL_BillNumber = "BN123";
			bill.ABL_GoodsDescription = "goods description";
			bill.ABL_GrossWeight = 0.9;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_Volume = 1.1;
			bill.ABL_VolumeUQ = "L";
			bill.ABL_ManifestQty = 13;
			bill.ABL_GoodsValue = 110;
			bill.ABL_RX_NKGoodsValueCurrency = "AUD";
			bill.ABL_PrepaidCollect = "PPD";
			bill.ABL_SellerRegNo = "1234";

			pack1.APA_PackUQ = "BX";
			pack2.APA_PackUQ = "FR";
			item1.API_FormattedTariff = "11081990009";
			item1.API_GoodsDescription = "item1 goods description";
			item1.API_RN_NKGoodsOrigin = "AU";
			item1.API_RX_NKGoodsValueCurrency = "AUD";
			item2.API_FormattedTariff = "11081990010";
			item2.API_GoodsDescription = "item2 goods description";
			item2.API_RN_NKGoodsOrigin = "US";
			item2.API_RX_NKGoodsValueCurrency = "USD";
			Factory.Save();
			return bill;
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { AsycudaBill.Schema.ABL_SellerRegNoType };
		}
	}
}
