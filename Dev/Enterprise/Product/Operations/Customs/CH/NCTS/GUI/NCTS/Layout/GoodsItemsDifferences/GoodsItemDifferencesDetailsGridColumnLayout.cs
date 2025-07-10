using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

internal class GoodsItemDifferencesDetailsGridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	static IGridColumnLayout CreateLayout()
	{
		var euGridColumnBag = Phase5GoodsItemDifferencesDetailsGridColumnsBag.Instance;
		var chGridColumnBag = GoodsItemDifferencesDetailsGridColumnBag.Instance;

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
		builder.AddColumn(chGridColumnBag.FormattedHarmonisedTariffColumn);

		return builder.Build();
	}
}
