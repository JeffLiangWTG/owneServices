using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataConverters.Accounting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.Accounting
{
	[TestedType(typeof(MainForm))]
	sealed internal class MainFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		#region Mock Class for Converter

		sealed internal class MockConverter : Converter
		{
			public MockConverter()
			{
			}

			public override Journal[] ImportFile(string fileName, bool isDebtors, ZDateTime postDate)
			{
				Journals = ProcessFile(fileName, postDate);
				RaiseImportCompletedEvent();
				return Journals;
			}

			protected override Journal[] ProcessFile(string fileName, ZDateTime postDate)
			{
				PostDate = postDate;
				if (DisplayError)
				{
					DisplayFormatErrorMessage(ErrorMsg, LineNo);
				}
				return Journals;
			}

			protected override bool ValidateClearingAccount()
			{
				return ClearingAccountValidation;
			}

			protected override bool ValidateFile()
			{
				return FileValidation;
			}

			public string ErrorMsg;
			public int LineNo;
			public bool ClearingAccountValidation;
			public bool FileValidation;
			public Journal[] Journals;
			public ZDateTime PostDate;

			public bool DisplayError;
		}

		#endregion

		[RequiresSTA]
		[TestDate(2003, 06, 05)]
		public void TestValidateBeforeImport()
		{
			Helper.SetupPeriods();
			var creator = new Enterprise.Accounting.Business.TestObjectCreator(Factory);
			var testConverter = new Converter();
			var testFileName = Env.TempPath + "test.csv";
			using (var writer = new StreamWriter(testFileName))
			{
				writer.WriteLine("Account,Reference,InvoiceDate,DueDate,Currency,ForeignAmount,LocalAmount,Branch,Department");
				writer.WriteLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"CNY\",\"100.00\",\"20.00\"", creator.Debtor.OH_Code));
			}
			using (var testForm = new MainForm(testConverter))
			{
				testForm.PathTextBoxInternal.Text = testFileName;
				testForm.Show();
				testForm.PostDateDateEditInternal.Text = "";
				testForm.StartConversionButton_Click(null, EventArgs.Empty);
				
				AssertEquals("There were 1 errors during import. Save function is disabled.\r\nPlease fix the errors and run the process again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("[Accounts Receivable Journal] Invoice Post Date: Please enter an Invoice Post Date.", testForm.OutPutListTextBoxInternal.Text);

				testForm.PostDateDateEditInternal.Text = "AFUENW";
				testForm.StartConversionButton_Click(null, EventArgs.Empty);
				AssertEquals("There were 1 errors during import. Save function is disabled.\r\nPlease fix the errors and run the process again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Please enter a valid Post Date", testForm.OutPutListTextBoxInternal.Text);

				var postDate = ZDateTime.Now;
				testForm.PostDateDateEditInternal.Text = postDate.ToShortDateString();
				testForm.StartConversionButton_Click(null, EventArgs.Empty);
				
				AssertEquals("Import Complete. Please check the import output window and save if you're happy with the result.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			File.Delete(testFileName);
		}

		public void TestFilePathValidationBeforeImport()
		{
			Helper.SetupPeriods();
			var testConverter = new MockConverter();
			using (var testForm = new MainForm(testConverter))
			{
				//to make form get past initial post date validation
				testForm.PostDateDateEditInternal.Text = ZDateTime.Now.ToShortTimeString();

				testForm.StartConversionButton_Click(null, EventArgs.Empty);
				AssertEquals("Please enter the file location", UnitTestUserNotification.Instance.LastMessage.Text);

				testForm.PathTextBoxInternal.Text = "testfile";
				testForm.StartConversionButton_Click(null, EventArgs.Empty);
				AssertEquals("Import Complete. Please check the import output window and save if you're happy with the result.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRegistryValidationBeforeImport()
		{
			Helper.SetupPeriods();
			var testConverter = new MockConverter();
			using (var testForm = new MainForm(testConverter))
			{
				testForm.PathTextBoxInternal.Text = "testfile";
				//to make form get past initial post date validation
				testForm.PostDateDateEditInternal.Text = ZDateTime.Now.ToShortTimeString();

				testForm.DebtorsRadioButtonInternal.Checked = true;
				testForm.CreditorsRadioButtonInternal.Checked = false;
				AssertGLHeaderValidation(testForm, AccountingConfigurationRegistry.Instance.ARJournalAccount);

				testForm.DebtorsRadioButtonInternal.Checked = false;
				testForm.CreditorsRadioButtonInternal.Checked = true;
				AssertGLHeaderValidation(testForm, AccountingConfigurationRegistry.Instance.APJournalAccount);
			}
		}

		void AssertGLHeaderValidation(MainForm testForm, AccountingConfigurationRegistry.AccountingRegistryItem registryItem)
		{
			var origionalPK = registryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var zorigionalPK = new ZGuid(origionalPK);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Guid()); //ZGuid.Empty

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			testForm.PostDateDateEditInternal.Text = ZDateTime.Now.ToShortTimeString();
			testForm.StartConversionButton_Click(null, EventArgs.Empty);

			AssertEquals(@"You cannot import without the following GL Accounts in the registry:
Accounting->General Ledger Defaults->Link Account->AR Journal Account
Accounting->General Ledger Defaults->Link Account->AP Journal Account", UnitTestUserNotification.Instance.LastMessage.Text);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZGuid.NewZGuid().ToGuid()); //Random Guid

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			testForm.PostDateDateEditInternal.Text = ZDateTime.Now.ToShortTimeString();
			testForm.StartConversionButton_Click(null, EventArgs.Empty);

			AssertEquals(@"The following registry settings need to have a valid and active GLHeader:
Accounting->General Ledger Defaults->Link Account->AR Journal Account
Accounting->General Ledger Defaults->Link Account->AP Journal Account", UnitTestUserNotification.Instance.LastMessage.Text);

			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, origionalPK);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			testForm.PostDateDateEditInternal.Text = ZDateTime.Now.ToShortTimeString();
			testForm.StartConversionButton_Click(null, EventArgs.Empty);

			AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasError);

			var gLHeader = Factory.Load<AccGLHeader>(zorigionalPK);
			gLHeader.AG_IsActive = false;
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			testForm.StartConversionButton_Click(null, EventArgs.Empty);

			AssertEquals(@"The following registry settings need to have a valid and active GLHeader:
Accounting->General Ledger Defaults->Link Account->AR Journal Account
Accounting->General Ledger Defaults->Link Account->AP Journal Account", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[RequiresSTA]
		public void TestPostDateAfterLoad()
		{
			Helper.SetupPeriods();
			var testConverter = new MockConverter();
			using (var testForm = new MainForm(testConverter))
			{
				testForm.PathTextBoxInternal.Text = "testfile";
				var dateBeforeLoad = ZDateTime.Now;
				testForm.Show();
				testForm.StartConversionButton_Click(null, EventArgs.Empty);
				Assert(testConverter.PostDate >= dateBeforeLoad.Date);
				Assert(ZDateTime.Now >= testConverter.PostDate);
			}
		}

		[DeveloperOnlyTest]
		public void TestCopyToClipBoardButtonEnabledIfErrorExist()
		{
			Helper.SetupPeriods();
			var testConverter = new MockConverter();
			using (var testForm = new MainForm(testConverter))
			{
				testForm.PathTextBoxInternal.Text = "testfile";
				testConverter.FileValidation = true;
				testConverter.ClearingAccountValidation = true;
				testConverter.DisplayError = true;

				testForm.InterbaseCheckBox.Enabled = false;
				testForm.Show();
				testForm.StartConversionButton_Click(testForm, EventArgs.Empty);

				Assert(testForm.CopyOutputToClipboardButtonInternal.Enabled);
				Assert(testForm.CopyOutputToClipboardButtonInternal.Visible);
			}
		}

		[DeveloperOnlyTest]
		public void TestClipboardContentIfErrorExist()
		{
			Helper.SetupPeriods();
			var testConverter = new MockConverter();
			using (var testForm = new MainForm(testConverter))
			{
				testForm.PathTextBoxInternal.Text = "testfile";
				testConverter.FileValidation = true;
				testConverter.ClearingAccountValidation = true;
				testConverter.ErrorMsg = "Error 1";
				testConverter.DisplayError = true;

				testForm.InterbaseCheckBox.Enabled = false;
				testForm.Show();
				testForm.StartConversionButton_Click(testForm, EventArgs.Empty);

				testForm.ClipboardCopyButton_Click(testForm, EventArgs.Empty);
				var clipBoardInfo = SafeClipboard.GetDataObject();
				var result = clipBoardInfo.GetData(DataFormats.Text) as string;

				AssertEquals("Line 0" + CsvJournalConverter.Tab + CsvJournalConverter.Tab + "Error 1", result);
			}
		}

		[RequiresSTA]
		public void TestSaveButtonEnabledIfNoError()
		{
			Helper.SetupPeriods();
			var testConverter = new MockConverter();
			using (var testForm = new MainForm(testConverter))
			{
				testForm.PathTextBoxInternal.Text = "testfile";
				testConverter.FileValidation = true;
				testConverter.ClearingAccountValidation = true;
				testConverter.DisplayError = false;

				testForm.InterbaseCheckBox.Enabled = false;
				testForm.Show();
				testForm.StartConversionButton_Click(testForm, EventArgs.Empty);

				Assert(!testForm.CopyOutputToClipboardButtonInternal.Enabled);
				Assert(!testForm.CopyOutputToClipboardButtonInternal.Visible);
				Assert(testForm.SaveButtonInternal.Enabled);
			}
		}

		public void TestDataSourceComboBox_SelectedIndexChanged()
		{
			using (var testForm = new MainForm(new Converter()))
			{
				testForm.InterbaseCheckBox.Checked = false;
				AssertEquals(false, testForm.ConnectionTextBox.Enabled);
				AssertEquals(false, testForm.ConnectionButton.Enabled);

				testForm.InterbaseCheckBox.Checked = true;
				AssertEquals(true, testForm.ConnectionTextBox.Enabled);
				AssertEquals(true, testForm.ConnectionButton.Enabled);
			}
		}

		[RequiresSTA]
		public void TestOutputTextBoxReadOnly()
		{
			using (var testForm = new MainForm(new Converter()))
			{
				testForm.Show();
				Assert(testForm.OutPutListTextBoxInternal.Visible);
				Assert(testForm.OutPutListTextBoxInternal.ReadOnly);
			}
		}

		[RequiresSTA]
		[TestDate(2003, 06, 05)]
		public void TestValidationBetweenImportAndSave()
		{
			var creator = new Enterprise.Accounting.Business.TestObjectCreator(Factory);
			creator.CreateTestPeriods(ZDateTime.Today);

			var testFileName = Env.TempPath + "test.csv";
			var code = "TestCode";
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			organisation.OH_Code = code;
			organisation.OH_IsCreditor = true;
			Factory.Save();

			using (var writer = new StreamWriter(testFileName))
			{
				writer.WriteLine("Account,Reference,InvoiceDate,DueDate,Currency,ForeignAmount,LocalAmount,Branch,Department");
				writer.WriteLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"CNY\",\"100.00\",\"20.00\"", creator.Debtor.OH_Code));
			}

			using (var testForm = new MainForm(new Converter()))
			{
				testForm.PostDateDateEditInternal.Text = ZDateTime.Now.ToShortTimeString();
				testForm.PathTextBoxInternal.Text = testFileName;

				testForm.InterbaseCheckBox.Enabled = false;
				testForm.Show();
				testForm.StartConversionButton_Click(testForm, EventArgs.Empty);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var company = Factory.Load<GlbCompany>(Env.CurrentCompanyPK);
				company.GC_RX_NKLocalCurrency = "CNY";
				Factory.Save();

				testForm.SaveButton_Click(testForm, EventArgs.Empty);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertEquals("Currency of current company has been updated by other operator. Imported data becomes invalid. Please reopen the form.\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Currency of current company has been updated by other operator. Imported data becomes invalid. Please reopen the form.\r\n", testForm.OutPutListTextBoxInternal.Text);
			}

			File.Delete(testFileName);
		}

		[TestDate(2003, 06, 05)]
		public void TestCannotSaveAfterCriticalErrorExceptionIsHandledBySaveButtonClick()
		{
			Helper.SetupPeriods();
			var testFileName = Env.TempPath + "test.csv";
			var code = "TestCode";
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			organisation.OH_Code = code;
			organisation.OH_IsCreditor = true;
			Factory.Save();

			using (var writer = new StreamWriter(testFileName))
			{
				writer.WriteLine("Account,Reference,InvoiceDate,DueDate,Currency,ForeignAmount,LocalAmount,Branch,Department");
				writer.WriteLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.00\",\"100.00\"", organisation.OH_Code));
			}

			var converter = new Converter();
			var createdJournals = converter.ImportFile(testFileName, false, ZDateTime.Now);
			createdJournals[0].Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.DummyErrorKeyForTest);

			using (var testForm = new MainForm(converter))
			{
				try
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					testForm.SaveButton_Click(testForm, EventArgs.Empty);
				}
				catch (OnSavingCriticalCheckException)
				{
					ErrorReporter.Clear();
				}
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown("CannotSaveAfterCriticalErrorException should be handled by this form.", () => testForm.SaveButton_Click(testForm, EventArgs.Empty));
			}

			File.Delete(testFileName);
		}

		protected override Form GetFormToBashCore()
		{
			return new MainForm(new Converter());
		}

		AccountingPeriodTestHelper Helper
		{
			get
			{
				return fHelper ?? (fHelper = new AccountingPeriodTestHelper());
			}
		}
		AccountingPeriodTestHelper fHelper;
	}
}
