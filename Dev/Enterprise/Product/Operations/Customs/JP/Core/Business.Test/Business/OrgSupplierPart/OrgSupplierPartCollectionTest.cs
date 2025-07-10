using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartCollection))]
	sealed class OrgSupplierPartCollectionTest : Customs.Business.Testing.OrgSupplierPartCollectionTest
	{
		public void TestAddPivotWithAdditionalLineDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "JI_Description";
			invoiceLine.JI_CountryOfOrigin = "JP";
			invoiceLine.JI_PrimaryPreference = "1234567890";
			invoiceLine.JI_SecondaryPreference = "13579";
			invoiceLine.JI_TradeControlOrderAppendix = "1";
			invoiceLine.JI_FEFTAArticle48 = "A";
			invoiceLine.JI_StorageType = "B";
			invoiceLine.JI_AdvanceRulingOnClassification = "C";
			invoiceLine.JI_AdvanceRulingOnOrigin = "D";
			invoiceLine.JI_DutyReductionExemptionRefundCode = "E";
			invoiceLine.JI_DomesticConsumptionTaxExemptionCode = "F";
			invoiceLine.JI_DomesticConsumptionTaxExemptionIsPartial = true;
			invoiceLine.JI_DutyReductionAmount = 618;

			var cusClass = Factory.New<CusClassification>();
			cusClass.CC_ClassificationType = CusClassification.ClassificationType.Both;
			cusClass.CC_TariffNum = "0121323122";
			invoiceLine.JI_CC = cusClass.PK;

			var collection = new OrgSupplierPartCollection(Factory, invoiceLine, false);
			var orgSupplierPart = collection.AddNew() as OrgSupplierPart;
			var pivot = orgSupplierPart.PivotsForBinding[0] as CusClassPartPivot;

			CombineAssertions(() =>
			{
				AssertEquals("CI_Description", invoiceLine.JI_Description, pivot.CI_Description);
				AssertEquals("CI_RN_NKCountryOfOrigin", invoiceLine.JI_CountryOfOrigin, pivot.CI_RN_NKCountryOfOrigin);
				AssertEquals("CI_PrimaryPreference", invoiceLine.JI_PrimaryPreference, pivot.CI_PrimaryPreference);
				AssertEquals("CI_SecondaryPreference", invoiceLine.JI_SecondaryPreference, pivot.CI_SecondaryPreference);
				AssertEquals("CI_TradeControlOrderAppendix", invoiceLine.JI_TradeControlOrderAppendix, pivot.CI_TradeControlOrderAppendix);
				AssertEquals("CI_FEFTAArticle48", invoiceLine.JI_FEFTAArticle48, pivot.CI_FEFTAArticle48);
				AssertEquals("CI_AdvanceRulingOnClassification", invoiceLine.JI_AdvanceRulingOnClassification, pivot.CI_AdvanceRulingOnClassification);
				AssertEquals("CI_AdvanceRulingOnOrigin", invoiceLine.JI_AdvanceRulingOnOrigin, pivot.CI_AdvanceRulingOnOrigin);
				AssertEquals("CI_DutyReductionExemptionRefundCode", invoiceLine.JI_DutyReductionExemptionRefundCode, pivot.CI_DutyReductionExemptionRefundCode);
				AssertEquals("CI_DomesticConsumptionTaxExemptionCode", invoiceLine.JI_DomesticConsumptionTaxExemptionCode, pivot.CI_DomesticConsumptionTaxExemptionCode);
				AssertEquals("CI_DomesticConsumptionTaxExemptionIsPartial", invoiceLine.JI_DomesticConsumptionTaxExemptionIsPartial, pivot.CI_DomesticConsumptionTaxExemptionIsPartial);
				AssertEquals("CI_DutyReductionAmount", invoiceLine.JI_DutyReductionAmount, pivot.CI_DutyReductionAmount);
			});
		}
	}
}
