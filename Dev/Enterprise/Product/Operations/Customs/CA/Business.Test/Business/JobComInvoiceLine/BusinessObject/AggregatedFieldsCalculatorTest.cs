using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AggregatedFieldsCalculatorTest : TestCaseWithFactory
	{
		public void TestAggregatedFieldsCalculatedForInvoiceLine()
		{
			var invoiceLine = CreateInvoiceLine();
			invoiceLine.DutiesAndTaxes.DeleteAll();

			CombineAssertions("SIMA", () =>
			{
				var add = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.ADD);
				add.C1_ExemptCode = SIMACodes.Codes.C20;
				add.C1_Amount = 10m;
				add.C1_Rate = 0.5m;
				add.C1_RateType = "V";

				AggregatedFieldsCalculator.CalculateAggregatedFields(invoiceLine);
				AssertCalculateResult(SIMACodes.Codes.C20, 10m, "0.5V", invoiceLine.CA_SIMExemptCode, invoiceLine.CA_SIMAmount, invoiceLine.CA_SIMRateDescription, "ADD only");

				var sima = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SIMADuty);
				sima.C1_ExemptCode = SIMACodes.Codes.C30;
				sima.C1_Amount = 20m;
				sima.C1_Rate = 1m;
				sima.C1_RateType = "S";

				AggregatedFieldsCalculator.CalculateAggregatedFields(invoiceLine);
				AssertCalculateResult(SIMACodes.Codes.C30, 30m, "0.5V 1S", invoiceLine.CA_SIMExemptCode, invoiceLine.CA_SIMAmount, invoiceLine.CA_SIMRateDescription, "SIM first");

				var cvd = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.CVD);
				cvd.C1_Amount = 30m;

				var sur = invoiceLine.DutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.SUR);
				sur.C1_Amount = 40m;
				AggregatedFieldsCalculator.CalculateAggregatedFields(invoiceLine);
				AssertCalculateResult(SIMACodes.Codes.C30, 100m, "0.5V 1S", invoiceLine.CA_SIMExemptCode, invoiceLine.CA_SIMAmount, invoiceLine.CA_SIMRateDescription, "SIM first, sum amount");

				invoiceLine.DutiesAndTaxes.Delete(sima);
				AggregatedFieldsCalculator.CalculateAggregatedFields(invoiceLine);
				AssertCalculateResult(SIMACodes.Codes.C20, 80m, "0.5V", invoiceLine.CA_SIMExemptCode, invoiceLine.CA_SIMAmount, invoiceLine.CA_SIMRateDescription, "Without SIM, ADD becomes the top dog");

				invoiceLine.DutiesAndTaxes.DeleteAll();
				AggregatedFieldsCalculator.CalculateAggregatedFields(invoiceLine);
				AssertCalculateResult("", 0m, "", invoiceLine.CA_SIMExemptCode, invoiceLine.CA_SIMAmount, invoiceLine.CA_SIMRateDescription, "SIMA empty");
			});

			AssertCalculateSingleDutyTax(DutyAndTaxTypes.Codes.CPT, () => invoiceLine.CA_CPTExemptCode, () => invoiceLine.CA_CPTAmount, () => invoiceLine.CA_CPTRateDescription);
			AssertCalculateSingleDutyTax(DutyAndTaxTypes.Codes.CTA, () => invoiceLine.CA_CTAExemptCode, () => invoiceLine.CA_CTAAmount, () => invoiceLine.CA_CTARateDescription);
			AssertCalculateSingleDutyTax(DutyAndTaxTypes.Codes.CustomsDuty, () => invoiceLine.CA_DTYExemptCode, () => invoiceLine.CA_DTYAmount, () => invoiceLine.CA_DTYRateDescription);
			AssertCalculateSingleDutyTax(DutyAndTaxTypes.Codes.GST, () => invoiceLine.CA_GSTExemptCode, () => invoiceLine.CA_GSTAmount, () => invoiceLine.CA_GSTRateDescription);
			AssertCalculateSingleDutyTax(DutyAndTaxTypes.Codes.ExciseTax, () => invoiceLine.CA_EXSExemptCode, () => invoiceLine.CA_EXSAmount, () => invoiceLine.CA_EXSRateDescription);

			void AssertCalculateSingleDutyTax(ZString rateType, Func<ZString> acualExemptCode, Func<ZDecimal> acualAmount, Func<ZString> acualDesc)
			{
				CombineAssertions(rateType, () =>
				{
					var dty = invoiceLine.DutiesAndTaxes.AddNew(rateType);
					dty.C1_ExemptCode = SIMACodes.Codes.C20;
					dty.C1_Amount = 10m;
					dty.C1_Rate = 0.5m;
					dty.C1_RateType = "V";
					AggregatedFieldsCalculator.CalculateAggregatedFields(invoiceLine);
					AssertCalculateResult(SIMACodes.Codes.C20, 10m, "0.5V", acualExemptCode(), acualAmount(), acualDesc());

					var dty2 = invoiceLine.DutiesAndTaxes.AddNew(rateType);
					dty2.C1_ExemptCode = SIMACodes.Codes.C30;
					dty2.C1_Amount = 20m;
					dty2.C1_Rate = 1m;
					dty2.C1_RateType = "S";
					AggregatedFieldsCalculator.CalculateAggregatedFields(invoiceLine);
					AssertCalculateResult(SIMACodes.Codes.C30, 30m, "0.5V 1S", acualExemptCode(), acualAmount(), acualDesc());

					invoiceLine.DutiesAndTaxes.Delete(dty2);
					AggregatedFieldsCalculator.CalculateAggregatedFields(invoiceLine);
					AssertCalculateResult(SIMACodes.Codes.C20, 10m, "0.5V", acualExemptCode(), acualAmount(), acualDesc());

					invoiceLine.DutiesAndTaxes.Delete(dty);
					AggregatedFieldsCalculator.CalculateAggregatedFields(invoiceLine);
					AssertCalculateResult("", 0m, "", acualExemptCode(), acualAmount(), acualDesc());
				});
			}
		}

		void AssertCalculateResult(ZString expectedExemptCode, ZDecimal expectedAmount, ZString expectedDesc, ZString acualExemptCode, ZDecimal acualAmount, ZString acualDesc, string message = "")
		{
			AssertEquals(message, expectedExemptCode, acualExemptCode);
			AssertEquals(message, expectedAmount, acualAmount);
			AssertEquals(message, expectedDesc, acualDesc);
		}

		JobComInvoiceLine CreateInvoiceLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			return (JobComInvoiceLine)header.InvoiceLines.AddNew();
		}
	}
}
