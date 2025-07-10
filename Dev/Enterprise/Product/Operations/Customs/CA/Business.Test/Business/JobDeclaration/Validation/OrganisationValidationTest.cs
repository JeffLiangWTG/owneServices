using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class OrganisationValidationTest : TestCaseWithFactory
	{
		public void TestValidateCAPContactPhoneAndEmailRecommened()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = "IID";
			var invoiceHeader = declaration.Invoices.AddNew();
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "123ABC";
			importer.MainAddress.Address1 = "123ABC";
			importer.MainAddress.City = "AUSYD";

			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.CA_HCInd = "Y";
			var hcHeader = invoiceLine.HCPGAHeader;
			hcHeader.CA_CPRProgramInd = "Y";

			declaration.JE_OH_Importer = importer.PK;
			invoiceHeader.ManufacturerOrgPK = importer.PK;
			var contact1 = importer.Contacts.AddNew();
			contact1.OC_ContactName = "test";
			contact1.OC_Title = "developer";
			var allocatedContact1 = contact1.Allocations.AddNew();
			allocatedContact1.PC_Type = "CAP";
			invoiceHeader.ManufacturerOrgPK = importer.PK;
			invoiceHeader.Validation.ValidateJZ_OA_ManufacturerAddress();
			AssertHasWarningContaining(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.ContactPhoneNumberIsRecommended);
			AssertHasWarningContaining(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.ContactEmailIsRecommended);

			contact1.OC_Phone = "1234567";
			contact1.OC_Email = "test@mail.com";
			invoiceHeader.Validation.ValidateJZ_OA_ManufacturerAddress();
			OrganisationValidation.ValidateCAPContactOrAddressPhoneAndEmailRecommened(invoiceHeader.JZ_OA_ManufacturerAddressInfo, invoiceHeader.ManufacturerAddress);
			AssertNoWarningContaining(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.ContactPhoneNumberIsRecommended);
			AssertNoWarningContaining(invoiceHeader.JZ_OA_ManufacturerAddressInfo, OrganisationValidation.ContactEmailIsRecommended);
		}

		public void TestValidateNamesAndAddressesSpecial()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "123ABC";
			importer.MainAddress.Address1 = "123ABC";

			declaration.JE_OH_Importer = importer.PK;
			invoiceHeader.ManufacturerOrgPK = importer.PK;
			AssertNoWarning(declaration.JE_OH_ImporterInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(declaration.JE_OH_ImporterInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(invoiceHeader.JZ_OA_ManufacturerAddressInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertNoWarning(invoiceHeader.JZ_OA_ManufacturerAddressInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			importer.OH_FullName = "123ABC– ";
			importer.MainAddress.Address1 = "123ABC– ";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasWarning(declaration.JE_OH_ImporterInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(declaration.JE_OH_ImporterInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			invoiceHeader.Validation.ValidateJZ_OA_ManufacturerAddress();
			AssertHasWarning(invoiceHeader.JZ_OA_ManufacturerAddressInfo, "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
			AssertHasWarning(invoiceHeader.JZ_OA_ManufacturerAddressInfo, "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			importer.Delete();
			AssertNoExceptionThrown("no exception when importer deleted", () => declaration.Validation.ValidateJE_OH_Importer());
		}

		public void TestValidateCAPAllocatedContact()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CA_ServiceOption = "IID";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			var invoiceLine2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.CA_HCInd = "Y";
			var hcHeader = invoiceLine1.HCPGAHeader;

			var importer = Factory.New<OrgHeader>();

			var contact1 = importer.Contacts.AddNew();
			contact1.OC_ContactName = "amy.xiang";
			contact1.OC_Title = "developer";

			var allocatedContact1 = contact1.Allocations.AddNew();
			allocatedContact1.PC_Type = "NZB";

			var contact2 = importer.Contacts.AddNew();
			contact2.OC_ContactName = "kevin";
			contact2.OC_Title = "sales manager";

			var allocatedContact2 = contact2.Allocations.AddNew();
			allocatedContact2.PC_Type = "NZC";

			declaration.JE_OH_Importer = importer.PK;
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, string.Format(OrganisationValidation.CAPAllocationIsRequired, PGACodes.Descriptions.HC));

			var allocatedContact3 = contact2.Allocations.AddNew();
			allocatedContact3.PC_Type = "CAP";

			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, string.Format(OrganisationValidation.CAPAllocationIsRequired, PGACodes.Descriptions.HC));

			AssertHasMessageError(declaration.JE_OH_ImporterInfo, OrganisationValidation.ContactEmailIsRequired);
			contact2.OC_Email = "jason.zhu@wisetechglobal.com";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, OrganisationValidation.ContactEmailIsRequired);

			AssertHasMessageError(declaration.JE_OH_ImporterInfo, OrganisationValidation.ContactPhoneNumberIsRequired);
			contact2.OC_Phone = "+1 888-913-4581";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			contact2.OC_Phone = "";
			contact2.OC_Mobile = "+1 888-913-4582";
			declaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(declaration.JE_OH_ImporterInfo, OrganisationValidation.ContactPhoneNumberIsRequired);

			contact2.OC_IsActive = false;
			declaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(declaration.JE_OH_ImporterInfo, string.Format(OrganisationValidation.CAPAllocationIsRequired, PGACodes.Descriptions.HC));
		}
	}
}
