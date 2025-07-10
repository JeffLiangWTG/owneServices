using System;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CsvTransactionsImportController))]
	public class CsvTransactionsImportControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CsvTransactionsImport;
		}

		public override void TestNewForm()
		{
			CsvTransactionsImportController controller = new CsvTransactionsImportController();
			controller.ShowNewForm();
			AssertEquals(false, ((DataImporterForm)ZFormModaliser.LastFormShownDialogForTest).Importer.OnlySaveDataWhenNoRecordsHaveErrors);

			AccountingConfigurationRegistry.Instance.DataImportShouldSaveOnlyWhenThereAreNoErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			controller.ShowNewForm();
			AssertEquals(true, ((DataImporterForm)ZFormModaliser.LastFormShownDialogForTest).Importer.OnlySaveDataWhenNoRecordsHaveErrors);
		}

		public void TestDataImporterFormCorrectlyInitialized()
		{
			CsvTransactionsImportController controller = new CsvTransactionsImportController();
			controller.ShowNewForm();

			AssertNotNull("Precondition: ZFormModaliser.LastFormShownDialogForTest must be initialized.", ZFormModaliser.LastFormShownDialogForTest);
			AssertNotNull("Precondition: ZFormModaliser.LastIBusinessShownOnDialogForTest must be initialized.", ZFormModaliser.LastIBusinessShownOnDialogForTest);

			DataImporterForm dataImporterForm = ZFormModaliser.LastFormShownDialogForTest as DataImporterForm;
			DataImporterBusinessObjectWithResultReporter formBizo = ZFormModaliser.LastIBusinessShownOnDialogForTest as DataImporterBusinessObjectWithResultReporter;

			AssertNotNull("New form must be DataImporterForm", dataImporterForm);
			AssertNotNull("DataImporterForm must have DataImporterBusinessObjectWithResultReporter business entity.", formBizo);
			AssertEquals("DataTransferResultReporter must be initialized by the form importer.", dataImporterForm.Importer, formBizo.DataTransferResultReporter);
		}
	}
}
