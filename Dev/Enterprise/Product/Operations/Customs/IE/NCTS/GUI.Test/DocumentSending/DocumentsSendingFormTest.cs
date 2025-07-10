using System;
using System.Windows.Forms;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.GUI.Testing
{
	[TestedType(typeof(DocumentsSendingForm))]
	class DocumentsSendingFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = GetFormToBashCore() as ZForm)
			{
				AssertEquals("FormHeading", "Upload Supporting Documents", form.FormHeading);
				form.Show();
				AssertEquals("Text", "Upload Supporting Documents", form.Text);
			}
		}

		public void TestSize()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				AssertEquals("MinimumSize", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 598, true), form.MinimumSize);
			}
		}

		public void TestShowFormType()
		{
			DocumentsSendingForm.ShowForm(nctsHeader);
			AssertType<DocumentsSendingForm>("Dialog form type = DocumentsSendingForm", ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestCaptions()
		{
			using (var form = new DocumentsSendingForm(sendingActionParent))
			{
				AssertEquals("SupportingDocumentsGroupBox.CaptionResourceString", "Supporting Documents", form.SupportingDocumentsGroupBox.CaptionResourceString.Caption);
				AssertEquals("AdditionalInformationsGroupBox.CaptionResourceString", "Additional Information", form.AdditionalInformationsGroupBox.CaptionResourceString.Caption);
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		public override Type FormToBashType => typeof(DocumentsSendingForm);

		protected override Form GetFormToBashCore() => new DocumentsSendingForm(sendingActionParent);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			sendingActionParent = new DocumentSendingActionParent(nctsHeader);
		}
		NctsHeader nctsHeader;
		DocumentSendingActionParent sendingActionParent;
	}
}
