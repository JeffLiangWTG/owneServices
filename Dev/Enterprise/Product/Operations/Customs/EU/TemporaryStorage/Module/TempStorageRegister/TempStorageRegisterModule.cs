using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeaderImport;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Modules;
using CusTempStorageRegHeader = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public class TempStorageRegisterModule : ZFilterGridModule, IImportCollectionInfoProvider
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.TempStorageRegister;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.EU.TempStorageRegister);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TempStorageRegisterFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new TempStorageRegisterFilterStripControl(GridCollection, (TempStorageRegisterFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory);
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowNew => false;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EUTempStorageRegister;

		#region Implementation of IImportCollectionInfoProvider

		IImportCollectionInfo IImportCollectionInfoProvider.ImportCollectionInfo
		{
			get
			{
				if (storageRegHeaderCollectionInfoImpl == null)
				{
					storageRegHeaderCollection = storageRegHeaderCollection ?? new CusTempStorageRegLineTransactionFlattenedCollection(Factory);
					storageRegHeaderCollectionInfoImpl = new CusTempStorageRegLineTransactionFlattenedImportCollectionInfo(storageRegHeaderCollection);
				}
				return storageRegHeaderCollectionInfoImpl;
			}
		}
		ImportCollectionInfoImpl storageRegHeaderCollectionInfoImpl;
		protected CusTempStorageRegLineTransactionFlattenedCollection storageRegHeaderCollection;

		public string ContextKey
		{
			get { return "16E858E1-BE38-4BF6-8E86-36E2E0C3AB1C"; }
		}

		#endregion

		#region Implementation of ZFilterGridModule

		protected override void RunImportDataWizard()
		{
			storageRegHeaderCollection = new CusTempStorageRegLineTransactionFlattenedCollection(Factory);
			var storageRegHeaderCollectionInfo = new CusTempStorageRegLineTransactionFlattenedImportCollectionInfo(storageRegHeaderCollection);
			var rowsDataProcessor = new CusTempStorageRegLineTransactionsDataTransferProcessor(storageRegHeaderCollection, storageRegHeaderCollectionInfo);
			var rowsSaveProcessor = new DataSaveProcessor(Factory,
				inProgressMessage: Res.GetString("C0E51904-FF58-4747-97D1-6484B8F4955E", "Saving Temporary Storage Register Transactions. This may take a long time depending on the amount being imported."),
				completedMessage: Res.GetString("CCF0ABCB-3AB7-451B-8A27-6FF96385B44D", "Temporary Storage Register Transactions saved."));

			RunImportWizardForm(storageRegHeaderCollectionInfo, ContextKey, rowsDataProcessor, rowsSaveProcessor);
		}

		void RunImportWizardForm(ImportCollectionInfoImpl storageRegHeaderCollectionInfo, string contextKey, CusTempStorageRegLineTransactionsDataTransferProcessor rowsDataProcessor, DataSaveProcessor rowsSaveProcessor)
		{
			bool isCancelled = false;
			DataImportWizardForm form = new MultistepDataImportWizardForm(storageRegHeaderCollectionInfo, contextKey, new DataTransferProcessor[] { rowsDataProcessor, rowsSaveProcessor }, this.ID.Description.ToString());
			form.Cancelled += (s, e) => { rowsDataProcessor.Rollback(); isCancelled = true; };
			form.Imported += (s, e) => { DisplayResult(rowsDataProcessor, isCancelled); };
			form.Show();
		}

		void DisplayResult(CusTempStorageRegLineTransactionsDataTransferProcessor rowsDataProcessor, bool isCancelled)
		{
			string mesgs;
			string caption;

			if (isCancelled)
			{
				mesgs = Res.GetString("C4D88D58-74FD-420E-8AF2-4BE184CC55A1", "No Temporary Storage Register Transaction was created.");
				caption = Res.GetString("EF49F9B8-669C-46F1-851B-0BBC4C8CB67B", "Import canceled");
			}
			else
			{
				mesgs = Res.GetString("82B20D85-CD23-4D44-BC56-A036CAAE5076", "Temporary Storage Register Transactions to Import = {0}", rowsDataProcessor.TransactionsToCreate) + "\r\n";
				mesgs += rowsDataProcessor.Log;
				mesgs += "\r\n" + Res.GetString("0972555C-378D-42A1-85C8-FFE999A4F787", "TOTAL: Temporary Storage Register Transactions created = {0}, Temporary Storage Register Transactions created with warnings = {1}, Temporary Storage Registers excluded = {2}", rowsDataProcessor.TransactionsCreated, rowsDataProcessor.TransactionsWithWarnings, rowsDataProcessor.TransactionsExcluded) + "\r\n";

				caption = Res.GetString("6C102655-0314-40CF-8D90-E521B9E87573", "Import completed");
			}

			using (ZMessageBox notification = new ZMessageBox(mesgs, caption, MessageBoxButtons.OK, MessageBoxIcon.Information))
			{
				notification.ShowDialog();
			}
		}

		#endregion
	}
}
