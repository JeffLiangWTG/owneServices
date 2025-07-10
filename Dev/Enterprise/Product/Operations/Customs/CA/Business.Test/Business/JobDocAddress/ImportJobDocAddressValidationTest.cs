using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ImportJobDocAddressValidationTest : ImportJobDeclarationValidationTest
	{
		public void TestCFIAdeliveryWithoutContact()
		{
			var importer = Factory.New<OrgHeader>();
			OrgAddress address = importer.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_Address1 = "Address";
			address.OA_City = "City";
			address.OA_CompanyNameOverride = "Company";
			declaration.ImporterDeliveryAddress.OrganisationPK = importer.PK;
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Contact();
			AssertNoMessageErrors(declaration.ImporterDeliveryAddress.OrganisationPKInfo);

			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.CA_OGDCFIA = true;
			declaration.ImporterDeliveryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(declaration.ImporterDeliveryAddress.OrganisationPKInfo, ImportJobDocAddressValidation.cFIAPhoneAndFaxMessage);
			address.OA_Phone = "123421234";
			address.OA_Fax = "123421235";
			declaration.ImporterDeliveryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.ImporterDeliveryAddress.OrganisationPKInfo, ImportJobDocAddressValidation.cFIAPhoneAndFaxMessage);

			address.OA_Phone = ZString.Empty;
			declaration.ImporterDeliveryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(declaration.ImporterDeliveryAddress.OrganisationPKInfo, ImportJobDocAddressValidation.cFIAPhoneAndFaxMessage);

			address.OA_Phone = "123421234";
			address.OA_Fax = ZString.Empty;
			declaration.ImporterDeliveryAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(declaration.ImporterDeliveryAddress.OrganisationPKInfo, ImportJobDocAddressValidation.cFIAPhoneAndFaxMessage);

			OrgContact contact = importer.ContactsActive.AddNew();
			contact.OC_ContactName = "Name";
			declaration.ImporterDeliveryAddress.ContactPK = contact.PK;
			declaration.ImporterDeliveryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.ImporterDeliveryAddress.OrganisationPKInfo, ImportJobDocAddressValidation.cFIAPhoneAndFaxMessage);
		}

		public void TestCheckE2_Contact()
		{
			const string messageError = "Delivery contact with Phone and Fax is required for CFIA shipments.";
			var importer = Factory.New<OrgHeader>();
			OrgAddress address = importer.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_Address1 = "Address";
			address.OA_City = "City";
			address.OA_CompanyNameOverride = "Company";
			declaration.ImporterDeliveryAddress.OrganisationPK = importer.PK;
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Contact();
			AssertNoMessageErrors(declaration.ImporterDeliveryAddress.OrganisationPKInfo);
			AssertNoMessageErrors(declaration.ImporterDeliveryAddress.E2_ContactInfo);

			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.CA_OGDCFIA = true;
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Contact();
			AssertHasMessageError(declaration.ImporterDeliveryAddress.E2_ContactInfo, messageError);

			OrgContact contact = importer.ContactsActive.AddNew();
			contact.OC_ContactName = "Name";
			declaration.ImporterDeliveryAddress.ContactPK = contact.PK;
			AssertHasMessageError(declaration.ImporterDeliveryAddress.E2_ContactInfo, messageError);

			contact.OC_Fax = string.Empty;
			contact.OC_Phone = "123421234";
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Contact();
			AssertHasMessageError(declaration.ImporterDeliveryAddress.E2_ContactInfo, messageError);

			contact.OC_Fax = "123421234";
			contact.OC_Phone = string.Empty;
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Contact();
			AssertHasMessageError(declaration.ImporterDeliveryAddress.E2_ContactInfo, messageError);

			contact.OC_Fax = "123421234";
			contact.OC_Phone = "123421234";
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Contact();
			AssertNoMessageError(declaration.ImporterDeliveryAddress.E2_ContactInfo, messageError);

			address.OA_City = string.Empty;
			contact.OC_Phone = string.Empty;
			declaration.ImporterDeliveryAddress.Validation.ValidateOrganisationPK();
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Contact();
			AssertHasMessageErrors(declaration.ImporterDeliveryAddress.OrganisationPKInfo);
			AssertNoMessageErrors(declaration.ImporterDeliveryAddress.E2_ContactInfo);
		}

		public void TestCheckE2_ContactOverridden()
		{
			const string messageError = "Delivery contact with Phone and Fax is required for CFIA shipments.";

			declaration.ImporterDeliveryAddress.E2_AddressOverride = true;
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Contact();
			AssertNoMessageErrors(declaration.ImporterDeliveryAddress.E2_ContactInfo);

			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.CA_OGDCFIA = true;
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Contact();
			AssertHasMessageError(declaration.ImporterDeliveryAddress.E2_ContactInfo, messageError);

			declaration.ImporterDeliveryAddress.E2_Contact = "Contact";
			AssertNoMessageError(declaration.ImporterDeliveryAddress.E2_ContactInfo, messageError);
		}

		public void TestCheckE2_Phone()
		{
			const string messageError = "Phone number is required for CFIA shipments.";
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Phone();
			AssertNoMessageErrors(declaration.ImporterDeliveryAddress.E2_PhoneInfo);

			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.CA_OGDCFIA = true;
			declaration.ImporterDeliveryAddress.E2_AddressOverride = true;
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Phone();
			AssertHasMessageError(declaration.ImporterDeliveryAddress.E2_PhoneInfo, messageError);

			declaration.ImporterDeliveryAddress.E2_Phone = "1234235";
			AssertNoMessageError(declaration.ImporterDeliveryAddress.E2_PhoneInfo, messageError);

			declaration.ImporterDeliveryAddress.E2_AddressOverride = false;
			declaration.ImporterDeliveryAddress.E2_Phone = string.Empty;
			AssertNoMessageError(declaration.ImporterDeliveryAddress.E2_PhoneInfo, messageError);
		}

		public void TestCheckE2_Fax()
		{
			const string messageError = "Fax number is required for CFIA shipments.";
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Fax();
			AssertNoMessageErrors(declaration.ImporterDeliveryAddress.E2_FaxInfo);

			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.CA_OGDCFIA = true;
			declaration.ImporterDeliveryAddress.E2_AddressOverride = true;
			declaration.ImporterDeliveryAddress.Validation.ValidateE2_Fax();
			AssertHasMessageError(declaration.ImporterDeliveryAddress.E2_FaxInfo, messageError);

			declaration.ImporterDeliveryAddress.E2_Fax = "1234235";
			AssertNoMessageError(declaration.ImporterDeliveryAddress.E2_FaxInfo, messageError);

			declaration.ImporterDeliveryAddress.E2_AddressOverride = false;
			declaration.ImporterDeliveryAddress.E2_Fax = string.Empty;
			AssertNoMessageError(declaration.ImporterDeliveryAddress.E2_FaxInfo, messageError);
		}

		public void TestCheckOrganisationPK_WarehouseDocAddress()
		{
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.WarehouseDocAddress.OrganisationPKInfo);

			const string messageError = "CBSA Warehouse Code (CPW) must be specified on Organization Registration Numbers tab.";
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			declaration.WarehouseDocAddress.OrganisationPK = org.PK;
			AssertHasMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, messageError);

			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "1234", Factory.Load<RefCountry>(Constants.CountryGuids.Canada));
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, messageError);
		}

		public void TestCheckOrganisationPK_WarehouseDocAddressForCAD()
		{
			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.WarehouseDocAddress.OrganisationPKInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Confirming;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.WarehouseDocAddress.OrganisationPKInfo);

			const string messageError = "CBSA Warehouse Code (CPW) must be specified on Organization Registration Numbers tab.";
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			declaration.WarehouseDocAddress.OrganisationPK = org.PK;
			AssertHasMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, messageError);

			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "1234", Factory.Load<RefCountry>(Constants.CountryGuids.Canada));
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, messageError);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			org.CustomsCodes.RemoveAndDeleteAll();
			declaration.WarehouseDocAddress.OrganisationPK = ZGuid.Empty;
			declaration.WarehouseDocAddress.OrganisationPK = org.PK;
			AssertHasMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, messageError);

			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "1234", Factory.Load<RefCountry>(Constants.CountryGuids.Canada));
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(declaration.WarehouseDocAddress.OrganisationPKInfo, messageError);
		}

		public void TestCheckOrganisationPK_ImporterDeliveryAddress()
		{
			const string capiton = "Importer Delivery";
			string importerDocAddressNotConfiguredMessageError = CAAddressValidator.GetAddressNotConfiguredMessageError(capiton);
			string importerDocAddressIsInvalidMessageError = CAAddressValidator.GetAddressIsInvalidMessageError(capiton);
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			OrgAddress address = importer.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_RN_NKCountryCode = "AU";

			declaration.ImporterDeliveryAddress.OrganisationPK = importer.PK;
			AssertNoMessageError(declaration.ImporterDeliveryAddress.OrganisationPKInfo, importerDocAddressNotConfiguredMessageError);
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.OrganisationPKInfo, importerDocAddressIsInvalidMessageError);

			declaration.CA_ServiceOption = ServiceOptions.Codes.PARSOGD;
			declaration.CA_OGDCFIA = true;
			declaration.ImporterDeliveryAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.ImporterDeliveryAddress.OrganisationPKInfo, importerDocAddressNotConfiguredMessageError);
			AssertHasMessageErrorContaining(declaration.ImporterDeliveryAddress.OrganisationPKInfo, importerDocAddressIsInvalidMessageError);

			declaration.ImporterDeliveryAddress.OrganisationPK = ZGuid.Empty;
			AssertHasMessageError(declaration.ImporterDeliveryAddress.OrganisationPKInfo, importerDocAddressNotConfiguredMessageError);
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.OrganisationPKInfo, importerDocAddressIsInvalidMessageError);

			address.OA_Address1 = "Address";
			address.OA_City = "City";
			address.OA_CompanyNameOverride = "Company";
			declaration.ImporterDeliveryAddress.OrganisationPK = importer.PK;
			AssertNoMessageError(declaration.ImporterDeliveryAddress.OrganisationPKInfo, importerDocAddressNotConfiguredMessageError);
			AssertNoMessageErrorContaining(declaration.ImporterDeliveryAddress.OrganisationPKInfo, importerDocAddressIsInvalidMessageError);
		}

		public void TestCheckOrganisationPK_ImporterOfRecord()
		{
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = Customs.Business.YesNoList.Codes.Yes;
			var importerOfRecord = Factory.New<OrgHeader>();
			declaration.ImporterOfRecordAddress.OrganisationPK = importerOfRecord.PK;
			AssertHasMessageErrorContaining(declaration.ImporterOfRecordAddress.OrganisationPKInfo, OrgImpAddInfo.CFIAPaymentMethodNotSpecifiedErrorText);
			OrgImpAddInfo.Get(importerOfRecord).ZO_CFIAFeePaymentMethod = CFIAPaymentMethods.Codes.Importer;
			declaration.ImporterOfRecordAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(declaration.ImporterOfRecordAddress.OrganisationPKInfo, OrgImpAddInfo.CFIAPaymentMethodNotSpecifiedErrorText);

			AssertHasMessageErrorContaining(declaration.ImporterOfRecordAddress.OrganisationPKInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnImporterErrorText);
			importerOfRecord.SetCustomsCode(OrgCusCode.CACodeTypes.CFIAAccountNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "12345");
			declaration.ImporterOfRecordAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining(declaration.ImporterOfRecordAddress.OrganisationPKInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnImporterErrorText);

			var proxy = Factory.New<OrgHeader>();
			declaration.Branch.GB_OH_OrgProxy = proxy.PK;
			OrgImpAddInfo.Get(importerOfRecord).ZO_CFIAFeePaymentMethod = CFIAPaymentMethods.Codes.Broker;
			declaration.ImporterOfRecordAddress.Validation.ValidateOrganisationPK();
			AssertHasWarning(declaration.ImporterOfRecordAddress.OrganisationPKInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);
			proxy.SetCustomsCode(OrgCusCode.CACodeTypes.CFIAAccountNumber, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Canada), "23456");
			declaration.ImporterOfRecordAddress.Validation.ValidateOrganisationPK();
			AssertNoWarning(declaration.ImporterOfRecordAddress.OrganisationPKInfo, OrgImpAddInfo.CFIAAccountNumberNotSpecifiedOnOrgProxyErrorText);
		}

		public void TestCheckOrganisationPK_ImporterOfRecord_ValidateBondType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var bondCollection = new CusBondDetailCollection(org);
			var bondData1 = bondCollection.AddNew();
			bondData1.PW_BondType = BondTypeList.Codes.NotOnPortal;
			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData1.PW_BondNumber = "00001";
			bondData1.PW_SuretyCode = "001";
			var bondData2 = bondCollection.AddNew();
			bondData2.PW_BondType = BondTypeList.Codes.OnPortal;
			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			bondData2.PW_BondNumber = "00002";
			bondData2.PW_SuretyCode = "002";

			var bondNotOnPortal = ImportAddInfoJobDeclarationValidation.ImporterIsNotOnCARMPortal;
			var bondWillBRequired = ImportAddInfoJobDeclarationValidation.ClinetIsInCARMPortalButNoBondOnFile;

			declaration.ImporterOfRecordAddress.OrganisationPK = org.PK;
			declaration.ImporterOfRecordAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, bondNotOnPortal);
			AssertHasWarning(declaration.ImporterOfRecordAddress.OrganisationPKInfo, bondWillBRequired);

			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(10);
			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(10);
			Factory.Save();

			declaration.ImporterOfRecordAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, bondNotOnPortal);
			AssertNoWarning(declaration.ImporterOfRecordAddress.OrganisationPKInfo, bondWillBRequired);

			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declarationLoaded = newFactory.Load<JobDeclaration>(declaration.PK);
			declarationLoaded.ImporterOfRecordAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declarationLoaded.ImporterOfRecordAddress.OrganisationPKInfo, bondNotOnPortal);
			AssertHasWarning(declarationLoaded.ImporterOfRecordAddress.OrganisationPKInfo, bondWillBRequired);

			bondData2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(10);
			Factory.Save();

			declarationLoaded.ImporterOfRecordAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError(declarationLoaded.ImporterOfRecordAddress.OrganisationPKInfo, bondNotOnPortal);
			AssertNoWarning(declarationLoaded.ImporterOfRecordAddress.OrganisationPKInfo, bondWillBRequired);

			bondData1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-10);
			Factory.Save();
			declarationLoaded.ImporterOfRecordAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageError(declarationLoaded.ImporterOfRecordAddress.OrganisationPKInfo, bondNotOnPortal);
			AssertNoWarning(declarationLoaded.ImporterOfRecordAddress.OrganisationPKInfo, bondWillBRequired);
		}

		public void TestCheckOrganisationPK()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.SupplierDocumentaryAddress.OrganisationPKInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.SupplierDocumentaryAddress.OrganisationPKInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.SupplierDocumentaryAddress.OrganisationPKInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "123ABC";
			orgHeader.MainAddress.Address1 = "123ABC";

			declaration.WarehouseDocAddress.OrganisationPK = orgHeader.PK;
			declaration.SupplierDocumentaryAddress.OrganisationPK = orgHeader.PK;
			declaration.ImporterOfRecordAddress.OrganisationPK = orgHeader.PK;
			declaration.CommercialInvoiceOriginator.OrganisationPK = orgHeader.PK;

			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			declaration.ImporterOfRecordAddress.Validation.ValidateOrganisationPK();
			declaration.CommercialInvoiceOriginator.Validation.ValidateOrganisationPK();
			AssertNoWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(declaration.ImporterOfRecordAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(declaration.ImporterOfRecordAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(declaration.CommercialInvoiceOriginator.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(declaration.CommercialInvoiceOriginator.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			orgHeader.OH_FullName = "123ABC– ";
			orgHeader.MainAddress.Address1 = "123ABC– ";
			declaration.WarehouseDocAddress.Validation.ValidateOrganisationPK();
			declaration.SupplierDocumentaryAddress.Validation.ValidateOrganisationPK();
			declaration.ImporterOfRecordAddress.Validation.ValidateOrganisationPK();
			declaration.CommercialInvoiceOriginator.Validation.ValidateOrganisationPK();
			AssertHasWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(declaration.WarehouseDocAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(declaration.SupplierDocumentaryAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(declaration.ImporterOfRecordAddress.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(declaration.ImporterOfRecordAddress.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(declaration.CommercialInvoiceOriginator.OrganisationPKInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(declaration.CommercialInvoiceOriginator.OrganisationPKInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		}

		public void TestValidateSupplierDocumentAddressWhenAddressOverriden()
		{
			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			declaration.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			declaration.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertHasMessageErrorContaining(declaration.SupplierDocumentaryAddress.E2_CompanyNameInfo, "You have not entered a name");
			declaration.SupplierDocumentaryAddress.E2_CompanyName = "123456789012345678901234567890123456";
			AssertNoMessageErrorContaining(declaration.SupplierDocumentaryAddress.E2_CompanyNameInfo, "You have not entered a name");
			AssertHasWarningContaining(declaration.SupplierDocumentaryAddress.E2_CompanyNameInfo, "exceeds the maximum allowed");
			declaration.SupplierDocumentaryAddress.E2_CompanyName = "12345678901234567890123456789012345";
			AssertNoWarningContaining(declaration.SupplierDocumentaryAddress.E2_CompanyNameInfo, "exceeds the maximum allowed");

			declaration.SupplierDocumentaryAddress.E2_Address1 = "123456789012345678901234567890123456";
			declaration.SupplierDocumentaryAddress.E2_Address2 = "123456789012345678901234567890123456";
			AssertHasWarningContaining(declaration.SupplierDocumentaryAddress.E2_Address1Info, "exceeds the maximum allowed");
			AssertHasWarningContaining(declaration.SupplierDocumentaryAddress.E2_Address2Info, "exceeds the maximum allowed");
			declaration.SupplierDocumentaryAddress.E2_Address1 = "12345678901234567890123456789012345";
			declaration.SupplierDocumentaryAddress.E2_Address2 = "12345678901234567890123456789012345";
			AssertNoWarningContaining(declaration.SupplierDocumentaryAddress.E2_Address1Info, "exceeds the maximum allowed");
			AssertNoWarningContaining(declaration.SupplierDocumentaryAddress.E2_Address2Info, "exceeds the maximum allowed");

			AssertHasMessageErrorContaining(declaration.SupplierDocumentaryAddress.E2_CityInfo, "You have not entered a city");
			declaration.SupplierDocumentaryAddress.E2_City = "1234567890abcdefghijklmnopqrstuvwxyz";
			AssertNoMessageErrorContaining(declaration.SupplierDocumentaryAddress.E2_CityInfo, "You have not entered a city");
			AssertHasWarningContaining(declaration.SupplierDocumentaryAddress.E2_CityInfo, "exceeds the maximum allowed");
			declaration.SupplierDocumentaryAddress.E2_City = "123456789";
			AssertNoWarningContaining(declaration.SupplierDocumentaryAddress.E2_CityInfo, "exceeds the maximum allowed");

			AssertNoMessageErrorContaining(declaration.SupplierDocumentaryAddress.E2_StateInfo, "A province/state is required when the country/region is Canada or United States");
			AssertNoMessageErrorContaining(declaration.SupplierDocumentaryAddress.E2_PostcodeInfo, "A postal/zip code is required when the country/region is Canada or United States");
			declaration.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "CA";

			AssertHasMessageErrorContaining(declaration.SupplierDocumentaryAddress.E2_StateInfo, "A province/state is required when the country/region is Canada or United States");
			declaration.SupplierDocumentaryAddress.E2_State = "1234567890";
			AssertNoMessageErrorContaining(declaration.SupplierDocumentaryAddress.E2_StateInfo, "A province/state is required when the country/region is Canada or United States");
			AssertHasWarningContaining(declaration.SupplierDocumentaryAddress.E2_StateInfo, "exceeds the maximum allowed");
			declaration.SupplierDocumentaryAddress.E2_State = "123456789";
			AssertNoWarningContaining(declaration.SupplierDocumentaryAddress.E2_StateInfo, "exceeds the maximum allowed");

			AssertHasMessageErrorContaining(declaration.SupplierDocumentaryAddress.E2_PostcodeInfo, "A postal/zip code is required when the country/region is Canada or United States");
			declaration.SupplierDocumentaryAddress.E2_Postcode = "1234567890";
			AssertNoMessageErrorContaining(declaration.SupplierDocumentaryAddress.E2_PostcodeInfo, "A postal/zip code is required when the country/region is Canada or United States");
			AssertHasWarningContaining(declaration.SupplierDocumentaryAddress.E2_PostcodeInfo, "exceeds the maximum allowed");
			declaration.SupplierDocumentaryAddress.E2_Postcode = "123456789";
			AssertNoWarningContaining(declaration.SupplierDocumentaryAddress.E2_PostcodeInfo, "exceeds the maximum allowed");
		}

		public void TestValidateImporterOfRecordInCanadian()
		{
			const string msgError = "The Importer of Record Address is not in Canada, and you have not specified a Canadian Purchaser or Consignee on the Invoice Header tab.";
			var importer = Factory.New<OrgHeader>();
			importer.MainAddress.OA_RL_NKRelatedPortCode = "CATOR";
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			var invoice1 = declaration.Invoices.AddNew();
			AssertNoMessageError(declaration.ImporterOfRecordAddress.E2_OA_AddressInfo, msgError);
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertHasMessageError(declaration.ImporterOfRecordAddress.E2_OA_AddressInfo, msgError);
			var buyer = Factory.New<OrgHeader>();
			buyer.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			invoice1.JZ_OH_Buyer = buyer.PK;
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertHasMessageError(declaration.ImporterOfRecordAddress.E2_OA_AddressInfo, msgError);
			buyer.MainAddress.OA_RL_NKRelatedPortCode = "CATOR";
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertNoMessageError(declaration.ImporterOfRecordAddress.E2_OA_AddressInfo, msgError);
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_OH_Buyer = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertHasMessageError(declaration.ImporterOfRecordAddress.E2_OA_AddressInfo, msgError);
			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "CATOR";
			invoice2.JZ_OH_Consignee = consignee.PK;
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertNoMessageError(declaration.ImporterOfRecordAddress.E2_OA_AddressInfo, msgError);
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertHasMessageError(declaration.ImporterOfRecordAddress.E2_OA_AddressInfo, msgError);
		}

		public void TestValidateImporterOfRecord_BusinessNumberForImportExport()
		{
			var messageError = "This Importer of Record does not have a business number for import/export (BRM) or Business Number Importer Commercial (CAI) configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.";
			var importer = Factory.New<OrgHeader>();
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertHasMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
			AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

			var cusCode = importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123456789RMASDF");
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
			AssertHasMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

			cusCode.OK_CustomsRegNo = "123456789RM0001";
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
			AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

			importer.CustomsCodes.RemoveAndDelete(cusCode);
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertHasMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
			AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat);

			cusCode = importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial, "12345678");
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
			AssertHasMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat);

			cusCode.OK_CustomsRegNo = "123456789";
			declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
			AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
			AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat);
		}

		public void TestValidateImporterOfRecord_BusinessNumberForImportExportWhenCADEnabled()
		{
			var messageError = "This Importer of Record does not have a Business Number Importer Commercial (CAI) or business number for import/export (BRM) configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.";
			AssertValidateImporterOfRecordWhenCADEnabled(messageError,
				OrgCusCode.CACodeTypes.BusinessNumberImporterCommercial,
				"123456789",
				CanadianCustomsCodeValidator.Constants.BusinessNumberImporterCommercialFormat,
				false);

			messageError = "This Importer of Record does not have a Business Number Importer Non-Commercial (BNC) or business number for import/export (BRM) configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.";
			AssertValidateImporterOfRecordWhenCADEnabled(messageError,
				OrgCusCode.CACodeTypes.BusinessNumberImporterNonCommercial,
				"123456789RM0002",
				CanadianCustomsCodeValidator.Constants.BusinessNumberImporterNonCommercialFormat,
				true);
		}

		void AssertValidateImporterOfRecordWhenCADEnabled(string messageError, string codeType, string codeValue, string validationFormat, bool isCasual)
		{
			var importer = Factory.New<OrgHeader>();
			var importer1 = Factory.New<OrgHeader>();

			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				if (isCasual)
				{
					var header = declaration.Invoices.AddNew();
					var line = header.JobComInvoiceLines.AddNew();
					line.CA_IsCasualImport = true;
				}

				declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
				AssertHasMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
				AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, validationFormat);

				declaration.JE_OH_Importer = importer.PK;
				AssertHasMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
				AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, validationFormat);

				declaration.JE_OH_Importer = ZGuid.Empty;
				var cusCode = importer.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, "123456789RMASDF");
				declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
				declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
				AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
				AssertHasMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

				cusCode.OK_CustomsRegNo = "123456789RM0001";
				declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
				declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
				AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
				AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, CanadianCustomsCodeValidator.Constants.BusinessNumberForImportExportFormat);

				cusCode = importer.CustomsCodes.AddNew(codeType, "12345678");
				declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
				declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
				AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
				AssertHasMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, validationFormat);

				cusCode.OK_CustomsRegNo = codeValue;
				declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
				declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
				AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
				AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, validationFormat);

				importer.CustomsCodes.RemoveAndDeleteAll();
				importer1.CustomsCodes.AddNew(codeType, codeValue);
				declaration.JE_OH_Importer = importer1.PK;
				declaration.ImporterOfRecordAddress.OrganisationPK = ZGuid.Empty;
				declaration.ImporterOfRecordAddress.OrganisationPK = importer.PK;
				AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, messageError);
				AssertNoMessageError(declaration.ImporterOfRecordAddress.OrganisationPKInfo, validationFormat);
			}
		}

		public void TestCheckE2_State()
		{
			var importer = Factory.New<OrgHeader>();
			var address = importer.MainAddress;
			address.OA_RN_NKCountryCode = "CA";
			address.OA_State = "State";
			declaration.OGDProcessInspectionLPCO.OrganisationPK = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining(declaration.OGDProcessInspectionLPCO.E2_StateInfo, ImportJobDocAddressValidation.StateCodeMustBe2Characters);
			address.OA_State = "ST";
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining(declaration.OGDProcessInspectionLPCO.E2_StateInfo, ImportJobDocAddressValidation.StateCodeMustBe2Characters);
			address.OA_RN_NKCountryCode = "CN";
			address.OA_State = "State";
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining(declaration.OGDProcessInspectionLPCO.E2_StateInfo, ImportJobDocAddressValidation.StateCodeMustBe2Characters);
			address.OA_RN_NKCountryCode = "CA";
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining(declaration.OGDProcessInspectionLPCO.E2_StateInfo, ImportJobDocAddressValidation.StateCodeMustBe2Characters);
		}

		public void TestCheckE2_City()
		{
			var importer = Factory.New<OrgHeader>();
			var address = importer.MainAddress;
			declaration.OGDProcessInspectionLPCO.OrganisationPK = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.Validation.ValidateAll();
			AssertHasMessageErrorContaining(declaration.OGDProcessInspectionLPCO.E2_CityInfo, ImportJobDocAddressValidation.CityNameMustBeEntered);
			address.OA_City = "City";
			declaration.Validation.ValidateAll();
			AssertNoMessageErrorContaining(declaration.OGDProcessInspectionLPCO.E2_CityInfo, ImportJobDocAddressValidation.CityNameMustBeEntered);
		}
	}
}
