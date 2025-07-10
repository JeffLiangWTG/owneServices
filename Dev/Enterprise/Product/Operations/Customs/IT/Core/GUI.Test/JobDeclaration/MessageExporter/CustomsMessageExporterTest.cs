using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

abstract class CustomsMessageExporterTest : TestCaseWithFactory
{
	public void TestSaveToFileWhenUsersOK()
	{
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
		ZFormModaliser.FileNameToSelectInShowCommonDialog = TestFilename;

		message.EM_MessageText = "MESSAGE TEXT";
		CustomsMessageExporter.SaveToFile(message);

		AssertEquals(typeof(SaveFileDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());
		if (File.Exists(TestFilename))
		{
			AssertUTF8EncodingWithoutByteOrderMark(TestFilename, message.EM_MessageText);
			File.Delete(TestFilename);
		}
		else
		{
			Fail("File has not been created");
		}
	}

	void AssertUTF8EncodingWithoutByteOrderMark(ZString filename, ZString fileContent)
	{
		var utf8EncodingWithoutBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
		AssertArrayEqualsByElements("Expected UTF8 encoding without BOM (Byte Order Mark)", utf8EncodingWithoutBOM.GetBytes(fileContent), File.ReadAllBytes(filename));
	}

	public void TestSaveToFileWhenUsersCancel()
	{
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
		ZFormModaliser.FileNameToSelectInShowCommonDialog = TestFilename;

		CustomsMessageExporter.SaveToFile(message);

		AssertEquals(typeof(SaveFileDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());
		if (!File.Exists(TestFilename))
		{
			Assert("File has not been created", true);
		}
		else
		{
			File.Delete(TestFilename);
			Fail("File has been created");
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		var interchange = Factory.New<EDIInterchange>();
		message = Factory.New<ITEDIMessage>();
		interchange.ContainedMessages.Add(message);
	}

	ITEDIMessage message;

	ICustomsMessageExporter CustomsMessageExporter => customsMessageExporter ?? (customsMessageExporter = GetCustomsMessageExporter());
	ICustomsMessageExporter customsMessageExporter;

	ZString TestFilename => Path.Combine(EnvProxy.Instance.TempPath, "IT.GUI.Testing.TestSaveToFileWhenUsersOK.tmp");

	public abstract void TestGetFileName();

	public abstract void TestGetFileContent();

	protected abstract ICustomsMessageExporter GetCustomsMessageExporter();
}
