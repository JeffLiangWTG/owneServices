using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
	sealed class GLJournalXmlDataTransferDirectorTest : TestCaseWithFactory
	{
		public void TestPromptUserAndImport_FileOpen()
		{
			GLJournalXmlDataTransferDirectorTestClass director = new GLJournalXmlDataTransferDirectorTestClass(new GLJournalDataAdapter(), false);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			director.PromptUserAndImport(BillingInterfaceName.Test);
			AssertEquals(ZFormModaliser.ResultToReturnFromShowDialog, DialogResult.Cancel);
		}

		public void TestFileBeingUsedbyAnotherProcess()
		{
			GLJournalXmlDataTransferDirectorTestClass director = new GLJournalXmlDataTransferDirectorTestClass(new GLJournalDataAdapter(), false);
			string fileName = Env.TempPath + "test.xml";

			try
			{
				using (StreamWriter writer = new StreamWriter(fileName))
				{
					writer.WriteLine("-------------------");
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				using (Stream fs = File.OpenWrite(fileName))
				{
					ZFormModaliser.FileNameToSelectInShowCommonDialog = fileName;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					director.PromptUserAndImport(BillingInterfaceName.Test);
				}
				AssertEquals("The process cannot access the file '" + fileName + "' because it is being used by another process.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		class GLJournalXmlDataTransferDirectorTestClass : GUI.GLJournals.GlJournalXmlDataTransferDirector
		{
			public GLJournalXmlDataTransferDirectorTestClass(GLJournalDataAdapter adapter, bool hasLicence)
				: base(adapter, hasLicence)
			{
			}

			public new void PromptUserAndImport(BillingInterfaceName interfaceName)
			{
				base.PromptUserAndImport(interfaceName);
			}
		}
	}
}
