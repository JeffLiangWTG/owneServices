using System.Data;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI.Testing
{
	internal class JASInvoicingFormHelperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", Form, Helper.Form);
			AssertNull("Export JXC Financial Message menu item should not be added as CreditNote is not in database", Form.ActionsMenuItem.MenuItems.FindByText("Export JXC Financial Message"));
			CreditNote.FillWithValidTestData();
			Factory.Save();
			ReloadHelper();
			AssertNotNull("Export JXC Financial Message menu item should be added now", Form.ActionsMenuItem.MenuItems.FindByText("Export JXC Financial Message"));
		}

		public void TestMenuItemClick_CannotBeExported()
		{
			CreditNote.FillWithValidTestData();
			Factory.Save();
			Form.Show();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Assert("Pre-condition", UnitTestUserNotification.Instance.LastMessage.WasNone);
			MenuItem menuItem = Form.ActionsMenuItem.MenuItems.FindByText("Export JXC Financial Message");
			menuItem.PerformClick();
			AssertEquals("Should not allow export, should show error message box", "There are errors that need to be corrected before JXC message can be exported", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNoExportFilesInExportDir("There are JXC warnings");
		}

		public void TestMenuItemClick()
		{
			using (CreditNote.SuspendValidationTesting())
			{
				CreditNote.FillWithValidTestData();
				CreditNote.IsManuallySetTransactionNumber_ForTestOnly = true;
				CreditNote.AH_TransactionNum = "099998";
				Factory.Save();
				CreditNote.ExpectNoErrorsAndWarningsOnRunPreSaveValidation = true;
				MenuItem menuItem = Form.ActionsMenuItem.MenuItems.FindByText("Export JXC Financial Message");
				menuItem.PerformClick();
				string[] files = Directory.GetFiles(JASDataRegistry.Instance.JXCOutgoingDirectoryName);
				AssertEquals("There should be one NCDT message exported", 1, files.Length);
				AssertEquals("099998.txt", Path.GetFileName(files[0]));
				using (StreamReader reader = File.OpenText(files[0]))
				{
					object readHeader = reader.ReadLine();
					Assert("should be exported as NCDT message", reader.ReadLine().StartsWith("NCDT"));
				}

				Assert("Dialog message", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Dialog message", "JXC Message for '" + CreditNote.HumanReadableName + "' has been successfully exported to \"" + JASDataRegistry.Instance.JXCOutgoingDirectoryName + "\"", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation
		JASInvoicingFormHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new JASInvoicingFormHelper(Form);
				}

				return fHelper;
			}
		}

		JASARCreditNoteFormForTest Form
		{
			get
			{
				if (fForm == null)
				{
					fForm = new JASARCreditNoteFormForTest(CreditNote);
				}

				return fForm;
			}
		}

		JASARCreditNoteForTest CreditNote
		{
			get
			{
				if (fCreditNote == null)
				{
					fCreditNote = Factory.New<JASARCreditNoteForTest>();
				}

				return fCreditNote;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			string testDir = Path.Combine(Env.TempPath, "JASInvoicingFormHelperTest");
			Directory.CreateDirectory(testDir);
			JASDataRegistry.Instance.JXCOutgoingDirectoryName = testDir;
			JASDataRegistry.Instance.QueryUserForDirectoryOnManualJXCExport = false;
		}

		protected override void TearDown()
		{
			Form.Dispose();
			TempDirectory.DeleteDirectory(JASDataRegistry.Instance.JXCOutgoingDirectoryName);
			base.TearDown();
		}

		void AssertNoExportFilesInExportDir(string errorMessage)
		{
			AssertEquals(errorMessage + " There should be no files exported", 0, Directory.GetFiles(JASDataRegistry.Instance.JXCOutgoingDirectoryName).Length);
		}

		void ReloadHelper()
		{
			fHelper = null;
			object lazyLoad = Helper;
		}

		JASInvoicingFormHelper fHelper;
		JASARCreditNoteFormForTest fForm;
		JASARCreditNoteForTest fCreditNote;
		#region class JASARCreditNoteForTest
		class JASARCreditNoteForTest : JASARCreditNote
		{
			public JASARCreditNoteForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void RunPreSaveValidationCore()
			{
				base.RunPreSaveValidationCore();
				if (ExpectNoErrorsAndWarningsOnRunPreSaveValidation)
				{
					ClearAllNotifications();
				}
			}

			public bool ExpectNoErrorsAndWarningsOnRunPreSaveValidation;
		}
		#endregion
		#endregion
	}
}
