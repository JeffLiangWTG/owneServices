using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Moq;

namespace Enterprise.Customs.ES.Business.Testing;

internal class GuidedDecisionMakingMultiInvoiceLinesTargetTest : TestCaseWithFactory
{
	public void TestTaxOrFeeDetail()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var taxOrFeeDetails = invoiceLine.Lookups.TaxOrFeeDetailEntities;
		var tax1 = new EU.Business.Declaration.TaxOrFeeDetailEntity() { VATCode = "RED", AdditionalCode = "V001", Category = "A001" };
		var tax2 = new EU.Business.Declaration.TaxOrFeeDetailEntity() { VATCode = "STD", AdditionalCode = "V002", Category = "A001" };
		var tax3 = new EU.Business.Declaration.TaxOrFeeDetailEntity() { VATCode = "SRR", AdditionalCode = "", Category = "A001" };
		var tax4 = new EU.Business.Declaration.TaxOrFeeDetailEntity() { VATCode = "RED", AdditionalCode = "", Category = "A001" };

		taxOrFeeDetails.Add(tax1);
		taxOrFeeDetails.Add(tax2);
		taxOrFeeDetails.Add(tax3);
		taxOrFeeDetails.Add(tax4);

		invoiceLine.JI_TaxOrFeeDetail = tax1.PK;
		invoiceLine2.JI_TaxOrFeeDetail = tax4.PK;

		var gdmBasic = GuidedDecisionMakingBasicTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory);
		var selectedVAT = new Mock<EU.Business.GuidedDecisionMakingVAT>(gdmBasic);
		selectedVAT.Setup(v => v.AdditionalCode).Returns("V002");
		selectedVAT.Setup(v => v.VATCode).Returns("STD");
		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new System.Collections.Generic.List<EU.Business.Declaration.JobComInvoiceLine> { invoiceLine, invoiceLine2 });
		guidedDecisionMakingTarget.SetVATCode(selectedVAT.Object.VATCode, selectedVAT.Object.AdditionalCode);
		AssertEquals("Invoice Line 1 JI_TaxOrFeeDetail should be correctly set with tax2", tax2.PK, invoiceLine.JI_TaxOrFeeDetail);
		AssertEquals("Invoice Line 1 JI_ZZF_NKTaxType should be correctly set with tax2", "STD", invoiceLine.JI_ZZF_NKTaxType);
		AssertEquals("Invoice Line 2 JI_TaxOrFeeDetail should be correctly set with tax2", tax2.PK, invoiceLine2.JI_TaxOrFeeDetail);
		AssertEquals("Invoice Line 2 JI_ZZF_NKTaxType should be correctly set with tax2", "STD", invoiceLine2.JI_ZZF_NKTaxType);

		selectedVAT.Setup(v => v.AdditionalCode).Returns("");
		selectedVAT.Setup(v => v.VATCode).Returns("SRR");
		guidedDecisionMakingTarget.SetVATCode(selectedVAT.Object.VATCode, selectedVAT.Object.AdditionalCode);
		AssertEquals("Invoice Line 1 JI_TaxOrFeeDetail should be correctly set with tax3", tax3.PK, invoiceLine.JI_TaxOrFeeDetail);
		AssertEquals("Invoice Line 1 JI_ZZF_NKTaxType should be correctly set with tax3", "SRR", invoiceLine.JI_ZZF_NKTaxType);
		AssertEquals("Invoice Line 2 JI_TaxOrFeeDetail should be correctly set with tax3", tax3.PK, invoiceLine2.JI_TaxOrFeeDetail);
		AssertEquals("Invoice Line 2 JI_ZZF_NKTaxType should be correctly set with tax3", "SRR", invoiceLine2.JI_ZZF_NKTaxType);

		invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
		invoiceLine.JI_ZZF_NKTaxType = ZString.Empty;
		invoiceLine2.JI_TaxOrFeeDetail = ZGuid.Empty;
		invoiceLine2.JI_ZZF_NKTaxType = ZString.Empty;
		selectedVAT.Setup(v => v.AdditionalCode).Returns("");
		selectedVAT.Setup(v => v.VATCode).Returns("EX");
		guidedDecisionMakingTarget.SetVATCode(selectedVAT.Object.VATCode, selectedVAT.Object.AdditionalCode);
		AssertEquals("Invoice Line 1 JI_TaxOrFeeDetail should be left as it was", ZGuid.Empty, invoiceLine.JI_TaxOrFeeDetail);
		AssertEquals("Invoice Line 1 JI_ZZF_NKTaxType should be correctly set with EX", "EX", invoiceLine.JI_ZZF_NKTaxType);
		AssertEquals("Invoice Line 2 JI_TaxOrFeeDetail should be left as it was", ZGuid.Empty, invoiceLine2.JI_TaxOrFeeDetail);
		AssertEquals("Invoice Line 2 JI_ZZF_NKTaxType should be correctly set with EX", "EX", invoiceLine2.JI_ZZF_NKTaxType);
	}
}
