using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(MessageAttacheeSelectionCollection))]
	sealed class MessageAttacheeSelectionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageAttacheeSelectionCollection>
	{
		public void TestAllowNew()
		{
			MessageAttacheeSelectionCollection testCollection = new MessageAttacheeSelectionCollection(new DummyMessageAttacheeHolder(Factory), MessageAttacheeMessageType.Original);
			AssertEquals("Should not allow new", false, testCollection.AllowNew);
		}

		public void TestGetMessageAttacheeToSendAMessageFor()
		{
			MessageAttacheeSelectionCollection testCollection = new MessageAttacheeSelectionCollection(new DummyMessageAttacheeHolder(Factory), MessageAttacheeMessageType.Original);
			AssertEquals("Has nothing to send a message for", 0, testCollection.GetMessageAttacheesToSendMessagesFor().Length);

			MessageAttacheeSelection selection = new MessageAttacheeSelection(new DummyMessageAttachee(Factory));
			testCollection.Add(selection);
			AssertEquals("by default it is 'ShouldSendNow", true, selection.ShouldSendNow);
			AssertEquals("Has one to send a message for", 1, testCollection.GetMessageAttacheesToSendMessagesFor().Length);

			selection.ShouldSendNow = false;
			AssertEquals("Has nothing to send a message for", 0, testCollection.GetMessageAttacheesToSendMessagesFor().Length);
		}

		public void TestCreateItemsOnContruction()
		{
			DummyMessageAttachee attachee1 = new DummyMessageAttachee(Factory);
			attachee1.IsValidToSendForOriginalExposed = true;
			attachee1.IsValidToSendForAmendExposed = false;
			attachee1.IsValidToSendForWithdrawalExposed = false;

			DummyMessageAttachee attachee2 = new DummyMessageAttachee(Factory);
			attachee2.IsValidToSendForOriginalExposed = false;
			attachee2.IsValidToSendForAmendExposed = true;
			attachee2.IsValidToSendForWithdrawalExposed = false;

			DummyMessageAttachee attachee3 = new DummyMessageAttachee(Factory);
			attachee3.IsValidToSendForOriginalExposed = false;
			attachee3.IsValidToSendForAmendExposed = false;
			attachee3.IsValidToSendForWithdrawalExposed = true;

			DummyMessageAttacheeHolder holder = new DummyMessageAttacheeHolder(Factory);
			holder.MessageAttacheesExposed = new IMessageAttachee[] { attachee1, attachee2, attachee3 };

			MessageAttacheeSelectionCollection testCollection = new MessageAttacheeSelectionCollection(holder, MessageAttacheeMessageType.Original);
			AssertEquals("There should be only one item in the collection", 1, testCollection.Count);
			AssertEquals("The item should be Attachee1", attachee1, testCollection[0].MessageAttachee);

			testCollection = new MessageAttacheeSelectionCollection(holder, MessageAttacheeMessageType.Amend);
			AssertEquals("There should be only one item in the collection", 1, testCollection.Count);
			AssertEquals("The item should be Attachee2", attachee2, testCollection[0].MessageAttachee);

			testCollection = new MessageAttacheeSelectionCollection(holder, MessageAttacheeMessageType.Withdraw);
			AssertEquals("There should be only one item in the collection", 1, testCollection.Count);
			AssertEquals("The item should be Attachee3", attachee3, testCollection[0].MessageAttachee);
		}

		protected override MessageAttacheeSelectionCollection GetCollectionToTest()
		{
			return new MessageAttacheeSelectionCollection(new DummyMessageAttacheeHolder(Factory), MessageAttacheeMessageType.Original);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MessageAttacheeSelection(new DummyMessageAttachee(Factory));
		}
	}
}
