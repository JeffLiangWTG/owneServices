using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI;

public class ConsignmentItemsUcc6GridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	IGridColumnLayout layout;

	IGridColumnLayout CreateLayout()
	{
		var commonColumnBag = ConsignmentItemsGridColumnsBag.Instance;
		var builder = GridColumnLayoutBuilder.Create();

		builder.AddColumn(commonColumnBag.LineNumberTextBoxColumn);
		builder.AddColumn(commonColumnBag.GrossMassCalEditColumn);
		builder.AddColumn(commonColumnBag.NetMassCalcEditColumn);
		builder.AddColumn(commonColumnBag.DiscrepancyStatusDropEditColumn);
		builder.AddColumn(commonColumnBag.UniqueConsignmentReferenceTextBoxColumn);
		builder.AddColumn(commonColumnBag.UniqueConsignmentReferenceStatusDropEditColumn);

		return builder.Build();
	}
}
