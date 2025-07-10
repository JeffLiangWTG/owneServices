using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Modules.AutoRefresh
{
	public partial class AutoRefreshForm : ZChildForm
	{
		public AutoRefreshForm(byte currentTimeOut)
			: base(new AutoRefreshBizO(currentTimeOut))
		{
			MainStatusBar.Visible = false;
			FormBorderStyle = FormBorderStyle.FixedDialog;
		}

		public AutoRefreshBizO BizO
		{
			get { return (AutoRefreshBizO)BusinessEntity; }
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				BizO.Validation.ValidateAll();
				if (BizO.HasErrors)
				{
					e.Cancel = true;
				}
			}
			base.OnClosing(e);
		}
	}
}
