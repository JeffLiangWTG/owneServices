using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class SiscomexUsageEntryFeeCalculatorTest : TestCaseWithFactory
	{
		public void TestUpdateSiscomexUsageFeeOnEntryLines()
		{
			ReferenceTestDataHelper.CreateSiscomexUsageEntryFees(Factory);

			var chargeType = Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee;

			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;

			CombineAssertions(() =>
			{
				cusEntryHeader.MergedLines.First().CL_CustomsValue = 600m;
				cusEntryHeader.MergedLines.AddNew().CL_CustomsValue = 500m;

				var calculator = new SiscomexUsageEntryFeeCalculator(cusEntryHeader);
				AssertEquals("Total SUF Fee for 2 lines", 192.79m, calculator.CalculateTotalFee());
				calculator.UpdateFeeOnEntryLines();

				AssertContainsExactElementsInExactOrder(new ZDecimal[] { 105.16m, 87.63m }, cusEntryHeader.MergedLines.Select(x => x.Fees.GetAmount(chargeType)));

				cusEntryHeader.MergedLines.AddNew().CL_CustomsValue = 400m;

				calculator = new SiscomexUsageEntryFeeCalculator(cusEntryHeader);
				AssertEquals("Total SUF Fee for 3 lines", 223.64m, calculator.CalculateTotalFee());
				calculator.UpdateFeeOnEntryLines();
				AssertContainsExactElementsInExactOrder(new ZDecimal[] { 89.45m, 74.55m, 59.64m }, cusEntryHeader.MergedLines.Select(x => x.Fees.GetAmount(chargeType)));

				cusEntryHeader.MergedLines.AddNew().CL_CustomsValue = 400m;
				cusEntryHeader.MergedLines.AddNew().CL_CustomsValue = 300m;

				calculator = new SiscomexUsageEntryFeeCalculator(cusEntryHeader);
				AssertEquals("Total SUF Fee for 5 lines", 285.34m, calculator.CalculateTotalFee());
				calculator.UpdateFeeOnEntryLines();
				AssertContainsExactElementsInExactOrder(new ZDecimal[] { 77.82m, 64.85m, 51.88m, 51.88m, 38.91m }, cusEntryHeader.MergedLines.Select(x => x.Fees.GetAmount(chargeType)));

				cusEntryHeader.MergedLines.AddNew().CL_CustomsValue = 200m;
				cusEntryHeader.MergedLines.AddNew().CL_CustomsValue = 300m;
				cusEntryHeader.MergedLines.AddNew().CL_CustomsValue = 100m;
				cusEntryHeader.MergedLines.AddNew().CL_CustomsValue = 200m;
				cusEntryHeader.MergedLines.AddNew().CL_CustomsValue = 300m;
				cusEntryHeader.MergedLines.AddNew().CL_CustomsValue = 100m;

				calculator = new SiscomexUsageEntryFeeCalculator(cusEntryHeader);
				AssertEquals("Total SUF Fee for 11 lines", 416.46m, calculator.CalculateTotalFee());
				calculator.UpdateFeeOnEntryLines();
				AssertContainsExactElementsInExactOrder(new ZDecimal[] { 73.47m, 61.24m, 49.00m, 49.00m, 36.75m, 24.50m, 36.75m, 12.25m, 24.50m, 36.75m, 12.25m }, cusEntryHeader.MergedLines.Select(x => x.Fees.GetAmount(chargeType)));

				foreach (var entryLine in cusEntryHeader.MergedLines)
				{
					entryLine.CL_CustomsValue = 0;
				}
				calculator = new SiscomexUsageEntryFeeCalculator(cusEntryHeader);
				AssertEquals("Total SUF Fee for 11 lines", 416.46m, calculator.CalculateTotalFee());
				calculator.UpdateFeeOnEntryLines();
				AssertContainsExactElementsInExactOrder(new ZDecimal[] { 416.46m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m }, cusEntryHeader.MergedLines.Select(x => x.Fees.GetAmount(chargeType)));
			});
		}

		public void TestUpdateSiscomexUsageFeeOnSUFEntryHeaders()
		{
			ReferenceTestDataHelper.CreateSiscomexUsageEntryFees(Factory);

			var chargeType = Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee;

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var sufEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			sufEntryHeader.CH_MessageType = MessageTypeList.Codes.SUF;
			sufEntryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var formalEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			formalEntryHeader.CH_MessageType = MessageTypeList.Codes.CIH;
			formalEntryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = sufEntryHeader.MergedLines.AddNew().PK;

			CombineAssertions(() =>
			{
				sufEntryHeader.MergedLines.First().CL_CustomsValue = 600m;
				sufEntryHeader.MergedLines.AddNew().CL_CustomsValue = 500m;

				formalEntryHeader.MergedLines.AddNew().CL_CustomsValue = 400m;
				formalEntryHeader.MergedLines.AddNew().CL_CustomsValue = 200m;
				formalEntryHeader.MergedLines.AddNew().CL_CustomsValue = 500m;

				var calculator = new SiscomexUsageEntryFeeCalculator(sufEntryHeader);
				AssertEquals("Total SUF Fee for 2 lines", 192.79m, calculator.CalculateTotalFee());
				calculator.UpdateFeeOnEntryLines();

				AssertContainsExactElementsInExactOrder(new ZDecimal[] { 105.16m, 87.63m }, sufEntryHeader.MergedLines.Select(x => x.Fees.GetAmount(chargeType)));

				AssertContainsExactElementsInExactOrder(new ZDecimal[] { 70.11m, 35.05m, 87.63m }, formalEntryHeader.MergedLines.Select(x => x.Fees.GetAmount(chargeType)));
			});
		}

		public void TestGetRefCusTaxOrFeeList()
		{
			ReferenceTestDataHelper.CreateSiscomexUsageEntryFees(Factory);

			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryHeader.MergedLines.AddNew().PK;

			var calculator = new SiscomexUsageEntryFeeCalculator(entryHeader);
			var realData1 = calculator.GetRefCusTaxOrFee(2);
			var realData2 = calculator.GetRefCusTaxOrFee(4);
			var realData3 = calculator.GetRefCusTaxOrFee(7);
			var realData4 = calculator.GetRefCusTaxOrFee(11);
			var realData5 = calculator.GetRefCusTaxOrFee(20);
			var realData6 = calculator.GetRefCusTaxOrFee(21);
			var realData7 = calculator.GetRefCusTaxOrFee(95);
			CombineAssertions(() =>
			{
				AssertEquals("ZZF_Code should be", "ENT1", realData1.ZZF_Code);
				AssertEquals("ZZF_Code should be", "ENT2", realData2.ZZF_Code);
				AssertEquals("ZZF_Code should be", "ENT3", realData3.ZZF_Code);
				AssertEquals("ZZF_Code should be", "ENT4", realData4.ZZF_Code);
				AssertEquals("ZZF_Code should be", "ENT4", realData5.ZZF_Code);
				AssertEquals("ZZF_Code should be", "ENT5", realData6.ZZF_Code);
				AssertEquals("ZZF_Code should be", "ENT6", realData7.ZZF_Code);
			});
		}
	}
}
