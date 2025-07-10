using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.DataInterface;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;

namespace Enterprise.Accounting.GUI.DataInterface
{
	public partial class CNReconciliationExportGUI : ZChildForm
	{
		public CNReconciliationExportGUI(ChinaReconciliationExportWrapper businessEntity)
			: base(businessEntity)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region GenerateButton_Click

#if DEBUG
		public
#else
		protected
#endif
void NextButton_Click(object sender, EventArgs e)
		{
			ContinueWithSave continueExport = ContinueWithSave.No;

			try
			{
				continueExport = ValidateAndSave();
			}
			catch (ZCannotSaveException)
			{
			}

			if (continueExport == ContinueWithSave.Yes)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				InvoiceReconciliationExportAdapter adapter = new InvoiceReconciliationExportAdapter(factory, (ChinaReconciliationExportWrapper)BusinessEntity);
				IExportCollectionInfo collectionInfo = adapter.GetMultiTypeCollectionInfo(adapter.GetBusinessObjectsForExport());

				if (!string.IsNullOrEmpty(adapter.Notification))
				{
					Globals.Message.ShowWarning(adapter.Notification);
					return;
				}

				var key = "ReconciliationExport";
				try
				{
					using (DataExportWizardForm form = new DataExportWizardForm(collectionInfo, key))
					{
						((ExportWizard)form.BusinessEntity).EncodingType = ExportWizard.Constants.EncodingTypes.UTF8;
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
						{
							adapter.AddEventForExportedTransaction();
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(Res.GetString("5624e3c8-014b-4d31-8427-a7001a5259b6",
						"Error Exporting Invoice Reconciliation"));
					throw;
				}
			}
		}

		#endregion

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
