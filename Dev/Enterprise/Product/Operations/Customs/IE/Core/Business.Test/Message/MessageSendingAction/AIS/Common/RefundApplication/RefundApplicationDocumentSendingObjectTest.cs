using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(RefundApplicationDocumentSendingObject))]
	sealed class RefundApplicationDocumentSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Refund Application Document", sendingObject.HumanReadableName);
		}

		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("EntryHeader missing", () => new RefundApplicationDocumentSendingObjectCollection(null, parent));
				AssertExceptionThrown<ArgumentException>("Action missing", () => new RefundApplicationDocumentSendingObjectCollection(entryHeader, null));
			});
		}

		public void TestDocumentType_Caption()
		{
			var resData = Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObject.DocumentTypeInfo);
			AssertEquals("Caption", "Document Type", resData.Caption);
			AssertEquals("ShortCaption", "Type", resData.ShortCaption);
		}

		public void TestDescriptionOfGrounds_MaxLength()
		{
			AssertEquals(4, sendingObject.DocumentTypeInfo.MaxLength);
		}

		public void TestDocumentIdentifier_Caption()
		{
			var resData = Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObject.DocumentIdentifierInfo);
			AssertEquals("Caption", "Document Identifier", resData.Caption);
			AssertEquals("Caption", "Identifier", resData.MediumCaption);
			AssertEquals("ShortCaption", "ID", resData.ShortCaption);
		}

		public void TestDocumentIdentifier_MaxLength()
		{
			AssertEquals(35, sendingObject.DocumentIdentifierInfo.MaxLength);
	}

		public void TestDocumentDate_Caption()
		{
			var resData = Customs.Business.ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(sendingObject.DocumentDateInfo);
			AssertEquals("Caption", "Document Date", resData.Caption);
			AssertEquals("ShortCaption", "Date", resData.ShortCaption);
		}

		public void TestLookups()
		{
			AssertType<RefundApplicationDocumentSendingObjectLookups>(sendingObject.Lookups);
		}
		protected override BusinessObject GetNewBusinessObject() => sendingObject;

		protected override void SetUp()
		{
			base.SetUp();
			var (entryHeaderWrapper, _) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			entryHeader = entryHeaderWrapper.EntryHeader;
			parent = new RefundApplicationMessageSendingAction(entryHeader);
			sendingObject = new RefundApplicationDocumentSendingObject(entryHeader, parent);
		}
		CusEntryHeader entryHeader;
		RefundApplicationDocumentSendingObject sendingObject;
		RefundApplicationMessageSendingAction parent;
	}
}
