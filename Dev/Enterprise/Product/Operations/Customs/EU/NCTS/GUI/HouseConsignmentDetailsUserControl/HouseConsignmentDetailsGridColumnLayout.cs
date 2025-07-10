using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class HouseConsignmentDetailsGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

		IGridColumnLayout CreateLayout()
		{
			var euGridColumnBag = HouseConsignmentDetailsGridColumnsBag.Instance;

			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(euGridColumnBag.SequenceNumberTextBoxColumn);
			builder.AddColumn(euGridColumnBag.CountryOfExportDropEditColumn);
			builder.AddColumn(euGridColumnBag.WeightCalcEditColumn);
			builder.AddColumn(euGridColumnBag.WeightUQDropEditColumn);
			builder.AddColumn(euGridColumnBag.ReferenceIDTextBoxColumn);
			builder.AddColumn(euGridColumnBag.TransportPaymentMethodDropEditColumn);
			builder.AddColumn(euGridColumnBag.CountryOfDestinationDropEditColumn);
			builder.AddColumn(euGridColumnBag.ConsignorOrganisationFindBoxColumn);
			builder.AddColumn(euGridColumnBag.ConsignorAddressDropEditColumn);
			builder.AddColumn(euGridColumnBag.ConsigneeOrganisationFindBoxColumn);
			builder.AddColumn(euGridColumnBag.ConsigneeAddressDropEditColumn);
			builder.AddColumn(euGridColumnBag.LinePriceCurrencyDropEditColumn);

			return builder.Build();
		}

		IGridColumnLayout layout;
	}
}
