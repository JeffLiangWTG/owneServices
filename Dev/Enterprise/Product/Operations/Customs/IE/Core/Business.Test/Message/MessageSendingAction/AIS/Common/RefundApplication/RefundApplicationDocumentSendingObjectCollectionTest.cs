using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(RefundApplicationDocumentSendingObjectCollection))]
	sealed class RefundApplicationDocumentSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RefundApplicationDocumentSendingObjectCollection>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("EntryHeader missing", () => new RefundApplicationDocumentSendingObjectCollection(null, parentObject));
				AssertExceptionThrown<ArgumentException>("Action missing", () => new RefundApplicationDocumentSendingObjectCollection(entryHeader, null));
			});
		}

		public void TestMaxCount()
		{
			AssertEquals("Should have set MaxCount.", 99, ((ISupportMaxCountValidation)Collection).MaxCountValidator.MaxCount);
		}

		protected override RefundApplicationDocumentSendingObjectCollection GetCollectionToTest() => new RefundApplicationDocumentSendingObjectCollection(entryHeader, parentObject);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new RefundApplicationDocumentSendingObject(entryHeader, parentObject);

		protected override void SetUp()
		{
			base.SetUp();
			var (entryHeaderWrapper, _) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			entryHeader = entryHeaderWrapper.EntryHeader;
			parentObject = new RefundApplicationMessageSendingAction(entryHeader);
		}
		CusEntryHeader entryHeader;
		RefundApplicationMessageSendingAction parentObject;
	}
}
