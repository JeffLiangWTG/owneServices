using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PrimaryOrgSelectorForm : ZChildForm
	{
		public PrimaryOrgSelectorForm(PrimaryOrgSelectorCollection primaryOrgSelectors)
			: base(primaryOrgSelectors)
		{
			InitializeComponent();
			SelectedOrgPK = ZGuid.Empty;
		}

		protected PrimaryOrgSelectorForm()
			: base()
		{
			InitializeComponent();
		}

		public ZGuid SelectedOrgPK;

		void ContinueButton_Click(object sender, EventArgs e)
		{
			if (OrganisationsGrid.SelectedElements == null || OrganisationsGrid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(Res.GetString("EEF83CFC-9CB6-4c9a-B1C2-E283A7A15C86", "Please select primary organization"));
			}
			else if (OrganisationsGrid.SelectedElements.Length > 1)
			{
				Globals.Message.Show(Res.GetString("4962BD97-8D9A-4864-8F26-F4BF85019452", "Please select only one primary organization"));
			}
			else
			{
				SelectedOrgPK = ((PrimaryOrgSelector)OrganisationsGrid.SelectedElements[0]).OrganisationPK;
				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
