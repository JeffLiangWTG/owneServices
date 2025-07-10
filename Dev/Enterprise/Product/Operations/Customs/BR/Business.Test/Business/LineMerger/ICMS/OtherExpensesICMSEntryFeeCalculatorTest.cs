using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class OtherExpensesICMSFCPFeeCalculatorTest : TestCaseWithFactory
	{
		public void TestUpdateOtherExpensesICMSFeeOnEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = cusEntryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			var calculator = new OtherExpensesICMSFCPFeeCalculator(entryLine);

			calculator.UpdateOtherExpensesICMSFeeOnEntryLine();

			AssertNull("No EIC fee added", entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.OtherExpensesICMS));

			invoiceLine1.Charges.AddNew(ImportChargesProvider.OtherExpensesICMS.Code, 1000m, Core.Constants.CurrencyCodes.Brazil);
			invoiceLine1.Charges.AddNew(ImportChargesProvider.OtherExpensesICMS.Code, 1000m, Core.Constants.CurrencyCodes.Brazil);
			invoiceLine2.Charges.AddNew(ImportChargesProvider.OtherExpensesICMS.Code, 1000m, Core.Constants.CurrencyCodes.Brazil);
			invoiceLine2.Charges.AddNew(ImportChargesProvider.OtherExpensesICMS.Code, 1000m, Core.Constants.CurrencyCodes.Brazil);

			calculator.UpdateOtherExpensesICMSFeeOnEntryLine();
			var fee = entryLine.Fees.GetElementWithThisCode(Constants.RateTypes.OtherExpensesICMS);
			AssertEquals("CF_ChargeAmount", 4000m, fee.CF_ChargeAmount);
			AssertEquals("CF_BaseValue", ZDecimal.Zero, fee.CF_BaseValue);
			AssertEquals("CF_Rate", ZDecimal.Zero, fee.CF_Rate);
			AssertEquals("CF_MethodOfCalculation", ZString.Empty, fee.CF_MethodOfCalculation);
		}
	}
}
