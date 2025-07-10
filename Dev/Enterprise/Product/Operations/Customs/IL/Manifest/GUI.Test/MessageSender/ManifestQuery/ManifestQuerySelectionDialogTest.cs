using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(ManifestQuerySelectionDialog))]
	sealed class ManifestQuerySelectionDialogTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestCaption()
		{
			using var form = GetFormToBash();
			AssertEquals("Caption should be as expected", "Send Manifest Query", form.Text);
		}

		public void TestGrid()
		{
			using var form = GetFormToBash();

			form.Show();

			var grid = form.FindSingle<ZGrid>("ItemsGrid");
			AssertEquals("Should be NoRemovePossible.", RemoveAction.NoRemovePossible, grid.RemoveAction);

			var expectedColumns = new[]
			{
				nameof(AsycudaManifestQueryMessageSendingObject.ShouldSend),
				nameof(AsycudaManifestQueryMessageSendingObject.Schema.MessageType),
				nameof(AsycudaManifestQueryMessageSendingObject.Schema.MessageSubType),
				nameof(AsycudaManifestQueryMessageSendingObject.Schema.MessageSubTypeDescription),
				nameof(AsycudaManifestQueryMessageSendingObject.Schema.ManifestNumber),
				nameof(AsycudaManifestQueryMessageSendingObject.Schema.ParentDealNumber)
			};

			var actualColumns = grid.Columns.Select(c => c.ColumnName).ToArray();
			AssertArrayEqualsByElements(expectedColumns, actualColumns);
		}

		public void TestRunPreSendValidation_WhenManifestNumberIsEmpty()
		{
			using var dialog = (ManifestQuerySelectionDialog)GetFormToBash();
			var manifestQueryHeaderWrapper = (AsycudaManifestQueryMessageSendingObjectParent)dialog.DataSource;
			var manifestQuerySendObject = manifestQueryHeaderWrapper.SendingObjectsCollection[0];
			manifestQuerySendObject.ParentDealNumber = ZString.Empty;
			manifestQuerySendObject.ShouldSend = true;

			dialog.Show();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var sendButton = dialog.FindSingle<ZButton>("SendButton");
			sendButton.PerformClick();

			var lastMessageText = UnitTestUserNotification.Instance.LastMessage.Text;

			CombineAssertions(() =>
			{
				AssertContains("Manifest number and Parent Deal number (IL2) must have a value before sending", lastMessageText);
			});
		}

		protected override Form GetFormToBashCore()
		{
			var factory = Factory;
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestNumber = "123";
			var bill = header.Bills.AddNew();
			var asycudaTransportDocumentInfo = bill.TransportDocuments.AddNew();
			asycudaTransportDocumentInfo.CSI_Code = TransportDocsTypeList.Codes.IL2;
			asycudaTransportDocumentInfo.CSI_ReferenceNumber = "380";

			var messageSendingObjectParent = header.MessageSendingConfiguration.GetNewQueryMessageSendingObjectParent(header);
			return new ManifestQuerySelectionDialog(messageSendingObjectParent);
		}
	}
}
