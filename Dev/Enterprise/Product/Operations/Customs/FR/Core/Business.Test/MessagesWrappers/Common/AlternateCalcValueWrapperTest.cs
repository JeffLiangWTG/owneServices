using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	class AlternateCalcValueWrapperTest : TestCaseWithFactory
	{
		public void TestMotivationFieldIfTariffByPassCodeNotBlank()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new ArticleWrapper(null, null); });
			var itemErrorCollector = new ErrorCollector();
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryLine1 = cusEntryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "5603939040";
			var entryLine2 = cusEntryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_TariffBypassCode = "E";
			invoiceLine.JI_TariffBypassReason = "test bypass";
			invoiceLine.JI_CEI = entryInstruction.PK;

			entryLine1.InvoiceLines.Add(invoiceLine);

			var wrapper = new AlternateCalcValueWrapper(entryLine1);
			AssertEquals("test bypass", wrapper.Motivation);

			invoiceLine.JI_TariffBypassCode = "";
			invoiceLine.JI_TariffBypassReason = "test bypass";
			wrapper = new AlternateCalcValueWrapper(entryLine1);
			AssertEquals(ZString.Empty, wrapper.Motivation);

			invoiceLine.JI_TariffBypassCode = "E";
			invoiceLine.JI_TariffBypassReason = "";
			wrapper = new AlternateCalcValueWrapper(entryLine1);
			AssertEquals(ZString.Empty, wrapper.Motivation);

			entryInstruction.ZG_BypassCode = FRConstants.ValuationBypassCodes.ReasonRequiredCode;
			entryInstruction.ZG_BypassReason = "";
			wrapper = new AlternateCalcValueWrapper(entryLine1);
			AssertEquals(0, itemErrorCollector.ErrorCount);
		}
	}
}
