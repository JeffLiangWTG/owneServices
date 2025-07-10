using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsCargoDescFeeCollection))]
sealed class NctsCargoDescFeeCollectionExtensionTest : NctsCargoDescFeeCollectionTest
{
	public void TestGetDutiesIncludedInMessageSending()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var nctsMovementHeader = nctsHeader.MovementHeader;
		var goodsItem = nctsMovementHeader.GoodsItems.AddNew();

		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.ImmediatePaymentInCashA);
		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentCustomsProcedureF);
		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE);

		AssertNotNull("Fees should not be null", goodsItem.Fees);
		AssertEquals("Fees count", 3, goodsItem.Fees.Count);
		AssertEquals("2 Fees should included in message sending: A and F", 2, goodsItem.Fees.IncludedInMessageSending().Count());

		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR, UniversalReferenceConstants.RefCusRateCodes.TemporaryAntiDumpingDuty);
		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR, UniversalReferenceConstants.RefCusRateCodes.TemportaryCountervailingDuty);
		AddLineFee(goodsItem, UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR);

		AssertEquals("Fees count", 6, goodsItem.Fees.Count);
		AssertEquals("4 Fees should be inclided in message sending: A, F, R(A35), R(A45)", 4, goodsItem.Fees.IncludedInMessageSending().Count());

		void AddLineFee(NctsDepartureCargoDesc nctsCargoDesc, ZString methodOfPayment, string chargeType = null)
		{
			var lineFee = nctsCargoDesc.Fees.AddNew();
			lineFee.BFE_MethodOfPayment = methodOfPayment;
			lineFee.BFE_ChargeType = chargeType;
		}
	}

	public void TestTotalTaxedAmount()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var nctsMovementHeader = nctsHeader.MovementHeader;
		var goodsItem = nctsMovementHeader.GoodsItems.AddNew();

		AssertEquals("TotalTaxedAmount", 0m, goodsItem.Fees.Cast<NctsCargoDescFee>().GetTotalTaxedAmount());

		var feeA00 = goodsItem.Fees.AddNew();
		feeA00.BFE_ChargeType = "A00";
		feeA00.BFE_ChargeAmount = 100m;

		AssertEquals("TotalTaxedAmount", 100m, goodsItem.Fees.Cast<NctsCargoDescFee>().GetTotalTaxedAmount());

		var feeB00 = goodsItem.Fees.AddNew();
		feeB00.BFE_ChargeType = "B00";
		feeB00.BFE_ChargeAmount = 200;

		AssertEquals("TotalTaxedAmount", 300m, goodsItem.Fees.Cast<NctsCargoDescFee>().GetTotalTaxedAmount());

		var feeTypeEmpty = goodsItem.Fees.AddNew();
		feeTypeEmpty.BFE_ChargeType = "";
		feeTypeEmpty.BFE_ChargeAmount = 300;

		AssertEquals("TotalTaxedAmount", 600m, goodsItem.Fees.Cast<NctsCargoDescFee>().GetTotalTaxedAmount());
	}
}
