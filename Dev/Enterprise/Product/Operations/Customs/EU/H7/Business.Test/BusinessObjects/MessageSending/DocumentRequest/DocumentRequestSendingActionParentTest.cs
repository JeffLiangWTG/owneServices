using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	[TestedType(typeof(DocumentRequestSendingActionParent<DocumentRequestSendingAction>))]
	class DocumentRequestSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentRequestSendingActionParent<DocumentRequestSendingAction>(Factory.New<AsycudaManifestHeader>());
		}

		public void TestMessageSendingObjectProperties()
		{
			var sendingObjectParent = GetNewBusinessObject() as BaseMessageSendingObjectParent;
			var sendingObjectPropertyNames = sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName).ToArray();
			AssertArrayEqualsByElements("MessageSendingObjectProperties", new ZString[] { "BillNumber", "MovementReferenceNumber" }, sendingObjectPropertyNames);
		}

		public void TestSendingObjectsCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bill1";

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "Bill2";

			var parent = new DocumentRequestSendingActionParent<DocumentRequestSendingActionForTest>(header);
			var sendingObjectCollection = parent.SendingObjectsCollection;

			AssertEquals("SendingObjectsCollection contains 1 entry for each bill", 2, sendingObjectCollection.Count);
			var sendingObject1 = sendingObjectCollection[0];
			var sendingObject2 = sendingObjectCollection[1];

			CombineAssertions(() =>
			{
				AssertType<DocumentRequestSendingActionForTest>(sendingObject1);
				AssertEquals("Bill1", sendingObject1.BillNumber);
				AssertEquals("Bill2", sendingObject2.BillNumber);
			});
		}

		class DocumentRequestSendingActionForTest : DocumentRequestSendingAction
		{
			public DocumentRequestSendingActionForTest(AsycudaBill bill) : base(bill)
			{
			}
		}
	}
}
