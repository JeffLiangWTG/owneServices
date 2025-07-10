using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(DocumentSendingActionCollection))]
	class DocumentSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentSendingActionCollection>
	{
		public void TestOnAdd()
		{
			AssertEquals("MessageType should be set when added.", NCTSOutgoingMessageTypeList.Codes.UploadSupportingDocuments, ((DocumentSendingAction)Collection.Single()).MessageType);
		}

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

		protected override DocumentSendingActionCollection GetCollectionToTest()
			=> (DocumentSendingActionCollection)new DocumentSendingActionParent(GetNewNctsHeader()).SendingObjectsCollection;

		protected override BusinessObject GetNewElementToAddToTheCollection() => null;

		NctsHeader GetNewNctsHeader()
		{
			var header = Factory.New<NctsHeader>();
			return header;
		}
	}
}
