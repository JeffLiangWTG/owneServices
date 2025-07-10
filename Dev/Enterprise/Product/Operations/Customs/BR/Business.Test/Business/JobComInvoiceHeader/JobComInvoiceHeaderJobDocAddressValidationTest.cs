using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class JobComInvoiceHeaderJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckOrganisationPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			invoice.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertNoMessageError(invoice.SupplierDocumentaryAddress.OrganisationPKInfo, "You have not entered a Supplier.");
			AssertNoMessageError(invoice.SupplierDocOrgPKInfo, "You have not entered a Supplier.");

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			invoice.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertHasMessageError(invoice.SupplierDocumentaryAddress.OrganisationPKInfo, "You have not entered a Supplier.");
			AssertHasMessageError(invoice.SupplierDocOrgPKInfo, "You have not entered a Supplier.");

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			invoice.SupplierDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNoMessageError(invoice.SupplierDocumentaryAddress.OrganisationPKInfo, "You have not entered a Supplier.");
			AssertNoMessageError(invoice.SupplierDocOrgPKInfo, "You have not entered a Supplier.");
		}

		public void TestCheckE2_OA_Address()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var invoice = declaration.Invoices.AddNew();

			invoice.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertHasMessageError(invoice.SupplierDocumentaryAddress.E2_OA_AddressInfo, "You have not entered a Supplier Address.");

			invoice.SupplierDocumentaryAddress.DocAddressType = DocAddressType.SupplierPickupDeliveryAddress;
			invoice.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertNoMessageError(invoice.SupplierDocumentaryAddress.E2_OA_AddressInfo, "You have not entered a Supplier Address.");

			invoice.SupplierDocumentaryAddress.DocAddressType = DocAddressType.SupplierDocumentaryAddress;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			invoice.SupplierDocumentaryAddress.Validation.ValidateAll();
			AssertNoMessageError(invoice.SupplierDocumentaryAddress.E2_OA_AddressInfo, "You have not entered a Supplier Address.");

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			invoice.SupplierDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNoMessageError(invoice.SupplierDocumentaryAddress.E2_OA_AddressInfo, "You have not entered a Supplier Address.");
		}

		public void TestCheckE2_OA_Address_StatusAndStreetNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoice = declaration.Invoices.AddNew();

			AddressValidationHelperTest.TestCheckAddressStatusAndStreetNumber(Factory, invoice.SupplierDocumentaryAddress.E2_OA_AddressInfo);
		}
	}
}

