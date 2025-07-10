using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class UOMDefaulterTest : TestCaseWithFactory
	{
		public void TestUOMAllocationAfterDutyCalculation_NoUnitToAllocate()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var entryLine = (CusEntryLine)entryHeader.MergedLines.AddNew();
			var invLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			invLine.JI_CustomsQuantity = 20;
			invLine.JI_CustomsUnitQty = "TNE";

			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = "0A7";
			fee.CF_MethodOfCalculation = "PVP";

			CombineAssertions(() =>
			{
				var fees = entryLine.Fees.OfType<CusEntryLineFee>();
				UOMDefaulter.DefaultUOMIfApplicable(declaration);

				AssertEquals("There is 1 fee", 1, fees.Count());
				AssertEquals("0A7 fee has PVP as Method of Calculation", "PVP", fees.First(x => x.CF_ChargeType == "0A7").CF_MethodOfCalculation);
				AssertEquals("ThirdQtyUnit is not set to the fee's method of calculation when it is PVP", ZString.Empty, invLine.JI_CustomsThirdUnitQty);
				AssertEquals("FourthQtyUnit is not set to the fee's method of calculation when it is PVP", ZString.Empty, invLine.JI_CustomsFourthUnitQty);

				fee.CF_ChargeType = "0A1";
				fee.CF_MethodOfCalculation = "%";
				fees = entryLine.Fees.OfType<CusEntryLineFee>();
				UOMDefaulter.DefaultUOMIfApplicable(declaration);

				AssertEquals("There is 1 fee", 1, fees.Count());
				AssertEquals("0A1 fee has PVP as Method of Calculation", "%", fees.First(x => x.CF_ChargeType == "0A1").CF_MethodOfCalculation);
				AssertEquals("ThirdQtyUnit is not set to the fee's method of calculation when it is %", ZString.Empty, invLine.JI_CustomsThirdUnitQty);
				AssertEquals("FourthQtyUnit is not set to the fee's method of calculation when it is %", ZString.Empty, invLine.JI_CustomsFourthUnitQty);
			});
		}

		public void TestUOMAllocationAfterDutyCalculation_OneInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var entryLine = (CusEntryLine)entryHeader.MergedLines.AddNew();
			var invLine = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			invLine.JI_CustomsQuantity = 20;
			invLine.JI_CustomsUnitQty = "TNE";

			SetFees(entryLine);

			CombineAssertions(() =>
			{
				var fees = entryLine.Fees.OfType<CusEntryLineFee>();
				AssertEquals("There are 2 fees", 2, fees.Count());
				AssertEquals("0A0 fee has HG as Method of Calculation", "HG", fees.First(x => x.CF_ChargeType == "0A0").CF_MethodOfCalculation);
				AssertEquals("5A0 fee has ASVX as Method of Calculation", "ASVX", fees.First(x => x.CF_ChargeType == "5A0").CF_MethodOfCalculation);

				UOMDefaulter.DefaultUOMIfApplicable(declaration);

				AssertEquals("ThirdQtyUnit is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine.JI_CustomsThirdUnitQty);
				AssertEquals("FourthQtyUnit is set to the second fee's method of calculation when that unit or it's conversion is not present in any qty unit", "ASVX", invLine.JI_CustomsFourthUnitQty);

				invLine.JI_CustomsUnitQty = "LPA";
				invLine.JI_CustomsThirdUnitQty = ZString.Empty;
				invLine.JI_CustomsFourthUnitQty = ZString.Empty;

				UOMDefaulter.DefaultUOMIfApplicable(declaration);

				AssertEquals("ThirdQtyUnit is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine.JI_CustomsThirdUnitQty);
				AssertEquals("FourthQtyUnit is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine.JI_CustomsFourthUnitQty);

				invLine.JI_CustomsUnitQty = "LPA";
				invLine.JI_CustomsSecondUnitQty = "HG";
				invLine.JI_CustomsThirdUnitQty = ZString.Empty;
				invLine.JI_CustomsFourthUnitQty = ZString.Empty;

				UOMDefaulter.DefaultUOMIfApplicable(declaration);

				AssertEquals("ThirdQtyUnit is not set to the first fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine.JI_CustomsThirdUnitQty);
				AssertEquals("FourthQtyUnit is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine.JI_CustomsFourthUnitQty);

				invLine.JI_CustomsUnitQty = "TNE";
				invLine.JI_CustomsSecondUnitQty = ZString.Empty;
				invLine.JI_CustomsThirdUnitQty = "TNE";
				invLine.JI_CustomsFourthUnitQty = ZString.Empty;

				UOMDefaulter.DefaultUOMIfApplicable(declaration);

				AssertEquals("ThirdQtyUnit is filled so it is not set to the first fee's method of calculation", "TNE", invLine.JI_CustomsThirdUnitQty);
				AssertEquals("FourthQtyUnit is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine.JI_CustomsFourthUnitQty);
			});
		}

		public void TestUOMAllocationAfterDutyCalculation_MultipleInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var entryLine = (CusEntryLine)entryHeader.MergedLines.AddNew();
			var invLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_CustomsQuantity = 20;
			invLine1.JI_CustomsUnitQty = "TNE";

			var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine2.JI_CEI = entryInstruction.PK;
			invLine2.JI_CustomsQuantity = 30;
			invLine2.JI_CustomsUnitQty = "TNE";

			SetFees(entryLine);

			CombineAssertions(() =>
			{
				var fees = entryLine.Fees.OfType<CusEntryLineFee>();
				AssertEquals("There are 2 fees", 2, fees.Count());
				AssertEquals("0A0 fee has HG as Method of Calculation", "HG", fees.First(x => x.CF_ChargeType == "0A0").CF_MethodOfCalculation);
				AssertEquals("5A0 fee has ASVX as Method of Calculation", "ASVX", fees.First(x => x.CF_ChargeType == "5A0").CF_MethodOfCalculation);

				UOMDefaulter.DefaultUOMIfApplicable(declaration);

				AssertEquals("ThirdQtyUnit in first invoice line is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine1.JI_CustomsThirdUnitQty);
				AssertEquals("ThirdQtyUnit in second invoice line is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine2.JI_CustomsThirdUnitQty);

				AssertEquals("FourthQtyUnit in first invoice line is set to the second fee's method of calculation when that unit or it's conversion is not present in any qty unit", "ASVX", invLine1.JI_CustomsFourthUnitQty);
				AssertEquals("FourthQtyUnit in second invoice line is set to the second fee's method of calculation when that unit or it's conversion is not present in any qty unit", "ASVX", invLine2.JI_CustomsFourthUnitQty);

				invLine1.JI_CustomsUnitQty = "LPA";
				invLine1.JI_CustomsThirdUnitQty = ZString.Empty;
				invLine1.JI_CustomsFourthUnitQty = ZString.Empty;

				invLine2.JI_CustomsUnitQty = "LPA";
				invLine2.JI_CustomsThirdUnitQty = ZString.Empty;
				invLine2.JI_CustomsFourthUnitQty = ZString.Empty;

				UOMDefaulter.DefaultUOMIfApplicable(declaration);

				AssertEquals("ThirdQtyUnit in first invoice is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine1.JI_CustomsThirdUnitQty);
				AssertEquals("ThirdQtyUnit in second invoice is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine2.JI_CustomsThirdUnitQty);

				AssertEquals("FourthQtyUnit in first invoice is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine1.JI_CustomsFourthUnitQty);
				AssertEquals("FourthQtyUnit in second invoice is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine2.JI_CustomsFourthUnitQty);

				invLine1.JI_CustomsUnitQty = "LPA";
				invLine1.JI_CustomsSecondUnitQty = "HG";
				invLine1.JI_CustomsThirdUnitQty = ZString.Empty;
				invLine1.JI_CustomsFourthUnitQty = ZString.Empty;

				invLine2.JI_CustomsUnitQty = "LPA";
				invLine2.JI_CustomsSecondUnitQty = "HG";
				invLine2.JI_CustomsThirdUnitQty = ZString.Empty;
				invLine2.JI_CustomsFourthUnitQty = ZString.Empty;

				UOMDefaulter.DefaultUOMIfApplicable(declaration);

				AssertEquals("ThirdQtyUnit in first invoice is not set to the first fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine1.JI_CustomsThirdUnitQty);
				AssertEquals("ThirdQtyUnit in second invoice is not set to the first fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine2.JI_CustomsThirdUnitQty);

				AssertEquals("FourthQtyUnit in first invoice is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine1.JI_CustomsFourthUnitQty);
				AssertEquals("FourthQtyUnit in second invoice is not set to the second fee's method of calculation when that unit or it's conversion is present in a qty unit", ZString.Empty, invLine2.JI_CustomsFourthUnitQty);

				invLine1.JI_CustomsUnitQty = "TNE";
				invLine1.JI_CustomsSecondUnitQty = ZString.Empty;
				invLine1.JI_CustomsThirdUnitQty = "TNE";
				invLine1.JI_CustomsFourthUnitQty = ZString.Empty;

				invLine2.JI_CustomsUnitQty = "TNE";
				invLine2.JI_CustomsSecondUnitQty = ZString.Empty;
				invLine2.JI_CustomsThirdUnitQty = "TNE";
				invLine2.JI_CustomsFourthUnitQty = ZString.Empty;

				UOMDefaulter.DefaultUOMIfApplicable(declaration);

				AssertEquals("ThirdQtyUnit in first invoice is filled so it is not set to the first fee's method of calculation", "TNE", invLine1.JI_CustomsThirdUnitQty);
				AssertEquals("ThirdQtyUnit in second invoice is filled so it is not set to the first fee's method of calculation", "TNE", invLine2.JI_CustomsThirdUnitQty);
				AssertEquals("FourthQtyUnit in first invoice line is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine1.JI_CustomsFourthUnitQty);
				AssertEquals("FourthQtyUnit in second invoice line is set to the first fee's method of calculation when that unit or it's conversion is not present in any qty unit", "HG", invLine2.JI_CustomsFourthUnitQty);
			});
		}

		void SetFees(CusEntryLine entryLine)
		{
			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeType = "0A0";
			fee1.CF_MethodOfCalculation = "HG";

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeType = "5A0";
			fee2.CF_MethodOfCalculation = "ASVX";
		}
	}
}
