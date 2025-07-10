using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

internal class GoodsItemPreviousDocumentsGridColumnsLayout : IGridColumnLayoutProvider
{
	public GoodsItemPreviousDocumentsGridColumnsLayout()
	{
		Layout = CreateGoodsItemPreviousDocumentsGridColumnsLayout();
	}

	IGridColumnLayout Layout { get; }

	IGridColumnLayout IGridColumnLayoutProvider.Layout => Layout;

	IGridColumnLayout CreateGoodsItemPreviousDocumentsGridColumnsLayout()
	{
		var euGridColumnBag = Phase5GoodsItemPreviousDocumentsGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.TypeCodeFindBoxColumn);
		builder.AddColumn(euGridColumnBag.ReferenceNumberTextBoxColumn);
		builder.AddColumn(euGridColumnBag.ItemNumberCalcEditColumn);
		builder.AddColumn(euGridColumnBag.ComplementTextBoxColumn);

		return builder.Build();
	}
}
