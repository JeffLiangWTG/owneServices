using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(AlertOrRejectSendingActionCollection))]
	sealed class AlertOrRejectSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AlertOrRejectSendingActionCollection>
	{
		public void TestGetSendingAction()
		{
			var collection = GetCollectionToTest();
			collection.PopulateElements();
			AssertEquals(typeof(AlertOrRejectSendingAction), collection[0].GetType());
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Not supporting adding, this check is not required.", true);
		}

		protected override Type GetExpectedCollectionType() => typeof(AlertOrRejectSendingActionCollection);

		protected override AlertOrRejectSendingActionCollection GetCollectionToTest()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var sendingActionParent = new AlertOrRejectSendingActionParent(declaration);
			return (AlertOrRejectSendingActionCollection)sendingActionParent.SendingObjectsCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AlertOrRejectSendingAction(Factory.New<EMCSJobDeclaration>());
	}
}
