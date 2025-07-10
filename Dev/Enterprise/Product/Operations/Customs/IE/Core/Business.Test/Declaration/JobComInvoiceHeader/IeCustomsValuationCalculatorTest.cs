using Enterprise.Customs.EU.Business.Declaration.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class IeCustomsValuationCalculatorTest : EuCustomsValuationCalculatorTest
	{
		public void TestStatValue2xGroupCharge()
		{
			var (invoiceLine, declaration) = SetupData();
			var charge2x = declaration.TopGroupInvoice.Charges.AddNew(AISChargeCodeList.Codes._2X, 1000m, declaration.LocalCurrencyCode);
			charge2x.J7_IsDutiable = false;
			charge2x.J7_IsIncludedInITOT = true;
			declaration.ResumeApportionment();
			CombineAssertions("Line calculations in case of 2X group charge", () =>
			{
				AssertEquals("Customs Value", 4000m, invoiceLine.JI_CustomsValue);
				AssertEquals("CIF Value", 4000m, invoiceLine.JI_Calc_CIF);
				AssertEquals("Statistical Value", 5000m, invoiceLine.JI_Calc_StatisticalValue);
			});
		}

		public void TestStatValue2xCharge()
		{
			var (invoiceLine, declaration) = SetupData();
			var charge2x = invoiceLine.Charges.AddNew(AISChargeCodeList.Codes._2X, 1100m, declaration.LocalCurrencyCode);
			charge2x.J7_IsDutiable = false;
			charge2x.J7_IsIncludedInITOT = true;
			CombineAssertions("Line calculations in case of 2X group charge", () =>
			{
				AssertEquals("Customs Value", 3900m, invoiceLine.JI_CustomsValue);
				AssertEquals("CIF Value", 3900m, invoiceLine.JI_Calc_CIF);
				AssertEquals("Statistical Value", 5000m, invoiceLine.JI_Calc_StatisticalValue);
			});
		}

		public void TestStatValueGroupCharge()
		{
			var (invoiceLine, declaration) = SetupData();
			var charge = declaration.TopGroupInvoice.Charges.AddNew(AISChargeCodeList.Codes._1X, 900m, declaration.LocalCurrencyCode);
			charge.J7_IsDutiable = true;
			charge.J7_IsIncludedInITOT = false;
			declaration.ResumeApportionment();
			CombineAssertions("Line calculations in case of 2X group charge", () =>
			{
				AssertEquals("Customs Value", 5900m, invoiceLine.JI_CustomsValue);
				AssertEquals("CIF Value", 5900m, invoiceLine.JI_Calc_CIF);
				AssertEquals("Statistical Value", 5000m, invoiceLine.JI_Calc_StatisticalValue);
			});
		}

		public void TestStatValueCharge()
		{
			var (invoiceLine, declaration) = SetupData();
			var charge = invoiceLine.InvoiceHeader.Charges.AddNew(AISChargeCodeList.Codes.AB, 800m, declaration.LocalCurrencyCode);
			charge.J7_IsDutiable = true;
			charge.J7_IsIncludedInITOT = false;
			declaration.ResumeApportionment();
			CombineAssertions("Line calculations in case of 2X group charge", () =>
			{
				AssertEquals("Customs Value", 5800m, invoiceLine.JI_CustomsValue);
				AssertEquals("CIF Value", 5800m, invoiceLine.JI_Calc_CIF);
				AssertEquals("Statistical Value", 5800m, invoiceLine.JI_Calc_StatisticalValue);
			});
		}

		protected override EU.Business.Declaration.JobDeclaration GetNewJobDeclaration() => Factory.New<JobDeclaration>();

		(JobComInvoiceLine invoiceLine, JobDeclaration declaration) SetupData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var inv = declaration.Invoices.AddNew();
			inv.JZ_InvoiceAmount = 5000m;
			inv.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine = inv.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = inv.JZ_InvoiceAmount;

			return (invoiceLine, declaration);
		}
	}
}
