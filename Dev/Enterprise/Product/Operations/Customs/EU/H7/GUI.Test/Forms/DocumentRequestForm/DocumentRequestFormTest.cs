using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.EU.H7.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(DocumentRequestForm))]
	sealed class DocumentRequestFormTest : MessageSendingFormWithValidationDetailsAbstractTest
	{
		protected override bool AllowHasChangesOnFormOpen => true;

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var messageSendingObjectParent = new DocumentRequestSendingActionParent<DocumentRequestSendingAction>(header);
			return new DocumentRequestForm(messageSendingObjectParent);
		}

		public void TestMessageSendingGridColumns()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var messageSendingGrid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				CombineAssertions(() =>
				{
					AssertNotNull("MessageSendingObjectsGrid", messageSendingGrid);
					AssertContainsExactElementsInAnyOrder(["ShouldSend", "BillNumber", "MRN", "MessageStatus", "CustomsStatus"], messageSendingGrid.Columns.Select(x => x.ColumnName));
				});
			}
		}

		public void TestSendWithValidationErrorsCheckBoxHidden()
		{
			using (var form = GetFormToBashCore())
			{
				var checkBox = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				Assert(!checkBox.Visible);
			}
		}

		[RequiresSTA]
		public void TestWarningSplitContainerHidden()
		{
			using (var form = GetFormToBashCore())
			{
				var warningsSplitContainer = form.FindSingle<KSplitContainer>("WarningSplitContainer");
				Assert(!warningsSplitContainer.Visible);
			}
		}

		public void TestFormHeading()
		{
			using (var form = (DocumentRequestForm)GetFormToBashCore())
			{
				AssertEquals("Form Heading", "Request Documents", form.FormHeading);
			}
		}

		public void TestSelectAllButton()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var messageSendingObject1 = new DocumentRequestSendingAction(header.Bills.AddNew());
			var messageSendingObject2 = new DocumentRequestSendingAction(header.Bills.AddNew());
			var messageSendingObject3 = new DocumentRequestSendingAction(header.Bills.AddNew());
			var testingParent = new DocumentRequestSendingActionParent<DocumentRequestSendingAction>(header);
			testingParent.SendingObjectsCollection[0].ShouldSend = false;

			var selectedMessageSendingObjectCount = testingParent.SendingObjectsCollection.OfType<BaseMessageSendingObject>().Count(mso => mso.ShouldSend);
			AssertEquals("Precondition: There is a non-selected object", 2, selectedMessageSendingObjectCount);

			using (var form = new DocumentRequestForm(testingParent))
			{
				form.Show();
				var selectAllButton = form.Controls.Find("SelectAllButton", searchAllChildren: true).Single() as ZButton;
				AssertEquals("Button Text", "Select/Deselect All", selectAllButton.Text);

				selectAllButton.PerformClick();
				selectedMessageSendingObjectCount = testingParent.SendingObjectsCollection.OfType<BaseMessageSendingObject>().Count(mso => mso.ShouldSend);
				AssertEquals("Button should select all if there are any non-selected object", 3, selectedMessageSendingObjectCount);

				selectAllButton.PerformClick();
				selectedMessageSendingObjectCount = testingParent.SendingObjectsCollection.OfType<BaseMessageSendingObject>().Count(mso => mso.ShouldSend);
				AssertEquals("Button should deselect all if all objects are selected", 0, selectedMessageSendingObjectCount);
			}
		}

		public void TestSendMessages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var branch = header.Branch.Company.Branches.AddNew();
			header.AMA_GB = branch.PK;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			var sendingObjectParent = new DocumentRequestSendingActionParent<DocumentRequestSendingActionForTest>(header);
			using (var form = new DocumentRequestForm(sendingObjectParent))
			{
				form.Show();
				var sendingObject1 = sendingObjectParent.SendingObjectsCollection
					.Cast<MessageSendingObject>().Single(x => x.Bill == bill1);
				var sendingObject2 = sendingObjectParent.SendingObjectsCollection
					.Cast<MessageSendingObject>().Single(x => x.Bill == bill2);
				var sendingObject3 = sendingObjectParent.SendingObjectsCollection
					.Cast<MessageSendingObject>().Single(x => x.Bill == bill3);
				sendingObject1.ShouldSend = true;
				sendingObject2.ShouldSend = false;
				sendingObject3.ShouldSend = true;

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();
				AssertEquals("2 message(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertBillHasMessage(bill1);
				AssertEquals("bill2.Messages.Count", 0, bill2.Messages.Count);
				AssertBillHasMessage(bill3);

				void AssertBillHasMessage(AsycudaBill bill)
				{
					CombineAssertions("New message created", () =>
					{
						AssertEquals(1, bill.Messages.Count);
						var message = bill.Messages[0];
						AssertEquals("Direction", "TRX", message.EM_ReceiveTransmit);
						AssertEquals("Status", "QUE", message.EM_Status);
					});
				}
			}
		}

		public void TestMessageSendingGridColumnLayoutProvider()
		{
			using (var form = GetFormToBash())
			{
				var layoutProvider = typeof(DocumentRequestForm).GetField("messageSendingGridColumnLayoutProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(form);
				AssertEquals(typeof(DocumentRequestGridColumnLayout), layoutProvider.GetType());
			}
		}
	}

	class DocumentRequestSendingActionForTest : DocumentRequestSendingAction
	{
		public DocumentRequestSendingActionForTest(AsycudaBill bill) : base(bill)
		{
		}

		public override Business.MessageSender CreateSender()
		{
			return new MessageSenderForTest(this);
		}
	}
}
