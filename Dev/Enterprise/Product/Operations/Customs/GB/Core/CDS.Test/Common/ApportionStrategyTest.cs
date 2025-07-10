using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class ApportionStrategyTest : TestCaseWithFactory
	{
		public void TestRoundFunctionTruncateAt2Decimals()
		{
			var apportionStrategy = new ApportionStrategy();
			AssertEquals(5.67m, apportionStrategy.Round(5.6789m));
		}

		public void TestShouldBackApportion()
		{
			CombineAssertions(() =>
			{
				var apportionStrategy = new ApportionStrategy();
				var chargeKey = new ApportionChargeKey(CustomsChargeTypeList.Codes.AdditionCharge, false, true, ApportionmentTypeList.Codes.FullApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m, false, "", false);
				AssertEquals("ADD", false, apportionStrategy.ShouldBackApportion(chargeKey));

				chargeKey = new ApportionChargeKey(ChargesProvider.AirFreightCode, false, true, ApportionmentTypeList.Codes.FullApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m, false, "", false);
				AssertEquals("AFT - isIncludedInITOT: No", true, apportionStrategy.ShouldBackApportion(chargeKey));
			});
		}

		public void TestApportionWithoutRoundingErrorForSingleInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 630.0m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100.0m;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 630.0m;
			declaration.ResumeApportionment();

			AssertEquals(1, invoiceLine.ApportionedCharges.Count);
			AssertEquals(100.0m, invoiceLine.ApportionedCharges[0].J7_Amount);

			invoiceLine.JI_LinePrice = 456.7m;
			declaration.ResumeApportionment();

			AssertEquals(1, invoiceLine.ApportionedCharges.Count);
			AssertEquals(100.0m, invoiceLine.ApportionedCharges[0].J7_Amount);
		}
	}
}
