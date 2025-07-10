using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.GUI.Testing;
using Enterprise.Customs.IE.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.GUI.Testing
{
	[TestedType(typeof(RF415MessageSendingForm))]
	sealed class RF415MessageSendingFormTest : ZFormBasherTest
	{
		public void TestMessageSendingObjectsGridColumns()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var messageSendingObjectParent = new RF415MessageSendingObjectParent(header);
			using (var form = new RF415MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();
				var messageSendingObjectsGrid = form.Controls.Find("MessageSendingObjectsGrid", searchAllChildren: true).Single() as ZGrid;

				EUH7GUITestHelper.AssertGridLayout(messageSendingObjectsGrid,
					[
						"ShouldSend",
						"MovementReferenceNumber",
						"BillNumber",
						"RefundType",
						"OfficeOfDebt",
						"OfficeOfResponsibility",
						"LegalBasis",
						"DescriptionOfGrounds",
						"BankDetails",
						"Amount",
						"AdditionalInformation"
					]);
			}
		}

		public void TestDocumentsGridColumns()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var messageSendingObjectParent = new RF415MessageSendingObjectParent(header);
			using (var form = new RF415MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();
				var documentsGrid = form.Controls.Find("DocumentsGrid", searchAllChildren: true).Single() as ZGrid;

				EUH7GUITestHelper.AssertGridLayout(documentsGrid,
					[
						("DocumentType", typeof(ZCodeFindBoxColumnStyleInfo)),
						("DocumentIdentifier", typeof(ZTextBoxColumnStyleInfo)),
						("DocumentDate", typeof(ZDateEditColumnStyleInfo))
					]);

				var documentDateColumn = documentsGrid.GetColumnStyle("DocumentDate");
				AssertEquals("DateTimeFormat should be short", ZArchitecture.Core.ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)documentDateColumn).DateTimeFormat);
			}
		}

		public void TestShouldShowProgressForm()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var messageSendingObjectParent = new RF415MessageSendingObjectParent(header);
			using var form = new RF415MessageSendingFormForTesting(messageSendingObjectParent);

			AssertEquals("Should not show progress form", false, form.ShouldShowProgressForm);
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var messageSendingObjectParent = new RF415MessageSendingObjectParent(header);
			return new RF415MessageSendingForm(messageSendingObjectParent);
		}

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		protected override bool AllowHasChangesOnFormOpen => true;
	}

	class RF415MessageSendingFormForTesting : RF415MessageSendingForm
	{
		public RF415MessageSendingFormForTesting(BaseMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		public new bool ShouldShowProgressForm => base.ShouldShowProgressForm;
	}
}
