using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TagRuleControl : ZUserControl
	{
		public TagRuleControl()
		{
			InitializeComponent();
			filterCustomisationControl.Load += FilterCustomisationControl_Load;
		}

		void FilterCustomisationControl_Load(object sender, System.EventArgs e)
		{
			CheckIndexFiltersPermission();
		}

		void IndexSearchCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			filterCustomisationControl.ReloadFilters(IndexSearchCheckBox.Checked ? SearchType.Index : SearchType.Sql);
			CheckIndexFiltersPermission();
		}

		void CheckIndexFiltersPermission()
		{
			if (IndexSearchCheckBox.Checked && !filterCustomisationControl.IndexUsagePermitted)
			{
				filterCustomisationControl.Enabled = false;
			}
			else
			{
				filterCustomisationControl.Enabled = true;
			}
		}
	}
}
