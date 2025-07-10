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
	[TestedType(typeof(DepositRefundApplicationSendingForm))]
	sealed class DepositRefundApplicationSendingFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = (DepositRefundApplicationSendingForm)GetFormToBashCore())
			{
				AssertEquals("Form Heading", "Deposit Refund Application", form.FormHeading);
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
				AssertEquals("Deposit Refund Application", messageSendingObjectsGroupBox.CaptionResourceString.Caption);
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

		public void TestSplitContainer_Panel2Collapsed()
		{
			using (var form = (DepositRefundApplicationSendingForm)GetFormToBash())
			{
				form.Show();
				var splitContainer = form.FindSingle<CargoWise.Windows.UI.KSplitContainer>("SplitContainer");
				AssertEquals("Panel2Collapsed", true, splitContainer.Panel2Collapsed);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var enryHeader = declaration.CustomsEntryHeaders.AddNew();
			enryHeader.CH_CEI_Instruction = Factory.NewWithValidTestData<CusEntryInstruction>().PK;
			var messageSendingObjectParent = new DepositRefundApplicationMessageSendingActionParent(declaration);
			return new DepositRefundApplicationSendingForm(messageSendingObjectParent);
		}

		public void TestPreviewMessageCheckboxVisible()
		{
			using (var form = (DepositRefundApplicationSendingForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals("PreviewMessageCheckBox visibility", true, form.PreviewMessageCheckBox.Visible);
			}
		}
	}
}
