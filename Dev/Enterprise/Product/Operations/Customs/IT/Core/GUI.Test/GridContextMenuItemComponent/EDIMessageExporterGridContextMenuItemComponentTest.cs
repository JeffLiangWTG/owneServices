using System.IO;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

public abstract class EDIMessageExporterGridContextMenuItemComponentTest : GridContextMenuItemComponentAbstractTest<ITEDIMessage>
{
	public void TestSaveEDIFile_IDOC()
	{
		using (var form = GetNewZForm())
		using (var messageUserControl = GetNewMessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var messagesGrid = GetMessageGrid(messageUserControl);

			var saveEdiFileMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Save EDI file");
			messagesGrid.Select(0);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var filepath = EnvProxy.Instance.TempPath;
			ZFormModaliser.PathToSelectInShowCommonDialog = filepath;
			saveEdiFileMenuItem.PerformClick();
			AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var text = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertEquals("1 message(s) were exported", UnitTestUserNotification.Instance.LastMessage.Text);

			var idocFilePath = Path.Combine(filepath, idocFilename);
			if (File.Exists(idocFilePath))
			{
				try
				{
					Assert("File has been created", true);
					var result = File.ReadAllText(idocFilePath);
					AssertEquals("File written correctly", idocMessageResultText, result);
				}
				finally
				{
					File.Delete(idocFilePath);
				}
			}
			else
			{
				Fail("File has not been created");
			}
		}
	}

	public void TestSaveEDIFile_NotIDoc()
	{
		using (var form = GetNewZForm())
		using (var messageUserControl = GetNewMessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var messagesGrid = GetMessageGrid(messageUserControl);
			var saveEdiFileMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Save EDI file");
			messagesGrid.Select(1);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var filepath = EnvProxy.Instance.TempPath;
			ZFormModaliser.PathToSelectInShowCommonDialog = filepath;
			saveEdiFileMenuItem.PerformClick();
			AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var text = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertEquals("1 message(s) were exported", UnitTestUserNotification.Instance.LastMessage.Text);

			var irispFilePath = Path.Combine(filepath, irispFilename);
			if (File.Exists(irispFilePath))
			{
				try
				{
					Assert("File has been created", true);
					var result = File.ReadAllText(irispFilePath);
					AssertEquals("File written correctly", string.Concat(irispMessageText), result);
				}
				finally
				{
					File.Delete(irispFilePath);
				}
			}
			else
			{
				Fail("File has not been created");
			}
		}
	}

	public void TestSaveEDIFile_MultipleSelection()
	{
		using (var form = GetNewZForm())
		using (var messageUserControl = GetNewMessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var messagesGrid = GetMessageGrid(messageUserControl);
			var saveEdiFileMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Save EDI file");
			messagesGrid.SelectAllElements();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var filepath = EnvProxy.Instance.TempPath;
			ZFormModaliser.PathToSelectInShowCommonDialog = filepath;
			saveEdiFileMenuItem.PerformClick();
			AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var text = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertEquals("2 message(s) were exported", UnitTestUserNotification.Instance.LastMessage.Text);

			var irispFilePath = Path.Combine(filepath, irispFilename);
			var idocFilePath = Path.Combine(filepath, idocFilename);
			if (File.Exists(irispFilePath) && File.Exists(idocFilePath))
			{
				Assert("Files have been created", true);
				File.Delete(idocFilePath);
				File.Delete(irispFilePath);
			}
			else
			{
				Fail("Files have not been created");
			}
		}
	}

