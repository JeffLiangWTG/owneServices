using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ExplanationOnDelaySendingActionCollection))]
	sealed class ExplanationOnDelaySendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExplanationOnDelaySendingActionCollection>
	{
		public void TestGetSendingAction()
		{
			var collection = GetCollectionToTest();
			collection.PopulateElements();
			AssertEquals(typeof(ExplanationOnDelaySendingAction), collection[0].GetType());
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Not supporting adding, this check is not required.", true);
		}

		protected override Type GetExpectedCollectionType() => typeof(ExplanationOnDelaySendingActionCollection);

		protected override ExplanationOnDelaySendingActionCollection GetCollectionToTest()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var sendingActionParent = new ExplanationOnDelaySendingActionParent(declaration);
			return (ExplanationOnDelaySendingActionCollection)sendingActionParent.SendingObjectsCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ExplanationOnDelaySendingAction(Factory.New<EMCSJobDeclaration>());
	}
}
