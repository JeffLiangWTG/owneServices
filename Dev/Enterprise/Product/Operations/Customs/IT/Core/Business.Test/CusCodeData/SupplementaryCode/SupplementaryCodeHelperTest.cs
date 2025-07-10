using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SupplementaryCodeHelperTest : TestCaseWithFactory
{
	public void TestIsQVatSupplementaryCode()
	{
		AssertEquals("Code: C001, IsQVatSupplementaryCode?", false, SupplementaryCodeHelper.IsQVatSupplementaryCode("C001"));

		AssertEquals("Code: Q001, IsQVatSupplementaryCode?", true, SupplementaryCodeHelper.IsQVatSupplementaryCode("Q001"));

		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var supplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();

		supplementaryCode.CY_Code = "C001";
		AssertEquals("SupplementaryCode with Code: C001, IsQVatSupplementaryCode?", false, supplementaryCode.IsQVatAdditionalCode());

		supplementaryCode.CY_Code = "Q001";
		AssertEquals("SupplementaryCode with Code: Q001, IsQVatSupplementaryCode?", true, supplementaryCode.IsQVatAdditionalCode());

		AssertEquals("null IsQVatSupplementaryCode?", false, (null as SupplementaryCode).IsQVatAdditionalCode());
	}

	public void TestHasMoreThanOneVatQVatAdditionalCode()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertEquals("When Supplementary Code Supporter has no Supplementary Codes, HasMoreThanOneVatQVatAdditionalCode()", false, invoiceLine.HasMoreThanOneVatQVatAdditionalCode());

		var supplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode.CY_Code = "Q001";
		AssertEquals("When Supplementary Code Supporter has One Supplementary Codes, HasMoreThanOneVatQVatAdditionalCode()", false, invoiceLine.HasMoreThanOneVatQVatAdditionalCode());

		invoiceLine.JI_SupplementaryCode1 = "Q002";
		AssertEquals("When Supplementary Code Supporter has Two Supplementary Codes, HasMoreThanOneVatQVatAdditionalCode()", true, invoiceLine.HasMoreThanOneVatQVatAdditionalCode());
	}
}
