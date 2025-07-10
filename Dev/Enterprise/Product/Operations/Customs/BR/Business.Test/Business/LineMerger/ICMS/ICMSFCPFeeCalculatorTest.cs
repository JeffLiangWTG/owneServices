using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ICMSFCPFeeCalculatorTest : TestCaseWithFactory
	{
		public void TestICMSCalculationWhenTaxRegimeIsEmpty()
		{
			invoiceLine.ICMSFCPRateValue = 0m;
			invoiceLine.JI_ICMSRate = 0m;
			calculator.UpdateFeesOnEntryLine(entryLine);
			AssertNull("No ICMS fee added", entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.ICMS));
			AssertNull("No ICMS fee added", entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.ICMSFCP));

			invoiceLine.JI_ICMSRate = 100m;
			calculator.UpdateFeesOnEntryLine(entryLine);
			AssertNull("No ICMS fee added", entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.ICMS));

			invoiceLine.JI_ICMSRate = 12m;

			AssertICMSTaxValues(12m, 0m, 1237.5m);
			AssertNull("No ICMS fee added", entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.ICMSFCP));

			invoiceLine.ICMSFCPRateValue = 10m;
			AssertICMSTaxValues(12m, 0m, 1396.15m);
			AssertICMSFCPTaxValues(10m, 0m, 1396.15m);
		}

		public void TestICMSCalculationWhenTaxRegimeIsReduction()
		{
			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			invoiceLine.JI_ICMSFormula = ZString.Empty;
			invoiceLine.JI_ICMSRate = 12m;
			invoiceLine.JI_ICMSBaseValueReductionPercentage = 10m;
			calculator.UpdateFeesOnEntryLine(entryLine);
			AssertNull("No ICMS fee added", entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.ICMS));

			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			invoiceLine.JI_ICMSFormula = ICMSFormulaList.Codes.BC;
			calculator.UpdateFeesOnEntryLine(entryLine);

			AssertICMSTaxValues(12m, 150.78m, 1256.54m);
			AssertICMSFCPTaxValues(10m, 125.65m, 1256.54m);

			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.Reduction;
			invoiceLine.JI_ICMSBaseValueReductionPercentage = 30m;
			invoiceLine.JI_ICMSFormula = ICMSFormulaList.Codes.BCR;
			calculator.UpdateFeesOnEntryLine(entryLine);

			AssertICMSTaxValues(12m, 108.13m, 901.06m);
			AssertICMSFCPTaxValues(10m, 90.11m, 901.06m);

			invoiceLine.JI_ICMSTotalAmountReductionPercentage = 6m;
			invoiceLine.JI_ICMSBaseValueReductionPercentage = 0m;

			AssertICMSTaxValues(12m, 157.49m, 1396.15m);
			AssertICMSFCPTaxValues(10m, 139.62m, 1396.15m);
		}

		public void TestICMSCalculationWithTotalAmountReduction()
		{
			invoiceLine.ICMSTaxRegime = ICMSTaxRegimeList.Codes.FullCollection;
			invoiceLine.JI_ICMSRate = 12m;
			invoiceLine.JI_ICMSTotalAmountReductionPercentage = 6m;

			AssertICMSTaxValues(12m, 157.49m, 1396.15m);
			AssertICMSFCPTaxValues(10m, 139.62m, 1396.15m);
		}

		public void TestICMSCalculationWhenICMSFeeCalculationNotApplicable()
		{
			foreach (var icmsTaxRegime in new ICMSTaxRegimeList().GetAllCodes().Except(new[] { ICMSTaxRegimeList.Codes.FullCollection, ICMSTaxRegimeList.Codes.Reduction }))
			{
				invoiceLine.ICMSTaxRegime = icmsTaxRegime;
				invoiceLine.JI_ICMSRate = 12m;
				invoiceLine.JI_ICMSTotalAmountReductionPercentage = 6m;
				AssertICMSTaxValues(12m, 0m, 1396.15m);
				AssertICMSFCPTaxValues(10m, 0m, 1396.15m);
			}
		}

		void AssertICMSTaxValues(ZDecimal rate, ZDecimal chargeAmount, ZDecimal baseValue)
		{
			calculator.UpdateFeesOnEntryLine(entryLine);

			CombineAssertions(() =>
			{
				var icmsFee = entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.ICMS);
				AssertEquals("CF_ChargeAmount", chargeAmount, icmsFee.CF_ChargeAmount);
				AssertEquals("CF_BaseValue", baseValue, icmsFee.CF_BaseValue);
				AssertEquals("CF_Rate", rate, icmsFee.CF_Rate);
				AssertEquals("CF_MethodOfCalculation", Constants.MethodOfCalculation.Percentage, icmsFee.CF_MethodOfCalculation);
			});
		}

		void AssertICMSFCPTaxValues(ZDecimal rate, ZDecimal chargeAmount, ZDecimal baseValue)
		{
			calculator.UpdateFeesOnEntryLine(entryLine);

			CombineAssertions(() =>
			{
				var icmsFee = entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.ICMSFCP);
				if (rate == 0)
				{
					AssertNull("ICMS FCP should not be generated when FCP rate is zero.", icmsFee);
				}
				else
				{
					AssertEquals("CF_ChargeAmount", chargeAmount, icmsFee.CF_ChargeAmount);
					AssertEquals("CF_BaseValue", baseValue, icmsFee.CF_BaseValue);
					AssertEquals("CF_Rate", rate, icmsFee.CF_Rate);
					AssertEquals("CF_MethodOfCalculation", Constants.MethodOfCalculation.Percentage, icmsFee.CF_MethodOfCalculation);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.ICMSFCPRateValue = 10m;

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryLine = cusEntryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 600m;
			entryLine.Fees.AddOrUpdate(ChargeTypesList.Codes.DTY, 100m);
			entryLine.Fees.AddOrUpdate(Constants.RateTypes.IPI, 10m);
			entryLine.Fees.AddOrUpdate(Constants.RateTypes.PIS, 50m);
			entryLine.Fees.AddOrUpdate(Constants.RateTypes.Cofins, 90m);
			entryLine.Fees.AddOrUpdate(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, 20m);
			entryLine.Fees.AddOrUpdate(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, 12m);
			entryLine.Fees.AddOrUpdate(Constants.RateTypes.Antidumping, 7m);
			entryLine.Fees.AddOrUpdate(Constants.RateTypes.OtherExpensesICMS, 200m);

			invoiceLine.JI_CL = entryLine.PK;

			calculator = new ICMSFCPFeeCalculator();
		}

		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		ICMSFCPFeeCalculator calculator;
	}
}
