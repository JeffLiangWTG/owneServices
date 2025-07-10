using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AISMessageSendingActionCollection))]
	class AISMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AISMessageSendingActionCollection>
	{
		public override void TestAdd()
		{
			Assert("AISMessageSendingActionCollection does not support adding.", true);
		}

		public override void TestDelete()
		{
			Assert("AISMessageSendingActionCollection does not support deleting.", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("AISMessageSendingActionCollection does not support RemoveFromRelationship.", true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("AISMessageSendingActionCollection haschanges as default", true);
		}

		protected override AISMessageSendingActionCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			_ = declaration.ActiveEntryHeaders.AddNew();
			var sendingAction = new AISMessageSendingActionParent(declaration);
			return (AISMessageSendingActionCollection)sendingAction.SendingObjectsCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => null;
	}
}
