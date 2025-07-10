using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(CancellationSendingActionCollection))]
	sealed class CancellationSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CancellationSendingActionCollection>
	{
		public void TestGetSendingAction()
		{
			var collection = GetCollectionToTest();
			collection.PopulateElements();
			AssertEquals(typeof(CancellationSendingAction), collection[0].GetType());
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Not supporting adding, this check is not required.", true);
		}

		protected override Type GetExpectedCollectionType() => typeof(CancellationSendingActionCollection);

		protected override CancellationSendingActionCollection GetCollectionToTest()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var sendingActionParent = new CancellationSendingActionParent(declaration);
			return (CancellationSendingActionCollection)sendingActionParent.SendingObjectsCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CancellationSendingAction(Factory.New<EMCSJobDeclaration>());
	}
}
