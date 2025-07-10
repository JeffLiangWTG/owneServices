using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class Phase5GoodsItemDetailsGridColumnsLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());

		#region Implementation

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = Phase5GoodsItemDetailsGridColumnsBag.Instance;

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
			builder.AddColumn(euGridColumnBag.TransportChargesMethodOfPaymentDropEditColumn);
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

			return builder.Build();
		}

		IGridColumnLayout layout;

		#endregion
	}
}
