using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ImportJobComInvoiceHeaderJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		public void TestValidateAllWhenStandAloneInvoice()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertInvoiceDocAddress(invoice.SupplierDocumentaryAddress);
			AssertInvoiceDocAddress(invoice.SupplierPickupDeliveryAddress);
			AssertInvoiceDocAddress(invoice.FinalConsigneeAddress);
			AssertInvoiceDocAddress(invoice.ExporterDocumentaryAddress);
			AssertInvoiceDocAddress(invoice.BuyerDocumentaryAddress);
		}

		void AssertInvoiceDocAddress(JobDocAddress docAddress)
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			docAddress.E2_Address1 = "Address 1";
			docAddress.E2_Address2 = "Address 2";
			docAddress.E2_CompanyName = "Company 1";
			docAddress.OrganisationPK = org1.PK;
			AssertNoExceptionThrown("No exception for validation of standalone invoice.", () => docAddress.Validation.ValidateAll());

			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "Address 3";
			docAddress.E2_Address2 = "Address 4";
			docAddress.E2_CompanyName = "Company 2";
			docAddress.OrganisationPK = org2.PK;
			AssertNoExceptionThrown("No exception for validation of standalone invoice.", () => docAddress.Validation.ValidateAll());
		}

		public void TestCheckSupplierDocumentAddresss_ContactOrAddressPhone()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var supplierDocAddresss = invoice.SupplierDocumentaryAddress;
			supplierDocAddresss.OrganisationPK = supplier.PK;

			supplierDocAddresss.Validation.ValidateAll();
			AssertNoMessageError(supplierDocAddresss.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			invoiceLine.CA_HCInd = Customs.Business.YesNoList.Codes.Yes;
			var hcPgaHeader = invoiceLine.HCPGAHeader;
			hcPgaHeader.CA_PESProgramInd = Customs.Business.YesNoList.Codes.Yes;
			supplierDocAddresss.Validation.ValidateAll();
			AssertHasMessageError(supplierDocAddresss.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			var contact = supplier.Contacts.AddNew();
			var allocatedContact = contact.Allocations.AddNew();
			allocatedContact.PC_Type = "CAP";
			supplierDocAddresss.Validation.ValidateAll();
			AssertNoMessageError(supplierDocAddresss.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);
			AssertHasMessageError(supplierDocAddresss.OrganisationPKInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			contact.OC_Phone = "15850503354";
			supplierDocAddresss.Validation.ValidateAll();
			AssertNoMessageError(supplierDocAddresss.OrganisationPKInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			hcPgaHeader.CA_PESProgramInd = Customs.Business.YesNoList.Codes.No;
			hcPgaHeader.CA_CPRProgramInd = Customs.Business.YesNoList.Codes.Yes;

			supplierDocAddresss.Validation.ValidateAll();
			AssertNoWarningContaining(supplierDocAddresss.E2_OA_AddressInfo, OrganisationValidation.ContactPhoneNumberIsRecommended);
			AssertHasWarningContaining(supplierDocAddresss.E2_OA_AddressInfo, OrganisationValidation.ContactEmailIsRecommended);

			contact.OC_Phone = "";
			contact.OC_Email = "test@test.com";

			supplierDocAddresss.Validation.ValidateAll();
			AssertHasWarningContaining(supplierDocAddresss.E2_OA_AddressInfo, OrganisationValidation.ContactPhoneNumberIsRecommended);
			AssertNoWarningContaining(supplierDocAddresss.E2_OA_AddressInfo, OrganisationValidation.ContactEmailIsRecommended);
		}

		public void TestCheckSupplierDocumentAddresss()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var supplierDocAddresss = invoiceHeader.SupplierDocumentaryAddress;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			supplierDocAddresss.Validation.ValidateAll();
			AssertNoMessageErrorContaining(supplierDocAddresss.OrganisationPKInfo, "You have not entered a Vendor");
			AssertNoMessageErrorContaining(supplierDocAddresss.E2_OA_AddressInfo, "You have not entered a Vendor");

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			supplierDocAddresss.Validation.ValidateAll();
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			AssertHasMessageErrorContaining(supplierDocAddresss.OrganisationPKInfo, "You have not entered a Vendor");

			supplierDocAddresss.OrganisationPK = supplier.PK;
			supplierDocAddresss.E2_OA_Address = ZGuid.NewZGuid();
			AssertNoMessageErrorContaining(supplierDocAddresss.OrganisationPKInfo, "You have not entered a Vendor");
			AssertHasMessageErrorContaining(supplierDocAddresss.E2_OA_AddressInfo, "Vendor Address is invalid and has following errors");

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			supplierDocAddresss.Validation.ValidateAll();
			AssertNoMessageErrorContaining(supplierDocAddresss.OrganisationPKInfo, "You have not entered a Vendor");
			AssertHasMessageErrorContaining(supplierDocAddresss.E2_OA_AddressInfo, "Vendor Address is invalid and has following errors");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_CompanyNameOverride = "1234567890123456789012345678901";
			org.MainAddress.OA_Address1 = "Address";
			org.MainAddress.OA_City = "City";
			supplierDocAddresss.OrganisationPK = org.PK;
			AssertHasWarningContaining(supplierDocAddresss.E2_OA_AddressInfo, "The length of data entered into name (31) exceeds the maximum allowed. Only the first 30 characters will be transmitted to Customs.");

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			supplierDocAddresss.Validation.ValidateAll();
			AssertNoWarningContaining(supplierDocAddresss.E2_OA_AddressInfo, "The length of data entered into name (31) exceeds the maximum allowed. Only the first 30 characters will be transmitted to Customs.");

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);

			foreach (var country in new[] { Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.PuertoRico,
				Core.Constants.CountryCodes.VirginIslands, Core.Constants.CountryCodes.UnitedStatesMinorIslands })
			{
				ValidationTestHelper.SetTotalValueForDuty(JobComInvoiceHeaderTest.VFDOverLimit, declaration);
				invoiceHeader.CA_RN_NKExport = country;
				org.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
				supplierDocAddresss.Validation.ValidateAll();
				AssertHasMessageError(supplierDocAddresss.E2_OA_AddressInfo, ImportJobComInvoiceHeaderJobDocAddressValidation.VendorStateIsInvalid);
			}
		}

		public void TestCheckSupplierPickupDeliveryAddress()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var shipperAddresss = invoiceHeader.SupplierPickupDeliveryAddress;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			shipperAddresss.Validation.ValidateAll();

			ValidationTestHelper.AssertAddressUsesCAAddressValidationIfOrgSpecified(shipperAddresss, "Shipper");
		}

		public void TestValidateSupplierDocumentAddressWhenAddressOverriden()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var shipperAddresss = invoiceHeader.SupplierPickupDeliveryAddress;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			invoiceHeader.SupplierDocumentaryAddress.E2_AddressOverride = true;
			invoiceHeader.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertHasMessageErrorContaining(invoiceHeader.SupplierDocumentaryAddress.E2_CompanyNameInfo, "You have not entered a name");
			invoiceHeader.SupplierDocumentaryAddress.E2_CompanyName = "123456789012345678901234567890123456";
			AssertNoMessageErrorContaining(invoiceHeader.SupplierDocumentaryAddress.E2_CompanyNameInfo, "You have not entered a name");
			AssertHasWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_CompanyNameInfo, "exceeds the maximum allowed");
			invoiceHeader.SupplierDocumentaryAddress.E2_CompanyName = "12345678901234567890123456789012345";
			AssertNoWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_CompanyNameInfo, "exceeds the maximum allowed");
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			invoiceHeader.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertHasWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_CompanyNameInfo, "exceeds the maximum allowed");
			invoiceHeader.SupplierDocumentaryAddress.E2_CompanyName = "123456789012345678901234567890";
			AssertNoWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_CompanyNameInfo, "exceeds the maximum allowed");

			invoiceHeader.SupplierDocumentaryAddress.E2_Address1 = "123456789012345678901234567890123456";
			invoiceHeader.SupplierDocumentaryAddress.E2_Address2 = "123456789012345678901234567890123456";
			AssertNoWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_Address1Info, "exceeds the maximum allowed");
			AssertNoWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_Address2Info, "exceeds the maximum allowed");

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			invoiceHeader.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertHasWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_Address1Info, "exceeds the maximum allowed");
			AssertHasWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_Address2Info, "exceeds the maximum allowed");
			invoiceHeader.SupplierDocumentaryAddress.E2_Address1 = "12345678901234567890123456789012345";
			invoiceHeader.SupplierDocumentaryAddress.E2_Address2 = "12345678901234567890123456789012345";
			AssertNoWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_Address1Info, "exceeds the maximum allowed");
			AssertNoWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_Address2Info, "exceeds the maximum allowed");

			AssertHasMessageErrorContaining(invoiceHeader.SupplierDocumentaryAddress.E2_CityInfo, "You have not entered a city");
			invoiceHeader.SupplierDocumentaryAddress.E2_City = "1234567890abcdefghijklmnopqrstuvwxyz";
			AssertNoMessageErrorContaining(invoiceHeader.SupplierDocumentaryAddress.E2_CityInfo, "You have not entered a city");
			AssertHasWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_CityInfo, "exceeds the maximum allowed");
			invoiceHeader.SupplierDocumentaryAddress.E2_City = "123456789";
			AssertNoWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_CityInfo, "exceeds the maximum allowed");

			invoiceHeader.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			AssertNoMessageErrorContaining(invoiceHeader.SupplierDocumentaryAddress.E2_StateInfo, "A province/state is required when the country/region is Canada or United States");
			AssertNoMessageErrorContaining(invoiceHeader.SupplierDocumentaryAddress.E2_PostcodeInfo, "A postal/zip code is required when the country/region is Canada or United States");
			invoiceHeader.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "CA";

			AssertHasMessageErrorContaining(invoiceHeader.SupplierDocumentaryAddress.E2_StateInfo, "A province/state is required when the country/region is Canada or United States");
			invoiceHeader.SupplierDocumentaryAddress.E2_State = "1234567890";
			AssertNoMessageErrorContaining(invoiceHeader.SupplierDocumentaryAddress.E2_StateInfo, "A province/state is required when the country/region is Canada or United States");
			AssertHasWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_StateInfo, "exceeds the maximum allowed");
			invoiceHeader.SupplierDocumentaryAddress.E2_State = "123456789";
			AssertNoWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_StateInfo, "exceeds the maximum allowed");

			AssertHasMessageErrorContaining(invoiceHeader.SupplierDocumentaryAddress.E2_PostcodeInfo, "A postal/zip code is required when the country/region is Canada or United States");
			invoiceHeader.SupplierDocumentaryAddress.E2_Postcode = "1234567890";
			AssertNoMessageErrorContaining(invoiceHeader.SupplierDocumentaryAddress.E2_PostcodeInfo, "A postal/zip code is required when the country/region is Canada or United States");
			AssertHasWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_PostcodeInfo, "exceeds the maximum allowed");
			invoiceHeader.SupplierDocumentaryAddress.E2_Postcode = "123456789";
			AssertNoWarningContaining(invoiceHeader.SupplierDocumentaryAddress.E2_PostcodeInfo, "exceeds the maximum allowed");

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			foreach (var country in new[] { Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.PuertoRico,
				Core.Constants.CountryCodes.VirginIslands, Core.Constants.CountryCodes.UnitedStatesMinorIslands })
			{
				ValidationTestHelper.SetTotalValueForDuty(JobComInvoiceHeaderTest.VFDOverLimit, declaration);
				invoiceHeader.SupplierDocumentaryAddress.E2_State = "AL";
				invoiceHeader.CA_RN_NKExport = country;
				invoiceHeader.SupplierDocumentaryAddress.Validation.ValidateAll();
				AssertHasMessageError(invoiceHeader.SupplierDocumentaryAddress.E2_StateInfo, ImportJobComInvoiceHeaderJobDocAddressValidation.VendorStateIsInvalid);
			}
		}

		public void TestValidateSupplierPickupDeliveryAddressWhenAddressOverriden()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var shipperAddresss = invoiceHeader.SupplierPickupDeliveryAddress;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader.SupplierPickupDeliveryAddress.E2_AddressOverride = true;
			invoiceHeader.SupplierPickupDeliveryAddress.Validation.ValidateAll();
			AssertHasMessageErrorContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_CompanyNameInfo, "You have not entered a name");
			invoiceHeader.SupplierPickupDeliveryAddress.E2_CompanyName = "12345678901234567890123456789012345678901234567890123456789012345678901";
			AssertNoMessageErrorContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_CompanyNameInfo, "You have not entered a name");
			AssertHasWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_CompanyNameInfo, "exceeds the maximum allowed");
			invoiceHeader.SupplierPickupDeliveryAddress.E2_CompanyName = "1234567890123456789012345678901234567890123456789012345678901234567890";
			AssertHasWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_CompanyNameInfo, "exceeds the maximum allowed");

			invoiceHeader.SupplierPickupDeliveryAddress.E2_Address1 = "123456789012345678901234567890123456";
			invoiceHeader.SupplierPickupDeliveryAddress.E2_Address2 = "123456789012345678901234567890123456";
			AssertHasWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_Address1Info, "exceeds the maximum allowed");
			AssertHasWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_Address2Info, "exceeds the maximum allowed");
			invoiceHeader.SupplierPickupDeliveryAddress.E2_Address1 = "12345678901234567890123456789012345";
			invoiceHeader.SupplierPickupDeliveryAddress.E2_Address2 = "12345678901234567890123456789012345";
			AssertNoWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_Address1Info, "exceeds the maximum allowed");
			AssertNoWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_Address2Info, "exceeds the maximum allowed");

			AssertHasMessageErrorContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_CityInfo, "You have not entered a city");
			invoiceHeader.SupplierPickupDeliveryAddress.E2_City = "1234567890abcdefghijklmnopqrstuvwxyz";
			AssertNoMessageErrorContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_CityInfo, "You have not entered a city");
			AssertHasWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_CityInfo, "exceeds the maximum allowed");
			invoiceHeader.SupplierPickupDeliveryAddress.E2_City = "123456789";
			AssertNoWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_CityInfo, "exceeds the maximum allowed");

			invoiceHeader.SupplierPickupDeliveryAddress.E2_RN_NKCountryCode = "AU";
			AssertNoMessageErrorContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_StateInfo, "A province/state is required when the country/region is Canada or United States");
			AssertNoMessageErrorContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_PostcodeInfo, "A postal/zip code is required when the country/region is Canada or United States");
			invoiceHeader.SupplierPickupDeliveryAddress.E2_RN_NKCountryCode = "CA";

			AssertHasMessageErrorContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_StateInfo, "A province/state is required when the country/region is Canada or United States");
			invoiceHeader.SupplierPickupDeliveryAddress.E2_State = "1234567890";
			AssertNoMessageErrorContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_StateInfo, "A province/state is required when the country/region is Canada or United States");
			AssertHasWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_StateInfo, "exceeds the maximum allowed");
			invoiceHeader.SupplierPickupDeliveryAddress.E2_State = "123456789";
			AssertNoWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_StateInfo, "exceeds the maximum allowed");

			AssertHasMessageErrorContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_PostcodeInfo, "A postal/zip code is required when the country/region is Canada or United States");
			invoiceHeader.SupplierPickupDeliveryAddress.E2_Postcode = "1234567890";
			AssertNoMessageErrorContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_PostcodeInfo, "A postal/zip code is required when the country/region is Canada or United States");
			AssertHasWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_PostcodeInfo, "exceeds the maximum allowed");
			invoiceHeader.SupplierPickupDeliveryAddress.E2_Postcode = "123456789";
			AssertNoWarningContaining(invoiceHeader.SupplierPickupDeliveryAddress.E2_PostcodeInfo, "exceeds the maximum allowed");
		}

		public void TestCheckFinalConsigneeAddress()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var consigneeAddress = invoiceHeader.FinalConsigneeAddress;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			consigneeAddress.Validation.ValidateAll();

			ValidationTestHelper.AssertAddressUsesCAAddressValidationIfOrgSpecified(consigneeAddress, "Consignee");

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			consigneeAddress.OrganisationPK = consignee.PK;
			consigneeAddress.E2_OA_Address = consignee.MainAddress.PK;
			consigneeAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(consigneeAddress.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			var contact = consignee.Contacts.AddNew();
			var allocatedContact = contact.Allocations.AddNew();
			allocatedContact.PC_Type = "CAP";
			consigneeAddress.Validation.ValidateAll();
			AssertNoMessageError(consigneeAddress.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);
			AssertHasMessageError(consigneeAddress.OrganisationPKInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			contact.OC_Phone = "12233333";
			consigneeAddress.Validation.ValidateAll();
			AssertNoMessageError(consigneeAddress.OrganisationPKInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			consignee.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			consigneeAddress.Validation.ValidateAll();
			AssertHasWarning(consigneeAddress.E2_OA_AddressInfo, ImportJobComInvoiceLineValidation.ConsigneeAddrIsNotCA);
			consignee.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			consigneeAddress.Validation.ValidateAll();
			AssertNoWarning(consigneeAddress.E2_OA_AddressInfo, ImportJobComInvoiceLineValidation.ConsigneeAddrIsNotCA);
		}

		public void TestCheckExporterDocumentaryAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var exporterDocumentaryAddress = invoiceHeader.ExporterDocumentaryAddress;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			exporterDocumentaryAddress.Validation.ValidateAll();

			ValidationTestHelper.AssertAddressUsesCAAddressValidationIfOrgSpecified(exporterDocumentaryAddress, "Exporter");

			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			var address = exporter.MainAddress;
			address.OA_Address1 = "Address line 1";
			address.OA_RN_NKCountryCode = "CA";

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			exporterDocumentaryAddress.E2_OA_Address = address.PK;
			exporterDocumentaryAddress.Validation.ValidateAll();
			AssertHasMessageError(exporterDocumentaryAddress.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			var contact = exporter.Contacts.AddNew();
			var allocatedContact = contact.Allocations.AddNew();
			allocatedContact.PC_Type = "CAP";
			exporterDocumentaryAddress.Validation.ValidateAll();
			AssertNoMessageError(exporterDocumentaryAddress.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);
			AssertHasMessageError(exporterDocumentaryAddress.OrganisationPKInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			contact.OC_Phone = "1333333";
			exporterDocumentaryAddress.Validation.ValidateAll();
			AssertNoMessageError(exporterDocumentaryAddress.OrganisationPKInfo, OrganisationValidation.ContactPhoneNumberIsRequired);
		}

		public void TestCheckExporterDocumentaryAddressForPGA()
		{
			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			var address = exporter.MainAddress;
			address.OA_Address1 = "Address line 1";
			address.OA_RN_NKCountryCode = "CA";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ServiceOptions.Codes.IID;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CNSCInd = YesNoList.Codes.Yes;

			invoiceHeader.ExporterDocumentaryAddress.Validation.ValidateAll();

			ValidationTestHelper.AssertAddressUsesCAAddressValidationIfOrgSpecified(invoiceHeader.ExporterDocumentaryAddress, "Exporter");
		}

		public void TestCheckBuyerDocumentaryAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var buyerDocumentaryAddress = invoiceHeader.BuyerDocumentaryAddress;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			buyerDocumentaryAddress.Validation.ValidateAll();

			ValidationTestHelper.AssertAddressUsesCAAddressValidationIfOrgSpecified(buyerDocumentaryAddress, "Purchaser");

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			buyerDocumentaryAddress.E2_OA_Address = buyer.MainAddress.PK;
			buyerDocumentaryAddress.Validation.ValidateAll();
			AssertHasMessageError(buyerDocumentaryAddress.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);

			var contact = buyer.Contacts.AddNew();
			var allocatedContact = contact.Allocations.AddNew();
			allocatedContact.PC_Type = "CAP";

			buyerDocumentaryAddress.Validation.ValidateAll();
			AssertNoMessageError(buyerDocumentaryAddress.E2_OA_AddressInfo, OrganisationValidation.AddressWorkPhoneIsRequired);
			AssertHasMessageError(buyerDocumentaryAddress.OrganisationPKInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			contact.OC_Phone = "12345667889";
			buyerDocumentaryAddress.Validation.ValidateAll();
			AssertNoMessageError(buyerDocumentaryAddress.OrganisationPKInfo, OrganisationValidation.ContactPhoneNumberIsRequired);
		}

		public void TestCheckOrganisationPKForECCCPGA()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.CA_ServiceOption = "IID";
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = YesNoList.Codes.Yes;
			invoiceLine.ECCCPGAHeader.CA_WENProgramInd = YesNoList.Codes.Yes;

			var supplierDocAddresss = invoiceHeader.SupplierDocumentaryAddress;
			supplierDocAddresss.Validation.ValidateAll();
			AssertHasMessageErrorContaining(supplierDocAddresss.OrganisationPKInfo, "You have not entered a Vendor");
			supplierDocAddresss.OrganisationPK = supplier.PK;
			supplier.Addresses.MainAddress.OA_RL_NKRelatedPortCode = "CNSHA";
			invoiceHeader.ExporterDocumentaryAddress.OrganisationPK = supplier.PK;
			var contact = supplier.Contacts.AddNew();
			contact.OC_ContactName = "test";
			contact.OC_Title = "sales manager";
			var allocatedContact = contact.Allocations.AddNew();
			allocatedContact.PC_Type = "CAP";

			supplierDocAddresss.Validation.ValidateAll();
			AssertNoMessageErrorContaining(supplierDocAddresss.OrganisationPKInfo, "You have not entered a Vendor");
			var errorMessage = string.Format(OrganisationValidation.CAPAllocationIsRequired, PGACodes.Descriptions.ECCC);
			AssertNoMessageErrorContaining(supplierDocAddresss.OrganisationPKInfo, errorMessage);
			allocatedContact.PC_Type = "USP";
			supplierDocAddresss.Validation.ValidateAll();
			AssertHasMessageErrorContaining(supplierDocAddresss.OrganisationPKInfo, errorMessage);

			var export = Factory.NewWithValidTestData<OrgHeader>();
			var exportDocAddresss = invoiceHeader.ExporterDocumentaryAddress;
			exportDocAddresss.OrganisationPK = export.PK;
			export.Addresses.MainAddress.OA_RL_NKRelatedPortCode = "CNEXP";
			invoiceHeader.JZ_OH_Supplier = export.PK;
			var contact2 = export.Contacts.AddNew();
			contact2.OC_ContactName = "test";
			contact2.OC_Title = "sales manager";
			var allocatedContact2 = contact2.Allocations.AddNew();
			allocatedContact2.PC_Type = "CAP";

			exportDocAddresss.Validation.ValidateAll();
			AssertNoMessageErrorContaining(exportDocAddresss.OrganisationPKInfo, OrganisationValidation.CAPAllocationIsRequired.Split(':')[0]);
			allocatedContact2.PC_Type = "USP";
			exportDocAddresss.Validation.ValidateAll();
			AssertHasMessageErrorContaining(exportDocAddresss.OrganisationPKInfo, OrganisationValidation.CAPAllocationIsRequired.Split(':')[0]);
		}

		public void TestCheckOrganisationPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "123ABC";
			orgHeader.MainAddress.Address1 = "123ABC";

			invoiceHeader.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			invoiceHeader.FinalConsigneeAddress.OrganisationPK = orgHeader.PK;
			invoiceHeader.ExporterDocumentaryAddress.OrganisationPK = orgHeader.PK;
			invoiceHeader.BuyerDocumentaryAddress.OrganisationPK = orgHeader.PK;

			invoiceHeader.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			invoiceHeader.FinalConsigneeAddress.Validation.ValidateOrganisationPK();
			invoiceHeader.ExporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			invoiceHeader.BuyerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertNoWarning(invoiceHeader.SupplierDocumentaryAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(invoiceHeader.SupplierDocumentaryAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(invoiceHeader.FinalConsigneeAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(invoiceHeader.FinalConsigneeAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(invoiceHeader.ExporterDocumentaryAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(invoiceHeader.ExporterDocumentaryAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(invoiceHeader.BuyerDocumentaryAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(invoiceHeader.BuyerDocumentaryAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			orgHeader.OH_FullName = "123ABC– ";
			orgHeader.MainAddress.Address1 = "123ABC– ";
			invoiceHeader.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			invoiceHeader.FinalConsigneeAddress.Validation.ValidateOrganisationPK();
			invoiceHeader.ExporterDocumentaryAddress.Validation.ValidateOrganisationPK();
			invoiceHeader.BuyerDocumentaryAddress.Validation.ValidateOrganisationPK();
			AssertHasWarning(invoiceHeader.SupplierDocumentaryAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(invoiceHeader.SupplierDocumentaryAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(invoiceHeader.FinalConsigneeAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(invoiceHeader.FinalConsigneeAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(invoiceHeader.ExporterDocumentaryAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(invoiceHeader.ExporterDocumentaryAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(invoiceHeader.BuyerDocumentaryAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(invoiceHeader.BuyerDocumentaryAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		}
	}
}
