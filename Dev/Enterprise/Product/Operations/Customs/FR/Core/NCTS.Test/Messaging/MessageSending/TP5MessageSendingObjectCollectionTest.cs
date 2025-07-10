using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	[TestedType(typeof(TP5MessageSendingObjectCollection))]
	internal class TP5MessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TP5MessageSendingObjectCollection>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TP5MessageSendingObjectCollection(null));
		}

		public void TestAddMessageSendingActions_Departure()
		{
			var collection = new TP5MessageSendingObjectCollection(nctsHeader);
			AssertType<FR.Business.NCTS.NctsDepartureMovementHeader>(collection[0].NctsHeader.MovementHeader);
		}

		public void TestAddMessageSendingActions_Arrival()
		{
			var nctsHeader = Factory.New<FR.Business.NCTS.NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var collection = new TP5MessageSendingObjectCollection(nctsHeader);
			AssertType<FR.Business.NCTS.NctsArrivalMovementHeader>(collection[0].NctsHeader.ArrivalMovementHeader);
		}

		public override void TestAdd()
		{
			Assert("MessageSendingActionCollection does not support adding.", true);
		}

		public override void TestDelete()
		{
			Assert("MessageSendingActionCollection does not support deleting.", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("MessageSendingActionCollection does not support RemoveFromRelationship.", true);
		}

		protected override TP5MessageSendingObjectCollection GetCollectionToTest() => new TP5MessageSendingObjectCollection(nctsHeader);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new TP5MessageSendingObjectParent(nctsHeader).SendingObjectsCollection.AddNew();

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<FR.Business.NCTS.NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}

		FR.Business.NCTS.NctsHeader nctsHeader;
	}
}
