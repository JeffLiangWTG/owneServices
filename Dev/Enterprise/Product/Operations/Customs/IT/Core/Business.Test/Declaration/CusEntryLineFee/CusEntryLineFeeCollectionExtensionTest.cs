using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFeeCollection))]
sealed class CusEntryLineFeeCollectionExtensionTest : CusEntryLineFeeCollectionTest
{
	public void TestGetInCustomsCompliantOrder()
	{
		var entryLine = Factory.New<CusEntryLine>();
		SetUpFees(entryLine.Fees, "407", "406", "405", "927", "911", "201", "165", "116", "A35", "A30", "A20", "A10", "A00");
		AssertEquals("entryLine.Fees count", 13, entryLine.Fees.Count);

		var feesInCustomsCompliantOrder = entryLine.Fees.Cast<CusEntryLineFee>().InCustomsCompliantOrder();
		AssertNotNull("feesInCustomsCompliantOrder not null", feesInCustomsCompliantOrder);
		AssertEquals("feesInCustomsCompliantOrder count", 13, feesInCustomsCompliantOrder.Count());
		AssertArrayEqualsByElements("feesInCustomsCompliantOrder order", new ZString[] { "A00", "A10", "A20", "A30", "A35", "116", "165", "201", "911", "927", "405", "406", "407" }, feesInCustomsCompliantOrder.Select(x => x.CF_ChargeType).ToArray());
	}

	public void TestGetDutiesIncludedInMessageSending()
	{
		var cusEntryLine = Factory.New<CusEntryLine>();

		AddLineFee(cusEntryLine, UniversalReferenceConstants.DutyMethodOfPayment.ImmediatePaymentInCashA);
		AddLineFee(cusEntryLine, UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentCustomsProcedureF);
		AddLineFee(cusEntryLine, UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE);

		AssertNotNull("Fees should not be null", cusEntryLine.Fees);
		AssertEquals("Fees count", 3, cusEntryLine.Fees.Count);
		AssertEquals("2 Fees should included in message sending: A and F", 2, cusEntryLine.Fees.IncludedInMessageSending().Count());

		AddLineFee(cusEntryLine, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR, UniversalReferenceConstants.RefCusRateCodes.TemporaryAntiDumpingDuty);
		AddLineFee(cusEntryLine, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR, UniversalReferenceConstants.RefCusRateCodes.TemportaryCountervailingDuty);
		AddLineFee(cusEntryLine, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR);

		AssertEquals("Fees count", 6, cusEntryLine.Fees.Count);
		AssertEquals("4 Fees should be included in message sending: A, F, R(A35), R(A45)", 4, cusEntryLine.Fees.IncludedInMessageSending().Count());

		void AddLineFee(CusEntryLine entryLine, ZString methodOfPayment, string chargeType = null)
		{
			var lineFee = entryLine.Fees.AddNew();
			lineFee.CF_MethodOfPayment = methodOfPayment;
			lineFee.CF_ChargeType = chargeType;
		}
	}

	public void TestTotalTaxedAmount()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		AssertEquals("TotalTaxedAmount", 0m, entryLine.Fees.Cast<CusEntryLineFee>().GetTotalTaxedAmount());

		var feeA00 = entryLine.Fees.AddNew();
		feeA00.CF_ChargeType = "A00";
		feeA00.CF_ChargeAmount = 100m;

		AssertEquals("TotalTaxedAmount", 100m, entryLine.Fees.Cast<CusEntryLineFee>().GetTotalTaxedAmount());

		var feeB00 = entryLine.Fees.AddNew();
		feeB00.CF_ChargeType = "B00";
		feeB00.CF_ChargeAmount = 200;

		AssertEquals("TotalTaxedAmount", 300m, entryLine.Fees.Cast<CusEntryLineFee>().GetTotalTaxedAmount());

		var feeTypeEmpty = entryLine.Fees.AddNew();
		feeTypeEmpty.CF_ChargeType = "";
		feeTypeEmpty.CF_ChargeAmount = 300;

		AssertEquals("TotalTaxedAmount", 600m, entryLine.Fees.Cast<CusEntryLineFee>().GetTotalTaxedAmount());
	}
}
