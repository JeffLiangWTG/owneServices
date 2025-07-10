using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Matching
{
	public partial class SettlementOrganisationsForm : ZChildForm
	{
		public SettlementOrganisationsForm()
		{
			InitializeComponent();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		public ZArchitecture.ZGrid zGrid1_ForTestOnly
		{
			get { return zGrid1; }
			set { zGrid1 = value; }
		}
	}
}
