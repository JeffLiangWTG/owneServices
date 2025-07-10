using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DepositRefundApplicationMessageSendingActionCollection))]
	sealed class DepositRefundApplicationMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DepositRefundApplicationMessageSendingActionCollection>
	{
		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Not supporting adding, this check is not required.", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("Not supporting removing, this check is not required.", true);
		}

		public override void TestDelete()
		{
			Assert("Not supporting deleting, this check is not required.", true);
		}

		public override void TestAdd()
		{
			Assert("Not supporting adding, this check is not required.", true);
		}

		protected override DepositRefundApplicationMessageSendingActionCollection GetCollectionToTest()
		=> (DepositRefundApplicationMessageSendingActionCollection)new DepositRefundApplicationMessageSendingActionParent(testBizObjs.entryHeaderWrapper.Declaration).SendingObjectsCollection;

		protected override BusinessObject GetNewElementToAddToTheCollection() => null;

		protected override void SetUp()
		{
			base.SetUp();
			testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
		}

		(EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper) testBizObjs;
	}
}
