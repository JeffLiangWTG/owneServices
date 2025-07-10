using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class AddInfoJobComInvoiceLineValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestClearTariffBypsassReasonOnTariffBypassCodeChange()
		{
			var dec = Factory.New<JobDeclaration>();

			dec.JE_MessageType = "IMP";

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			invoiceLine.JI_TariffBypassCode = FRConstants.TariffBypassCodes.ReasonEnabledCode;
			invoiceLine.JI_TariffBypassReason = "Unit Testing";

			AssertEquals("Reason should be populated", "Unit Testing", invoiceLine.JI_TariffBypassReason);

			invoiceLine.JI_TariffBypassCode = TariffBypassCodeList.Codes.TariffBypass_D;
			AssertEquals("Reason should not be empty", TariffBypassCodeList.Descriptions.TariffBypass_D, invoiceLine.JI_TariffBypassReason);
		}

		public void TestTariffBypassCodePropogationToTaxLine()
		{
			var dec = Factory.New<JobDeclaration>();

			dec.JE_MessageType = "IMP";

			var invoice1 = dec.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			var taxLine1 = invoiceLine1.Taxes.AddNew();
			var taxLine2 = invoiceLine1.Taxes.AddNew();
			var invoiceLine2 = invoice1.InvoiceLines.AddNew();
			var taxLine3 = invoiceLine2.Taxes.AddNew();

			invoiceLine1.JI_TariffBypassCode = "X";
			invoiceLine2.JI_TariffBypassCode = "Y";

			AssertEquals("TariffBypassCode1 should be X", "X", taxLine1.TariffBypassCode);
			AssertEquals("TariffBypassCode1 should be X", "X", taxLine2.TariffBypassCode);
			AssertEquals("TariffBypassCode1 should be Y", "Y", taxLine3.TariffBypassCode);

			invoiceLine1.JI_TariffBypassCode = FRConstants.TariffBypassCodes.ReasonEnabledCode;

			AssertEquals("Tax line TariffBypassCode should be same as invoice line TariffBypassCode", FRConstants.TariffBypassCodes.ReasonEnabledCode, taxLine1.TariffBypassCode);
			AssertEquals("Tax line TariffBypassCode should be same as TariffBypassCode", FRConstants.TariffBypassCodes.ReasonEnabledCode, taxLine2.TariffBypassCode);
			AssertEquals("Tax line TariffBypassCode should still be Y", "Y", taxLine3.TariffBypassCode);

			invoiceLine2.JI_TariffBypassCode = "D";

			AssertEquals("Tax line TariffBypassCode should now be D", "D", taxLine3.TariffBypassCode);
		}
	}
}
