using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class GoodsItemPreviousDocumentsGridUserControl : ZUserControl
	{
		public GoodsItemPreviousDocumentsGridUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			UpdateGridColumnLayout();
		}

		new NctsHeader DataSource => base.DataSource as NctsHeader;

		void UpdateGridColumnLayout()
		{
			var layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode);
			PreviousDocumentsGrid.ApplyGridColumnLayout(layoutProvider.GetGoodsItemsPreviousDocumentsGridColumnLayout());
		}
	}
}
