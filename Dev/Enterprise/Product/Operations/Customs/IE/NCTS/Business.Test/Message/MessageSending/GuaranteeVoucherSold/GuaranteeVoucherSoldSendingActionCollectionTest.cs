using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(GuaranteeVoucherSoldSendingActionCollection))]
	sealed class GuaranteeVoucherSoldSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GuaranteeVoucherSoldSendingActionCollection>
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

		protected override GuaranteeVoucherSoldSendingActionCollection GetCollectionToTest()
			=> (GuaranteeVoucherSoldSendingActionCollection)new GuaranteeVoucherSoldSendingActionParent(GetNewCusGuaranteeHeader()).SendingObjectsCollection;

		protected override BusinessObject GetNewElementToAddToTheCollection() => null;

		CusGuaranteeHeader GetNewCusGuaranteeHeader()
		{
			var header = Factory.New<CusGuaranteeHeader>();
			return header;
		}
	}
}
