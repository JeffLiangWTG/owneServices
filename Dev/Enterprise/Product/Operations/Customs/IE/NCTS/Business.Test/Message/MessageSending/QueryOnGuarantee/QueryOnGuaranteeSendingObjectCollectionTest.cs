using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(QueryOnGuaranteeSendingObjectCollection))]
	sealed class QueryOnGuaranteeSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<QueryOnGuaranteeSendingObjectCollection>
	{
		public override void TestDelete()
		{
			Assert("Not supporting deleting, this check is not required.", true);
		}

		public override void TestAdd()
		{
			Assert("Not supporting adding, this check is not required.", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("Not supporting removing, this check is not required.", true);
		}

		protected override QueryOnGuaranteeSendingObjectCollection GetCollectionToTest()
			=> new QueryOnGuaranteeSendingAction(GetNewNctsHeader()).AllGuarantees;

		protected override BusinessObject GetNewElementToAddToTheCollection() => null;

		NctsHeader GetNewNctsHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			_ = header.MovementHeader.Guarantees.AddNew();
			_ = header.MovementHeader.Guarantees.AddNew();
			return header;
		}
	}
}
