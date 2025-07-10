using System;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class RefundDeclarationImportEntriesUserControl : ZUserControl
	{
		public RefundDeclarationImportEntriesUserControl()
		{
			InitializeComponent();
			ImportEntriesGrid.ListManagerListChanged += ImportEntriesGrid_ListManagerListChanged;
			HookToOriginalEntryValueChanged();
			PaidAndRefundPanel.UpdateLayout(new StandAloneRefundDetailsLayout());
			RefundForCancelPanel.UpdateLayout(new RefundForCancelLayout());
			DetailsPanel.UpdateLayout(new ImportEntriesDetailsLayout());
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateInvoiceLinesTabVisibility();
		}

		void ImportEntriesGrid_ListManagerListChanged(object sender, ZGrid.ListManagerListChangedEventArgs e)
		{
			var grid = (ZGrid)sender;
			var listManager = grid.ListManager;
			listManager.CurrentChanged -= ImportEntriesGrid_CurrentChanged;
			listManager.CurrentChanged += ImportEntriesGrid_CurrentChanged;
			UpdateInvoiceLinesTabVisibility();
		}

		CusReconEntryLine CurrentEntryLine => (CusReconEntryLine)ImportEntriesGrid.GetCurrent();

		void OriginalEntry_ValueChanged(object sender, EventArgs e)
		{
			UpdateInvoiceLinesTabVisibility();
		}

		void ImportEntriesGrid_CurrentChanged(object sender, EventArgs e)
		{
			HookToOriginalEntryValueChanged();
			UpdateInvoiceLinesTabVisibility();
		}

		void HookToOriginalEntryValueChanged()
		{
			if (CurrentEntryLine != null)
			{
				CurrentEntryLine.CRL_OriginalEntryLineNumberInfo.ValueChanged -= OriginalEntry_ValueChanged;
				CurrentEntryLine.CRL_OriginalEntryLineNumberInfo.ValueChanged += OriginalEntry_ValueChanged;
			}
		}

		void UpdateInvoiceLinesTabVisibility()
		{
			if (InvoiceLinesTabPage != null)
			{
				InvoiceLinesTabPage.TabVisible = !CurrentEntryLine?.CRL_OriginalEntryLineNumber.IsEmpty ?? false;
			}
		}
	}
}
