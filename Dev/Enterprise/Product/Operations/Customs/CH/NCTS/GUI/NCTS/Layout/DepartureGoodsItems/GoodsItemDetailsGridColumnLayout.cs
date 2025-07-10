using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public sealed class GoodsItemDetailsGridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	static IGridColumnLayout CreateLayout()
	{
		var euGridColumnBag = Phase5GoodsItemDetailsGridColumnsBag.Instance;
		var chGridColumnBag = GoodsItemDetailsGridColumnBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.LineNoTextBoxColumn);
		builder.AddColumn(euGridColumnBag.DeclarationGoodsItemNumberTextBoxColumn);
		builder.AddColumn(euGridColumnBag.DescriptionTextBoxColumn);
		builder.AddColumn(euGridColumnBag.GrossWeightCalcEditColumn);
		builder.AddColumn(euGridColumnBag.GrossWeightUnitDropEditColumn);
		builder.AddColumn(euGridColumnBag.NetWeightCalcEditColumn);
		builder.AddColumn(euGridColumnBag.NetWeightUnitDropEditColumn);
		builder.AddColumn(chGridColumnBag.FormattedHarmonisedTariffColumn);
		builder.AddColumn(euGridColumnBag.TypeDropEditColumn);
		builder.AddColumn(euGridColumnBag.CountryOfDispatchDropEditColumn);
		builder.AddColumn(euGridColumnBag.CountryOfDestinationDropEditColumn);
		builder.AddColumn(euGridColumnBag.CountryOfOriginDropEditColumn);
		builder.AddColumn(euGridColumnBag.CommercialReferenceNumberTextBoxColumn);
		builder.AddColumn(euGridColumnBag.TransportChargesMethodOfPaymentDropEditColumn);
		builder.AddColumn(euGridColumnBag.UNDGsGuidFindBoxColumn);
		builder.AddColumn(euGridColumnBag.CusC4NumberCodeFindBoxColumn);

		return builder.Build();
	}
}
