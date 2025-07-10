using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsCargoDescFee))]
sealed class NctsCargoDescFeeTest : EnterpriseBusinessObjectTestCase
{
	public void TestBFE_ChargeAmount_RecalculateLiabilityAmount()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

		var arrivalMovement = nctsHeader.ArrivalMovementHeader;
		var guarantee = arrivalMovement.SingleGuaranteeForArrival;
		AssertEquals("Initial Liability", (ZDecimal)0, guarantee.LiabilityAmount);

		var bill = nctsHeader.Bills.AddNew();
		var goodsItem = bill.ArrivalGoodsItems.AddNew();
		var dutyAmount = goodsItem.Fees.AddNew();
		dutyAmount.BFE_ChargeType = EU.NCTS.Business.NctsCommonCargoDesc.ChargeType.Duty;
		dutyAmount.BFE_ChargeAmount = 1;
		var antiDumpingDutyAmount = goodsItem.Fees.AddNew();
		antiDumpingDutyAmount.BFE_ChargeType = EU.NCTS.Business.NctsCommonCargoDesc.ChargeType.AntiDumpingDuty;
		antiDumpingDutyAmount.BFE_ChargeAmount = 1.5;
		var countervailingDutyAmount = goodsItem.Fees.AddNew();
		countervailingDutyAmount.BFE_ChargeType = EU.NCTS.Business.NctsCommonCargoDesc.ChargeType.CountervailingDuty;
		countervailingDutyAmount.BFE_ChargeAmount = 2.3;

		AssertEquals("Liability is calculated without saving", (ZDecimal)4.8, guarantee.LiabilityAmount);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewNctsCargoDescFee(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewNctsCargoDescFee(factory);

	NctsCargoDescFee GetNewNctsCargoDescFee(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var goodsItem = header.Bills.AddNew().ArrivalGoodsItems.AddNew();
		var fee = (NctsCargoDescFee)goodsItem.Fees.AddNew();
		fee.BFE_MethodOfPayment = "A";
		fee.BFE_MethodOfCalculation = "A";
		return fee;
	}
}
