using System.Windows.Forms;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageMessageSendingForm))]
	sealed class UCC6TemporaryStorageMessageSendingFormTest : Customs.GUI.Testing.MessageSendingFormWithValidationDetailsAbstractTest
	{
		public void TestPreviewMessageCheckboxVisible()
		{
			using (var form = (UCC6TemporaryStorageMessageSendingForm)GetFormToBash())
			{
				form.Show();

				AssertEquals("Preview message check box visibility", true, form.PreviewMessageCheckBox.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var messageSendingHeader = new TemporaryStorageMessageSendingObjectParent(storageHeader);
			return new UCC6TemporaryStorageMessageSendingForm(messageSendingHeader);
		}

		public void TestMessageSendingGridColumns()
		{
			using (var form = GetFormToBashCore())
			{
				var messageSendingGrid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				AssertNotNull("MessageSendingObjectsGrid", messageSendingGrid);
				AssertEquals("MessageSendingObjectsGrid ColumnStyles Count", 8, messageSendingGrid.ColumnStyles.Count);
			}
		}
	}
}
