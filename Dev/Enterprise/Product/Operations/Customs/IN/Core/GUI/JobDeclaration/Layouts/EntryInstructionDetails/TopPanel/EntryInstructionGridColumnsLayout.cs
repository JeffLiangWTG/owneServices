using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

sealed class EntryInstructionGridColumnsLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var gridColumnBag = EntryInstructionGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(gridColumnBag.LocalReferenceNumberTextBoxColumn);
		builder.AddColumn(gridColumnBag.LocalReferenceNumberDateEditColumn);
		builder.AddColumn(gridColumnBag.CEI_StyleDropEditColumn);
		builder.AddColumn(gridColumnBag.CEI_SubStyleDropEditColumn);
		builder.AddColumn(gridColumnBag.CEI_DescriptionTextBoxColumn);
		builder.AddColumn(gridColumnBag.ShippingBillNumberTextBoxColumn);
		builder.AddColumn(gridColumnBag.ShippingBillDateDateEditColumn);

		return builder.Build();
	}

	IGridColumnLayout layout;

	#endregion
}
