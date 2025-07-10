using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsCargoDescFeeCollection))]
abstract class NctsCargoDescFeeCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsCargoDescFeeCollection>
{
	public void TestGetTotalAmount()
	{
		var fees = new NctsCargoDescFeeCollection(goodsItem);
		AssertEquals(nameof(fees.GetTotalAmount), 0.00m, fees.GetTotalAmount());

		fees.AddNew().BFE_ChargeAmount = 10m;
		fees.AddNew().BFE_ChargeAmount = 12.99m;
		AssertEquals(nameof(fees.GetTotalAmount), 22.99m, fees.GetTotalAmount());
	}

	public void TestGetTotalAmountForMethodOfPayment()
	{
		var feeCollection = GetCollectionToTest();
		var fee1 = feeCollection.AddNew();
		fee1.BFE_MethodOfPayment = "A";
		fee1.BFE_ChargeAmount = 120m;
		var fee2 = feeCollection.AddNew();
		fee2.BFE_MethodOfPayment = "B";
		fee2.BFE_ChargeAmount = 11.22m;

		CombineAssertions(() =>
		{
			AssertEquals("BFE_MethodOfPayment = 'A'", 120m, feeCollection.GetTotalAmount("A"));
			AssertEquals("BFE_MethodOfPayment = 'B'", 11.22m, feeCollection.GetTotalAmount("B"));
			AssertEquals("BFE_MethodOfPayment = 'Z'", 0m, feeCollection.GetTotalAmount("Z"));
		});
	}

	public void TestSetDefaultsForNewChild()
	{
		var feeCollection = new NctsCargoDescFeeCollection(goodsItem);

		var fee = feeCollection.AddNew();
		AssertEquals($"When a new fee has been added to collection, {nameof(NctsCargoDescFee.BFE_RateOverrideReasonCode)}", "ADD", fee.BFE_RateOverrideReasonCode);

		AssertEquals($"Default {nameof(NctsCargoDescFee.BFE_RateOverrideReasonCode)}", "", Factory.New<NctsCargoDescFee>().BFE_RateOverrideReasonCode);
	}

	protected override void SetUp()
	{
		base.SetUp();

		goodsItem = Factory.New<NctsDepartureCargoDesc>();
	}
	NctsDepartureCargoDesc goodsItem;

	protected override NctsCargoDescFeeCollection GetCollectionToTest() => new NctsCargoDescFeeCollection(Factory.New<NctsDepartureCargoDesc>());
}
