using System.IO;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(CertificateOfOriginXmlUploaderGridContextMenuItemComponent))]
sealed class CertificateOfOriginXmlUploaderGridContextMenuItemComponentTest : GridContextMenuItemComponentAbstractTest<ITEDIMessage>
{
	public void TestUploadCertificateOfOriginXmlMenuItemVisibility()
	{
		using var form = new ZForm(declaration);
		using var messageUserControl = new MessageUserControl();
		form.Controls.Add(messageUserControl);
		form.Show();

		var tabControl = messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl");
		tabControl.SelectedTab = messageUserControl.FindSingle<ZTabPage>("MessageTabPage");
		var userControlMessages = (MessagesTabUserControl)messageUserControl.BaseMessageUserControl.HostedControl;
		var messagesGrid = userControlMessages.FindSingle<ZGrid>("MessagesGrid");
		messagesGrid.Select(0);

		CombineAssertions(() =>
		{
			var uploadCertificateOfOriginXmlMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText(MenuItemText);

			declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
			messagesGrid.ContextMenu.DoPopup();
			AssertEquals("When declaration is export has no MRN", false, uploadCertificateOfOriginXmlMenuItem.Visible);

			entryHeader.MovementReferenceNumberSetter("24ITQ3E080038828T1");
			declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
			messagesGrid.ContextMenu.DoPopup();
			AssertEquals("When declaration is export has MRN", true, uploadCertificateOfOriginXmlMenuItem.Visible);

			declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
			messagesGrid.ContextMenu.DoPopup();
			AssertEquals("When declaration is import", false, uploadCertificateOfOriginXmlMenuItem.Visible);
		});
	}

	public void TestUploadCertificateOfOriginXmlClick()
	{
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;

		using var tempFile = TempFile.New();
		using var form = new ZForm(declaration);
		using var messageUserControl = new MessageUserControl();
		form.Controls.Add(messageUserControl);
		form.Show();

		var tabControl = messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl");
		tabControl.SelectedTab = messageUserControl.FindSingle<ZTabPage>("MessageTabPage");
		var userControlMessages = (MessagesTabUserControl)messageUserControl.BaseMessageUserControl.HostedControl;
		var messagesGrid = userControlMessages.FindSingle<ZGrid>("MessagesGrid");

		CombineAssertions(() =>
		{
			var uploadCertificateOfOriginXmlMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText(MenuItemText);

			using (var streamWriter = new StreamWriter(File.Create(tempFile.Filename)))
			{
				streamWriter.Write("<CERTIFICATO_ATR></CERTIFICATO_ATR>");
			}
			messagesGrid.Select(0);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
			UnitTestUserNotification.Instance.ClearMessages();
			uploadCertificateOfOriginXmlMenuItem.PerformClick();
			AssertEquals("When uploading a supported file, LastMessage Text", "File has been uploaded.", UnitTestUserNotification.Instance.LastMessage.Text);

			using (var streamWriter = new StreamWriter(File.Create(tempFile.Filename)))
			{
				streamWriter.Write("<CERTIFICATO_XXX></CERTIFICATO_XXX>");
			}
			messagesGrid.Select(0);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
			UnitTestUserNotification.Instance.ClearMessages();
			uploadCertificateOfOriginXmlMenuItem.PerformClick();
			AssertEquals("When uploading an unsupported file, LastMessage Text", "The uploaded file is not supported.", UnitTestUserNotification.Instance.LastMessage.Text);
		});
	}

	protected override string MenuItemText => "Upload certificate of origin XML";

	protected override GridContextMenuItemComponent<ITEDIMessage> GetNewContextMenuItemComponent(ZGrid grid)
		=> new CertificateOfOriginXmlUploaderGridContextMenuItemComponent(grid);

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.Messages.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
}
