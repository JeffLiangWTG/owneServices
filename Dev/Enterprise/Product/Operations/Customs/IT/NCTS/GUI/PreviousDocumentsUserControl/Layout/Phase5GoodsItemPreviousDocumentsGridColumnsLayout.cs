using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class Phase5GoodsItemPreviousDocumentsGridColumnsLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var euGridColumnBag = EU.NCTS.GUI.Phase5GoodsItemPreviousDocumentsGridColumnsBag.Instance;
		var itGridColumnBag = Phase5GoodsItemPreviousDocumentsGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(itGridColumnBag.TypeCodeDropEditColumn);
		builder.AddColumn(euGridColumnBag.ReferenceNumberTextBoxColumn);
		builder.AddColumn(itGridColumnBag.NumOfPackagesCalcEditColumn);
		builder.AddColumn(itGridColumnBag.PackageTypeDropEditColumn);
		builder.AddColumn(euGridColumnBag.QuantityCalcEditColumn);
		builder.AddColumn(euGridColumnBag.UnitOfQuantityDropEditColumn);
		builder.AddColumn(euGridColumnBag.ItemNumberCalcEditColumn);
		builder.AddColumn(euGridColumnBag.ComplementTextBoxColumn);

		return builder.Build();
	}

	IGridColumnLayout layout;

	#endregion
}
