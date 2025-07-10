using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessagesTabUserControTest : TestCaseWithFactory
{
	public void TestUploadResponseFileMenuItemVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var message1 = entryHeader.Messages.AddNew();
		var message2 = entryHeader.Messages.AddNew();

		using (var form = new ZForm(declaration))
		using (var messageUserControl = new MessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var tabControl = messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl");
			tabControl.SelectedTab = messageUserControl.FindSingle<ZTabPage>("MessageTabPage");
			var userControlMessages = (MessagesTabUserControl)messageUserControl.BaseMessageUserControl.HostedControl;
			var messagesGrid = userControlMessages.FindSingle<ZGrid>("MessagesGrid");
			var uploadResponseFileMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Upload Response file");

			CombineAssertions("[Case 1]: no messages selected", () =>
			{
				messagesGrid.UnSelectAll();
				AssertEquals("No selected elements", 0, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !uploadResponseFileMenuItem.Visible);
			});

			CombineAssertions("[Case 2]: more than 1 message selected", () =>
			{
				messagesGrid.SelectAllElements();
				AssertEquals("2 elements selected", 2, messagesGrid.SelectedElements.Length);
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !uploadResponseFileMenuItem.Visible);
			});

			CombineAssertions("[Case 3]: the selected message has no interchange", () =>
			{
				messagesGrid.UnSelectAll();
				messagesGrid.Select(0);
				AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
				AssertNull("Selected element has no interchange", (messagesGrid.SelectedElements[0] as EDIMessage).Interchange);

				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !uploadResponseFileMenuItem.Visible);
			});

			var interchange = Factory.New<EDIInterchange>();
			interchange.ContainedMessages.Add(message1);

			CombineAssertions("[Case 4]: EM_MessageType NOT IN ('R','T')", () =>
			{
				messagesGrid.Select(0);
				AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
				AssertNotNull("Selected element has interchange", (messagesGrid.SelectedElements[0] as EDIMessage).Interchange);

				message1.EM_MessageType = SADConstants.CustomsInterchangeType.IrispX;
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item not visible", !uploadResponseFileMenuItem.Visible);
			});

			CombineAssertions("[Case 5]: EM_MessageType IN ('R','T')", () =>
			{
				messagesGrid.Select(0);
				AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
				AssertNotNull("Selected element has interchange", (messagesGrid.SelectedElements[0] as EDIMessage).Interchange);

				message1.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item visible for 'R'", uploadResponseFileMenuItem.Visible);

				message1.EM_MessageType = SADConstants.CustomsInterchangeType.IdocT;
				messagesGrid.ContextMenu.DoPopup();
				Assert("Menu item visible for 'T'", uploadResponseFileMenuItem.Visible);
			});
		}
	}

	public void TestUploadResponseFileMenuItemClick_UploadWithoutErrors()
	{
		sentInterchange.EI_HeaderText =
				   "<ITMessage>" +
					   "<Header>004R            004R1004.R04190015588917275100    01790970139     001 00005</Header>" +
				   "</ITMessage>";

		var filename = PerformUpload();
		AssertEquals($"Customs response file '{filename}' has been uploaded and waiting to be processed. Please refresh the form to view the results in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
	}

	public void TestUploadResponseFileMenuItemClick_UploadWithErrors()
	{
		const string expectedMessage = "The filename in the first row of the uploaded file does not match the filename of the selected IDOC.";
		sentInterchange.EI_HeaderText =
					  "<ITMessage>" +
						  "<Header>004R            99999999.999190015588917275100    01790970139     001 00005</Header>" +
					  "</ITMessage>";

		PerformUpload();
		AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
	}

	ZString PerformUpload()
	{
		using (var tempFile = TempFile.New())
		using (var form = new ZForm(declaration))
		using (var messageUserControl = new MessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var tabControl = messageUserControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl");
			tabControl.SelectedTab = messageUserControl.FindSingle<ZTabPage>("MessageTabPage");
			var userControlMessages = (MessagesTabUserControl)messageUserControl.BaseMessageUserControl.HostedControl;
			var messagesGrid = userControlMessages.FindSingle<ZGrid>("MessagesGrid");
			var uploadResponseFileMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Upload Response file");

			var file = File.Create(tempFile.Filename);
			using (var streamWriter = new StreamWriter(file))
			{
				streamWriter.Write(
					"004R            004R1004.X04190015588917275100    01790970139     001 00005    INVIO IN AMBIENTE REALE       \r\n" +
					" RICEVUTO  04/10/19 06:04,004R1004.R04\r\n" +
					" RL=002,RS=000,ME=001,MS=000,PE=001,PS=000\r\n" +
					"ESEGUITO   04/10/19  06:04\r\n" +
					"RIM          21309000275100P4 T 00031664X041019014135A000273G081119F081119 000000X91B0ZSVINCOLATA                                                                                                  ");
			}

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
			uploadResponseFileMenuItem.PerformClick();
			return Path.GetFileName(tempFile.Filename);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		sentMessage = entryHeader.Messages.AddNew();
		sentMessage.EM_MessageType = SADConstants.CustomsInterchangeType.IdocR;
		sentMessage.MessageNumberStrategy = new FixedMessageNumberStrategy("000001");

		sentInterchange = Factory.New<EDIInterchange>();
		sentInterchange.ContainedMessages.Add(sentMessage);
		sentInterchange.EI_ApplicationCode = EDIMessage.ApplicationCodes.ITCustoms;
		sentInterchange.EI_From = "FROM";
		sentInterchange.EI_To = "TO";
		sentInterchange.EI_SessionGUID = new ZGuid("8C2900CD-E6F1-482E-9D4A-0D4C60BA1292");
		sentInterchange.EI_Status = EDIInterchange.Status.Sent;
		Factory.Save();
	}

	JobDeclaration declaration;
	EDIInterchange sentInterchange;
	ITEDIMessage sentMessage;
}
