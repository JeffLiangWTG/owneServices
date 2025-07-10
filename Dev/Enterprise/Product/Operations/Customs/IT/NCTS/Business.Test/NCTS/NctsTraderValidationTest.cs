using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

abstract class NctsTraderValidationTest : TestCaseWithFactory
{
	public void TestNaturalConsignorHasValidCustomsCode() => AssertConsignorHasValidCustomsCode(ValidationCaptions.TraderJobDocAddressValidation.EoriCodeOrFiscalCodeIsRequired, OrgConstants.Category.NaturalPersonIndividual);

	public void TestBusinessConsignorHasValidCustomsCode() => AssertConsignorHasValidCustomsCode(ValidationCaptions.TraderJobDocAddressValidation.EoriCodeOrVatCodeIsRequired, OrgConstants.Category.Business);

	protected abstract JobDocAddress GetConsignorAddress();

	#region Implementation

	void AssertConsignorHasValidCustomsCode(string expectedMessageError, string orgCategory)
	{
		var consignor = GetConsignorAddress();

		CombineAssertions("Assert Business Consgnor validation", () =>
		{
			WhenIsEuAndHasNoValidCustomsCode();
			if (orgCategory == OrgConstants.Category.Business)
			{
				WhenIsEuAndHasValidVatCode();
			}
			else
			{
				WhenIsEuAndHasValidFiscalCode();
			}
			WhenIsEuAndHasValidEoriCode();
			WhenIsExtraEuAndHasNoValidCustomsCode();
		});

		void WhenIsEuAndHasNoValidCustomsCode()
		{
			var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, traderId: "EU1", consignor, suffix: "1", traderName: "EU Consignor with no valid customs code");
			ConfigureOrgHeader(orgHeader, orgCategory);
			consignor.Validation.ValidateOrganisationPK();
			AssertHasMessageErrorContaining("When consignor is EU and does not have any valid customs code", consignor.OrganisationPKInfo, expectedMessageError);
		}

		void WhenIsEuAndHasValidVatCode()
		{
			var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, traderId: "EU2", consignor, suffix: "2", traderName: "EU Consignor with vat code");
			ConfigureOrgHeader(orgHeader, OrgConstants.Category.Business);
			orgHeader.CustomsCodes.AddNew("IVA", "VAT (IVA) CODE", "IT");
			consignor.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("When consignor is EU and has any valid VAT code", consignor.OrganisationPKInfo, expectedMessageError);
		}

		void WhenIsEuAndHasValidFiscalCode()
		{
			var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, traderId: "EU2", consignor, suffix: "2", traderName: "EU Consignor with vat code");
			ConfigureOrgHeader(orgHeader, OrgConstants.Category.NaturalPersonIndividual);
			orgHeader.CustomsCodes.AddNew("COD", "FISCAL CODE", "IT");
			consignor.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("When consignor is EU and has any valid VAT code", consignor.OrganisationPKInfo, expectedMessageError);
		}

		void WhenIsEuAndHasValidEoriCode()
		{
			var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, traderId: "EU3", consignor, suffix: "3", traderName: "EU Consignor with eori code");
			ConfigureOrgHeader(orgHeader, orgCategory);
			orgHeader.CustomsCodes.AddNew("EOR", "EOR CODE", "IT");
			consignor.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("When consignor is EU and has any valid eori code", consignor.OrganisationPKInfo, expectedMessageError);
		}

		void WhenIsExtraEuAndHasNoValidCustomsCode()
		{
			var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, traderId: "AU1", consignor, suffix: "4", traderName: "Extra EU Consignor with any valid customs code");
			ConfigureOrgHeader(orgHeader, orgCategory, isEu: false);
			consignor.SetDefaultAddressFromOrg();
			consignor.Validation.ValidateOrganisationPK();
			AssertNoMessageErrorContaining("When consignor is Extra EU and does not have any valid customs code", consignor.OrganisationPKInfo, expectedMessageError);
		}

		void ConfigureOrgHeader(OrgHeader orgHeader, string category, bool isEu = true)
		{
			orgHeader.OH_Category = category;
			orgHeader.MainAddress.OA_RN_NKCountryCode = isEu ? "IT" : "AU";
			orgHeader.CustomsCodes.RemoveAndDeleteAll();
		}
	}

	#endregion
}
