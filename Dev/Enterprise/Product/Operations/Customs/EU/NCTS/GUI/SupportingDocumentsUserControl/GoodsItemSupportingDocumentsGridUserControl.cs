using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class GoodsItemSupportingDocumentsGridUserControl : ZUserControl
	{
		public GoodsItemSupportingDocumentsGridUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => (NctsHeader)base.DataSource;

		INctsPhase5LayoutProvider LayoutProvider => layoutProvider ?? (layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode));
		INctsPhase5LayoutProvider layoutProvider;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			UpdateGridColumnLayout();
		}

		void UpdateGridColumnLayout()
		{
			SupportingDocumentsGrid.ApplyGridColumnLayout(LayoutProvider.SuportingDocumentGridColumnLayout);
		}
	}
}
