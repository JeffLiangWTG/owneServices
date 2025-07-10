using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public sealed class ConsignmentItemsGridColumnLayout : IGridColumnLayoutProvider
	{
		IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ?? (layout = CreateLayout());
		IGridColumnLayout layout;

		IGridColumnLayout CreateLayout()
		{
			var columnBag = ConsignmentItemsGridColumnsBag.Instance;
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn(columnBag.LineNumberTextBoxColumn);
			builder.AddColumn(columnBag.GrossMassCalEditColumn);
			builder.AddColumn(columnBag.NetMassCalcEditColumn);
			builder.AddColumn(columnBag.UniqueConsignmentReferenceTextBoxColumn);
			return builder.Build();
		}
	}
}
