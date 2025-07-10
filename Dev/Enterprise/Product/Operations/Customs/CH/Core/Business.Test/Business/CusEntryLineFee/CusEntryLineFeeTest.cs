using System.Linq;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusEntryLineFee))]
class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
{
	public void TestChargeTypeDescription()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		var code = Core.Constants.Customs.CusEntryFeeTypes.VAT;
		entryLineFee.CF_ChargeType = code;
		AssertEquals(code, entryLineFee.ChargeTypeDescription);

		code = entryLineFee.Lookups.ChargeTypeList.GetAllCodes().First();
		var description = entryLineFee.Lookups.ChargeTypeList.GetDescriptionFromCode(code);
		entryLineFee.CF_ChargeType = code;
		AssertEquals(description, entryLineFee.ChargeTypeDescription);
	}

	public void TestIsVAT()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		entryLineFee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;
		AssertEquals(true, entryLineFee.IsVAT);

		entryLineFee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
		AssertEquals(false, entryLineFee.IsVAT);
	}

	public void TestIsDTY()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		entryLineFee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;
		AssertEquals(false, entryLineFee.IsDTY);

		entryLineFee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
		AssertEquals(true, entryLineFee.IsDTY);
	}

	public void TestIsDTYorVAT()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		entryLineFee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;
		AssertEquals(true, entryLineFee.IsDTYorVAT);

		entryLineFee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;
		AssertEquals(true, entryLineFee.IsDTYorVAT);

		entryLineFee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred;
		AssertEquals(false, entryLineFee.IsDTYorVAT);
	}

	protected virtual (JobDeclaration declaration, CusEntryLine entryLine, CusEntryLineFee entryLineFee) SetEntryLineFeeData()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var merger = new LineMerger(declaration);
		merger.DoMerge();
		var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First().MergedLines.Cast<CusEntryLine>().First();
		var entryLineFee = entryLine.Fees.AddNew();
		return (declaration, entryLine, entryLineFee);
	}
}
