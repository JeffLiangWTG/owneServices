using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

class LayoutUcc6ExportPreviousDocumentsUserControl : EU.GUI.PlugIn.LayoutPreviousDocumentsUserControl
{
	protected override IGridColumnLayoutProvider GetGridColumnLayoutProvider() => new Ucc6ExportPreviousDocumentGridColumnLayout();

	protected override void HandleDeclarationControlVisibilityChangedCore()
	{
		base.HandleDeclarationControlVisibilityChangedCore();
		ChangeGridColumnsCaptions();
	}

	void ChangeGridColumnsCaptions()
	{
		using (PreviousDocumentsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
		{
			PreviousDocumentsGrid.SetColumnCaption(nameof(PreviousDocument.CSI_Code), TypeCaption);
			PreviousDocumentsGrid.SetColumnCaption(nameof(PreviousDocument.CSI_ReferenceNumber), ReferenceNumberCaption);
			PreviousDocumentsGrid.SetColumnCaption(nameof(PreviousDocument.CSI_PackQty), NumberOfPackagesCaption);
			PreviousDocumentsGrid.SetColumnCaption(nameof(PreviousDocument.CSI_Quantity), QuantityCaption);
			PreviousDocumentsGrid.SetColumnCaption(nameof(PreviousDocument.CSI_UnitOfQuantity), UOMCaption);
			PreviousDocumentsGrid.SetColumnCaption(nameof(PreviousDocument.CSI_ItemNumber), GoodsItemIdentifierCaption);
		}
	}

	static string TypeCaption => Res.GetString("A75EF22A-1D28-4428-908C-326DE9A4D840", "Type");
	static string ReferenceNumberCaption => Res.GetString("5063A7F3-CCAC-4027-8E27-967631A1B264", "Reference Number");
	static string NumberOfPackagesCaption => Res.GetString("CF0E1707-CF50-4183-8CFE-5A793792DA49", "Number of Packages");
	static string QuantityCaption => Res.GetString("C0059C7C-BB49-4185-B9BE-914194904E5D", "Quantity");
	static string UOMCaption => Res.GetString("2E301A7A-3DB0-48F1-9569-D12FA409D35E", "UOM");
	static string GoodsItemIdentifierCaption => Res.GetString("BEE8F32F-BF59-4FCB-A7EC-EC83D90CC5E8", "Goods Item Identifier");
}
