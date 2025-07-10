using System;
using System.Windows.Forms;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	[TestedType(typeof(DocumentsSendingForm))]
	class DocumentsSendingFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			using (var form = new DocumentsSendingForm())
			{
				form.Show();
				AssertEquals("Upload Supporting Documents", form.FormHeading);
			}
		}

		public void TestBinding()
		{
			using (var form = GetFormToBash())
			{
				var mrnTextBox = form.GetControl<ZTextBox>("MrnTextBox");
				AssertNotNull(mrnTextBox);
				AssertEquals(mrnTextBox.BindTo, "SendingObjectsCollection.MovementReference");

				var lrnTextBox = form.GetControl<ZTextBox>("LrnTextBox");
				AssertNotNull(lrnTextBox);
				AssertEquals(lrnTextBox.BindTo, "SendingObjectsCollection.LocalReference");
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override bool AllowSaveOnFormForTestHasChanges => false;

		public override Type FormToBashType => typeof(DocumentsSendingForm);

		protected override Form GetFormToBashCore()
		{
			var cusExitReport = Factory.NewWithValidTestData<CusExitHeader>().CusExitReports.AddNew();
			var messageSendingObjectParent = new DocumentsSendingActionParent(cusExitReport, AESOutgoingMessageTypeList.Codes.DocumentUpload);
			return new DocumentsSendingForm(messageSendingObjectParent);
		}
	}
}
