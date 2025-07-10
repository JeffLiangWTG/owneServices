using System.IO;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.GUI.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

[TestedType(typeof(MessagesTabUserControl))]
sealed class MessageTabUserControl_EDIMessageExporterTest : GridContextMenuItemComponentAbstractTest<ITEDIMessage>
{
	[RequiresSTA]
	public void TestSaveMessageToDiskMenuItemVisibility()
	{
		using var form = GetNewZForm();
		using var messageUserControl = GetNewMessageUserControl();
		messageUserControl.SetDataBinding(nctsHeader, "Messages");
		form.Controls.Add(messageUserControl);
		form.Show();

		var messagesGrid = GetMessageGrid(messageUserControl);
		var saveMessageDiskMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText(MenuItemText);

		CombineAssertions("[Case 1]: no messages selected", () =>
		{
			messagesGrid.UnSelectAll();
			AssertEquals("No selected elements", 0, messagesGrid.SelectedElements.Length);
			messagesGrid.ContextMenu.DoPopup();
			Assert("Menu item not visible", !saveMessageDiskMenuItem.Visible);
		});

		CombineAssertions("[Case 2]: 1 message selected", () =>
		{
			messagesGrid.Select(0);
			AssertEquals("1 element selected", 1, messagesGrid.SelectedElements.Length);
			messagesGrid.ContextMenu.DoPopup();
			Assert("Menu item visible", saveMessageDiskMenuItem.Visible);
		});

		CombineAssertions("[Case 3]: more than 1 message selected", () =>
		{
			messagesGrid.SelectAllElements();
			AssertEquals("2 elements selected", 2, messagesGrid.SelectedElements.Length);
			messagesGrid.ContextMenu.DoPopup();
			Assert("Menu item visible", saveMessageDiskMenuItem.Visible);
		});
	}

	[RequiresSTA]
	public void TestSaveMessageToDisk()
	{
		using var form = GetNewZForm();
		using var messageUserControl = GetNewMessageUserControl();
		form.Controls.Add(messageUserControl);
		form.Show();

		var messagesGrid = GetMessageGrid(messageUserControl);

		var saveMessageDiskMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText(MenuItemText);
		messagesGrid.Select(0);

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		ZFormModaliser.PathToSelectInShowCommonDialog = _tempPath;
		saveMessageDiskMenuItem.PerformClick();
		AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		var text = UnitTestUserNotification.Instance.LastMessage.Text;
		AssertEquals("1 message(s) were exported", UnitTestUserNotification.Instance.LastMessage.Text);

		var sentMessageFilePath = Path.Combine(_tempPath, "EdiMessage_ITM_000001.txt");
		if (File.Exists(sentMessageFilePath))
		{
			Assert("File has been created", true);
			var result = File.ReadAllText(sentMessageFilePath);
			AssertEquals("File written correctly", sentMessageText, result);
		}
		else
		{
			Fail("File has not been created");
		}
	}

	[RequiresSTA]
	public void TestSaveMessageToDisk_MultipleSelection()
	{
		using var form = GetNewZForm();
		using var messageUserControl = GetNewMessageUserControl();
		form.Controls.Add(messageUserControl);
		form.Show();

		var messagesGrid = GetMessageGrid(messageUserControl);
		var saveMessageDiskMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText(MenuItemText);
		messagesGrid.SelectAllElements();

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		ZFormModaliser.PathToSelectInShowCommonDialog = _tempPath;
		saveMessageDiskMenuItem.PerformClick();
		AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		var text = UnitTestUserNotification.Instance.LastMessage.Text;
		AssertEquals("2 message(s) were exported", UnitTestUserNotification.Instance.LastMessage.Text);

		var sentMessageFilePath = Path.Combine(_tempPath, "EdiMessage_ITM_000001.txt");
		var receivedMessageFilePath = Path.Combine(_tempPath, "EdiMessage_ITM_000002.txt");

		if (File.Exists(sentMessageFilePath) && File.Exists(receivedMessageFilePath))
		{
			Assert("Files have been created", true);
		}
		else
		{
			Fail("Files have not been created");
		}
	}

