using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderArrivalMessageSendingObjectCollection))]
internal class NctsHeaderArrivalMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NctsHeaderArrivalMessageSendingObjectCollection>
{
	public void TestAllowNew()
	{
		AssertEquals("AllowNew", false, GetCollectionToTest().AllowNew);
	}

	protected override NctsHeaderArrivalMessageSendingObjectCollection GetCollectionToTest() => new NctsHeaderArrivalMessageSendingObjectCollection(Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		return new NctsHeaderArrivalMessageSendingObject(nctsHeader);
	}
}
