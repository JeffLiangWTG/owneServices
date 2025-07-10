using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;
using Enterprise.Accounting.DataTransfer.ReportingBook.AlternateGLAccountWithAttribute;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ImportAccountsControllerForm
{
	public partial class ImportAccountsControllerForm : ZChildForm
	{
		public ImportAccountsControllerForm(AccountsImportBusinessObject accountsImportBizO)
			: base()
		{
			fAccountsImportBuzinessObject = accountsImportBizO;
		}
		public AccountsImportBusinessObject fAccountsImportBuzinessObject;

		ZString MessageBeforeDeleteChargeCodes = Res.GetString("29002d2d-bbee-42c3-bed2-16a5830076d8", "{0} will now delete all charge codes for all companies on this installation.\r\nThe list of companies on this installation is as follows:", Core.Constants.ProductName);

		void ImportAccountsButton_Click(object sender, EventArgs e)
		{
			DataImporterBusinessObject businessEntity = new DataImporterBusinessObject(new BusinessObjectFactory());

			using (DataImporterForm form = new DataImporterForm(businessEntity, Res.GetString("Accounting|ImportCSVAccountChartForm", "Import CSV Account Chart"), BillingInterfaceName.CSVAccountChartImport)) // Interface name for billing purposes
			{
				GLHeaderAndChargeCodeFlatFileDataImporter importer = new GLHeaderAndChargeCodeFlatFileDataImporter();
				form.Importer = importer;
				form.SetParametersOfOnlySaveDataWhenNoRecordsHaveErrorsCheckBox(AccountingConfigurationRegistry.Instance.DataImportShouldSaveOnlyWhenThereAreNoErrors.Value, true);
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void ImportAlternateGLAccountsButton_Click(object sender, EventArgs e)
		{
			var businessEntity = new DataImporterBusinessObject(new BusinessObjectFactory());

			using (var form = new DataImporterForm(businessEntity, Res.GetString("0088AB72-1A5A-4DD1-BA59-79A8A04BF816", "Import CSV Alternate GL Accounts"), BillingInterfaceName.CSVAlternateGLAccountImport)) // Interface name for billing purposes
			{
				var importer = new AlternateGLAccountWithAttributeFlatFileDataImporter();
				form.Importer = importer;
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void DeleteGLHeadersButton_Click(object sender, EventArgs e)
		{
			try
			{
				fAccountsImportBuzinessObject.DeleteAllGLHeaders();
			}
			catch (ZCannotSaveException ex)
			{
				Globals.Message.ShowError(ex.Message);
			}
			BusinessObjectFactory.SavedEventHandler handler = new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			fAccountsImportBuzinessObject.Factory.Saved += handler;
			Save();
			fAccountsImportBuzinessObject.Factory.Saved -= handler;
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (!savedSuccessfully)
			{
				fAccountsImportBuzinessObject.RevertAllControlAccounts();
			}
		}

		void DeleteChargeCodesButton_Click(object sender, EventArgs e)
		{
			GlbCompanyCollection companies = new GlbCompanyCollection(fAccountsImportBuzinessObject.Factory, new ZQuery());
			foreach (GlbCompany company in companies)
			{
				MessageBeforeDeleteChargeCodes += System.Environment.NewLine + company.GC_Code + " " + company.GC_Name;
			}
			if (Globals.Message.Show(MessageBeforeDeleteChargeCodes, Res.GetString("78900c66-5e9f-4a30-b103-f13813abf8e7", "Delete Charge Codes"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				fAccountsImportBuzinessObject.DeleteAllChargeCodes();
				Save();
			}
		}

		void Save()
		{
			try
			{
				fAccountsImportBuzinessObject.Factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		#region TestCase
#if DEBUG
		public void RunImportAlternateGLAccountsButton_Click()
		{
			ImportAlternateGLAccountsButton_Click(new object(), new EventArgs());
		}

		public void RunImportAccountsButton_Click()
		{
			ImportAccountsButton_Click(new object(), new EventArgs());
		}

		public void RunDeleteGLHeadersButton_Click()
		{
			DeleteGLHeadersButton_Click(new object(), new EventArgs());
		}

		public void RunDeleteChargeCodesButton_Click()
		{
			DeleteChargeCodesButton_Click(new object(), new EventArgs());
		}

		public ZString GetUsedMessageBeforeDeleteChargeCodes()
		{
			return MessageBeforeDeleteChargeCodes;
		}

#endif
		#endregion
	}
}
