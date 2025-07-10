using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class ImportFromSailingForm : ZChildForm
	{
		public ImportFromSailingForm()
		{
			InitializeComponent();
		}

		BillImportActionCollection ImportActions => (BillImportActionCollection)base.DataSource;

		public ImportFromSailingForm(BillImportActionCollection importActions)
			: base(importActions)
		{
			InitializeComponent();
		}

		void DeselectAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (BillImportAction importAction in ImportActions)
			{
				importAction.IsSelected = false;
			}
		}

		void SelectAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (BillImportAction importAction in ImportActions)
			{
				importAction.IsSelected = true;
			}
		}

		public override string FormVerb => string.Empty;
	}
}
