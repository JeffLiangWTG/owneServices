using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	[TestedType(typeof(EUICS2AmendedItemsSelectionDialog))]
	sealed class EUICS2AmendedItemsSelectionDialogTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			requestHeader.EUS_Identifier = "A70";
			requestHeader.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			var amendedItemsHeader = new ICS2AmendedItemsHeader(MessageTypes.Codes.R02, manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);
			return new EUICS2AmendedItemsSelectionDialog(amendedItemsHeader, "Amend Manifest");
		}

		public void TestAmendedItemsGrid()
		{
			using (var form = GetFormToBash())
			{
				form.Show();

				var grid = form.FindSingle<ZGrid>("ItemsGrid");
				AssertEquals("Should be NoRemovePossible.", RemoveAction.NoRemovePossible, grid.RemoveAction);

				var expectedColumns = new[]
				{
					nameof(ICS2AmendedItem.IsSelected),
					nameof(ICS2AmendedItem.Identifier),
					nameof(ICS2AmendedItem.RequestTypeDescription),
					nameof(ICS2AmendedItem.ResponsibleMemberState),
					nameof(ICS2AmendedItem.RequestStatus)
				};

				var actualColumns = grid.Columns.Select(c => c.ColumnName).ToArray();
				AssertArrayEqualsByElements(expectedColumns, actualColumns);
			}
		}

		public void TestSelectAndDeselectAllButtons()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			var requestHeader1 = manifestHeader.RequestHeaders.AddNew();
			requestHeader1.EUS_Identifier = "A70";
			requestHeader1.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			var requestHeader2 = manifestHeader.RequestHeaders.AddNew();
			requestHeader2.EUS_Identifier = "B30";
			requestHeader2.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			var amendedItemsHeader = new ICS2AmendedItemsHeader(MessageTypes.Codes.R03, manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);

			using (var dialog = new EUICS2AmendedItemsSelectionDialog(amendedItemsHeader, "Amend &Manifest"))
			{
				var items = amendedItemsHeader.AmendedItems.Cast<ICS2AmendedItem>();
				AssertEquals("Precondition", 2, items.Count());

				Assert("Precondition", items.All(c => !c.IsSelected));

				dialog.Show();

				var selectAllButton = dialog.FindSingle<ZButton>("SelectAllButton");
				selectAllButton.PerformClick();

				Assert("Should select all items", items.All(c => c.IsSelected));

				var deselectAllButton = dialog.FindSingle<ZButton>("DeselectAllButton");
				deselectAllButton.PerformClick();

				Assert("Should deselect all items", items.All(c => !c.IsSelected));
			}
		}

		public void TestRunPreSendValidation()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var requestHeader = manifestHeader.RequestHeaders.AddNew();
			requestHeader.EUS_Identifier = "A70";
			requestHeader.EUS_Type = EUICS2ReferralRequestTypeList.Codes.CL735_AMD;

			var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);

			using (var dialog = new EUICS2AmendedItemsSelectionDialog(amendedItemsHeader, "Amend &Manifest"))
			{
				amendedItemsHeader.AmendedItems[0].IsSelected = false;

				dialog.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertEquals("Should update the caption.", "Send Amend Manifest", dialog.Text);

				var sendButton = dialog.FindSingle<ZButton>("SendButton");
				sendButton.PerformClick();

				var lastMessageText = UnitTestUserNotification.Instance.LastMessage.Text;

				CombineAssertions(() =>
				{
					AssertContains("Please fix these errors before sending any messages:", lastMessageText);
					AssertContains("Please select at least one valid referral request to send the message.", lastMessageText);
				});
			}
		}
	}
}
