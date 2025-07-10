using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.DIF.Business;
using Enterprise.Customs.CA.DIF.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DIF.GUI.Testing
{
	[TestedType(typeof(DIFForm))]
	sealed class DIFFormTest : ZFormBasherTest
	{
		public void TestSecurityCheck()
		{
			var declaration = (ICADIFHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			Factory.Save();
			var hostWrapper = new DIFHostWrapper(declaration);
			Env.Security.CACustomsDIFEdit.IsAllowed = false;
			using (var form = new DIFForm(hostWrapper))
			{
				form.Show();
				AssertEquals(false, form.SendButton.Enabled);
			}
			Env.Security.CACustomsDIFEdit.IsAllowed = true;
			using (var form = new DIFForm(hostWrapper))
			{
				form.Show();
				AssertEquals(true, form.SendButton.Enabled);
				Env.Security.CACustomsDIFSendMessage.IsAllowed = false;
				Env.Security.CACustomsDIFSendWithMessageErrors.IsAllowed = false;
				form.SendButton.PerformClick();
				AssertEquals("Please enter at least one DIF document first.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				var difDocument = hostWrapper.DISDocuments.AddNew();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.CACustomsDIFSendMessage), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				Env.Security.CACustomsDIFSendMessage.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				difDocument.RequiredDocumentPK = requiredDocument.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals(Customs.Business.SingleMessageManager.MessageErrorsExistWithNoSecurityRight + " " + Env.Security.CACustomsDIFSendWithMessageErrors.DisplayTextPathToSecurityRight, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				Env.Security.CACustomsDIFSendWithMessageErrors.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendButton.PerformClick();
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestCancelledChangesNotSerialized()
		{
			const string xml1 = @"<DIFDocument xmlns=""http://www.cargowise.com/Schemas/DIFDocument"">
  <BusinessNumber>10000010</BusinessNumber>
  <PGA>12</PGA>
  <DocumentType>12345</DocumentType>
  <DocumentDescription>desc</DocumentDescription>
  <EDocsDocumentPK>3bf5d443-a5b6-457c-b7a9-d436dca3ea1e</EDocsDocumentPK>
  <Comment>Comment</Comment>
  <DocumentNumber>MaximumLength70</DocumentNumber>
  <EffectiveDate>2013-12-25T12:25:25</EffectiveDate>
  <ExpiryDate>2014-12-25T12:25:25</ExpiryDate>
</DIFDocument>";

			var declaration = (ICADIFHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			var addInfo = requiredDocument.AddInfos.AddNew();

			addInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF;
			addInfo.EX_AddInfo = xml1;

			Factory.Save();

			var hostWrapper = new DIFHostWrapper(declaration);

			using (var form1 = new DIFForm(hostWrapper))
			{
				form1.Show();

				Assert(!form1.SaveButton.Enabled);
				AssertEquals(1, hostWrapper.DISDocuments.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form1.DifUserControl.DocumentsGrid.CurrentRowIndex = 0;
				var deleteMenuButton = form1.DifUserControl.DocumentsGrid.ContextMenu.MenuItems.FindByText("Delete");
				deleteMenuButton.PerformClick();

				AssertEquals("Delete is stopped", 1, hostWrapper.DISDocuments.Count);
				Assert("no change is made", !hostWrapper.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form1.DifUserControl.DocumentsGrid.CurrentRowIndex = 0;
				deleteMenuButton = form1.DifUserControl.DocumentsGrid.ContextMenu.MenuItems.FindByText("Delete");
				deleteMenuButton.PerformClick();

				AssertEquals("Proceeded with Delete", 0, hostWrapper.DISDocuments.Count);
				Assert("change is made", hostWrapper.HasChanges);

				form1.CancelOrCloseButton.PerformClick();

				AssertContains("Should have asked if you would like to save changes", "This record has been modified", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var hostWrapper2 = new DIFHostWrapper(declaration);

			using (var form2 = new DIFForm(hostWrapper2))
			{
				form2.Show();

				AssertEquals("The cancelled action - Deleting a DIF record - should have been discarded", 1, hostWrapper2.DISDocuments.Count);
			}
		}

		public void TestSaveButton()
		{
			var declaration = (ICADIFHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var hostWrapper = new DIFHostWrapper(declaration);

			using (var form = new DIFForm(hostWrapper))
			{
				form.Show();

				Assert(!form.SaveButton.Enabled);
				var difDocument = hostWrapper.DISDocuments.AddNew();
				difDocument.RequiredDocumentPK = ZGuid.Empty;
				hostWrapper.HasChanges = true;//this usually happens when a new element is added to a grid on UI

				form.SaveButton.PerformClick();
				Assert("Precondition", difDocument.HasErrors);
				Assert("Should have validated and showed the reason why system cannot save", !requiredDocument.IsInDatabase);
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				difDocument.RequiredDocumentPK = requiredDocument.PK;
				Assert("Precondition", !difDocument.HasErrors);

				form.SaveButton.PerformClick();
				AssertNotContains("Should have validated and showed the reason why system cannot save", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("Saved", requiredDocument.IsInDatabase);
				Assert(!form.SaveButton.Enabled);
			}
		}

		public void TestSaveButton_DisabledWhenDisplayModeIsBrowse()
		{
			var declaration = (ICADIFHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;

			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			var difHostWrapper = new DIFHostWrapper(declaration);

			using (var form = new DIFForm(difHostWrapper))
			{
				AssertNotNull(form);

				form.Show();
				form.ControllerID = ControllerIDs.Customs.DocumentImageSystem;

				Assert(!form.SaveButton.Enabled);

				var difDocument = difHostWrapper.DISDocuments.AddNew();
				difDocument.RequiredDocumentPK = ZGuid.Empty;

				form.SaveButton.Enabled = true;

				form.DifUserControl.DocumentsGrid.CurrentRowIndex = 0;
				var deleteMenuButton = form.DifUserControl.DocumentsGrid.ContextMenu.MenuItems.FindByText("Delete");

				difHostWrapper.DISDocuments.RemoveAndDelete(difDocument);
				difHostWrapper.HasChanges = false;//this usually happens when a new element is deleted from a grid on UI
				form.SaveButton.Enabled = true;//this usually happens when a new element is deleted from a grid on UI

				deleteMenuButton.PerformClick();

				if (form.SaveButton.Enabled)
				{
					form.SaveButton.PerformClick();

					difHostWrapper.HasChanges = true;
					form.DisplayMode = ODisplayMode.Browse;
					form.ControllerID = ControllerIDs.Customs.DocumentImageSystem;

					AssertNoExceptionThrown(() =>
					{
						form.SaveButton.PerformClick();
					});
				}
				else
				{
					Assert(!form.SaveButton.Enabled);
				}
			}
		}

		public void TestButtonText()
		{
			HostWrapper.HasChanges = false;

			using (var form = new DIFForm(HostWrapper))
			{
				form.Show();
				AssertEquals("&Close", form.CancelOrCloseButton.CaptionResourceString.Caption);
				AssertEquals("&Close", form.CancelOrCloseButton.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("&Send", form.SendButton.CaptionResourceString.Caption);
				AssertEquals("&Send", form.SendButton.Extensions.Get<ILabelCaptionRenderer>().Caption);

				HostWrapper.HasChanges = true;
				AssertEquals("&Cancel", form.CancelOrCloseButton.CaptionResourceString.Caption);
				AssertEquals("&Cancel", form.CancelOrCloseButton.Extensions.Get<ILabelCaptionRenderer>().Caption);
				AssertEquals("Save && &Send", form.SendButton.CaptionResourceString.Caption);
				AssertEquals("Save && &Send", form.SendButton.Extensions.Get<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestSendButtonWithMessageError()
		{
			var declaration = (ICADIFHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			using (var form = new DIFForm(HostWrapper))
			{
				form.Show();

				var difDocument = HostWrapper.DISDocuments.AddNew();

				var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
				difDocument.RequiredDocumentPK = requiredDocument.PK;
				difDocument.EDocsDocumentPK = eDocs.UniqueKey;
				difDocument.EffectiveDate = new ZDateTime(2017, 1, 1);
				difDocument.ExpiryDate = new ZDateTime(2016, 1, 1);
				difDocument.RunPreSaveValidation();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				Assert("Precondition", !difDocument.HasErrors);
				Assert("Precondition", difDocument.HasMessageErrors);

				form.SendButton.PerformClick();
				Assert("Saved", requiredDocument.IsInDatabase);
				AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestSendButtonWithoutDocuments()
		{
			using (var form = new DIFForm(HostWrapper))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.SendButton.PerformClick();
				AssertContains("Please enter at least one DIF document first.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWordWrapDisabledForXml()
		{
			using (var form = new DIFForm(HostWrapper))
			{
				form.Show();

				// WordWrap must be disabled.
				// WordWrap causes performance problems because <ImageData> contains long encoded binary string.
				AssertEquals("WordWrap should be disabled", false, form.DifUserControl.InterpretedTextBox.WordWrap);
			}
		}

		public void TestRefreshFromAccordingToDIFDocument()
		{
			using (var form = new DIFForm(HostWrapper))
			{
				form.Show();
				Application.DoEvents();
				Assert(form.DifUserControl.RequiredDocumentsPanel.Visible);
				Assert(!form.DifUserControl.MessageSendPanel.Visible);
			}

			var difDocument1 = HostWrapper.DISDocuments.AddNew();
			difDocument1.Comment = "CFIA";
			var difDocument2 = HostWrapper.DISDocuments.AddNew();
			difDocument2.Comment = "HC";
			var difDocument3 = HostWrapper.DISDocuments.AddNew();
			difDocument3.Comment = "GAC";
			using (var form = new DIFForm(difDocument2))
			{
				form.Show();
				Application.DoEvents();
				Assert(!form.DifUserControl.RequiredDocumentsPanel.Visible);
				Assert(form.DifUserControl.MessageSendPanel.Visible);

				AssertEquals("difDocument2 should be selected", "HC", form.DifUserControl.CommentTextBox.Text);
			}
		}

		public void TestSendMessage_DIFDocumentIsNotNull()
		{
			var declaration = (ICADIFHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var requiredDocument = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			Factory.Save();
			var hostWrapper = new DIFHostWrapper(declaration);
			var difDocument1 = hostWrapper.DISDocuments.AddNew();
			difDocument1.RequiredDocumentPK = requiredDocument.PK;
			var difDocument2 = hostWrapper.DISDocuments.AddNew();
			Env.Security.CACustomsDIFEdit.IsAllowed = false;
			using (var form = new DIFForm(difDocument1))
			{
				form.Show();
				AssertEquals(false, form.SendButton.Enabled);
			}
			Env.Security.CACustomsDIFEdit.IsAllowed = true;
			using (var form = new DIFForm(difDocument2))
			{
				form.Show();
				AssertEquals(true, form.SendButton.Enabled);
				Env.Security.CACustomsDIFSendMessage.IsAllowed = false;
				Env.Security.CACustomsDIFSendWithMessageErrors.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.CACustomsDIFSendMessage), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				Env.Security.CACustomsDIFSendMessage.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				difDocument2.RequiredDocumentPK = requiredDocument.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.SendButton.PerformClick();
				AssertEquals(Customs.Business.SingleMessageManager.MessageErrorsExistWithNoSecurityRight + " " + Env.Security.CACustomsDIFSendWithMessageErrors.DisplayTextPathToSecurityRight, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				Env.Security.CACustomsDIFSendWithMessageErrors.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.SendButton.PerformClick();
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendButton.PerformClick();
				AssertContains("Message Sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				difDocument2.Messages.Load();
				AssertEquals(1, difDocument2.Messages.Count);
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Get<Integration.Customs.CA.ICACustomsDataRegistry>().AccountSecurityNo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");

			DbConnection connection = ((IDbConnected)Factory).Connection;
			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("GeneratorFountain-C-8E86F7C5", Guid.Empty);
				ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("GeneratorFountain-C-7BE2F10C", Guid.Empty);

				var companyFountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber12345");
				companyFountain.SetNext(Factory, 2000);
				var difFountain = Env.NumberFountains.GetCAEntryNumberGeneratorFountain("CADeclarationTransactionNumber12345DIF");
				difFountain.SetNext(Factory, 777);
				transactionManager.CommitTransaction();
			}
		}

		protected override Form GetFormToBashCore() => new DIFForm(HostWrapper);

		DIFHostWrapper hostWrapper;
		DIFHostWrapper HostWrapper => hostWrapper ?? (hostWrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration));

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