	public void TestSaveEDIFile_CloseBrowseDialog()
	{
		using (var form = GetNewZForm())
		using (var messageUserControl = GetNewMessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var messagesGrid = GetMessageGrid(messageUserControl);
			var saveEdiFileMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Save EDI file");
			messagesGrid.Select(0);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			var filepath = EnvProxy.Instance.TempPath;
			ZFormModaliser.PathToSelectInShowCommonDialog = filepath;
			saveEdiFileMenuItem.PerformClick();
			AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());

			var idocFilePath = Path.Combine(filepath, idocFilename);
			AssertEquals("File has not been written.", false, File.Exists(idocFilePath));
		}
	}

	public void TestSaveEDIFile_Overwrite()
	{
		using (var form = GetNewZForm())
		using (var messageUserControl = GetNewMessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var messagesGrid = GetMessageGrid(messageUserControl);
			var saveEdiFileMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Save EDI file");
			messagesGrid.Select(0);

			var filepath = EnvProxy.Instance.TempPath;
			var idocFilePath = Path.Combine(filepath, idocFilename);

			File.WriteAllText(idocFilePath, "");

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.PathToSelectInShowCommonDialog = filepath;
			saveEdiFileMenuItem.PerformClick();
			AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			AssertEquals("The folder already contains one or more files with the same names. If you continue, the files will be overwritten. Do you want to continue?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertEquals("1 message(s) were exported", UnitTestUserNotification.Instance.LastMessage.Text);

			if (File.Exists(idocFilePath))
			{
				Assert("File has been created", true);
				File.Delete(idocFilePath);
			}
			else
			{
				Fail("File has not been created");
			}
		}
	}

	public void TestSaveEDIFile_CancelOverwrite()
	{
		using (var form = GetNewZForm())
		using (var messageUserControl = GetNewMessageUserControl())
		{
			form.Controls.Add(messageUserControl);
			form.Show();

			var messagesGrid = GetMessageGrid(messageUserControl);
			var saveEdiFileMenuItem = messagesGrid.ContextMenu.MenuItems.FindByText("Save EDI file");
			messagesGrid.Select(0);

			var filepath = EnvProxy.Instance.TempPath;
			var idocFilePath = Path.Combine(filepath, idocFilename);

			File.WriteAllText(idocFilePath, string.Empty);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.PathToSelectInShowCommonDialog = filepath;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			saveEdiFileMenuItem.PerformClick();

			AssertEquals(typeof(FolderBrowserDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());

			AssertEquals("The folder already contains one or more files with the same names. If you continue, the files will be overwritten. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("File has not been overwrite", string.Empty, File.ReadAllText(idocFilePath));

			if (File.Exists(idocFilePath))
			{
				File.Delete(idocFilePath);
			}
		}
	}

	protected override GridContextMenuItemComponent<ITEDIMessage> GetNewContextMenuItemComponent(ZGrid grid)
		=> new EDIMessageExporterGridContextMenuItemComponent(grid);

	protected override string MenuItemText => "Save EDI file";

	protected abstract ZGrid GetMessageGrid(object messageUserControl);

	protected abstract ZForm GetNewZForm();

	protected abstract ZUserControl GetNewMessageUserControl();

	protected const string idocFilename = "11110122.R00";
	protected const string irispFilename = "11110115.X00";

	protected const string idocInterchangeHeaderText = "<ITMessage><Staff>BC</Staff><Node>1111</Node><MessageType>R</MessageType><AccountNumber>12345678911-001</AccountNumber><Header>1111            11110122.R00            279100    12345678911     001 00003</Header></ITMessage>";

	protected const string idocMessageText = @"TET           00000300	EX	A		15012021	0			1	IT13149600150	ACO XML TEST	VIA PALATINO 15	20148	MILANO	IT			IT0	GARBA S.R.L	VIA TRIBONIANO 103 MILANO	20156	MILAN	IT		1								IT	IT	AB123CD		IT	0	FOB	1	TEST LUOGO				US				EUR	1000.00		11	4	3												0																																										0	0
?ET1          00000300																																	0	0	TIN AND ARTICLES THEREOF UNWROUGHT TIN TIN, NOT ALLOYED					1	80011000		0		100	1000	1	0	90.455	Z	ZZZ	A3	123456			03122020	M	279100  												1	N380	US	2020	F123456				0	0									1000.00		0	0
";

	protected const string idocMessageResultText = @"1111            11110122.R00            279100    12345678911     001 00003
TET           00000300	EX	A		15012021	0			1	IT13149600150	ACO XML TEST	VIA PALATINO 15	20148	MILANO	IT			IT0	GARBA S.R.L	VIA TRIBONIANO 103 MILANO	20156	MILAN	IT		1								IT	IT	AB123CD		IT	0	FOB	1	TEST LUOGO				US				EUR	1000.00		11	4	3												0																																										0	0
?ET1          00000300																																	0	0	TIN AND ARTICLES THEREOF UNWROUGHT TIN TIN, NOT ALLOYED					1	80011000		0		100	1000	1	0	90.455	Z	ZZZ	A3	123456			03122020	M	279100  												1	N380	US	2020	F123456				0	0									1000.00		0	0
";

	protected const string irispInterchangeHeaderText = "<ITMessage><Staff>BC</Staff><Node>1111</Node><MessageType>R</MessageType><AccountNumber>12345678911-001</AccountNumber><Header>1111            11110115.X00            279100    12345678911     001 00003</Header></ITMessage>";

	protected const string irispMessageText = @"0RPE            11110115.X00190015527541119101    02046110033     001 00005    INVIO IN AMBIENTE REALE
 RICEVUTO  03/10/19 06:47,11110115.X00
 RL=002,RS=000,ME=001,MS=000,PE=001,PS=000
ESEGUITO   03/10/19  06:47
RET          00000200028100P1   00007705P191020014320X001352G231120 000000 000000                               20ITQVG1T1234567T1      310000.00
";
}
