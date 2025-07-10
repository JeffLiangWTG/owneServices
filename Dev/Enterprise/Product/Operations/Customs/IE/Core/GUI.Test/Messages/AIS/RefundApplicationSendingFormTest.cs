using System.Windows.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(RefundApplicationSendingForm))]
	class RefundApplicationSendingFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = (RefundApplicationSendingForm)GetFormToBashCore())
			{
				AssertEquals("Form Heading", "Refund applications", form.FormHeading);
			}
		}

		public void TestMessageSendingGridColumns()
		{
			using (var form = GetFormToBashCore())
			{
				AssertNotNull(form.FindSingle<ZGrid>("MessageSendingObjectsGrid"));
			}
		}

		public void TestMessageSendingObjectsGroupBox_Caption()
		{
			using (var form = GetFormToBashCore())
			{
				var messageSendingObjectsGroupBox = form.FindSingle<ZGroupBox>("messageSendingObjectsGroupBox");
				AssertNotNull(messageSendingObjectsGroupBox);
				AssertEquals("Refund Applications", messageSendingObjectsGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestValidationErrorsGroupBoxIsHidden()
		{
			using (var form = GetFormToBashCore())
			{
				var validationErrorsGroupBox = form.FindSingle<ZGroupBox>("ValidationErrorsGroupBox");
				Assert("ValidationErrorsGroupBox is hidden", !validationErrorsGroupBox.Visible);
			}
		}

		public void TestDocumentsGroupBox_Caption()
		{
			using (var form = GetFormToBashCore())
			{
				var documentsGroupBox = form.FindSingle<ZGroupBox>("DocumentsGroupBox");
				AssertNotNull(documentsGroupBox);
				AssertEquals("Documents", documentsGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestDocumentsGridGridColumns()
		{
			using (var form = GetFormToBashCore())
			{
				var grid = form.FindSingle<ZGrid>("DocumentsGrid");
				AssertNotNull("DocumentsGrid", grid);
				AssertEquals("DocumentsGrid ColumnStyles Count", 3, grid.ColumnStyles.Count);

				var documentType = grid.GetColumnStyle("DocumentType");
				AssertType("Type", typeof(ZCodeFindBoxColumnStyleInfo), documentType);
				var documentIdentifier = grid.GetColumnStyle("DocumentIdentifier");
				AssertType("Type", typeof(ZTextBoxColumnStyleInfo), documentIdentifier);
				var documentDate = grid.GetColumnStyle("DocumentDate");
				AssertType("Type", typeof(ZDateEditColumnStyleInfo), documentDate);
				AssertEquals("DateTimeFormat should be short", ZArchitecture.Core.ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)documentDate).DateTimeFormat);
			}
		}

		public void TestPreviewMessageCheckboxVisible()
		{
			using (var form = (RefundApplicationSendingForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals("PreviewMessageCheckBox visibility", true, form.PreviewMessageCheckBox.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var enryHeader = declaration.CustomsEntryHeaders.AddNew();
			enryHeader.CH_CEI_Instruction = Factory.NewWithValidTestData<CusEntryInstruction>().PK;
			var messageSendingObjectParent = new RefundApplicationMessageSendingActionParent(declaration);
			return new RefundApplicationSendingForm(messageSendingObjectParent);
		}
	}
}
