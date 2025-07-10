using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingForm))]
	public class JobDeclarationMessageSendingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var sendingObjectParent = new ExportJobDeclarationMessageSendingObjectParent(declaration);
			return new JobDeclarationMessageSendingForm(sendingObjectParent);
		}

		public void TestCheckIsOKToSendXtTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "11";
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine = testInvHeader.InvoiceLines.AddNew();
			testInvLine.JI_CEI = testInst.PK;
			testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var declarationObject = new ExportJobDeclarationMessageSendingObjectParent(declaration);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			using (var form = new JobDeclarationMessageSendingForm(declarationObject))
			{
				form.Show();

				var sendButton = form.FindSingle<ZButton>("SendButton");

				form.MessageSendingObjectParent.SendingObjectsCollection.Cast<ExportDeclarationMessageSendingObject>().ForEach(x => x.ShouldSend = true);
				var sendWithWarning = form.FindSingle<ZCheckBox>("SendWithAdditionalWarningCheckBox");
				sendWithWarning.Checked = true;

				var sendWithError = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithError.Checked = true;

				sendButton.PerformClick();

				AssertEquals("The selected messages will be sent to a test environment!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCheckIsOKToSendXtProd()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "11";
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine = testInvHeader.InvoiceLines.AddNew();
			testInvLine.JI_CEI = testInst.PK;
			testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var declarationObject = new ExportJobDeclarationMessageSendingObjectParent(declaration);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtProduction.Code))
			using (var form = new JobDeclarationMessageSendingForm(declarationObject))
			{
				form.Show();

				var sendButton = form.FindSingle<ZButton>("SendButton");

				form.MessageSendingObjectParent.SendingObjectsCollection.Cast<ExportDeclarationMessageSendingObject>().ForEach(x => x.ShouldSend = true);
				var sendWithWarning = form.FindSingle<ZCheckBox>("SendWithAdditionalWarningCheckBox");
				sendWithWarning.Checked = true;

				var sendWithError = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithError.Checked = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendButton.PerformClick();

				Assert(!UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).Contains("The selected messages will be sent to a test environment!"));
			}
		}
	}
}
