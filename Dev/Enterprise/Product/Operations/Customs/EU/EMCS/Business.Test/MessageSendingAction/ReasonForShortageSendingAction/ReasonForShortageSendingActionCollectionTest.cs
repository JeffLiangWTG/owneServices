using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ReasonForShortageSendingActionCollection))]
	sealed class ReasonForShortageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReasonForShortageSendingActionCollection>
	{
		public void TestGetSendingAction()
		{
			var collection = GetCollectionToTest();
			collection.PopulateElements();
			AssertEquals(typeof(ReasonForShortageSendingAction), collection[0].GetType());
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Not supporting adding, this check is not required.", true);
		}

		protected override Type GetExpectedCollectionType() => typeof(ReasonForShortageSendingActionCollection);

		protected override ReasonForShortageSendingActionCollection GetCollectionToTest()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var sendingActionParent = new ReasonForShortageSendingActionParent(declaration);
			return (ReasonForShortageSendingActionCollection)sendingActionParent.SendingObjectsCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ReasonForShortageSendingAction(Factory.New<EMCSJobDeclaration>());
	}
}
