using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module.OperationalActions
{
	public partial class GbDeclarationActionApplicatorControl : ZUserControl
	{
		public GbDeclarationActionApplicatorControl()
		{
			InitializeComponent();
		}

		void rdoAutomatic_CheckedChanged(object sender, EventArgs e)
		{
			UpdateGroupBoxes();
		}

		void UpdateGroupBoxes()
		{
			grpAutomatic.Enabled = rdoAutomatic.Checked;
			grpManual.Enabled = rdoManual.Checked;
			if (!grpManual.Enabled)
			{
				txtMucrManual.Text = string.Empty;  // wipe up, since a manual value takes priority over auto ones. 
			}
		}

		void rdoManual_CheckedChanged(object sender, EventArgs e)
		{
			UpdateGroupBoxes();
		}
	}
}
