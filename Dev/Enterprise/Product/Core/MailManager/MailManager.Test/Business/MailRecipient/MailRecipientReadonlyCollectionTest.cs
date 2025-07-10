using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MailManager.Business
{
	sealed class MailRecipientReadonlyCollectionTest : TestCaseWithFactory
	{
		MailRecipientCollection MyCollection;
		DummyMailItem MyItem;
		MailRecipient Recipient1;
		MailRecipient Recipient2;
		MailRecipient Recipient3;
		MailRecipientReadonlyCollection Collection;

		protected override void SetUp()
		{
			base.SetUp();
			MyItem = Factory.New<DummyMailItem>();
			Recipient1 = Factory.New<MailRecipient>();
			Recipient2 = Factory.New<MailRecipient>();
			Recipient3 = Factory.New<MailRecipient>();

			Recipient1.MR_MI = MyItem.PK;
			Recipient2.MR_MI = MyItem.PK;
			Recipient3.MR_MI = MyItem.PK;

			MyCollection = MyItem.MailRecipientsCore_Exposed;
			Collection = new MailRecipientReadonlyCollection(MyCollection);

			base.SetUp();
		}

		public void TestGetElement()
		{
			AssertEquals("First element is Recipient1", Recipient1, Collection[0]);
			AssertEquals("Second element is Recipient2", Recipient2, Collection[1]);
			AssertEquals("Third element is Recipient3", Recipient3, Collection[2]);
		}

		public void TestRemoveAndDeleteAll()
		{
			AssertEquals("Precondition - 3 elements", 3, Collection.Count);
			Collection.RemoveAndDeleteAll();
			AssertEquals("All cleared", 0, Collection.Count);
		}

		public void TestContains()
		{
			Assert("Should contain aaa address", Collection.Contains(Recipient1.PK));
			Assert("Should contain bbb address", Collection.Contains(Recipient2.PK));
			Assert("Should contain ccc address", Collection.Contains(Recipient3.PK));
			Assert("Should not contain ddd address", !Collection.Contains(ZGuid.NewZGuid()));
		}

		public void TestCount()
		{
			AssertEquals("Precondition - 3 elements", 3, Collection.Count);
		}
	}
}
