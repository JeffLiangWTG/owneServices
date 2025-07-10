using System;
using Enterprise.Customs.CA.Business;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class K84UserControl : ZUserControl
	{
		public K84UserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			var declaration = JobDeclaration;
			if (declaration != null && (declaration.IsB2Adjustments || declaration.IsLVS || declaration.IsB3X))
			{
				this.noticesTabControl.TabPages.Remove(this.CCNTabPage);
			}
		}

		JobDeclaration JobDeclaration
		{
			get { return DataSource as JobDeclaration; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			ChangeControlsVisibility();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			ChangeControlsVisibility();
		}

		public void ChangeControlsVisibility()
		{
			k84DeclarationDetailsUserControl.ChangeControlsVisibility(JobDeclaration);
		}

		void NoticesBoundGrid_Click(object sender, EventArgs e)
		{
			var grid = noticesBoundGrid;
			if (grid != null && grid.ListManager != null && grid.CurrentRowIndex >= 0)
			{
				var message = grid.ListManager.GetCurrent() as NoticesMessage;
				if (message != null)
				{
					ZFormModaliser.ShowDialogAndDispose(new EDIMessageForm(message.Message));
				}
			}
		}
	}
}
