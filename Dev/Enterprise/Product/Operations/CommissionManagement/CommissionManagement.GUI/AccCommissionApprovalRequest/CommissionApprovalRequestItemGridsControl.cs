using System.Windows.Forms;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class CommissionApprovalRequestItemGridsControl : ZUserControl
	{
		public CommissionApprovalRequestItemGridsControl()
		{
			InitializeComponent();
		}

		void DetailCommissionGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 2 && e.Button == MouseButtons.Left)
			{
				if (DetailCommissionGrid.HitTest(e.X, e.Y).Row > -1)
				{
					var current = DetailCommissionGrid.ListManager.GetCurrent() as CommissionApprovalRequestItemGrouping;
					if (current != null)
					{
						CommissionLineGroupingViewer.ShowViewForm(current);
					}
				}
			}
		}
	}
}
