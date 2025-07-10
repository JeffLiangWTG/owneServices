using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNJobDocAddress))]
	class CNJobDocAddressTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOverSeasPartyFallbackSequence()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, "AEO Mutual Recognition Country List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, Core.Constants.CountryCodes.Switzerland, "Switzerland", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org1, "CN", "MMR", "MMR001");
			AddRegNo(org1, "CN", "SMR", "SMR001");
			AddRegNo(org1, "CH", "AEO", "AEO001");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = org1.PK;
			var supplier = declaration.SupplierDocumentaryAddress;
			AssertEquals("MMR001", supplier.OverseasPartyCode);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org2, "CN", "SMR", "SMR002");
			AddRegNo(org2, "CH", "AEO", "AEO002");
			declaration.JE_OH_Supplier = org2.PK;
			AssertEquals("SMR002", supplier.OverseasPartyCode);

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org3, "CH", "AEO", "AEO003");
			declaration.JE_OH_Supplier = org3.PK;
			AssertEquals("CHAEO003", supplier.OverseasPartyCode);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org4, "CN", "MMR", "MMR004");
			AddRegNo(org4, "CN", "SMR", "SMR004");
			AddRegNo(org4, "CH", "AEO", "AEO004");
			declaration.JE_OH_Importer = org4.PK;
			var importer = declaration.ImporterDocumentaryAddress;
			AssertEquals("CHAEO004", importer.OverseasPartyCode);

			var org5 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org5, "CN", "MMR", "MMR005");
			AddRegNo(org5, "CN", "SMR", "SMR005");
			declaration.JE_OH_Importer = org5.PK;
			AssertEquals("MMR005", importer.OverseasPartyCode);

			var org6 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org6, "CN", "SMR", "SMR006");
			declaration.JE_OH_Importer = org6.PK;
			AssertEquals("SMR006", importer.OverseasPartyCode);
		}

		public void TestDefaultOverSeasPartyTypeAndCodeWhenOverrideChanged()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, "AEO Mutual Recognition Country List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, Core.Constants.CountryCodes.Switzerland, "Switzerland", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org1, "CN", "MMR", "MMR001");
			AddRegNo(org1, "CN", "SMR", "SMR001");
			AddRegNo(org1, "CH", "AEO", "AEO001");
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org2, "CN", "MMR", "MMR002");
			AddRegNo(org2, "CN", "SMR", "SMR002");
			AddRegNo(org2, "CH", "AEO", "AEO002");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_OH_Importer = org2.PK;

			var supplier = declaration.SupplierDocumentaryAddress;
			supplier.E2_AddressOverride = true;
			AssertEquals("MMR001", supplier.OverseasPartyCode);

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var importer = declaration.ImporterDocumentaryAddress;
			importer.E2_AddressOverride = true;
			AssertEquals("CHAEO002", importer.OverseasPartyCode);
		}

		public void TestDefaultOverSeasPartyCodeWhenTypeChangedIfIsOverride()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, "AEO Mutual Recognition Country List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, Core.Constants.CountryCodes.Switzerland, "Switzerland", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org1, "CN", "MMR", "MMR001");
			AddRegNo(org1, "CN", "SMR", "SMR001");
			AddRegNo(org1, "CH", "AEO", "AEO001");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = org1.PK;
			var supplier = declaration.SupplierDocumentaryAddress;

			supplier.E2_AddressOverride = true;
			AssertEquals("MMR001", supplier.OverseasPartyCode);

			supplier.OverseasPartyCodeType = "SMR";
			AssertEquals("SMR001", supplier.OverseasPartyCode);

			supplier.OverseasPartyCodeType = "AEO";
			AssertEquals("CHAEO001", supplier.OverseasPartyCode);
		}

		public void TestDeleteGenAddOnColumnsWhenDeleteJobDocAdress()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var docAddress = declaration.ImporterDocumentaryAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.CustomsCode = "CCD001";
			docAddress.CIQCode = "CIQ001";
			docAddress.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			docAddress.OverseasPartyCode = "GBX001";

			var docAddNumCollection = docAddress.DocAddressNumbers;
			var ccdNum = docAddNumCollection.FindFirstByNumberType(OrgCusCode.CodeTypes.CustomsClientCode);
			var ciqNum = docAddNumCollection.FindFirstByNumberType(OrgCusCode.ChinaCodeTypes.CIQ);
			var aeoNum = docAddNumCollection.FindFirstByNumberType(OrgCusCode.ChinaCodeTypes.AEO);

			declaration.Delete();
			AssertEquals("DocAddres should been deleted", true, docAddress.IsDeleted);
			AssertEquals("CCD DocAddressNumber should been deleted", true, ccdNum.IsDeleted);
			AssertEquals("CIQ DocAddressNumber should been deleted", true, ciqNum.IsDeleted);
			AssertEquals("AEO DocAddressNumber should been deleted", true, aeoNum.IsDeleted);
		}

		public void TestSettingAeoNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_MessageSubType = "CUS";
			var docAddress = declaration.ImporterDocumentaryAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.CustomsCode = "CCD001";
			docAddress.CIQCode = "CIQ001";

			docAddress.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			docAddress.OverseasPartyCode = "USX001";
			Factory.Save();
			docAddress.OverseasPartyCode = "GBX001";
			Factory.Save();

			var aeoQuery = new ZQuery(JobDocAddressNumberSchema.E2N_NumberType, OrgCusCode.ChinaCodeTypes.AEO);
			aeoQuery.AddToFilter(JobDocAddressNumberSchema.E2N_E2, docAddress.PK);
			var anotherFactory = new BusinessObjectFactory();

			var aeoNums = anotherFactory.Load<JobDocAddressNumber>(aeoQuery);
			AssertEquals("Should not create multiple AEO numbers after country part changes.", 1, aeoNums.Length);
			var aeoNum = aeoNums[0];
			AssertEquals("Should have correct E2N_Number", "X001", aeoNum.E2N_Number);
			AssertEquals("Should have correct E2N_RN_NKCountryCode", Core.Constants.CountryCodes.UnitedKingdom, aeoNum.E2N_RN_NKCountryCode);

			docAddress.OverseasPartyCode = "XXX001";
			Factory.Save();
			aeoNum = anotherFactory.LoadTop1<JobDocAddressNumber>(aeoQuery);
			AssertEquals("AEO country with invalid country part, should be as user input.", "X001", aeoNum.E2N_Number);
			AssertEquals("AEO country with invalid country part, should be as user input.", "XX", aeoNum.E2N_RN_NKCountryCode);
		}

		public void TestOverseasOrgCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, "AEO Mutual Recognition Country List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, Core.Constants.CountryCodes.Switzerland, "瑞士", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("SMR", "SMR001", Core.Constants.CountryCodes.China);
			orgHeader.CustomsCodes.AddNew("AEO", "AEO001", Core.Constants.CountryCodes.Switzerland);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = orgHeader.PK;

			var cnAddress = declaration.ImporterDocumentaryAddress;
			cnAddress.E2_AddressOverride = false;
			AssertEquals("OverseasPartyCodeType should be readonly for E2_AddressOverride false.", true, cnAddress.OverseasPartyCodeTypeInfo.ReadOnly);
			AssertEquals("OverseasPartyCode should be readonly for E2_AddressOverride false.", true, cnAddress.OverseasPartyCodeInfo.ReadOnly);
			AssertEquals("Should populate OverseasPartyCodeType from dbo.OrgHeader when E2_AddressOverride false.", "SMR", cnAddress.OverseasPartyCodeType);
			AssertEquals("Should populate OverseasPartyCode from dbo.OrgHeader when E2_AddressOverride false.", "SMR001", cnAddress.OverseasPartyCode);

			var anotherOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			anotherOrgHeader.CustomsCodes.AddNew("MMR", "MMR002", Core.Constants.CountryCodes.China);
			cnAddress.OrganisationPK = anotherOrgHeader.PK;
			AssertEquals("OverseasPartyCodeType should be refreshed when Organization changed.", "MMR", cnAddress.OverseasPartyCodeType);
			AssertEquals("OverseasPartyCodeType should be refreshed when Organization changed.", "MMR002", cnAddress.OverseasPartyCode);

			cnAddress.E2_AddressOverride = true;
			AssertEquals("OverseasPartyCodeType should be writable for E2_AddressOverride true.", false, cnAddress.OverseasPartyCodeTypeInfo.ReadOnly);
			AssertEquals("OverseasPartyCode should be writable for E2_AddressOverride true.", false, cnAddress.OverseasPartyCodeInfo.ReadOnly);

			cnAddress.OverseasPartyCodeType = "MMR";
			AssertNotNull("Should create a DocAddressNumber when setting OverseasPartyCodeType with E2_AddressOverride true.", cnAddress.OverseasPartyNumber);
			var singleDocAddressNumber = cnAddress.OverseasPartyNumber;
			cnAddress.OverseasPartyCode = "MMR003";
			AssertEquals("Values of DocAddressNumber should be set correctly: E2N_NumberType", "MMR", singleDocAddressNumber.E2N_NumberType);
			AssertEquals("Values of DocAddressNumber should be set correctly: E2N_Number", "MMR003", singleDocAddressNumber.E2N_Number);

			cnAddress.OverseasPartyCodeType = "SMR";
			var overSeasQuery = new ZQuery();
			overSeasQuery.AddToFilter(JobDocAddressNumberSchema.E2N_E2, cnAddress.PK);
			overSeasQuery.AddToFilter(JobDocAddressNumberSchema.E2N_NumberType, cnAddress.Lookups.OverseasPartyCodes.GetAllCodes());
			AssertSame("Should reuse(not create) DocAddressNumber when OverseasPartyCodeType changed.", singleDocAddressNumber, Factory.Load<JobDocAddressNumber>(overSeasQuery).Single());

			cnAddress.OverseasPartyCode = "MMR004";
			AssertEquals("E2N_Number should be set following OverseasPartyCode.", "MMR004", singleDocAddressNumber.E2N_Number);
		}

		public void TestDeleteDocAddressNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, "AEO Mutual Recognition Country List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, Core.Constants.CountryCodes.Switzerland, "瑞士", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew("SMR", "SMR001", Core.Constants.CountryCodes.China);
			orgHeader.CustomsCodes.AddNew("AEO", "AEO001", Core.Constants.CountryCodes.Switzerland);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_OH_Importer = orgHeader.PK;

			var cnAddress = declaration.ImporterDocumentaryAddress;
			cnAddress.E2_AddressOverride = true;
			cnAddress.OverseasPartyCodeType = "SMR";
			cnAddress.OverseasPartyCode = "MMR004";
			var singleDocAddressNumber = cnAddress.OverseasPartyNumber;
			Factory.Save();
			AssertEquals("DocAddressNumber should not be deletd when RequiresOverseasOrg and has values.", false, singleDocAddressNumber.IsDeleted);

			cnAddress.OverseasPartyCode = ZString.Empty;
			Factory.Save();
			AssertEquals("DocAddressNumber should be deleted when OverseasPartyCode empty.", true, singleDocAddressNumber.IsDeleted);

			cnAddress.OverseasPartyCodeType = "SMR";
			cnAddress.OverseasPartyCode = "MMR004";
			singleDocAddressNumber = cnAddress.OverseasPartyNumber;
			Factory.Save();
			AssertEquals("DocAddressNumber should not be deletd when RequiresOverseasOrg and has values.", false, singleDocAddressNumber.IsDeleted);
			cnAddress.OverseasPartyCodeType = ZString.Empty;
			Factory.Save();
			AssertEquals("DocAddressNumber hould be deleted when OverseasPartyCodeType empty.", true, singleDocAddressNumber.IsDeleted);

			cnAddress.OverseasPartyCodeType = "SMR";
			cnAddress.OverseasPartyCode = "MMR004";
			declaration.JE_MessageType = "IMP";
			Factory.Save();
			AssertEquals("DocAddressNumber hould be deleted when RequiresOverseasOrg false.", true, singleDocAddressNumber.IsDeleted);
		}

		public void TestDeleteGenAddOnColumnsWhenAddressOverrideDisabled()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "CUS";
			var importerDocAddress = declaration.ImporterDocumentaryAddress;
			importerDocAddress.E2_AddressOverride = true;
			importerDocAddress.CustomsCode = "CCD001";
			importerDocAddress.CIQCode = "CIQ001";
			Factory.Save();

			var docAddNumCollection = importerDocAddress.DocAddressNumbers;
			var ccdNum = docAddNumCollection.FindFirstByNumberType(OrgCusCode.CodeTypes.CustomsClientCode);
			var ciqNum = docAddNumCollection.FindFirstByNumberType(OrgCusCode.ChinaCodeTypes.CIQ);

			importerDocAddress.E2_AddressOverride = false;
			Factory.Save();
			AssertEquals("CCD DocAddressNumber should been deleted while E2_AddressOverride set to false.", true, ccdNum.IsDeleted);
			AssertEquals("CIQ DocAddressNumber should been deleted while E2_AddressOverride set to false.", true, ciqNum.IsDeleted);

			var supplierDocAddress = declaration.SupplierDocumentaryAddress;
			supplierDocAddress.E2_AddressOverride = true;
			supplierDocAddress.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			supplierDocAddress.OverseasPartyCode = "USAEO001";
			Factory.Save();
			var aeoNum = supplierDocAddress.DocAddressNumbers.FindFirstByNumberType(OrgCusCode.ChinaCodeTypes.AEO);
			supplierDocAddress.E2_AddressOverride = false;
			Factory.Save();
			AssertEquals("AEO DocAddressNumber should been deleted while E2_AddressOverride set to false.", true, aeoNum.IsDeleted);
		}

		public void TestDeleteEmptyNumbers()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var docAddress = declaration.ImporterDocumentaryAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.CustomsCode = "CCD001";
			docAddress.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			docAddress.OverseasPartyCode = "GBX001";

			var docAddNumCollection = docAddress.DocAddressNumbers;
			var ccdNum = docAddNumCollection.FindFirstByNumberType(OrgCusCode.CodeTypes.CustomsClientCode);
			var aeoNum = docAddNumCollection.FindFirstByNumberType(OrgCusCode.ChinaCodeTypes.AEO);

			Factory.Save();
			docAddress.CustomsCode = "";
			docAddress.OverseasPartyCode = "US";
			AssertEquals("Empty CCD number should be deleted when saving.", true, ccdNum.IsDeleted);
			AssertEquals("AEO number still have CountryCode, should NOT be deleted when saving.", false, aeoNum.IsDeleted);

			docAddress.OverseasPartyCode = "";
			Factory.Save();
			AssertEquals("AEO number CountryCode cleared, should be deleted when saving.", true, aeoNum.IsDeleted);
		}

		public void TestChineseCompanyName()
		{
			AssertNullOrEmpty(testWrapper.ChineseCompanyName);
			testOrgHeader.OH_FullName = "Test Company Full Name";
			AssertEquals("Fall back to OH_FullName", "Test Company Full Name", testWrapper.ChineseCompanyName);
			var mainAddress = testOrgHeader.MainAddress;
			mainAddress.CompanyName = "Test Company Name";
			AssertEquals("Fall back to OA_CompanyName", "Test Company Name", testWrapper.ChineseCompanyName);
			var translatedAddress = mainAddress.TranslatedAddresses.AddNew();
			translatedAddress.Language = Core.SharedConstants.Languages.ChineseSimplified;
			translatedAddress.OTA_CompanyName = "测试公司名称0001";
			AssertEquals("Use Translated Company Name", "测试公司名称0001", testWrapper.ChineseCompanyName);
			translatedAddress.OTA_CompanyName = @"测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001";
			AssertEquals("Should be left 80 chars", "测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001测试公司名称0001", testWrapper.ChineseCompanyName);
		}

		public void TestEnglishCompanyName()
		{
			testOrgHeader.OH_FullName = "test company full name";
			AssertEquals("test company full name", testWrapper.EnglishCompanyName);
			var mainAddress = testOrgHeader.MainAddress;
			var translatedAddress = mainAddress.TranslatedAddresses.AddNew();
			translatedAddress.Language = Core.SharedConstants.Languages.English;
			translatedAddress.OTA_CompanyName = "WiseTech Global";
			AssertEquals("WiseTech Global", testWrapper.EnglishCompanyName);
		}

		public void TestGovRegNumbers()
		{
			var ccdNumber = testOrgHeader.CustomsCodes.AddNew();
			ccdNumber.OK_CodeType = "CCD";
			ccdNumber.OK_RN_NKCodeCountry = "CN";
			ccdNumber.OK_CustomsRegNo = "CCD000001234567890";
			var cacNumber = testOrgHeader.CustomsCodes.AddNew();
			cacNumber.OK_CodeType = "USC";
			cacNumber.OK_RN_NKCodeCountry = "CN";
			cacNumber.OK_CustomsRegNo = "USC0000001234567890";
			var ciqNumber = testOrgHeader.CustomsCodes.AddNew();
			ciqNumber.OK_CodeType = "CIQ";
			ciqNumber.OK_RN_NKCodeCountry = "CN";
			ciqNumber.OK_CustomsRegNo = "CIQ0000001234567890";
			var aeoNumber = testOrgHeader.CustomsCodes.AddNew();
			aeoNumber.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Austria;
			aeoNumber.OK_CodeType = "EOR";
			aeoNumber.OK_CustomsRegNo = "222222123456789012345678901234567890";
			testDeclaration.JE_MessageType = "IMP";

			testJobDocAddress.E2_AddressOverride = false;
			AssertEquals("E2_AddressOverride = false, should get CCD Number from OrgHeader.", "CCD000001234567890", testWrapper.CustomsCode);
			AssertEquals("E2_AddressOverride = false, should get USC Number from OrgHeader.", "USC0000001234567890", testWrapper.SocialCreditCode);
			AssertEquals("E2_AddressOverride = false, should get CIQ Number from OrgHeader.", "CIQ0000001234567890", testWrapper.CIQCode);
			testJobDocAddress.E2_AddressOverride = true;
			AssertEquals("E2_AddressOverride = true, should get CCD Number from OrgHeader.", "CCD0000012", testWrapper.CustomsCode);
			AssertEquals("E2_AddressOverride = true, should get USC Number from OrgHeader.", "USC000000123456789", testWrapper.SocialCreditCode);
			AssertEquals("E2_AddressOverride = true, should get CIQ Number from OrgHeader.", "CIQ0000001", testWrapper.CIQCode);

			testWrapper.CustomsCode = "CCD000002";
			AssertEquals("CCD000002", testWrapper.DocAddressNumbers.FindFirstByNumberType("CCD").E2N_Number);

			testWrapper.SocialCreditCode = "USC00000021234567890";
			var uscNum = testWrapper.DocAddressNumbers.FindFirstByNumberType("USC");
			AssertNotNull("USC", uscNum);
			AssertEquals("USC000000212345678", uscNum.E2N_Number);

			Assert(testOrgHeader.PK != testWrapper.OrganisationPK);
			testJobDocAddress.E2_AddressOverride = false;
			testWrapper.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			testWrapper.OverseasPartyCode = "AT222222";
			AssertEquals(testOrgHeader.PK, testWrapper.OrganisationPK);
		}

		public void TestFindMatchingOrganizationByCode()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org1, "CN", "USC", "CNUSC001");
			AddRegNo(org1, "CN", "CCD", "CNCCD001");
			AddRegNo(org1, "CN", "CIQ", "CNCIQ001");
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org2, "CN", "CCD", "CNCCD002");
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org3, "CN", "CIQ", "CNCIQ003");
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			AddRegNo(org4, "CN", "CIQ", "CNCIQ001");
			Factory.Save();
			var supplier = declaration.SupplierDocumentaryAddress;
			var importer = declaration.ImporterDocumentaryAddress;
			var manufacturer = declaration.ManufacturerDocumentaryAddress;
			var buyer = declaration.BuyerDocAddress;
			Assert(!supplier.SocialCreditCodeInfo.ReadOnly);
			Assert(!supplier.CustomsCodeInfo.ReadOnly);
			Assert(!supplier.CIQCodeInfo.ReadOnly);
			Assert(!supplier.OrganisationPK.IsValid);
			supplier.SocialCreditCode = "CNUSC001";
			AssertEquals(org1.PK, supplier.OrganisationPK);
			AssertEquals("CNCCD001", supplier.CustomsCode);
			AssertEquals("CNCIQ001", supplier.CIQCode);
			supplier.OrganisationPK = ZGuid.Empty;
			supplier.CIQCode = "CNCIQ001";
			Assert(!supplier.OrganisationPK.IsValid);
			manufacturer.CustomsCode = "CNCCD002";
			AssertEquals(org2.PK, manufacturer.OrganisationPK);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			importer.CIQCode = "CNCIQ003";
			AssertEquals(org3.PK, importer.OrganisationPK);
			buyer.SocialCreditCode = "111";
			Assert(!buyer.OrganisationPK.IsValid);
		}

		public void TestAutoFillCompanyNameByCode()
		{
			var dec1 = Factory.NewWithValidTestData<JobDeclaration>();
			dec1.JE_EntrySubmittedDate = ZDateTime.Now.AddDays(-2);
			var sda1 = dec1.SupplierDocumentaryAddress;
			sda1.E2_AddressOverride = true;
			sda1.ChineseCompanyName = "SUP1";
			sda1.SocialCreditCode = "USC1";
			var dec2 = testDeclaration;
			dec2.JE_EntrySubmittedDate = ZDateTime.Now;
			var ida2 = testWrapper;
			ida2.ChineseCompanyName = "IMP2";
			ida2.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			ida2.OverseasPartyCode = "AEO2";
			var dec3 = Factory.NewWithValidTestData<JobDeclaration>();
			var ida3 = dec3.ImporterDocumentaryAddress;
			ida3.E2_AddressOverride = true;
			ida3.ChineseCompanyName = "IMP3";
			ida3.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			ida3.OverseasPartyCode = "AEO2";
			var dec4 = Factory.NewWithValidTestData<JobDeclaration>();
			dec4.JE_EntrySubmittedDate = ZDateTime.Now.AddDays(-1);
			var sda4 = dec4.SupplierDocumentaryAddress;
			sda4.E2_AddressOverride = true;
			sda4.ChineseCompanyName = "SUP4";
			sda4.SocialCreditCode = "USC1";
			sda4.CustomsCode = "CCD4";
			sda4.CIQCode = "CIQ4";
			var auCompany = Factory.NewWithValidTestData<GlbCompany>();
			auCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var auBranch = Factory.NewWithValidTestData<GlbBranch>();
			auBranch.GB_GC = auCompany.PK;
			var dec5 = Factory.NewWithValidTestData<JobDeclaration>();
			dec5.JE_GB = auBranch.PK;
			dec5.JE_EntrySubmittedDate = ZDateTime.Now;
			var sda5 = dec5.SupplierDocumentaryAddress;
			sda5.E2_AddressOverride = true;
			sda5.ChineseCompanyName = "SUP5";
			sda5.SocialCreditCode = "USC1";
			Factory.Save();
			var dec6 = Factory.NewWithValidTestData<JobDeclaration>();
			var sda6 = dec6.SupplierDocumentaryAddress;
			sda6.E2_AddressOverride = true;
			sda6.SocialCreditCode = "USC1";
			var ida6 = dec6.ImporterDocumentaryAddress;
			ida6.E2_AddressOverride = true;
			ida6.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			ida6.OverseasPartyCode = "AEO2";
			AssertEquals("SUP4", sda6.ChineseCompanyName);
			AssertEquals("CCD4", sda6.CustomsCode);
			AssertEquals("CIQ4", sda6.CIQCode);
			AssertEquals("IMP3", ida6.ChineseCompanyName);
			var dec7 = Factory.NewWithValidTestData<JobDeclaration>();
			var sda7 = dec7.SupplierDocumentaryAddress;
			sda7.E2_AddressOverride = true;
			sda7.CustomsCode = "CCD7";
			var ida7 = dec7.ImporterDocumentaryAddress;
			ida7.E2_AddressOverride = true;
			ida7.ChineseCompanyName = "IMP7";
			sda7.SocialCreditCode = "USC1";
			ida7.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			ida7.OverseasPartyCode = "AEO2";
			AssertEquals("", sda7.ChineseCompanyName);
			AssertEquals("CCD7", sda7.CustomsCode);
			AssertEquals("", sda7.CIQCode);
			AssertEquals("IMP7", ida7.ChineseCompanyName);
		}

		static void AddRegNo(OrgHeader org1, string countryCode, string codeType, string regNo)
		{
			var customsCode = org1.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = countryCode;
			customsCode.OK_CodeType = codeType;
			customsCode.OK_CustomsRegNo = regNo;
		}

		public void TestAddressOverrideChanged()
		{
			var importer = testOrgHeader;
			var importerCIQ = importer.CustomsCodes.AddNew();
			importerCIQ.OK_CodeType = "CIQ";
			importerCIQ.OK_RN_NKCodeCountry = "CN";
			importerCIQ.OK_CustomsRegNo = "CIQ0000001";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var supplierCCD = supplier.CustomsCodes.AddNew();
			supplierCCD.OK_CodeType = "CCD";
			supplierCCD.OK_RN_NKCodeCountry = "CN";
			supplierCCD.OK_CustomsRegNo = "CCD0000001";
			testDeclaration.JE_OH_Supplier = supplier.PK;
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var buyerUSC = buyer.CustomsCodes.AddNew();
			buyerUSC.OK_CodeType = "USC";
			buyerUSC.OK_RN_NKCodeCountry = "CN";
			buyerUSC.OK_CustomsRegNo = "USC0000001";
			testDeclaration.JE_OH_Buyer = buyer.PK;
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturerCIQ = manufacturer.CustomsCodes.AddNew();
			manufacturerCIQ.OK_CodeType = "CIQ";
			manufacturerCIQ.OK_RN_NKCodeCountry = "CN";
			manufacturerCIQ.OK_CustomsRegNo = "CIQ0000002";
			testDeclaration.JE_OH_Manufacturer = manufacturer.PK;
			var importerWrapper = testDeclaration.ImporterDocumentaryAddress;
			var supplierWrapper = testDeclaration.SupplierDocumentaryAddress;
			var buyerWrapper = testDeclaration.BuyerDocAddress;
			var manufacturerWrapper = testDeclaration.ManufacturerDocumentaryAddress;
			ResetCodeValue();
			void ResetOverride(bool toOverriden)
			{
				importerWrapper.E2_AddressOverride = supplierWrapper.E2_AddressOverride = buyerWrapper.E2_AddressOverride = manufacturerWrapper.E2_AddressOverride = !toOverriden;
				importerWrapper.E2_AddressOverride = supplierWrapper.E2_AddressOverride = buyerWrapper.E2_AddressOverride = manufacturerWrapper.E2_AddressOverride = toOverriden;
			}

			void ResetCodeValue()
			{
				importerWrapper.CIQCode = "CIQ0000001";
				supplierWrapper.CustomsCode = "CCD0000001";
				buyerWrapper.SocialCreditCode = "USC0000001";
				manufacturerWrapper.CIQCode = "CIQ0000002";
			}

			testDeclaration.JE_MessageType = "IMP";
			testDeclaration.JE_MessageSubType = "CUS";
			ResetOverride(true);
			Factory.Save();
			Assert("IMP+CUS, ImporterDocumentaryAddress CIQCode should be defaulted to USC/CCD/CIQ", importerWrapper.CIQCode == "CIQ0000001");
			Assert("IMP+CUS, SupplierDocumentaryAddress CustomsCode should be cleared", supplierWrapper.CustomsCode.IsEmpty);
			Assert("IMP+CUS, BuyerDocAddress SocialCreditCode should be defaulted to USC/CCD/CIQ", buyerWrapper.SocialCreditCode == "USC0000001");
			Assert("IMP+CUS, ManufacturerDocumentaryAddress CIQCode should be cleared", manufacturerWrapper.CIQCode.IsEmpty);

			ResetOverride(false);
			Factory.Save();
			ResetCodeValue();

			testDeclaration.JE_MessageSubType = "REC";
			ResetOverride(true);
			Factory.Save();
			Assert("IMP+REC, ImporterDocumentaryAddress CIQCode should be defaulted to USC/CCD/CIQ", importerWrapper.CIQCode == "CIQ0000001");
			Assert("IMP+REC, SupplierDocumentaryAddress CustomsCode should be cleared", supplierWrapper.CustomsCode.IsEmpty);
			Assert("IMP+REC, BuyerDocAddress SocialCreditCode should be defaulted to USC/CCD/CIQ", buyerWrapper.SocialCreditCode == "USC0000001");
			Assert("IMP+REC, ManufacturerDocumentaryAddress CIQCode should be cleared", manufacturerWrapper.CIQCode.IsEmpty);

			ResetOverride(false);
			Factory.Save();
			ResetCodeValue();

			testDeclaration.JE_MessageType = "EXP";
			ResetOverride(true);
			Factory.Save();
			Assert("EXP+REC, ImporterDocumentaryAddress CIQCode should be cleared", importerWrapper.CIQCode.IsEmpty);
			Assert("EXP+REC, SupplierDocumentaryAddress CustomsCode should be defaulted to USC/CCD/CIQ", supplierWrapper.CustomsCode == "CCD0000001");
			Assert("EXP+REC, BuyerDocAddress SocialCreditCode should be cleared", buyerWrapper.SocialCreditCode.IsEmpty);
			Assert("EXP+REC, ManufacturerDocumentaryAddress CIQCode should be defaulted to USC/CCD/CIQ", manufacturerWrapper.CIQCode == "CIQ0000002");

			ResetOverride(false);
			Factory.Save();
			ResetCodeValue();

			testDeclaration.JE_MessageSubType = "BTH";
			ResetOverride(true);
			Factory.Save();
			Assert("EXP+BTH, ImporterDocumentaryAddress CIQCode should be defaulted to USC/CCD/CIQ", importerWrapper.CIQCode == "CIQ0000001");
			Assert("EXP+BTH, SupplierDocumentaryAddress CustomsCode should be defaulted to USC/CCD/CIQ", supplierWrapper.CustomsCode == "CCD0000001");
			Assert("EXP+BTH, BuyerDocAddress SocialCreditCode should be defaulted to USC/CCD/CIQ", buyerWrapper.SocialCreditCode == "USC0000001");
			Assert("EXP+BTH, ManufacturerDocumentaryAddress CIQCode should be defaulted to USC/CCD/CIQ", manufacturerWrapper.CIQCode == "CIQ0000002");
		}

		public void TestCodeRequirements()
		{
			var importerWrapper = testDeclaration.ImporterDocumentaryAddress;
			var supplierWrapper = testDeclaration.SupplierDocumentaryAddress;
			var buyerWrapper = testDeclaration.BuyerDocAddress;
			var manufacturerWrapper = testDeclaration.ManufacturerDocumentaryAddress;
			void SetOverrides(bool value)
			{
				importerWrapper.E2_AddressOverride = supplierWrapper.E2_AddressOverride = buyerWrapper.E2_AddressOverride = manufacturerWrapper.E2_AddressOverride = value;
			}

			testDeclaration.JE_MessageType = "IMP";
			testDeclaration.JE_MessageSubType = "CUS";
			SetOverrides(false);
			AssertCodeRequirements(importerWrapper, true, false, true, false);
			AssertCodeRequirements(supplierWrapper, false, false, false, true);
			AssertCodeRequirements(buyerWrapper, false, true, true, false);
			AssertCodeRequirements(manufacturerWrapper, false, false, false, false);
			testDeclaration.JE_MessageSubType = "REC";
			AssertCodeRequirements(importerWrapper, true, false, true, false);
			AssertCodeRequirements(supplierWrapper, false, false, false, true);
			AssertCodeRequirements(buyerWrapper, false, true, true, false);
			AssertCodeRequirements(manufacturerWrapper, false, false, false, false);
			testDeclaration.JE_MessageType = "EXP";
			AssertCodeRequirements(importerWrapper, false, false, false, true);
			AssertCodeRequirements(supplierWrapper, true, false, true, false);
			AssertCodeRequirements(buyerWrapper, false, false, false, false);
			AssertCodeRequirements(manufacturerWrapper, false, true, true, false);
			testDeclaration.JE_MessageSubType = "BTH";
			AssertCodeRequirements(importerWrapper, true, false, true, false);
			AssertCodeRequirements(supplierWrapper, true, false, true, false);
			AssertCodeRequirements(buyerWrapper, false, true, true, false);
			AssertCodeRequirements(manufacturerWrapper, false, true, true, false);
		}

		class CNJobDocAddressLightValidationTester : LightValidationTester
		{
			public CNJobDocAddressLightValidationTester(BusinessObject bo) : base(bo) { }

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				return base.ShouldTestProperty(info) && info.Name != JobDocAddressNumber.Schema.E2N_NumberType;
			}
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new CNJobDocAddressLightValidationTester(bizObjToTest);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var testWrapper = Factory.NewWithValidTestData<JobDeclaration>().ImporterDocumentaryAddress;
			testWrapper.E2_AddressOverride = true;
			return testWrapper;
		}

		protected override void SetUp()
		{
			base.SetUp();

			temporaryCountry = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China);

			testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			testDeclaration.JE_OH_Importer = testOrgHeader.PK;
			testJobDocAddress = testDeclaration.ImporterDocumentaryAddress;
			testJobDocAddress.E2_OA_Address = testOrgHeader.MainAddress.PK;
			testWrapper = testJobDocAddress as CNJobDocAddress;
			Factory.Save();
		}

		protected override void TearDown()
		{
			temporaryCountry.Dispose();
			base.TearDown();
		}

		JobDeclaration testDeclaration;
		OrgHeader testOrgHeader;
		CNJobDocAddress testWrapper;
		JobDocAddress testJobDocAddress;

		IDisposable temporaryCountry;

		static void AssertCodeRequirements(CNJobDocAddress wrapper, bool requiresTradeOrg, bool requiresOwnerOrg, bool requiresDomesticOrg, bool reqiresOverseasOrg)
		{
			CombineAssertions(() =>
			{
				Assert("RequiresTradeOrg", wrapper.RequiresTradeOrg == requiresTradeOrg);
				Assert("RequiresOwnerOrg", wrapper.RequiresOwnerOrg == requiresOwnerOrg);
				Assert("RequiresDomesticOrg", wrapper.RequiresDomesticOrg == requiresDomesticOrg);
				Assert("RequiresOverseasOrg", wrapper.RequiresOverseasOrg == reqiresOverseasOrg);
			});
		}
	}
}
