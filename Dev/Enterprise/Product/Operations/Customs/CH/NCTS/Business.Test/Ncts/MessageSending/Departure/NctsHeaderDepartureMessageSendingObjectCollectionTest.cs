using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderDepartureMessageSendingObjectCollection))]
internal class NctsHeaderDepartureMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NctsHeaderDepartureMessageSendingObjectCollection>
{
	public void TestAllowNew()
	{
		AssertEquals("AllowNew", false, GetCollectionToTest().AllowNew);
	}

	protected override NctsHeaderDepartureMessageSendingObjectCollection GetCollectionToTest() => new NctsHeaderDepartureMessageSendingObjectCollection(Factory);

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		return new NctsHeaderDepartureMessageSendingObject(nctsHeader);
	}
}
