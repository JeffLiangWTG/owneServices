using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ExitSummaryMessageSendingActionCollection))]
	class ExitSummaryMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExitSummaryMessageSendingActionCollection>
	{
		public void TestPassMessagingObjectsInConstructor()
		{
			var ced1 = AddCusExitDetail(UniversalReferenceConstants.CusExitDetailStatus._310);
			var ced2 = AddCusExitDetail(UniversalReferenceConstants.CusExitDetailStatus._371);
			AddCusExitDetail(UniversalReferenceConstants.CusExitDetailStatus._342);

			var collection = new ExitSummaryMessageSendingActionCollection(exitHeader, new BusinessObject[] { ced1, ced2 });
			collection.PopulateElements();
			AssertContainsExactElementsInAnyOrder(new[] { ced1, ced2 }, collection.Cast<MessageSendingAction>().Select(o => o.MessagingObject));

			CusExitDetail AddCusExitDetail(string status)
			{
				var exitDetail = exitHeader.CusExitDetails.AddNew();
				exitDetail.CED_Status = status;
				return exitDetail;
			}
		}

		public void TestGetSendingAction()
		{
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = "MRN123";
			var coll = GetCollectionToTest();
			coll.PopulateElements();
			var element = (ExitSummaryMessageSendingAction)coll.Single();
			AssertEquals("Correct getDetails function", "MRN123", element.Details);
		}

		protected override Type GetExpectedCollectionType() => typeof(ExitSummaryMessageSendingActionCollection);

		protected override ExitSummaryMessageSendingActionCollection GetCollectionToTest()
		{
			return new ExitSummaryMessageSendingActionCollection(exitHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = exitHeader.CusExitDetails.AddNew();
			return new ExitSummaryMessageSendingAction(entry);
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.New<CusExitControlHeader>();
		}
		CusExitControlHeader exitHeader;
	}
}
