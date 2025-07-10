using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZColorSchemeRuleForm : ZChildForm
	{
		public ZColorSchemeRuleForm()
		{
			InitializeComponent();
		}

		public ZColorSchemeRuleForm(GridColourStripBusinessObject strip)
			: base(strip)
		{
			InitializeComponent();
			gridColourStripBO = strip;
			RuleNameTextBox.Text = gridColourStripBO.RuleName;
		}

		readonly GridColourStripBusinessObject gridColourStripBO;

		#region Save

		protected override ContinueWithSave ValidateAndSave()
		{
			return ContinueWithSave.No;
		}

		#endregion

		#region Closing

		protected override void OnClosing(CancelEventArgs e)
		{
			gridColourStripBO.RuleName = RuleNameTextBox.Text;

			if (gridColourStripBO.HasErrors && DialogResult != DialogResult.Cancel)
			{
				e.Cancel = true;
			}
			base.OnClosing(e);
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			if (!gridColourStripBO.HasErrors)
			{
				Close();
			}
		}

		#endregion
	}
}
