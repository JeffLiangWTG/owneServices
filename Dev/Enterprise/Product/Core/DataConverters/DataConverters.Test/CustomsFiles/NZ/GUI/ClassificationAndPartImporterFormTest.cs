using Enterprise.DataConverters.CustomsFiles.NZ.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.CustomsFiles.NZ.GUI
{
	sealed internal class ClassificationAndPartImporterFormTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		class TestClassificationAndPartImporterForm : ClassificationAndPartImporterForm
		{
			public string LastMessage;

			protected override void ShowErrorMessage(string message)
			{
				LastMessage = message;
			}

			protected override void ShowInformationMessage(string message)
			{
				LastMessage = message;
			}
		}

		[RequiresSTA]
		public void TestClipboardCopy()
		{
			using (var testImporterForm = new TestClassificationAndPartImporterForm())
			{
				AssertEquals("Precondition: TestImporter.LastErrorMessage", null, testImporterForm.LastMessage);

				testImporterForm.CopyLogButton_Click(null, null);
				AssertEquals("ErrorMessage after Copy Log with no Log to Copy", ClassificationAndPartImporterForm.CopyToClipboardNotPossibleYet, testImporterForm.LastMessage);

				testImporterForm.Logger = new ProgressLogger();
				testImporterForm.Logger.Add("LALALA");
				testImporterForm.CopyLogButton_Click(null, null);
				var gotSuccessMessage = testImporterForm.LastMessage == ClassificationAndPartImporterForm.CopyToClipboardSuccessful;
				var gotFailureMessage = testImporterForm.LastMessage == ClassificationAndPartImporterForm.CopyToClipboardFailed;
				Assert("InformationMessage after Copy Log with a Real Log was either a Success or a Failure", gotSuccessMessage || gotFailureMessage);
				// Clipboard fails intermittantly, and there's nothing that can be done about it. Either result means that the right code was run.
				if (gotSuccessMessage)
				{
					var clipBoardText = SafeClipboard.GetDataObject().GetData(typeof(string)) as string;
					AssertEquals("Clipboard Contents after copying log", "LALALA\r\n", clipBoardText);
				}
			}
		}

		public void TestSetEnabledForUserModifiableControls()
		{
			using (var importerForm = new ClassificationAndPartImporterForm())
			{
				importerForm.SetEnabledForUserModifiableControls(false);
				AssertEquals("DataSourceTypeGroupBox.Enabled", false, importerForm.DataSourceTypeGroupBoxInternal.Enabled);
				AssertEquals("DataToImportGroupBox.Enabled", false, importerForm.DataToImportGroupBoxInternal.Enabled);
				AssertEquals("DataSourcePathTextBox.Enabled", false, importerForm.DataSourcePathTextBoxInternal.Enabled);
				AssertEquals("DataSourcePathBrowseButton.Enabled", false, importerForm.DataSourcePathBrowseButtonInternal.Enabled);
				AssertEquals("UpdateExistingRecordsCheckBox.Enabled", false, importerForm.UpdateExistingRecordsCheckBoxInternal.Enabled);
				AssertEquals("StartImportButton.Enabled", false, importerForm.StartImportButtonInternal.Enabled);
				AssertEquals("CopyLogButton.Enabled", false, importerForm.CopyLogButtonInternal.Enabled);
				AssertEquals("CloseButton.Enabled", false, importerForm.CloseButtonInternal.Enabled);

				importerForm.SetEnabledForUserModifiableControls(true);
				AssertEquals("DataSourceTypeGroupBox.Enabled", true, importerForm.DataSourceTypeGroupBoxInternal.Enabled);
				AssertEquals("DataToImportGroupBox.Enabled", true, importerForm.DataToImportGroupBoxInternal.Enabled);
				AssertEquals("DataSourcePathTextBox.Enabled", true, importerForm.DataSourcePathTextBoxInternal.Enabled);
				AssertEquals("DataSourcePathBrowseButton.Enabled", true, importerForm.DataSourcePathBrowseButtonInternal.Enabled);
				AssertEquals("UpdateExistingRecordsCheckBox.Enabled", true, importerForm.UpdateExistingRecordsCheckBoxInternal.Enabled);
				AssertEquals("StartImportButton.Enabled", true, importerForm.StartImportButtonInternal.Enabled);
				AssertEquals("CopyLogButton.Enabled", true, importerForm.CopyLogButtonInternal.Enabled);
				AssertEquals("CloseButton.Enabled", true, importerForm.CloseButtonInternal.Enabled);
			}
		}

		[RequiresSTA]
		public void TestCloseFunctionality()
		{
			using (var importerForm = new ClassificationAndPartImporterForm())
			{
				importerForm.Show();
				importerForm.SetEnabledForUserModifiableControls(false);
				importerForm.Close();
				AssertEquals("Importer.IsDisposed after attempted close while user controls disabled", false, importerForm.IsDisposed);
				importerForm.SetEnabledForUserModifiableControls(true);
				importerForm.Close();
				AssertEquals("Importer.IsDisposed after attempted close while user controls enabled", true, importerForm.IsDisposed);
			}
		}
	}
}
