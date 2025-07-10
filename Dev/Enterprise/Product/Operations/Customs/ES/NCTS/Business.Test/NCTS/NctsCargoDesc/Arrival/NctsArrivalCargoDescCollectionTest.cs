using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalCargoDescCollection))]
sealed class NctsArrivalCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsArrivalCargoDescCollection>
{
	public void TestAllowNew() => CombineAssertions(() =>
	{
		var movement = nctsHeader.ArrivalMovementHeader;
		movement.BM_NoChangesToReport = false;
		AssertEquals("Prereq: NoChanges equals to UnloadingDifferenceDataReadOnly", expected: false, movement.UnloadingDifferenceDataReadOnly);
		AssertEquals("Allow new when NoChangesToReport is false", expected: true, bill.ArrivalGoodsItems.AllowNew);

		movement.BM_NoChangesToReport = true;
		AssertEquals("Prereq: NoChanges equals to UnloadingDifferenceDataReadOnly", expected: true, movement.UnloadingDifferenceDataReadOnly);
		AssertEquals("Not Allow new when NoChangesToReport is true", expected: false, bill.ArrivalGoodsItems.AllowNew);

		movement.BM_NoChangesToReport = false;
		nctsHeader.EffectiveMessageStatus = Common.EU.LogicalStatusList.Codes.Sent;
		AssertEquals("Prereq: NoChanges equals to UnloadingDifferenceDataReadOnly", expected: false, movement.UnloadingDifferenceDataReadOnly);
		AssertEquals("Not Allow new when header is sent", expected: false, bill.ArrivalGoodsItems.AllowNew);
	});

	protected override NctsArrivalCargoDescCollection GetCollectionToTest() => (NctsArrivalCargoDescCollection)bill.ArrivalGoodsItems;

	protected override void SetUp()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		bill = nctsHeader.Bills.AddNew();
	}
	NctsHeader nctsHeader;
	NctsBill bill;
}
