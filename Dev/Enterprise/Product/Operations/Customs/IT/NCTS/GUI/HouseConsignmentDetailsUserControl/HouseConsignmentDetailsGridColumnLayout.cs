using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI;

public sealed class HouseConsignmentDetailsGridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout(false));
	IGridColumnLayout layout;

	internal static IGridColumnLayout CreateLayout(bool isInPhase5TransitionPeriod)
	{
		var euGridColumnBag = EU.NCTS.GUI.HouseConsignmentDetailsGridColumnsBag.Instance;
		var itGridColumnBag = HouseConsignmentDetailsGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(euGridColumnBag.SequenceNumberTextBoxColumn);
		builder.AddColumn(euGridColumnBag.CountryOfExportDropEditColumn);

		if (!isInPhase5TransitionPeriod)
		{
			builder.AddColumn(itGridColumnBag.CountryOfDestinationDropEditColumn);
		}

		builder.AddColumn(euGridColumnBag.WeightCalcEditColumn);
		builder.AddColumn(euGridColumnBag.ReferenceIDTextBoxColumn);

		if (!isInPhase5TransitionPeriod)
		{
			builder.AddColumn(euGridColumnBag.TransportPaymentMethodDropEditColumn);
		}
		builder.AddColumn(itGridColumnBag.StatusDropEditColumn);
		return builder.Build();
	}
}
