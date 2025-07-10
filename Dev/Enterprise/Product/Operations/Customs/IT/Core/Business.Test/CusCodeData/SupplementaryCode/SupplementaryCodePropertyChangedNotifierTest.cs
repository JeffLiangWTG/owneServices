using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SupplementaryCodePropertyChangedNotifierTest : EU.Business.Testing.SupplementaryCodePropertyChangedNotifierTest
{
	public void TestNotifyPropertyCY_CodeChangedForInvoiceLine()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateTaxOrFeeWithRelatedVatApplicability("IMP", "99999999", ("ORD", 21m, ""), ("RID", 10m, "Q001"), ("MIN", 4m, "Q002"));

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "99999999";

		invoiceLine.JI_SupplementaryCode1 = "Q001";
		AssertEquals("JI_ZZF_NKTaxType", "RID", invoiceLine.JI_ZZF_NKTaxType);

		invoiceLine.JI_SupplementaryCode1 = "";
		invoiceLine.JI_ZZF_NKTaxType = "";

		var supplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
		supplementaryCode.CY_Code = "Q002";
		AssertEquals("JI_ZZF_NKTaxType", "MIN", invoiceLine.JI_ZZF_NKTaxType);

		invoiceLine.JI_SupplementaryCode1 = "Q001";
		AssertEquals("When Invoice Line has two Q Vat additional code, no action are expected. JI_ZZF_NKTaxType", "MIN", invoiceLine.JI_ZZF_NKTaxType);

		invoiceLine.JI_SupplementaryCode1 = "";
		invoiceLine.JI_ZZF_NKTaxType = "";
		using (invoiceLine.SuspendTaxTypeDefaulting())
		{
			invoiceLine.JI_SupplementaryCode1 = "Q001";
		}
		AssertEquals("When Tax Type Defaulting is suspended, JI_ZZF_NKTaxType", "", invoiceLine.JI_ZZF_NKTaxType);

		invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
		invoiceLine.JI_SupplementaryCode1 = "Q999";
		AssertEquals("When Q Vat additional code is not present in related VAT Applicability, JI_ZZF_NKTaxType", "", invoiceLine.JI_ZZF_NKTaxType);

		invoiceLine.JI_ZZF_NKTaxType = "RID";
		invoiceLine.JI_SupplementaryCode1 = "";
		AssertEquals("When user clear Q Vat additional code, JI_ZZF_NKTaxType", "", invoiceLine.JI_ZZF_NKTaxType);
	}
}
