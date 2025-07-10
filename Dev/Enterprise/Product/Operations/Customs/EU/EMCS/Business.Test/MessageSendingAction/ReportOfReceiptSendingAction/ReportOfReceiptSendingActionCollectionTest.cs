using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ReportOfReceiptSendingActionCollection))]
	sealed class ReportOfReceiptSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReportOfReceiptSendingActionCollection>
	{
		public void TestGetSendingAction()
		{
			var collection = GetCollectionToTest();
			collection.PopulateElements();
			AssertEquals(typeof(ReportOfReceiptSendingAction), collection[0].GetType());
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Not supporting adding, this check is not required.", true);
		}

		protected override Type GetExpectedCollectionType() => typeof(ReportOfReceiptSendingActionCollection);

		protected override ReportOfReceiptSendingActionCollection GetCollectionToTest()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var sendingActionParent = new ReportOfReceiptSendingActionParent(declaration);
			return (ReportOfReceiptSendingActionCollection)sendingActionParent.SendingObjectsCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReportOfReceiptSendingAction(Factory.New<EMCSJobDeclaration>());
		}
	}
}
