using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemDifferencesDetailsGridColumnsLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = Phase5GoodsItemDifferencesDetailsGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.LineNoTextBoxColumn);
			builder.AddColumn(euGridColumnBag.DeclarationGoodsItemNumberTextBoxColumn);
			builder.AddColumn(euGridColumnBag.UnloadedStateDropEditColumn);
			builder.AddColumn(euGridColumnBag.CusC4NumberCodeFindBoxColumn);
			builder.AddColumn(euGridColumnBag.DescriptionTextBoxColumn);
			builder.AddColumn(euGridColumnBag.GrossWeightCalcEditColumn);
			builder.AddColumn(euGridColumnBag.GrossWeightUnitDropEditColumn);
			builder.AddColumn(euGridColumnBag.NetWeightCalcEditColumn);
			builder.AddColumn(euGridColumnBag.NetWeightUnitDropEditColumn);
			builder.AddColumn(euGridColumnBag.FormattedHarmonisedTariffColumn);

			return builder.Build();
		}

		IGridColumnLayout layout;

		#endregion
	}
}
