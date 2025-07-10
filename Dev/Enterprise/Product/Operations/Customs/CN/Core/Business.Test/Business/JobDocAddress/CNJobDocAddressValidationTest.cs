using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOverseasPartyCodeType()
		{
			var importerDocAddress = declaration.ImporterDocumentaryAddress;
			importerDocAddress.E2_AddressOverride = true;

			importerDocAddress.OverseasPartyCodeType = "SMR";
			importerDocAddress.Validation.ValidateAll();
			AssertNoErrors(importerDocAddress.OverseasPartyCodeTypeInfo);

			importerDocAddress.OverseasPartyCodeType = "XXX";
			importerDocAddress.Validation.ValidateAll();
			AssertHasErrorContaining("OverseasPartyCodeTypeInfo not allow incorrect input.", importerDocAddress.OverseasPartyCodeTypeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckDepotAndWarehouseAddresses()
		{
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_Code = "OR3";
			var depotAddress = depot.Addresses.AddNew();
			depotAddress.OA_OH = depot.PK;
			var depotCodeCPW = depot.CustomsCodes.AddNew();
			depotCodeCPW.OK_OH = depot.PK;
			depotCodeCPW.OK_CodeType = "CPW";
			depotCodeCPW.OK_CustomsRegNo = "W10000001";
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			warehouse.OH_Code = "OR4";
			var warehouseAddress = warehouse.Addresses.AddNew();
			warehouseAddress.OA_OH = warehouse.PK;
			var warehouseCodeCPD = warehouse.CustomsCodes.AddNew();
			warehouseCodeCPD.OK_OH = warehouseCodeCPD.PK;
			warehouseCodeCPD.OK_CodeType = "CPD";
			warehouseCodeCPD.OK_CustomsRegNo = "D10000001";
			declaration.DepotDocAddress.OrganisationPK = depot.PK;
			declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;
			declaration.WarehouseDocAddress.E2_OA_Address = warehouseAddress.PK;
			declaration.DepotDocAddress.Validation.ValidateAll();
			declaration.WarehouseDocAddress.Validation.ValidateAll();
			AssertHasWarning(declaration.DepotDocAddress.E2_OA_AddressInfo, "CPD Registration Number for CN is required for Customs Depot Address. Please press F3, go to Details -> Config -> Registration Numbers/Codes and enter the Registration Number.");
			AssertHasWarning(declaration.WarehouseDocAddress.E2_OA_AddressInfo, "CPW Registration Number for CN is required for Customs Warehouse Address. Please press F3, go to Details -> Config -> Registration Numbers/Codes and enter the Registration Number.");
			var depotCodeCPD = depot.CustomsCodes.AddNew();
			depotCodeCPD.OK_OH = depot.PK;
			depotCodeCPD.OK_CodeType = "CPD";
			depotCodeCPD.OK_CustomsRegNo = "D10000002";
			var warehouseCodeCPW = warehouse.CustomsCodes.AddNew();
			warehouseCodeCPW.OK_OH = warehouse.PK;
			warehouseCodeCPW.OK_CodeType = "CPW";
			warehouseCodeCPW.OK_CustomsRegNo = "W1000002";
			declaration.DepotDocAddress.Validation.ValidateAll();
			declaration.WarehouseDocAddress.Validation.ValidateAll();
			AssertHasWarning(declaration.DepotDocAddress.E2_OA_AddressInfo, "CPD Registration Number for CN is required for Customs Depot Address. Please press F3, go to Details -> Config -> Registration Numbers/Codes and enter the Registration Number.");
			AssertHasWarning(declaration.WarehouseDocAddress.E2_OA_AddressInfo, "CPW Registration Number for CN is required for Customs Warehouse Address. Please press F3, go to Details -> Config -> Registration Numbers/Codes and enter the Registration Number.");
			depotCodeCPD.OK_OA_PremisesAddress = depotAddress.PK;
			warehouseCodeCPW.OK_OA_PremisesAddress = warehouseAddress.PK;
			declaration.DepotDocAddress.Validation.ValidateAll();
			declaration.WarehouseDocAddress.Validation.ValidateAll();
			AssertNoWarning(declaration.DepotDocAddress.E2_OA_AddressInfo, "CPD Registration Number for CN is required for Customs Depot Address. Please press F3, go to Details -> Config -> Registration Numbers/Codes and enter the Registration Number.");
			AssertNoWarning(declaration.WarehouseDocAddress.E2_OA_AddressInfo, "CPW Registration Number for CN is required for Customs Warehouse Address. Please press F3, go to Details -> Config -> Registration Numbers/Codes and enter the Registration Number.");
		}

		public void TestCheckE2_Phone_Formatted()
		{
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			ForceRefreshDeclarationDocOrgs();
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			ForceRefreshDeclarationDocOrgs();
			declaration.SupplierDocumentaryAddress.Validation.ValidateE2_Phone_Formatted();
			declaration.ImporterDocumentaryAddress.Validation.ValidateE2_Phone_Formatted();
			AssertHasWarning(declaration.SupplierDocumentaryAddress.E2_Phone_FormattedInfo, "You have not entered a Supplier Documentary Address: Telephone Number.");
			AssertNoWarnings(declaration.ImporterDocumentaryAddress.E2_Phone_FormattedInfo);
			declaration.SupplierDocumentaryAddress.E2_Contact = "";
			declaration.SupplierDocumentaryAddress.Validation.ValidateE2_Phone_Formatted();
			declaration.ImporterDocumentaryAddress.Validation.ValidateE2_Phone_Formatted();
			AssertNoWarnings(declaration.SupplierDocumentaryAddress.E2_Phone_FormattedInfo);
			AssertNoWarnings(declaration.ImporterDocumentaryAddress.E2_Phone_FormattedInfo);
			declaration.SupplierDocumentaryAddress.E2_Contact = "Supplier Contact";
			declaration.SupplierDocumentaryAddress.E2_Phone = "+61 (2) 1234 5678";
			declaration.SupplierDocumentaryAddress.Validation.ValidateE2_Phone_Formatted();
			declaration.ImporterDocumentaryAddress.Validation.ValidateE2_Phone_Formatted();
			AssertNoWarnings(declaration.SupplierDocumentaryAddress.E2_Phone_FormattedInfo);
			AssertNoWarnings(declaration.ImporterDocumentaryAddress.E2_Phone_FormattedInfo);
		}

		public void TestCheckMandatories()
		{
			var importerDocAddress = declaration.ImporterDocumentaryAddress;
			var targetValidation = importerDocAddress.Validation;
			var chineseNameInfo = importerDocAddress.ChineseCompanyNameInfo;
			targetValidation.ValidateAll();
			AssertNoMessageErrors(chineseNameInfo);
			importerDocAddress.E2_AddressOverride = true;
			importerDocAddress.ChineseCompanyName = "COMPANY";
			targetValidation.ValidateAll();
			AssertNoMessageErrors(chineseNameInfo);
			importerDocAddress.ChineseCompanyName = string.Empty;
			targetValidation.ValidateAll();
			AssertHasError(chineseNameInfo, "Please enter a Company Name, or remove the override for this Address.");
		}

		public void TestCheckImporterCodes()
		{
			var importerDocAddress = declaration.ImporterDocumentaryAddress;
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "CUS";
			ValidateRegCodes(importerDocAddress);
			AssertNoNote(importerDocAddress, "CCD");
			AssertNoNote(importerDocAddress, "USC");
			AssertNoNote(importerDocAddress, "CIQ");
			importerDocAddress.OrganisationPK = importer.PK;
			importerDocAddress.E2_OA_Address = importerAddress.PK;
			importer.OH_FullName = "COMPANY";
			ValidateRegCodes(importerDocAddress);
			AssertRegCodeInfo(importerDocAddress, "CCD", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(importerDocAddress, "USC", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(importerDocAddress, "CIQ", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			declaration.JE_MessageType = "EXP";
			declaration.JE_MessageSubType = "REC";
			ValidateRegCodes(importerDocAddress);
			AssertNoNote(importerDocAddress, "CCD");
			AssertNoNote(importerDocAddress, "USC");
			AssertNoNote(importerDocAddress, "CIQ");
			declaration.JE_MessageSubType = "BTH";
			ValidateRegCodes(importerDocAddress);
			AssertRegCodeInfo(importerDocAddress, "CCD", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(importerDocAddress, "USC", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(importerDocAddress, "CIQ", shouldHave: false, messageErrorOrWarning: true, requiredOrFormat: true);
			declaration.CustomsEntryInstructions.AddNew().CEI_CIQRequires = true;
			ValidateRegCodes(importerDocAddress);
			AssertRegCodeInfo(importerDocAddress, "CIQ", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			var ciqCode = importer.CustomsCodes.AddNew();
			ciqCode.OK_CodeType = OrgCusCode.ChinaCodeTypes.CIQ;
			ciqCode.OK_CustomsRegNo = "C123";
			ciqCode.OK_RN_NKCodeCountry = "CN";
			ValidateRegCodes(importerDocAddress);
			AssertRegCodeInfo(importerDocAddress, "CCD", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(importerDocAddress, "USC", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(importerDocAddress, "CIQ", shouldHave: false, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(importerDocAddress, "CIQ", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: false);
			ciqCode.OK_CustomsRegNo = "1234567890";
			ValidateRegCodes(importerDocAddress);
			AssertRegCodeInfo(importerDocAddress, "CCD", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(importerDocAddress, "USC", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertNoNote(importerDocAddress, "CIQ");
			importer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "C123", "CN");
			ValidateRegCodes(importerDocAddress);
			AssertRegCodeInfo(importerDocAddress, "CCD", shouldHave: false, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(importerDocAddress, "CCD", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: false);
			importer.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "C123", "CN");
			ValidateRegCodes(importerDocAddress);
			AssertRegCodeInfo(importerDocAddress, "USC", shouldHave: false, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(importerDocAddress, "USC", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: false);
		}

		public void TestCheckSupplierCodes()
		{
			var supplierDocAddress = declaration.SupplierDocumentaryAddress;
			supplier.OH_FullName = "COMPANY";
			ValidateRegCodes(supplierDocAddress);
			AssertNoNote(supplierDocAddress, "CCD");
			AssertNoNote(supplierDocAddress, "USC");
			AssertNoNote(supplierDocAddress, "CIQ");
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "CUS";
			AssertNoNote(supplierDocAddress, "CCD");
			AssertNoNote(supplierDocAddress, "USC");
			AssertNoNote(supplierDocAddress, "CIQ");
			declaration.JE_MessageType = "EXP";
			declaration.JE_MessageSubType = "REC";
			ValidateRegCodes(supplierDocAddress);
			AssertNoNote(supplierDocAddress, "CCD");
			AssertNoNote(supplierDocAddress, "USC");
			AssertNoNote(supplierDocAddress, "CIQ");
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "BTH";
			ValidateRegCodes(supplierDocAddress);
			AssertNoNote(supplierDocAddress, "CCD");
			AssertNoNote(supplierDocAddress, "USC");
			AssertNoNote(supplierDocAddress, "CIQ");
			declaration.JE_MessageType = "EXP";
			declaration.JE_MessageSubType = "REC";
			supplierDocAddress.OrganisationPK = supplier.PK;
			supplierDocAddress.E2_OA_Address = supplierAddress.PK;
			ValidateRegCodes(supplierDocAddress);
			AssertRegCodeInfo(supplierDocAddress, "CCD", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(supplierDocAddress, "USC", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(supplierDocAddress, "CIQ", shouldHave: false, messageErrorOrWarning: true, requiredOrFormat: true);
			declaration.JE_MessageSubType = "BTH";
			ValidateRegCodes(supplierDocAddress);
			AssertRegCodeInfo(supplierDocAddress, "CCD", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(supplierDocAddress, "USC", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(supplierDocAddress, "CIQ", shouldHave: false, messageErrorOrWarning: true, requiredOrFormat: true);
			var ccdCode = supplier.CustomsCodes.AddNew();
			ccdCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			ccdCode.OK_CustomsRegNo = "C123";
			ccdCode.OK_RN_NKCodeCountry = "CN";
			supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "C123", "CN");
			ValidateRegCodes(supplierDocAddress);
			AssertRegCodeInfo(supplierDocAddress, "CCD", shouldHave: false, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(supplierDocAddress, "CCD", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: false);
			AssertRegCodeInfo(supplierDocAddress, "USC", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			AssertRegCodeInfo(supplierDocAddress, "CIQ", shouldHave: false, messageErrorOrWarning: true, requiredOrFormat: true);
			ccdCode.OK_CustomsRegNo = "1234567890";
			ValidateRegCodes(supplierDocAddress);
			AssertNoNote(supplierDocAddress, "CCD");
			declaration.CustomsEntryInstructions.AddNew().CEI_CIQRequires = true;
			ValidateRegCodes(supplierDocAddress);
			AssertRegCodeInfo(supplierDocAddress, "CIQ", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			supplier.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.CIQ, "C123", "CN");
			ValidateRegCodes(supplierDocAddress);
			AssertRegCodeInfo(supplierDocAddress, "CIQ", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: false);
		}

		public void TestCheckBuyerCodes()
		{
			var buyerDocAddress = declaration.BuyerDocAddress;
			buyer.OH_FullName = "COMPANY";
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "CUS";
			ValidateRegCodes(buyerDocAddress);
			AssertNoNote(buyerDocAddress, "CCD");
			AssertNoNote(buyerDocAddress, "USC");
			AssertNoNote(buyerDocAddress, "CIQ");
			buyerDocAddress.OrganisationPK = buyer.PK;
			buyerDocAddress.E2_OA_Address = buyerAddress.PK;
			ValidateRegCodes(buyerDocAddress);
			AssertNoNote(buyerDocAddress, "CCD");
			AssertRegCodeInfo(buyerDocAddress, "USC", shouldHave: true, messageErrorOrWarning: false, requiredOrFormat: true);
			AssertRegCodeInfo(buyerDocAddress, "CIQ", shouldHave: false, messageErrorOrWarning: false, requiredOrFormat: true);
			declaration.CustomsEntryInstructions.AddNew().CEI_CIQRequires = true;
			ValidateRegCodes(buyerDocAddress);
			AssertNoNote(buyerDocAddress, "CCD");
			AssertRegCodeInfo(buyerDocAddress, "USC", shouldHave: true, messageErrorOrWarning: false, requiredOrFormat: true);
			AssertRegCodeInfo(buyerDocAddress, "CIQ", shouldHave: true, messageErrorOrWarning: true, requiredOrFormat: true);
			var cacCode = buyer.CustomsCodes.AddNew();
			cacCode.OK_CodeType = "USC";
			cacCode.OK_CustomsRegNo = "REG002";
			ValidateRegCodes(buyerDocAddress);
			AssertRegCodeInfo(buyerDocAddress, "USC", shouldHave: false, messageErrorOrWarning: false, requiredOrFormat: true);
			AssertRegCodeInfo(buyerDocAddress, "USC", shouldHave: true, messageErrorOrWarning: false, requiredOrFormat: false);
			cacCode.OK_CustomsRegNo = "12345678912345678U";
			ValidateRegCodes(buyerDocAddress);
			AssertNoNote(buyerDocAddress, "USC");
		}

		public void TestCheckManufacturerCodes()
		{
			var manufacturerDocAddress = declaration.ManufacturerDocumentaryAddress;
			manufacturer.OH_FullName = "COMPANY";
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "CUS";
			ValidateRegCodes(manufacturerDocAddress);
			AssertNoNote(manufacturerDocAddress, "CCD");
			AssertNoNote(manufacturerDocAddress, "USC");
			AssertNoNote(manufacturerDocAddress, "CIQ");
			declaration.JE_MessageType = "EXP";
			declaration.JE_MessageSubType = "REC";
			ValidateRegCodes(manufacturerDocAddress);
			AssertNoNote(manufacturerDocAddress, "CCD");
			AssertNoNote(manufacturerDocAddress, "USC");
			AssertNoNote(manufacturerDocAddress, "CIQ");
			manufacturerDocAddress.OrganisationPK = manufacturer.PK;
			ValidateRegCodes(manufacturerDocAddress);
			AssertNoNote(manufacturerDocAddress, "CCD");
			AssertRegCodeInfo(manufacturerDocAddress, "USC", shouldHave: true, messageErrorOrWarning: false, requiredOrFormat: true);
			var cacCode = manufacturer.CustomsCodes.AddNew();
			cacCode.OK_CodeType = "USC";
			cacCode.OK_CustomsRegNo = "REG002";
			ValidateRegCodes(manufacturerDocAddress);
			AssertNoNote(manufacturerDocAddress, "CCD");
			AssertRegCodeInfo(manufacturerDocAddress, "USC", shouldHave: false, messageErrorOrWarning: false, requiredOrFormat: true);
			AssertRegCodeInfo(manufacturerDocAddress, "USC", shouldHave: true, messageErrorOrWarning: false, requiredOrFormat: false);
			cacCode.OK_CustomsRegNo = "12345678912345678U";
			ValidateRegCodes(manufacturerDocAddress);
			AssertNoNote(manufacturerDocAddress, "CCD");
			AssertNoNote(manufacturerDocAddress, "USC");
			AssertNoNote(manufacturerDocAddress, "CIQ");
		}

		public void TestCheckOverseasPartyCode()
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, "AEO Mutual Recognition Country List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, Core.Constants.CountryCodes.Switzerland, "瑞士", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			newFactory.Save();

			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "CUS";
			var docAddress = declaration.SupplierDocumentaryAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.OverseasPartyCodeType = OrgCusCode.ChinaCodeTypes.AEO;
			docAddress.OverseasPartyCode = "CHX001";
			docAddress.Validation.ValidateOverseasPartyCode();
			var targetInfo = docAddress.OverseasPartyCodeInfo;
			AssertNoNotifications("Should have no notifications for valid AEO number.", targetInfo);

			docAddress.OverseasPartyCode = "XXX";
			docAddress.Validation.ValidateOverseasPartyCode();
			AssertHasMessageError("Should have message error for unvalid country code.", targetInfo, "The code should start with 2-letter country code of an AEO mutual recognition country.");

			docAddress.OverseasPartyCode = "A";
			docAddress.Validation.ValidateOverseasPartyCode();
			AssertHasError("Should have error for AEO number less than 3 chars.", targetInfo, "AEO number should have at least 3 characters.");
		}

		public void TestValidationModeProvider()
		{
			var docAddress = declaration.SupplierDocumentaryAddress;
			ValidationExtensionsTest.AssertValidationModeProvider(declaration, docAddress.Validation.ValidationModeProvider);
			docAddress = Factory.New<CNJobDocAddress>();
			AssertNull(docAddress.Validation.ValidationModeProvider);
		}

		static void ValidateRegCodes(CNJobDocAddress wrapper)
		{
			wrapper.Validation.ValidateSocialCreditCode();
			wrapper.Validation.ValidateCustomsCode();
			wrapper.Validation.ValidateCIQCode();
		}

		void AssertRegCodeInfo(CNJobDocAddress wrapper, string codeType, bool shouldHave, bool messageErrorOrWarning, bool requiredOrFormat)
		{
			var targetInfo = regCodeInfoMapping[codeType](wrapper);
			var message = requiredOrFormat ? ValidationHelper.RegNumIsRequired(codeType) : ValidationHelper.InvalidRegNoFormat(codeType);
			if (messageErrorOrWarning)
			{
				if (shouldHave)
				{
					AssertHasMessageErrorContaining(targetInfo, message);
				}
				else
				{
					AssertNoMessageErrorContaining(targetInfo, message);
				}
			}
			else
			{
				if (shouldHave)
				{
					AssertHasWarningContaining(targetInfo, message);
				}
				else
				{
					AssertNoWarningContaining(targetInfo, message);
				}
			}
		}

		void ForceRefreshDeclarationDocOrgs()
		{
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_OH_Buyer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Buyer = buyer.PK;
		}

		void AssertNoNote(CNJobDocAddress wrapper, string codeType)
		{
			var targetInfo = regCodeInfoMapping[codeType](wrapper);
			AssertNoMessageErrors(targetInfo);
			AssertNoWarnings(targetInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "OR1";
			importerAddress = importer.Addresses.AddNew();
			importerAddress.OA_OH = importer.PK;
			importerContact = importer.Contacts.AddNew();
			importerContact.OC_OH = importer.PK;
			importerContact.OC_ContactName = "Importer Contact";
			var importerAllocation = importerContact.Allocations.AddNew();
			importerAllocation.PC_Type = "CNC";
			supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "OR2";
			supplierAddress = supplier.Addresses.AddNew();
			supplierAddress.OA_OH = supplier.PK;
			supplierContact = supplier.Contacts.AddNew();
			supplierContact.OC_OH = supplier.PK;
			supplierContact.OC_ContactName = "Supplier Contact";
			var supplierAllocation = supplierContact.Allocations.AddNew();
			supplierAllocation.PC_Type = "CNC";
			buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "OR3";
			buyerAddress = buyer.Addresses.AddNew();
			buyerAddress.OA_OH = buyer.PK;
			buyerContact = buyer.Contacts.AddNew();
			buyerContact.OC_OH = buyer.PK;
			buyerContact.OC_ContactName = "Buyer Contact";
			var buyerAllocation = buyerContact.Allocations.AddNew();
			buyerAllocation.PC_Type = "CNC";
			manufacturer = Factory.NewWithValidTestData<OrgHeader>();
		}

		readonly Dictionary<string, Func<CNJobDocAddress, ZPropertyInfo>> regCodeInfoMapping = new Dictionary<string, Func<CNJobDocAddress, ZPropertyInfo>>
		{
			{ OrgCusCode.ChinaCodeTypes.USC, wrapper => wrapper.SocialCreditCodeInfo },
			{ OrgCusCode.CodeTypes.CustomsClientCode, wrapper => wrapper.CustomsCodeInfo },
			{ OrgCusCode.ChinaCodeTypes.CIQ, wrapper => wrapper.CIQCodeInfo },
		};
		JobDeclaration declaration;
		OrgHeader importer;
		OrgHeader supplier;
		OrgHeader buyer;
		OrgHeader manufacturer;
		OrgAddress importerAddress;
		OrgAddress supplierAddress;
		OrgAddress buyerAddress;
		OrgContact importerContact;
		OrgContact supplierContact;
		OrgContact buyerContact;
	}
}
