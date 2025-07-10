using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class JobComInvoiceLineJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestCheckOrganisationPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;

			invoiceLine.ManufacturerDocAddress.Validation.ValidateAll();

			AssertHasMessageErrorContaining(invoiceLine.ManufacturerDocAddress.OrganisationPKInfo, "You have not entered a Manufacturer");

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			invoiceLine.ManufacturerDocAddress.Validation.ValidateAll();

			AssertNoMessageErrors(invoiceLine.ManufacturerDocAddress.OrganisationPKInfo);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.MainAddress.CompanyName = "MANUFACTURER";
			manufacturer.MainAddress.OA_Email = "MANUFACTURER@TEST.COM";
			manufacturer.MainAddress.OA_Address1 = "MANUFACTURER ADDRESS 1";
			manufacturer.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "MANUFACTURER ADDITIONAL ADDRESS";
			manufacturer.MainAddress.OA_City = "MANUFACTURER CITY";
			manufacturer.MainAddress.OA_State = "MS";

			invoiceLine.ManufacturerDocAddress.OrganisationPK = manufacturer.PK;

			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;
		}

		public void TestCheckE2_OA_Address()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;

			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.MainAddress.CompanyName = "MANUFACTURER";
			manufacturer.MainAddress.OA_Email = "MANUFACTURER@TEST.COM";
			manufacturer.MainAddress.OA_Address1 = "MANUFACTURER ADDRESS 1";
			manufacturer.MainAddress.OA_AdditionalAddressInformation = "MANUFACTURER ADDITIONAL ADDRESS";
			manufacturer.MainAddress.OA_City = "MANUFACTURER CITY";
			manufacturer.MainAddress.OA_State = "MS";

			invoiceLine.ManufacturerDocAddressPK = manufacturer.MainAddress.PK;

			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;
			invoice.JZ_OA_SupplierAddress = manufacturer.MainAddress.PK;
			invoiceLine.ManufacturerDocAddress.Validation.ValidateAll();
			AssertNoMessageErrorContaining(invoiceLine.ManufacturerDocAddress.E2_OA_AddressInfo, "Manufacturer is equals to Supplier. Manufacturer Indicator should not be \"2 - The Manufacturer is not the Supplier\"");

			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			invoiceLine.ManufacturerDocAddress.Validation.ValidateAll();
			AssertNoMessageErrorContaining(invoiceLine.ManufacturerDocAddress.E2_OA_AddressInfo, "Manufacturer is equals to Supplier. Manufacturer Indicator should not be \"2 - The Manufacturer is not the Supplier\"");

			invoiceLine.ManufacturerDocAddressPK = manufacturer.MainAddress.PK;
			AssertHasMessageErrorContaining(invoiceLine.ManufacturerDocAddress.E2_OA_AddressInfo, "Manufacturer is equals to Supplier. Manufacturer Indicator should not be \"2 - The Manufacturer is not the Supplier\"");
		}

		public void TestCheckE2_OA_Address_StatusAndStreetNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			AddressValidationHelperTest.TestCheckAddressStatusAndStreetNumber(Factory, invoiceLine.ManufacturerDocAddress.E2_OA_AddressInfo, true);
			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._1;
			AddressValidationHelperTest.TestCheckAddressStatusAndStreetNumber(Factory, invoiceLine.ManufacturerDocAddress.E2_OA_AddressInfo, false);
		}
	}
}
