using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLConsolidations
{
	public partial class GLEliminationJournalCreationForm : ZChildForm
	{
		public GLEliminationJournalCreationForm(ConsolidationBatchExportAdapter adapter)
			: base(adapter)
		{
			InitializeComponent();
		}

		#region Implementation

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

		void CreateButton_Click(object sender, System.EventArgs e)
		{
			var adapter = (ConsolidationBatchExportAdapter)DataSource;
			adapter.FromPeriod = ZInt.ParseSafe(FromPeriodEdit.Text, 0);
			adapter.ToPeriod = ZInt.ParseSafe(ToPeriodEdit.Text, 0);
			DialogResult = System.Windows.Forms.DialogResult.OK;
			Close();
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}
	}
}

