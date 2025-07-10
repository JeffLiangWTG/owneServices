using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;
public sealed class SWProductionGridColumnLayout : IGridColumnLayoutProvider
{
	IGridColumnLayout IGridColumnLayoutProvider.Layout => layout ??= CreateLayout();

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var gridColumnBag = SWProductionGridColumnsBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(gridColumnBag.LineNoCalcEditColumn);
		builder.AddColumn(gridColumnBag.ReferenceNumberTextBoxColumn);
		builder.AddColumn(gridColumnBag.BatchQuantityColumn);
		builder.AddColumn(gridColumnBag.UnitOfQuantityColumn);
		builder.AddColumn(gridColumnBag.ManufacturingDateColumn);
		builder.AddColumn(gridColumnBag.ExpiryDateColumn);
		builder.AddColumn(gridColumnBag.BestBeforeDateColumn);
		return builder.Build();
	}

	IGridColumnLayout layout;

	#endregion
}
