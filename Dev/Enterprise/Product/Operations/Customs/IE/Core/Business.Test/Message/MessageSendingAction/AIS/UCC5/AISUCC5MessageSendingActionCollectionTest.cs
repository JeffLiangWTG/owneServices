using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AISUCC5MessageSendingActionCollection))]
	sealed class AISUCC5MessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AISUCC5MessageSendingActionCollection>
	{
		public void TestElementDefaultMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			AssertElementDefaultMessageType("When Entry Status is empty", string.Empty, "415");
			AssertElementDefaultMessageType("When Entry Status is Rejected", "REJ", "415");
			AssertElementDefaultMessageType("When Entry Status is Amendment Requested", "AMR", "413");
			AssertElementDefaultMessageType("When Entry Status is Pre-lodged", "PRE", "432");
			AssertElementDefaultMessageType("When Entry Status is Released", "REL", "414");

			void AssertElementDefaultMessageType(string message, string entryStatus, string expectedMessageType)
			{
				entryHeader.CH_EntryStatus = entryStatus;
				var sendingActionParent = new AISUCC5MessageSendingActionParent(declaration);
				var sendingObjectsCollection = sendingActionParent.SendingObjectsCollection;
				AssertEquals(message, expectedMessageType, sendingObjectsCollection[0].MessageType);
			}
		}

		public override void TestAdd()
		{
			Assert("AISUCC5MessageSendingActionCollection does not support adding.", true);
		}

		public override void TestDelete()
		{
			Assert("AISUCC5MessageSendingActionCollection does not support deleting.", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("AISUCC5MessageSendingActionCollection does not support RemoveFromRelationship.", true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("AISUCC5MessageSendingActionCollection haschanges as default", true);
		}

		protected override AISUCC5MessageSendingActionCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			_ = declaration.ActiveEntryHeaders.AddNew();
			var sendingAction = new AISUCC5MessageSendingActionParent(declaration);
			return (AISUCC5MessageSendingActionCollection)sendingAction.SendingObjectsCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => null;
	}
}
