using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLConsolidations
{
	public partial class GLConsolidationExportForm : ZChildForm
	{
		public GLConsolidationExportForm()
		{
		}

		public GLConsolidationExportForm(ConsolidationBatchExportAdapter adapter)
			: base(adapter)
		{
			InitializeComponent();

			ZFormMenuStrategy.AddAdornments(this);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.FileMenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.ActionsMenuItemName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.HelpMenuItemName, false);
		}

		#region Implementation

		ConsolidationBatchExportAdapter Adapter
		{
			get { return (ConsolidationBatchExportAdapter)BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		void ExportButton_Click(object sender, System.EventArgs e)
		{
			Adapter.RunPreSaveValidation();

			if (Adapter.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				Adapter.FromPeriod = ZInt.ParseSafe(FromPeriodEdit.Text, 0);
				Adapter.ToPeriod = ZInt.ParseSafe(ToPeriodEdit.Text, 0);
				DialogResult = System.Windows.Forms.DialogResult.OK;
				Close();
			}
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Adapter.HasChanges = false;
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}

