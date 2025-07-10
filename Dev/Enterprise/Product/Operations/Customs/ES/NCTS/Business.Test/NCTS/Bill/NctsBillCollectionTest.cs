using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsBillCollection))]
sealed class NctsBillCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsBillCollection>
{
	public void TestAllowNew() => CombineAssertions(() =>
	{
		var movement = nctsHeader.ArrivalMovementHeader;
		movement.BM_NoChangesToReport = false;
		AssertEquals("Prereq: NoChanges equals to UnloadingDifferenceDataReadOnly", expected: false, movement.UnloadingDifferenceDataReadOnly);
		AssertEquals("Allow new when NoChangesToReport is false", expected: true, nctsHeader.Bills.AllowNew);

		movement.BM_NoChangesToReport = true;
		AssertEquals("Prereq: NoChanges equals to UnloadingDifferenceDataReadOnly", expected: true, movement.UnloadingDifferenceDataReadOnly);
		AssertEquals("Not Allow new when NoChangesToReport is true", expected: false, nctsHeader.Bills.AllowNew);

		movement.BM_NoChangesToReport = false;
		nctsHeader.EffectiveMessageStatus = Common.EU.LogicalStatusList.Codes.Sent;
		AssertEquals("Prereq: NoChanges equals to UnloadingDifferenceDataReadOnly", expected: false, movement.UnloadingDifferenceDataReadOnly);
		AssertEquals("Not Allow new when header is sent", expected: false, nctsHeader.Bills.AllowNew);
	});

	protected override NctsBillCollection GetCollectionToTest() => (NctsBillCollection)nctsHeader.Bills;

	protected override void SetUp()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
	}
	NctsHeader nctsHeader;
}
