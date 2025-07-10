using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class ImportFromSailingForm : ZChildForm
	{
		public ImportFromSailingForm()
		{
			InitializeComponent();
		}

		BillImportActionCollection ImportActions
		{
			get { return (BillImportActionCollection)base.DataSource; }
		}

		public ImportFromSailingForm(BillImportActionCollection importActions)
			: base(importActions)
		{
			InitializeComponent();
		}

		void deselectAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (BillImportAction importAction in ImportActions)
			{
				importAction.IsSelected = false;
			}
		}

		void selectAllButton_Click(object sender, System.EventArgs e)
		{
			foreach (BillImportAction importAction in ImportActions)
			{
				importAction.IsSelected = true;
			}
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}
	}
}
