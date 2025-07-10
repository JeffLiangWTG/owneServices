using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DocumentsSendingActionCollection))]
	class DocumentsSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentsSendingActionCollection>
	{
		public void TestOnAdded()
		{
			AssertEquals("MessageType should be set when added.", AESOutgoingMessageTypeList.Codes.DocumentUpload, ((DocumentsSendingAction)Collection.Single()).MessageType);
		}

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

		protected override DocumentsSendingActionCollection GetCollectionToTest()
			=> (DocumentsSendingActionCollection)new DocumentsSendingActionParent(testBizObjs.entryHeaderWrapper.Declaration, AESOutgoingMessageTypeList.Codes.DocumentUpload).SendingObjectsCollection;

		protected override BusinessObject GetNewElementToAddToTheCollection() => null;

		protected override void SetUp()
		{
			base.SetUp();
			testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
		}

		(EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper) testBizObjs;
	}
}
