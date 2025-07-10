using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	sealed class Phase5GoodsItemPreviousDocumentsGridColumnsLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = EU.NCTS.GUI.Phase5GoodsItemPreviousDocumentsGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.TypeCodeFindBoxColumn);
			builder.AddColumn(euGridColumnBag.ReferenceNumberTextBoxColumn);
			builder.AddColumn(euGridColumnBag.ItemNumberCalcEditColumn);
			builder.AddColumn(euGridColumnBag.ComplementTextBoxColumn);

			return builder.Build();
		}

		IGridColumnLayout layout;

		#endregion
	}
}
