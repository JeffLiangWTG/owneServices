using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Business.Testing;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(MSXMessageSendingForm))]
	public class MSXMessageSendingFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();
			header.CH_PhaseStatus = CustomsDeclarationPhases.Codes.IDC;
			var parent = new MSXMessageSendingObjectParent(declaration);
			return new MSXMessageSendingForm(parent);
		}

		public void TestMessageSendingObjectsGrid()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();
			header.CH_PhaseStatus = CustomsDeclarationPhases.Codes.IDC;
			var parent = new MSXMessageSendingObjectParent(declaration);
			using (var form = new MSXMessageSendingForm(parent))
			{
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				var phaseColumnStyleInfo = grid.GetColumnStyle("Header+CH_PhaseStatus");
				AssertNotNull(grid.GetColumnStyle("Header+EntryNumber"));
				AssertNotNull(phaseColumnStyleInfo);
				AssertNotNull(grid.GetColumnStyle("Header+PhaseDescription"));
				AssertNotNull(grid.GetColumnStyle("RegistrationType"));
				AssertNotNull(grid.GetColumnStyle("Communication"));
				Assert(phaseColumnStyleInfo.IsReadOnly);
			}
		}

		public void TestSendButtonEnabled()
		{
			var declaration = Factory.NewWithValidTestData<DeclarationForTestSendingObject>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_PhaseStatus = CustomsDeclarationPhases.Codes.EDC;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var messageSendingParent = new MSXMessageSendingObjectParent(declaration);
			using var form = new MSXMessageSendingForm(messageSendingParent);
			form.Show();

			var sendButton = form.FindSingle<ZButton>("SendButton");
			Assert(!sendButton.Enabled);

			var messageSendingObject = messageSendingParent.SendingObjectsCollection.Cast<MSXMessageSendingObject>().FirstOrDefault();
			messageSendingObject.ShouldSend = true;
			Assert(!sendButton.Enabled);

			SetCompanyWithMailboxCredential(declaration);
			messageSendingObject.ShouldSend = false;
			messageSendingObject.ShouldSend = true;
			Assert(sendButton.Enabled);
		}

		public void TestDocumentsGrid()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var parent = new MSXMessageSendingObjectParent(declaration);
			using (var form = new MSXMessageSendingForm(parent))
			{
				var grid = form.FindSingle<ZGrid>("DocumentsGrid");
				AssertNotNull(grid.GetColumnStyle("File"));
				AssertNotNull(grid.GetColumnStyle("Type"));
				AssertNotNull(grid.GetColumnStyle("TypeDescription"));
				AssertNotNull(grid.GetColumnStyle("FileSize"));
				AssertEquals("MaximumRows of DocumentsGrid must be equal to MSXMessageSendingObjectAttachmentCollection.MaxRowCount", MSXMessageSendingObjectAttachmentCollection.MaxRowCount, grid.MaximumRows);
			}
		}

		public void TestExportPathTextBox()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var parent = new MSXMessageSendingObjectParent(declaration);
			using var form = new MSXMessageSendingForm(parent);
			var exportPathTextBox = form.FindSingle<ZTextBox>("ExportPathTextBox");

			AssertEquals("ExportPathTextBox Anchor", AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom, exportPathTextBox.Anchor);
		}

		void SetCompanyWithMailboxCredential(JobDeclaration declaration)
		{
			var settings = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "TEST", DomainName = "TEST", Status = XtCredentialStatusList.Codes.Registered };
			JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var wrapper = GlbCompanyWrapper.GetWrapper<JPGlbCompanyWrapper>(declaration.Company);
			var mailboxCredential = wrapper.MailboxCredential;
			mailboxCredential.GP_MailBoxID = "TEST";
			mailboxCredential.CurrentDecryptedPassword = "TEST";
		}
	}
}
