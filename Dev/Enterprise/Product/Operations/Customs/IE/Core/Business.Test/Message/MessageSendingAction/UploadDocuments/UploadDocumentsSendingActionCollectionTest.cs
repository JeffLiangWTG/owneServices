using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(UploadDocumentsSendingActionCollection))]
	sealed class UploadDocumentsSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UploadDocumentsSendingActionCollection>
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

		public void TestOnAdd_Calls_CheckHasOpenDocumentsOrIsControl()
		{
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var sendingActionForTest = new UploadDocumentsSendingActionForTest(entryHeader);
			var collection = GetCollectionToTest();

			collection.Add(sendingActionForTest);

			(sendingActionForTest.GetValidationMock()).Verify(v => v.CheckHasOpenDocumentsOrIsControl(), Times.Once);

			Assert("Needed to avoid 'Empty test' error", true);
		}

		protected override UploadDocumentsSendingActionCollection GetCollectionToTest()
			=> (UploadDocumentsSendingActionCollection)new UploadDocumentsSendingActionParent(testBizObjs.entryHeaderWrapper.Declaration, AESOutgoingMessageTypeList.Codes.DocumentUpload).SendingObjectsCollection;

		protected override BusinessObject GetNewElementToAddToTheCollection() => null;

		protected override void SetUp()
		{
			base.SetUp();
			testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
		}

		(EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper) testBizObjs;

		class UploadDocumentsSendingActionForTest : UploadDocumentsSendingAction
		{
			public UploadDocumentsSendingActionForTest(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
			{
			}

			public new UploadDocumentsSendingActionValidation Validation => (UploadDocumentsSendingActionValidation)GetNewValidation();

			public Mock<UploadDocumentsSendingActionValidation> GetValidationMock()
			{
				return validationMock ??= new Mock<UploadDocumentsSendingActionValidation>(this);
			}
			Mock<UploadDocumentsSendingActionValidation> validationMock;

			protected override CusEntryHeaderMessageSendingActionValidation GetNewValidation() => GetValidationMock().Object;
		}
	}
}
