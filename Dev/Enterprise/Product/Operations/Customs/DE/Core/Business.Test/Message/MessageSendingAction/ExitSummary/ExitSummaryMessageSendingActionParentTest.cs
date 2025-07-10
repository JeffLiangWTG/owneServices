using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ExitSummaryMessageSendingActionParent))]
	class ExitSummaryMessageSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAllowMultipleObjects()
		{
			exitHeader.CusExitDetails.AddNew();
			exitHeader.CusExitDetails.AddNew();

			var parent = (ExitSummaryMessageSendingActionParent)GetNewBusinessObject();
			parent.SendingObjectsCollection[0].ShouldSend = true;
			parent.SendingObjectsCollection[1].ShouldSend = true;
			parent.RunPreSaveValidation();
			AssertNoRowError("Allow multiple objects", parent, "Please select only one declaration to send.");
		}

		public void TestSendingObjectCollection()
		{
			var ced1 = AddCusExitDetail(string.Empty);
			var ced2 = AddCusExitDetail(UniversalReferenceConstants.CusExitDetailStatus._301);
			AddCusExitDetail(UniversalReferenceConstants.CusExitDetailStatus._371);

			var parent = (ExitSummaryMessageSendingActionParent)GetNewBusinessObject();
			var collection = parent.SendingObjectsCollection;
			CombineAssertions(() =>
			{
				AssertType<ExitSummaryMessageSendingActionCollection>("Type", collection);
				AssertContainsExactElementsInAnyOrder("Includes CusExitDetails with status ('', '301')", new[] { ced1, ced2 }, collection.Cast<MessageSendingAction>().Select(o => o.MessagingObject));
			});
		}

		public void TestSendingObjectCollection_DefaultMessageType()
		{
			AddCusExitDetail(string.Empty);
			AddCusExitDetail(UniversalReferenceConstants.CusExitDetailStatus._301);

			var parent = (ExitSummaryMessageSendingActionParent)GetNewBusinessObject();
			var collection = parent.SendingObjectsCollection.Cast<ExitSummaryMessageSendingAction>().ToArray();
			var actionWithStatus301 = collection.Single(x => x.Status == UniversalReferenceConstants.CusExitDetailStatus._301);
			var actionWithOtherStatus = collection.Single(x => x.Status.IsEmpty);
			CombineAssertions(() =>
			{
				AssertEquals("Status = '301'", ExitSummaryMessageTypeList.Codes.Presentation, actionWithStatus301.MessageType);
				AssertEquals("Status <> '301'", ExitSummaryMessageTypeList.Codes.Anticipation, actionWithOtherStatus.MessageType);
			});
		}

		public void TestMessageSendingObjectProperties()
		{
			var parent = (ExitSummaryMessageSendingActionParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;
			CombineAssertions(() =>
			{
				AssertEquals("MRN caption", "MRN", properties.ElementAt(0).ResourceString.Caption);
				AssertEquals("Reference Number caption", "Reference Number", properties.ElementAt(1).ResourceString.Caption);
				AssertEquals("Status caption", "Status", properties.ElementAt(2).ResourceString.Caption);
				AssertEquals("Status Description caption", "Status Description", properties.ElementAt(3).ResourceString.Caption);
				AssertEquals("Message Type caption", "Message Type", properties.ElementAt(4).ResourceString.Caption);

				AssertEquals("MRN width", 119, properties.ElementAt(0).ColumnWidth);
				AssertEquals("Reference Number width", 114, properties.ElementAt(1).ColumnWidth);
				AssertEquals("Status width", 53, properties.ElementAt(2).ColumnWidth);
				AssertEquals("Status Description width", 140, properties.ElementAt(3).ColumnWidth);
				AssertEquals("Message Type width", 92, properties.ElementAt(4).ColumnWidth);

				Assert("All properties mandatory", properties.All(x => x.IsMandatory));
			});
		}

		protected override BusinessObject GetNewBusinessObject() => new ExitSummaryMessageSendingActionParent(exitHeader);

		protected override void SetUp()
		{
			base.SetUp();
			exitHeader = Factory.New<CusExitControlHeader>();
		}
		CusExitControlHeader exitHeader;

		CusExitDetail AddCusExitDetail(string status)
		{
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_Status = status;
			return exitDetail;
		}
	}
}
