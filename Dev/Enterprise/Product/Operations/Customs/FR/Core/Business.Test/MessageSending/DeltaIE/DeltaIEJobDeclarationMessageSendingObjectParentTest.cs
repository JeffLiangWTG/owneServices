using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	[TestedType(typeof(DeltaIEJobDeclarationMessageSendingObjectParent))]
	public class DeltaIEJobDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeltaIEJobDeclarationMessageSendingObjectParent(Factory.New<JobDeclaration>());
		}

		public void TestMessageSendingObjectProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testItem = new DeltaIEJobDeclarationMessageSendingObjectParent(declaration).MessageSendingObjectProperties.ToList();

			AssertEquals(9, testItem.Count);
			Assert("MessageSendingObjectProperties should contain \"Update\"", testItem.Exists(item => item.PropertyName == "Update"));
			Assert("MessageSendingObjectProperties should contain \"MessageType\"", testItem.Exists(item => item.PropertyName == "MessageType"));
			Assert("MessageSendingObjectProperties should contain \"EntryType\"", testItem.Exists(item => item.PropertyName == "EntryType"));
			Assert("MessageSendingObjectProperties should contain \"SubStyle\"", testItem.Exists(item => item.PropertyName == "SubStyle"));
			Assert("MessageSendingObjectProperties should contain \"Description\"", testItem.Exists(item => item.PropertyName == "Description"));
			Assert("MessageSendingObjectProperties should contain \"DateTime\"", testItem.Exists(item => item.PropertyName == "DateTime"));
			Assert("MessageSendingObjectProperties should contain \"EntryStatus\"", testItem.Exists(item => item.PropertyName == "EntryStatus"));
			Assert("MessageSendingObjectProperties should contain \"ChangeAcknowledgementIndicator\"", testItem.Exists(item => item.PropertyName == "ChangeAcknowledgementIndicator"));
			Assert("MessageSendingObjectProperties should contain \"VOCReason\"", testItem.Exists(item => item.PropertyName == "VOCReason"));
		}

		public void TestGetSendingObjectsCollection()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var testWrapper1 = new DeltaIEJobDeclarationMessageSendingObjectParent(declaration1);
			AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(0, testWrapper1.SendingObjectsCollection.Count);

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.CustomsEntryHeaders.AddNew();
			declaration2.CustomsEntryHeaders.AddNew();
			var testWrapper2 = new DeltaIEJobDeclarationMessageSendingObjectParent(declaration2);
			AssertEquals(2, testWrapper2.SendingObjectsCollection.Count);
		}
	}
}