	[RequiresSTA]
	public void TestSaveMessageToDisk_CloseBrowseDialog()
	{
		using var form = GetNewZForm();
		using var messageUserControl = GetNewMessageUserControl();
		form.Controls.Add(messageUserControl);
		form.Show();

		var messagesGrid = GetMessageGrid(messageUserControl);
		var saveMessageDiskMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText(MenuItemText);
		messagesGrid.Select(0);

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		ZFormModaliser.PathToSelectInShowCommonDialog = _tempPath;
		saveMessageDiskMenuItem.PerformClick();
		AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());

		var sentMessageFilePath = Path.Combine(_tempPath, "EdiMessage_ITM_000001.txt");
		AssertEquals("File has not been written.", false, File.Exists(sentMessageFilePath));
	}

	[RequiresSTA]
	public void TestSaveMessageToDisk_Overwrite()
	{
		using var form = GetNewZForm();
		using var messageUserControl = GetNewMessageUserControl();
		form.Controls.Add(messageUserControl);
		form.Show();

		var messagesGrid = GetMessageGrid(messageUserControl);
		var saveMessageDiskMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText(MenuItemText);
		messagesGrid.Select(0);

		var sentMessageFilePath = Path.Combine(_tempPath, "EdiMessage_ITM_000001.txt");

		File.WriteAllText(sentMessageFilePath, "");

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		ZFormModaliser.PathToSelectInShowCommonDialog = _tempPath;
		saveMessageDiskMenuItem.PerformClick();
		AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
		AssertEquals("The folder already contains one or more files with the same names. If you continue, the files will be overwritten. Do you want to continue?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		AssertEquals("1 message(s) were exported", UnitTestUserNotification.Instance.LastMessage.Text);

		if (File.Exists(sentMessageFilePath))
		{
			Assert("File has been overwritten", true);
			var result = File.ReadAllText(sentMessageFilePath);
			AssertEquals("File overwritten correctly", sentMessageText, result);
		}
		else
		{
			Fail("File has not been created");
		}
	}

	[RequiresSTA]
	public void TestSaveMessageToDisk_CancelOverwrite()
	{
		using var form = GetNewZForm();
		using var messageUserControl = GetNewMessageUserControl();
		form.Controls.Add(messageUserControl);
		form.Show();

		var messagesGrid = GetMessageGrid(messageUserControl);
		var saveMessageDiskMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText(MenuItemText);
		messagesGrid.Select(0);

		var sentMessageFilePath = Path.Combine(_tempPath, "EdiMessage_ITM_000001.txt");

		File.WriteAllText(sentMessageFilePath, string.Empty);

		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		ZFormModaliser.PathToSelectInShowCommonDialog = _tempPath;

		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

		saveMessageDiskMenuItem.PerformClick();

		AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());

		AssertEquals("The folder already contains one or more files with the same names. If you continue, the files will be overwritten. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
		AssertEquals("File has not been overwrite", string.Empty, File.ReadAllText(sentMessageFilePath));
	}

	protected override void SetUp()
	{
		base.SetUp();

		_tempPath = EnvProxy.Instance.TempPath;

		nctsHeader = Factory.NewDepartureNctsHeader();
		var sentMessage = nctsHeader.Messages.AddNew();
		sentMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.ITCustoms;
		sentMessage.EM_MessageNum = "000001";
		sentMessage.EM_MessageText = sentMessageText;

		var sentInterchange = Factory.New<EDIInterchange>();
		sentInterchange.ContainedMessages.Add(sentMessage);

		var receivedMessage = nctsHeader.Messages.AddNew();
		receivedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.ITCustoms;
		receivedMessage.EM_MessageNum = "000002";
		receivedMessage.EM_MessageText = receivedMessageText;

		var receivedInterchange = Factory.New<EDIInterchange>();
		receivedInterchange.ContainedMessages.Add(receivedMessage);
	}

	protected override void TearDown()
	{
		base.TearDown();

		File.Delete(Path.Combine(_tempPath, "EdiMessage_ITM_000001.txt"));
		File.Delete(Path.Combine(_tempPath, "EdiMessage_ITM_000002.txt"));
	}

	NctsHeader nctsHeader;

	string _tempPath;

	ZForm GetNewZForm() => new ZForm(nctsHeader);

	ZUserControl GetNewMessageUserControl()
	{
		var control = new MessagesTabUserControl();
		control.SetDataBinding(nctsHeader, "Messages");
		return control;
	}

	ZGrid GetMessageGrid(object parentUserControl)
	{
		var messageUserControl = (MessagesTabUserControl)parentUserControl;
		return messageUserControl.FindSingle<ZGrid>("MessageGrid");
	}

	protected override GridContextMenuItemComponent<ITEDIMessage> GetNewContextMenuItemComponent(ZGrid grid)
		=> new EDIMessageExporterGridContextMenuItemComponent(grid, ResString.GetMultilingualString("E38400AF-9D6D-43C8-B625-67F815D08A3A", "Save Message to Disk"));

	protected override string MenuItemText => "Save Message to Disk";

	const string sentMessageText = "<soapenv:Envelope xmlns:tns=\"http://transitoservice.domest.sogei.it\" xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"><soapenv:Header /><soapenv:Body><tns:Input><tns:serviceId>invioDichiarazione</tns:serviceId><tns:data><tns:xml>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz4NCjxNZXNzYWdlPg0KICA8RGVjbGFyYXRpb25EMT4NCiAgICA8RGljaENvbXBsPlM8L0RpY2hDb21wbD4NCiAgICA8TGFzdFNlbmQ+UzwvTGFzdFNlbmQ+DQogICAgPERhdGFEMT4NCiAgICAgIDxUcmFuc2l0T3BlcmF0aW9uPg0KICAgICAgICA8TWVzc2FnZUluZm9ybWF0aW9uPg0KICAgICAgICAgIDxTZWN1cml0eT4wPC9TZWN1cml0eT4NCiAgICAgICAgICA8UmVkdWNlZERhdGFzZXRJbmRpY2F0b3I+MDwvUmVkdWNlZERhdGFzZXRJbmRpY2F0b3I+DQogICAgICAgIDwvTWVzc2FnZUluZm9ybWF0aW9uPg0KICAgICAgICA8UmVmZXJlbmNlc01lc3NhZ2VzRG9jdW1lbnRzQ2VydGlmaWNhdGVzQXV0aG9yaXNhdGlvbnM+DQogICAgICAgICAgPExSTj4yMDI0V1RMUkYyMDAwMDAwMDAwMTQwPC9MUk4+DQogICAgICAgIDwvUmVmZXJlbmNlc01lc3NhZ2VzRG9jdW1lbnRzQ2VydGlmaWNhdGVzQXV0aG9yaXNhdGlvbnM+DQogICAgICAgIDxQYXJ0aWVzPg0KICAgICAgICAgIDxIb2xkZXJPZlRoZVRyYW5zaXRQcm9jZWR1cmU+DQogICAgICAgICAgICA8SWRlbnRpZmljYXRpb25OdW1iZXI+SVRURVNULUVPUjwvSWRlbnRpZmljYXRpb25OdW1iZXI+DQogICAgICAgICAgPC9Ib2xkZXJPZlRoZVRyYW5zaXRQcm9jZWR1cmU+DQogICAgICAgIDwvUGFydGllcz4NCiAgICAgICAgPFBsYWNlc0NvdW50cmllc1JlZ2lvbnM+DQogICAgICAgICAgPEJpbmRpbmdJdGluZXJhcnk+MDwvQmluZGluZ0l0aW5lcmFyeT4NCiAgICAgICAgPC9QbGFjZXNDb3VudHJpZXNSZWdpb25zPg0KICAgICAgICA8Q3VzdG9tc09mZmljZXMgLz4NCiAgICAgICAgPE90aGVyRGF0YUVsZW1lbnRzIC8+DQogICAgICA8L1RyYW5zaXRPcGVyYXRpb24+DQogICAgICA8Q29uc2lnbm1lbnQ+DQogICAgICAgIDxSZWZlcmVuY2VzTWVzc2FnZXNEb2N1bWVudHNDZXJ0aWZpY2F0ZXNBdXRob3Jpc2F0aW9ucyAvPg0KICAgICAgICA8UGFydGllcyAvPg0KICAgICAgICA8VmFsdWF0aW9uSW5mb3JtYXRpb25UYXhlcyAvPg0KICAgICAgICA8UGxhY2VzQ291bnRyaWVzUmVnaW9ucyAvPg0KICAgICAgICA8R29vZHNJZGVudGlmaWNhdGlvbj4NCiAgICAgICAgICA8R3Jvc3NNYXNzPjEwPC9Hcm9zc01hc3M+DQogICAgICAgIDwvR29vZHNJZGVudGlmaWNhdGlvbj4NCiAgICAgICAgPFRyYW5zcG9ydEluZm9ybWF0aW9uPg0KICAgICAgICAgIDxDb250YWluZXJJbmRpY2F0b3I+MDwvQ29udGFpbmVySW5kaWNhdG9yPg0KICAgICAgICA8L1RyYW5zcG9ydEluZm9ybWF0aW9uPg0KICAgICAgPC9Db25zaWdubWVudD4NCiAgICAgIDxIb3VzZUNvbnNpZ25tZW50Pg0KICAgICAgICA8TWVzc2FnZUluZm9ybWF0aW9uPg0KICAgICAgICAgIDxIb3VzZUNvbnNpZ25tZW50SXRlbT4xPC9Ib3VzZUNvbnNpZ25tZW50SXRlbT4NCiAgICAgICAgPC9NZXNzYWdlSW5mb3JtYXRpb24+DQogICAgICAgIDxSZWZlcmVuY2VzTWVzc2FnZXNEb2N1bWVudHNDZXJ0aWZpY2F0ZXNBdXRob3Jpc2F0aW9ucyAvPg0KICAgICAgICA8UGFydGllcyAvPg0KICAgICAgICA8VmFsdWF0aW9uSW5mb3JtYXRpb25UYXhlcyAvPg0KICAgICAgICA8R29vZHNJZGVudGlmaWNhdGlvbj4NCiAgICAgICAgICA8R3Jvc3NNYXNzPjEwPC9Hcm9zc01hc3M+DQogICAgICAgIDwvR29vZHNJZGVudGlmaWNhdGlvbj4NCiAgICAgICAgPENvbnNpZ25tZW50SXRlbT4NCiAgICAgICAgICA8TWVzc2FnZUluZm9ybWF0aW9uPg0KICAgICAgICAgICAgPEdvb2RzSXRlbU51bWJlcj4xPC9Hb29kc0l0ZW1OdW1iZXI+DQogICAgICAgICAgICA8RGVjbGFyYXRpb25Hb29kc0l0ZW1OdW1iZXI+MTwvRGVjbGFyYXRpb25Hb29kc0l0ZW1OdW1iZXI+DQogICAgICAgICAgPC9NZXNzYWdlSW5mb3JtYXRpb24+DQogICAgICAgICAgPFJlZmVyZW5jZXNNZXNzYWdlc0RvY3VtZW50c0NlcnRpZmljYXRlc0F1dGhvcmlzYXRpb25zIC8+DQogICAgICAgICAgPFBhcnRpZXMgLz4NCiAgICAgICAgICA8VmFsdWF0aW9uSW5mb3JtYXRpb25UYXhlcyAvPg0KICAgICAgICAgIDxQbGFjZXNDb3VudHJpZXNSZWdpb25zIC8+DQogICAgICAgICAgPEdvb2RzSWRlbnRpZmljYXRpb24+DQogICAgICAgICAgICA8RGVzY3JpcHRpb25PZkdvb2RzPjEyMzwvRGVzY3JpcHRpb25PZkdvb2RzPg0KICAgICAgICAgIDwvR29vZHNJZGVudGlmaWNhdGlvbj4NCiAgICAgICAgPC9Db25zaWdubWVudEl0ZW0+DQogICAgICAgIDxDb25zaWdubWVudEl0ZW0+DQogICAgICAgICAgPE1lc3NhZ2VJbmZvcm1hdGlvbj4NCiAgICAgICAgICAgIDxHb29kc0l0ZW1OdW1iZXI+MjwvR29vZHNJdGVtTnVtYmVyPg0KICAgICAgICAgICAgPERlY2xhcmF0aW9uR29vZHNJdGVtTnVtYmVyPjI8L0RlY2xhcmF0aW9uR29vZHNJdGVtTnVtYmVyPg0KICAgICAgICAgIDwvTWVzc2FnZUluZm9ybWF0aW9uPg0KICAgICAgICAgIDxSZWZlcmVuY2VzTWVzc2FnZXNEb2N1bWVudHNDZXJ0aWZpY2F0ZXNBdXRob3Jpc2F0aW9ucyAvPg0KICAgICAgICAgIDxQYXJ0aWVzIC8+DQogICAgICAgICAgPFZhbHVhdGlvbkluZm9ybWF0aW9uVGF4ZXMgLz4NCiAgICAgICAgICA8UGxhY2VzQ291bnRyaWVzUmVnaW9ucyAvPg0KICAgICAgICAgIDxHb29kc0lkZW50aWZpY2F0aW9uPg0KICAgICAgICAgICAgPERlc2NyaXB0aW9uT2ZHb29kcz4yMzQ8L0Rlc2NyaXB0aW9uT2ZHb29kcz4NCiAgICAgICAgICA8L0dvb2RzSWRlbnRpZmljYXRpb24+DQogICAgICAgIDwvQ29uc2lnbm1lbnRJdGVtPg0KICAgICAgPC9Ib3VzZUNvbnNpZ25tZW50Pg0KICAgIDwvRGF0YUQxPg0KICA8L0RlY2xhcmF0aW9uRDE+DQo8L01lc3NhZ2U+</tns:xml><tns:dichiarante>123456</tns:dichiarante></tns:data></tns:Input></soapenv:Body></soapenv:Envelope>";

	const string receivedMessageText = "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><soapenv:Body><ns3:recuperaEsitoResponse xmlns:ns3=\"http://service.ws.sogei.it\"><recuperaEsitoReturn><IUT>20231228D17021414096</IUT><esito><codice>200</codice></esito><data>PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9InllcyI/Pg0KPGRhdGE+DQogICAgPERhdGFUaW1lU3RhcnRFbGFiPjIwMjMtMTItMjhUMTQ6MTQ6MTYuMDcyPC9EYXRhVGltZVN0YXJ0RWxhYj4NCiAgICA8RGF0YVRpbWVFbmRFbGFiPjIwMjMtMTItMjhUMTQ6MTQ6MTkuMjE5PC9EYXRhVGltZUVuZEVsYWI+DQogICAgPFN0YXRlPjQ8L1N0YXRlPg0KICAgIDxNcm4+MjJJVFFURzRURDU5MTA1NVI4PC9Ncm4+DQogICAgPExybj4yMDIzV1RMRFBMMDAwMDAwMDAwODIzPC9Mcm4+DQogICAgPEluZm9ybWF0aW9uPg0KICAgICAgICA8Q29kZT4wPC9Db2RlPg0KICAgICAgICA8RGVzY3JpcHRpb24+R2xpIGFydGljb2xpIGRlbGxhIGRpY2hpYXJhemlvbmUgMjJJVFFURzRURDU5MTA1NVI4IHNvbm8gc3RhdGkgc3ZpbmNvbGF0aSBjb24NCiAgICAgICAgICAgIGNvZGljZSBkaSBzdmluY29sbyBJTjVaNlMgaW4gZGF0YSAyOC0xMi0yMDIzPC9EZXNjcmlwdGlvbj4NCiAgICAgICAgPE51bWJlci8+DQogICAgICAgIDxEYXRhPjI4LTEyLTIwMjM8L0RhdGE+DQogICAgICAgIDxMZXZlbD5TPC9MZXZlbD4NCiAgICA8L0luZm9ybWF0aW9uPg0KPC9kYXRhPg==</data><dataRegistrazione>2023-12-28+01:00</dataRegistrazione></recuperaEsitoReturn></ns3:recuperaEsitoResponse></soapenv:Body></soapenv:Envelope>";
}
