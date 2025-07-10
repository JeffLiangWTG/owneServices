using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	sealed class Phase5GoodsItemPreviousDocumentsGridColumnsLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = EU.NCTS.GUI.Phase5GoodsItemPreviousDocumentsGridColumnsBag.Instance;
			var frGidColumnBag = Phase5GoodsItemPreviousDocumentsGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.TypeCodeFindBoxColumn);
			builder.AddColumn(frGidColumnBag.ReferenceNumberMultiControlColumn);
			builder.AddColumn(euGridColumnBag.ItemNumberCalcEditColumn);
			builder.AddColumn(euGridColumnBag.NumOfPackagesCalcEditColumn);
			builder.AddColumn(euGridColumnBag.PackageTypeDropEditColumn);
			builder.AddColumn(euGridColumnBag.QuantityCalcEditColumn);
			builder.AddColumn(euGridColumnBag.UnitOfQuantityDropEditColumn);
			builder.AddColumn(euGridColumnBag.ComplementTextBoxColumn);
			return builder.Build();
		}

		IGridColumnLayout layout;

		#endregion
	}
}
