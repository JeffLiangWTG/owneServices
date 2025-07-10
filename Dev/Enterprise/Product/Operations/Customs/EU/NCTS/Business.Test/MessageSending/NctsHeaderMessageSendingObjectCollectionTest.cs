using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderMessageSendingObjectCollection))]
	internal class NctsHeaderMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NctsHeaderMessageSendingObjectCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, GetCollectionToTest().AllowNew);
		}

		protected override NctsHeaderMessageSendingObjectCollection GetCollectionToTest() => new NctsHeaderMessageSendingObjectCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			return new NctsHeaderMessageSendingObject(nctsHeader);
		}
	}
}
