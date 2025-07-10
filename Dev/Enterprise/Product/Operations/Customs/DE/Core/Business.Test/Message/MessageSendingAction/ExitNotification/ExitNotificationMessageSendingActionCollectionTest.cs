using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ExitNotificationMessageSendingActionCollection))]
	class ExitNotificationMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExitNotificationMessageSendingActionCollection>
	{
		public void TestGetSendingAction()
		{
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = "MRN123";
			var coll = GetCollectionToTest();
			coll.PopulateElements();
			var element = (ExitNotificationMessageSendingAction)coll.Single();
			AssertEquals("Correct getDetails function", "MRN123", element.Details);
		}

		protected override Type GetExpectedCollectionType() => typeof(ExitNotificationMessageSendingActionCollection);

		protected override ExitNotificationMessageSendingActionCollection GetCollectionToTest()
		{
			return new ExitNotificationMessageSendingActionCollection(exitHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = exitHeader.CusExitDetails.AddNew();
			return new ExitNotificationMessageSendingAction(entry);
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.New<CusExitControlHeader>();
		}
		CusExitControlHeader exitHeader;
	}
}
