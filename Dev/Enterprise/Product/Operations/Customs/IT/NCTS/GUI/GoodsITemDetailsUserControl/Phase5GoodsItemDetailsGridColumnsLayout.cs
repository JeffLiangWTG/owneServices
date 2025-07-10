using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class Phase5GoodsItemDetailsGridColumnsLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout(false));
	IGridColumnLayout layout;

	internal static IGridColumnLayout CreateLayout(bool isInPhase5TransitionPeriod)
	{
		var euGridColumnBag = EU.NCTS.GUI.Phase5GoodsItemDetailsGridColumnsBag.Instance;
		var itGridColumnBag = Phase5GoodsItemDetailsGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.LineNoTextBoxColumn);
		builder.AddColumn(euGridColumnBag.DeclarationGoodsItemNumberTextBoxColumn);
		builder.AddColumn(euGridColumnBag.DescriptionTextBoxColumn);
		builder.AddColumn(euGridColumnBag.GrossWeightCalcEditColumn);
		builder.AddColumn(euGridColumnBag.GrossWeightUnitDropEditColumn);
		builder.AddColumn(euGridColumnBag.NetWeightCalcEditColumn);
		builder.AddColumn(euGridColumnBag.NetWeightUnitDropEditColumn);
		builder.AddColumn(euGridColumnBag.FormattedHarmonisedTariffColumn);
		builder.AddColumn(euGridColumnBag.TypeDropEditColumn);
		builder.AddColumn(euGridColumnBag.CountryOfDispatchDropEditColumn);
		builder.AddColumn(euGridColumnBag.CountryOfDestinationDropEditColumn);
		builder.AddColumn(euGridColumnBag.CountryOfOriginDropEditColumn);
		builder.AddColumn(euGridColumnBag.CommercialReferenceNumberTextBoxColumn);
		if (isInPhase5TransitionPeriod)
		{
			builder.AddColumn(euGridColumnBag.TransportChargesMethodOfPaymentDropEditColumn);
		}
		builder.AddColumn(euGridColumnBag.UNDGsGuidFindBoxColumn);
		builder.AddColumn(euGridColumnBag.CusC4NumberCodeFindBoxColumn);
		builder.AddColumn(euGridColumnBag.ConsigneeOrganisationFindBoxColumn);
		builder.AddColumn(euGridColumnBag.ConsigneeAddressDropEditColumn);
		builder.AddColumn(euGridColumnBag.SupplementaryQuantityEditColumn);
		builder.AddColumn(euGridColumnBag.SupplementaryQuantityUnitDropEditColumn);
		builder.AddColumn(euGridColumnBag.CustomsQuantityDropEditColumn);
		builder.AddColumn(euGridColumnBag.ThirdQuantityEditColumn);
		builder.AddColumn(euGridColumnBag.ThirdQuantityUnitDropEditColumn);
		builder.AddColumn(euGridColumnBag.FourthQuantityEditColumn);
		builder.AddColumn(euGridColumnBag.FourthQuantityUnitDropEditColumn);
		builder.AddColumn(euGridColumnBag.CustomsValueCalcDropEditColumn);
		builder.AddColumn(euGridColumnBag.TaxOrFeeDropEditColumn);
		builder.AddColumn(euGridColumnBag.SupplementaryCodesFindBoxColumn);
		builder.AddColumn(euGridColumnBag.LinePriceCalcEditColumn);
		builder.AddColumn(euGridColumnBag.LinePriceCurrencyDropEditColumn);
		builder.AddColumn(itGridColumnBag.StatusDropEditColumn);

		return builder.Build();
	}
}
