using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CsvTransactionsImportController : ZSingletonController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.CsvTransactionsImport; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override IZForm ShowNewForm()
		{
			using (var form = GetNewForm())
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}

			return null;
		}

		static DataImporterForm GetNewForm()
		{
			var dataImporterBusinessEntity = new DataImporterBusinessObjectWithResultReporter(new BusinessObjectFactory());
			var form = new DataImporterForm(dataImporterBusinessEntity, FormCaption, BillingInterfaceName.TransactionsCsvImport);
			var flatFileDataImporter = new TxnHeaderFlatFileDataImporter();
			flatFileDataImporter.ImportingSingleTransaction = false;
			flatFileDataImporter.RunExtraValidation = true;
			form.Importer = flatFileDataImporter;
			dataImporterBusinessEntity.DataTransferResultReporter = flatFileDataImporter;
			form.SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(AccountingConfigurationRegistry.Instance.DataImportShouldSaveOnlyWhenThereAreNoErrors.Value, true);

			return form;
		}

		static string FormCaption
		{
			get { return Res.GetString("af82890f-f10c-4ddf-a86c-0d33038a3272", "Accounting Transactions CSV Import"); }
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleTemplateCopyNotSupportedException("You cannot Show a Template CopyForm for a Transaction Import");
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ImportCsvAccountingTransactions; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#region Test
#if DEBUG

		public DataImporterForm GetNewForm_ForTest()
		{
			return GetNewForm();
		}

#endif
		#endregion
	}
}
