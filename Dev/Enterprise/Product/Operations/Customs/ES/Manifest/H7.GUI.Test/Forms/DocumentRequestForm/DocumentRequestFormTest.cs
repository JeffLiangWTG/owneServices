using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(DocumentRequestForm))]
	public class DocumentRequestFormTest : ZFormBasherTest
	{
		public void TestSendMessages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsProfile = "1111";

			var bill = header.Bills.AddNew();
			bill.H7MovementReferenceNumber = "555";
			var clearanceReferenceNumber = CusEntryNumber.New<CusEntryNumber>(bill, "CLR", "ES", "H7");
			clearanceReferenceNumber.CE_EntryNum = "666";

			var sendingParent = new DocumentRequestSendingActionParent<DocumentRequestSendingAction>(header);
			sendingParent.SendingObjectsCollection[0].ShouldSend = true;

			using (var form = new DocumentRequestForm(sendingParent))
			{
				form.Show();

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();
				AssertEquals("1 message(s) queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestExceptionHappensWhenSendingMessages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var billWithNoMRN = header.Bills.AddNew();
			var sendingParent = new DocumentRequestSendingActionParent<DocumentRequestSendingAction>(header);

			var invalidSendingAction = new DocumentRequestSendingAction(billWithNoMRN);
			invalidSendingAction.ShouldSend = true;
			sendingParent.SendingObjectsCollection.Add(invalidSendingAction);

			using (var form = new DocumentRequestForm(sendingParent))
			{
				form.Show();

				var sendButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "SendButton");
				sendButton.PerformClick();
				AssertEquals("Could not create document request message for bill(s).", UnitTestUserNotification.Instance.LastMessage.Text);
			}
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
					AssertContainsExactElementsInAnyOrder(["ShouldSend", "BillNumber", "G3LocalReferenceNumber", "G3MovementReferenceNumber", "H7MovementReferenceNumber", "MessageStatus", "CustomsStatus"], messageSendingGrid.Columns.Select(x => x.ColumnName));
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsProfile = "1111";

			var bill = header.Bills.AddNew();
			bill.H7MovementReferenceNumber = "555";
			var clearanceReferenceNumber = CusEntryNumber.New<CusEntryNumber>(bill, "CLR", "ES", "H7");
			clearanceReferenceNumber.CE_EntryNum = "666";

			var sendingParent = new DocumentRequestSendingActionParent<DocumentRequestSendingAction>(header);

			return new DocumentRequestForm(sendingParent);
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override bool AllowSaveOnFormForTestHasChanges => false;
	}
}
