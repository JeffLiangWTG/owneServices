using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class InvoiceLineTraderJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		var jobDocAddress = Factory.New<JobDocAddress>();
		AssertExceptionThrown<ArgumentNullException>("Null declaration", () => new InvoiceLineTraderJobDocAddressValidation(jobDocAddress, null));
	}

	public void TestCustomizedTraderNames()
	{
		AssertCustomizedName(invoiceLine.SellerDocAddress, "Seller");
		AssertCustomizedName(invoiceLine.BuyerDocAddress, "Buyer");
	}

	public void AssertCustomizedName(JobDocAddress traderAddress, string traderName)
	{
		declaration.JE_MessageType = "IMP";

		var organization = Factory.NewWithValidTestData<OrgHeader>();
		var organizationAddress = organization.Addresses.AddNew();
		var propertyInfo = traderAddress.OrganisationPKInfo;
		traderAddress.OrganisationPK = organization.PK;
		traderAddress.E2_OA_Address = organizationAddress.PK;

		var expectedAddressWarningMessage = $"{traderName} Address is longer than 70 characters, it will be truncated in the message.";

		organizationAddress.OA_Address1 = "".PadRight(35, 'A');
		organizationAddress.OA_Address2 = "".PadRight(36, 'A');
		traderAddress.Validation.ValidateOrganisationPK();

		AssertHasWarningContaining($"Warning message must use the word {traderName} and not Selling or Buying Party", propertyInfo, expectedAddressWarningMessage);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
}
