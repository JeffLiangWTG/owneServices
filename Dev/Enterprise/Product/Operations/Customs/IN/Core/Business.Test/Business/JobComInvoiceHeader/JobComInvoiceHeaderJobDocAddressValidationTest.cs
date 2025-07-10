using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IN.Business.Testing;

sealed class JobComInvoiceHeaderJobDocAddressValidationTest : TestCaseWithFactory
{
	public void TestCheckOrganisationPK_BuyerDocAddress()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		AssertNoMessageErrorContaining(invoice.BuyerDocAddress.OrganisationPKInfo, "You have not entered");

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoice.BuyerDocAddress.E2_AddressOverride = true;
		invoice.BuyerDocAddress.OrganisationPK = ZGuid.Empty;
		AssertNoMessageErrorContaining(invoice.BuyerDocAddress.OrganisationPKInfo, "You have not entered");

		invoice.BuyerDocAddress.E2_AddressOverride = false;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoice.BuyerDocAddress.OrganisationPKInfo);
	}

	public void TestCheckOrganisationPK_AuthorizedEconomicOperator()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		const string messageErrorAEO = "You have not entered an AEO";

		CombineAssertions("AuthorizedEconomicOperatorOrgPK validation check", () =>
		{
			invoice.JZ_AuthorizedEconomicOperatorRole = "ABC";
			ValidationTestHelper.AssertFieldIsNotMandatory(invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_AuthorizedEconomicOperatorRole = ZString.Empty;
			invoice.AuthorizedEconomicOperatorAddress.OrganisationPK = ZGuid.BrettsGuid;
			AssertNoMessageError("AEO not empty, AEO Role empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEO);

			invoice.JZ_AuthorizedEconomicOperatorRole = "ABC";
			invoice.AuthorizedEconomicOperatorAddress.OrganisationPK = ZGuid.Empty;
			AssertHasMessageError("AEO empty, AEO Role not empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEO);

			invoice.JZ_AuthorizedEconomicOperatorRole = ZString.Empty;
			invoice.AuthorizedEconomicOperatorAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageError("AEO empty, AEO Role empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEO);
		});

		const string messageErrorAEOCode = "AEO Code not entered under selected Organization.";
		const string messageErrorAEOCountry = "AEO Country not entered under selected Organization.";

		var org = Factory.NewWithValidTestData<OrgHeader>();
		var addressIN = org.Addresses.AddNew();
		OrgCusCode cusCode = org.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, "1234");
		cusCode.OK_OA_PremisesAddress = addressIN.PK;
		var org2 = Factory.NewWithValidTestData<OrgHeader>();
		var addressIN2 = org2.Addresses.AddNew();
		OrgCusCode cusCode2 = org2.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.AEO, "5678");
		cusCode2.OK_OA_PremisesAddress = addressIN2.PK;

		CombineAssertions("AuthorizedEconomicOperatorCountry amd AuthorizedEconomicOperatorCode validation check", () =>
		{
			invoice.AuthorizedEconomicOperatorAddress.E2_OA_Address = addressIN.PK;
			invoice.AuthorizedEconomicOperatorOrgPK = ZGuid.Empty;
			AssertNoMessageErrorContaining("AEO empty, Code empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEOCode);
			AssertNoMessageErrorContaining("AEO empty, Country empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEOCountry);

			invoice.AuthorizedEconomicOperatorOrgPK = ZGuid.BrettsGuid;
			AssertHasMessageErrorContaining("AEO not empty, Code empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEOCode);
			AssertHasMessageErrorContaining("AEO not empty, Country empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEOCountry);

			invoice.AuthorizedEconomicOperatorAddress.E2_OA_Address = addressIN2.PK;
			invoice.AuthorizedEconomicOperatorAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("AEO not empty, Code not empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEOCode);
			AssertHasMessageErrorContaining("AEO not empty, Country empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEOCountry);

			org.MainAddress.OA_RN_NKCountryCode = "IN";
			invoice.AuthorizedEconomicOperatorAddress.E2_OA_Address = addressIN.PK;
			invoice.AuthorizedEconomicOperatorAddress.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("AEO not empty, Code empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEOCode);
			AssertNoMessageErrorContaining("AEO not empty, Country not empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEOCountry);

			org2.MainAddress.OA_RN_NKCountryCode = "IN";
			invoice.AuthorizedEconomicOperatorAddress.E2_OA_Address = addressIN2.PK;
			invoice.AuthorizedEconomicOperatorAddress.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("AEO not empty, Code not empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEOCode);
			AssertNoMessageErrorContaining("AEO not empty, Country not empty", invoice.AuthorizedEconomicOperatorAddress.OrganisationPKInfo, messageErrorAEOCountry);
		});
	}
}
