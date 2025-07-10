using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(AgreedRateMessageSendingObjectParent))]
	sealed class AgreedRateMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new AgreedRateMessageSendingObjectParent(declaration);
		}

		public void TestSendingObjectsCollection()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var objectParent1 = new AgreedRateMessageSendingObjectParent(declaration1);
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);

			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.CustomsEntryHeaders.AddNew();
			declaration2.CustomsEntryHeaders.AddNew();
			var objectParent2 = new AgreedRateMessageSendingObjectParent(declaration2);
			AssertEquals(2, objectParent2.SendingObjectsCollection.Count);
		}

		public void TestObjectsToSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var objectParent = new AgreedRateMessageSendingObjectParent(declaration);
			objectParent.SendingObjectsCollection[1].ShouldSend = true;
			AssertEquals(1, objectParent.ObjectsToSend.Count());
			AssertEquals(entry, objectParent.ObjectsToSend.Single().Header);
		}
	}
}
