namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Accounting.Business.ComplianceReport.TPAR;

	internal class TparRepoTparReportCreditorLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckARL_OverriddenGSTAmount()
		{
			var line = Factory.NewWithValidTestData<TparReportCreditorLine>();

			line.ARL_OverriddenTotalAmountIncludingTax = 50m;
			line.ARL_OverriddenGSTAmount = decimal.Zero;
			line.RunPreSaveValidation();
			AssertNoErrors(line.ARL_OverriddenGSTAmountInfo);

			line.ARL_OverriddenGSTAmount = 10m;
			line.RunPreSaveValidation();
			AssertNoErrors(line.ARL_OverriddenGSTAmountInfo);

			line.ARL_OverriddenGSTAmount = -5m;
			line.RunPreSaveValidation();
			AssertHasError(line.ARL_OverriddenGSTAmountInfo, "Ovr. Tax Amt cannot be negative.");

			line.ARL_OverriddenGSTAmount = 50m;
			line.RunPreSaveValidation();
			AssertNoErrors(line.ARL_OverriddenGSTAmountInfo);

			line.ARL_OverriddenGSTAmount = 60m;
			line.RunPreSaveValidation();
			AssertHasError(line.ARL_OverriddenGSTAmountInfo, "The Ovr. Tax Amt must be less than or equal to the Ovr. Total Amt.");

			line.ARL_OverriddenGSTAmount = 111111111111m;
			line.RunPreSaveValidation();
			AssertHasError(line.ARL_OverriddenGSTAmountInfo, @"The number 111,111,111,111 is too large, the maximum value allowed for Ovr. Tax Amt is 99,999,999,999.");
		}

		public void TestCheckARL_OverriddenTotalAmountIncludingTax()
		{
			var line = Factory.NewWithValidTestData<TparReportCreditorLine>();

			line.ARL_OverriddenTotalAmountIncludingTax = decimal.Zero;
			line.RunPreSaveValidation();
			AssertNoErrors(line.ARL_OverriddenTotalAmountIncludingTaxInfo);

			line.ARL_OverriddenTotalAmountIncludingTax = 10m;
			line.RunPreSaveValidation();
			AssertNoErrors(line.ARL_OverriddenTotalAmountIncludingTaxInfo);

			line.ARL_OverriddenTotalAmountIncludingTax = -5m;
			line.RunPreSaveValidation();
			AssertHasError(line.ARL_OverriddenTotalAmountIncludingTaxInfo, "Ovr. Total Amt cannot be negative.");

			line.ARL_OverriddenTotalAmountIncludingTax = 111111111111m;
			line.RunPreSaveValidation();
			AssertHasError(line.ARL_OverriddenTotalAmountIncludingTaxInfo, "The number 111,111,111,111 is too large, the maximum value allowed for Ovr. Total Amt is 99,999,999,999.");
		}

		public void TestCheckARL_OverriddenPaymentsBasisWithholdingTaxAmount()
		{
			var line = Factory.NewWithValidTestData<TparReportCreditorLine>();

			line.ARL_OverriddenTotalAmountIncludingTax = 50m;
			line.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = decimal.Zero;
			line.RunPreSaveValidation();
			AssertNoErrors(line.ARL_OverriddenPaymentsBasisWithholdingTaxAmountInfo);

			line.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = 10m;
			line.RunPreSaveValidation();
			AssertNoErrors(line.ARL_OverriddenPaymentsBasisWithholdingTaxAmountInfo);

			line.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = -5m;
			line.RunPreSaveValidation();
			AssertHasError(line.ARL_OverriddenPaymentsBasisWithholdingTaxAmountInfo, "Ovr. WHT Amt cannot be negative.");

			line.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = 50m;
			line.RunPreSaveValidation();
			AssertNoErrors(line.ARL_OverriddenPaymentsBasisWithholdingTaxAmountInfo);

			line.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = 60m;
			line.RunPreSaveValidation();
			AssertHasError(line.ARL_OverriddenPaymentsBasisWithholdingTaxAmountInfo, "The Ovr. WHT Amt must be less than or equal to the Ovr. Total Amt.");

			line.ARL_OverriddenPaymentsBasisWithholdingTaxAmount = 111111111111m;
			line.RunPreSaveValidation();
			AssertHasError(line.ARL_OverriddenPaymentsBasisWithholdingTaxAmountInfo, @"The number 111,111,111,111 is too large, the maximum value allowed for Ovr. WHT Amt is 99,999,999,999.");
		}
	}
}