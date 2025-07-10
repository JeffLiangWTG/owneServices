using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectCollection))]
	class MessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingObjectCollection>
	{
		public void TestUnRegisterBillAsEditableChildObject()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			var sendigBill1 = new MessageSendingObject(bill1, ActionCode.Registering, TestSendingAction);

			var bill2 = header.Bills.AddNew();
			var sendigBill2 = new MessageSendingObject(bill2, ActionCode.Registering, TestSendingAction);

			var collection = new MessageSendingObjectCollection(Factory);
			collection.Add(sendigBill1);
			collection.Add(sendigBill2);
			AssertEquals(true, sendigBill1.IsRegisteredEditableChildObject(bill1));
			AssertEquals(true, sendigBill2.IsRegisteredEditableChildObject(bill2));

			collection.UnRegisterBillAsEditableChildObject();
			AssertEquals(false, sendigBill1.IsRegisteredEditableChildObject(bill1));
			AssertEquals(false, sendigBill2.IsRegisteredEditableChildObject(bill2));
		}

		protected override MessageSendingObjectCollection GetCollectionToTest() => new MessageSendingObjectCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			return new MessageSendingObject(bill, ActionCode.Registering, TestSendingAction);
		}

		MessageSendingAction TestSendingAction
		{
			get { return testSendingAction ?? (testSendingAction = new MessageSendingAction(Factory.New<JPAFRHeader>(), ActionCode.AmendingAdd)); }
		}
		MessageSendingAction testSendingAction;
	}
}
