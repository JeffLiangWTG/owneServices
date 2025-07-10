using System;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class Phase5ArrivalNotificationTabUserControl : EU.NCTS.GUI.Phase5ArrivalNotificationTabUserControl
	{
		public Phase5ArrivalNotificationTabUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			DynamicSummaryDeclarationLayoutPanel.UpdateLayout(new Phase5ArrivalSummaryDeclarationLayout());
		}
	}
}
