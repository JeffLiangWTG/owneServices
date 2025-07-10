using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(MinimalSendingActionCollection))]
	sealed class MinimalSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MinimalSendingActionCollection>
	{
		public void TestGetSendingAction()
		{
			var collection = GetCollectionToTest();
			collection.PopulateElements();
			AssertEquals(typeof(EMCSMessageSendingAction), collection[0].GetType());
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Not supporting adding, this check is not required.", true);
		}

		protected override Type GetExpectedCollectionType() => typeof(MinimalSendingActionCollection);

		protected override MinimalSendingActionCollection GetCollectionToTest()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var sendingAction = new MinimalSendingActionParent(declaration);
			return (MinimalSendingActionCollection)sendingAction.SendingObjectsCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<EMCSJobDeclaration>();
	}
}
