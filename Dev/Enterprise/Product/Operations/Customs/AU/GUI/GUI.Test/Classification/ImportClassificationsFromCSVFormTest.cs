using System.IO;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(ImportClassificationsFromCSVForm))]
	sealed class ImportClassificationsFromCSVFormTest : MasterFiles.GUI.Testing.DataLoaderFormTestCase
	{
		public void TestFormHeading()
		{
			using (ImportClassificationsFromCSVForm testForm = new ImportClassificationsFromCSVForm())
			{
				testForm.Show();
				AssertEquals("Form Heading", "Import Classification Lookup Data", testForm.Text);
			}
		}

		public void TestConfirmOKLoadData()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			using (TempFile tempFile = TempFile.NewWithExtension(".csv"))
			{
				PopulateTestFile(tempFile);
				using (var testForm = new ImportClassificationsFromCSVFormForTest())
				{
					testForm.Show();
					testForm.FileNameTextBoxInternal.Text = tempFile.Filename;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					testForm.StartButtonInternal.PerformClick();
					AssertNotNull("PreCondition: Message Shown", UnitTestUserNotification.Instance.LastMessage);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Please Note: Only classifications with valid tariff details will be loaded.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Import Complete.", testForm.MainStatusBar.Text);
					Assert(testForm.CopyLogToClipboardButtonInternal.Enabled);
					Assert(testForm.CopyLogToClipboardButtonInternal.Visible);
					Assert(testForm.CloseButtonInternal.Enabled);
					Assert(testForm.CloseButtonInternal.Visible);
					Assert(testForm.OutputListBoxInternal.Items.Count > 0);
					AssertEquals("Progress Bar total has not been set", 4, testForm.ProgressBarInternal.Maximum);
					AssertEquals("Progress Bar has not been updated", 4, testForm.ProgressBarInternal.Value);
					string logData = testForm.GetLogInternal();
					Assert("Log Data not as expected", logData.StartsWith("Classifications to Import = 3"));
					string logDataOutputFileName = null;
					try
					{
						logDataOutputFileName = testForm.CreateLogInDataDirectoryInternal(logData);
						Assert(logDataOutputFileName.Length > 0);
						Assert(logDataOutputFileName != "Not Created");
					}
					finally
					{
						File.Delete(logDataOutputFileName);
					}
				}
			}
		}

		protected override ImportFromCSVForm GetNewImportFromCSVFormCore() => new ImportClassificationsFromCSVForm();

		protected override string CountryCode => Core.Constants.CountryCodes.Eritrea;

		void PopulateTestFile(TempFile tempFile)
		{
			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("CODE,TYPE,DESCRIPTION,TARIFF,TREATMENT,INSTRUMENT,CONCESSION");
				sw.WriteLine("TEST,IMP,Test Lookup,6205900054,,,");
				sw.WriteLine("Lookup with concession,IMP,Lookup Description - code with concession,6205900054,218,TC1,8333890");
				sw.WriteLine("Joggers,EXP,,64051000,,,");
				sw.Flush();
			}
		}

		sealed class ImportClassificationsFromCSVFormForTest : ImportClassificationsFromCSVForm
		{
			internal ZTextBox FileNameTextBoxInternal => FileNameTextBox;
			internal ZButton StartButtonInternal => StartButton;
			internal ZButton CopyLogToClipboardButtonInternal => CopyLogToClipboardButton;
			internal ZButton CloseButtonInternal => CloseButton;
			internal KListBox OutputListBoxInternal => OutputListBox;
			internal KProgressBar ProgressBarInternal => ProgressBar;
			internal string GetLogInternal() => GetLog();
			internal string CreateLogInDataDirectoryInternal(string logData) => CreateLogInDataDirectory(logData);
		}
	}
}
